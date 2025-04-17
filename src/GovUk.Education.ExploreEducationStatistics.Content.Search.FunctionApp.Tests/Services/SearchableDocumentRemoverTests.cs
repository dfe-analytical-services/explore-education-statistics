using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Services;

public class SearchableDocumentRemoverTests
{
    private readonly ContentApiClientMockBuilder _contentApiClientMockBuilder = new();
    private readonly AzureBlobStorageClientMockBuilder _azureBlobStorageClientMockBuilder = new();
    private AppOptions _appOptions = new();

    private SearchableDocumentRemover GetSut() => new(
        _contentApiClientMockBuilder.Build(),
        _azureBlobStorageClientMockBuilder.Build(),
        Microsoft.Extensions.Options.Options.Create(_appOptions));

    public class BasicTests : SearchableDocumentRemoverTests
    {
        [Fact]
        public void CanInstantiateSut() => Assert.NotNull(GetSut());
    }

    public class RemovePublicationSearchableDocumentsTests : SearchableDocumentRemoverTests
    {
        [Fact]
        public async Task ShouldRequestReleasesForPublicationFromContentApi()
        {
            var request = new RemovePublicationSearchableDocumentsRequest { PublicationSlug = "publication-slug" };

            await GetSut().RemovePublicationSearchableDocuments(request);

            _contentApiClientMockBuilder.Assert.ReleasesRequestedForPublication("publication-slug");
        }

        [Fact]
        public async Task ShouldNotDeleteBlobsIfNoReleasesFound()
        {
            ReleaseInfo[] releases = [];
            var request = new RemovePublicationSearchableDocumentsRequest { PublicationSlug = "publication-slug" };
            _contentApiClientMockBuilder.WherePublicationHasReleases(releases);

            await GetSut().RemovePublicationSearchableDocuments(request);

            _azureBlobStorageClientMockBuilder.Assert.NoBlobsDeleted();
        }

        [Fact]
        public async Task ShouldDeleteBlobsFromExpectedStorageContainer()
        {
            _appOptions = new AppOptions
            {
                SearchStorageConnectionString = "azure storage connection string",
                SearchableDocumentsContainerName = "searchable-documents-container-name"
            };

            ReleaseInfo[] releases =
            [
                new() { ReleaseId = Guid.NewGuid() }
            ];
            var request = new RemovePublicationSearchableDocumentsRequest { PublicationSlug = "publication-slug" };
            _contentApiClientMockBuilder.WherePublicationHasReleases(releases);

            await GetSut().RemovePublicationSearchableDocuments(request);

            _azureBlobStorageClientMockBuilder.Assert.BlobWasDeleted(
                containerName: _appOptions.SearchableDocumentsContainerName);
        }

        [Fact]
        public async Task ShouldDeleteBlobsForReleases()
        {
            ReleaseInfo[] releases =
            [
                new() { ReleaseId = Guid.NewGuid() },
                new() { ReleaseId = Guid.NewGuid() }
            ];
            var request = new RemovePublicationSearchableDocumentsRequest { PublicationSlug = "publication-slug" };
            _contentApiClientMockBuilder.WherePublicationHasReleases(releases);

            await GetSut().RemovePublicationSearchableDocuments(request);

            _azureBlobStorageClientMockBuilder.Assert.BlobWasDeleted(
                blobName: releases[0].ReleaseId.ToString());
            _azureBlobStorageClientMockBuilder.Assert.BlobWasDeleted(
                blobName: releases[1].ReleaseId.ToString());
        }

        [Fact]
        public async Task ShouldReturnDeletionResultInResponse()
        {
            ReleaseInfo[] releases =
            [
                new() { ReleaseId = Guid.NewGuid() },
                new() { ReleaseId = Guid.NewGuid() }
            ];
            var request = new RemovePublicationSearchableDocumentsRequest { PublicationSlug = "publication-slug" };
            _contentApiClientMockBuilder.WherePublicationHasReleases(releases);
            _azureBlobStorageClientMockBuilder.WhereDeleteBlobFailsFor(releases[0].ReleaseId.ToString());

            var response = await GetSut().RemovePublicationSearchableDocuments(request);

            Assert.Equal(2, response.ReleaseIdToDeletionResult.Count);
            Assert.False(response.ReleaseIdToDeletionResult[releases[0].ReleaseId]);
            Assert.True(response.ReleaseIdToDeletionResult[releases[1].ReleaseId]);
        }
    }
}
