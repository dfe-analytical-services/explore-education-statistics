using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Strategies;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies;

public class AnalyticsWriteDataSetCallsStrategy(
    IAnalyticsPathResolver analyticsPathResolver,
    ICommonAnalyticsWriteStrategyWorkflow<CaptureDataSetCallRequest> workflow
) : IAnalyticsWriteStrategy
{
    public static readonly string[] OutputSubPaths = ["public-api", "data-sets"];

    private readonly IWorkflowActor<CaptureDataSetCallRequest> _workflowActor = new WorkflowActor(
        analyticsPath: analyticsPathResolver.BuildOutputDirectory(OutputSubPaths)
    );

    public Type RequestType => typeof(CaptureDataSetCallRequest);

    public async Task Report(IAnalyticsCaptureRequest request, CancellationToken cancellationToken)
    {
        if (request is not CaptureDataSetCallRequest captureRequest)
        {
            throw new ArgumentException(
                $"Request must be of type {nameof(CaptureDataSetCallRequest)}. It is {request.GetType().FullName}",
                nameof(request)
            );
        }
        await workflow.Report(_workflowActor, captureRequest, cancellationToken);
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
