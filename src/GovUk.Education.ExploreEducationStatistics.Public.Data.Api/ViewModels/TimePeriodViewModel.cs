using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

/// <summary>
/// A time period relating to some data.
/// </summary>
public record TimePeriodViewModel
{
    [JsonConverter(typeof(EnumToEnumValueJsonConverter<TimeIdentifier>))]
    public required TimeIdentifier Code { get; init; }

    /// <summary>
    /// The period covered by the data e.g. '2020' or '2020/2021'.
    /// </summary>
    public required string Period { get; init; }
}
