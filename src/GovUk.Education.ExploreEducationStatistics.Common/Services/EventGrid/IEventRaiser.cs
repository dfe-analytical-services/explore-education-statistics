using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.EventGrid;

/// <summary>
/// IEventRaiser responsibility is to request the client appropriately configured to the event being raised
/// from the IConfiguredEventGridClientFactory, and to invoke the client with the event.
/// </summary>
public interface IEventRaiser
{
    Task RaiseEvent<TEventBuilder>(TEventBuilder eventBuilder, CancellationToken cancellationToken = default)
        where TEventBuilder : IEvent;
    
    Task RaiseEvents<TEventBuilder>(IEnumerable<TEventBuilder> eventBuilders, CancellationToken cancellationToken = default)
        where TEventBuilder : IEvent;
}
