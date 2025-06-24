#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record ScreenerTestResultViewModel
{
    /// <summary>
    /// An ID to enable client code to differentiate results in the same collection.
    /// </summary>
    /// <remarks><see cref="TestFunctionName">TestFunctionName</see> won't always be unique, as the same screening function can be called for each file in a result set.</remarks>
    public Guid Id { get; init; } = Guid.NewGuid();

    public required string TestFunctionName { get; init; }

    public required string Result { get; init; }

    public string? Notes { get; init; }

    public required string Stage { get; init; }
}
