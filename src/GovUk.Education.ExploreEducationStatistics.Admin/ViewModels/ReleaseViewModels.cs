#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.NamingUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.TimePeriodLabelFormatter;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public record ReleaseViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public Guid PublicationId { get; set; }

        public string PublicationTitle { get; set; } = string.Empty;

        public string PublicationSummary { get; set; } = string.Empty;

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

        public Contact Contact { get; set; } = null!;

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

        public bool CanViewRelease { get; set; }

        public bool CanUpdateRelease { get; init; }

        public bool CanDeleteRelease { get; init; }

        public bool CanMakeAmendmentOfRelease { get; init; }
    }

    public class ReleaseCreateRequest
    {
        public Guid PublicationId { get; set; }

        [Required] public ReleaseType Type { get; set; }

        [Required]
        [JsonConverter(typeof(TimeIdentifierJsonConverter))]
        public TimeIdentifier TimePeriodCoverage { get; init; }

        public string Slug => SlugFromTitle(Title);

        private string Title => Format(Year, TimePeriodCoverage);

        [Range(1000, 9999)]
        public int Year { get; init; }

        public Guid? TemplateReleaseId { get; init; }
    }

    public class ReleaseUpdateRequest
    {
        [Required] public ReleaseType Type { get; init; }

        [JsonConverter(typeof(TimeIdentifierJsonConverter))]
        [Required]
        public TimeIdentifier TimePeriodCoverage { get; init; }

        public string PreReleaseAccessList { get; init; } = String.Empty;

        public string Slug => SlugFromTitle(Title);

        private string Title => Format(Year, TimePeriodCoverage);

        [Range(1000, 9999)]
        public int Year { get; init; }
    }

    public class ReleaseSummaryViewModel
    {
        public Guid Id { get; init; }

        public string Title { get; init; } = string.Empty;

        public string Slug { get; init; } = string.Empty;

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

        public Guid? PreviousVersionId { get; init; }

        public ReleasePermissions? Permissions { get; set; }
    }
}
