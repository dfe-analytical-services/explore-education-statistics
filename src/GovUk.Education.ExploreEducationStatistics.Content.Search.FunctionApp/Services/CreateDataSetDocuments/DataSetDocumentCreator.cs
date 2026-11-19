using System.Net.Mime;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateDataSetDocuments;

/// <summary>
/// Stores the data set documents of a release or publication as JSON blobs.
/// </summary>
internal class DataSetDocumentCreator(
    IContentApiClient contentApiClient,
    IAzureBlobStorageClient azureBlobStorageClient,
    IOptions<AppOptions> appOptions
) : IDataSetDocumentCreator
{
    public async Task<CreateDataSetDocumentsResponse> CreateReleaseDataSetDocuments(
        CreateReleaseDataSetDocumentsRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var dataSetDocuments = await contentApiClient.GetReleaseDataSetDocuments(
            request.PublicationSlug,
            request.ReleaseSlug,
            cancellationToken
        );

        // Guard against uploading the documents of another release
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

        return new CreateDataSetDocumentsResponse(await UploadDataSetDocuments(dataSetDocuments, cancellationToken));
    }

    public async Task<CreateDataSetDocumentsResponse> CreatePublicationDataSetDocuments(
        CreatePublicationDataSetDocumentsRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var dataSetDocuments = await contentApiClient.GetPublicationDataSetDocuments(
            request.PublicationSlug,
            cancellationToken
        );

        // Guard against uploading the documents of another publication
        if (dataSetDocuments.Any(dataSetDocument => dataSetDocument.PublicationId != request.PublicationId))
        {
            throw new InvalidOperationException(
                $"""
                The Data Set Documents for publication "{request.PublicationSlug}" do not all belong to publication {request.PublicationId}
                """
            );
        }

        return new CreateDataSetDocumentsResponse(await UploadDataSetDocuments(dataSetDocuments, cancellationToken));
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
}
