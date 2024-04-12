using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;

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
    /// - `timePeriod` to sort by time period
    /// - `geographicLevel` to sort by the geographic level of the data
    /// - `location|{level}` to sort by locations in a geographic level where `{level}` is the level code (e.g. `REG`, `LA`)
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

    public string ToSortString() => $"{Field}|{Direction}";

    public static DataSetQuerySort FromString(string sort)
    {
        var directionDelimiter = sort.LastIndexOf('|');

        return new DataSetQuerySort
        {
            Field = sort[..directionDelimiter],
            Direction = sort[(directionDelimiter + 1)..]
        };
    }
}
