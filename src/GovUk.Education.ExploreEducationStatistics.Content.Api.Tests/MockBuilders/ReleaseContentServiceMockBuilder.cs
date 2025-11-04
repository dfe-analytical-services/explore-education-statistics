using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Releases;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.MockBuilders;

public class ReleaseContentServiceMockBuilder
{
    private readonly Mock<IReleaseContentService> _mock = new(MockBehavior.Strict);

    private ReleaseContentDto? _releaseContent;

    private static readonly Expression<
        Func<IReleaseContentService, Task<Either<ActionResult, ReleaseContentDto>>>
    > GetReleaseContent = m =>
        m.GetReleaseContent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>());

    public ReleaseContentServiceMockBuilder()
    {
        _mock.Setup(GetReleaseContent).ReturnsAsync(() => _releaseContent ?? new ReleaseContentDtoBuilder().Build());
    }

    public IReleaseContentService Build() => _mock.Object;

    public ReleaseContentServiceMockBuilder WhereHasReleaseContent(ReleaseContentDto releaseContent)
    {
        _releaseContent = releaseContent;
        return this;
    }

    public ReleaseContentServiceMockBuilder WhereGetReleaseContentReturnsNotFound(
        string publicationSlug,
        string releaseSlug
    )
    {
        _mock
            .Setup(m => m.GetReleaseContent(publicationSlug, releaseSlug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFoundResult());

        return this;
    }

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IReleaseContentService> mock)
    {
        public void GetReleaseContentWasCalled(string? publicationSlug = null, string? releaseSlug = null)
        {
            mock.Verify(
                m =>
                    m.GetReleaseContent(
                        It.Is<string>(actual => publicationSlug == null || actual == publicationSlug),
                        It.Is<string>(actual => releaseSlug == null || actual == releaseSlug),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }
    }
}
