#nullable enable
using System;
using System.Collections.Generic;
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
            .Callback(
                (IWorkflowActor<TAnalyticsCaptureRequest> workflowActor,
                TAnalyticsCaptureRequest _,
                CancellationToken _) 
                    => _reportCalls.Add(new ReportCallArguments(workflowActor)))
            .Returns(Task.CompletedTask);
        
        return _mock.Object;
    }

    private readonly List<ReportCallArguments> _reportCalls = new();
    private record ReportCallArguments(IWorkflowActor<TAnalyticsCaptureRequest> WorkflowActor);

    public Asserter Assert => new(this);
    
    public class Asserter(CommonAnalyticsWriteStrategyWorkflowMockBuilder<TAnalyticsCaptureRequest> parent)
    {
        public void ReportCalled(
            Func<TAnalyticsCaptureRequest, bool>? requestPredicate = null
        )
        {
            parent._mock.Verify(m => m.Report(
                It.IsAny<IWorkflowActor<TAnalyticsCaptureRequest>>(), 
                It.Is<TAnalyticsCaptureRequest>(actual => requestPredicate == null || requestPredicate(actual)), 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Workflow actors are created within their corresponding write strategies and passed into the workflow.
        /// Use this method to make an assertion against the WorkflowActor.
        /// <example>
        /// _commonAnalyticsWriteStrategyWorkflowMockBuilder.Assert.WorkflowActor(workflowActor => Assert.Equal("c:\\temp\\output\\", workflowActor.GetAnalyticsPath()));
        /// </example>
        /// </summary>
        public void WorkflowActor(Action<IWorkflowActor<TAnalyticsCaptureRequest>> assertion) => assertion(Xunit.Assert.Single(parent._reportCalls).WorkflowActor);
    }
}
