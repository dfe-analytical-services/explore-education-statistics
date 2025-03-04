using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using Microsoft.Extensions.Options;
using Blob = GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;

/// <summary>
/// We provide a searchable version of a Release for the Azure AI Search Indexers to ingest.
/// The contents of a Release along with its metadata are retrieved from the ContentAPI and uploaded
/// to Azure Blob storage 
/// </summary>
internal class SearchableDocumentCreator(
    IContentApiClient contentApiClient, 
    IAzureBlobStorageClient azureBlobStorageClient, 
    IOptions<AppOptions> appOptions) : ISearchableDocumentCreator
{
    public async Task<CreatePublicationLatestReleaseSearchableDocumentResponse> CreatePublicationLatestReleaseSearchableDocument(
        CreatePublicationLatestReleaseSearchableDocumentRequest request, 
        CancellationToken cancellationToken = default)
    {
        var publicationSlug = request.PublicationSlug;
        var getResponse = await contentApiClient.GetPublicationLatestReleaseSearchableDocumentAsync(new GetRequest(publicationSlug), cancellationToken);
        return getResponse switch
        {
            GetResponse.Successful msg => await Upload(msg.ReleaseSearchableDocument, cancellationToken),
            GetResponse.NotFound => new CreatePublicationLatestReleaseSearchableDocumentResponse.NotFound(),
            GetResponse.Error msg => new CreatePublicationLatestReleaseSearchableDocumentResponse.Error(msg.ErrorMessage),
            _ => throw new ArgumentOutOfRangeException(nameof(getResponse), getResponse, null)
        };
    }

    private async Task<CreatePublicationLatestReleaseSearchableDocumentResponse> Upload(
        ReleaseSearchableDocument searchableDocument, 
        CancellationToken cancellationToken)
    {
        var blobName = searchableDocument.ReleaseId.ToString();
        var uploadBlobRequest = new UploadBlobRequest(
            appOptions.Value.SearchableDocumentsContainerName,
            blobName,
            new Blob(searchableDocument.HtmlContent, searchableDocument.BuildMetadata()));
            
        var uploadBlobResponse = await azureBlobStorageClient.UploadBlob(uploadBlobRequest, cancellationToken);

        return uploadBlobResponse switch
        {
            UploadBlobResponse.Success =>  new CreatePublicationLatestReleaseSearchableDocumentResponse.Success(searchableDocument.ReleaseVersionId, blobName),
            UploadBlobResponse.Error uploadMsg => new CreatePublicationLatestReleaseSearchableDocumentResponse.Error(uploadMsg.ErrorMessage),
            _ => throw new ArgumentOutOfRangeException(nameof(uploadBlobResponse), uploadBlobResponse, null)
        };
    }

}

