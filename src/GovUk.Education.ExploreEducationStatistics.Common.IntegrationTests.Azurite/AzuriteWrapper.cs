using Azure.Storage.Blobs;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.Azurite;

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
