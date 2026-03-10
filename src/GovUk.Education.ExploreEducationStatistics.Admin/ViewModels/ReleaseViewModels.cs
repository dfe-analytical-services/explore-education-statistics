#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record ReleaseViewModel
{
    public required Guid Id { get; init; }

    public required Guid PublicationId { get; init; }

    public required string Slug { get; init; }

    [JsonConverter(typeof(TimeIdentifierJsonConverter))]
    public required TimeIdentifier TimePeriodCoverage { get; init; }

    public required int Year { get; init; }

    public string? Label { get; init; }

    public required string Title { get; init; }
}

public record ReleaseVersionViewModel
{
    public Guid Id { get; init; }

    public Guid ReleaseId { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public string? Label { get; init; }

    public int Version { get; init; }

    public Guid PublicationId { get; init; }

    public string PublicationTitle { get; init; } = string.Empty;

    public string PublicationSlug { get; init; } = string.Empty;

    public int Year { get; init; }

    public string YearTitle { get; init; } = string.Empty;

    public PartialDate? NextReleaseDate { get; init; }

    public DateOnly? PublishScheduled { get; init; }

    public List<OrganisationViewModel> PublishingOrganisations { get; init; } = [];

    public DateTimeOffset? Published { get; init; }

    public bool Live => Published != null;

    [JsonConverter(typeof(TimeIdentifierJsonConverter))]
    public TimeIdentifier TimePeriodCoverage { get; init; }

    public string? PreReleaseAccessList { get; init; }

    public bool PreReleaseUsersOrInvitesAdded { get; init; }

    public bool LatestRelease { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public ReleaseType Type { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public ReleaseApprovalStatus ApprovalStatus { get; init; }

    public bool NotifySubscribers { get; init; }

    public string? LatestInternalReleaseNote { get; init; }

    public bool Amendment { get; init; }

    public Guid? PreviousVersionId { get; init; }

    public bool UpdatePublishedDisplayDate { get; init; }

    public static ReleaseVersionViewModel FromReleaseVersion(
        ReleaseVersion releaseVersion,
        bool preReleaseUsersOrInvitesAdded,
        List<OrganisationViewModel> publishingOrganisations
    ) =>
        new()
        {
            Id = releaseVersion.Id,
            ReleaseId = releaseVersion.ReleaseId,
            Title = releaseVersion.Release.Title,
            Slug = releaseVersion.Release.Slug,
            Label = releaseVersion.Release.Label,
            Version = releaseVersion.Version,
            PublicationId = releaseVersion.Release.PublicationId,
            PublicationTitle = releaseVersion.Release.Publication.Title,
            PublicationSlug = releaseVersion.Release.Publication.Slug,
            Year = releaseVersion.Release.Year,
            YearTitle = releaseVersion.Release.YearTitle,
            NextReleaseDate = releaseVersion.NextReleaseDate,
            PublishScheduled = releaseVersion.PublishScheduled?.ToUkDateOnly(),
            PublishingOrganisations = publishingOrganisations,
            Published = releaseVersion.PublishedDisplayDate,
            TimePeriodCoverage = releaseVersion.Release.TimePeriodCoverage,
            PreReleaseAccessList = releaseVersion.PreReleaseAccessList,
            PreReleaseUsersOrInvitesAdded = preReleaseUsersOrInvitesAdded,
            LatestRelease = releaseVersion.Release.Publication.LatestPublishedReleaseVersionId == releaseVersion.Id,
            Type = releaseVersion.Type,
            ApprovalStatus = releaseVersion.ApprovalStatus,
            NotifySubscribers = releaseVersion.NotifySubscribers,
            LatestInternalReleaseNote = releaseVersion.LatestInternalReleaseNote,
            Amendment = releaseVersion.Amendment,
            PreviousVersionId = releaseVersion.PreviousVersionId,
            UpdatePublishedDisplayDate = releaseVersion.UpdatePublishedDisplayDate,
        };
}

public record ReleasePermissions
{
    public bool CanAddPrereleaseUsers { get; init; }

    public bool CanUpdateRelease { get; init; }

    public bool CanViewReleaseVersion { get; set; }

    public bool CanUpdateReleaseVersion { get; init; }

    public bool CanDeleteReleaseVersion { get; init; }

    public bool CanMakeAmendmentOfReleaseVersion { get; init; }
}

public record ReleaseVersionSummaryViewModel
{
    public Guid Id { get; init; }

    public Guid ReleaseId { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public string? Label { get; init; }

    public int Year { get; init; }

    public string YearTitle { get; init; } = string.Empty;

    [JsonConverter(typeof(TimeIdentifierJsonConverter))]
    public TimeIdentifier TimePeriodCoverage { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public ReleaseApprovalStatus ApprovalStatus { get; init; }

    public DateTimeOffset? Published { get; init; }

    public bool Live => Published != null;

    public DateOnly? PublishScheduled { get; init; }

    public PartialDate? NextReleaseDate { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public ReleaseType Type { get; init; }

    public bool Amendment { get; init; }

    public Guid? PreviousVersionId { get; init; }

    public ReleasePermissions? Permissions { get; init; }

    public PublicationSummaryViewModel? Publication { get; init; }

    public static ReleaseVersionSummaryViewModel FromReleaseVersion(
        ReleaseVersion releaseVersion,
        ReleasePermissions? permissions = null,
        PublicationSummaryViewModel? publicationSummary = null
    ) =>
        new()
        {
            Id = releaseVersion.Id,
            ReleaseId = releaseVersion.ReleaseId,
            Title = releaseVersion.Release.Title,
            Slug = releaseVersion.Release.Slug,
            Label = releaseVersion.Release.Label,
            Year = releaseVersion.Release.Year,
            YearTitle = releaseVersion.Release.YearTitle,
            TimePeriodCoverage = releaseVersion.Release.TimePeriodCoverage,
            ApprovalStatus = releaseVersion.ApprovalStatus,
            Published = releaseVersion.PublishedDisplayDate,
            PublishScheduled = releaseVersion.PublishScheduled?.ToUkDateOnly(),
            NextReleaseDate = releaseVersion.NextReleaseDate,
            Type = releaseVersion.Type,
            Amendment = releaseVersion.Amendment,
            PreviousVersionId = releaseVersion.PreviousVersionId,
            Permissions = permissions,
            Publication = publicationSummary,
        };
}
