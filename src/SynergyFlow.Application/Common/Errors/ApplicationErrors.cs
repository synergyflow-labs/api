using SynergyFlow.Domain.Common.Results;

namespace SynergyFlow.Application.Common.Errors;

public static class ApplicationErrors
{
    public static readonly Error MissingRefreshToken = Error.Validation(
        code: "Auth.RefreshToken.Missing",
        description: "Refresh token is missing from the request.");

    public static readonly Error InvalidRefreshToken = Error.NotFound(
        code: "Auth.RefreshToken.Invalid",
        description: "Refresh token is invalid or does not exist.");

    public static readonly Error InvalidCredentials = Error.Unauthorized(
        code: "Auth.InvalidCredentials",
        description: "Invalid email or password.");

    public static readonly Error ExpiredOrRevokedRefreshToken = Error.Forbidden(
        code: "Auth.RefreshToken.ExpiredOrRevoked",
        description: "Refresh token has expired or has been revoked.");

    public static readonly Error ExpiredAccessTokenInvalid = Error.Validation(
        code: "Auth.ExpiredAccessToken.Invalid",
        description: "Expired access token is not valid.");

    public static readonly Error UserIdClaimInvalid = Error.Validation(
        code: "Auth.UserIdClaim.Invalid",
        description: "Invalid user ID claim.");

    public static readonly Error UserNotFound = Error.NotFound(
        code: "Auth.User.NotFound",
        description: "User not found.");

    public static readonly Error TokenGenerationFailed = Error.Failure(
        code: "Auth.TokenGeneration.Failed",
        description: "Failed to generate new JWT token.");

    public static readonly Error UserEmailAlreadyExists = Error.Conflict(
        code: "User.Email.AlreadyExists",
        description: "A user with this email address already exists.");
}
