#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.EventGrid;

/// <summary>
/// IEventRaiser responsibility is to request the client appropriately configured to the event being raised
/// from the IConfiguredEventGridClientFactory, and to invoke the client with the event.
/// </summary>
public class EventRaiser(IConfiguredEventGridClientFactory eventGridClientFactory) : IEventRaiser
{
    public async Task RaiseEvent<TEventBuilder>(TEventBuilder eventBuilder, CancellationToken cancellationToken = default)
        where TEventBuilder : IEvent
    {
        // Try to obtain an event grid client, configured using the configuration keyed on EventTopicOptionsKey 
        if (!eventGridClientFactory.TryCreateClient(
                TEventBuilder.EventTopicOptionsKey,
                out var client))
        {
            return;
        }
        
        await client.SendEventAsync(eventBuilder.ToEventGridEvent(), cancellationToken);
    }

    public async Task RaiseEvents<TEventBuilder>(IEnumerable<TEventBuilder> eventBuilders, CancellationToken cancellationToken = default)
        where TEventBuilder : IEvent
    {
        foreach (var eventBuilder in eventBuilders)
        {
            await RaiseEvent(eventBuilder, cancellationToken);
        }
    }
}
