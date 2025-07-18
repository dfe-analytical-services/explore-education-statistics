#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Strategies;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests.Builders;

public class CommonAnalyticsWriteStrategyWorkflowMockBuilder<TAnalyticsCaptureRequest> where TAnalyticsCaptureRequest : IAnalyticsCaptureRequest
{
    private readonly Mock<ICommonAnalyticsWriteStrategyWorkflow<TAnalyticsCaptureRequest>> _mock = new(MockBehavior.Strict);
    
    public ICommonAnalyticsWriteStrategyWorkflow<TAnalyticsCaptureRequest> Build()
    {
        _mock
            .Setup(m => m.Report(It.IsAny<IWorkflowActor<TAnalyticsCaptureRequest>>(), It.IsAny<TAnalyticsCaptureRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        return _mock.Object;
    }

    public Asserter Assert => new(_mock);
    
    public class Asserter(Mock<ICommonAnalyticsWriteStrategyWorkflow<TAnalyticsCaptureRequest>> mock)
    {
        public void ReportCalled(
            Func<TAnalyticsCaptureRequest, bool>? requestPredicate = null
        )
        {
            mock.Verify(m => m.Report(
                It.IsAny<IWorkflowActor<TAnalyticsCaptureRequest>>(), 
                It.Is<TAnalyticsCaptureRequest>(actual => requestPredicate == null || requestPredicate(actual)), 
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
