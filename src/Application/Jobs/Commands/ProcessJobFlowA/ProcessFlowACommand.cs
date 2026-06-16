namespace Application.Jobs.Commands.ProcessJobFlowA;

using MediatR;
using System;

public record ProcessFlowACommand(
    Guid JobId,
    string? AvatarId,
    string? VoiceId
) : IRequest;
