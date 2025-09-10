#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record ReleaseSeriesTableEntryViewModel
{
    public required Guid Id { get; init; }
    public required string Description { get; init; }

    // used by EES release series item
    public Guid? ReleaseId { get; init; }
    public string? ReleaseSlug { get; init; }
    public bool? IsLatest { get; init; }
    public bool? IsPublished { get; init; }

    // used by legacy link series item
    public string? LegacyLinkUrl { get; init; }

    public bool IsLegacyLink => ReleaseId == null;
}
