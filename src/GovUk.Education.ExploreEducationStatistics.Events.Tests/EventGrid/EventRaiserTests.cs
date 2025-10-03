using Azure.Messaging.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Events.EventGrid;
using GovUk.Education.ExploreEducationStatistics.Events.Tests.EventGrid.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Events.Tests.EventGrid;

public class EventRaiserTests
{
    private readonly ConfiguredEventGridClientFactoryMockBuilder _eventGridClientFactoryMockBuilder = new();

    private IEventRaiser GetSut() => new EventRaiser(_eventGridClientFactoryMockBuilder.Build());

    private class TestEvent : IEvent
    {
        public string Subject => "subject";
        public string EventType => "event type";
        public string DataVersion => "version";
        public object Payload => "payload";
        public static string EventTopicOptionsKey => "TestEventTopicOptionsKey";

        public EventGridEvent ToEventGridEvent() => new(Subject, EventType, DataVersion, Payload);
    }

    [Fact]
    public void Can_instantiate_SUT() => Assert.NotNull(GetSut());

    [Fact]
    public async Task GivenNoTopicConfiguration_WhenEventRaised_ThenNothingHappens()
    {
        // ARRANGE
        _eventGridClientFactoryMockBuilder.WhereNoTopicConfigFound();
        var sut = GetSut();

        // ACT
        await sut.RaiseEvent(new TestEvent());

        // ASSERT
        _eventGridClientFactoryMockBuilder.Client.Assert.NoEventsWerePublished();
    }

    [Fact]
    public async Task GivenTopicConfigured_WhenTestEventRaised_ThenEventPublished()
    {
        // ARRANGE
        var sut = GetSut();

        // ACT
        var testEvent = new TestEvent();
        await sut.RaiseEvent(testEvent);

        // ASSERT
        var actualEvent = Assert.Single(_eventGridClientFactoryMockBuilder.Client.Assert.EventsPublished);

        var expectedEvent = testEvent;
        var expectedPayload = testEvent.Payload;

        Assert.Equal(expectedEvent.Subject, actualEvent.Subject);
        Assert.Equal(expectedEvent.EventType, actualEvent.EventType);
        Assert.Equal(expectedEvent.DataVersion, actualEvent.DataVersion);

        var actualPayload = actualEvent.Data.ToObjectFromJson<string>();
        Assert.Equal(expectedPayload, actualPayload);
    }
}
