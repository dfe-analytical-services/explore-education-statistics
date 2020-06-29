using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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

        [RegularExpression(@"(^\d{4}-\d{2}-\d{2}$)", ErrorMessage = "Invalid date format. Expected yyyy-MM-dd")]
        public string PublishScheduled { get; set; }

        public DateTime? PublishScheduledDate =>
            DateTime.ParseExact(PublishScheduled, "yyyy-MM-dd", InvariantCulture, DateTimeStyles.None)
                .AsStartOfDayUtc();

        [JsonConverter(typeof(StringEnumConverter))]
        public MethodologyStatus Status { get; set; }
    }
}