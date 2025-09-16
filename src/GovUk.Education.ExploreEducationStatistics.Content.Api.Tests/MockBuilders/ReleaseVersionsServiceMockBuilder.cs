using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Releases;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.MockBuilders;

public class ReleaseVersionsServiceMockBuilder
{
    private readonly Mock<IReleaseVersionsService> _mock = new(MockBehavior.Strict);

    private ReleaseVersionSummaryDto? _releaseVersionSummary;

    private static readonly Expression<Func<IReleaseVersionsService,
        Task<Either<ActionResult, ReleaseVersionSummaryDto>>>> GetReleaseVersionSummary =
        m => m.GetReleaseVersionSummary(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>());

    public ReleaseVersionsServiceMockBuilder()
    {
        _mock.Setup(GetReleaseVersionSummary).ReturnsAsync(() =>
            _releaseVersionSummary ?? new ReleaseVersionSummaryDtoBuilder().Build());
    }

    public IReleaseVersionsService Build() => _mock.Object;

    public ReleaseVersionsServiceMockBuilder WhereHasReleaseVersionSummary(
        ReleaseVersionSummaryDto releaseVersionSummary)
    {
        _releaseVersionSummary = releaseVersionSummary;
        return this;
    }

    public ReleaseVersionsServiceMockBuilder WhereGetReleaseVersionSummaryReturnsNotFound(
        string publicationSlug,
        string releaseSlug)
    {
        _mock.Setup(m => m.GetReleaseVersionSummary(
                publicationSlug,
                releaseSlug,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFoundResult());

        return this;
    }

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IReleaseVersionsService> mock)
    {
        public void GetReleaseVersionSummaryWasCalled(
            string? publicationSlug = null,
            string? releaseSlug = null)
        {
            mock.Verify(m => m.GetReleaseVersionSummary(
                    It.Is<string>(actual => publicationSlug == null || actual == publicationSlug),
                    It.Is<string>(actual => releaseSlug == null || actual == releaseSlug),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
