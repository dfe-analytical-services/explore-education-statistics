using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.Releases;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Releases;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.MockBuilders;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers.Releases;

public abstract class ReleaseSearchableDocumentsControllerTests
{
    private readonly ReleaseSearchableDocumentsServiceMockBuilder _releaseSearchableDocumentsService =
        new();

    private const string PublicationSlug = "test-publication";

    public class GetLatestReleaseAsSearchableDocumentTests
        : ReleaseSearchableDocumentsControllerTests
    {
        [Fact]
        public async Task WhenServiceReturnsSearchableDocument_ReturnsOk()
        {
            // Arrange
            var searchableDocument = new ReleaseSearchableDocumentDtoBuilder().Build();
            _releaseSearchableDocumentsService.WhereHasSearchableDocument(searchableDocument);

            var sut = BuildController();

            // Act
            var result = await sut.GetLatestReleaseAsSearchableDocument(PublicationSlug);

            // Assert
            _releaseSearchableDocumentsService.Assert.GetLatestReleaseAsSearchableDocumentWasCalled(
                PublicationSlug
            );
            result.AssertOkResult(searchableDocument);
        }

        [Fact]
        public async Task WhenServiceReturnsNotFound_ReturnsNotFound()
        {
            // Arrange
            _releaseSearchableDocumentsService.WhereGetLatestReleaseAsSearchableDocumentReturnsNotFound();

            var sut = BuildController();

            // Act
            var result = await sut.GetLatestReleaseAsSearchableDocument(PublicationSlug);

            // Assert
            _releaseSearchableDocumentsService.Assert.GetLatestReleaseAsSearchableDocumentWasCalled(
                PublicationSlug
            );
            result.AssertNotFoundResult();
        }
    }

    private ReleaseSearchableDocumentsController BuildController() =>
        new(_releaseSearchableDocumentsService.Build());
}
