using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using Microsoft.Extensions.Options;
using Blob = GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;

/// <summary>
/// We provide a searchable version of a Release for the Azure AI Search Indexers to ingest.
/// The contents of a Release along with its metadata are retrieved from the ContentAPI and uploaded
/// to Azure Blob storage 
/// </summary>
public class SearchableDocumentCreator(
    IContentApiClient contentApiClient, 
    IAzureBlobStorageClient azureBlobStorageClient, 
    IOptions<AppOptions> appOptions) : ISearchableDocumentCreator
{
    public async Task<CreatePublicationLatestReleaseSearchableDocumentResponse> CreatePublicationLatestReleaseSearchableDocument(
        CreatePublicationLatestReleaseSearchableDocumentRequest request, 
        CancellationToken cancellationToken = default)
    {
        var searchViewModel = await contentApiClient.GetPublicationLatestReleaseSearchViewModelAsync(request.PublicationSlug, cancellationToken);

        var blobName = searchViewModel.ReleaseId.ToString();
        await azureBlobStorageClient.UploadBlob(
            appOptions.Value.SearchableDocumentsContainerName, 
            blobName, 
            new Blob(searchViewModel.HtmlContent, searchViewModel.BuildMetadata()), 
            cancellationToken);

        return new CreatePublicationLatestReleaseSearchableDocumentResponse
        {
            PublicationSlug = request.PublicationSlug,
            ReleaseVersionId = searchViewModel.ReleaseVersionId,
            BlobName = blobName
        };
    }
}

