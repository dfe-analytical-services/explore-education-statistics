#nullable enable
using System;
using System.Globalization;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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

    [DateTimeFormatValidator("yyyy-MM-dd")]
    public string? PublishScheduled { get; init; }

    public DateTime? PublishScheduledDate
    {
        get
        {
            if (PublishScheduled.IsNullOrEmpty())
            {
                return null;
            }

            DateTime.TryParseExact(
                PublishScheduled,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var dateTime
            );
            return dateTime.AsStartOfDayUtcForTimeZone();
        }
    }

    [PartialDateValidator] public PartialDate? NextReleaseDate { get; init; }

    public bool? UpdatePublishedDate { get; init; }
}
