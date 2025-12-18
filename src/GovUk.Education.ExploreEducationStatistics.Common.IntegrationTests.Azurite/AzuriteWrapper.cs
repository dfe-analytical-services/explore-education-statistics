using Azure.Storage.Blobs;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.Azurite;

/// <summary>
/// A convenience class to allow operations such as test data cleardown to be performed
/// on an Azurite test container, without exposing the container itself to the calling code.
/// </summary>
public class AzuriteWrapper(Func<string> connectionString)
{
    public string GetConnectionString() => connectionString();

    public async Task ClearTestData()
    {
        var service = new BlobServiceClient(connectionString());

        await foreach (var containerItem in service.GetBlobContainersAsync())
        {
            var container = service.GetBlobContainerClient(containerItem.Name);
            await container.DeleteIfExistsAsync();
            await service.CreateBlobContainerAsync(containerItem.Name);
        }
    }
}
