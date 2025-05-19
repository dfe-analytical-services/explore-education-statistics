using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Services;

public class PublisherEventRaiserMockBuilder
{
    private readonly Mock<IPublisherEventRaiser> _mock = new(MockBehavior.Strict);
    public IPublisherEventRaiser Build() => _mock.Object;
    public Asserter Assert => new(_mock);

    public PublisherEventRaiserMockBuilder()
    {
        _mock
            .Setup(m => m.OnReleaseVersionsPublished(
                It.IsAny<IReadOnlyList<PublishedPublicationInfo>>()))
            .Returns(Task.CompletedTask);
    }

    public class Asserter(Mock<IPublisherEventRaiser> mock)
    {
        public void ReleaseVersionPublishedEventWasRaised(Func<PublishedPublicationInfo, bool> expected)
        {
            mock.Verify(m => m.OnReleaseVersionsPublished(
                It.Is<IReadOnlyList<PublishedPublicationInfo>>(
                    actual => actual.Any(expected))));
        }
    }
}
