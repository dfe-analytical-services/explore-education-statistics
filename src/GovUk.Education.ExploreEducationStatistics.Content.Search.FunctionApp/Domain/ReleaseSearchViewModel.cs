namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

public record ReleaseSearchableDocument
{
    public Guid ReleaseId { get; init; }
    public Guid ReleaseVersionId { get; init; }
    public DateTimeOffset Published { get; init; } = DateTimeOffset.MinValue;
    public string PublicationTitle { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string Theme { get; init; } = string.Empty;
    public string ReleaseType { get; init; } = string.Empty;

    public int TypeBoost { get; init; }

    public string PublicationSlug { get; init; } = string.Empty;
    public string ReleaseSlug { get; init; } = string.Empty;

    public string HtmlContent { get; init; } = string.Empty;
}
