using Generator.Equals;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;

[Equatable]
public partial record PublicationsTreeThemeDto
{
    public required Guid Id { get; init; }

    public required string Summary { get; init; }

    public required string Title { get; init; }

    [OrderedEquality]
    public required PublicationsTreePublicationDto[] Publications { get; init; }

    public static PublicationsTreeThemeDto FromTheme(Theme theme, PublicationsTreePublicationDto[] publications) =>
        new()
        {
            Id = theme.Id,
            Summary = theme.Summary,
            Title = theme.Title,
            Publications = publications,
        };
}

public record PublicationsTreePublicationDto
{
    public required Guid Id { get; init; }

    public required string Slug { get; init; }

    public required string Title { get; init; }

    public required PublicationsTreePublicationSupersededByPublicationDto? SupersededBy { get; init; }

    public required bool AnyLiveReleaseHasData { get; init; }

    public bool IsSuperseded => SupersededBy != null;

    public required bool LatestReleaseHasData { get; init; }

    public static PublicationsTreePublicationDto FromPublication(
        Publication publication,
        bool isSuperseded,
        bool latestReleaseHasData,
        bool anyLiveReleaseHasData
    ) =>
        new()
        {
            Id = publication.Id,
            Slug = publication.Slug,
            Title = publication.Title,
            SupersededBy = isSuperseded
                ? PublicationsTreePublicationSupersededByPublicationDto.FromPublication(
                    publication.SupersededBy
                        ?? throw new InvalidOperationException("Publication that is superseded must have SupersededBy")
                )
                : null,
            AnyLiveReleaseHasData = anyLiveReleaseHasData,
            LatestReleaseHasData = latestReleaseHasData,
        };
}

public record PublicationsTreePublicationSupersededByPublicationDto
{
    public required Guid Id { get; init; }

    public required string Slug { get; init; }

    public required string Title { get; init; }

    public static PublicationsTreePublicationSupersededByPublicationDto FromPublication(Publication publication) =>
        new()
        {
            Id = publication.Id,
            Slug = publication.Slug,
            Title = publication.Title,
        };
}
