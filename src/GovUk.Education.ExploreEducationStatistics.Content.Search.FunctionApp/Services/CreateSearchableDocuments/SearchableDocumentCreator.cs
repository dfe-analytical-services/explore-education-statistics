using System.Net.Mime;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using Microsoft.Extensions.Options;
using Blob = GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateSearchableDocuments;

/// <summary>
/// We provide a searchable version of a Release for the Azure AI Search Indexers to ingest.
/// The contents of a Release along with its metadata are retrieved from the ContentAPI and uploaded
/// to Azure Blob storage
/// </summary>
internal class SearchableDocumentCreator(
    IContentApiClient contentApiClient,
    IAzureBlobStorageClient azureBlobStorageClient,
    IOptions<AppOptions> appOptions
) : ISearchableDocumentCreator
{
    public async Task<CreatePublicationLatestReleaseSearchableDocumentResponse> CreatePublicationLatestReleaseSearchableDocument(
        CreatePublicationLatestReleaseSearchableDocumentRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var releaseSearchableDocument =
            await contentApiClient.GetPublicationLatestReleaseSearchableDocument(
                request.PublicationSlug,
                cancellationToken
            );

        var blobName = releaseSearchableDocument.ReleaseId.ToString();
        await azureBlobStorageClient.UploadBlob(
            containerName: appOptions.Value.SearchableDocumentsContainerName,
            blobName: blobName,
            blob: new Blob(
                releaseSearchableDocument.HtmlContent,
                releaseSearchableDocument.BuildMetadata()
            ),
            contentType: MediaTypeNames.Text.Html,
            cancellationToken: cancellationToken
        );

        return new CreatePublicationLatestReleaseSearchableDocumentResponse
        {
            PublicationSlug = request.PublicationSlug,
            ReleaseId = releaseSearchableDocument.ReleaseId,
            ReleaseSlug = releaseSearchableDocument.ReleaseSlug,
            ReleaseVersionId = releaseSearchableDocument.ReleaseVersionId,
            BlobName = blobName,
        };
    }
}
