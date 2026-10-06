using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using SynergyFlow.Application.Common.Errors;
using SynergyFlow.Application.Common.Interfaces;
using SynergyFlow.Application.Features.Auth.DTOs;
using SynergyFlow.Domain.Common.Results;
using SynergyFlow.Domain.Entities.Identity;
using SynergyFlow.Infrastructure.Settings;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace SynergyFlow.Infrastructure.Identity;

public sealed class AuthService(
    UserManager<AppUser> userManager,
    IAppDbContext dbContext,
    IOptions<JwtSettings> jwtOptions,
    TimeProvider timeProvider,
    ILogger<AuthService> logger)
    : IAuthService
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public async Task<Result<AuthResponseDto>> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return ApplicationErrors.InvalidCredentials;
        }

        var isPasswordValid = await userManager.CheckPasswordAsync(user, password);
        if (!isPasswordValid)
        {
            return ApplicationErrors.InvalidCredentials;
        }

        var roles = await userManager.GetRolesAsync(user);
        var tokenResult = await GenerateTokensAsync(user, roles, ct);

        return tokenResult;
    }

    public async Task<Result<AuthResponseDto>> RefreshTokenAsync(
        string expiredAccessToken,
        string refreshToken,
        CancellationToken ct = default)
    {
        var principal = GetPrincipalFromExpiredToken(expiredAccessToken);
        if (principal is null)
        {
            logger.LogError("Expired access token is not valid.");
            return ApplicationErrors.ExpiredAccessTokenInvalid;
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            logger.LogError("Invalid user ID claim in token.");
            return ApplicationErrors.UserIdClaimInvalid;
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return ApplicationErrors.UserNotFound;
        }

        var existingRefreshToken = await dbContext.UserRefreshTokens
            .FirstOrDefaultAsync(r => r.UserId == userId && r.Token == refreshToken, ct);

        if (existingRefreshToken is null)
        {
            return ApplicationErrors.InvalidRefreshToken;
        }

        if (!existingRefreshToken.IsActive)
        {
            return ApplicationErrors.ExpiredOrRevokedRefreshToken;
        }

        // Revoke the used refresh token (token rotation)
        existingRefreshToken.RevokedOnUtc = timeProvider.GetUtcNow();

        var roles = await userManager.GetRolesAsync(user);
        var newTokensResult = await GenerateTokensAsync(user, roles, ct);

        await dbContext.SaveChangesAsync(ct);

        return newTokensResult;
    }

    public async Task<Result<string>> RegisterUserAsync(
        string email,
        string password,
        string role = "User",
        CancellationToken ct = default)
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            return ApplicationErrors.UserEmailAlreadyExists;
        }

        var newUser = new AppUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(newUser, password);
        if (!result.Succeeded)
        {
            var errors = result.Errors
                .Select(e => Error.Validation(e.Code, e.Description))
                .ToList();
            return errors;
        }

        await userManager.AddToRoleAsync(newUser, role);
        return newUser.Id;
    }

    public async Task<string> GetUserNameAsync(string userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user?.FullName ?? user?.UserName ?? "User";
    }

    private async Task<Result<AuthResponseDto>> GenerateTokensAsync(
        AppUser user,
        IList<string> roles,
        CancellationToken ct)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var now = timeProvider.GetUtcNow();
        var expires = now.AddMinutes(_jwtSettings.ExpiryMinutes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires.UtcDateTime,
            Issuer = _jwtSettings.Issuer,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature),
        };

        if (_jwtSettings.Audiences.Length > 0)
        {
            tokenDescriptor.Audience = _jwtSettings.Audiences[0];
        }

        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var accessToken = tokenHandler.WriteToken(securityToken);

        // Generate cryptographically secure refresh token
        var refreshTokenString = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshToken = new UserRefreshToken
        {
            Token = refreshTokenString,
            UserId = user.Id,
            ExpiresOnUtc = now.AddDays(_jwtSettings.RefreshTokenExpirationDays),
        };

        await dbContext.UserRefreshTokens.AddAsync(refreshToken, ct);
        await dbContext.SaveChangesAsync(ct);

        var expiresInSeconds = (int)(expires - now).TotalSeconds;

        return new AuthResponseDto(
            AccessToken: accessToken,
            RefreshToken: refreshTokenString,
            ExpiresInSeconds: expiresInSeconds);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
            ValidateLifetime = false, // We expect the token to be expired
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
