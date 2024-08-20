using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

/// <summary>
/// A geographic level (e.g. national, regional) covered by a data set.
/// </summary>
public record GeographicLevelViewModel
{
    [JsonConverter(typeof(EnumToEnumValueJsonConverter<GeographicLevel>))]
    public required GeographicLevel Code { get; init; }

    /// <summary>
    /// The human-readable label for the geographic level.
    /// </summary>
    public string Label => Code.GetEnumLabel();

    public static GeographicLevelViewModel Create(GeographicLevel level)
    {
        return new GeographicLevelViewModel
        {
            Code = level,
        };
    }
}
