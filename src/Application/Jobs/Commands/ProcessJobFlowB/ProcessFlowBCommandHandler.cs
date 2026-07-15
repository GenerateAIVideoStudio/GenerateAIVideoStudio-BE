namespace Application.Jobs.Commands.ProcessJobFlowB;

using Application.Common.Interfaces;
using Application.Common.DTOs;
using Domain.Entities;
using Domain.Constants;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class ProcessFlowBCommandHandler : IRequestHandler<ProcessFlowBCommand>
{
    private readonly IVideoJobRepository _jobRepo;
    private readonly IModelImageRepository _modelImageRepo;
    private readonly IScriptGeneratorService _scriptGen;
    private readonly IVoiceSynthesisService _voiceService;
    private readonly IKlingService _klingService;
    private readonly IStorageService _storage;
    private readonly IVideoComposerService _composer;
    private readonly HttpClient _httpClient;

    public ProcessFlowBCommandHandler(
        IVideoJobRepository jobRepo,
        IModelImageRepository modelImageRepo,
        IScriptGeneratorService scriptGen,
        IVoiceSynthesisService voiceService,
        IKlingService klingService,
        IStorageService storage,
        IVideoComposerService composer,
        HttpClient httpClient)
    {
        _jobRepo = jobRepo;
        _modelImageRepo = modelImageRepo;
        _scriptGen = scriptGen;
        _voiceService = voiceService;
        _klingService = klingService;
        _storage = storage;
        _composer = composer;
        _httpClient = httpClient;
    }

    public async Task Handle(ProcessFlowBCommand request, CancellationToken ct)
    {
        var job = await _jobRepo.GetByIdAsync(request.JobId, ct);
        if (job == null)
            throw new ArgumentException($"Job with id {request.JobId} not found");

        var product = JsonSerializer.Deserialize<ProductInfo>(job.ProductInfo!.RootElement.GetRawText())!;

        try
        {
            // 1. Generate script (voice-over)
            await SetStatus(job, JobStatus.Scripting, ct);
            var script = await _scriptGen.GenerateAsync(product, job.VideoType!, job.TargetAudience!, ct: ct);
            job.Script = JsonSerializer.SerializeToDocument(script);
            await _jobRepo.UpdateAsync(job, ct);
            await Track(job, AiService.OpenAi, "script", 0.005m, ct);

            // 2. Voice synthesis
            await SetStatus(job, JobStatus.Voicing, ct);
            var audioKey = await _voiceService.SynthesizeAsync(
                script.FullText, request.VoiceId ?? "default-vi", ct);
            await Track(job, AiService.ElevenLabs, "tts", 0.03m, ct);

            // 3. Kling Virtual Try-On — 3 models concurrently
            await SetStatus(job, JobStatus.TryingOn, ct);
            var garmentImageUrl = product.ImageUrls.FirstOrDefault()
                ?? throw new InvalidOperationException("No product image available");

            // Fetch 3 model images from DB based on gender
            var models = await _modelImageRepo.GetByGenderAsync(
                request.ModelGender ?? "female", count: 3, ct);

            if (models.Count == 0)
                throw new InvalidOperationException($"No model images found in database for gender: {request.ModelGender}");

            // Submit 3 try-on jobs concurrently
            var taskIds = await Task.WhenAll(models.Select(async model =>
            {
                var modelUrl = await _storage.GetPresignedUrlAsync(model.ObjectKey, 1, ct);
                var taskId = await _klingService.SubmitTryOnAsync(modelUrl, garmentImageUrl, ct);
                return (model, taskId);
            }));
            await Track(job, AiService.Kling, "tryon", 0.105m * taskIds.Length, ct); // 3 calls typically

            // Poll all 3 concurrently
            var tryOnResults = await Task.WhenAll(taskIds.Select(async t =>
            {
                var result = await PollKlingUntilDone(t.taskId, KlingTaskType.TryOn, ct);
                return (t.model, imageUrl: result?.ImageUrls?.FirstOrDefault());
            }));

            var validResults = tryOnResults.Where(r => r.imageUrl != null).ToList();
            if (!validResults.Any())
                throw new InvalidOperationException("All try-on jobs failed");

            // 4. Download and upload try-on images to R2
            var uploadedKeys = await Task.WhenAll(validResults.Select(async r =>
            {
                using var stream = await _httpClient.GetStreamAsync(r.imageUrl!, ct);
                var key = $"tryon/{Guid.NewGuid()}.jpg";
                await _storage.UploadAsync(stream, key, "image/jpeg", ct);
                return key;
            }));

            var variation = new JobVariation
            {
                JobId = job.Id,
                HookText = script.Hook,
                VoiceId = request.VoiceId ?? "default-vi",
                AudioUrl = audioKey,
                TryonImages = JsonSerializer.SerializeToDocument(uploadedKeys.ToList()),
                VariationIndex = 0,
                ModelImageId = validResults.FirstOrDefault().model?.Id
            };

            // 5. FFmpeg compose: slideshow try-on + voiceover
            await SetStatus(job, JobStatus.Composing, ct);
            var finalKey = await _composer.ComposeAsync(new VideoComposeRequest(
                AvatarVideoObjectKey: null,
                TryOnImageUrls: uploadedKeys.ToList(),
                ProductVideoObjectKey: null,
                ProductImageUrls: product.ImageUrls.Take(2).ToList(),
                ScriptFullText: script.FullText,
                OutputFormat: job.OutputFormat,
                FlowType: Domain.Constants.FlowType.TryOn,
                AudioObjectKey: audioKey
            ), ct);

            variation.FinalUrl = finalKey;
            variation.Status = JobStatus.Done;

            job.Variations.Add(variation);
            job.OutputUrl = await _storage.GetPresignedUrlAsync(finalKey, ct: ct);
            job.Status = JobStatus.Done;
            job.CompletedAt = DateTime.UtcNow;
            await _jobRepo.UpdateAsync(job, ct);
        }
        catch (Exception ex)
        {
            job.Status = JobStatus.Error;
            job.ErrorMessage = ex.Message;
            await _jobRepo.UpdateAsync(job, ct);
            throw;
        }
    }

    private async Task SetStatus(VideoJob job, string status, CancellationToken ct)
    {
        job.Status = status;
        await _jobRepo.UpdateAsync(job, ct);
    }

    private async Task Track(VideoJob job, string service, string action, decimal costUsd, CancellationToken ct)
    {
        var apiCost = new ApiCost
        {
            JobId = job.Id,
            Service = service,
            Action = action,
            CostUsd = costUsd
        };
        job.ApiCosts.Add(apiCost);
        await _jobRepo.UpdateAsync(job, ct);
    }

    private async Task<KlingTaskResult> PollKlingUntilDone(
        string taskId, KlingTaskType taskType, CancellationToken ct, int timeoutMin = 5)
    {
        var deadline = DateTime.UtcNow.AddMinutes(timeoutMin);
        while (DateTime.UtcNow < deadline)
        {
            await Task.Delay(5000, ct);
            var result = await _klingService.GetResultAsync(taskId, taskType, ct);
            if (result != null)
            {
                if (result.IsSuccess)
                    return result;
                throw new InvalidOperationException(result.ErrorMessage ?? "Kling task failed");
            }
        }
        throw new TimeoutException($"Kling task {taskId} timed out after {timeoutMin} min");
    }
}
