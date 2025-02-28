using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using Blob = GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Services;

public class SearchableDocumentCreatorTests
{
    private readonly ContentApiClientBuilder _contentApiClientBuilder = new();
    private readonly AzureBlobStorageClientBuilder _azureBlobStorageClientBuilder = new();
    private AppOptions _appOptions = new();
    
    private SearchableDocumentCreator GetSut() => new(
        _contentApiClientBuilder.Build(), 
        _azureBlobStorageClientBuilder.Build(), 
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
            _contentApiClientBuilder.Assert.ContentWasLoadedFor("publication-slug");
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
            _azureBlobStorageClientBuilder.Assert.BlobWasUploaded(containerName: _appOptions.SearchableDocumentsContainerName);
        }
        
        [Fact]
        public async Task Should_upload_searchable_document_to_blob_with_releaseId_as_blob_name()
        {
            // ARRANGE
            var releaseId = new Guid("11223344-5566-7788-9900-123456789abc");
            _contentApiClientBuilder.WhereReleaseSearchViewModelIs(builder => builder.WithReleaseId(releaseId));
            
            var sut = GetSut();
            
            // ACT
            await sut.CreatePublicationLatestReleaseSearchableDocument(new CreatePublicationLatestReleaseSearchableDocumentRequest { PublicationSlug = "publication-slug" });
            
            // ASSERT
            _azureBlobStorageClientBuilder.Assert.BlobWasUploaded(blobName: releaseId.ToString());
        }
        
        [Fact]
        public async Task Should_upload_searchable_document_blob()
        {
            // ARRANGE
            var releaseSearchViewModel = new ReleaseSearchViewModelBuilder().Build();
            _contentApiClientBuilder.WhereReleaseSearchViewModelIs(releaseSearchViewModel);
            var sut = GetSut();
            
            // ACT
            await sut.CreatePublicationLatestReleaseSearchableDocument(new CreatePublicationLatestReleaseSearchableDocumentRequest { PublicationSlug = "publication-slug" });
            
            // ASSERT
            var expected = new Blob(releaseSearchViewModel.HtmlContent, releaseSearchViewModel.BuildMetadata());
            _azureBlobStorageClientBuilder.Assert.BlobWasUploaded(whereBlob: blob => blob == expected);
        }
        
        [Fact]
        public async Task Should_return_information_in_response()
        {
            // ARRANGE
            var releaseSearchViewModel = new ReleaseSearchViewModelBuilder().Build();
            _contentApiClientBuilder.WhereReleaseSearchViewModelIs(releaseSearchViewModel);
            var sut = GetSut();
            
            // ACT
            var response = await sut.CreatePublicationLatestReleaseSearchableDocument(new CreatePublicationLatestReleaseSearchableDocumentRequest { PublicationSlug = "publication-slug" });

            // ASSERT
            Assert.Equal(releaseSearchViewModel.ReleaseVersionId, response.ReleaseVersionId);
            Assert.Equal(releaseSearchViewModel.ReleaseId.ToString(), response.BlobName);
            Assert.Equal("publication-slug", response.PublicationSlug);
        }
    }
    
}
