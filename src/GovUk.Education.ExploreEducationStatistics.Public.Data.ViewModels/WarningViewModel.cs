namespace GovUk.Education.ExploreEducationStatistics.Public.Data.ViewModels;

/// <summary>
/// A warning that points to a potential issue. This is not a critical error,
/// but may require attention to get the desired response.
/// </summary>
public record WarningViewModel
{
    /// <summary>
    /// The warning message.
    /// </summary>
    /// <example>There are no results.</example>
    public required string Message { get; init; }

    /// <summary>
    /// The path to the property on the request that the warning relates to.
    /// May be omitted or be empty if no specific property of the
    /// request relates to the warning (it is a 'global' warning).
    /// </summary>
    /// <example>results</example>
    public string? Path { get; init; }

    /// <summary>
    /// The warning's machine-readable code. Can be used for further
    /// processing of the warning before presenting to users.
    /// </summary>
    /// <example>NoResults</example>
    public required string Code { get; init; }

    /// <summary>
    /// Additional detail about the warning that can be used to provide
    /// more context to users. May be omitted if there is none.
    /// </summary>
    /// <example>{ "total": 0 }</example>
    public object? Detail { get; init; }
}
