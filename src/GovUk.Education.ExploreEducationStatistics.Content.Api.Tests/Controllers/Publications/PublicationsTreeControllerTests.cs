using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Publications;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers.Publications;

public abstract class PublicationsTreeControllerTests
{
    private readonly PublicationsTreeServiceMockBuilder _publicationsTreeService = new();

    public class GetPublicationsTreeTests : PublicationsTreeControllerTests
    {
        [Fact]
        public async Task WhenServiceReturnsPublicationsTree_ReturnsOk()
        {
            // Arrange
            PublicationsTreeThemeDto[] publicationsTree =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    Summary = "Summary",
                    Title = "Title",
                    Publications =
                    [
                        new PublicationsTreePublicationDto
                        {
                            Id = Guid.NewGuid(),
                            Slug = "Slug",
                            Title = "Title",
                            AnyLiveReleaseHasData = true,
                            LatestReleaseHasData = true,
                            SupersededBy = new PublicationsTreePublicationSupersededByPublicationDto
                            {
                                Id = Guid.NewGuid(),
                                Slug = "Slug",
                                Title = "Title",
                            },
                        },
                    ],
                },
            ];
            _publicationsTreeService.WhereHasPublicationsTree(publicationsTree);
            const PublicationsTreeFilter filter = PublicationsTreeFilter.DataTables;

            var sut = BuildController();

            // Act
            var result = await sut.GetPublicationsTree(filter);

            // Assert
            _publicationsTreeService.Assert.GetPublicationsTreeFilteredWasCalled(filter);
            result.AssertOkResult(publicationsTree);
        }
    }

    private PublicationsTreeController BuildController() => new(_publicationsTreeService.Build());
}
