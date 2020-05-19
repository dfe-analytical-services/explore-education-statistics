using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class MethodologySummaryViewModel
    {
        public Guid Id { get; set; }

        public DateTime? Published { get; set; }

        public DateTime? PublishScheduled { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MethodologyStatus Status { get; set; }

        public string Title { get; set; }
    }
}