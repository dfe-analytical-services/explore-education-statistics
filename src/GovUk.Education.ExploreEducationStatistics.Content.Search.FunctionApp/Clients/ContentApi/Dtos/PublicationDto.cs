// ReSharper disable ClassNeverInstantiated.Global
namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi.Dtos;

public record PublicationDto
{
    public Guid Id { get; init; }
    
    /// <summary>
    /// Publication Slug
    /// </summary>
    public string? Slug { get; init; }
    public string? LatestReleaseSlug { get; init; }
    public string? Summary { get; init; }
    public string? Title { get; init; }
    public string? Theme { get; init; }
    public DateTimeOffset? Published { get; init; }
}
