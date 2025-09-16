using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.MockBuilders;

public class ReleasesServiceMockBuilder
{
    private readonly Mock<IReleasesService> _mock = new(MockBehavior.Strict);

    private PaginatedListViewModel<IReleaseEntryDto>? _paginatedReleaseEntries;

    private static readonly Expression<Func<IReleasesService,
            Task<Either<ActionResult, PaginatedListViewModel<IReleaseEntryDto>>>>>
        GetPaginatedReleaseEntriesForPublication =
            m => m.GetPaginatedReleaseEntriesForPublication(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>());

    public ReleasesServiceMockBuilder()
    {
        _mock.Setup(GetPaginatedReleaseEntriesForPublication)
            .ReturnsAsync(() => _paginatedReleaseEntries ?? new PaginatedListViewModel<IReleaseEntryDto>([], 0, 1, 10));
    }

    public IReleasesService Build() => _mock.Object;

    public ReleasesServiceMockBuilder WhereHasPaginatedReleaseEntries(
        PaginatedListViewModel<IReleaseEntryDto> paginatedReleaseEntries)
    {
        _paginatedReleaseEntries = paginatedReleaseEntries;
        return this;
    }

    public ReleasesServiceMockBuilder WhereGetPaginatedReleaseEntriesForPublicationReturnsNotFound()
    {
        _mock.Setup(m => m.GetPaginatedReleaseEntriesForPublication(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFoundResult());

        return this;
    }

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IReleasesService> mock)
    {
        public void GetPaginatedReleaseEntriesForPublicationWasCalled(
            string? publicationSlug = null,
            int? page = null,
            int? pageSize = null)
        {
            mock.Verify(m => m.GetPaginatedReleaseEntriesForPublication(
                    It.Is<string>(actual => publicationSlug == null || actual == publicationSlug),
                    It.Is<int>(actual => page == null || actual == page),
                    It.Is<int>(actual => pageSize == null || actual == pageSize),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
