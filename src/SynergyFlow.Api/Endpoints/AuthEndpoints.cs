using SynergyFlow.Api.DTOs.Requests;
using SynergyFlow.Application.Common.Caching;
using SynergyFlow.Application.Features.Auth.Commands.Login;
using SynergyFlow.Application.Features.Auth.Commands.RefreshToken;
using SynergyFlow.Application.Features.Auth.DTOs;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace SynergyFlow.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v{version:apiVersion}/auth")
            .WithTags("Auth");

        group.MapPost("login", async (
            [FromBody] LoginRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new LoginCommand(request.Email, request.Password);
            var result = await sender.Send(command, ct);
            return result.ToOk();
        })
        .AllowAnonymous()
        .RequireRateLimiting(RateLimiterPolicies.Auth)
        .Produces<AuthResponseDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("refresh-token", async (
            [FromBody] RefreshTokenRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new RefreshTokenCommand(request.ExpiredAccessToken, request.RefreshToken);
            var result = await sender.Send(command, ct);
            return result.ToOk();
        })
        .AllowAnonymous()
        .Produces<AuthResponseDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden);

        return app;
    }
}
