#nullable enable
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using Azure;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.BlobSasService;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Services.BlobServiceTestUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Services;

public abstract class BlobSasServiceTests
{
    public class CreateBlobDownloadTokenTests : BlobSasServiceTests
    {
        [Fact]
        public async Task Success()
        {
            const string filename = "test.pdf";
            const string path = "path/to/test.pdf";
            var now = DateTime.UtcNow;
            var container = BlobContainers.PublicContent;
            var expiryDuration = TimeSpan.FromSeconds(37);

            var dateTimeProvider = new DateTimeProvider(now);

            var blobClient = MockBlobClient(
                name: path,
                createdOn: DateTimeOffset.UtcNow,
                contentLength: 10 * 1024,
                contentType: MediaTypeNames.Application.Pdf,
                metadata: new Dictionary<string, string>()
            );

            blobClient.SetupCanGenerateSasUri(true);

            blobClient.SetupGenerateReadonlySasUri(
                expectedExpiry: now.Add(expiryDuration),
                expectedContainerName: container.Name,
                uriToReturn: "https://sasurl?param1=1&param2=2");

            var blobContainerClient = MockBlobContainerClient(container.Name, blobClient);
            var blobServiceClient = MockBlobServiceClient(blobContainerClient);

            var service = BuildService(dateTimeProvider: dateTimeProvider);

            var result = await service
                .CreateBlobDownloadToken(
                    blobServiceClient: blobServiceClient.Object,
                    container: BlobContainers.PublicContent,
                    filename: filename,
                    path: path,
                    expiryDuration: expiryDuration,
                    cancellationToken: default);

            var token = result.AssertRight();

            Assert.Equal("param1=1&param2=2", token.Token);
            Assert.Equal(container.Name, token.ContainerName);
            Assert.Equal(filename, token.Filename);
            Assert.Equal(path, token.Path);
            Assert.Equal(MediaTypeNames.Application.Pdf, token.ContentType);
        }

        [Fact]
        public async Task BlobNotFound()
        {
            const string filename = "test.pdf";
            const string path = "path/to/test.pdf";
            var container = BlobContainers.PublicContent;

            var blobClient = MockBlobClient(
                name: path,
                createdOn: DateTimeOffset.UtcNow,
                contentLength: 10 * 1024,
                contentType: MediaTypeNames.Application.Pdf,
                metadata: new Dictionary<string, string>()
            );

            blobClient
                .Setup(client => client.ExistsAsync(default))
                .ReturnsAsync(Response.FromValue(false, null!));

            var blobContainerClient = MockBlobContainerClient(container.Name, blobClient);
            var blobServiceClient = MockBlobServiceClient(blobContainerClient);

            var service = BuildService();

            var result = await service
                .CreateBlobDownloadToken(
                    blobServiceClient: blobServiceClient.Object,
                    container: BlobContainers.PublicContent,
                    filename: filename,
                    path: path,
                    expiryDuration: TimeSpan.FromSeconds(37),
                    cancellationToken: default);

            result.AssertNotFound();
        }
    }

    public class CreateSecureBlobClientTests : BlobSasServiceTests
    {
        [Fact]
        public async Task Success()
        {
            var originalToken = new BlobDownloadToken(
                Token: "a-token",
                ContainerName: "a-container",
                Path: "a-path",
                Filename: "a-filename.csv",
                ContentType: MediaTypeNames.Text.Csv);

            var blobClient = MockBlobClient(
                name: originalToken.Path,
                createdOn: DateTimeOffset.UtcNow,
                contentLength: 10 * 1024,
                contentType: MediaTypeNames.Application.Pdf,
                metadata: new Dictionary<string, string>()
            );

            var secureClient = MockBlobClient(
                name: originalToken.Path,
                createdOn: DateTimeOffset.UtcNow.AddMinutes(1),
                contentLength: 10 * 1024,
                contentType: MediaTypeNames.Application.Pdf,
                metadata: new Dictionary<string, string>()
            );

            // The container client is looked up by the container name in the download token. 
            var blobContainerClient = MockBlobContainerClient(
                containerName: originalToken.ContainerName,
                blobClient);

            var blobServiceClient = MockBlobServiceClient(blobContainerClient);

            var originalBlobClientUri = new Uri("https://original-client-uri");

            blobClient
                .Setup(c => c.Uri)
                .Returns(originalBlobClientUri);

            var secureBlobClientCreator = new Mock<ISecureBlobClientCreator>(Strict);

            secureBlobClientCreator
                .Setup(c => c.CreateSecureBlobClient(
                    It.Is<Uri>(uri => uri.Equals(originalBlobClientUri)),
                    originalToken.Token))
                .Returns(secureClient.Object);

            var service = BuildService(secureBlobClientCreator: secureBlobClientCreator.Object);

            var result = await service
                .CreateSecureBlobClient(
                    blobServiceClient: blobServiceClient.Object,
                    token: originalToken);

            secureBlobClientCreator.Verify();

            var returnedClient = result.AssertRight();

            Assert.Same(secureClient.Object, returnedClient);
        }
    }

    private BlobSasService BuildService(
        DateTimeProvider? dateTimeProvider = null,
        ISecureBlobClientCreator? secureBlobClientCreator = null)
    {
        return new BlobSasService(
            dateTimeProvider: dateTimeProvider ?? new DateTimeProvider(),
            logger: Mock.Of<ILogger<BlobSasService>>(),
            secureBlobClientCreator: secureBlobClientCreator);
    }
}
