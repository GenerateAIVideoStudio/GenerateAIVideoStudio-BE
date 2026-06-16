namespace Application.Jobs.Commands.CreateJob;

using Application.Common.Interfaces;
using Application.Common.DTOs;
using Domain.Entities;
using Domain.Constants;
using MediatR;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class CreateJobCommandHandler : IRequestHandler<CreateJobCommand, Guid>
{
    private readonly IProductScraperService _scraper;
    private readonly IProductCategoryDetector _categoryDetector;
    private readonly IVideoJobRepository _jobRepo;

    public CreateJobCommandHandler(
        IProductScraperService scraper,
        IProductCategoryDetector categoryDetector,
        IVideoJobRepository jobRepo)
    {
        _scraper = scraper;
        _categoryDetector = categoryDetector;
        _jobRepo = jobRepo;
    }

    public async Task<Guid> Handle(CreateJobCommand request, CancellationToken ct)
    {
        // 1. Scrape product info
        ProductInfo? product = null;
        if (!string.IsNullOrWhiteSpace(request.ProductUrl))
        {
            product = await _scraper.ScrapeAsync(request.ProductUrl, ct);
        }

        // 2. Auto-detect flow if user did not choose one
        var flowType = request.FlowType;
        if (string.IsNullOrWhiteSpace(flowType) && product != null)
        {
            var category = await _categoryDetector.DetectCategoryAsync(
                product.Name, product.Description, ct);
            flowType = _categoryDetector.GetFlowType(category);
        }
        
        if (string.IsNullOrWhiteSpace(flowType))
        {
            flowType = Domain.Constants.FlowType.Avatar;
        }

        // 3. Create job
        var job = new VideoJob
        {
            ProductUrl = request.ProductUrl,
            ProductInfo = product != null ? JsonSerializer.SerializeToDocument(product) : null,
            VideoType = request.VideoType,
            TargetAudience = request.TargetAudience,
            FlowType = flowType,
            CustomerEmail = request.CustomerEmail,
            Notes = request.Notes,
            OutputFormat = request.OutputFormat ?? VideoFormat.Portrait,
            Status = JobStatus.Pending
        };

        await _jobRepo.AddAsync(job, ct);
        return job.Id;
    }
}
