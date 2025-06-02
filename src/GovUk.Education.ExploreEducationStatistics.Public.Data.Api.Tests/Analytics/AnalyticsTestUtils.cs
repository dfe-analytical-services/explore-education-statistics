using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Analytics;

public static class AnalyticsTheoryData
{
    public record PreviewTokenSummary(
        string Label,
        DateTimeOffset Created,
        DateTimeOffset Expiry);

    public static readonly TheoryData<PreviewTokenSummary>
        PreviewTokens =
        [
            null,
            new(Label: "Preview token",
                Created: DateTimeOffset.UtcNow.AddDays(-1),
                Expiry: DateTimeOffset.UtcNow.AddDays(1))
        ];

    public static readonly TheoryData<(PreviewTokenSummary?, string?)>
        PreviewTokensAndRequestedDataSetVersions =
        [
            (null, null),
            (new PreviewTokenSummary(Label: "Preview token",
                Created: DateTimeOffset.UtcNow.AddDays(-1),
                Expiry: DateTimeOffset.UtcNow.AddDays(1)), "1.0.*")
        ];
}

public static class AnalyticsTestAssertions
{
    public static async Task AssertDataSetVersionAnalyticsCallCaptured(
        DataSet dataSet,
        DataSetVersion dataSetVersion,
        DataSetVersionCallType expectedType,
        string expectedAnalyticsPath,
        object? expectedParameters,
        string? expectedRequestedDataSetVersion,
        AnalyticsTheoryData.PreviewTokenSummary? expectedPreviewToken)
    {
        // Add a slight delay as the writing of the analytics capture is non-blocking
        // and could occur slightly after the Controller response is returned to the user.
        Thread.Sleep(2000);

        // Expect the successful call to have been recorded for analytics.
        Assert.True(Directory.Exists(expectedAnalyticsPath));
        var analyticsFiles = Directory.GetFiles(expectedAnalyticsPath);
        var analyticFile = Assert.Single(analyticsFiles);
        var contents = await File.ReadAllTextAsync(analyticFile);

        var capturedCall = JsonConvert.DeserializeObject<CaptureDataSetVersionCallRequest>(contents);

        Assert.NotNull(capturedCall);
        Assert.Equal(expectedType, capturedCall.Type);
        Assert.Equal(dataSet.Id, capturedCall.DataSetId);
        Assert.Equal(dataSet.Title, capturedCall.DataSetTitle);
        Assert.Equal(dataSetVersion.Id, capturedCall.DataSetVersionId);
        Assert.Equal(dataSetVersion.SemVersion().ToString(), capturedCall.DataSetVersion);
        Assert.Equal(expectedRequestedDataSetVersion, capturedCall.RequestedDataSetVersion);
        capturedCall.StartTime.AssertUtcNow(withinMillis: 5000);

        if (expectedParameters == null)
        {
            Assert.Null(capturedCall.Parameters);
        }
        else
        {
            // Expect any additional parameters to have been recorded. 
            Assert.NotNull(capturedCall.Parameters);
            var parameters = JsonConvert.DeserializeObject(
                capturedCall.Parameters.ToString()!, expectedParameters.GetType());
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
            Assert.Equal(expectedPreviewToken.Created.TruncateMicroseconds(),
                capturedCall.PreviewToken.Created);
            Assert.Equal(expectedPreviewToken.Expiry.TruncateMicroseconds(), capturedCall.PreviewToken.Expiry);
        }
    }

    public static void AssertAnalyticsCallNotCaptured(string expectedAnalyticsPath)
    {
        // Add a slight delay as the writing of the analytics capture is non-blocking
        // and could occur slightly after the Controller response is returned to the user.
        Thread.Sleep(2000);

        // Expect that nothing was recorded for analytics.
        Assert.False(Directory.Exists(expectedAnalyticsPath));
    }
}
