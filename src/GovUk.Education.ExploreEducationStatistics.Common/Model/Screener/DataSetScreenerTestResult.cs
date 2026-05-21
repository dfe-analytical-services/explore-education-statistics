using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Screener;

public class DataScreenerTestResult
{
    public static readonly DataScreenerTestResult ExternalScreenerWarningResult = new()
    {
        Result = TestResult.WARNING,
        TestFunctionName = "External screening",
        Notes =
            "I confirm that this data set has been separately screened by, and passed, the EES data screener (https://rsconnect/rsc/dfe-published-data-qa/).",
        Stage = "Manual acknowledgement",
    };

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
