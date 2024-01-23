namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

public record PublicationSummaryViewModel
{
    public required Guid Id { get; init; }
    
    public required string Title { get; init; }
    
    public required string Slug { get; init; }
    
    public required string Summary { get; init; }
    
    public required DateTimeOffset LastPublished { get; init; }
}
