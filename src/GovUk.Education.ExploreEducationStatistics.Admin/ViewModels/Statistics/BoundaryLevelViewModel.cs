#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Statistics;

public record BoundaryLevelViewModel
{
    public required long Id { get; init; }

    public required string Level { get; init; }

    public required string Label { get; init; }

    public required DateTime Published { get; init; }
}
