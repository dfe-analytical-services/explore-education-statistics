using GovUk.Education.ExploreEducationStatistics.Common.Services.EventGrid;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Services.EventGrid;

public class EventGridPublisherClientFactoryBuilder
{
    private readonly Mock<IEventGridPublisherClientFactory> _mock = new(MockBehavior.Strict);
    public IEventGridPublisherClientFactory Build()
    {
        _mock
            .Setup(m => m.CreateClient(It.IsAny<string>(), It.IsAny<string?>()))
            .Returns(Client.Build());
        return _mock.Object;
    }

    public EventGridPublisherClientBuilder Client { get; } = new();
}
