using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveReleaseDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Services.RemoveReleaseDocuments;

public class ReleaseDocumentRemoverTests
{
    private readonly ContentApiClientMockBuilder _contentApiClientMockBuilder = new();
    private readonly AzureBlobStorageClientMockBuilder _azureBlobStorageClientMockBuilder = new();
    private AppOptions _appOptions = new();

    private ReleaseDocumentRemover GetSut() =>
        new(
            _contentApiClientMockBuilder.Build(),
            _azureBlobStorageClientMockBuilder.Build(),
            Microsoft.Extensions.Options.Options.Create(_appOptions)
        );

    public class BasicTests : ReleaseDocumentRemoverTests
    {
        [Fact]
        public void CanInstantiateSut() => Assert.NotNull(GetSut());
    }

    public class RemovePublicationReleaseDocumentsTests : ReleaseDocumentRemoverTests
    {
        [Fact]
        public async Task ShouldRequestPublicationReleaseIdsFromContentApi()
        {
            var request = new RemovePublicationReleaseDocumentsRequest { PublicationSlug = "publication-slug" };

            await GetSut().RemovePublicationReleaseDocuments(request);

            _contentApiClientMockBuilder.Assert.ReleaseIdsRequestedForPublication("publication-slug");
        }

        [Fact]
        public async Task ShouldNotDeleteBlobsIfPublicationHasNoReleases()
        {
            Guid[] releaseIds = [];
            var request = new RemovePublicationReleaseDocumentsRequest { PublicationSlug = "publication-slug" };
            _contentApiClientMockBuilder.WherePublicationHasReleaseIds(releaseIds);

            await GetSut().RemovePublicationReleaseDocuments(request);

            _azureBlobStorageClientMockBuilder.Assert.NoBlobsDeleted();
        }

        [Fact]
        public async Task ShouldDeleteBlobsFromExpectedStorageContainer()
        {
            _appOptions = new AppOptions
            {
                SearchStorageConnectionString = "azure storage connection string",
                ReleaseDocumentsContainerName = "release-documents-container-name",
            };

            Guid[] releaseIds = [Guid.NewGuid()];
            var request = new RemovePublicationReleaseDocumentsRequest { PublicationSlug = "publication-slug" };
            _contentApiClientMockBuilder.WherePublicationHasReleaseIds(releaseIds);

            await GetSut().RemovePublicationReleaseDocuments(request);

            _azureBlobStorageClientMockBuilder.Assert.BlobWasDeleted(
                containerName: _appOptions.ReleaseDocumentsContainerName
            );
        }

        [Fact]
        public async Task ShouldDeleteBlobsForReleases()
        {
            Guid[] releaseIds = [Guid.NewGuid(), Guid.NewGuid()];
            var request = new RemovePublicationReleaseDocumentsRequest { PublicationSlug = "publication-slug" };
            _contentApiClientMockBuilder.WherePublicationHasReleaseIds(releaseIds);

            await GetSut().RemovePublicationReleaseDocuments(request);

            _azureBlobStorageClientMockBuilder.Assert.BlobWasDeleted(blobName: releaseIds[0].ToString());
            _azureBlobStorageClientMockBuilder.Assert.BlobWasDeleted(blobName: releaseIds[1].ToString());
        }

        [Fact]
        public async Task ShouldReturnDeletionResultInResponse()
        {
            Guid[] releaseIds = [Guid.NewGuid(), Guid.NewGuid()];
            var request = new RemovePublicationReleaseDocumentsRequest { PublicationSlug = "publication-slug" };
            _contentApiClientMockBuilder.WherePublicationHasReleaseIds(releaseIds);
            _azureBlobStorageClientMockBuilder.WhereDeleteBlobFailsFor(releaseIds[0].ToString());

            var response = await GetSut().RemovePublicationReleaseDocuments(request);

            Assert.Equal(2, response.ReleaseIdToDeletionResult.Count);
            Assert.False(response.ReleaseIdToDeletionResult[releaseIds[0]]);
            Assert.True(response.ReleaseIdToDeletionResult[releaseIds[1]]);
        }
    }

    public class RemoveReleaseDocumentTests : ReleaseDocumentRemoverTests
    {
        [Fact]
        public async Task ShouldDeleteBlobFromExpectedStorageContainer()
        {
            // Arrange
            _appOptions = new AppOptions
            {
                SearchStorageConnectionString = "azure storage connection string",
                ReleaseDocumentsContainerName = "release-documents-container-name",
            };

            var releaseId = Guid.NewGuid();

            var request = new RemoveReleaseDocumentRequest { ReleaseId = releaseId };

            // Act
            await GetSut().RemoveReleaseDocument(request);

            // Assert
            _azureBlobStorageClientMockBuilder.Assert.BlobWasDeleted(
                containerName: _appOptions.ReleaseDocumentsContainerName
            );
        }

        [Fact]
        public async Task ShouldDeleteBlobForRelease()
        {
            // Arrange
            var releaseId = Guid.NewGuid();

            var request = new RemoveReleaseDocumentRequest { ReleaseId = releaseId };

            // Act
            await GetSut().RemoveReleaseDocument(request);

            // Assert
            _azureBlobStorageClientMockBuilder.Assert.BlobWasDeleted(blobName: releaseId.ToString());
        }

        [Fact]
        public async Task ShouldReturnDeletionResultInResponse()
        {
            // Arrange
            var releaseId = Guid.NewGuid();

            var request = new RemoveReleaseDocumentRequest { ReleaseId = releaseId };
            _azureBlobStorageClientMockBuilder.WhereDeleteBlobFailsFor(releaseId.ToString());

            // Act
            var response = await GetSut().RemoveReleaseDocument(request);

            // Assert
            Assert.False(response.Success);
        }
    }

    public class RemoveAllReleaseDocumentsTests : ReleaseDocumentRemoverTests
    {
        [Fact]
        public async Task ShouldDeleteBlobsFromExpectedStorageContainer()
        {
            // Arrange
            var releaseDocumentsContainerName = "release-documents-container-name";
            _appOptions = new AppOptions
            {
                SearchStorageConnectionString = "azure storage connection string",
                ReleaseDocumentsContainerName = releaseDocumentsContainerName,
            };
            var sut = GetSut();

            // Act
            await sut.RemoveAllReleaseDocuments();

            // Assert
            _azureBlobStorageClientMockBuilder.Assert.AllBlobsWereDeleted(releaseDocumentsContainerName);
        }
    }
}
