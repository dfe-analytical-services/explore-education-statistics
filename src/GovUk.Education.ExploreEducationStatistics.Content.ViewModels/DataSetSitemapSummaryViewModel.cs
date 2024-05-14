namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record DataSetSitemapSummaryViewModel
{
    public string Id { get; set; } = string.Empty;

    public DateTime? LastModified { get; init; }
}
