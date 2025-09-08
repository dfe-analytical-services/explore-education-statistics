using GovUk.Education.ExploreEducationStatistics.Events.EventGrid;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Events.Tests.EventGrid.Builders;

public class ConfiguredEventGridClientFactoryMockBuilder
{
    private readonly Mock<IConfiguredEventGridClientFactory> _mock = new(MockBehavior.Strict);
    private bool _topicConfigFound = true;

    public IConfiguredEventGridClientFactory Build()
    {
        _mock
            .Setup(m => m.TryCreateClient(It.IsAny<string>(), out It.Ref<IEventGridClient?>.IsAny))
            .Returns(
                (string _, out IEventGridClient? client) =>
                {
                    if (_topicConfigFound)
                    {
                        client = Client.Build();
                        return true;
                    }

                    client = null;
                    return false;
                }
            );
        return _mock.Object;
    }

    public EventGridClientMockBuilder Client { get; } = new();

    public ConfiguredEventGridClientFactoryMockBuilder WhereNoTopicConfigFound()
    {
        _topicConfigFound = false;
        return this;
    }
}
