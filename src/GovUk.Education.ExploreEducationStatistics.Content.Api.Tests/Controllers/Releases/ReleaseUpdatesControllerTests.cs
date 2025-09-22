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
    private const int Page = 1;
    private const int PageSize = 10;

    public class GetReleaseUpdatesTests : ReleaseUpdatesControllerTests
    {
        [Fact]
        public async Task WhenServiceReturnsReleaseUpdates_ReturnsOk()
        {
            // Arrange
            var releaseUpdates = PaginatedListViewModel<ReleaseUpdateDto>.Paginate([
                    new ReleaseUpdateDtoBuilder().Build()
                ],
                page: Page,
                pageSize: PageSize);

            var request = new GetReleaseUpdatesRequest
            {
                PublicationSlug = PublicationSlug,
                ReleaseSlug = ReleaseSlug,
                Page = Page,
                PageSize = PageSize
            };

            _releaseUpdatesService.WhereHasReleaseUpdates(releaseUpdates);

            var sut = BuildController();

            // Act
            var result = await sut.GetReleaseUpdates(request);

            // Assert
            _releaseUpdatesService.Assert.GetReleaseUpdatesWasCalled(
                publicationSlug: request.PublicationSlug,
                releaseSlug: request.ReleaseSlug,
                page: request.Page,
                pageSize: request.PageSize);
            result.AssertOkResult(releaseUpdates);
        }

        [Fact]
        public async Task WhenNoQueryParameters_UsesPaginationDefaults()
        {
            // Arrange
            const int defaultPage = 1;
            const int defaultPageSize = 10;
            var releaseUpdates = PaginatedListViewModel<ReleaseUpdateDto>.Paginate([
                    new ReleaseUpdateDtoBuilder().Build()
                ],
                page: defaultPage,
                pageSize: defaultPageSize);

            // No page or pageSize query parameters set on request
            var request = new GetReleaseUpdatesRequest
            {
                PublicationSlug = PublicationSlug,
                ReleaseSlug = ReleaseSlug
            };

            _releaseUpdatesService.WhereHasReleaseUpdates(releaseUpdates);

            var sut = BuildController();

            // Act
            var result = await sut.GetReleaseUpdates(request);

            // Assert
            _releaseUpdatesService.Assert.GetReleaseUpdatesWasCalled(
                publicationSlug: request.PublicationSlug,
                releaseSlug: request.ReleaseSlug,
                page: defaultPage,
                pageSize: defaultPageSize);
            result.AssertOkResult(releaseUpdates);
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

            _releaseUpdatesService.WhereGetReleaseUpdatesReturnsNotFound();

            var sut = BuildController();

            // Act
            var result = await sut.GetReleaseUpdates(request);

            // Assert
            _releaseUpdatesService.Assert.GetReleaseUpdatesWasCalled(
                publicationSlug: request.PublicationSlug,
                releaseSlug: request.ReleaseSlug);
            result.AssertNotFoundResult();
        }
    }

    private ReleaseUpdatesController BuildController() => new(_releaseUpdatesService.Build());
}
