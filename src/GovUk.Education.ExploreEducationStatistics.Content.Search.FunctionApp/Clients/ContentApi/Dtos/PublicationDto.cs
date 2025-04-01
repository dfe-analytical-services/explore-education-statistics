namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi.Dtos;

public record PublicationDto
{
    public Guid Id { get; init; }
    
    /// <summary>
    /// Publication Slug
    /// </summary>
    public string Slug { get; init; } = string.Empty;
    public required string LatestReleaseSlug { get; init; }
    public string Summary { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Theme { get; init; } = string.Empty;
    public DateTimeOffset Published { get; init; }
}
