#nullable enable

using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public class ReleaseSeriesItemUpdateRequest
{
    public Guid Id { get; set; }
    public Guid? ReleaseParentId { get; set; }

    public string? LegacyLinkDescription { get; set; }
    public string? LegacyLinkUrl { get; set; }
}

