#enable nullable
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

        public string Title { get; set; }

        public string Slug { get; set;  }

        public Guid PublicationId { get; set; }

        public string PublicationTitle { get; set; }

        public string PublicationSlug { get; set; }

        public string ReleaseName { get; set; }

        public string YearTitle { get; set; }

        public Guid? TypeId { get; set; }

        public PartialDate NextReleaseDate { get; set; }

        [JsonConverter(typeof(DateTimeToDateJsonConverter))]
        public DateTime? PublishScheduled { get; set; }

        public DateTime? Published { get; set; }

        public bool Live => Published != null;

        [JsonConverter(typeof(TimeIdentifierJsonConverter))]
        public TimeIdentifier? TimePeriodCoverage { get; set; }

        public string PreReleaseAccessList { get; set; } = "";

        public bool LatestRelease { get; set; }

        public ReleaseType Type { get; set; }

        public Contact Contact { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseApprovalStatus ApprovalStatus { get; set; }

        public string LatestInternalReleaseNote { get; set; }

        public bool Amendment { get; set; }

        public Guid? PreviousVersionId { get; set; }
    }

    public class ReleaseCreateViewModel
    {
        public Guid PublicationId { get; set; }

        [Required] public Guid? TypeId { get; set; }

        [Required]
        [JsonConverter(typeof(TimeIdentifierJsonConverter))]
        public TimeIdentifier TimePeriodCoverage { get; set; }

        [DateTimeFormatValidator("yyyy-MM-dd")]
        public string PublishScheduled { get; set; }

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

        [PartialDateValidator] public PartialDate NextReleaseDate { get; set; }

        [RegularExpression(@"^([0-9]{4})?$")] public string ReleaseName { get; set; }

        public string Slug => SlugFromTitle(Title);

        private string Title => Format(Year, TimePeriodCoverage);

        private int Year => int.Parse(ReleaseName);

        public Guid? TemplateReleaseId { get; set; }
    }

    public class ReleaseUpdateViewModel
    {
        [Required] public Guid TypeId { get; set; }

        [JsonConverter(typeof(TimeIdentifierJsonConverter))]
        [Required]
        public TimeIdentifier TimePeriodCoverage { get; set; }

        public string PreReleaseAccessList { get; set; }

        [RegularExpression(@"^([0-9]{4})?$")] public string ReleaseName { get; set; }

        public string Slug => SlugFromTitle(Title);

        private string Title => Format(Year, TimePeriodCoverage);

        private int Year => int.Parse(ReleaseName);
    }

    public class ReleaseStatusCreateViewModel
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseApprovalStatus ApprovalStatus { get; set; }

        public string LatestInternalReleaseNote { get; set; }

        public bool? NotifySubscribers { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PublishMethod? PublishMethod { get; set; }

        [DateTimeFormatValidator("yyyy-MM-dd")]
        public string PublishScheduled { get; set; }

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

        [PartialDateValidator] public PartialDate NextReleaseDate { get; set; }
    }
}
