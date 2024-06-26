#nullable enable

using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public class ReleaseSeriesLegacyLinkAddRequest
{
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class ReleaseSeriesItemUpdateRequest
{
    public Guid Id { get; set; }
    public Guid? ReleaseId { get; set; }

    public string? LegacyLinkDescription { get; set; }
    public string? LegacyLinkUrl { get; set; }
}

