using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

/// <summary>
/// Specifies how the results from a data set query should be sorted.
///
/// This consists of a field in the data set and a direction to sort by.
/// </summary>
public record DataSetQuerySort
{
    /// <summary>
    /// The name of the field to sort by. This can be one of the following:
    ///
    /// - `time_period` to sort by time period
    /// - `geographic_level` to sort by the geographic level
    /// - A location level code (e.g. `REG`, `LA`) to sort by the locations in that level
    /// - A filter ID (e.g. `characteristic`, `school_type`) to sort by the options in that filter
    /// - An indicator ID (e.g. `sess_authorised`, `enrolments`) to sort by the values in that indicator
    /// </summary>
    public required string Field { get; init; }

    /// <summary>
    /// The direction to sort by. This can be one of the following:
    ///
    /// - `Asc` - sort by ascending order
    /// - `Desc` - sort by descending order
    /// </summary>
    public required string Direction { get; init; }

    [JsonIgnore]
    public SortDirection ParsedDirection => EnumUtil.GetFromEnumValue<SortDirection>(Direction);

    public static DataSetQuerySort FromString(string sort)
    {
        var parts = sort.Split('|');

        return new DataSetQuerySort
        {
            Field = parts[0],
            Direction = parts[1]
        };
    }
}
