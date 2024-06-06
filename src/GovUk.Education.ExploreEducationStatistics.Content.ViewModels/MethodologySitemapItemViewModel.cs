namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record MethodologySitemapItemViewModel
{
    public required string Slug { get; init; }

    public DateTime? LastModified { get; init; }
}
