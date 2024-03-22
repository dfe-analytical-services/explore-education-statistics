using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

/// <summary>
/// A time period to filter results by.
///
/// - `period` is the time period or range (e.g. `2020` or `2020/2021`)
/// - `code` is the code identifying the time period type (e.g. `AY`, `CY`, `M1`, `W20`)
///
/// The `period` should be a single year like `2020`, or a range like `2020/2021`.
/// Currently, only years (or year ranges) are supported.
///
/// Some time period types span two years e.g. financial year part 2 (`P2`), or may fall in a
/// latter year e.g. academic year summer term (`T3`). For these types, a singular year `period`
/// like `2020` is considered as `2020/2021`.
///
/// For example, a `period` value of `2020` is applicable to the following time periods:
///
/// - 2020 calendar year
/// - 2020/2021 academic year
/// - 2020/2021 financial year part 2 (October to March)
/// - 2020/2021 academic year's summer term
///
/// If you wish to be more explicit, you may use a range for the `period` e.g. `2020/2021`.
/// However, a range cannot be used with time period types which only span a single year,
/// for example, `2020/21` cannot be used with `CY`, `M` or `W` codes.
///
/// Some examples:
///
/// - `2020|AY` is the 2020/21 academic year
/// - `2021|FY` is the 2021/22 financial year
/// - `2020|T3` is the 2020/21 academic year's summer term
/// - `2020|P2` is the 2020/21 financial year part 2 (October to March)
/// - `2020|CY` is the 2020 calendar year
/// - `2020|W32` is 2020 week 32
/// - `2020/2021|AY` is the 2020/21 academic year
/// - `2021/2022|FY` is the 2021/22 financial year
/// </summary>
public record DataSetQueryTimePeriod
{
    /// <summary>
    /// The time period to match results by.
    ///
    /// This should be a single year like `2020` or a range like `2020/2021`.
    /// </summary>
    public required string Period { get; init; }

    /// <summary>
    /// The code identifying the time period type to match results by.
    ///
    /// This should be a valid time period code e.g. `AY`, `CY`, `M1`, `W20`.
    /// </summary>
    public required string Code { get; init; }

    [JsonIgnore]
    public TimeIdentifier ParsedCode => EnumUtil.GetFromEnumValue<TimeIdentifier>(Code);

    public static DataSetQueryTimePeriod FromString(string timePeriod)
    {
        var parts = timePeriod.Split('|');
        var period = parts[0];
        var code = parts[1];

        return new DataSetQueryTimePeriod
        {
            Period = period,
            Code = code
        };
    }
}
