using System;
using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.ViewModels;

public record ApiSubscriptionViewModel
{
    public required Guid DataSetId { get; init; }

    public required string DataSetTitle { get; init; }

    public required string Email { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required ApiSubscriptionStatus Status { get; init; }
}
