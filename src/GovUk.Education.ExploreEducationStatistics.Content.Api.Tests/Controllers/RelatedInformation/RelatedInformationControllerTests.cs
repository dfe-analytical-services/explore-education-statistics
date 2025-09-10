using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.RelatedInformation;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Content.Services.RelatedInformation.Dtos;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers.RelatedInformation;

public abstract class RelatedInformationControllerTests
{
    private readonly RelatedInformationServiceMockBuilder _relatedInformationService = new();

    private const string PublicationSlug = "test-publication";
    private const string ReleaseSlug = "test-release";

    public class GetRelatedInformationForReleaseTests : RelatedInformationControllerTests
    {
        [Fact]
        public async Task GetRelatedInformationForRelease_WhenServiceReturnsRelatedInformation_ReturnsOk()
        {
            // Arrange
            RelatedInformationDto[] relatedInformation =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Related information 1",
                    Url = "https://example.com/related-1"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Related information 2",
                    Url = "https://example.com/related-2"
                }
            ];
            _relatedInformationService.WhereHasRelatedInformation(relatedInformation);

            var sut = BuildController();

            // Act
            var result = await sut.GetRelatedInformationForRelease(
                publicationSlug: PublicationSlug,
                releaseSlug: ReleaseSlug);

            // Assert
            _relatedInformationService.Assert.GetRelatedInformationForReleaseWasCalled(
                publicationSlug: PublicationSlug,
                releaseSlug: ReleaseSlug);
            result.AssertOkResult(relatedInformation);
        }

        [Fact]
        public async Task GetRelatedInformationForRelease_WhenServiceReturnsNotFound_ReturnsNotFound()
        {
            // Arrange
            _relatedInformationService.WhereGetRelatedInformationForReleaseReturnsNotFound(
                publicationSlug: PublicationSlug,
                releaseSlug: ReleaseSlug);

            var sut = BuildController();

            // Act
            var result = await sut.GetRelatedInformationForRelease(
                publicationSlug: PublicationSlug,
                releaseSlug: ReleaseSlug);

            // Assert
            _relatedInformationService.Assert.GetRelatedInformationForReleaseWasCalled(
                publicationSlug: PublicationSlug,
                releaseSlug: ReleaseSlug);
            result.AssertNotFoundResult();
        }
    }

    private RelatedInformationController BuildController() => new(_relatedInformationService.Build());
}
