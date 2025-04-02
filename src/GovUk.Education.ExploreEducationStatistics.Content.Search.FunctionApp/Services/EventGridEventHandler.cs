using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;

public class EventGridEventHandler(ILogger<EventGridEventHandler> logger) : IEventGridEventHandler
{
    public async Task<TResponse> Handle<TPayload, TResponse>(
        FunctionContext context,
        EventGridEvent eventGridEvent,
        Func<TPayload, CancellationToken, Task<TResponse>> handler)
    {
        logger.LogInformation("{FunctionName} triggered: {Request}", context.FunctionDefinition.Name, eventGridEvent);
        
        var payload = eventGridEvent.Data.ToObjectFromJson<TPayload>() 
                      ?? throw new Exception(
                          $"Unable to deserialise the payload of event into type {typeof(TPayload).Name}");

        try
        {
            var response = await handler(payload, context.CancellationToken);
            logger.LogInformation("{FunctionName} completed. {Response}", context.FunctionDefinition.Name, response);
            return response;
        }
        catch (Exception e)
        {
            logger.LogError(e, "{FunctionName} errored processing {Request}.", context.FunctionDefinition.Name, eventGridEvent);
            throw;
        }
    }
}
