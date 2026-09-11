namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

public record ReleaseSearchableDocument
{
    public required Guid ReleaseId { get; init; }
    public required string ReleaseSlug { get; init; }
    public required Guid ReleaseVersionId { get; init; }
    public required Guid PublicationId { get; init; }
    public required string PublicationSlug { get; init; }
    public required string Summary { get; init; }
    public required string PublicationTitle { get; init; }
    public required DateTimeOffset Published { get; init; }
    public required Guid ThemeId { get; init; }
    public required string ThemeTitle { get; init; }
    public required string ReleaseType { get; init; }
    public required int TypeBoost { get; init; }
    public required PublishingOrganisation[] PublishingOrganisations { get; init; }
    public required string HtmlContent { get; init; }
}

public record PublishingOrganisation
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
}
