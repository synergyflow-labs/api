using SynergyFlow.Application.Features.Auth.DTOs;
using SynergyFlow.Domain.Common.Results;

using MediatR;

namespace SynergyFlow.Application.Features.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string ExpiredAccessToken, string RefreshToken)
    : IRequest<Result<AuthResponseDto>>;
