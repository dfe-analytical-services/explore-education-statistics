using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class IndicatorMeta
{
    [JsonPropertyName("Id")]
    public required string Identifier { get; set; }

    public required string Label { get; set; }

    public IndicatorUnit? Unit { get; set; }

    public byte? DecimalPlaces { get; set; }
}
