using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Strategies;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Strategies;

public class AnalyticsWritePublicZipDownloadStrategy(
    IAnalyticsPathResolver analyticsPathResolver,
    ICommonAnalyticsWriteStrategyWorkflow<CaptureZipDownloadRequest> workflow
) : IAnalyticsWriteStrategy
{
    public static readonly string[] OutputSubPaths = ["public", "zip-downloads"];

    private readonly IWorkflowActor<CaptureZipDownloadRequest> _workflowActor =
        new WorkflowActor(analyticsPath: analyticsPathResolver.BuildOutputDirectory(OutputSubPaths));

    public Type RequestType => typeof(CaptureZipDownloadRequest);

    public async Task Report(IAnalyticsCaptureRequest request, CancellationToken cancellationToken)
    {
        if (request is not CaptureZipDownloadRequest captureRequest)
        {
            throw new ArgumentException($"Request must be of type {nameof(CaptureZipDownloadRequest)}. It is {request.GetType().FullName}", nameof(request));
        }
        await workflow.Report(_workflowActor, captureRequest, cancellationToken);
    }

    private class WorkflowActor(string analyticsPath)
        : WorkflowActorBase<CaptureZipDownloadRequest>(analyticsPath)
    {
        public override string GetFilenamePart(CaptureZipDownloadRequest request)
        {
            return request.ReleaseVersionId.ToString();
        }
    }
}
