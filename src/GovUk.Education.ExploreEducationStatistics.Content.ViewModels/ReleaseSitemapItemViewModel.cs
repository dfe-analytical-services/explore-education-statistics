namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record ReleaseSitemapItemViewModel
{
    public required string Slug { get; init; }

    public DateTime? LastModified { get; init; }
}
