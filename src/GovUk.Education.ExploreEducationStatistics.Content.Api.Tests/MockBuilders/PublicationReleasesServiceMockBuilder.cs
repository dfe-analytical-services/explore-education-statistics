using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.MockBuilders;

public class PublicationReleasesServiceMockBuilder
{
    private readonly Mock<IPublicationReleasesService> _mock = new(MockBehavior.Strict);

    private PaginatedListViewModel<IPublicationReleaseEntryDto>? _publicationReleases;
    private Guid[]? _releaseIds;

    private static readonly Expression<
        Func<
            IPublicationReleasesService,
            Task<Either<ActionResult, PaginatedListViewModel<IPublicationReleaseEntryDto>>>
        >
    > GetPublicationReleases = m =>
        m.GetPublicationReleases(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>());

    private static readonly Expression<
        Func<IPublicationReleasesService, Task<Either<ActionResult, Guid[]>>>
    > GetPublicationReleaseIds = m => m.GetPublicationReleaseIds(It.IsAny<string>(), It.IsAny<CancellationToken>());

    public PublicationReleasesServiceMockBuilder()
    {
        _mock
            .Setup(GetPublicationReleases)
            .ReturnsAsync(() =>
                _publicationReleases ?? PaginatedListViewModel<IPublicationReleaseEntryDto>.Paginate([], 1, 10)
            );

        _mock.Setup(GetPublicationReleaseIds).ReturnsAsync(() => _releaseIds ?? []);
    }

    public IPublicationReleasesService Build() => _mock.Object;

    public PublicationReleasesServiceMockBuilder WhereHasPublicationReleases(
        PaginatedListViewModel<IPublicationReleaseEntryDto> publicationReleases
    )
    {
        _publicationReleases = publicationReleases;
        return this;
    }

    public PublicationReleasesServiceMockBuilder WhereHasPublicationReleaseIds(Guid[] releaseIds)
    {
        _releaseIds = releaseIds;
        return this;
    }

    public PublicationReleasesServiceMockBuilder WhereGetPublicationReleasesReturnsNotFound()
    {
        _mock
            .Setup(m =>
                m.GetPublicationReleases(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new NotFoundResult());

        return this;
    }

    public PublicationReleasesServiceMockBuilder WhereGetPublicationReleaseIdsReturnsNotFound()
    {
        _mock
            .Setup(m => m.GetPublicationReleaseIds(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFoundResult());

        return this;
    }

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IPublicationReleasesService> mock)
    {
        public void GetPublicationReleasesWasCalled(
            string? publicationSlug = null,
            int? page = null,
            int? pageSize = null
        )
        {
            mock.Verify(
                m =>
                    m.GetPublicationReleases(
                        It.Is<string>(actual => publicationSlug == null || actual == publicationSlug),
                        It.Is<int>(actual => page == null || actual == page),
                        It.Is<int>(actual => pageSize == null || actual == pageSize),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }

        public void GetPublicationReleaseIdsWasCalled(string? publicationSlug = null)
        {
            mock.Verify(
                m =>
                    m.GetPublicationReleaseIds(
                        It.Is<string>(actual => publicationSlug == null || actual == publicationSlug),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }
    }
}
