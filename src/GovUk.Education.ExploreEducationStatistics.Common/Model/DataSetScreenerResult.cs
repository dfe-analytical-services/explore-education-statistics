#nullable enable
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model;

public class DataSetScreenerResult
{
    // TODO (EES-5353): Discuss if this can just be a boolean (e.g. "Passed", "IsSuccessful")
    [JsonPropertyName("overall_stage")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ScreenerResult Result { get; set; }

    [JsonPropertyName("overall_message")]
    public required string Message { get; set; }

    [JsonPropertyName("results_table")]
    public List<DataScreenerTestResult> TestResults { get; set; } = [];
}

public enum ScreenerResult
{
    Passed,
    Failed,
}
