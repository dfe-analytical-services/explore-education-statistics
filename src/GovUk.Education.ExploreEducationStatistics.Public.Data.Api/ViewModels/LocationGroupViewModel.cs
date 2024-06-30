using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

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
    public required string Label { get; init; }
}
