using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi.Dtos;

internal record ReleaseSearchViewModelDto
{
    public Guid ReleaseId { get; init; }
    public Guid ReleaseVersionId { get; init; }
    public DateTimeOffset Published { get; init; } = DateTimeOffset.MinValue;
    public Guid PublicationId { get; init; }
    public string PublicationTitle { get; init; } = string.Empty;
    public Guid ThemeId { get; init; }
    public string ThemeTitle { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;

    public int TypeBoost { get; init; }

    public string PublicationSlug { get; init; } = string.Empty;
    public string ReleaseSlug { get; init; } = string.Empty;

    public string HtmlContent { get; init; } = string.Empty;

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
            HtmlContent = HtmlContent,
        };
}
