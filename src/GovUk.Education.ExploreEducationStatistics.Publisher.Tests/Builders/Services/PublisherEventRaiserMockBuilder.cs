using GovUk.Education.ExploreEducationStatistics.Content.Model;
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
            .Setup(m => m.OnPublicationArchived(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        _mock
            .Setup(m => m.OnReleaseVersionsPublished(
                It.IsAny<IReadOnlyList<PublishedPublicationInfo>>()))
            .Returns(Task.CompletedTask);
    }

    public class Asserter(Mock<IPublisherEventRaiser> mock)
    {
        public void PublicationArchivedEventWasRaised(Publication publication)
        {
            mock.Verify(m => m.OnPublicationArchived(
                    publication.Id,
                    publication.Slug,
                    publication.SupersededById!.Value),
                Times.Once());
        }

        public void PublicationArchivedEventWasNotRaised()
        {
            mock.Verify(m => m.OnPublicationArchived(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<Guid>()),
                Times.Never());
        }

        public void ReleaseVersionPublishedEventWasRaised(Func<PublishedPublicationInfo, bool> expected)
        {
            mock.Verify(m => m.OnReleaseVersionsPublished(
                It.Is<IReadOnlyList<PublishedPublicationInfo>>(
                    actual => actual.Any(expected))));
        }
    }
}
