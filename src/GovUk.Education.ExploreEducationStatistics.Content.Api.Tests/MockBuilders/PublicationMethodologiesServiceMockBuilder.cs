using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.MockBuilders;

public class PublicationMethodologiesServiceMockBuilder
{
    private readonly Mock<IPublicationMethodologiesService> _mock = new(MockBehavior.Strict);

    private PublicationMethodologiesDto? _methodologies;

    private static readonly Expression<Func<IPublicationMethodologiesService,
        Task<Either<ActionResult, PublicationMethodologiesDto>>>> GetPublicationMethodologies =
        m => m.GetPublicationMethodologies(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>());

    public PublicationMethodologiesServiceMockBuilder()
    {
        _mock.Setup(GetPublicationMethodologies)
            .ReturnsAsync(() =>
                _methodologies ?? new PublicationMethodologiesDto
                {
                    Methodologies = [],
                    ExternalMethodology = null
                });
    }

    public IPublicationMethodologiesService Build() => _mock.Object;

    public PublicationMethodologiesServiceMockBuilder WhereHasMethodologies(PublicationMethodologiesDto methodologies)
    {
        _methodologies = methodologies;
        return this;
    }

    public PublicationMethodologiesServiceMockBuilder WhereGetPublicationMethodologiesReturnsNotFound(
        string publicationSlug)
    {
        _mock.Setup(m => m.GetPublicationMethodologies(
                publicationSlug,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFoundResult());

        return this;
    }

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IPublicationMethodologiesService> mock)
    {
        public void GetPublicationMethodologiesWasCalled(string? publicationSlug = null)
        {
            mock.Verify(m => m.GetPublicationMethodologies(
                    It.Is<string>(actual => publicationSlug == null || actual == publicationSlug),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
