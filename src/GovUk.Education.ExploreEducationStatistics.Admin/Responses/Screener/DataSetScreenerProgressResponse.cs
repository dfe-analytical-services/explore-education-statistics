#nullable enable
using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Responses.Screener;

public record DataSetScreenerProgressResponse
{
    [JsonPropertyName("data_set_id")]
    public required Guid DataSetId { get; init; }

    [JsonPropertyName("percentage_complete")]
    public required double PercentageComplete { get; init; }

    [JsonPropertyName("completed")]
    public bool Completed { get; init; }

    [JsonPropertyName("passed")]
    public bool Passed { get; init; }

    [JsonPropertyName("stage")]
    public string Stage { get; init; } = null!;
}
