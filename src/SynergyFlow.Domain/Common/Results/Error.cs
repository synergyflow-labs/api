namespace SynergyFlow.Domain.Common.Results;

public readonly record struct Error(string Code, string Description, ErrorKind Type = ErrorKind.Failure)
{
    public static Error Failure(string code = "General.Failure", string description = "A failure has occurred.") =>
        new(code, description, ErrorKind.Failure);

    public static Error Unexpected(string code = "General.Unexpected", string description = "An unexpected error has occurred.") =>
        new(code, description, ErrorKind.Unexpected);

    public static Error Validation(string code = "General.Validation", string description = "A validation error has occurred.") =>
        new(code, description, ErrorKind.Validation);

    public static Error Conflict(string code = "General.Conflict", string description = "A conflict has occurred.") =>
        new(code, description, ErrorKind.Conflict);

    public static Error NotFound(string code = "General.NotFound", string description = "A 'Not Found' error has occurred.") =>
        new(code, description, ErrorKind.NotFound);

    public static Error Unauthorized(string code = "General.Unauthorized", string description = "An 'Unauthorized' error has occurred.") =>
        new(code, description, ErrorKind.Unauthorized);

    public static Error Forbidden(string code = "General.Forbidden", string description = "A 'Forbidden' error has occurred.") =>
        new(code, description, ErrorKind.Forbidden);
}
