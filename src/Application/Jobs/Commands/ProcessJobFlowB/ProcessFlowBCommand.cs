namespace Application.Jobs.Commands.ProcessJobFlowB;

using MediatR;
using System;

public record ProcessFlowBCommand(
    Guid JobId,
    string? ModelGender,
    string? VoiceId
) : IRequest;
