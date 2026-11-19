using System.Net.Mime;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateDataSetDocuments;
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

    private readonly AppOptions _appOptions = new() { DataSetDocumentsContainerName = DataSetDocumentsContainerName };

    private DataSetDocumentCreator GetSut() =>
        new(
            _contentApiClientMockBuilder.Build(),
            _azureBlobStorageClientMockBuilder.Build(),
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

    public class CreateReleaseDataSetDocumentsTests : DataSetDocumentCreatorTests
    {
        private readonly CreateReleaseDataSetDocumentsRequest _request = new()
        {
            PublicationId = PublicationId,
            PublicationSlug = PublicationSlug,
            ReleaseId = ReleaseId,
            ReleaseSlug = ReleaseSlug,
        };

        public CreateReleaseDataSetDocumentsTests()
        {
            _contentApiClientMockBuilder.WhereReleaseHasDataSetDocuments(DataSetDocument1, DataSetDocument2);
        }

        [Fact]
        public async Task ShouldRequestDataSetDocumentsOfReleaseFromContentApi()
        {
            await GetSut().CreateReleaseDataSetDocuments(_request);

            _contentApiClientMockBuilder.Assert.DataSetDocumentsRequestedForRelease(PublicationSlug, ReleaseSlug);
        }

        [Fact]
        public async Task ShouldUploadEachDataSetDocumentAsJsonWithoutMetadata()
        {
            await GetSut().CreateReleaseDataSetDocuments(_request);

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
        public async Task ShouldReturnKeysOfUploadedDataSetDocuments()
        {
            var response = await GetSut().CreateReleaseDataSetDocuments(_request);

            Assert.Equal([DataSetDocument1.GetKey(), DataSetDocument2.GetKey()], response.Keys);
        }

        [Fact]
        public async Task WhenReleaseHasNoDataSetDocuments_ShouldUploadNothingAndReturnNoKeys()
        {
            _contentApiClientMockBuilder.WhereReleaseHasDataSetDocuments();

            var response = await GetSut().CreateReleaseDataSetDocuments(_request);

            _azureBlobStorageClientMockBuilder.Assert.NoBlobsUploaded();
            Assert.Empty(response.Keys);
        }

        [Fact]
        public async Task WhenDataSetDocumentBelongsToAnotherRelease_ShouldThrowWithoutUploading()
        {
            _contentApiClientMockBuilder.WhereReleaseHasDataSetDocuments(DataSetDocument1, OtherReleaseDataSetDocument);

            await Assert.ThrowsAsync<InvalidOperationException>(() => GetSut().CreateReleaseDataSetDocuments(_request));

            _azureBlobStorageClientMockBuilder.Assert.NoBlobsUploaded();
        }
    }

    public class CreatePublicationDataSetDocumentsTests : DataSetDocumentCreatorTests
    {
        private readonly CreatePublicationDataSetDocumentsRequest _request = new()
        {
            PublicationId = PublicationId,
            PublicationSlug = PublicationSlug,
        };

        public CreatePublicationDataSetDocumentsTests()
        {
            _contentApiClientMockBuilder.WherePublicationHasDataSetDocuments(
                DataSetDocument1,
                OtherReleaseDataSetDocument
            );
        }

        [Fact]
        public async Task ShouldRequestDataSetDocumentsOfPublicationFromContentApi()
        {
            await GetSut().CreatePublicationDataSetDocuments(_request);

            _contentApiClientMockBuilder.Assert.DataSetDocumentsRequestedForPublication(PublicationSlug);
        }

        [Fact]
        public async Task ShouldUploadDataSetDocumentsOfEveryRelease()
        {
            await GetSut().CreatePublicationDataSetDocuments(_request);

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
        public async Task ShouldReturnKeysOfUploadedDataSetDocuments()
        {
            var response = await GetSut().CreatePublicationDataSetDocuments(_request);

            Assert.Equal([DataSetDocument1.GetKey(), OtherReleaseDataSetDocument.GetKey()], response.Keys);
        }

        [Fact]
        public async Task WhenPublicationHasNoDataSetDocuments_ShouldUploadNothingAndReturnNoKeys()
        {
            _contentApiClientMockBuilder.WherePublicationHasDataSetDocuments();

            var response = await GetSut().CreatePublicationDataSetDocuments(_request);

            _azureBlobStorageClientMockBuilder.Assert.NoBlobsUploaded();
            Assert.Empty(response.Keys);
        }

        [Fact]
        public async Task WhenDataSetDocumentBelongsToAnotherPublication_ShouldThrowWithoutUploading()
        {
            _contentApiClientMockBuilder.WherePublicationHasDataSetDocuments(
                DataSetDocument1,
                NewDataSetDocument(Guid.NewGuid(), Guid.NewGuid())
            );

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                GetSut().CreatePublicationDataSetDocuments(_request)
            );

            _azureBlobStorageClientMockBuilder.Assert.NoBlobsUploaded();
        }
    }
}
