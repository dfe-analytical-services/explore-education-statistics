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
        [Fact]
        public async Task WhenServiceReturnsReleases_ReturnsOk()
        {
            // Arrange
            const int page = 2;
            const int pageSize = 5;

            var publicationReleases = PaginatedListViewModel<IPublicationReleaseEntryDto>.Paginate(
                [
                    new LegacyPublicationReleaseEntryDtoBuilder().Build(),
                    new PublicationReleaseEntryDtoBuilder().Build(),
                ],
                page: page,
                pageSize: pageSize
            );

            var request = new GetPublicationReleasesRequest
            {
                PublicationSlug = PublicationSlug,
                Page = page,
                PageSize = pageSize,
            };

            _publicationReleasesService.WhereHasPublicationReleases(publicationReleases);

            var sut = BuildController();

            // Act
            var result = await sut.GetPublicationReleases(request);

            // Assert
            _publicationReleasesService.Assert.GetPublicationReleasesWasCalled(
                PublicationSlug,
                page: page,
                pageSize: pageSize
            );
            result.AssertOkResult(publicationReleases);
        }

        [Fact]
        public async Task WhenPageAndPageSizeAreNull_PassesNullToService()
        {
            // Arrange
            var publicationReleases = PaginatedListViewModel<IPublicationReleaseEntryDto>.Paginate(
                [
                    new LegacyPublicationReleaseEntryDtoBuilder().Build(),
                    new PublicationReleaseEntryDtoBuilder().Build(),
                ],
                page: 1,
                pageSize: 2
            );

            // Pagination parameters omitted from request (Page and PageSize are null)
            var request = new GetPublicationReleasesRequest
            {
                PublicationSlug = PublicationSlug,
                Page = null,
                PageSize = null,
            };

            _publicationReleasesService.WhereHasPublicationReleases(publicationReleases);

            var sut = BuildController();

            // Act
            var result = await sut.GetPublicationReleases(request);

            // Assert
            // The service should be called with null Page and PageSize so that all results are returned
            _publicationReleasesService.Assert.GetPublicationReleasesWasCalled(
                PublicationSlug,
                page: null,
                pageSize: null
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
            _publicationReleasesService.Assert.GetPublicationReleasesWasCalled(PublicationSlug);
            result.AssertNotFoundResult();
        }
    }

    public class GetPublicationReleaseIdsTests : PublicationsControllerTests
    {
        [Fact]
        public async Task WhenServiceReturnsPublicationReleaseIds_ReturnsOk()
        {
            // Arrange
            Guid[] releaseIds = [Guid.NewGuid(), Guid.NewGuid()];
            _publicationReleasesService.WhereHasPublicationReleaseIds(releaseIds);

            var sut = BuildController();

            // Act
            var result = await sut.GetPublicationReleaseIds(PublicationSlug);

            // Assert
            _publicationReleasesService.Assert.GetPublicationReleaseIdsWasCalled(PublicationSlug);
            result.AssertOkResult(releaseIds);
        }

        [Fact]
        public async Task WhenServiceReturnsNotFound_ReturnsNotFound()
        {
            // Arrange
            _publicationReleasesService.WhereGetPublicationReleaseIdsReturnsNotFound();

            var sut = BuildController();

            // Act
            var result = await sut.GetPublicationReleaseIds(PublicationSlug);

            // Assert
            _publicationReleasesService.Assert.GetPublicationReleaseIdsWasCalled(PublicationSlug);
            result.AssertNotFoundResult();
        }
    }

    public class GetPublicationSummaryTests : PublicationsControllerTests
    {
        private readonly Guid _publicationId = Guid.NewGuid();

        [Fact]
        public async Task WhenServiceReturnsPublicationSummary_ReturnsOk()
        {
            // Arrange
            var publicationSummary = new PublicationSummaryDtoBuilder().Build();
            _publicationsService.WhereHasPublicationSummary(publicationSummary);

            var sut = BuildController();

            // Act
            var result = await sut.GetPublicationSummary(_publicationId);

            // Assert
            _publicationsService.Assert.GetPublicationSummaryWasCalled(_publicationId);
            result.AssertOkResult(publicationSummary);
        }

        [Fact]
        public async Task WhenServiceReturnsNotFound_ReturnsNotFound()
        {
            // Arrange
            _publicationsService.WhereGetPublicationSummaryReturnsNotFound(_publicationId);

            var sut = BuildController();

            // Act
            var result = await sut.GetPublicationSummary(_publicationId);

            // Assert
            _publicationsService.Assert.GetPublicationSummaryWasCalled(_publicationId);
            result.AssertNotFoundResult();
        }
    }

    public class GetPublicationTitleTests : PublicationsControllerTests
    {
        [Fact]
        public async Task WhenServiceReturnsPublicationTitle_ReturnsOk()
        {
            // Arrange
            var publicationTitle = new PublicationTitleDtoBuilder().Build();
            _publicationsService.WhereHasPublicationTitle(publicationTitle);

            var sut = BuildController();

            // Act
            var result = await sut.GetPublicationTitle(PublicationSlug);

            // Assert
            _publicationsService.Assert.GetPublicationTitleWasCalled(PublicationSlug);
            result.AssertOkResult(publicationTitle);
        }

        [Fact]
        public async Task WhenServiceReturnsNotFound_ReturnsNotFound()
        {
            // Arrange
            _publicationsService.WhereGetPublicationTitleReturnsNotFound(PublicationSlug);

            var sut = BuildController();

            // Act
            var result = await sut.GetPublicationTitle(PublicationSlug);

            // Assert
            _publicationsService.Assert.GetPublicationTitleWasCalled(PublicationSlug);
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
