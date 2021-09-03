#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.NamingUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology
{
    public class MethodologyUpdateRequest
    {
        public string? LatestInternalReleaseNote { get; set; }

        // TODO SOW4 EES-2212 - update to AlternativeTitle
        [Required] public string Title { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MethodologyStatus Status { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MethodologyPublishingStrategy PublishingStrategy { get; set; }

        public Guid? WithReleaseId { get; set; }

        public string Slug => SlugFromTitle(Title);

        public bool IsStatusUpdateForMethodology(Content.Model.MethodologyVersion methodologyVersion)
        {
            return methodologyVersion.Status != Status
                   || methodologyVersion.PublishingStrategy != PublishingStrategy
                   || methodologyVersion.ScheduledWithReleaseId != WithReleaseId
                   || methodologyVersion.InternalReleaseNote != LatestInternalReleaseNote;
        }

        public bool IsDetailUpdateForMethodology(Content.Model.MethodologyVersion methodologyVersion)
        {
            return methodologyVersion.Title != Title;
        }
    }
}
