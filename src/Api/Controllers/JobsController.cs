namespace Api.Controllers;

using Application.Jobs.Commands.CreateJob;
using Application.Jobs.Commands.ProcessJobFlowA;
using Application.Jobs.Commands.ProcessJobFlowB;
using Application.Jobs.Commands.ProcessJobFlowC;
using Application.Jobs.Queries.GetJob;
using Application.Jobs.Queries.GetJobList;
using Domain.Constants;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

[ApiController]
[Route("api/jobs")]
public class JobsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IBackgroundJobClient _hangfire;

    public JobsController(IMediator mediator, IBackgroundJobClient hangfire)
    {
        _mediator = mediator;
        _hangfire = hangfire;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateJobCommand cmd, CancellationToken ct)
    {
        var jobId = await _mediator.Send(cmd, ct);
        var job = await _mediator.Send(new GetJobQuery(jobId), ct);

        if (job == null)
        {
            return BadRequest(new { message = "Failed to create job." });
        }

        // Route to the correct background processing pipeline
        switch (job.FlowType)
        {
            case FlowType.TryOn:
                _hangfire.Enqueue<IMediator>(m =>
                    m.Send(new ProcessFlowBCommand(jobId, cmd.ModelGender, cmd.VoiceId),
                           CancellationToken.None));
                break;

            case FlowType.ImageVideo:
                _hangfire.Enqueue<IMediator>(m =>
                    m.Send(new ProcessFlowCCommand(jobId, cmd.VoiceId),
                           CancellationToken.None));
                break;

            default: // FlowType.Avatar
                _hangfire.Enqueue<IMediator>(m =>
                    m.Send(new ProcessFlowACommand(jobId, cmd.AvatarId, cmd.VoiceId),
                           CancellationToken.None));
                break;
        }

        return Accepted(new { jobId, flowType = job.FlowType, status = JobStatus.Pending });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var job = await _mediator.Send(new GetJobQuery(id), ct);
        if (job == null)
        {
            return NotFound();
        }
        return Ok(job);
    }

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] string? status,
        [FromQuery] string? flowType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetJobListQuery(status, flowType, page, pageSize), ct);
        return Ok(result);
    }
}
