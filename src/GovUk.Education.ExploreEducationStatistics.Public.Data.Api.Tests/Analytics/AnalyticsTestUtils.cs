using System.Diagnostics;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using JsonSerializerOptions = System.Text.Json.JsonSerializerOptions;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Analytics;

public static class AnalyticsTheoryData
{
    public record PreviewTokenSummary(string Label, DateTimeOffset Created, DateTimeOffset Expiry);

    public static readonly TheoryData<PreviewTokenSummary> PreviewTokens =
    [
        null,
        new(
            Label: "Preview token",
            Created: DateTimeOffset.UtcNow.AddDays(-1),
            Expiry: DateTimeOffset.UtcNow.AddDays(1)
        ),
    ];

    public static readonly TheoryData<(
        PreviewTokenSummary?,
        string?
    )> PreviewTokensAndRequestedDataSetVersions =
    [
        (null, null),
        (
            new PreviewTokenSummary(
                Label: "Preview token",
                Created: DateTimeOffset.UtcNow.AddDays(-1),
                Expiry: DateTimeOffset.UtcNow.AddDays(1)
            ),
            "1.0.*"
        ),
    ];
}

public static class AnalyticsTestAssertions
{
    public static async Task AssertTopLevelAnalyticsCallCaptured(
        TopLevelCallType expectedType,
        string expectedAnalyticsPath,
        object? expectedParameters
    )
    {
        // Expect the successful call to have been recorded for analytics.
        await WaitForDirectoryToExist(expectedAnalyticsPath);
        await WaitForFilesToExistInDirectory(expectedAnalyticsPath);

        var analyticsFiles = Directory.GetFiles(expectedAnalyticsPath);
        var analyticFile = Assert.Single(analyticsFiles);
        var capturedCall = await ReadFile<CaptureTopLevelCallRequest>(analyticFile);

        Assert.NotNull(capturedCall);
        Assert.Equal(expectedType, capturedCall.Type);
        capturedCall.StartTime.AssertUtcNow();

        if (expectedParameters == null)
        {
            Assert.Null(capturedCall.Parameters);
        }
        else
        {
            // Expect any additional parameters to have been recorded.
            Assert.NotNull(capturedCall.Parameters);
            var parameters = JsonConvert.DeserializeObject(
                capturedCall.Parameters.ToString()!,
                expectedParameters.GetType()
            );
            Assert.NotNull(parameters);
            parameters.AssertDeepEqualTo(expectedParameters);
        }
    }

    public static async Task AssertPublicationAnalyticsCallCaptured(
        Guid publicationId,
        string publicationTitle,
        PublicationCallType expectedType,
        string expectedAnalyticsPath,
        object? expectedParameters
    )
    {
        // Expect the successful call to have been recorded for analytics.
        await WaitForDirectoryToExist(expectedAnalyticsPath);
        await WaitForFilesToExistInDirectory(expectedAnalyticsPath);

        var analyticsFiles = Directory.GetFiles(expectedAnalyticsPath);
        var analyticFile = Assert.Single(analyticsFiles);
        var capturedCall = await ReadFile<CapturePublicationCallRequest>(analyticFile);

        Assert.NotNull(capturedCall);
        Assert.Equal(expectedType, capturedCall.Type);
        Assert.Equal(publicationId, capturedCall.PublicationId);
        Assert.Equal(publicationTitle, capturedCall.PublicationTitle);
        capturedCall.StartTime.AssertUtcNow();

        if (expectedParameters == null)
        {
            Assert.Null(capturedCall.Parameters);
        }
        else
        {
            // Expect any additional parameters to have been recorded.
            Assert.NotNull(capturedCall.Parameters);
            var parameters = JsonConvert.DeserializeObject(
                capturedCall.Parameters.ToString()!,
                expectedParameters.GetType()
            );
            Assert.NotNull(parameters);
            parameters.AssertDeepEqualTo(expectedParameters);
        }
    }

    public static async Task AssertDataSetAnalyticsCallCaptured(
        DataSet dataSet,
        DataSetCallType expectedType,
        string expectedAnalyticsPath,
        object? expectedParameters,
        AnalyticsTheoryData.PreviewTokenSummary? expectedPreviewToken,
        Guid? expectedPreviewTokenDataSetVersionId
    )
    {
        // Expect the successful call to have been recorded for analytics.
        await WaitForDirectoryToExist(expectedAnalyticsPath);
        await WaitForFilesToExistInDirectory(expectedAnalyticsPath);

        var analyticsFiles = Directory.GetFiles(expectedAnalyticsPath);
        var analyticFile = Assert.Single(analyticsFiles);
        var capturedCall = await ReadFile<CaptureDataSetCallRequest>(analyticFile);

        Assert.NotNull(capturedCall);
        Assert.Equal(expectedType, capturedCall.Type);
        Assert.Equal(dataSet.Id, capturedCall.DataSetId);
        Assert.Equal(dataSet.Title, capturedCall.DataSetTitle);
        capturedCall.StartTime.AssertUtcNow();

        if (expectedParameters == null)
        {
            Assert.Null(capturedCall.Parameters);
        }
        else
        {
            // Expect any additional parameters to have been recorded.
            Assert.NotNull(capturedCall.Parameters);
            var parameters = JsonConvert.DeserializeObject(
                capturedCall.Parameters.ToString()!,
                expectedParameters.GetType()
            );
            Assert.NotNull(parameters);
            parameters.AssertDeepEqualTo(expectedParameters);
        }

        if (expectedPreviewToken == null)
        {
            Assert.Null(capturedCall.PreviewToken);
        }
        else
        {
            Assert.NotNull(capturedCall.PreviewToken);
            Assert.Equal(expectedPreviewToken.Label, capturedCall.PreviewToken.Label);
            Assert.Equal(
                expectedPreviewTokenDataSetVersionId,
                capturedCall.PreviewToken.DataSetVersionId
            );
            Assert.Equal(
                expectedPreviewToken.Created.TruncateMicroseconds(),
                capturedCall.PreviewToken.Created
            );
            Assert.Equal(
                expectedPreviewToken.Expiry.TruncateMicroseconds(),
                capturedCall.PreviewToken.Expiry
            );
        }
    }

    public static async Task AssertDataSetVersionAnalyticsCallCaptured(
        DataSet dataSet,
        DataSetVersion dataSetVersion,
        DataSetVersionCallType expectedType,
        string expectedAnalyticsPath,
        object? expectedParameters,
        string? expectedRequestedDataSetVersion,
        AnalyticsTheoryData.PreviewTokenSummary? expectedPreviewToken
    )
    {
        // Expect the successful call to have been recorded for analytics.
        await WaitForDirectoryToExist(expectedAnalyticsPath);
        await WaitForFilesToExistInDirectory(expectedAnalyticsPath);

        var analyticsFiles = Directory.GetFiles(expectedAnalyticsPath);
        var analyticFile = Assert.Single(analyticsFiles);
        var capturedCall = await ReadFile<CaptureDataSetVersionCallRequest>(analyticFile);

        Assert.NotNull(capturedCall);
        Assert.Equal(expectedType, capturedCall.Type);
        Assert.Equal(dataSet.Id, capturedCall.DataSetId);
        Assert.Equal(dataSet.Title, capturedCall.DataSetTitle);
        Assert.Equal(dataSetVersion.Id, capturedCall.DataSetVersionId);
        Assert.Equal(dataSetVersion.SemVersion().ToString(), capturedCall.DataSetVersion);
        Assert.Equal(expectedRequestedDataSetVersion, capturedCall.RequestedDataSetVersion);
        capturedCall.StartTime.AssertUtcNow();

        if (expectedParameters == null)
        {
            Assert.Null(capturedCall.Parameters);
        }
        else
        {
            // Expect any additional parameters to have been recorded.
            Assert.NotNull(capturedCall.Parameters);
            var parameters = JsonConvert.DeserializeObject(
                capturedCall.Parameters.ToString()!,
                expectedParameters.GetType()
            );
            Assert.NotNull(parameters);
            parameters.AssertDeepEqualTo(expectedParameters);
        }

        if (expectedPreviewToken == null)
        {
            Assert.Null(capturedCall.PreviewToken);
        }
        else
        {
            Assert.NotNull(capturedCall.PreviewToken);
            Assert.Equal(expectedPreviewToken.Label, capturedCall.PreviewToken.Label);
            Assert.Equal(dataSetVersion.Id, capturedCall.PreviewToken.DataSetVersionId);
            Assert.Equal(
                expectedPreviewToken.Created.TruncateMicroseconds(),
                capturedCall.PreviewToken.Created
            );
            Assert.Equal(
                expectedPreviewToken.Expiry.TruncateMicroseconds(),
                capturedCall.PreviewToken.Expiry
            );
        }
    }

    public static async Task AssertDataSetVersionQueryAnalyticsCaptured(
        DataSetVersion dataSetVersion,
        string expectedAnalyticsPath,
        DataSetQueryRequest expectedRequest,
        int expectedResultsCount,
        int expectedTotalRows
    )
    {
        // Add a slight delay as the writing of the query details for analytics is non-blocking
        // and could occur slightly after the query result is returned to the user.
        await WaitForDirectoryToExist(expectedAnalyticsPath);
        await WaitForFilesToExistInDirectory(expectedAnalyticsPath);

        var queryFiles = Directory.GetFiles(expectedAnalyticsPath);
        var queryFile = Assert.Single(queryFiles);
        var capturedQuery = await ReadFile<CaptureDataSetVersionQueryRequest>(
            queryFile,
            useSystemJson: false
        );

        Assert.NotNull(capturedQuery);

        capturedQuery.Query.AssertDeepEqualTo(expectedRequest);

        Assert.Equal(capturedQuery.DataSetId, dataSetVersion.DataSetId);
        Assert.Equal(capturedQuery.DataSetVersionId, dataSetVersion.Id);
        Assert.Equal(expectedResultsCount, capturedQuery.ResultsCount);
        Assert.Equal(expectedTotalRows, capturedQuery.TotalRowsCount);

        capturedQuery.StartTime.AssertUtcNow();
        capturedQuery.EndTime.AssertUtcNow();
        Assert.True(capturedQuery.EndTime > capturedQuery.StartTime);
    }

    public static void AssertAnalyticsCallNotCaptured(string expectedAnalyticsPath)
    {
        // Add a slight delay as the writing of the analytics capture is non-blocking
        // and could occur slightly after the Controller response is returned to the user.
        Thread.Sleep(2000);

        // Expect that nothing was recorded for analytics.
        Assert.False(Directory.Exists(expectedAnalyticsPath));
    }

    // Allow waiting for a slight delay, as the writing of the analytics capture is non-blocking
    // and could occur slightly after the Controller response is returned to the user.
    private static async Task WaitForDirectoryToExist(string expectedPath, int timeoutMillis = 5000)
    {
        await WaitForConditionToBeTrue(
            conditionTest: () => Directory.Exists(expectedPath),
            failureMessage: $"Directory {expectedPath} does not exist after {timeoutMillis} milliseconds",
            timeoutMillis: timeoutMillis
        );
    }

    // Allow waiting for a slight delay, as the writing of the analytics capture is non-blocking
    // and could occur slightly after the Controller response is returned to the user.
    private static async Task WaitForFilesToExistInDirectory(
        string expectedPath,
        int timeoutMillis = 5000
    )
    {
        await WaitForConditionToBeTrue(
            conditionTest: () => Directory.GetFiles(expectedPath).Length > 0,
            failureMessage: $"Directory {expectedPath} does not exist after {timeoutMillis} milliseconds",
            timeoutMillis: timeoutMillis
        );
    }

    // Allow waiting for a slight delay, as the writing of the analytics capture is non-blocking
    // and could occur slightly after the Controller response is returned to the user.
    private static async Task WaitForConditionToBeTrue(
        Func<Task<bool>> conditionTest,
        string failureMessage,
        int timeoutMillis = 2000
    )
    {
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.ElapsedMilliseconds <= timeoutMillis)
        {
            if (await conditionTest())
            {
                return;
            }

            await Task.Delay(100);
        }

        Assert.Fail(failureMessage);
    }

    private static async Task<TResult?> ReadFile<TResult>(
        string filePath,
        bool useSystemJson = true
    )
    {
        var result = default(TResult?);

        // Wait for the JSON file to be fully written before reading successfully.
        await WaitForConditionToBeTrue(
            async () =>
            {
                var contents = await File.ReadAllTextAsync(filePath);
                result = useSystemJson
                    ? JsonConvert.DeserializeObject<TResult>(contents)
                    : JsonSerializer.Deserialize<TResult>(
                        contents,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                return result != null;
            },
            "Failed to read analytics JSON file"
        );

        return result;
    }

    private static async Task WaitForConditionToBeTrue(
        Func<bool> conditionTest,
        string failureMessage,
        int timeoutMillis = 2000
    )
    {
        await WaitForConditionToBeTrue(
            () => Task.FromResult(conditionTest()),
            failureMessage,
            timeoutMillis
        );
    }
}
