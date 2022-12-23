using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record KeyStatisticDataBlockCreateRequest
{
    public Guid DataBlockId { get; set; }

    public string? Trend { get; set; }

    public string? GuidanceTitle { get; set; }

    public string? GuidanceText { get; set; }
}

public record KeyStatisticDataBlockUpdateRequest
{
    public string? Trend { get; set; }

    public string? GuidanceTitle { get; set; }

    public string? GuidanceText { get; set; }
}
