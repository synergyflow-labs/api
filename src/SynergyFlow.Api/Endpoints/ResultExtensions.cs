using SynergyFlow.Domain.Common.Results;

namespace SynergyFlow.Api.Endpoints;

public static class ResultExtensions
{
    public static IResult ToOk<TValue>(this Result<TValue> result)
    {
        return result.Match(Results.Ok, Problem);
    }

    public static IResult ToCreated<TValue>(this Result<TValue> result, string uri)
    {
        return result.Match(val => Results.Created(uri, val), Problem);
    }

    public static IResult ToNoContent<TValue>(this Result<TValue> result)
    {
        return result.Match(
            _ => Results.NoContent(),
            Problem);
    }

    public static IResult Problem(List<Error> errors)
    {
        if (errors.Count is 0)
        {
            return Results.Problem();
        }

        if (errors.All(error => error.Type == ErrorKind.Validation))
        {
            return ValidationProblem(errors);
        }

        return Problem(errors[0]);
    }

    private static IResult Problem(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorKind.Conflict => StatusCodes.Status409Conflict,
            ErrorKind.Validation => StatusCodes.Status400BadRequest,
            ErrorKind.NotFound => StatusCodes.Status404NotFound,
            ErrorKind.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorKind.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };

        return Results.Problem(statusCode: statusCode, title: error.Description);
    }

    private static IResult ValidationProblem(List<Error> errors)
    {
        var dictionary = new Dictionary<string, string[]>();
        foreach (var error in errors)
        {
            if (dictionary.TryGetValue(error.Code, out var currentErrors))
            {
                dictionary[error.Code] = [.. currentErrors, error.Description];
            }
            else
            {
                dictionary[error.Code] = [error.Description];
            }
        }

        return Results.ValidationProblem(dictionary);
    }
}
