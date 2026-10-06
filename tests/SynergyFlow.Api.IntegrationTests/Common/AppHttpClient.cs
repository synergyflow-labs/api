using System.Net.Http.Headers;

using SynergyFlow.Api.DTOs.Requests;
using SynergyFlow.Application.Features.Auth.DTOs;

namespace SynergyFlow.Api.IntegrationTests.Common;

public class AppHttpClient(HttpClient httpClient) : IDisposable
{
    private string? _token;

    public async Task AuthenticateAsync(string email, string password)
    {
        var token = await GenerateTokenAsync(email, password);
        SetAuthToken(token);
    }

    public async Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        ApplyAuthorizationHeader(request);
        return await httpClient.SendAsync(request, ct);
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct = default)
    {
        ApplyAuthorizationHeader(request);
        return await httpClient.SendAsync(request, ct);
    }

    public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T value, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(value),
        };
        ApplyAuthorizationHeader(request);
        return await httpClient.SendAsync(request, ct);
    }

    public async Task<HttpResponseMessage> PutAsJsonAsync<T>(string requestUri, T value, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, requestUri)
        {
            Content = JsonContent.Create(value),
        };
        ApplyAuthorizationHeader(request);
        return await httpClient.SendAsync(request, ct);
    }

    public async Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
        ApplyAuthorizationHeader(request);
        return await httpClient.SendAsync(request, ct);
    }

    public void Dispose()
    {
        httpClient.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<string> GenerateTokenAsync(string email, string password)
    {
        var loginRequest = new LoginRequest(email, password);
        var response = await httpClient.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Token generation failed with HTTP status {response.StatusCode}");
        }

        var authDto = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        if (authDto?.AccessToken is null)
        {
            throw new InvalidOperationException("Response did not contain a valid Access Token.");
        }

        return authDto.AccessToken;
    }

    private void SetAuthToken(string token)
    {
        _token = token;
    }

    private void ApplyAuthorizationHeader(HttpRequestMessage request)
    {
        if (!string.IsNullOrWhiteSpace(_token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }
    }
}
