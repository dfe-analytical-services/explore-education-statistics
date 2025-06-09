#nullable enable
using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model;

public class DataScreenerTestResult
{
    [JsonPropertyName("check")]
    public required string TestFunctionName { get; set; }

    [JsonPropertyName("result")]
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    public TestResult TestResult { get; set; }

    // TODO (EES-5353): The screener currently returns a single string, but the intention is for it to return a List
    [JsonPropertyName("message")]
    public string? Notes { get; set; }

    [JsonPropertyName("stage")]
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    public Stage Stage { get; set; }
}

// TODO (EES-5353): Rename stages to be more descriptive
public enum Stage
{
    InitialFileValidation = 1,
    PreScreening1 = 2,
    PreScreening2 = 3,
    FullChecks = 4,
    Passed = 5,
}

public enum TestResult
{
    PASS,
    FAIL,
    WARNING,
}
