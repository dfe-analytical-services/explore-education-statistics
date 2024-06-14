namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record PublicationSitemapItemViewModel
{
    public required string Slug { get; init; }

    public DateTime? LastModified { get; init; }
    
    public List<ReleaseSitemapItemViewModel>? Releases { get; init; }
}
