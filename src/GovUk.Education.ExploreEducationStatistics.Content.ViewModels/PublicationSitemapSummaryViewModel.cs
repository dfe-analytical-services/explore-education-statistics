namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record PublicationSitemapSummaryViewModel
{
    public string Slug { get; set; } = string.Empty;

    public DateTime? LastModified { get; init; }
    
    public List<ReleaseSitemapSummaryViewModel> Releases { get; set; }
}
