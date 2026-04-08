using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.MockBuilders;

public class PublicationsTreeServiceMockBuilder
{
    private readonly Mock<IPublicationsTreeService> _mock = new(MockBehavior.Strict);

    private PublicationsTreeThemeDto[]? _publicationsTree;

    public PublicationsTreeServiceMockBuilder()
    {
        _mock
            .Setup(m =>
                m.GetPublicationsTreeFiltered(It.IsAny<PublicationsTreeFilter>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(() => _publicationsTree ?? []);
    }

    public IPublicationsTreeService Build() => _mock.Object;

    public PublicationsTreeServiceMockBuilder WhereHasPublicationsTree(PublicationsTreeThemeDto[] publicationsTree)
    {
        _publicationsTree = publicationsTree;
        return this;
    }

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IPublicationsTreeService> mock)
    {
        public void GetPublicationsTreeFilteredWasCalled(PublicationsTreeFilter? filter)
        {
            mock.Verify(
                m =>
                    m.GetPublicationsTreeFiltered(
                        It.Is<PublicationsTreeFilter>(actual => filter == null || actual == filter),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }
    }
}
