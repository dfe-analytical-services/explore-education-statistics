using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

/// <summary>
/// A group of locations in a data set based on their geographic level.
/// </summary>
public record LocationGroupViewModel
{
    [JsonConverter(typeof(EnumToEnumValueJsonConverter<GeographicLevel>))]
    public required GeographicLevel Level { get; init; }

    /// <summary>
    /// The human-readable label of the geographic level.
    /// </summary>
    public string Label => Level.GetEnumLabel();

    public static LocationGroupViewModel Create(LocationMeta meta)
    {
        return new LocationGroupViewModel
        {
            Level = meta.Level,
        };
    }
}
