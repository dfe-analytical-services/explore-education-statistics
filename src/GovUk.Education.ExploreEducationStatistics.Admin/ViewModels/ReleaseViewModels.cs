using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static System.Globalization.CultureInfo;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.NamingUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.TimePeriodLabelFormatter;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class ReleaseViewModel
    {
        public Guid Id { get; set; }

        public string? Title { get; set; }

        public string? Slug { get; set; }

        public Guid PublicationId { get; set; }

        public string? PublicationTitle { get; set; }

        public string PublicationSummary { get; set; }

        public string? PublicationSlug { get; set; }

        public string? ReleaseName { get; set; }

        public string? YearTitle { get; set; }

        public PartialDate? NextReleaseDate { get; set; }

        [JsonConverter(typeof(DateTimeToDateJsonConverter))]
        public DateTime? PublishScheduled { get; set; }

        public DateTime? Published { get; set; }

        public bool Live => Published != null;

        [JsonConverter(typeof(TimeIdentifierJsonConverter))]
        public TimeIdentifier? TimePeriodCoverage { get; set; }

        public string PreReleaseAccessList { get; set; } = "";

        public bool LatestRelease { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseType Type { get; set; }

        public Contact? Contact { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseApprovalStatus ApprovalStatus { get; set; }

        public bool NotifySubscribers { get; set; }

        public string? LatestInternalReleaseNote { get; set; }

        public bool Amendment { get; set; }

        public Guid? PreviousVersionId { get; set; }

        public PermissionsSet? Permissions { get; set; }

        public class PermissionsSet
        {
            public bool CanAddPrereleaseUsers { get; init; }

            public bool CanUpdateRelease { get; init; }

            public bool CanDeleteRelease { get; init; }

            public bool CanMakeAmendmentOfRelease { get; init; }
        }
    }

    public class ReleaseCreateViewModel
    {
        public Guid PublicationId { get; set; }

        [Required] public ReleaseType Type { get; set; }

        [Required]
        [JsonConverter(typeof(TimeIdentifierJsonConverter))]
        public TimeIdentifier TimePeriodCoverage { get; init; }

        [DateTimeFormatValidator("yyyy-MM-dd")]
        public string? PublishScheduled { get; init; }

        public DateTime? PublishScheduledDate
        {
            get
            {
                if (PublishScheduled == null)
                {
                    return null;
                }

                DateTime.TryParseExact(
                    PublishScheduled,
                    "yyyy-MM-dd",
                    InvariantCulture,
                    DateTimeStyles.None,
                    out var dateTime
                );
                return dateTime.AsStartOfDayUtcForTimeZone();
            }
        }

        [PartialDateValidator] public PartialDate? NextReleaseDate { get; set; }

        [RegularExpression(@"^([0-9]{4})?$")] public string? ReleaseName { get; init; }

        public string Slug => SlugFromTitle(Title);

        private string? Title => Year.HasValue ? Format(Year.Value, TimePeriodCoverage) : null;

        private int? Year => ReleaseName != null ? int.Parse(ReleaseName) : null;

        public Guid? TemplateReleaseId { get; init; }
    }

    public class ReleaseUpdateViewModel
    {
        [Required] public ReleaseType Type { get; init; }

        [JsonConverter(typeof(TimeIdentifierJsonConverter))]
        [Required]
        public TimeIdentifier TimePeriodCoverage { get; init; }

        public string? PreReleaseAccessList { get; init; }

        [RegularExpression(@"^([0-9]{4})?$")] public string? ReleaseName { get; init; }

        public string Slug => SlugFromTitle(Title);

        private string? Title => Year.HasValue ? Format(Year.Value, TimePeriodCoverage) : null;

        private int? Year => ReleaseName != null ? int.Parse(ReleaseName) : null;
    }

    public class ReleaseStatusCreateViewModel
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseApprovalStatus ApprovalStatus { get; init; }

        public string? LatestInternalReleaseNote { get; init; }

        public bool? NotifySubscribers { get; init; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PublishMethod? PublishMethod { get; init; }

        [DateTimeFormatValidator("yyyy-MM-dd")]
        public string? PublishScheduled { get; init; }

        public DateTime? PublishScheduledDate
        {
            get
            {
                if (PublishScheduled.IsNullOrEmpty())
                {
                    return null;
                }

                DateTime.TryParseExact(
                    PublishScheduled,
                    "yyyy-MM-dd",
                    InvariantCulture,
                    DateTimeStyles.None,
                    out var dateTime
                );
                return dateTime.AsStartOfDayUtcForTimeZone();
            }
        }

        [PartialDateValidator] public PartialDate? NextReleaseDate { get; init; }
    }

    public class ReleaseListItemViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public bool Live { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseApprovalStatus ApprovalStatus { get; set; }
    }
}
