using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Releases;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Releases.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Releases;
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
        public async Task WhenServiceReturnsPaginatedUpdates_ReturnsOk()
        {
            // Arrange
            var paginatedUpdates = new PaginatedListViewModel<ReleaseUpdateDto>([
                    new ReleaseUpdateDtoBuilder().Build()
                ],
                totalResults: 1,
                page: 1,
                pageSize: 10);

            var request = new GetReleaseUpdatesRequest
            {
                PublicationSlug = PublicationSlug,
                ReleaseSlug = ReleaseSlug,
                Page = Page,
                PageSize = PageSize
            };

            _releaseUpdatesService.WhereHasPaginatedUpdates(paginatedUpdates);

            var sut = BuildController();

            // Act
            var result = await sut.GetPaginatedUpdatesForRelease(request);

            // Assert
            _releaseUpdatesService.Assert.GetPaginatedUpdatesForReleaseWasCalled(
                publicationSlug: request.PublicationSlug,
                releaseSlug: request.ReleaseSlug,
                page: request.Page,
                pageSize: request.PageSize);
            result.AssertOkResult(paginatedUpdates);
        }

        [Fact]
        public async Task WhenNoQueryParameters_UsesPaginationDefaults()
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

            // No page or pageSize query parameters set on request
            var request = new GetReleaseUpdatesRequest
            {
                PublicationSlug = PublicationSlug,
                ReleaseSlug = ReleaseSlug
            };

            _releaseUpdatesService.WhereHasPaginatedUpdates(paginatedUpdates);

            var sut = BuildController();

            // Act
            var result = await sut.GetPaginatedUpdatesForRelease(request);

            // Assert
            _releaseUpdatesService.Assert.GetPaginatedUpdatesForReleaseWasCalled(
                publicationSlug: request.PublicationSlug,
                releaseSlug: request.ReleaseSlug,
                page: defaultPage,
                pageSize: defaultPageSize);
            result.AssertOkResult(paginatedUpdates);
        }

        [Fact]
        public async Task WhenServiceReturnsNotFound_ReturnsNotFound()
        {
            // Arrange
            var request = new GetReleaseUpdatesRequest
            {
                PublicationSlug = PublicationSlug,
                ReleaseSlug = ReleaseSlug
            };

            _releaseUpdatesService.WhereGetPaginatedUpdatesForReleaseReturnsNotFound();

            var sut = BuildController();

            // Act
            var result = await sut.GetPaginatedUpdatesForRelease(request);

            // Assert
            _releaseUpdatesService.Assert.GetPaginatedUpdatesForReleaseWasCalled(
                publicationSlug: request.PublicationSlug,
                releaseSlug: request.ReleaseSlug);
            result.AssertNotFoundResult();
        }
    }

    private ReleaseUpdatesController BuildController() => new(_releaseUpdatesService.Build());
}
