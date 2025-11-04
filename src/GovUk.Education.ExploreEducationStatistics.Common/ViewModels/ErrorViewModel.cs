using FluentValidation.Results;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

/// <summary>
/// Describes an error that occurred with the request. This will typically
/// need to be rectified before the request can be fully processed.
/// </summary>
public record ErrorViewModel
{
    /// <summary>
    /// The error message.
    /// </summary>
    /// <example>Must be 50 characters or fewer.</example>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// The path to the property on the request that the error relates to.
    /// May be omitted or empty if no specific property of the
    /// request relates to the error (it is a 'global' error).
    /// </summary>
    /// <example>person.name</example>
    public string? Path { get; init; }

    /// <summary>
    /// The error's machine-readable code. Can be used for further
    /// processing of the error before presenting to users.
    /// May be omitted if there is none.
    /// </summary>
    /// <example>MaximumLength</example>
    public string? Code { get; init; }

    /// <summary>
    /// Additional detail about the error that can be used to provide
    /// more context to users. May be omitted if there is none.
    /// </summary>
    /// <example>
    /// {
    ///     "maxLength": 50
    /// }
    /// </example>
    public object? Detail { get; init; }

    public static ErrorViewModel Create(ValidationFailure failure)
    {
        var detail = failure.GetErrorDetail();
        var path = failure.PropertyName.Split('.').Select(part => part.ToLowerFirst()).JoinToString('.');

        return new ErrorViewModel
        {
            Path = path,
            Code = failure.ErrorCode.Replace("Validator", ""),
            Message = failure.ErrorMessage,
            Detail = detail.Count > 0 ? detail : null,
        };
    }
}
