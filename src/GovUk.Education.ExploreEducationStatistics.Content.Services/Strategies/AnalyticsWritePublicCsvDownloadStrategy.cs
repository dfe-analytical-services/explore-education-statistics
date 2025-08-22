#nullable enable
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Strategies;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Strategies;

public class AnalyticsWritePublicCsvDownloadStrategy(
    IAnalyticsPathResolver analyticsPathResolver,
    ICommonAnalyticsWriteStrategyWorkflow<CaptureCsvDownloadRequest> workflow
) : IAnalyticsWriteStrategy
{
    public static readonly string[] OutputSubPaths = ["public", "csv-downloads"];
    
    private readonly IWorkflowActor<CaptureCsvDownloadRequest> _workflowActor =
        new WorkflowActor(analyticsPath: analyticsPathResolver.BuildOutputDirectory(OutputSubPaths));
        
    public Type RequestType => typeof(CaptureCsvDownloadRequest);

    public async Task Report(IAnalyticsCaptureRequest request, CancellationToken cancellationToken)
    {
        if (request is not CaptureCsvDownloadRequest captureRequest)
        {
            throw new ArgumentException($"Request must be of type {nameof(CaptureCsvDownloadRequest)}. It is {request.GetType().FullName}", nameof(request));
        }
        await workflow.Report(_workflowActor, captureRequest, cancellationToken);
    }

    private class WorkflowActor(string analyticsPath)
        : WorkflowActorBase<CaptureCsvDownloadRequest>(analyticsPath)
    {
        public override string GetFilenamePart(CaptureCsvDownloadRequest request)
        {
            return $"{request.ReleaseVersionId}_{request.SubjectId}";
        }
    }
}
