#nullable enable

using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public class Result<T>
    where T : class
{
    public T? Value { get; set; }
    public ErrorViewModel[] Errors = [];

    public bool IsSuccess => Errors.Length == 0;
    public bool IsFailure => !IsSuccess;

    public Result(T value)
    {
        Value = value;
    }

    public Result(ErrorViewModel error)
    {
        Errors = [error];
    }

    public Result(ErrorViewModel[] errors)
    {
        Errors = errors;
    }

    public static implicit operator Result<T>(T value)
    {
        return new Result<T>(value);
    }

    public static implicit operator Result<T>(ErrorViewModel error)
    {
        return new Result<T>(error);
    }

    public static implicit operator Result<T>(ErrorViewModel[] errors)
    {
        return new Result<T>(errors);
    }
}
