using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

/// <summary>
/// A geographic level option (e.g. national, regional) that can be used to
/// filter a data set.
/// </summary>
public record GeographicLevelOptionViewModel
{
    [JsonConverter(typeof(EnumToEnumValueJsonConverter<GeographicLevel>))]
    public required GeographicLevel Level { get; init; }

    /// <summary>
    /// The human-readable label for the geographic level.
    /// </summary>
    public string Label => Level.GetEnumLabel();

    public static GeographicLevelOptionViewModel Create(GeographicLevel level)
    {
        return new GeographicLevelOptionViewModel
        {
            Level = level,
        };
    }
}
