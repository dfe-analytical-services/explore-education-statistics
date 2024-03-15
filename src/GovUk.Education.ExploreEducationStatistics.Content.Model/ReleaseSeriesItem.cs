#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class ReleaseSeriesItem
{
    public Guid Id { get; set; }

    public Guid? ReleaseId { get; set; }

    public string? LegacyLinkUrl { get; set; }
    public string? LegacyLinkDescription { get; set; }

    public bool IsLegacyLink => ReleaseId == null;
}
