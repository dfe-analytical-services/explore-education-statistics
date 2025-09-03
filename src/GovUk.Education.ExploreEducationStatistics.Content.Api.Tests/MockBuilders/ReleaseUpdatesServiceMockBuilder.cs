using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.MockBuilders;

public class ReleaseUpdatesServiceMockBuilder
{
    private readonly Mock<IReleaseUpdatesService> _mock = new(MockBehavior.Strict);

    private PaginatedListViewModel<ReleaseUpdateDto>? _paginatedUpdates;

    private static readonly Expression<Func<IReleaseUpdatesService,
        Task<Either<ActionResult, PaginatedListViewModel<ReleaseUpdateDto>>>>> GetPaginatedUpdatesForRelease =
        m => m.GetPaginatedUpdatesForRelease(
            It.IsAny<GetReleaseUpdatesRequest>(),
            It.IsAny<CancellationToken>());

    public ReleaseUpdatesServiceMockBuilder()
    {
        _mock.Setup(GetPaginatedUpdatesForRelease)
            .ReturnsAsync(() => _paginatedUpdates ?? new PaginatedListViewModel<ReleaseUpdateDto>([], 0, 1, 10));
    }

    public IReleaseUpdatesService Build() => _mock.Object;

    public ReleaseUpdatesServiceMockBuilder WhereHasPaginatedUpdates(
        PaginatedListViewModel<ReleaseUpdateDto> paginatedUpdates)
    {
        _paginatedUpdates = paginatedUpdates;
        return this;
    }

    public ReleaseUpdatesServiceMockBuilder WhereGetPaginatedUpdatesForReleaseReturnsNotFound()
    {
        _mock.Setup(m => m.GetPaginatedUpdatesForRelease(
                It.IsAny<GetReleaseUpdatesRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFoundResult());

        return this;
    }

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IReleaseUpdatesService> mock)
    {
        public void GetPaginatedUpdatesForReleaseWasCalled(
            string? publicationSlug = null,
            string? releaseSlug = null,
            int? page = null,
            int? pageSize = null)
        {
            mock.Verify(m => m.GetPaginatedUpdatesForRelease(
                    It.Is<GetReleaseUpdatesRequest>(request =>
                        (publicationSlug == null || request.PublicationSlug == publicationSlug) &&
                        (releaseSlug == null || request.ReleaseSlug == releaseSlug) &&
                        (page == null || request.Page == page) &&
                        (pageSize == null || request.PageSize == pageSize)),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
