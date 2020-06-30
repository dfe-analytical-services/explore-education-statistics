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

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class CreateReleaseViewModel
    {
        public Guid PublicationId { get; set; }

        [Required]
        public Guid? TypeId { get; set; }

        [Required]
        [JsonConverter(typeof(TimeIdentifierJsonConverter))]
        public TimeIdentifier TimePeriodCoverage { get; set; }

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

        [RegularExpression(@"^([0-9]{4})?$")] public string ReleaseName { get; set; }

        public string Slug => SlugFromTitle(Title);

        private string Title => Format(Year, TimePeriodCoverage);

        private int Year => int.Parse(ReleaseName);

        public Guid? TemplateReleaseId { get; set; }
    }
}