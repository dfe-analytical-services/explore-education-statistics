using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Strategies;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies;

public class AnalyticsWritePublicApiQueryStrategy(
    IAnalyticsPathResolver analyticsPathResolver,
    ICommonAnalyticsWriteStrategyWorkflow<CaptureDataSetVersionQueryRequest> workflow
    ) : IAnalyticsWriteStrategy
{
    public static readonly string[] OutputSubPaths = ["public-api", "queries"];
    
    private readonly IWorkflowActor<CaptureDataSetVersionQueryRequest> _workflowActor =
        new WorkflowActor(analyticsPath: analyticsPathResolver.BuildOutputDirectory(OutputSubPaths));
        
    public Type RequestType => typeof(CaptureDataSetVersionQueryRequest);

    public async Task Report(IAnalyticsCaptureRequest request, CancellationToken cancellationToken)
    {
        if (request is not CaptureDataSetVersionQueryRequest captureRequest)
        {
            throw new ArgumentException($"Request must be of type {nameof(CaptureDataSetVersionQueryRequest)}. It is {request.GetType().FullName}", nameof(request));
        }
        await workflow.Report(_workflowActor, captureRequest, cancellationToken);
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
