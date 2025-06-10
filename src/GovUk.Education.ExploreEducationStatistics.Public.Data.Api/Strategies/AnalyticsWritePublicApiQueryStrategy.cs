using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Strategies;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Utils;
using IAnalyticsPathResolver = GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces.IAnalyticsPathResolver;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies;

public class AnalyticsWritePublicApiQueryStrategy(
    IAnalyticsPathResolver analyticsPathResolver,
    ICommonAnalyticsWriteStrategyWorkflow<CaptureDataSetVersionQueryRequest> workflow
    ) : IAnalyticsWriteStrategy
{
    private readonly IWorkflowActor<CaptureDataSetVersionQueryRequest> _workflowActor =
        new WorkflowActor(analyticsPath: analyticsPathResolver.PublicApiQueriesDirectoryPath());
        
    public Type RequestType => typeof(CaptureDataSetVersionQueryRequest);

    public async Task Report(IAnalyticsCaptureRequestBase request, CancellationToken cancellationToken)
    {
        await workflow.Report(_workflowActor, request, cancellationToken);
    }

    private class WorkflowActor(string analyticsPath) 
        : WorkflowActorBase<CaptureDataSetVersionQueryRequest>(analyticsPath)
    {
        public override string GetFilenamePart(CaptureDataSetVersionQueryRequest request)
        {
            return request.DataSetVersionId.ToString();
        }

        public override CaptureDataSetVersionQueryRequest PrepareForSerialisation(
            CaptureDataSetVersionQueryRequest originalRequest)
        {
            return originalRequest with { Query = DataSetQueryNormalisationUtil.NormaliseQuery(originalRequest.Query) };
        }
    }
}
