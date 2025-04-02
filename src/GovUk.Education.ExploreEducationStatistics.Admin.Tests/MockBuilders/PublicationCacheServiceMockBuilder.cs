using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class PublicationCacheServiceMockBuilder
{
    private readonly Mock<IPublicationCacheService> _mock = new(MockBehavior.Strict);
    public IPublicationCacheService Build()
    {
        _mock
            .Setup(m => m.UpdatePublication(It.IsAny<string>()))
            .ReturnsAsync((string publicationSlug) => new PublicationCacheViewModel());
        
        return _mock.Object;
    }
}