using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Strategies;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies;

public class AnalyticsWriteDataSetVersionCallsStrategy(
    IAnalyticsPathResolver analyticsPathResolver,
    ICommonAnalyticsWriteStrategyWorkflow<CaptureDataSetVersionCallRequest> workflow
) : IAnalyticsWriteStrategy
{
    public static readonly string[] OutputSubPaths = ["public-api", "data-set-versions"];

    private readonly IWorkflowActor<CaptureDataSetVersionCallRequest> _workflowActor =
        new WorkflowActor(
            analyticsPath: analyticsPathResolver.BuildOutputDirectory(OutputSubPaths)
        );

    public Type RequestType => typeof(CaptureDataSetVersionCallRequest);

    public async Task Report(IAnalyticsCaptureRequest request, CancellationToken cancellationToken)
    {
        if (request is not CaptureDataSetVersionCallRequest captureRequest)
        {
            throw new ArgumentException(
                $"Request must be of type {nameof(CaptureDataSetVersionCallRequest)}. It is {request.GetType().FullName}",
                nameof(request)
            );
        }
        await workflow.Report(_workflowActor, captureRequest, cancellationToken);
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
