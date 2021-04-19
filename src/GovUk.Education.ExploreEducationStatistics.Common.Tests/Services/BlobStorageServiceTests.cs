using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Services
{
    public class BlobStorageServiceTests
    {
        [Fact]
        public async Task CheckBlobExists_BlobExists()
        {
            var blobClient = MockBlobClient(
                name: "path/to/test.png",
                exists: true);

            var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name, blobClient);
            var blobServiceClient = MockBlobServiceClient(blobContainerClient);

            var service = SetupBlobStorageService(blobServiceClient: blobServiceClient.Object);

            Assert.True(await service.CheckBlobExists(PublicReleaseFiles, "path/to/test.png"));
        }

        [Fact]
        public async Task CheckBlobExists_BlobDoesNotExist()
        {
            var blobClient = MockBlobClient(
                name: "path/to/test.png",
                exists: false);

            var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name, blobClient);
            var blobServiceClient = MockBlobServiceClient(blobContainerClient);

            var service = SetupBlobStorageService(blobServiceClient: blobServiceClient.Object);

            Assert.False(await service.CheckBlobExists(PublicReleaseFiles, "path/to/test.png"));
        }

        [Fact]
        public async Task GetBlob()
        {
            var createdOn = DateTimeOffset.UtcNow;

            var blobClient = MockBlobClient(
                name: "path/to/test.pdf",
                createdOn: createdOn,
                contentLength: 10 * 1024,
                contentType: "application/pdf",
                metadata: new Dictionary<string, string>()
            );

            var blobContainerClient = MockBlobContainerClient(PublicReleaseFiles.Name, blobClient);
            var blobServiceClient = MockBlobServiceClient(blobContainerClient);

            var service = SetupBlobStorageService(blobServiceClient: blobServiceClient.Object);

            var result = await service.GetBlob(PublicReleaseFiles, "path/to/test.pdf");

            Assert.Equal(createdOn, result.Created);
            Assert.Equal(10 * 1024, result.ContentLength);
            Assert.Equal("application/pdf", result.ContentType);
            Assert.Equal("test.pdf", result.FileName);
            Assert.Equal(new Dictionary<string, string>(), result.Meta);
            Assert.Equal("path/to/test.pdf", result.Path);
            Assert.Equal("10 Kb", result.Size);
        }

        private static Mock<BlobServiceClient> MockBlobServiceClient(
            params Mock<BlobContainerClient>[] blobContainerClients)
        {
            var blobServiceClient = new Mock<BlobServiceClient>(MockBehavior.Strict);

            blobServiceClient.Setup(s => s.Uri)
                .Returns(new Uri("http://data-storage:10001/devstoreaccount1;"));

            foreach (var blobContainerClient in blobContainerClients)
            {
                blobServiceClient.Setup(client => client.GetBlobContainerClient(blobContainerClient.Object.Name))
                    .Returns(blobContainerClient.Object);
            }

            return blobServiceClient;
        }

        private static Mock<BlobContainerClient> MockBlobContainerClient(string containerName,
            params Mock<BlobClient>[] blobClients)
        {
            var blobContainerClient = new Mock<BlobContainerClient>(MockBehavior.Strict);

            blobContainerClient
                .SetupGet(client => client.Name)
                .Returns(containerName);

            blobContainerClient.Setup(s => s.CreateIfNotExistsAsync(PublicAccessType.None, default, default, default))
                .ReturnsAsync(Response.FromValue(BlobsModelFactory.BlobContainerInfo(ETag.All, DateTimeOffset.UtcNow),
                    null));

            foreach (var blobClient in blobClients)
            {
                blobContainerClient.Setup(client => client.GetBlobClient(blobClient.Object.Name))
                    .Returns(blobClient.Object);
            }

            return blobContainerClient;
        }

        private static Mock<BlobClient> MockBlobClient(string name,
            DateTimeOffset createdOn = default,
            int contentLength = 0,
            string contentType = null,
            IDictionary<string, string> metadata = null,
            bool exists = true)
        {
            var blobClient = new Mock<BlobClient>(MockBehavior.Strict);

            blobClient.SetupGet(client => client.Name)
                .Returns(name);

            blobClient.Setup(client => client.ExistsAsync(default))
                .ReturnsAsync(Response.FromValue(exists, null));

            var blobProperties = BlobsModelFactory.BlobProperties(
                createdOn: createdOn,
                contentLength: contentLength,
                contentType: contentType,
                metadata: metadata
            );

            blobClient.Setup(client => client.GetPropertiesAsync(default, default))
                .ReturnsAsync(Response.FromValue(blobProperties, null));

            return blobClient;
        }

        private static BlobStorageService SetupBlobStorageService(
            BlobServiceClient blobServiceClient = null)
        {
            return new BlobStorageService(
                connectionString: "",
                blobServiceClient ?? new Mock<BlobServiceClient>().Object,
                new Mock<ILogger<BlobStorageService>>().Object
            );
        }
    }
}
