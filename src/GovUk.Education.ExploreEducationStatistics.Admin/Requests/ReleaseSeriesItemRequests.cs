#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record ReleaseSeriesLegacyLinkAddRequest
{
    public required string Description { get; init; }

    public required string Url { get; init; }
}

public record ReleaseSeriesItemUpdateRequest
{
    public Guid? ReleaseId { get; init; }

    public string? LegacyLinkDescription { get; init; }
    public string? LegacyLinkUrl { get; init; }
}
