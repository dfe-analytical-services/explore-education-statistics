#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;

public class MethodologyApprovalUpdateRequest
{
    public string? LatestInternalReleaseNote { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public MethodologyApprovalStatus Status { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public MethodologyPublishingStrategy PublishingStrategy { get; set; }

    public Guid? WithReleaseId { get; set; }

    public bool IsStatusUpdateRequired(MethodologyVersion methodologyVersion)
    {
        return methodologyVersion.Status != Status
               || methodologyVersion.PublishingStrategy != PublishingStrategy
               || methodologyVersion.ScheduledWithReleaseVersionId != WithReleaseId;
    }
}
