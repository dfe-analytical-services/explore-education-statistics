#nullable enable
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

        public string Title { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public Guid PublicationId { get; set; }

        public string PublicationTitle { get; set; } = string.Empty;

        public string PublicationSummary { get; set; } = string.Empty;

        public string PublicationSlug { get; set; } = string.Empty;

        public string ReleaseName { get; set; } = string.Empty;

        public string YearTitle { get; set; } = string.Empty;

        public PartialDate NextReleaseDate { get; set; } = null!;

        [JsonConverter(typeof(DateTimeToDateJsonConverter))]
        public DateTime? PublishScheduled { get; set; }

        public DateTime? Published { get; set; }

        public bool Live => Published != null;

        [JsonConverter(typeof(TimeIdentifierJsonConverter))]
        public TimeIdentifier? TimePeriodCoverage { get; set; }

        public string PreReleaseAccessList { get; set; } = string.Empty;

        public bool LatestRelease { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseType Type { get; set; }

        public Contact Contact { get; set; } = null!;

        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseApprovalStatus ApprovalStatus { get; set; }

        public bool NotifySubscribers { get; set; }

        public string LatestInternalReleaseNote { get; set; } = string.Empty;

        public bool Amendment { get; set; }

        public Guid? PreviousVersionId { get; set; }

        public ReleasePermissions? Permissions { get; set; }
    }
    
    public record ReleasePermissions
    {
        public bool CanAddPrereleaseUsers { get; init; }

        public bool CanUpdateRelease { get; init; }

        public bool CanDeleteRelease { get; init; }

        public bool CanMakeAmendmentOfRelease { get; init; }
    }

    public class ReleaseCreateViewModel
    {
        public Guid PublicationId { get; set; }

        [Required] public ReleaseType Type { get; set; }

        [Required]
        [JsonConverter(typeof(TimeIdentifierJsonConverter))]
        public TimeIdentifier TimePeriodCoverage { get; init; }

        [DateTimeFormatValidator("yyyy-MM-dd")]
        public string PublishScheduled { get; init; } = string.Empty;

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

        [PartialDateValidator] public PartialDate? NextReleaseDate { get; set; } = null!;

        [RegularExpression(@"^([0-9]{4})?$")] public string ReleaseName { get; init; } = string.Empty;

        public string Slug => SlugFromTitle(Title);

        private string Title => Format(Year, TimePeriodCoverage);

        private int Year => int.Parse(ReleaseName);

        public Guid? TemplateReleaseId { get; init; }
    }

    public class ReleaseUpdateViewModel
    {
        [Required] public ReleaseType Type { get; init; }

        [JsonConverter(typeof(TimeIdentifierJsonConverter))]
        [Required]
        public TimeIdentifier TimePeriodCoverage { get; init; }

        public string PreReleaseAccessList { get; init; } = String.Empty;

        [RegularExpression(@"^([0-9]{4})?$")] public string ReleaseName { get; init; } = string.Empty;

        public string Slug => SlugFromTitle(Title);

        private string Title => Format(Year, TimePeriodCoverage);

        private int Year => int.Parse(ReleaseName);
    }

    public class ReleaseStatusCreateViewModel
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseApprovalStatus ApprovalStatus { get; init; }

        public string LatestInternalReleaseNote { get; init; } = string.Empty;

        public bool? NotifySubscribers { get; init; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PublishMethod? PublishMethod { get; init; }

        [DateTimeFormatValidator("yyyy-MM-dd")]
        public string PublishScheduled { get; init; } = string.Empty;

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
        public Guid Id { get; init; }

        public string Title { get; init; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public int Year { get; set; }

        [JsonConverter(typeof(TimeIdentifierJsonConverter))]
        public TimeIdentifier? TimePeriodCoverage { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseApprovalStatus ApprovalStatus { get; set; }

        public DateTime? Published { get; set; }

        public bool Live { get; set; }

        [JsonConverter(typeof(DateTimeToDateJsonConverter))]
        public DateTime? PublishScheduled { get; set; }

        public PartialDate NextReleaseDate { get; set; } = null!;

        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseType Type { get; set; }

        public string LatestInternalReleaseNote { get; set; } = string.Empty;

        public bool Amendment { get; set; }
        
        public ReleasePermissions? Permissions { get; set; }
    }
}
