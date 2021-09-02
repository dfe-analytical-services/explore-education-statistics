#nullable enable
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

        public Guid MethodologyParentId { get; set; }

        public string? LatestInternalReleaseNote { get; set; }

        public TitleAndIdViewModel OwningPublication { get; set; }

        public List<TitleAndIdViewModel> OtherPublications { get; set; } = new List<TitleAndIdViewModel>();

        public DateTime? Published { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MethodologyPublishingStrategy PublishingStrategy { get; set; }

        public TitleAndIdViewModel? ScheduledWithRelease { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MethodologyStatus Status { get; set; }

        public string Title { get; set; } = string.Empty;

        public bool Amendment { get; set; }

        public Guid? PreviousVersionId { get; set; }
    }
}
