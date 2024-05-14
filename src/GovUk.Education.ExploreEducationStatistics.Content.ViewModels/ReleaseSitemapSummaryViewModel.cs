namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record ReleaseSitemapSummaryViewModel
{
    public string Slug { get; set; } = string.Empty;

    public DateTime? LastModified { get; init; }
}
