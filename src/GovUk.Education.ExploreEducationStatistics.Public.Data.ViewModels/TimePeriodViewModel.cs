using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.ViewModels;

/// <summary>
/// A time period relating to some data.
/// </summary>
public record TimePeriodViewModel
{
    /// <summary>
    /// The code identifying the time period's type.
    /// </summary>
    /// <example>AYQ1</example>>
    [JsonConverter(typeof(EnumToEnumValueJsonConverter<TimeIdentifier>))]
    public required TimeIdentifier Code { get; init; }

    /// <summary>
    /// The period covered by the data e.g. '2020' or '2020/2021'.
    /// </summary>
    /// <example>2020/2021</example>
    public required string Period { get; init; }
}
