using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;

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

    public static TimePeriodOptionViewModel Create(TimePeriodMeta meta)
    {
        return new TimePeriodOptionViewModel
        {
            Code = meta.Code,
            Period = meta.Period,
            Label = TimePeriodFormatter.FormatLabel(meta.Period, meta.Code)
        };
    }
}
