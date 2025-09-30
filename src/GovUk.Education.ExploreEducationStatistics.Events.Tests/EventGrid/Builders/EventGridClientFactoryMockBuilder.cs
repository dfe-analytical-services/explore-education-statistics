using GovUk.Education.ExploreEducationStatistics.Events.EventGrid;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Events.Tests.EventGrid.Builders;

public class EventGridClientFactoryMockBuilder
{
    private readonly Mock<IEventGridClientFactory> _mock = new(MockBehavior.Strict);
    private bool _creatingClientFails = false;

    public IEventGridClientFactory Build()
    {
        if (_creatingClientFails)
        {
            _mock
                .Setup(m => m.CreateClient(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("TEST EXCEPTION - Client could not be created."));
        }
        else
        {
            _mock
                .Setup(m => m.CreateClient(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Client.Build);
        }
        return _mock.Object;
    }

    public EventGridClientMockBuilder Client { get; } = new();

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IEventGridClientFactory> mock)
    {
        public void ClientRequested(
            Func<string, bool>? whereTopicEndpoint = null,
            Func<string, bool>? whereAccessKey = null
        )
        {
            mock.Verify(
                m =>
                    m.CreateClient(
                        It.Is<string>(actualEndpoint =>
                            whereTopicEndpoint == null || whereTopicEndpoint(actualEndpoint)
                        ),
                        It.Is<string>(actualAccessKey =>
                            whereAccessKey == null || whereAccessKey(actualAccessKey)
                        )
                    ),
                Times.Once
            );
        }
    }

    public EventGridClientFactoryMockBuilder WhereCreatingClientFails()
    {
        _creatingClientFails = true;
        return this;
    }
}
