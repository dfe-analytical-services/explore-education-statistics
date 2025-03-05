#nullable enable
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models;

public class DataScreenerTestResult
{
    [JsonPropertyName("check")]
    public required string TestFunctionName { get; set; }

    [JsonPropertyName("result")]
    public TestResult TestResult { get; set; }

    // TODO (EES-5353): The screener currently returns a single string, so this list will fail to deserialise until the response has been updated
    [JsonPropertyName("message")]
    public List<string> Notes { get; set; } = [];

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
