using SynergyFlow.Application.Common.Interfaces;
using SynergyFlow.Application.Features.Auth.DTOs;
using SynergyFlow.Domain.Common.Results;

using MediatR;

namespace SynergyFlow.Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler(IAuthService authService)
    : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
{
    public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        return await authService.RefreshTokenAsync(request.ExpiredAccessToken, request.RefreshToken, ct);
    }
}
