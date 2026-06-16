namespace Application.Jobs.Commands.ProcessJobFlowC;

using MediatR;
using System;

public record ProcessFlowCCommand(
    Guid JobId,
    string? VoiceId
) : IRequest;
