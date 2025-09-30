using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Publications;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.MockBuilders;

public class PublicationsServiceMockBuilder
{
    private readonly Mock<IPublicationsService> _mock = new(MockBehavior.Strict);

    private PublicationDto? _publication;

    private static readonly Expression<Func<IPublicationsService,
        Task<Either<ActionResult, PublicationDto>>>> GetPublication =
        m => m.GetPublication(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>());

    public PublicationsServiceMockBuilder()
    {
        _mock.Setup(GetPublication).ReturnsAsync(() => _publication ?? new PublicationDtoBuilder().Build());
    }

    public IPublicationsService Build() => _mock.Object;

    public PublicationsServiceMockBuilder WhereHasPublication(PublicationDto publication)
    {
        _publication = publication;
        return this;
    }

    public PublicationsServiceMockBuilder WhereGetPublicationReturnsNotFound(string publicationSlug)
    {
        _mock.Setup(m => m.GetPublication(
                publicationSlug,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFoundResult());

        return this;
    }

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IPublicationsService> mock)
    {
        public void GetPublicationWasCalled(string? publicationSlug = null)
        {
            mock.Verify(m => m.GetPublication(
                    It.Is<string>(actual => publicationSlug == null || actual == publicationSlug),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
