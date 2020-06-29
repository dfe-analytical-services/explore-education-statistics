using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Methodologies
{
    public class CreateMethodologyRequest
    {
        [Required] public string Title { get; set; }

        [RegularExpression(@"(^\d{4}-\d{2}-\d{2}$)", ErrorMessage = "Invalid date format. Expected yyyy-MM-dd")]
        public string PublishScheduled { get; set; }

        public DateTime? PublishScheduledDate =>
            DateTime.ParseExact(PublishScheduled, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None)
                .AsStartOfDayUtc();

        // TODO EES-899 Contact in the request is being ignored!
    }
}