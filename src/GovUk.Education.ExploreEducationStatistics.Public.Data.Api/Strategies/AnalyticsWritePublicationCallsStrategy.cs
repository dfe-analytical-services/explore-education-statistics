using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Strategies;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using IAnalyticsPathResolver = GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces.IAnalyticsPathResolver;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies;

public class AnalyticsWritePublicationCallsStrategy(
    IAnalyticsPathResolver analyticsPathResolver,
    ICommonAnalyticsWriteStrategyWorkflow<CapturePublicationCallRequest> workflow
    ) : IAnalyticsWriteStrategy
{
    private readonly IWorkflowActor<CapturePublicationCallRequest> _workflowActor =
        new WorkflowActor(analyticsPath: analyticsPathResolver.PublicApiPublicationCallsDirectoryPath());
        
    public Type RequestType => typeof(CapturePublicationCallRequest);

    public async Task Report(IAnalyticsCaptureRequest request, CancellationToken cancellationToken)
    {
        if (request is not CapturePublicationCallRequest captureRequest)
        {
            throw new ArgumentException($"Request must be of type CapturePublicationCallRequest. It is {request.GetType().FullName}", nameof(request));
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
