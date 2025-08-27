namespace GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

public record IdTitleSlugViewModel : IdTitleViewModel
{
    public required string Slug { get; init; }
}
