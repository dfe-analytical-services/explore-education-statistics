using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class IndicatorMeta
{
    [JsonPropertyName("Id")]
    public required string Identifier { get; set; }

    public required string Label { get; set; }

    // TODO: Change to Unit type
    public string Unit { get; set; } = string.Empty;

    public byte? DecimalPlaces { get; set; } = null;
}
