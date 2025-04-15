using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.EventGrid;

public interface IEventRaiser
{
    Task RaiseEvent<TEventBuilder>(TEventBuilder eventBuilder, CancellationToken cancellationToken = default)
        where TEventBuilder : IEvent;
    
    Task RaiseEvents<TEventBuilder>(IEnumerable<TEventBuilder> eventBuilders, CancellationToken cancellationToken = default)
        where TEventBuilder : IEvent;
}
