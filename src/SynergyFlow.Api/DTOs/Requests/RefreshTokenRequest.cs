namespace SynergyFlow.Api.DTOs.Requests;

public sealed record RefreshTokenRequest(string ExpiredAccessToken, string RefreshToken);
