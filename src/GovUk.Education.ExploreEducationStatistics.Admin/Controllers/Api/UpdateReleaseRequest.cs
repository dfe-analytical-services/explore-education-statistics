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

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    public enum PublishMethod
    {
        Scheduled,
        Immediate
    }

    public class UpdateReleaseRequest
    {
        [Required]
        public Guid TypeId { get; set; }

        [JsonConverter(typeof(TimeIdentifierJsonConverter))]
        [Required]
        public TimeIdentifier TimePeriodCoverage { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseStatus Status { get; set; }

        public string InternalReleaseNote { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PublishMethod? PublishMethod { get; set; }

        [DateTimeFormatValidator("yyyy-MM-dd")]
        public string PublishScheduled { get; set; }

        public DateTime? PublishScheduledDate
        {
            get
            {
                if (PublishScheduled == null || PublishScheduled == "") {
                    return null;
                }

                DateTime.TryParseExact(PublishScheduled, "yyyy-MM-dd", InvariantCulture,DateTimeStyles.None,
                    out var dateTime);
                return dateTime.AsStartOfDayUtc();
            }
        }

        [PartialDateValidator]
        public PartialDate NextReleaseDate { get; set; }

        [RegularExpression(@"^([0-9]{4})?$")]
        public string ReleaseName { get; set; }

        public string Slug => SlugFromTitle(Title);

        private string Title => Format(Year, TimePeriodCoverage);

        private int Year => int.Parse(ReleaseName);
    }
}