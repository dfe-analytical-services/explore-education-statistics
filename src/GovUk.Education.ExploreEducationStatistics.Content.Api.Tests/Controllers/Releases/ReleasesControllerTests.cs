using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Releases;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Releases.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Releases;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers.Releases;

public abstract class ReleasesControllerTests
{
    private readonly ReleasesServiceMockBuilder _releasesService = new();

    private const string PublicationSlug = "test-publication";
    private const int Page = 5;
    private const int PageSize = 50;

    public class GetPaginatedReleasesForPublicationTests : ReleasesControllerTests
    {
        [Fact]
        public async Task WhenServiceReturnsPaginatedReleaseEntries_ReturnsOk()
        {
            // Arrange
            var paginatedReleaseEntries = PaginatedListViewModel<IReleaseEntryDto>.Paginate(
                [
                    new LegacyReleaseEntryDtoBuilder().Build(),
                    new ReleaseEntryDtoBuilder().Build()
                ],
                page: Page,
                pageSize: PageSize);

            var request = new GetPaginatedReleasesForPublicationRequest
            {
                PublicationSlug = PublicationSlug,
                Page = Page,
                PageSize = PageSize
            };

            _releasesService.WhereHasPaginatedReleaseEntries(paginatedReleaseEntries);

            var sut = BuildController();

            // Act
            var result = await sut.GetPaginatedReleaseEntriesForPublication(request);

            // Assert
            _releasesService.Assert.GetPaginatedReleaseEntriesForPublicationWasCalled(
                request.PublicationSlug,
                page: request.Page,
                pageSize: request.PageSize);
            result.AssertOkResult(paginatedReleaseEntries);
        }

        [Fact]
        public async Task WhenNoQueryParameters_UsesPaginationDefaults()
        {
            // Arrange
            const int defaultPage = 1;
            const int defaultPageSize = 10;
            var paginatedReleaseEntries = PaginatedListViewModel<IReleaseEntryDto>.Paginate(
                [
                    new LegacyReleaseEntryDtoBuilder().Build(),
                    new ReleaseEntryDtoBuilder().Build()
                ],
                page: defaultPage,
                pageSize: defaultPageSize);

            // No page or pageSize query parameters set on request
            var request = new GetPaginatedReleasesForPublicationRequest { PublicationSlug = PublicationSlug };

            _releasesService.WhereHasPaginatedReleaseEntries(paginatedReleaseEntries);

            var sut = BuildController();

            // Act
            var result = await sut.GetPaginatedReleaseEntriesForPublication(request);

            // Assert
            _releasesService.Assert.GetPaginatedReleaseEntriesForPublicationWasCalled(
                request.PublicationSlug,
                page: defaultPage,
                pageSize: defaultPageSize);
            result.AssertOkResult(paginatedReleaseEntries);
        }

        [Fact]
        public async Task WhenServiceReturnsNotFound_ReturnsNotFound()
        {
            // Arrange
            var request = new GetPaginatedReleasesForPublicationRequest { PublicationSlug = PublicationSlug };

            _releasesService.WhereGetPaginatedReleaseEntriesForPublicationReturnsNotFound();

            var sut = BuildController();

            // Act
            var result = await sut.GetPaginatedReleaseEntriesForPublication(request);

            // Assert
            _releasesService.Assert.GetPaginatedReleaseEntriesForPublicationWasCalled(
                request.PublicationSlug);
            result.AssertNotFoundResult();
        }
    }

    private ReleasesController BuildController() => new(_releasesService.Build());
}
