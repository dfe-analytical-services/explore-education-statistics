#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Strategies;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Strategies;

public class AnalyticsWritePublicZipDownloadStrategy(
    IAnalyticsPathResolver analyticsPathResolver,
    ICommonAnalyticsWriteStrategyWorkflow<CaptureZipDownloadRequest> workflow
) : IAnalyticsWriteStrategy
{
    private readonly IWorkflowActor<CaptureZipDownloadRequest> _workflowActor =
        new WorkflowActor(analyticsPath: analyticsPathResolver.PublicZipDownloadsDirectoryPath());

    public Type RequestType => typeof(CaptureZipDownloadRequest);

    public async Task Report(IAnalyticsCaptureRequestBase request, CancellationToken cancellationToken)
    {
        if (request is not CaptureZipDownloadRequest captureRequest)
        {
            throw new ArgumentException($"Request must be of type CaptureZipDownloadRequest. It is {request.GetType().FullName}", nameof(request));
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
