using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Services;

public class PublicationCacheServiceMockBuilder
{
    private readonly Mock<IPublicationCacheService> _mock = new(MockBehavior.Strict);
    public Asserter Assert => new(_mock);

    public IPublicationCacheService Build() => _mock.Object;

    public PublicationCacheServiceMockBuilder()
    {
        _mock
            .Setup(m => m.UpdatePublication(It.IsAny<string>()))
            .ReturnsAsync(new Either<ActionResult, PublicationCacheViewModel>(new NoOpResult()));
    }

    public class Asserter(Mock<IPublicationCacheService> mock)
    {
        public void PublicationUpdated(string publicationSlug)
        {
            mock.Verify(m => m.UpdatePublication(publicationSlug), Times.Once);
        }

        public void PublicationNotUpdated(string publicationSlug)
        {
            mock.Verify(m => m.UpdatePublication(publicationSlug), Times.Never);
        }
    }
}
