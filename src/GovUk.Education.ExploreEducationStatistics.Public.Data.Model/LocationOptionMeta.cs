using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class LocationOptionMeta
{
    [JsonPropertyName("Id")]
    public required string Identifier { get; set; }

    public required string Label { get; set; }

    public string Code { get; set; } = string.Empty;
}
