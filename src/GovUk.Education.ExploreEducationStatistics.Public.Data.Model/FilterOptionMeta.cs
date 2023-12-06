using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class FilterOptionMeta
{
    [JsonPropertyName("Id")]
    public required string Identifier { get; set; }

    public required string Label { get; set; }

    public bool? IsAggregate { get; set; } = null!;

    public FilterMeta FilterMeta { get; set; } = null!;
}
