using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Services;

public class NotificationsServiceMockBuilder
{
    private readonly Mock<INotificationsService> _mock = new(MockBehavior.Strict);
    public INotificationsService Build() => _mock.Object;
    public Asserter Assert => new(_mock);

    public NotificationsServiceMockBuilder()
    {
        _mock
            .Setup(m => m.NotifySubscribersIfApplicable(It.IsAny<IReadOnlyList<Guid>>()))
            .Returns(Task.CompletedTask);
    }

    public class Asserter(Mock<INotificationsService> mock)
    {
        public void NotifySubscribersIfApplicableCalled(params Guid[] expectedReleaseVersionIds)
        {
            mock.Verify(
                m => m.NotifySubscribersIfApplicable(
                    It.Is<IReadOnlyList<Guid>>(
                        actual => expectedReleaseVersionIds.All(actual.Contains) &&
                                  actual.All(expectedReleaseVersionIds.Contains))),
                Times.Once);
        }
    }
}
