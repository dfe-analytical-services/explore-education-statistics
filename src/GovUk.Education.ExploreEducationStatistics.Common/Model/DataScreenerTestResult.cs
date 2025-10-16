#nullable enable
using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model;

public class DataScreenerTestResult
{
    [JsonPropertyName("check")]
    public required string TestFunctionName { get; set; }

    [JsonPropertyName("result")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TestResult Result { get; set; }

    [JsonPropertyName("message")]
    public string? Notes { get; set; }

    [JsonPropertyName("guidance_url")]
    public string? GuidanceUrl { get; set; }

    [JsonPropertyName("stage")]
    public required string Stage { get; set; }
}

public enum TestResult
{
    PASS,
    FAIL,
    WARNING,
}
