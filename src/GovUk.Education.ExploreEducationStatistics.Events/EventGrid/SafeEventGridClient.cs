using Azure;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Events.EventGrid;

/// <summary>
/// This client will attempt to raise an event.
/// Errors will be logged but not thrown.
/// </summary>
public class SafeEventGridClient(ILogger logger, EventGridPublisherClient client)
    : IEventGridClient
{
    /// <summary>
    /// Publish event to event grid
    /// </summary>
    /// <param name="eventGridEvent">Event to publish</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>The Azure response or null if an error occurred</returns>
    public async Task<Response?> SendEventAsync(
        EventGridEvent eventGridEvent,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await client.SendEventAsync(eventGridEvent, cancellationToken);
            if (response.IsError)
            {
                logger.LogError(
                    "Error occurred whilst trying to raise event {@Event}. Response:{@SendEventResponse}",
                    eventGridEvent,
                    response);
            }
            logger.LogDebug("Event raised: {@Event}", eventGridEvent);
            return response;
        }
        catch (Exception exception)
        {
            logger.LogError(
                "Error occurred whilst trying to raise event {@Event}. Exception:{@Exception}",
                eventGridEvent,
                exception);
            return null;
        }
    }
}
