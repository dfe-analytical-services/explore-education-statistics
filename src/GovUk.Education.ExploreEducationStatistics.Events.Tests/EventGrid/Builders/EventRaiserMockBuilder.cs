using GovUk.Education.ExploreEducationStatistics.Events.EventGrid;

namespace GovUk.Education.ExploreEducationStatistics.Events.Tests.EventGrid.Builders;

public class EventRaiserMockBuilder
{
    private readonly MockEventRaiser _mock = new();
    public IEventRaiser Build() => _mock;
    public Asserter Assert => new(this);

    public class Asserter(EventRaiserMockBuilder parent)
    {
        public void EventRaised<TEventBuilder>(TEventBuilder expectedEvent) 
            where TEventBuilder : IEvent
            => Xunit.Assert.True(parent._mock.EventWasRaised(expectedEvent));

        public void EventsRaised<TEventBuilder>(IEnumerable<TEventBuilder> expectedEvents)
            where TEventBuilder : IEvent
            => Xunit.Assert.All(expectedEvents, e => Xunit.Assert.True(parent._mock.EventWasRaised(e)));

        public void NoEventRaised() 
            => Xunit.Assert.False(parent._mock.EventWasRaised());
    }

    private class MockEventRaiser : IEventRaiser
    {
        private readonly List<object> _events = new();

        private Task AddEvent<TEventBuilder>(TEventBuilder @event)
        {
            _events.Add(@event ?? throw new ArgumentNullException(nameof(@event)));
            return Task.CompletedTask;
        }
        private Task AddEvents<TEventBuilder>(IEnumerable<TEventBuilder> @events)
        {
            _events.AddRange(@events.Cast<object>());
            return Task.CompletedTask;
        }

        public bool EventWasRaised<TEventBuilder>(TEventBuilder expectedEvent) => 
            _events.OfType<TEventBuilder>().Any(actual => actual!.Equals(expectedEvent));

        public bool EventWasRaised() => _events.Any();

        public Task RaiseEvent<TEventBuilder>(TEventBuilder eventBuilder, CancellationToken cancellationToken = default) 
            where TEventBuilder : IEvent => 
            AddEvent(eventBuilder);

        public Task RaiseEvents<TEventBuilder>(IEnumerable<TEventBuilder> eventBuilders, CancellationToken cancellationToken = default) 
            where TEventBuilder : IEvent => 
            AddEvents(eventBuilders);
    }
}
