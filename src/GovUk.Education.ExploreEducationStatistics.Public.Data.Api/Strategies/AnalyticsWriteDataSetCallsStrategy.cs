using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies.Workflow;
using IAnalyticsPathResolver = GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces.IAnalyticsPathResolver;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies;

public class AnalyticsWriteDataSetCallsStrategy(
    IAnalyticsPathResolver analyticsPathResolver,
    ICommonAnalyticsWriteStrategyWorkflow<CaptureDataSetCallRequest> workflow
    ) : IAnalyticsWriteStrategy
{
    private readonly IWorkflowActor<CaptureDataSetCallRequest> _workflowActor =
        new WorkflowActor(analyticsPath: analyticsPathResolver.PublicApiDataSetCallsDirectoryPath());
        
    public Type RequestType => typeof(CaptureDataSetCallRequest);

    public async Task Report(IAnalyticsCaptureRequestBase request, CancellationToken cancellationToken)
    {
        await workflow.Report(_workflowActor, request, cancellationToken);
    }

    private class WorkflowActor(string analyticsPath)
        : WorkflowActorBase<CaptureDataSetCallRequest>(analyticsPath)
    {
        public override string GetFilenamePart(CaptureDataSetCallRequest request)
        {
            return request.DataSetId.ToString();
        }
    }
}
