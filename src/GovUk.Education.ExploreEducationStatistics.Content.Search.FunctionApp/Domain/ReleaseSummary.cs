namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

public record ReleaseSummary
{
    public required string Id { get; init; }
    public required string ReleaseId { get; init; }
    public required string Title { get; init; }
    public required string Slug { get; init; }
    public string? YearTitle { get; init; }
    public string? CoverageTitle { get; init; }
    public DateTimeOffset? Published { get; init; }
    public string? Type { get; init; }
    public bool? IsLatestRelease { get; init; }
    
    public string? PublicationId { get; init; }
    public string? PublicationTitle { get; init; }
    public string? PublicationSlug { get; init; }
}
