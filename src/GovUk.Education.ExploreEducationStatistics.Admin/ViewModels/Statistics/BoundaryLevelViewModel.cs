#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Statistics;

public record BoundaryLevelViewModel
{
    public required long Id { get; init; }

    [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
    public required GeographicLevel Level { get; init; }

    public required string Label { get; init; }

    public required DateTime Published { get; init; }
}
