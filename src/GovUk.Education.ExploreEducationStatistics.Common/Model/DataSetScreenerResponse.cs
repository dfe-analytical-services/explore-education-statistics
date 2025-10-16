#nullable enable
using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model;

public record DataSetScreenerResponse
{
    [JsonPropertyName("overall_stage")]
    public required string OverallResult { get; init; }

    [JsonPropertyName("passed")]
    public bool Passed { get; init; }

    [JsonPropertyName("results_table")]
    public List<DataScreenerTestResult> TestResults { get; init; } = [];
}
