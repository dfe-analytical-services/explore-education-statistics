using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;

public record PublicationDto
{
    public required Guid Id { get; init; }

    public required string Slug { get; init; }

    public required string Summary { get; init; }

    public required string Title { get; init; }

    public required PublicationContactDto Contact { get; init; }

    public required PublicationLatestReleaseDto LatestRelease { get; init; }

    public required PublicationSupersededByPublicationDto? SupersededByPublication { get; init; }

    public required PublicationThemeDto Theme { get; init; }

    public static PublicationDto FromPublication(
        Publication publication,
        Publication? supersededByPublication) =>
        new()
        {
            Id = publication.Id,
            Slug = publication.Slug,
            Summary = publication.Summary,
            Title = publication.Title,
            Contact = PublicationContactDto.FromContact(publication.Contact),
            LatestRelease = PublicationLatestReleaseDto.FromRelease(publication.LatestPublishedReleaseVersion!.Release),
            SupersededByPublication = supersededByPublication is null
                ? null
                : PublicationSupersededByPublicationDto.FromPublication(supersededByPublication),
            Theme = PublicationThemeDto.FromTheme(publication.Theme)
        };
}

public record PublicationContactDto
{
    public required Guid Id { get; init; }

    public required string ContactName { get; init; }

    public required string? ContactTelNo { get; init; }

    public required string TeamEmail { get; init; }

    public required string TeamName { get; init; }

    public static PublicationContactDto FromContact(Contact contact) =>
        new()
        {
            Id = contact.Id,
            ContactName = contact.ContactName,
            ContactTelNo = contact.ContactTelNo,
            TeamEmail = contact.TeamEmail,
            TeamName = contact.TeamName
        };
}

public record PublicationLatestReleaseDto
{
    public required Guid Id { get; init; }

    public required string Slug { get; init; }

    public required string Title { get; init; }

    public static PublicationLatestReleaseDto FromRelease(Release release) =>
        new()
        {
            Id = release.Id,
            Slug = release.Slug,
            Title = release.Title
        };
}

public record PublicationSupersededByPublicationDto
{
    public required Guid Id { get; init; }

    public required string Slug { get; init; }

    public required string Title { get; init; }

    public static PublicationSupersededByPublicationDto FromPublication(Publication publication) =>
        new()
        {
            Id = publication.Id,
            Slug = publication.Slug,
            Title = publication.Title
        };
}

public record PublicationThemeDto
{
    public required Guid Id { get; init; }

    public required string Summary { get; init; }

    public required string Title { get; init; }

    public static PublicationThemeDto FromTheme(Theme theme) =>
        new()
        {
            Id = theme.Id,
            Summary = theme.Summary,
            Title = theme.Title
        };
}
