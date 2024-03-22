#nullable enable
using System;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public record ReleaseSeriesItem
{
    public Guid Id { get; set; }

    public Guid? ReleaseId { get; set; }

    public string? LegacyLinkUrl { get; set; }
    public string? LegacyLinkDescription { get; set; }

    [JsonIgnore]
    public bool IsLegacyLink => ReleaseId == null;
}
