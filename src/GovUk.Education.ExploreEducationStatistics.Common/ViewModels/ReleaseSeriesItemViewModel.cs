#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

public record ReleaseSeriesItemViewModel
{
    public Guid Id { get; set; }
    public bool IsLegacyLink { get; set; }
    public string Description { get; set; } = string.Empty;

    // used by EES release series item
    public Guid? ReleaseId { get; set; }
    public string? ReleaseSlug { get; set; }

    // used by legacy link series item
    public string? LegacyLinkUrl { get; set; }
}
