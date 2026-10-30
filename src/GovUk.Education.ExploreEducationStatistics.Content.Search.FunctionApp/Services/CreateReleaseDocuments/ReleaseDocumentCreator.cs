using System.Net.Mime;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using Microsoft.Extensions.Options;
using Blob = GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateReleaseDocuments;

/// <summary>
/// We provide a searchable version of a Release for the Azure AI Search Indexers to ingest.
/// The contents of a Release along with its metadata are retrieved from the ContentAPI and uploaded
/// to Azure Blob storage
/// </summary>
internal class ReleaseDocumentCreator(
    IContentApiClient contentApiClient,
    IAzureBlobStorageClient azureBlobStorageClient,
    IOptions<AppOptions> appOptions
) : IReleaseDocumentCreator
{
    public async Task<CreatePublicationLatestReleaseDocumentResponse> CreatePublicationLatestReleaseDocument(
        CreatePublicationLatestReleaseDocumentRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var releaseDocument = await contentApiClient.GetPublicationLatestReleaseSearchableDocument(
            request.PublicationSlug,
            cancellationToken
        );

        var blobName = releaseDocument.ReleaseId.ToString();
        await azureBlobStorageClient.UploadBlob(
            containerName: appOptions.Value.ReleaseDocumentsContainerName,
            blobName: blobName,
            blob: new Blob(releaseDocument.HtmlContent, releaseDocument.BuildMetadata()),
            contentType: MediaTypeNames.Text.Html,
            cancellationToken: cancellationToken
        );

        return new CreatePublicationLatestReleaseDocumentResponse
        {
            PublicationSlug = request.PublicationSlug,
            ReleaseId = releaseDocument.ReleaseId,
            ReleaseSlug = releaseDocument.ReleaseSlug,
            ReleaseVersionId = releaseDocument.ReleaseVersionId,
            BlobName = blobName,
        };
    }
}
