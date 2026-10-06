using SynergyFlow.Application.Features.Auth.DTOs;
using SynergyFlow.Domain.Common.Results;

namespace SynergyFlow.Application.Common.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> LoginAsync(string email, string password, CancellationToken ct = default);

    Task<Result<AuthResponseDto>> RefreshTokenAsync(string expiredAccessToken, string refreshToken, CancellationToken ct = default);

    Task<Result<string>> RegisterUserAsync(string email, string password, string role = "User", CancellationToken ct = default);

    Task<string> GetUserNameAsync(string userId, CancellationToken ct = default);
}
