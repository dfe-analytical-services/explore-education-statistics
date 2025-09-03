using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Releases;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers.Releases;

public abstract class ReleaseUpdatesControllerTests
{
    private readonly ReleaseUpdatesServiceMockBuilder _releaseUpdatesService = new();

    private const string PublicationSlug = "test-publication";
    private const string ReleaseSlug = "test-release";
    private const int Page = 5;
    private const int PageSize = 50;

    public class GetPaginatedUpdatesForReleaseTests : ReleaseUpdatesControllerTests
    {
        [Fact]
        public async Task GetPaginatedUpdatesForRelease_WhenServiceReturnsPaginatedUpdates_ReturnsOk()
        {
            // Arrange
            var paginatedUpdates = new PaginatedListViewModel<ReleaseUpdateDto>([
                    new ReleaseUpdateDtoBuilder().Build()
                ],
                totalResults: 1,
                page: 1,
                pageSize: 10);

            _releaseUpdatesService.WhereHasPaginatedUpdates(paginatedUpdates);

            var sut = BuildController();

            // Act
            var result = await sut.GetPaginatedUpdatesForRelease(
                publicationSlug: PublicationSlug,
                releaseSlug: ReleaseSlug,
                page: Page,
                pageSize: PageSize);

            // Assert
            _releaseUpdatesService.Assert.GetPaginatedUpdatesForReleaseWasCalled(
                publicationSlug: PublicationSlug,
                releaseSlug: ReleaseSlug,
                page: Page,
                pageSize: PageSize);
            result.AssertOkResult(paginatedUpdates);
        }

        [Fact]
        public async Task GetReleaseUpdates_WhenNoQueryParameters_UsesPaginationDefaults()
        {
            // Arrange
            const int defaultPage = 1;
            const int defaultPageSize = 10;
            var paginatedUpdates = new PaginatedListViewModel<ReleaseUpdateDto>([
                    new ReleaseUpdateDtoBuilder().Build()
                ],
                totalResults: 1,
                page: 1,
                pageSize: 10);

            _releaseUpdatesService.WhereHasPaginatedUpdates(paginatedUpdates);

            var sut = BuildController();

            // Act
            // No page or pageSize query parameters provided
            var result = await sut.GetPaginatedUpdatesForRelease(
                publicationSlug: PublicationSlug,
                releaseSlug: ReleaseSlug);

            // Assert
            _releaseUpdatesService.Assert.GetPaginatedUpdatesForReleaseWasCalled(
                publicationSlug: PublicationSlug,
                releaseSlug: ReleaseSlug,
                page: defaultPage,
                pageSize: defaultPageSize);
            result.AssertOkResult(paginatedUpdates);
        }

        [Fact]
        public async Task GetPaginatedUpdatesForRelease_WhenServiceReturnsNotFound_ReturnsNotFound()
        {
            // Arrange
            _releaseUpdatesService.WhereGetPaginatedUpdatesForReleaseReturnsNotFound();

            var sut = BuildController();

            // Act
            var result = await sut.GetPaginatedUpdatesForRelease(
                publicationSlug: PublicationSlug,
                releaseSlug: ReleaseSlug);

            // Assert
            _releaseUpdatesService.Assert.GetPaginatedUpdatesForReleaseWasCalled(
                publicationSlug: PublicationSlug,
                releaseSlug: ReleaseSlug);
            result.AssertNotFoundResult();
        }
    }

    private ReleaseUpdatesController BuildController() => new(_releaseUpdatesService.Build());
}
