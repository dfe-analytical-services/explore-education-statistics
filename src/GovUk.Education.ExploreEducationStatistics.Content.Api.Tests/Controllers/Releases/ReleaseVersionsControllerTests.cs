using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Releases;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Releases;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.MockBuilders;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers.Releases;

public abstract class ReleaseVersionsControllerTests
{
    private readonly ReleaseVersionsServiceMockBuilder _releaseVersionsService = new();

    private const string PublicationSlug = "test-publication";
    private const string ReleaseSlug = "test-release";

    public class GetReleaseVersionSummaryTests : ReleaseVersionsControllerTests
    {
        [Fact]
        public async Task WhenServiceReturnsReleaseVersion_ReturnsOk()
        {
            // Arrange
            var releaseVersionSummary = new ReleaseVersionSummaryDtoBuilder().Build();
            _releaseVersionsService.WhereHasReleaseVersionSummary(releaseVersionSummary);

            var sut = BuildController();

            // Act
            var result = await sut.GetReleaseVersionSummary(PublicationSlug, ReleaseSlug);

            // Assert
            _releaseVersionsService.Assert.GetReleaseVersionSummaryWasCalled(PublicationSlug, ReleaseSlug);
            result.AssertOkResult(releaseVersionSummary);
        }

        [Fact]
        public async Task WhenServiceReturnsNotFound_ReturnsNotFound()
        {
            // Arrange
            _releaseVersionsService.WhereGetReleaseVersionSummaryReturnsNotFound(PublicationSlug, ReleaseSlug);

            var sut = BuildController();

            // Act
            var result = await sut.GetReleaseVersionSummary(PublicationSlug, ReleaseSlug);

            // Assert
            _releaseVersionsService.Assert.GetReleaseVersionSummaryWasCalled(PublicationSlug, ReleaseSlug);
            result.AssertNotFoundResult();
        }
    }

    private ReleaseVersionsController BuildController() => new(_releaseVersionsService.Build());
}
