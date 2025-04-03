#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
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
        public Guid Id { get; set; }

        public Guid ReleaseId { get; set; }
        
        public string Title { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string? Label { get; set;}

        public int Version { get; set; }

        public Guid PublicationId { get; set; }

        public string PublicationTitle { get; set; } = string.Empty;

        public string PublicationSlug { get; set; } = string.Empty;

        public int Year { get; set; }

        public string YearTitle { get; set; } = string.Empty;

        public PartialDate? NextReleaseDate { get; set; }

        [JsonConverter(typeof(DateTimeToDateJsonConverter))]
        public DateTime? PublishScheduled { get; set; }

        public DateTime? Published { get; set; }

        public bool Live => Published != null;

        [JsonConverter(typeof(TimeIdentifierJsonConverter))]
        public TimeIdentifier TimePeriodCoverage { get; set; }

        public string PreReleaseAccessList { get; set; } = string.Empty;

        public bool PreReleaseUsersOrInvitesAdded { get; set; }

        public bool LatestRelease { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseType Type { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseApprovalStatus ApprovalStatus { get; set; }

        public bool NotifySubscribers { get; set; }

        public string? LatestInternalReleaseNote { get; set; }

        public bool Amendment { get; set; }

        public Guid? PreviousVersionId { get; set; }

        public ReleasePermissions? Permissions { get; set; }

        public bool UpdatePublishedDate { get; set; }
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

        public DateTime? Published { get; init; }

        public bool Live => Published != null;

        [JsonConverter(typeof(DateTimeToDateJsonConverter))]
        public DateTime? PublishScheduled { get; init; }

        public PartialDate? NextReleaseDate { get; init; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseType Type { get; init; }

        public bool Amendment { get; init; }

        public bool LatestRelease { get; init; }

        public Guid? PreviousVersionId { get; init; }

        public ReleasePermissions? Permissions { get; set; }

        public PublicationSummaryViewModel? Publication { get; set; }
    }
}
