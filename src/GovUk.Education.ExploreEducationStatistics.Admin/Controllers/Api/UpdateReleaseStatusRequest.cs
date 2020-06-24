using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Globalization;
using static System.Globalization.CultureInfo;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    public enum PublishMethod
    {
        Scheduled,
        Immediate
    }

    public class UpdateReleaseStatusRequest
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseStatus Status { get; set; }

        public string InternalReleaseNote { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PublishMethod PublishMethod { get; set; }

        [DateTimeFormatValidator("yyyy-MM-dd")]
        public string PublishScheduled { get; set; }

        public DateTime? PublishScheduledDate
        {
            get
            {
                DateTime.TryParseExact(PublishScheduled, "yyyy-MM-dd", InvariantCulture, DateTimeStyles.None,
                    out var dateTime);
                return dateTime.AsStartOfDayUtc();
            }
        }

        [PartialDateValidator]
        public PartialDate NextReleaseDate { get; set; }
    }
}