using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Strategies.Workflow;

public interface ICommonAnalyticsWriteStrategyWorkflow<TAnalyticsRequest>
    where TAnalyticsRequest : IAnalyticsCaptureRequestBase
{
    Task Report(
        IWorkflowActor<TAnalyticsRequest> workflowActor,
        IAnalyticsCaptureRequestBase request,
        CancellationToken cancellationToken);
}

public class CommonAnalyticsWriteStrategyWorkflow<TAnalyticsRequest>(
    DateTimeProvider dateTimeProvider,
    ILogger<CommonAnalyticsWriteStrategyWorkflow<TAnalyticsRequest>> logger)
    : ICommonAnalyticsWriteStrategyWorkflow<TAnalyticsRequest>
    where TAnalyticsRequest : IAnalyticsCaptureRequestBase
{
    public async Task Report(
        IWorkflowActor<TAnalyticsRequest> workflowActor,
        IAnalyticsCaptureRequestBase request,
        CancellationToken cancellationToken)
    {
        if (typeof(TAnalyticsRequest) != request.GetType())
        {
            throw new ArgumentException($"Request isn't of type {typeof(TAnalyticsRequest)}");
        }

        var analyticsRequest = (TAnalyticsRequest)request;

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

            var serialisedRequest = JsonSerializationUtils.Serialize(
                obj: requestToSerialise,
                formatting: Formatting.Indented,
                orderedProperties: true,
                camelCase: true);

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
    where TAnalyticsRequest : IAnalyticsCaptureRequestBase
{
    string GetAnalyticsPath();
    
    string GetFilenamePart(TAnalyticsRequest request);
    
    TAnalyticsRequest PrepareForSerialisation(TAnalyticsRequest originalRequest);
}

public abstract class WorkflowActorBase<TAnalyticsRequest>(string analyticsPath) 
    : IWorkflowActor<TAnalyticsRequest> 
    where TAnalyticsRequest : IAnalyticsCaptureRequestBase
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
