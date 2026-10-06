namespace SynergyFlow.Domain.Common.Results.Abstractions;

public interface IResult
{
    bool IsSuccess { get; }

    bool IsFailure { get; }

    List<Error> Errors { get; }

    Error TopError { get; }
}

public interface IResult<out TValue> : IResult
{
    TValue Value { get; }
}
