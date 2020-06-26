using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Newtonsoft.Json;
using static System.Globalization.CultureInfo;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.NamingUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.TimePeriodLabelFormatter;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    public class UpdateReleaseSummaryRequest
    {
        [Required]
        public Guid TypeId { get; set; }

        [JsonConverter(typeof(TimeIdentifierJsonConverter))]
        [Required]
        public TimeIdentifier TimePeriodCoverage { get; set; }

        [RegularExpression(@"(\d{4}-\d{2}-\d{2})", ErrorMessage = "Invalid date format. Expected yyyy-MM-dd")]
        public string PublishScheduled { get; set; }

        public DateTime? PublishScheduledDate =>
            DateTime.ParseExact(PublishScheduled, "yyyy-MM-dd", InvariantCulture, DateTimeStyles.None)
                .AsStartOfDayUtc();

        [PartialDateValidator]
        public PartialDate NextReleaseDate { get; set; }

        [RegularExpression(@"^([0-9]{4})?$")]
        public string ReleaseName { get; set; }

        public string Slug => SlugFromTitle(Title);

        private string Title => Format(Year, TimePeriodCoverage);

        private int Year => int.Parse(ReleaseName);
    }
}