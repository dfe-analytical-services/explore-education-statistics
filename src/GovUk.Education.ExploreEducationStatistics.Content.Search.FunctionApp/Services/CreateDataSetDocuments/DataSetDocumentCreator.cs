using System.Net.Mime;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveDataSetDocuments;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateDataSetDocuments;

/// <summary>
/// This service stores each data set document as a JSON blob, and removes the blobs of data set documents that no
/// longer exist.
/// </summary>
internal class DataSetDocumentCreator(
    IContentApiClient contentApiClient,
    IAzureBlobStorageClient azureBlobStorageClient,
    IDataSetDocumentRemover dataSetDocumentRemover,
    IOptions<AppOptions> appOptions
) : IDataSetDocumentCreator
{
    public async Task<RefreshDataSetDocumentsResponse> RefreshReleaseDataSetDocuments(
        RefreshReleaseDataSetDocumentsRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var dataSetDocuments = await contentApiClient.GetReleaseDataSetDocuments(
            request.PublicationSlug,
            request.ReleaseSlug,
            cancellationToken
        );

        // Guard against removing the documents of one release while uploading those of another
        if (
            dataSetDocuments.Any(dataSetDocument =>
                dataSetDocument.PublicationId != request.PublicationId || dataSetDocument.ReleaseId != request.ReleaseId
            )
        )
        {
            throw new InvalidOperationException(
                $"""
                The Data Set Documents for release "{request.ReleaseSlug}" under publication "{request.PublicationSlug}" do not all belong to release {request.ReleaseId} of publication {request.PublicationId}
                """
            );
        }

        var keys = await UploadDataSetDocuments(dataSetDocuments, cancellationToken);

        var removed = await dataSetDocumentRemover.RemoveDataSetDocuments(
            new RemoveDataSetDocumentsRequest
            {
                PublicationId = request.PublicationId,
                ReleaseId = request.ReleaseId,
                Except = keys,
            },
            cancellationToken
        );

        return BuildResponse(keys, removed);
    }

    public async Task<RefreshDataSetDocumentsResponse> RefreshPublicationDataSetDocuments(
        RefreshPublicationDataSetDocumentsRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var dataSetDocuments = await contentApiClient.GetPublicationDataSetDocuments(
            request.PublicationSlug,
            cancellationToken
        );

        // Guard against removing the documents of one publication while uploading those of another
        if (dataSetDocuments.Any(dataSetDocument => dataSetDocument.PublicationId != request.PublicationId))
        {
            throw new InvalidOperationException(
                $"""
                The Data Set Documents for publication "{request.PublicationSlug}" do not all belong to publication {request.PublicationId}
                """
            );
        }

        var keys = await UploadDataSetDocuments(dataSetDocuments, cancellationToken);

        var removed = await dataSetDocumentRemover.RemoveDataSetDocuments(
            new RemoveDataSetDocumentsRequest { PublicationId = request.PublicationId, Except = keys },
            cancellationToken
        );

        return BuildResponse(keys, removed);
    }

    private async Task<DataSetDocumentKey[]> UploadDataSetDocuments(
        DataSetDocument[] dataSetDocuments,
        CancellationToken cancellationToken
    )
    {
        await Task.WhenAll(
            dataSetDocuments.Select(dataSetDocument =>
                azureBlobStorageClient.UploadBlob(
                    containerName: appOptions.Value.DataSetDocumentsContainerName,
                    blobName: dataSetDocument.GetKey().BlobName,
                    blob: new Blob(dataSetDocument.ToJson()),
                    contentType: MediaTypeNames.Application.Json,
                    cancellationToken: cancellationToken
                )
            )
        );

        return [.. dataSetDocuments.Select(dataSetDocument => dataSetDocument.GetKey())];
    }

    private static RefreshDataSetDocumentsResponse BuildResponse(
        DataSetDocumentKey[] keys,
        RemoveDataSetDocumentsResponse removed
    ) => new([.. keys.Select(key => key.BlobName)], removed.RemovedBlobNames);
}
