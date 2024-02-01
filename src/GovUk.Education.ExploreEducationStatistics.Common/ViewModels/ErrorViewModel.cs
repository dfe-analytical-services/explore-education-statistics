#nullable enable

namespace GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

public record ErrorViewModel
{
    /// <summary>
    /// The error message.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// The path to the property on the request that the error relates to.
    /// May be omitted or be empty if no specific property of the
    /// request relates to the error (it is a 'global' error).
    /// </summary>
    public string? Path { get; init; }

    /// <summary>
    /// The error's machine-readable code. Can be used for further
    /// processing of the error before presenting to users.
    /// May be omitted if there is none.
    /// </summary>
    public string? Code { get; init; }

    /// <summary>
    /// Additional detail about the error that can be used to provide
    /// more context to users. May be omitted if there is none.
    /// </summary>
    public object? Detail { get; init; }
}
