namespace Application.Jobs.Commands.ProcessJobFlowC;

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

public class ProcessFlowCCommandHandler : IRequestHandler<ProcessFlowCCommand>
{
    private static readonly Dictionary<string, string> MotionPrompts = new()
    {
        ["beauty"] = "the product floats gently, cap opens revealing texture, slow rotation, studio lighting",
        ["skincare"] = "cream texture close-up, skin absorption effect, dewy finish, macro shot",
        ["food"] = "steam rises from the dish, close-up texture, natural warm lighting, appetizing look",
        ["gadget"] = "product powers on, screen lights up, camera slowly orbits around the product",
        ["jewelry"] = "slowly rotates 360 degrees, catching light with sparkle effect, black velvet background",
    };

    private readonly IVideoJobRepository _jobRepo;
    private readonly IScriptGeneratorService _scriptGen;
    private readonly IVoiceSynthesisService _voiceService;
    private readonly IKlingService _klingService;
    private readonly IStorageService _storage;
    private readonly IVideoComposerService _composer;
    private readonly HttpClient _httpClient;

    public ProcessFlowCCommandHandler(
        IVideoJobRepository jobRepo,
        IScriptGeneratorService scriptGen,
        IVoiceSynthesisService voiceService,
        IKlingService klingService,
        IStorageService storage,
        IVideoComposerService composer,
        HttpClient httpClient)
    {
        _jobRepo = jobRepo;
        _scriptGen = scriptGen;
        _voiceService = voiceService;
        _klingService = klingService;
        _storage = storage;
        _composer = composer;
        _httpClient = httpClient;
    }

    public async Task Handle(ProcessFlowCCommand request, CancellationToken ct)
    {
        var job = await _jobRepo.GetByIdAsync(request.JobId, ct);
        if (job == null)
            throw new ArgumentException($"Job with id {request.JobId} not found");

        var product = JsonSerializer.Deserialize<ProductInfo>(job.ProductInfo!.RootElement.GetRawText())!;

        try
        {
            // 1. Generate script
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

            // 3. Kling Image-to-Video
            await SetStatus(job, JobStatus.Animating, ct);
            var productImageUrl = product.ImageUrls.FirstOrDefault()
                ?? throw new InvalidOperationException("No product image available");

            var category = product.Category?.ToLower() ?? "general";
            var motionPrompt = MotionPrompts.GetValueOrDefault(category,
                "product slowly rotates, studio lighting, clean background");

            // Retry up to 3 times if output is distorted or fails
            string? productVideoKey = null;
            for (int retry = 0; retry < 3; retry++)
            {
                try
                {
                    var promptWithRetry = motionPrompt + (retry > 0 ? " high quality" : "");
                    var taskId = await _klingService.SubmitImageToVideoAsync(productImageUrl, promptWithRetry, 5, ct);
                    await Track(job, AiService.Kling, "image2video", 0.14m, ct);

                    var result = await PollKlingUntilDone(taskId, KlingTaskType.ImageToVideo, ct);
                    if (result?.IsSuccess == true && result.VideoUrl != null)
                    {
                        // Download & upload to R2
                        using var stream = await _httpClient.GetStreamAsync(result.VideoUrl, ct);
                        productVideoKey = $"product_videos/{Guid.NewGuid()}.mp4";
                        await _storage.UploadAsync(stream, productVideoKey, "video/mp4", ct);
                        break;
                    }
                }
                catch (Exception)
                {
                    if (retry == 2)
                    {
                        // Final retry failed, fallback will be handled in composer (slideshow)
                        break;
                    }
                }
            }

            var variation = new JobVariation
            {
                JobId = job.Id,
                HookText = script.Hook,
                VoiceId = request.VoiceId ?? "default-vi",
                AudioUrl = audioKey,
                ProductVideoUrl = productVideoKey,
                VariationIndex = 0
            };

            // 4. FFmpeg compose
            await SetStatus(job, JobStatus.Composing, ct);
            var finalKey = await _composer.ComposeAsync(new VideoComposeRequest(
                AvatarVideoObjectKey: null,
                TryOnImageUrls: null,
                ProductVideoObjectKey: productVideoKey, // can be null if Kling failed
                ProductImageUrls: product.ImageUrls.Take(3).ToList(),
                ScriptFullText: script.FullText,
                OutputFormat: job.OutputFormat,
                FlowType: Domain.Constants.FlowType.ImageVideo
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
