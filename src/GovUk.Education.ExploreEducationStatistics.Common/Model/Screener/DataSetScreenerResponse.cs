using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Screener;

// TODO EES-7003 - rename to "DataSetScreenerCompletionReport" in a follow-up PR.
public record DataSetScreenerResponse
{
    [JsonPropertyName("overall_stage")]
    public required string OverallResult { get; init; }

    [JsonPropertyName("passed")]
    public bool Passed { get; init; }

    // TODO EES-6693 - replace set with init when the need for a manual warning entry is no longer necessary.
    [JsonPropertyName("results_table")]
    public List<DataScreenerTestResult> TestResults { get; set; } = [];

    [JsonPropertyName("api_suitable")]
    public bool PublicApiCompatible { get; init; }
}
