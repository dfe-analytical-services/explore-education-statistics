using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class IndicatorMeta
{
    [JsonPropertyName("Id")]
    public string Identifier { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;

    public byte DecimalPlaces { get; set; }
}
