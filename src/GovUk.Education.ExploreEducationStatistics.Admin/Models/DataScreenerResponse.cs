#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models;

public class DataScreenerResponse
{
    // TODO (EES-5353): The screening result will need to be persisted somewhere in the backend.
    // If a user attempts to confirm a data set for import, the result should be validated.
    // A failed screening should block the import process.
    [JsonIgnore]
    public Guid Id { get; set; }

    // TODO (EES-5353): Discuss if this can just be a boolean (e.g. "Passed", "IsSuccessful")
    [JsonPropertyName("overall_stage")]
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
