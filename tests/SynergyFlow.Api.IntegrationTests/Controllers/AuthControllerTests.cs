using System.Net;

using SynergyFlow.Api.DTOs.Requests;
using SynergyFlow.Api.IntegrationTests.Common;
using SynergyFlow.Application.Features.Auth.DTOs;
using SynergyFlow.Tests.Common.Security;

using Xunit;

namespace SynergyFlow.Api.IntegrationTests.Controllers;

public class AuthControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Login_WithValidCredentials_Returns200AndTokens()
    {
        // Arrange
        using var client = factory.CreateAppHttpClient();
        var request = new LoginRequest(TestUsers.Admin.User.Email!, TestUsers.Admin.Password);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(authResponse);
        Assert.False(string.IsNullOrWhiteSpace(authResponse.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(authResponse.RefreshToken));
        Assert.True(authResponse.ExpiresInSeconds > 0);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_Returns401Unauthorized()
    {
        // Arrange
        using var client = factory.CreateAppHttpClient();
        var request = new LoginRequest(TestUsers.Admin.User.Email!, "WrongPassword123!");

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsNewTokens()
    {
        // Arrange
        using var client = factory.CreateAppHttpClient();
        var loginRequest = new LoginRequest(TestUsers.Admin.User.Email!, TestUsers.Admin.Password);
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        var loginTokens = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(loginTokens);

        var refreshRequest = new RefreshTokenRequest(loginTokens.AccessToken, loginTokens.RefreshToken);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/refresh-token", refreshRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var newTokens = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(newTokens);
        Assert.False(string.IsNullOrWhiteSpace(newTokens.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(newTokens.RefreshToken));
    }
}
