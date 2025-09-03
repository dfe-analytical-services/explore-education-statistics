using GovUk.Education.ExploreEducationStatistics.Content.Services.RelatedInformation;
using GovUk.Education.ExploreEducationStatistics.Content.Services.RelatedInformation.Dtos;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.MockBuilders;

public class RelatedInformationServiceMockBuilder
{
    private readonly Mock<IRelatedInformationService> _mock = new(MockBehavior.Strict);

    private RelatedInformationDto[]? _relatedInformation;

    public IRelatedInformationService Build()
    {
        _mock.Setup(m => m.GetRelatedInformationForRelease(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_relatedInformation ?? []);

        return _mock.Object;
    }

    public RelatedInformationServiceMockBuilder WhereHasRelatedInformation(RelatedInformationDto[] relatedInformation)
    {
        _relatedInformation = relatedInformation;
        return this;
    }

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IRelatedInformationService> mock)
    {
        public void GetRelatedInformationForReleaseWasCalled(
            string? publicationSlug = null,
            string? releaseSlug = null)
        {
            mock.Verify(m => m.GetRelatedInformationForRelease(
                    It.Is<string>(actual => publicationSlug == null || actual == publicationSlug),
                    It.Is<string>(actual => releaseSlug == null || actual == releaseSlug),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
