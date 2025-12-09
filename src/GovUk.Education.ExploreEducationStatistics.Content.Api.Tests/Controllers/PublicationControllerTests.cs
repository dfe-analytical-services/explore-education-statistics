using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers;

public class PublicationControllerTests
{
    [Fact]
    public async Task GetPublicationTree()
    {
        var publicationCacheService = new Mock<IPublicationCacheService>(Strict);

        var controller = BuildPublicationController(publicationCacheService: publicationCacheService.Object);

        publicationCacheService
            .Setup(s => s.GetPublicationTree(PublicationTreeFilter.DataCatalogue))
            .ReturnsAsync(new List<PublicationTreeThemeViewModel> { new() { Publications = [new()] } });

        var result = await controller.GetPublicationTree(PublicationTreeFilter.DataCatalogue);

        VerifyAllMocks(publicationCacheService);

        var publicationTree = result.AssertOkResult();

        var theme = Assert.Single(publicationTree);

        Assert.Single(theme.Publications);
    }

    private static PublicationController BuildPublicationController(
        IPublicationCacheService? publicationCacheService = null,
        IPublicationService? publicationService = null
    )
    {
        return new PublicationController(
            publicationCacheService ?? Mock.Of<IPublicationCacheService>(Strict),
            publicationService ?? Mock.Of<IPublicationService>(Strict)
        );
    }
}
