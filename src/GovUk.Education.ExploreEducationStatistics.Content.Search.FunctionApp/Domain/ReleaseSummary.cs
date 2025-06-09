namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

public record ReleaseSummary
{
    public required string Id { get; init; }
    public required string ReleaseId { get; init; }
    public required string Title { get; init; }
    public required string Slug { get; init; }
    public required string YearTitle { get; init; }
    public required string CoverageTitle { get; init; }
    public DateTimeOffset Published { get; init; }
    public required string Type { get; init; }
    public bool LatestRelease { get; init; }
    
    public required string PublicationId { get; init; }
    public required string PublicationTitle { get; init; }
    public required string PublicationSlug { get; init; }
}
