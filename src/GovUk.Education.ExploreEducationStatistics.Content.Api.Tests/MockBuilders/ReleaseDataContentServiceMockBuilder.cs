using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Releases;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.MockBuilders;

public class ReleaseDataContentServiceMockBuilder
{
    private readonly Mock<IReleaseDataContentService> _mock = new(MockBehavior.Strict);

    private ReleaseDataContentDto? _releaseDataContent;

    private static readonly Expression<
        Func<IReleaseDataContentService, Task<Either<ActionResult, ReleaseDataContentDto>>>
    > GetReleaseDataContent = m =>
        m.GetReleaseDataContent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>());

    public ReleaseDataContentServiceMockBuilder()
    {
        _mock
            .Setup(GetReleaseDataContent)
            .ReturnsAsync(() => _releaseDataContent ?? new ReleaseDataContentDtoBuilder().Build());
    }

    public IReleaseDataContentService Build() => _mock.Object;

    public ReleaseDataContentServiceMockBuilder WhereHasReleaseDataContent(ReleaseDataContentDto releaseDataContent)
    {
        _releaseDataContent = releaseDataContent;
        return this;
    }

    public ReleaseDataContentServiceMockBuilder WhereGetReleaseDataContentReturnsNotFound(
        string publicationSlug,
        string releaseSlug
    )
    {
        _mock
            .Setup(m => m.GetReleaseDataContent(publicationSlug, releaseSlug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFoundResult());

        return this;
    }

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IReleaseDataContentService> mock)
    {
        public void GetReleaseDataContentWasCalled(string? publicationSlug = null, string? releaseSlug = null)
        {
            mock.Verify(
                m =>
                    m.GetReleaseDataContent(
                        It.Is<string>(actual => publicationSlug == null || actual == publicationSlug),
                        It.Is<string>(actual => releaseSlug == null || actual == releaseSlug),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }
    }
}
