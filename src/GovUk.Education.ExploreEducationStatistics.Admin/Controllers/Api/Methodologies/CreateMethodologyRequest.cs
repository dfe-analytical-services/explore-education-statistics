using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using static System.Globalization.CultureInfo;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Methodologies
{
    public class CreateMethodologyRequest
    {
        [Required]
        public string Title { get; set; }

        [DateTimeFormatValidator("yyyy-MM-dd")]
        public string PublishScheduled { get; set; }

        public DateTime? PublishScheduledDate
        {
            get
            {
                DateTime.TryParseExact(PublishScheduled, "yyyy-MM-dd", InvariantCulture,DateTimeStyles.None,
                    out var dateTime);
                return dateTime.AsStartOfDayUtc();
            }
        }

        // TODO EES-899 Contact in the request is being ignored!
    }
}