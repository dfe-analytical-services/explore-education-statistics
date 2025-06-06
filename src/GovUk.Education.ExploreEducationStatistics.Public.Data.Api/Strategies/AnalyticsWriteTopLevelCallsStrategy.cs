using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using IAnalyticsPathResolver =
    GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces.IAnalyticsPathResolver;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies;

public class AnalyticsWriteTopLevelCallsStrategy(
    IAnalyticsPathResolver analyticsPathResolver,
    ICommonAnalyticsWriteStrategyWorkflow<CaptureTopLevelCallRequest> workflow
) : IAnalyticsWriteStrategy
{
    private readonly IWorkflowActor<CaptureTopLevelCallRequest> _workflowActor =
        new WorkflowActor(analyticsPath: analyticsPathResolver.PublicApiTopLevelCallsDirectoryPath());

    public Type RequestType => typeof(CaptureTopLevelCallRequest);

    public async Task Report(IAnalyticsCaptureRequestBase request, CancellationToken cancellationToken)
    {
        await workflow.Report(_workflowActor, request, cancellationToken);
    }

    private class WorkflowActor(string analyticsPath)
        : WorkflowActorBase<CaptureTopLevelCallRequest>(analyticsPath)
    {
        public override string GetFilenamePart(CaptureTopLevelCallRequest request)
        {
            return "top-level";
        }
    }
}
