namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public class PublicationSitemapSummaryViewModel
{
    public string Slug { get; set; } = string.Empty;

    public DateTime? LastModified { get; init; }
    
    public List<ReleaseSitemapSummaryViewModel> Releases { get; set; }
}
