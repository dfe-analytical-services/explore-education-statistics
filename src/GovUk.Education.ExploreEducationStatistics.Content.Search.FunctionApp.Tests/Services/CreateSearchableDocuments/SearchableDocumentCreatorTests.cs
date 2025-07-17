using System.Net.Mime;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateSearchableDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using Blob = GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Services.CreateSearchableDocuments;

public class SearchableDocumentCreatorTests
{
    private readonly ContentApiClientMockBuilder _contentApiClientMockBuilder = new();
    private readonly AzureBlobStorageClientMockBuilder _azureBlobStorageClientMockBuilder = new();
    private AppOptions _appOptions = new();
    
    private SearchableDocumentCreator GetSut() => new(
        _contentApiClientMockBuilder.Build(), 
        _azureBlobStorageClientMockBuilder.Build(), 
        Microsoft.Extensions.Options.Options.Create(_appOptions));

    public class BasicTests : SearchableDocumentCreatorTests
    {
        [Fact]
        public void Can_instantiate_Sut() => Assert.NotNull(GetSut());
    }
    
    public class CreatingSearchableDocument : SearchableDocumentCreatorTests
    {
        [Fact]
        public async Task Should_load_searchable_document_from_ContentApi()
        {
            // ARRANGE
            var sut = GetSut();
            
            // ACT
            await sut.CreatePublicationLatestReleaseSearchableDocument(new CreatePublicationLatestReleaseSearchableDocumentRequest { PublicationSlug = "publication-slug" });
            
            // ASSERT
            _contentApiClientMockBuilder.Assert.ContentWasLoadedFor("publication-slug");
        }

        [Fact]
        public async Task Should_upload_searchable_document_to_expected_AzureBlobStorage_container()
        {
            // ARRANGE
            _appOptions = new AppOptions()
            {
                SearchStorageConnectionString = "azure storage connection string",
                SearchableDocumentsContainerName = "searchable-documents-container-name"
            };

            var sut = GetSut();
            
            // ACT
            await sut.CreatePublicationLatestReleaseSearchableDocument(new CreatePublicationLatestReleaseSearchableDocumentRequest { PublicationSlug = "publication-slug" });
            
            // ASSERT
            _azureBlobStorageClientMockBuilder.Assert.BlobWasUploaded(containerName: _appOptions.SearchableDocumentsContainerName);
        }
        
        [Fact]
        public async Task Should_upload_searchable_document_to_blob_with_releaseId_as_blob_name()
        {
            // ARRANGE
            var releaseId = new Guid("11223344-5566-7788-9900-123456789abc");
            _contentApiClientMockBuilder.WhereReleaseSearchViewModelIs(builder => builder.WithReleaseId(releaseId));
            
            var sut = GetSut();
            
            // ACT
            await sut.CreatePublicationLatestReleaseSearchableDocument(new CreatePublicationLatestReleaseSearchableDocumentRequest { PublicationSlug = "publication-slug" });
            
            // ASSERT
            _azureBlobStorageClientMockBuilder.Assert.BlobWasUploaded(blobName: releaseId.ToString());
        }

        [Fact]
        public async Task Should_upload_searchable_document_blob()
        {
            // ARRANGE
            var releaseSearchableDocument = new ReleaseSearchableDocumentBuilder().Build();
            _contentApiClientMockBuilder.WhereReleaseSearchViewModelIs(releaseSearchableDocument);
            var sut = GetSut();

            // ACT
            await sut.CreatePublicationLatestReleaseSearchableDocument(
                new CreatePublicationLatestReleaseSearchableDocumentRequest { PublicationSlug = "publication-slug" });

            // ASSERT
            var expected = new Blob(releaseSearchableDocument.HtmlContent, releaseSearchableDocument.BuildMetadata());
            _azureBlobStorageClientMockBuilder.Assert.BlobWasUploaded(
                contentType: MediaTypeNames.Text.Html,
                whereBlob: blob => blob == expected);
        }

        [Fact]
        public async Task Should_return_information_in_response()
        {
            // ARRANGE
            var releaseSearchableDocument = new ReleaseSearchableDocumentBuilder().Build();
            _contentApiClientMockBuilder.WhereReleaseSearchViewModelIs(releaseSearchableDocument);
            var sut = GetSut();
            
            // ACT
            var response = await sut.CreatePublicationLatestReleaseSearchableDocument(new CreatePublicationLatestReleaseSearchableDocumentRequest { PublicationSlug = "publication-slug" });

            // ASSERT
            Assert.Equal(releaseSearchableDocument.ReleaseVersionId, response.ReleaseVersionId);
            Assert.Equal(releaseSearchableDocument.ReleaseId.ToString(), response.BlobName);
        }
    }
    
}
