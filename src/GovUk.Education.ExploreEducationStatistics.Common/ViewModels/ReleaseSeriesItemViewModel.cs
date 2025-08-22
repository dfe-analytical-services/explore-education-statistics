#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

public record ReleaseSeriesItemViewModel
{
    public required string Description { get; init; }

    // used by EES release series item
    public Guid? ReleaseId { get; init; }
    public string? ReleaseSlug { get; init; }

    // used by legacy link series item
    public string? LegacyLinkUrl { get; init; }

    public bool IsLegacyLink => ReleaseId == null;
}
