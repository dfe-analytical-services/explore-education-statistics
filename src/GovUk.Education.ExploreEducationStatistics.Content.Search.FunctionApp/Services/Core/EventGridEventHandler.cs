using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.Core;

public class EventGridEventHandler(ILogger<EventGridEventHandler> logger) : IEventGridEventHandler
{
    public async Task<TResponse> Handle<TPayload, TResponse>(
        FunctionContext context,
        EventGridEvent eventGridEvent,
        Func<TPayload, CancellationToken, Task<TResponse>> handler
    )
    {
        var payload = eventGridEvent.Data.ToObjectFromJson<TPayload>();

        logger.LogDebug(
            "{FunctionName} triggered: {@EventGridEvent} {@Payload}",
            context.FunctionDefinition.Name,
            eventGridEvent,
            payload
        );

        if (payload is null)
        {
            throw new Exception($"Unable to deserialise the payload of event into type {typeof(TPayload).Name}");
        }

        try
        {
            var response = await handler(payload, context.CancellationToken);
            logger.LogDebug("{FunctionName} completed. {@Response}", context.FunctionDefinition.Name, response);
            return response;
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "{FunctionName} errored processing {@EventGridEvent}.",
                context.FunctionDefinition.Name,
                eventGridEvent
            );
            throw;
        }
    }
}
