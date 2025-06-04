using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies.Workflow;
using IAnalyticsPathResolver = GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces.IAnalyticsPathResolver;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies;

public class AnalyticsWriteDataSetVersionCallsStrategy(
    IAnalyticsPathResolver analyticsPathResolver,
    ICommonAnalyticsWriteStrategyWorkflow<CaptureDataSetVersionCallRequest> workflow 
    ) : IAnalyticsWriteStrategy
{
    private readonly IWorkflowActor<CaptureDataSetVersionCallRequest> _workflowActor =
        new WorkflowActor(analyticsPath: analyticsPathResolver.PublicApiDataSetVersionCallsDirectoryPath());
        
    public Type RequestType => typeof(CaptureDataSetVersionCallRequest);

    public async Task Report(IAnalyticsCaptureRequestBase request, CancellationToken cancellationToken)
    {
        await workflow.Report(_workflowActor, request, cancellationToken);
    }

    private class WorkflowActor(string analyticsPath) 
        : WorkflowActorBase<CaptureDataSetVersionCallRequest>(analyticsPath)
    {
        public override string GetFilenamePart(CaptureDataSetVersionCallRequest request)
        {
            return request.DataSetVersionId.ToString();
        }
    }
}
