using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Publications;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Publications.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Publications;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Releases;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers.Publications;

public abstract class PublicationsControllerTests
{
    private readonly PublicationMethodologiesServiceMockBuilder _publicationMethodologiesService = new();
    private readonly PublicationReleasesServiceMockBuilder _publicationReleasesService = new();
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

    public class GetPublicationMethodologiesTests : PublicationsControllerTests
    {
        [Fact]
        public async Task WhenServiceReturnsMethodologies_ReturnsOk()
        {
            // Arrange
            var methodologies = new PublicationMethodologiesDtoBuilder()
                .WithMethodologies([new PublicationMethodologyDtoBuilder().Build()])
                .WithExternalMethodology(new PublicationExternalMethodologyDtoBuilder().Build())
                .Build();

            _publicationMethodologiesService.WhereHasMethodologies(methodologies);

            var sut = BuildController();

            // Act
            var result = await sut.GetPublicationMethodologies(PublicationSlug);

            // Assert
            _publicationMethodologiesService.Assert.GetPublicationMethodologiesWasCalled(PublicationSlug);
            result.AssertOkResult(methodologies);
        }

        [Fact]
        public async Task WhenServiceReturnsNotFound_ReturnsNotFound()
        {
            // Arrange
            _publicationMethodologiesService.WhereGetPublicationMethodologiesReturnsNotFound(PublicationSlug);

            var sut = BuildController();

            // Act
            var result = await sut.GetPublicationMethodologies(PublicationSlug);

            // Assert
            _publicationMethodologiesService.Assert.GetPublicationMethodologiesWasCalled(PublicationSlug);
            result.AssertNotFoundResult();
        }
    }

    public class GetPublicationReleasesTests : PublicationsControllerTests
    {
        private const int Page = 1;
        private const int PageSize = 10;

        [Fact]
        public async Task WhenServiceReturnsPublicationReleases_ReturnsOk()
        {
            // Arrange
            var publicationReleases = PaginatedListViewModel<IPublicationReleaseEntryDto>.Paginate(
                [
                    new LegacyPublicationReleaseEntryDtoBuilder().Build(),
                    new PublicationReleaseEntryDtoBuilder().Build(),
                ],
                page: Page,
                pageSize: PageSize
            );

            var request = new GetPublicationReleasesRequest
            {
                PublicationSlug = PublicationSlug,
                Page = Page,
                PageSize = PageSize,
            };

            _publicationReleasesService.WhereHasPublicationReleases(publicationReleases);

            var sut = BuildController();

            // Act
            var result = await sut.GetPublicationReleases(request);

            // Assert
            _publicationReleasesService.Assert.GetPublicationReleasesWasCalled(
                request.PublicationSlug,
                page: request.Page,
                pageSize: request.PageSize
            );
            result.AssertOkResult(publicationReleases);
        }

        [Fact]
        public async Task WhenNoQueryParameters_UsesPaginationDefaults()
        {
            // Arrange
            const int defaultPage = 1;
            const int defaultPageSize = 10;
            var publicationReleases = PaginatedListViewModel<IPublicationReleaseEntryDto>.Paginate(
                [
                    new LegacyPublicationReleaseEntryDtoBuilder().Build(),
                    new PublicationReleaseEntryDtoBuilder().Build(),
                ],
                page: defaultPage,
                pageSize: defaultPageSize
            );

            // No page or pageSize query parameters set on request
            var request = new GetPublicationReleasesRequest { PublicationSlug = PublicationSlug };

            _publicationReleasesService.WhereHasPublicationReleases(publicationReleases);

            var sut = BuildController();

            // Act
            var result = await sut.GetPublicationReleases(request);

            // Assert
            _publicationReleasesService.Assert.GetPublicationReleasesWasCalled(
                request.PublicationSlug,
                page: defaultPage,
                pageSize: defaultPageSize
            );
            result.AssertOkResult(publicationReleases);
        }

        [Fact]
        public async Task WhenServiceReturnsNotFound_ReturnsNotFound()
        {
            // Arrange
            var request = new GetPublicationReleasesRequest { PublicationSlug = PublicationSlug };

            _publicationReleasesService.WhereGetPublicationReleasesReturnsNotFound();

            var sut = BuildController();

            // Act
            var result = await sut.GetPublicationReleases(request);

            // Assert
            _publicationReleasesService.Assert.GetPublicationReleasesWasCalled(request.PublicationSlug);
            result.AssertNotFoundResult();
        }
    }

    private PublicationsController BuildController() =>
        new(
            _publicationMethodologiesService.Build(),
            _publicationReleasesService.Build(),
            _publicationsService.Build()
        );
}
