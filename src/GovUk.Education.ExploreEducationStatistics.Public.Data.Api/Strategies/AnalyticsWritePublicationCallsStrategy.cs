using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Strategies;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies;

public class AnalyticsWritePublicationCallsStrategy(
    IAnalyticsPathResolver analyticsPathResolver,
    ICommonAnalyticsWriteStrategyWorkflow<CapturePublicationCallRequest> workflow
    ) : IAnalyticsWriteStrategy
{
    public static readonly string[] OutputSubPaths = ["public-api", "publications"];

    private readonly IWorkflowActor<CapturePublicationCallRequest> _workflowActor =
        new WorkflowActor(analyticsPath: analyticsPathResolver.BuildOutputDirectory(OutputSubPaths));

    public Type RequestType => typeof(CapturePublicationCallRequest);

    public async Task Report(IAnalyticsCaptureRequest request, CancellationToken cancellationToken)
    {
        if (request is not CapturePublicationCallRequest captureRequest)
        {
            throw new ArgumentException($"Request must be of type {nameof(CapturePublicationCallRequest)}. It is {request.GetType().FullName}", nameof(request));
        }
        await workflow.Report(_workflowActor, captureRequest, cancellationToken);
    }

    private class WorkflowActor(string analyticsPath)
        : WorkflowActorBase<CapturePublicationCallRequest>(analyticsPath)
    {
        public override string GetFilenamePart(CapturePublicationCallRequest request)
        {
            return request.PublicationId.ToString();
        }
    }
}
