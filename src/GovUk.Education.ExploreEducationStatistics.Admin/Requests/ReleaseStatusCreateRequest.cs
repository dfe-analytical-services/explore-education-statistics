#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record ReleaseStatusCreateRequest
{
    [JsonConverter(typeof(StringEnumConverter))]
    public ReleaseApprovalStatus ApprovalStatus { get; init; }

    public string? InternalReleaseNote { get; init; }

    public bool? NotifySubscribers { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public PublishMethod? PublishMethod { get; init; }

    public DateOnly? PublishScheduled { get; init; }

    [PartialDateValidator]
    public PartialDate? NextReleaseDate { get; init; }

    // TODO EES-6832 rename this to UpdatePublishedDisplayDate.
    public bool? UpdatePublishedDate { get; init; }
}
