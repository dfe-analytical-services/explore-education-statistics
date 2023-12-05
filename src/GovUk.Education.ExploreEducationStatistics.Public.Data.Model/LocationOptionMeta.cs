using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class LocationOptionMeta
{
    [JsonPropertyName("Id")]
    public string Identifier { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;
}
