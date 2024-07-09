using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using System;
using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.ViewModels;

public record ApiSubscriptionViewModel
{
    public Guid DataSetId { get; init; }

    public string DataSetTitle { get; init; } = null!;

    public string Email { get; init; } = null!;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ApiSubscriptionStatus Status { get; init; }
}
