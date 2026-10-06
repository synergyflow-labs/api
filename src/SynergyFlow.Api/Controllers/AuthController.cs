using SynergyFlow.Api.DTOs.Requests;
using SynergyFlow.Application.Common.Caching;
using SynergyFlow.Application.Features.Auth.Commands.Login;
using SynergyFlow.Application.Features.Auth.Commands.RefreshToken;
using SynergyFlow.Application.Features.Auth.DTOs;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SynergyFlow.Api.Controllers;

[Route("api/v{version:apiVersion}/auth")]
public class AuthController(ISender sender) : ApiController
{
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimiterPolicies.Auth)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await sender.Send(command, ct);

        return HandleResult(result);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct)
    {
        var command = new RefreshTokenCommand(request.ExpiredAccessToken, request.RefreshToken);
        var result = await sender.Send(command, ct);

        return HandleResult(result);
    }
}
