#nullable enable
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public record ReleaseSeriesItem
{
    /// <summary>
    /// Unique identifier for the ReleaseSeriesItem which exists to allow safely managing legacy links in the UI.
    /// </summary>
    public Guid Id { get; set; }

    public Guid? ReleaseId { get; set; }

    public string? LegacyLinkUrl { get; set; }
    public string? LegacyLinkDescription { get; set; }

    [JsonIgnore]
    public bool IsLegacyLink => ReleaseId == null;
}
