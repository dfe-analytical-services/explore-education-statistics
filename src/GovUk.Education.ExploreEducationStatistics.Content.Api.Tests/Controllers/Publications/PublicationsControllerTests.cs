using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Publications;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Publications;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.MockBuilders;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers.Publications;

public abstract class PublicationsControllerTests
{
    private readonly PublicationsServiceMockBuilder _publicationsService = new();

    private const string PublicationSlug = "test-publication";

    public class GetPublicationTests : PublicationsControllerTests
    {
        [Fact]
        public async Task WhenServiceReturnsPublication_ReturnsOk()
        {
            // Arrange
            var publication = new PublicationDtoBuilder().Build();
            _publicationsService.WhereHasPublication(publication);

            var sut = BuildController();

            // Act
            var result = await sut.GetPublication(PublicationSlug);

            // Assert
            _publicationsService.Assert.GetPublicationWasCalled(PublicationSlug);
            result.AssertOkResult(publication);
        }

        [Fact]
        public async Task WhenServiceReturnsNotFound_ReturnsNotFound()
        {
            // Arrange
            _publicationsService.WhereGetPublicationReturnsNotFound(PublicationSlug);

            var sut = BuildController();

            // Act
            var result = await sut.GetPublication(PublicationSlug);

            // Assert
            _publicationsService.Assert.GetPublicationWasCalled(PublicationSlug);
            result.AssertNotFoundResult();
        }
    }

    private PublicationsController BuildController() => new(_publicationsService.Build());
}
