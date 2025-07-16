using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Strategies;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using IAnalyticsPathResolver =
    GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces.IAnalyticsPathResolver;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies;

public class AnalyticsWriteTopLevelCallsStrategy(
    IAnalyticsPathResolver analyticsPathResolver,
    ICommonAnalyticsWriteStrategyWorkflow<CaptureTopLevelCallRequest> workflow
) : IAnalyticsWriteStrategy
{
    public static readonly string[] OutputSubPaths = ["public-api", "top-level"];
    
    private readonly IWorkflowActor<CaptureTopLevelCallRequest> _workflowActor =
        new WorkflowActor(analyticsPath: analyticsPathResolver.BuildOutputDirectory(OutputSubPaths));

    public Type RequestType => typeof(CaptureTopLevelCallRequest);

    public async Task Report(IAnalyticsCaptureRequest request, CancellationToken cancellationToken)
    {
        if (request is not CaptureTopLevelCallRequest captureRequest)
        {
            throw new ArgumentException($"Request must be of type {nameof(CaptureTopLevelCallRequest)}. It is {request.GetType().FullName}", nameof(request));
        }
        await workflow.Report(_workflowActor, captureRequest, cancellationToken);
    }

    private class WorkflowActor(string analyticsPath)
        : WorkflowActorBase<CaptureTopLevelCallRequest>(analyticsPath)
    {
        public override string GetFilenamePart(CaptureTopLevelCallRequest request)
        {
            return "top-level";
        }
    }
}
