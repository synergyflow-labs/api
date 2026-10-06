using SynergyFlow.Application.Common.Interfaces;
using SynergyFlow.Application.Features.Auth.DTOs;
using SynergyFlow.Domain.Common.Results;

using MediatR;

namespace SynergyFlow.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler(IAuthService authService)
    : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken ct)
    {
        return await authService.LoginAsync(request.Email, request.Password, ct);
    }
}
