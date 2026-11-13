using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveDataSetDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Services.RemoveDataSetDocuments;

public class DataSetDocumentRemoverTests
{
    private const string DataSetDocumentsContainerName = "data-set-documents-container-name";

    private static readonly Guid PublicationId = Guid.NewGuid();
    private static readonly Guid ReleaseId = Guid.NewGuid();
    private static readonly Guid OtherReleaseId = Guid.NewGuid();

    private static readonly DataSetDocumentKey Key1 = NewKey(PublicationId, ReleaseId);
    private static readonly DataSetDocumentKey Key2 = NewKey(PublicationId, ReleaseId);
    private static readonly DataSetDocumentKey OtherReleaseKey = NewKey(PublicationId, OtherReleaseId);
    private static readonly DataSetDocumentKey OtherPublicationKey = NewKey(Guid.NewGuid(), Guid.NewGuid());

    private readonly AzureBlobStorageClientMockBuilder _azureBlobStorageClientMockBuilder = new();

    private readonly AppOptions _appOptions = new() { DataSetDocumentsContainerName = DataSetDocumentsContainerName };

    public DataSetDocumentRemoverTests()
    {
        _azureBlobStorageClientMockBuilder.WhereContainerHasBlobs(
            Key1.BlobName,
            Key2.BlobName,
            OtherReleaseKey.BlobName,
            OtherPublicationKey.BlobName
        );
    }

    private DataSetDocumentRemover GetSut() =>
        new(_azureBlobStorageClientMockBuilder.Build(), Microsoft.Extensions.Options.Options.Create(_appOptions));

    private static DataSetDocumentKey NewKey(Guid publicationId, Guid releaseId) =>
        new()
        {
            PublicationId = publicationId,
            ReleaseId = releaseId,
            FileId = Guid.NewGuid(),
        };

    public class BasicTests : DataSetDocumentRemoverTests
    {
        [Fact]
        public void CanInstantiateSut() => Assert.NotNull(GetSut());
    }

    public class RemovePublicationDataSetDocumentsTests : DataSetDocumentRemoverTests
    {
        private readonly RemoveDataSetDocumentsRequest _request = new() { PublicationId = PublicationId };

        [Fact]
        public async Task ShouldListBlobsWithPublicationPrefixInDataSetDocumentsContainer()
        {
            await GetSut().RemoveDataSetDocuments(_request);

            _azureBlobStorageClientMockBuilder.Assert.BlobsWereListed(
                containerName: DataSetDocumentsContainerName,
                prefix: $"{PublicationId}/"
            );
        }

        [Fact]
        public async Task ShouldDeleteBlobsOfEveryReleaseOfPublication()
        {
            var response = await GetSut().RemoveDataSetDocuments(_request);

            _azureBlobStorageClientMockBuilder.Assert.BlobWasDeleted(DataSetDocumentsContainerName, Key1.BlobName);
            _azureBlobStorageClientMockBuilder.Assert.BlobWasDeleted(DataSetDocumentsContainerName, Key2.BlobName);
            _azureBlobStorageClientMockBuilder.Assert.BlobWasDeleted(
                DataSetDocumentsContainerName,
                OtherReleaseKey.BlobName
            );
            _azureBlobStorageClientMockBuilder.Assert.BlobWasNotDeleted(OtherPublicationKey.BlobName);
            Assert.Equal([Key1.BlobName, Key2.BlobName, OtherReleaseKey.BlobName], response.RemovedBlobNames);
        }

        [Fact]
        public async Task ShouldNotDeleteExceptedBlobs()
        {
            var response = await GetSut().RemoveDataSetDocuments(_request with { Except = [Key1, OtherReleaseKey] });

            _azureBlobStorageClientMockBuilder.Assert.BlobWasNotDeleted(Key1.BlobName);
            _azureBlobStorageClientMockBuilder.Assert.BlobWasNotDeleted(OtherReleaseKey.BlobName);
            Assert.Equal([Key2.BlobName], response.RemovedBlobNames);
        }

        [Fact]
        public async Task WhenPublicationHasNoBlobs_ShouldNotDeleteBlobs()
        {
            var response = await GetSut().RemoveDataSetDocuments(new() { PublicationId = Guid.NewGuid() });

            _azureBlobStorageClientMockBuilder.Assert.NoBlobsDeleted();
            Assert.Empty(response.RemovedBlobNames);
        }
    }

    public class RemoveReleaseDataSetDocumentsTests : DataSetDocumentRemoverTests
    {
        private readonly RemoveDataSetDocumentsRequest _request = new()
        {
            PublicationId = PublicationId,
            ReleaseId = ReleaseId,
        };

        [Fact]
        public async Task ShouldListBlobsWithReleasePrefixInDataSetDocumentsContainer()
        {
            await GetSut().RemoveDataSetDocuments(_request);

            _azureBlobStorageClientMockBuilder.Assert.BlobsWereListed(
                containerName: DataSetDocumentsContainerName,
                prefix: $"{PublicationId}/{ReleaseId}/"
            );
        }

        [Fact]
        public async Task ShouldDeleteBlobsOfReleaseOnly()
        {
            var response = await GetSut().RemoveDataSetDocuments(_request);

            _azureBlobStorageClientMockBuilder.Assert.BlobWasDeleted(DataSetDocumentsContainerName, Key1.BlobName);
            _azureBlobStorageClientMockBuilder.Assert.BlobWasDeleted(DataSetDocumentsContainerName, Key2.BlobName);
            _azureBlobStorageClientMockBuilder.Assert.BlobWasNotDeleted(OtherReleaseKey.BlobName);
            _azureBlobStorageClientMockBuilder.Assert.BlobWasNotDeleted(OtherPublicationKey.BlobName);
            Assert.Equal([Key1.BlobName, Key2.BlobName], response.RemovedBlobNames);
        }

        [Fact]
        public async Task ShouldNotDeleteExceptedBlobs()
        {
            var response = await GetSut().RemoveDataSetDocuments(_request with { Except = [Key2] });

            _azureBlobStorageClientMockBuilder.Assert.BlobWasNotDeleted(Key2.BlobName);
            Assert.Equal([Key1.BlobName], response.RemovedBlobNames);
        }
    }

    public class RemoveDataSetDocumentTests : DataSetDocumentRemoverTests
    {
        private readonly RemoveDataSetDocumentsRequest _request = new()
        {
            PublicationId = PublicationId,
            ReleaseId = ReleaseId,
            FileId = Key1.FileId,
        };

        [Fact]
        public async Task ShouldDeleteBlobOfDataFileOnly()
        {
            var response = await GetSut().RemoveDataSetDocuments(_request);

            _azureBlobStorageClientMockBuilder.Assert.BlobWasDeleted(DataSetDocumentsContainerName, Key1.BlobName);
            _azureBlobStorageClientMockBuilder.Assert.BlobWasNotDeleted(Key2.BlobName);
            Assert.Equal([Key1.BlobName], response.RemovedBlobNames);
        }

        [Fact]
        public async Task WhenBlobDoesNotExist_ShouldReturnNoRemovedBlobNames()
        {
            _azureBlobStorageClientMockBuilder.WhereDeleteBlobFailsFor(Key1.BlobName);

            var response = await GetSut().RemoveDataSetDocuments(_request);

            Assert.Empty(response.RemovedBlobNames);
        }

        [Fact]
        public async Task WhenReleaseIdNotSpecified_ShouldThrow()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                GetSut().RemoveDataSetDocuments(_request with { ReleaseId = null })
            );

            _azureBlobStorageClientMockBuilder.Assert.NoBlobsDeleted();
        }
    }

    public class RemoveAllDataSetDocumentsTests : DataSetDocumentRemoverTests
    {
        [Fact]
        public async Task ShouldDeleteAllBlobsInDataSetDocumentsContainer()
        {
            await GetSut().RemoveAllDataSetDocuments();

            _azureBlobStorageClientMockBuilder.Assert.AllBlobsWereDeleted(DataSetDocumentsContainerName);
        }

        [Fact]
        public async Task ShouldReturnNamesOfBlobsOfEveryPublication()
        {
            var response = await GetSut().RemoveAllDataSetDocuments();

            _azureBlobStorageClientMockBuilder.Assert.BlobsWereListed(
                containerName: DataSetDocumentsContainerName,
                prefix: null
            );
            Assert.Equal(
                [Key1.BlobName, Key2.BlobName, OtherReleaseKey.BlobName, OtherPublicationKey.BlobName],
                response.RemovedBlobNames
            );
        }
    }
}
