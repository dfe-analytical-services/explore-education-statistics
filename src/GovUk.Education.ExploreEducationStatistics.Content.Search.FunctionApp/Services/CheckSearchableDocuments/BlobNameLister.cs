using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CheckSearchableDocuments;

public class BlobNameLister(Func<IAzureBlobStorageClient> azureBlobStorageClientFactory) : IBlobNameLister
{
    public async Task<IList<string>> ListBlobsInContainer(
        string containerName,
        CancellationToken cancellationToken = default
    )
    {
        var blobStorageClient = azureBlobStorageClientFactory();
        return await blobStorageClient.ListBlobsInContainer(containerName, cancellationToken: cancellationToken);
    }
}
