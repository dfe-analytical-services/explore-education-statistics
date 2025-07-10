using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Strategies;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Dtos;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Writers;

public class CapturePermaLinkTableDownloadCallAnalyticsWriteStrategy(
    IAnalyticsPathResolver analyticsPathResolver,
    ICommonAnalyticsWriteStrategyWorkflow<CapturePermaLinkTableDownloadCall> workflow) : IAnalyticsWriteStrategy
{
    private readonly IWorkflowActor<CapturePermaLinkTableDownloadCall> _workflowActor =
        new WorkflowActor(analyticsPath: analyticsPathResolver.GetPermaLinkTableDownloadCallsDirectoryPath());
    public Type RequestType => typeof(CapturePermaLinkTableDownloadCall);

    public async Task Report(IAnalyticsCaptureRequest request, CancellationToken cancellationToken)
    {
        if (request is not CapturePermaLinkTableDownloadCall captureRequest)
        {
            throw new ArgumentException($"Request must be of type {nameof(CapturePermaLinkTableDownloadCall)}. It is {request.GetType().FullName}", nameof(request));
        }
        await workflow.Report(_workflowActor, captureRequest, cancellationToken);   
    }
    
    private class WorkflowActor(string analyticsPath)
        : WorkflowActorBase<CapturePermaLinkTableDownloadCall>(analyticsPath)
    {
        public override string GetFilenamePart(CapturePermaLinkTableDownloadCall request) => 
            request.PermalinkId.ToString();
    }
}
