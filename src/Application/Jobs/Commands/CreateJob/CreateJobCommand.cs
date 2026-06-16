namespace Application.Jobs.Commands.CreateJob;

using MediatR;
using System;

public record CreateJobCommand(
    string? ProductUrl,
    string? VideoType,
    string? TargetAudience,
    string? FlowType,
    string? AvatarId,
    string? ModelGender,
    string? VoiceId,
    string? OutputFormat,
    string? CustomerEmail,
    string? Notes
) : IRequest<Guid>;
