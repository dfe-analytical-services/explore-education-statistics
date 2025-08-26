#nullable enable
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Dtos;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Writers;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Tests.Builders;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests.Analytics.Writers;

public class CapturePermaLinkTableDownloadCallAnalyticsWriteStrategyTests
{
    private readonly AnalyticsPathResolverMockBuilder _analyticsPathResolverMockBuilder = new();
    private readonly CommonAnalyticsWriteStrategyWorkflowMockBuilder<CapturePermaLinkTableDownloadCall> _commonAnalyticsWriteStrategyWorkflowMockBuilder = new();

    private CapturePermaLinkTableDownloadCallAnalyticsWriteStrategy GetSut() =>
        new(
            _analyticsPathResolverMockBuilder.Build(),
            _commonAnalyticsWriteStrategyWorkflowMockBuilder.Build()
        );

    [Fact]
    public void Can_instantiate_Sut() => Assert.NotNull(GetSut());
    
    [Fact]
    public void RequestType_is_correct()
    {
        var sut = GetSut();
        Assert.Equal(typeof(CapturePermaLinkTableDownloadCall), sut.RequestType);
    }

    [Fact]
    public async Task GivenADifferentTypeOfAnalyticsCaptureRequest_WhenRecordIsCalled_ThenShouldThrow()
    {
        // ARRANGE
        var differentTypeRequest = new TestAnalyticsCaptureRequest();
        var sut = GetSut();
        
        // ACT
        var exception = await Record.ExceptionAsync(() => sut.Report(differentTypeRequest));

        // ASSERT
        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
    }
    
    [Fact]
    public async Task GivenACaptureRequest_WhenRecordIsCalled_ThenWorkflowCalled()
    {
        // ARRANGE
        _analyticsPathResolverMockBuilder.WhereOutputDirectoryIs("c:\\temp\\output\\");
        var request = new CapturePermaLinkTableDownloadCallBuilder().Build();
        var sut = GetSut();
        
        // ACT
        await sut.Report(request);

        // ASSERT
        _analyticsPathResolverMockBuilder.Assert.BuildOutputDirectoryCalled(CapturePermaLinkTableDownloadCallAnalyticsWriteStrategy.OutputSubPaths);
        _commonAnalyticsWriteStrategyWorkflowMockBuilder.Assert.ReportCalled(actual => actual == request);
        
        _commonAnalyticsWriteStrategyWorkflowMockBuilder.Assert.WorkflowActor(
            workflowActor => Assert.Equal("c:\\temp\\output\\", workflowActor.GetAnalyticsPath()));
    }
    
    private record TestAnalyticsCaptureRequest : IAnalyticsCaptureRequest;
}
