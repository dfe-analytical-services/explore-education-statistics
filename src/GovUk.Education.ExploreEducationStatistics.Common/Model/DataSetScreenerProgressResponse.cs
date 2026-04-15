using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model;

public record DataSetScreenerProgressResponse
{
    [JsonPropertyName("data_set_id")]
    public required Guid DataSetId { get; init; }

    [JsonPropertyName("percentage_complete")]
    public required string PercentageComplete { get; init; }

    [JsonPropertyName("completed")]
    public bool Completed { get; init; }

    [JsonPropertyName("stage")]
    public string Stage { get; init; } = null!;
}
