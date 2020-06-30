using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static System.Globalization.CultureInfo;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Methodologies
{
    public class UpdateMethodologyRequest
    {
        public string InternalReleaseNote { get; set; }

        [Required] public string Title { get; set; }

        [DateTimeFormatValidator("yyyy-MM-dd")]
        public string PublishScheduled { get; set; }

        public DateTime? PublishScheduledDate
        {
            get
            {
                DateTime.TryParseExact(PublishScheduled, "yyyy-MM-dd", InvariantCulture,DateTimeStyles.None,
                    out var dateTime);
                return dateTime;
            }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public MethodologyStatus Status { get; set; }
    }
}