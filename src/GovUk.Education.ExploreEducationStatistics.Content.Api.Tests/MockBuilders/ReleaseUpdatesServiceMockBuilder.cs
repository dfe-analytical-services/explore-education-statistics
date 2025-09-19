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

    private PaginatedListViewModel<ReleaseUpdateDto>? _releaseUpdates;

    private static readonly Expression<Func<IReleaseUpdatesService,
        Task<Either<ActionResult, PaginatedListViewModel<ReleaseUpdateDto>>>>> GetReleaseUpdates =
        m => m.GetReleaseUpdates(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>());

    public ReleaseUpdatesServiceMockBuilder()
    {
        _mock.Setup(GetReleaseUpdates)
            .ReturnsAsync(() => _releaseUpdates ?? PaginatedListViewModel<ReleaseUpdateDto>.Paginate([], 1, 10));
    }

    public IReleaseUpdatesService Build() => _mock.Object;

    public ReleaseUpdatesServiceMockBuilder WhereHasReleaseUpdates(
        PaginatedListViewModel<ReleaseUpdateDto> releaseUpdates)
    {
        _releaseUpdates = releaseUpdates;
        return this;
    }

    public ReleaseUpdatesServiceMockBuilder WhereGetReleaseUpdatesReturnsNotFound()
    {
        _mock.Setup(m => m.GetReleaseUpdates(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFoundResult());

        return this;
    }

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IReleaseUpdatesService> mock)
    {
        public void GetReleaseUpdatesWasCalled(
            string? publicationSlug = null,
            string? releaseSlug = null,
            int? page = null,
            int? pageSize = null)
        {
            mock.Verify(m => m.GetReleaseUpdates(
                    It.Is<string>(actual => publicationSlug == null || actual == publicationSlug),
                    It.Is<string>(actual => releaseSlug == null || actual == releaseSlug),
                    It.Is<int>(actual => page == null || actual == page),
                    It.Is<int>(actual => pageSize == null || actual == pageSize),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
