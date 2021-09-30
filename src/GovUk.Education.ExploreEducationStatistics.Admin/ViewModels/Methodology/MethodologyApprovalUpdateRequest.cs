#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology
{
    public class MethodologyApprovalUpdateRequest
    {
        public string? LatestInternalReleaseNote { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MethodologyStatus Status { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MethodologyPublishingStrategy PublishingStrategy { get; set; }

        public Guid? WithReleaseId { get; set; }

        public bool IsStatusUpdateForMethodology(MethodologyVersion methodologyVersion)
        {
            return methodologyVersion.Status != Status
                   || methodologyVersion.PublishingStrategy != PublishingStrategy
                   || methodologyVersion.ScheduledWithReleaseId != WithReleaseId
                   || methodologyVersion.InternalReleaseNote != LatestInternalReleaseNote;
        }
    }
}
