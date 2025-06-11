#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Strategies;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Strategies;

public class AnalyticsWritePublicCsvDownloadStrategy(
    IAnalyticsPathResolver analyticsPathResolver,
    ICommonAnalyticsWriteStrategyWorkflow<CaptureCsvDownloadRequest> workflow
) : IAnalyticsWriteStrategy
{
    private readonly IWorkflowActor<CaptureCsvDownloadRequest> _workflowActor =
        new WorkflowActor(analyticsPath: analyticsPathResolver.PublicCsvDownloadsDirectoryPath());
        
    public Type RequestType => typeof(CaptureCsvDownloadRequest);

    public async Task Report(IAnalyticsCaptureRequestBase request, CancellationToken cancellationToken)
    {
        await workflow.Report(_workflowActor, request, cancellationToken);
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
