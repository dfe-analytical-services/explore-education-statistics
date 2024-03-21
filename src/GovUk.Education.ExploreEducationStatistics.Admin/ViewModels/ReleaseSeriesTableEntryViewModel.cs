#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record ReleaseSeriesTableEntryViewModel
{
    public Guid Id { get; set; }
    public bool IsLegacyLink { get; set; }
    public string Description { get; set; } = string.Empty;

    // used by EES release series item
    public Guid? ReleaseId { get; set; }
    public string? PublicationSlug { get; set; } // @MarkFix redundant?
    public string? ReleaseSlug { get; set; }
    public bool? IsLatest { get; set; }
    public bool? IsPublished { get; set; }

    // used by legacy link series item
    public string? LegacyLinkUrl { get; set; }
}

