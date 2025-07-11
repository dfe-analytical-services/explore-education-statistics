using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.Extensions.Logging;
using Formatting = Newtonsoft.Json.Formatting;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Common.Strategies;

public interface ICommonAnalyticsWriteStrategyWorkflow<TAnalyticsRequest>
    where TAnalyticsRequest : IAnalyticsCaptureRequest
{
    Task Report(
        IWorkflowActor<TAnalyticsRequest> workflowActor,
        TAnalyticsRequest request,
        CancellationToken cancellationToken);
}

public class CommonAnalyticsWriteStrategyWorkflow<TAnalyticsRequest>(
    DateTimeProvider dateTimeProvider,
    ILogger<CommonAnalyticsWriteStrategyWorkflow<TAnalyticsRequest>> logger)
    : ICommonAnalyticsWriteStrategyWorkflow<TAnalyticsRequest>
    where TAnalyticsRequest : IAnalyticsCaptureRequest
{
    public async Task Report(
        IWorkflowActor<TAnalyticsRequest> workflowActor,
        TAnalyticsRequest analyticsRequest,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Capturing request of type {RequestType} for analytics", typeof(TAnalyticsRequest));

        var filename =
            $"{dateTimeProvider.UtcNow:yyyyMMdd-HHmmss}_" +
            $"{workflowActor.GetFilenamePart(analyticsRequest)}_" +
            $"{RandomUtils.RandomString()}.json";

        try
        {
            Directory.CreateDirectory(workflowActor.GetAnalyticsPath());

            var filePath = Path.Combine(workflowActor.GetAnalyticsPath(), filename);

            var requestToSerialise = workflowActor.PrepareForSerialisation(analyticsRequest);

            var serialisedRequest = AnalyticsRequestSerialiser.SerialiseRequest(requestToSerialise);

            await File.WriteAllTextAsync(
                path: filePath,
                contents: serialisedRequest,
                cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error whilst writing {RequestType} to disk", typeof(TAnalyticsRequest));
        }
    }
}

public interface IWorkflowActor<TAnalyticsRequest> 
    where TAnalyticsRequest : IAnalyticsCaptureRequest
{
    string GetAnalyticsPath();
    
    string GetFilenamePart(TAnalyticsRequest request);
    
    TAnalyticsRequest PrepareForSerialisation(TAnalyticsRequest originalRequest);
}

public abstract class WorkflowActorBase<TAnalyticsRequest>(string analyticsPath) 
    : IWorkflowActor<TAnalyticsRequest> 
    where TAnalyticsRequest : IAnalyticsCaptureRequest
{
    public string GetAnalyticsPath()
    {
        return analyticsPath;
    }

    public virtual TAnalyticsRequest PrepareForSerialisation(TAnalyticsRequest originalRequest)
    {
        return originalRequest;
    }

    public abstract string GetFilenamePart(TAnalyticsRequest request);
}
