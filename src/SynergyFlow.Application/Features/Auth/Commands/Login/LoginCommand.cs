using SynergyFlow.Application.Features.Auth.DTOs;
using SynergyFlow.Domain.Common.Results;

using MediatR;

namespace SynergyFlow.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<Result<AuthResponseDto>>;
