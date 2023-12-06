using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class FilterMeta
{
    [JsonPropertyName("Id")]
    public required string Identifier { get; set; }

    public required string Label { get; set; }

    public string Hint { get; set; } = string.Empty;

    public List<FilterOptionMeta> Options { get; set; } = new();
}
