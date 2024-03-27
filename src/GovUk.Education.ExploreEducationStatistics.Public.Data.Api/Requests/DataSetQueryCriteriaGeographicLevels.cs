using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

/// <summary>
/// The geographic levels criteria to filter results by in a data set query.
/// </summary>
public record DataSetQueryCriteriaGeographicLevels
{
    /// <summary>
    /// Filter the results to be in this geographic level.
    /// </summary>
    public string? Eq { get; init; }

    /// <summary>
    /// Filter the results to not be in this geographic level.
    /// </summary>
    public string? NotEq { get; init; }

    /// <summary>
    /// Filter the results to be in one of these geographic levels.
    /// </summary>
    public IReadOnlyList<string>? In { get; init; }

    /// <summary>
    /// Filter the results to not be in one of these geographic levels.
    /// </summary>
    public IReadOnlyList<string>? NotIn { get; init; }

    [JsonIgnore]
    public GeographicLevel? ParsedEq
        => Eq is not null ? EnumUtil.GetFromEnumValue<GeographicLevel>(Eq) : null;

    [JsonIgnore]
    public GeographicLevel? ParsedNotEq
        => NotEq is not null ? EnumUtil.GetFromEnumValue<GeographicLevel>(NotEq) : null;

    [JsonIgnore]
    public IReadOnlyList<GeographicLevel>? ParsedIn
        => In?.Select(EnumUtil.GetFromEnumValue<GeographicLevel>).ToList();

    [JsonIgnore]
    public IReadOnlyList<GeographicLevel>? ParsedNotIn
        => NotIn?.Select(EnumUtil.GetFromEnumValue<GeographicLevel>).ToList();

    public HashSet<GeographicLevel> GetOptions()
    {
        List<GeographicLevel?> options =
        [
            ParsedEq,
            ParsedNotEq,
            ..ParsedIn ?? [],
            ..ParsedNotIn ?? []
        ];

        return options
            .OfType<GeographicLevel>()
            .ToHashSet();
    }
}
