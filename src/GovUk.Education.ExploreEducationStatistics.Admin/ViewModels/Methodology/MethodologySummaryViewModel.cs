using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology
{
    public class MethodologySummaryViewModel
    {
        public Guid Id { get; set; }

        public string InternalReleaseNote { get; set; }

        public TitleAndIdViewModel OwningPublication { get; set; }

        public List<TitleAndIdViewModel> OtherPublications { get; set; }

        public DateTime? Published { get; set; }

        public TitleAndIdViewModel ScheduledWithRelease { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MethodologyStatus Status { get; set; }

        public string Title { get; set; }

        public bool Amendment { get; set; }

        public Guid PreviousVersionId { get; set; }
    }
}
