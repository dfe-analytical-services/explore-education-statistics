namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record ReleaseInfoViewModel
{
    public required Guid ReleaseId { get; init; }
    public required string ReleaseSlug { get; init; }
}
