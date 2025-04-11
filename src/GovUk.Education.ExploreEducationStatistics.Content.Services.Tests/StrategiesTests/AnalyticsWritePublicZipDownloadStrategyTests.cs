using System;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Strategies;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.StrategiesTests;

public class AnalyticsWritePublicZipDownloadStrategyTests
{
    [Fact]
    public void CanHandle_True()
    {
        var strategy = BuildStrategy();
        var result = strategy.CanHandle(new CaptureZipDownloadRequest(
            "publication name",
            Guid.NewGuid(),
            "release name",
            "release label"));
        Assert.True(result);
    }

    private record CaptureSomethingElseRequest : BaseCaptureRequest;

    [Fact]
    public void CanHandle_OtherCaptureRequest_False()
    {
        var strategy = BuildStrategy();
        var result = strategy.CanHandle(new CaptureSomethingElseRequest());
        Assert.False(result);
    }

    // @MarkFix more tests here

    private AnalyticsWritePublicZipDownloadStrategy BuildStrategy(
        IAnalyticsPathResolver? pathResolver = null,
        ILogger<AnalyticsWritePublicZipDownloadStrategy>? logger = null)
    {
        return new AnalyticsWritePublicZipDownloadStrategy(
            pathResolver ?? new TestAnalyticsPathResolver(),
            logger ?? Mock.Of<ILogger<AnalyticsWritePublicZipDownloadStrategy>>());
    }
}
