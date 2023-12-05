using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class FilterOptionMeta
{
    [JsonPropertyName("Id")]
    public string Identifier { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public bool? IsAggregate { get; set; } = null!;

    public FilterMeta FilterMeta { get; set; } = null!;
}
