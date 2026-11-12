using System.Net.Mime;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateDataSetDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveDataSetDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Services.CreateDataSetDocuments;

public class DataSetDocumentCreatorTests
{
    private const string DataSetDocumentsContainerName = "data-set-documents-container-name";
    private const string PublicationSlug = "publication-slug";
    private const string ReleaseSlug = "release-slug";

    private static readonly Guid PublicationId = Guid.NewGuid();
    private static readonly Guid ReleaseId = Guid.NewGuid();
    private static readonly Guid OtherReleaseId = Guid.NewGuid();

    private static readonly DataSetDocument DataSetDocument1 = NewDataSetDocument(PublicationId, ReleaseId);
    private static readonly DataSetDocument DataSetDocument2 = NewDataSetDocument(PublicationId, ReleaseId);
    private static readonly DataSetDocument OtherReleaseDataSetDocument = NewDataSetDocument(
        PublicationId,
        OtherReleaseId
    );

    private readonly ContentApiClientMockBuilder _contentApiClientMockBuilder = new();
    private readonly AzureBlobStorageClientMockBuilder _azureBlobStorageClientMockBuilder = new();
    private readonly DataSetDocumentRemoverMockBuilder _dataSetDocumentRemoverMockBuilder = new();

    private readonly AppOptions _appOptions = new() { DataSetDocumentsContainerName = DataSetDocumentsContainerName };

    private DataSetDocumentCreator GetSut() =>
        new(
            _contentApiClientMockBuilder.Build(),
            _azureBlobStorageClientMockBuilder.Build(),
            _dataSetDocumentRemoverMockBuilder.Build(),
            Microsoft.Extensions.Options.Options.Create(_appOptions)
        );

    private static DataSetDocument NewDataSetDocument(Guid publicationId, Guid releaseId) =>
        new DataSetDocumentBuilder()
            .WithPublicationId(publicationId)
            .WithReleaseId(releaseId)
            .WithFileId(Guid.NewGuid())
            .Build();

    public class BasicTests : DataSetDocumentCreatorTests
    {
        [Fact]
        public void CanInstantiateSut() => Assert.NotNull(GetSut());
    }

    public class RefreshReleaseDataSetDocumentsTests : DataSetDocumentCreatorTests
    {
        private readonly RefreshReleaseDataSetDocumentsRequest _request = new()
        {
            PublicationId = PublicationId,
            PublicationSlug = PublicationSlug,
            ReleaseId = ReleaseId,
            ReleaseSlug = ReleaseSlug,
        };

        public RefreshReleaseDataSetDocumentsTests()
        {
            _contentApiClientMockBuilder.WhereReleaseHasDataSetDocuments(DataSetDocument1, DataSetDocument2);
        }

        [Fact]
        public async Task ShouldRequestDataSetDocumentsOfReleaseFromContentApi()
        {
            await GetSut().RefreshReleaseDataSetDocuments(_request);

            _contentApiClientMockBuilder.Assert.DataSetDocumentsRequestedForRelease(PublicationSlug, ReleaseSlug);
        }

        [Fact]
        public async Task ShouldUploadEachDataSetDocumentAsJsonWithoutMetadata()
        {
            await GetSut().RefreshReleaseDataSetDocuments(_request);

            _azureBlobStorageClientMockBuilder.Assert.BlobWasUploaded(
                containerName: DataSetDocumentsContainerName,
                blobName: $"{PublicationId}/{ReleaseId}/{DataSetDocument1.FileId}.json",
                contentType: MediaTypeNames.Application.Json,
                whereBlob: blob => blob.Contents == DataSetDocument1.ToJson() && blob.Metadata is null
            );
            _azureBlobStorageClientMockBuilder.Assert.BlobWasUploaded(
                containerName: DataSetDocumentsContainerName,
                blobName: $"{PublicationId}/{ReleaseId}/{DataSetDocument2.FileId}.json",
                contentType: MediaTypeNames.Application.Json,
                whereBlob: blob => blob.Contents == DataSetDocument2.ToJson() && blob.Metadata is null
            );
        }

        [Fact]
        public async Task ShouldRemoveOtherDataSetDocumentsOfRelease()
        {
            await GetSut().RefreshReleaseDataSetDocuments(_request);

            _dataSetDocumentRemoverMockBuilder.Assert.RemoveDataSetDocumentsCalledFor(request =>
                request.PublicationId == PublicationId
                && request.ReleaseId == ReleaseId
                && request.FileId == null
                && request.Except.SequenceEqual([DataSetDocument1.GetKey(), DataSetDocument2.GetKey()])
            );
        }

        [Fact]
        public async Task WhenReleaseHasNoDataSetDocuments_ShouldRemoveAllDataSetDocumentsOfRelease()
        {
            _contentApiClientMockBuilder.WhereReleaseHasDataSetDocuments();

            await GetSut().RefreshReleaseDataSetDocuments(_request);

            _azureBlobStorageClientMockBuilder.Assert.NoBlobsUploaded();
            _dataSetDocumentRemoverMockBuilder.Assert.RemoveDataSetDocumentsCalledFor(request =>
                request.PublicationId == PublicationId
                && request.ReleaseId == ReleaseId
                && request.FileId == null
                && request.Except.Count == 0
            );
        }

        [Fact]
        public async Task ShouldReturnUploadedAndRemovedBlobNames()
        {
            _dataSetDocumentRemoverMockBuilder.WhereResponseIs(new RemoveDataSetDocumentsResponse(["removed-blob"]));

            var response = await GetSut().RefreshReleaseDataSetDocuments(_request);

            Assert.Equal(
                [DataSetDocument1.GetKey().BlobName, DataSetDocument2.GetKey().BlobName],
                response.UploadedBlobNames
            );
            Assert.Equal(["removed-blob"], response.RemovedBlobNames);
        }

        [Fact]
        public async Task WhenDataSetDocumentBelongsToAnotherRelease_ShouldThrowWithoutChangingBlobs()
        {
            _contentApiClientMockBuilder.WhereReleaseHasDataSetDocuments(DataSetDocument1, OtherReleaseDataSetDocument);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                GetSut().RefreshReleaseDataSetDocuments(_request)
            );

            _azureBlobStorageClientMockBuilder.Assert.NoBlobsUploaded();
            _dataSetDocumentRemoverMockBuilder.Assert.RemoveDataSetDocumentsNotCalled();
        }
    }

    public class RefreshPublicationDataSetDocumentsTests : DataSetDocumentCreatorTests
    {
        private readonly RefreshPublicationDataSetDocumentsRequest _request = new()
        {
            PublicationId = PublicationId,
            PublicationSlug = PublicationSlug,
        };

        public RefreshPublicationDataSetDocumentsTests()
        {
            _contentApiClientMockBuilder.WherePublicationHasDataSetDocuments(
                DataSetDocument1,
                OtherReleaseDataSetDocument
            );
        }

        [Fact]
        public async Task ShouldRequestDataSetDocumentsOfPublicationFromContentApi()
        {
            await GetSut().RefreshPublicationDataSetDocuments(_request);

            _contentApiClientMockBuilder.Assert.DataSetDocumentsRequestedForPublication(PublicationSlug);
        }

        [Fact]
        public async Task ShouldUploadDataSetDocumentsOfEveryRelease()
        {
            await GetSut().RefreshPublicationDataSetDocuments(_request);

            _azureBlobStorageClientMockBuilder.Assert.BlobWasUploaded(
                containerName: DataSetDocumentsContainerName,
                blobName: $"{PublicationId}/{ReleaseId}/{DataSetDocument1.FileId}.json",
                contentType: MediaTypeNames.Application.Json,
                whereBlob: blob => blob.Contents == DataSetDocument1.ToJson() && blob.Metadata is null
            );
            _azureBlobStorageClientMockBuilder.Assert.BlobWasUploaded(
                containerName: DataSetDocumentsContainerName,
                blobName: $"{PublicationId}/{OtherReleaseId}/{OtherReleaseDataSetDocument.FileId}.json",
                contentType: MediaTypeNames.Application.Json,
                whereBlob: blob => blob.Contents == OtherReleaseDataSetDocument.ToJson() && blob.Metadata is null
            );
        }

        [Fact]
        public async Task ShouldRemoveOtherDataSetDocumentsOfPublication()
        {
            await GetSut().RefreshPublicationDataSetDocuments(_request);

            _dataSetDocumentRemoverMockBuilder.Assert.RemoveDataSetDocumentsCalledFor(request =>
                request.PublicationId == PublicationId
                && request.ReleaseId == null
                && request.FileId == null
                && request.Except.SequenceEqual([DataSetDocument1.GetKey(), OtherReleaseDataSetDocument.GetKey()])
            );
        }

        [Fact]
        public async Task WhenPublicationHasNoDataSetDocuments_ShouldRemoveAllDataSetDocumentsOfPublication()
        {
            _contentApiClientMockBuilder.WherePublicationHasDataSetDocuments();

            await GetSut().RefreshPublicationDataSetDocuments(_request);

            _azureBlobStorageClientMockBuilder.Assert.NoBlobsUploaded();
            _dataSetDocumentRemoverMockBuilder.Assert.RemoveDataSetDocumentsCalledFor(request =>
                request.PublicationId == PublicationId
                && request.ReleaseId == null
                && request.FileId == null
                && request.Except.Count == 0
            );
        }

        [Fact]
        public async Task ShouldReturnUploadedAndRemovedBlobNames()
        {
            _dataSetDocumentRemoverMockBuilder.WhereResponseIs(new RemoveDataSetDocumentsResponse(["removed-blob"]));

            var response = await GetSut().RefreshPublicationDataSetDocuments(_request);

            Assert.Equal(
                [DataSetDocument1.GetKey().BlobName, OtherReleaseDataSetDocument.GetKey().BlobName],
                response.UploadedBlobNames
            );
            Assert.Equal(["removed-blob"], response.RemovedBlobNames);
        }

        [Fact]
        public async Task WhenDataSetDocumentBelongsToAnotherPublication_ShouldThrowWithoutChangingBlobs()
        {
            _contentApiClientMockBuilder.WherePublicationHasDataSetDocuments(
                DataSetDocument1,
                NewDataSetDocument(Guid.NewGuid(), Guid.NewGuid())
            );

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                GetSut().RefreshPublicationDataSetDocuments(_request)
            );

            _azureBlobStorageClientMockBuilder.Assert.NoBlobsUploaded();
            _dataSetDocumentRemoverMockBuilder.Assert.RemoveDataSetDocumentsNotCalled();
        }
    }
}
