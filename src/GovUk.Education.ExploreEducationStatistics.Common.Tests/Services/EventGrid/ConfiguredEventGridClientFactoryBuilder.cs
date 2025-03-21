#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.EventGrid;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Services.EventGrid;

public class ConfiguredEventGridClientFactoryBuilder
{
    private readonly Mock<IConfiguredEventGridClientFactory> _mock = new(MockBehavior.Strict);
    private bool _topicConfigFound = true;

    public IConfiguredEventGridClientFactory Build()
    {
        _mock
            .Setup(m => m.TryCreateClient(It.IsAny<string>(), out It.Ref<IEventGridClient?>.IsAny))
            .Returns((string _, out IEventGridClient? client) =>
            {
                if (_topicConfigFound)
                {
                    client = Client.Build();
                    return true;
                }

                client = null;
                return false;
            });
        return _mock.Object;
    }

    public EventGridClientBuilder Client { get; } = new();

    public ConfiguredEventGridClientFactoryBuilder WhereNoTopicConfigFound()
    {
        _topicConfigFound = false;
        return this;
    }
}
