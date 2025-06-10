using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CheckSearchableDocuments;

public interface IBlobNameLister
{
    Task<IList<string>> ListBlobsInContainer(CancellationToken cancellationToken = default);
}

public class BlobNameLister(Func<IAzureBlobStorageClient> azureBlobStorageClientFactory, IOptions<AppOptions> appOptions) : IBlobNameLister
{
    public async Task<IList<string>> ListBlobsInContainer(CancellationToken cancellationToken = default)
    {
        // Get a list of all blobs
        var blobStorageClient = azureBlobStorageClientFactory();
        var blobNames = await blobStorageClient.ListBlobsInContainer(
            appOptions.Value.SearchableDocumentsContainerName,
            cancellationToken:cancellationToken);
        return blobNames;
    }
}
