#nullable enable
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model;

public record DataSetScreenerResponse
{
    // TODO (EES-5353): Discuss if this can just be a boolean (e.g. "Passed", "IsSuccessful")
    [JsonPropertyName("overall_stage")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ScreenerResult OverallResult { get; init; }

    [JsonPropertyName("overall_message")]
    public required string Message { get; init; }

    [JsonPropertyName("results_table")]
    public List<DataScreenerTestResult> TestResults { get; init; } = [];
}

public enum ScreenerResult
{
    Passed,
    Failed,
}
