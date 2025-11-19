#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record PublicationViewModel
{
    public required Guid Id { get; init; }

    public required string Slug { get; init; }

    public required string Summary { get; init; }

    public required string Title { get; init; }

    public required PublicationContactDto Contact { get; init; }

    public required PublicationThemeDto Theme { get; init; }

    public required Guid? SupersededById { get; init; }

    // NOTE: IsSuperseded is necessary, as a publication only becomes superseded when it's SupersededById is set
    // _and_ that publication has a live release.
    public required bool IsSuperseded { get; init; }

    public PublicationPermissionsDto? Permissions { get; set; }

    public static PublicationViewModel FromModel(Publication publication, bool isSuperseded) =>
        new()
        {
            Id = publication.Id,
            Slug = publication.Slug,
            Summary = publication.Summary,
            Title = publication.Title,
            SupersededById = publication.SupersededById,
            IsSuperseded = isSuperseded,
            Contact = PublicationContactDto.FromModel(publication.Contact),
            Theme = PublicationThemeDto.FromModel(publication.Theme),
        };
}

public record PublicationPermissionsDto
{
    public required bool CanUpdatePublication { get; init; }

    public required bool CanUpdatePublicationSummary { get; init; }

    public required bool CanCreateReleases { get; init; }

    public required bool CanAdoptMethodologies { get; init; }

    public required bool CanCreateMethodologies { get; init; }

    public required bool CanManageExternalMethodology { get; init; }

    public required bool CanManageReleaseSeries { get; init; }

    public required bool CanUpdateContact { get; init; }

    public required bool CanUpdateContributorReleaseRole { get; init; }

    public required bool CanViewReleaseTeamAccess { get; init; }
}

public record PublicationContactDto
{
    public required Guid Id { get; init; }

    public required string ContactName { get; init; }

    public required string? ContactTelNo { get; init; }

    public required string TeamEmail { get; init; }

    public required string TeamName { get; init; }

    public static PublicationContactDto FromModel(Contact contact) =>
        new()
        {
            Id = contact.Id,
            ContactName = contact.ContactName,
            ContactTelNo = contact.ContactTelNo,
            TeamEmail = contact.TeamEmail,
            TeamName = contact.TeamName,
        };
}

public record PublicationThemeDto
{
    public required Guid Id { get; init; }

    public required string Summary { get; init; }

    public required string Title { get; init; }

    public static PublicationThemeDto FromModel(Theme theme) =>
        new()
        {
            Id = theme.Id,
            Summary = theme.Summary,
            Title = theme.Title,
        };
}
