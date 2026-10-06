namespace SynergyFlow.Application.Features.Auth.DTOs;

public sealed record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds,
    string TokenType = "Bearer");
