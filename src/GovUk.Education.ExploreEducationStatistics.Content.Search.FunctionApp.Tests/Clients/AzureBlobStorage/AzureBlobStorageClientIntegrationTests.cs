using System.Net.Mime;
using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Blob = GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Clients.AzureBlobStorage;

public class AzureBlobStorageClientIntegrationTests
{
    private AzureBlobStorageClient GetSut(string connectionString)
    {
        var blobServiceClient = new BlobServiceClient(connectionString);
        return new AzureBlobStorageClient(blobServiceClient, Mock.Of<ILogger<AzureBlobStorageClient>>());
    }

    public class IntegrationTests
    {
        /// <summary>
        /// Integration Tests.
        /// In order to run these:
        /// - set StorageAccountName to the name of the Storage Account in Azure
        /// - set StorageAccountAccessKey to one of its Access keys (found under Security + networking)
        /// - unskip the test
        /// </summary>
        public class HiveITAzureAccount : AzureBlobStorageClientIntegrationTests
        {
            private const string StorageAccountName = "-- azure storage account name here --";
            private const string StorageAccountAccessKey = "-- azure storage account access key here --";

            private const string IntegrationTestStorageAccountConnectionString =
                $"AccountName={StorageAccountName};AccountKey={StorageAccountAccessKey};";
            private const string IntegrationTestContainerName = "integration-tests";

            private AzureBlobStorageClient GetSut() => base.GetSut(IntegrationTestStorageAccountConnectionString);

            [Fact(Skip = "This integration test creates a blob in an Azure Storage Account and retrieves it again.")]
            public async Task CanUploadBlob()
            {
                // ARRANGE
                var uniqueBlobName = Guid.NewGuid().ToString();
                var sut = GetSut();
                var blob = new Blob(
                    "This is a test",
                    new Dictionary<string, string>
                    {
                        { "key1", "value1" },
                        { "key2", "value2" },
                        { "timestamp", DateTimeOffset.Now.ToString("u") },
                    }
                );
                const string contentType = MediaTypeNames.Text.Plain;

                // ACT
                await sut.UploadBlob(IntegrationTestContainerName, uniqueBlobName, blob, contentType);

                // ASSERT
                var actual = await AzureBlobStorageIntegrationHelper.DownloadAsync(
                    sut.BlobServiceClient,
                    IntegrationTestContainerName,
                    uniqueBlobName
                );
                Assert.Equal(blob, actual);
                await AzureBlobStorageIntegrationHelper.DeleteAsync(
                    sut.BlobServiceClient,
                    IntegrationTestContainerName,
                    uniqueBlobName
                );
            }

            [Fact(Skip = "This integration test gets a non-existent blob from Azure Storage Account.")]
            public async Task DownloadBlob_WhenBlobDoesNotExist_ThenThrows()
            {
                // ARRANGE
                var uniqueBlobName = Guid.NewGuid().ToString();
                var sut = GetSut();

                // ACT
                var actual = await Record.ExceptionAsync(() =>
                    AzureBlobStorageIntegrationHelper.DownloadAsync(
                        sut.BlobServiceClient,
                        IntegrationTestContainerName,
                        uniqueBlobName
                    )
                );

                // ASSERT
                Assert.NotNull(actual);
                var azureBlobStorageNotFoundException = Assert.IsType<AzureBlobStorageNotFoundException>(actual);
                Assert.Equal(uniqueBlobName, azureBlobStorageNotFoundException.BlobName);
                Assert.Equal(IntegrationTestContainerName, azureBlobStorageNotFoundException.ContainerName);
            }

            [Fact(Skip = "This integration test deletes a non-existent blob from Azure Storage Account.")]
            public async Task DeleteBlob_WhenBlobDoesNotExist_ThenDoesNotThrow()
            {
                // ARRANGE
                var uniqueBlobName = Guid.NewGuid().ToString();
                var sut = GetSut();

                // ACT
                await AzureBlobStorageIntegrationHelper.DeleteAsync(
                    sut.BlobServiceClient,
                    IntegrationTestContainerName,
                    uniqueBlobName
                );
            }

            [Fact(
                Skip = "This integration test deletes all blobs from the specified container in the Azure Storage Account."
            )]
            public async Task DeleteAllBlobsFromContainer()
            {
                // ARRANGE
                var sut = GetSut();

                // ACT
                await sut.DeleteAllBlobsFromContainer(IntegrationTestContainerName);
            }

            [Fact(
                Skip = "This integration test list all blobs from the specified container in the Azure Storage Account."
            )]
            public async Task ListAllBlobsInContainer()
            {
                // ARRANGE
                var sut = GetSut();

                // ACT
                var blobNames = await sut.ListBlobsInContainer(IntegrationTestContainerName);

                // ASSERT
                Assert.NotEmpty(blobNames);
            }
        }
    }
}

public class AzureBlobStorageNotFoundException : AzureBlobStorageException
{
    public AzureBlobStorageNotFoundException(string containerName, string blobName)
        : base(containerName, blobName, "Not found") { }
}
