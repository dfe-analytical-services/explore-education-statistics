using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.DataSets;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.DataSets;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Content.Services.DataSets.Dtos;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers.DataSets;

public abstract class DataSetSearchableDocumentsControllerTests
{
    private readonly DataSetSearchableDocumentsServiceMockBuilder _dataSetSearchableDocumentsService = new();

    private const string PublicationSlug = "test-publication";
    private const string ReleaseSlug = "test-release";

    public class GetReleaseDataSetSearchableDocumentsTests : DataSetSearchableDocumentsControllerTests
    {
        [Fact]
        public async Task WhenServiceReturnsSearchableDocuments_ReturnsOk()
        {
            // Arrange
            var searchableDocuments = new[]
            {
                new DataSetSearchableDocumentDtoBuilder().WithTitle("Data set 1").Build(),
                new DataSetSearchableDocumentDtoBuilder().WithTitle("Data set 2").Build(),
            };
            _dataSetSearchableDocumentsService.WhereHasSearchableDocuments(searchableDocuments);

            var sut = BuildController();

            // Act
            var result = await sut.GetReleaseDataSetSearchableDocuments(PublicationSlug, ReleaseSlug);

            // Assert
            _dataSetSearchableDocumentsService.Assert.GetReleaseDataSetSearchableDocumentsWasCalled(
                PublicationSlug,
                ReleaseSlug
            );
            result.AssertOkResult(searchableDocuments);
        }

        [Fact]
        public async Task WhenServiceReturnsNotFound_ReturnsNotFound()
        {
            // Arrange
            _dataSetSearchableDocumentsService.WhereGetReleaseDataSetSearchableDocumentsReturnsNotFound();

            var sut = BuildController();

            // Act
            var result = await sut.GetReleaseDataSetSearchableDocuments(PublicationSlug, ReleaseSlug);

            // Assert
            _dataSetSearchableDocumentsService.Assert.GetReleaseDataSetSearchableDocumentsWasCalled(
                PublicationSlug,
                ReleaseSlug
            );
            result.AssertNotFoundResult();
        }
    }

    public class GetPublicationDataSetSearchableDocumentsTests : DataSetSearchableDocumentsControllerTests
    {
        [Fact]
        public async Task WhenServiceReturnsSearchableDocuments_ReturnsOk()
        {
            // Arrange
            var searchableDocuments = new[]
            {
                new DataSetSearchableDocumentDtoBuilder().WithTitle("Data set 1").Build(),
                new DataSetSearchableDocumentDtoBuilder().WithTitle("Data set 2").Build(),
            };
            _dataSetSearchableDocumentsService.WhereHasSearchableDocuments(searchableDocuments);

            var sut = BuildController();

            // Act
            var result = await sut.GetPublicationDataSetSearchableDocuments(PublicationSlug);

            // Assert
            _dataSetSearchableDocumentsService.Assert.GetPublicationDataSetSearchableDocumentsWasCalled(
                PublicationSlug
            );
            result.AssertOkResult(searchableDocuments);
        }

        [Fact]
        public async Task WhenServiceReturnsNotFound_ReturnsNotFound()
        {
            // Arrange
            _dataSetSearchableDocumentsService.WhereGetPublicationDataSetSearchableDocumentsReturnsNotFound();

            var sut = BuildController();

            // Act
            var result = await sut.GetPublicationDataSetSearchableDocuments(PublicationSlug);

            // Assert
            _dataSetSearchableDocumentsService.Assert.GetPublicationDataSetSearchableDocumentsWasCalled(
                PublicationSlug
            );
            result.AssertNotFoundResult();
        }
    }

    public class GetDataSetSearchableDocumentKeysTests : DataSetSearchableDocumentsControllerTests
    {
        [Fact]
        public async Task WhenServiceReturnsSearchableDocumentKeys_ReturnsOk()
        {
            // Arrange
            var searchableDocumentKeys = new[]
            {
                new DataSetSearchableDocumentKeyDto
                {
                    PublicationId = Guid.NewGuid(),
                    ReleaseId = Guid.NewGuid(),
                    FileId = Guid.NewGuid(),
                },
                new DataSetSearchableDocumentKeyDto
                {
                    PublicationId = Guid.NewGuid(),
                    ReleaseId = Guid.NewGuid(),
                    FileId = Guid.NewGuid(),
                },
            };
            _dataSetSearchableDocumentsService.WhereHasSearchableDocumentKeys(searchableDocumentKeys);

            var sut = BuildController();

            // Act
            var result = await sut.GetDataSetSearchableDocumentKeys();

            // Assert
            _dataSetSearchableDocumentsService.Assert.GetDataSetSearchableDocumentKeysWasCalled();
            result.AssertOkResult(searchableDocumentKeys);
        }
    }

    private DataSetSearchableDocumentsController BuildController() => new(_dataSetSearchableDocumentsService.Build());
}
