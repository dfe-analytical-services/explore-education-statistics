using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class FilterMeta
{
    [JsonPropertyName("Id")]
    public string Identifier { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Hint { get; set; } = string.Empty;

    public List<FilterOptionMeta> Options { get; set; } = new();
}
