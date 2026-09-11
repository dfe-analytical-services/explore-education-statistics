using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi.Dtos;

internal record ReleaseSearchableDocumentDto
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
    public required string Type { get; init; }
    public required int TypeBoost { get; init; }
    public required ReleaseSearchableDocumentPublishingOrganisationDto[] PublishingOrganisations { get; init; }
    public required string HtmlContent { get; init; }

    public ReleaseSearchableDocument ToModel() =>
        new()
        {
            ReleaseId = ReleaseId,
            ReleaseVersionId = ReleaseVersionId,
            Published = Published,
            PublicationTitle = PublicationTitle,
            PublicationId = PublicationId,
            ThemeId = ThemeId,
            ThemeTitle = ThemeTitle,
            Summary = Summary,
            ReleaseType = Type,
            TypeBoost = TypeBoost,
            PublicationSlug = PublicationSlug,
            ReleaseSlug = ReleaseSlug,
            PublishingOrganisations =
            [
                .. PublishingOrganisations.Select(publishingOrganisation => new PublishingOrganisation
                {
                    Id = publishingOrganisation.Id,
                    Title = publishingOrganisation.Title,
                }),
            ],
            HtmlContent = HtmlContent,
        };
}

internal record ReleaseSearchableDocumentPublishingOrganisationDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
}
