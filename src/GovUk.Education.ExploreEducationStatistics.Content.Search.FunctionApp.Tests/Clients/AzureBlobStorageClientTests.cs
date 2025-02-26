using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;
using Blob = GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Clients;

public class AzureBlobStorageClientTests
{
    private IAzureBlobStorageClient GetSut(string connectionString)
    {
        var blobServiceClient = new BlobServiceClient(connectionString);
        return new AzureBlobStorageClient(blobServiceClient);
    }

    public class IntegrationTests
    {
        /// <summary>
        /// Integration Tests
        /// </summary>
        public class HiveITAzureAccount : AzureBlobStorageClientTests
        {
            private string _integrationTestStorageAccountConnectionString = "** add Azure connection string here **";
            private string _integrationTestContainerName = "integration-tests";

            private IAzureBlobStorageClient GetSut() => base.GetSut(_integrationTestStorageAccountConnectionString);

            [Fact(Skip = "This integration test creates a blob in an Azure Storage Account and retrieves it again.")]
            public async Task CanUploadBlob()
            {
                // ARRANGE
                var uniqueBlobName = Guid.NewGuid().ToString();
                var sut = GetSut();
                var blob = new Blob("This is a test", new Dictionary<string, string>
                {
                    {"key1", "value1"},
                    {"key2", "value2"},
                    {"timestamp", DateTimeOffset.Now.ToString("u")}
                });
                
                // ACT
                await sut.UploadAsync(_integrationTestContainerName, uniqueBlobName, blob);
                
                // ASSERT
                var actual = await sut.DownloadAsync(_integrationTestContainerName, uniqueBlobName);
                Assert.Equal(blob, actual);
                await sut.DeleteAsync(_integrationTestContainerName, uniqueBlobName);
            }
            
            [Fact(Skip = "This integration test gets a non-existent blob from Azure Storage Account.")]
            public async Task DownloadBlob_WhenBlobDoesNotExist_ThenThrows()
            {
                // ARRANGE
                var uniqueBlobName = Guid.NewGuid().ToString();
                var sut = GetSut();
                
                // ACT
                var actual = await Record.ExceptionAsync(() => sut.DownloadAsync(_integrationTestContainerName, uniqueBlobName));
                
                // ASSERT
                Assert.NotNull(actual);
                var azureBlobStorageNotFoundException = Assert.IsType<AzureBlobStorageNotFoundException>(actual);
                Assert.Equal(uniqueBlobName, azureBlobStorageNotFoundException.BlobName);
                Assert.Equal(_integrationTestContainerName, azureBlobStorageNotFoundException.ContainerName);
            }
            
            [Fact(Skip = "This integration test deletes a non-existent blob from Azure Storage Account.")]
            public async Task DeleteBlob_WhenBlobDoesNotExist_ThenDoesNotThrow()
            {
                // ARRANGE
                var uniqueBlobName = Guid.NewGuid().ToString();
                var sut = GetSut();
                
                // ACT
                await sut.DeleteAsync(_integrationTestContainerName, uniqueBlobName);
            }
        }    
    }
}
