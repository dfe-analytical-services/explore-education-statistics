#nullable enable
using System.Net.Mime;
using System.Web;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
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
                uriToReturn: "https://sasurl?param1=1&param2=2"
            );

            var blobContainerClient = MockBlobContainerClient(container.Name, blobClient);
            var blobServiceClient = MockBlobServiceClient(blobContainerClient);

            var service = BuildService(dateTimeProvider: dateTimeProvider);

            var result = await service.CreateBlobDownloadToken(
                blobServiceClient: blobServiceClient.Object,
                container: BlobContainers.PublicContent,
                filename: filename,
                path: path,
                expiryDuration: expiryDuration,
                cancellationToken: CancellationToken.None
            );

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
                .Setup(client => client.ExistsAsync(CancellationToken.None))
                .ReturnsAsync(Response.FromValue(false, null!));

            var blobContainerClient = MockBlobContainerClient(container.Name, blobClient);
            var blobServiceClient = MockBlobServiceClient(blobContainerClient);

            var service = BuildService();

            var result = await service.CreateBlobDownloadToken(
                blobServiceClient: blobServiceClient.Object,
                container: BlobContainers.PublicContent,
                filename: filename,
                path: path,
                expiryDuration: TimeSpan.FromSeconds(37),
                cancellationToken: CancellationToken.None
            );

            result.AssertNotFound();
        }

        [Fact]
        public async Task NoAccountKey_SignsWithUserDelegationKey()
        {
            const string path = "path/to/test.pdf";
            var now = DateTime.UtcNow;
            var container = BlobContainers.PublicContent;
            var expiryDuration = TimeSpan.FromSeconds(37);

            var (blobServiceClient, _) = MockIdentityBlobServiceClient(container, path);

            blobServiceClient
                .Setup(client =>
                    client.GetUserDelegationKeyAsync(
                        It.Is<BlobGetUserDelegationKeyOptions>(options => options.ExpiresOn == now.AddDays(1)),
                        CancellationToken.None
                    )
                )
                .ReturnsAsync(Response.FromValue(UserDelegationKey(expiresOn: now.AddDays(1)), null!));

            var service = BuildService(dateTimeProvider: new DateTimeProvider(now));

            var result = await service.CreateBlobDownloadToken(
                blobServiceClient: blobServiceClient.Object,
                container: container,
                filename: "test.pdf",
                path: path,
                expiryDuration: expiryDuration,
                cancellationToken: CancellationToken.None
            );

            var token = HttpUtility.ParseQueryString(result.AssertRight().Token);

            // A read-only container SAS, signed by the user delegation key's identity.
            Assert.Equal("c", token["sr"]);
            Assert.Equal("r", token["sp"]);
            Assert.Equal(UserDelegationKeyObjectId, token["skoid"]);
            Assert.Equal(new DateTimeOffset(now.Add(expiryDuration)).ToString("yyyy-MM-ddTHH:mm:ssZ"), token["se"]);
            Assert.NotEmpty(token["sig"]!);
        }

        [Fact]
        public async Task NoAccountKey_ReusesCachedUserDelegationKey()
        {
            const string path = "path/to/test.pdf";
            var now = DateTime.UtcNow;
            var container = BlobContainers.PublicContent;

            var (blobServiceClient, _) = MockIdentityBlobServiceClient(container, path);

            blobServiceClient
                .Setup(client =>
                    client.GetUserDelegationKeyAsync(
                        It.IsAny<BlobGetUserDelegationKeyOptions>(),
                        CancellationToken.None
                    )
                )
                .ReturnsAsync(Response.FromValue(UserDelegationKey(expiresOn: now.AddDays(1)), null!));

            var service = BuildService(dateTimeProvider: new DateTimeProvider(now));

            foreach (var _ in Enumerable.Range(0, 3))
            {
                var result = await service.CreateBlobDownloadToken(
                    blobServiceClient: blobServiceClient.Object,
                    container: container,
                    filename: "test.pdf",
                    path: path,
                    expiryDuration: TimeSpan.FromMinutes(5),
                    cancellationToken: CancellationToken.None
                );

                result.AssertRight();
            }

            blobServiceClient.Verify(
                client =>
                    client.GetUserDelegationKeyAsync(
                        It.IsAny<BlobGetUserDelegationKeyOptions>(),
                        CancellationToken.None
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task NoAccountKey_ReplacesUserDelegationKeyExpiringBeforeSas()
        {
            const string path = "path/to/test.pdf";
            var now = DateTime.UtcNow;
            var container = BlobContainers.PublicContent;

            var (blobServiceClient, _) = MockIdentityBlobServiceClient(container, path);

            // The first key expires in 10 minutes, which covers the first 5 minute SAS, but not the second 15 minute one.
            // Therefore we expect 2 keys to be requested, one for the first SAS and another for the second.
            blobServiceClient
                .SetupSequence(client =>
                    client.GetUserDelegationKeyAsync(
                        It.IsAny<BlobGetUserDelegationKeyOptions>(),
                        CancellationToken.None
                    )
                )
                .ReturnsAsync(Response.FromValue(UserDelegationKey(expiresOn: now.AddMinutes(10)), null!))
                .ReturnsAsync(Response.FromValue(UserDelegationKey(expiresOn: now.AddDays(1)), null!));

            var service = BuildService(dateTimeProvider: new DateTimeProvider(now));

            var firstResult = await service.CreateBlobDownloadToken(
                blobServiceClient: blobServiceClient.Object,
                container: container,
                filename: "test.pdf",
                path: path,
                expiryDuration: TimeSpan.FromMinutes(5),
                cancellationToken: CancellationToken.None
            );

            firstResult.AssertRight();

            var secondResult = await service.CreateBlobDownloadToken(
                blobServiceClient: blobServiceClient.Object,
                container: container,
                filename: "test.pdf",
                path: path,
                expiryDuration: TimeSpan.FromMinutes(15),
                cancellationToken: CancellationToken.None
            );

            secondResult.AssertRight();

            blobServiceClient.Verify(
                client =>
                    client.GetUserDelegationKeyAsync(
                        It.IsAny<BlobGetUserDelegationKeyOptions>(),
                        CancellationToken.None
                    ),
                Times.Exactly(2)
            );
        }

        private const string UserDelegationKeyObjectId = "00000000-0000-0000-0000-000000000001";

        private static UserDelegationKey UserDelegationKey(DateTimeOffset expiresOn) =>
            // Positional arguments, as the factory's overloads share the same parameter names.
            BlobsModelFactory.UserDelegationKey(
                UserDelegationKeyObjectId, // signedObjectId
                "00000000-0000-0000-0000-000000000002", // signedTenantId
                expiresOn.AddDays(-1), // signedStartsOn
                expiresOn, // signedExpiresOn
                "b", // signedService
                "2025-01-05", // signedVersion
                Convert.ToBase64String(new byte[32]) // value
            );

        private static (Mock<BlobServiceClient>, Mock<BlobClient>) MockIdentityBlobServiceClient(
            IBlobContainer container,
            string path
        )
        {
            var blobClient = MockBlobClient(
                name: path,
                createdOn: DateTimeOffset.UtcNow,
                contentLength: 10 * 1024,
                contentType: MediaTypeNames.Application.Pdf,
                metadata: new Dictionary<string, string>()
            );

            // A client connected with a managed identity has no account key to sign a SAS with.
            blobClient.SetupCanGenerateSasUri(false);
            blobClient
                .Setup(client => client.Uri)
                .Returns(new Uri($"https://testaccount.blob.core.windows.net/{container.Name}/{path}"));

            var blobContainerClient = MockBlobContainerClient(container.Name, blobClient);
            var blobServiceClient = MockBlobServiceClient(blobContainerClient);

            return (blobServiceClient, blobClient);
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
                ContentType: MediaTypeNames.Text.Csv
            );

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
            var blobContainerClient = MockBlobContainerClient(containerName: originalToken.ContainerName, blobClient);

            var blobServiceClient = MockBlobServiceClient(blobContainerClient);

            var originalBlobClientUri = new Uri("https://original-client-uri");

            blobClient.Setup(c => c.Uri).Returns(originalBlobClientUri);

            var secureBlobClientCreator = new Mock<ISecureBlobClientCreator>(Strict);

            secureBlobClientCreator
                .Setup(c =>
                    c.CreateSecureBlobClient(It.Is<Uri>(uri => uri.Equals(originalBlobClientUri)), originalToken.Token)
                )
                .Returns(secureClient.Object);

            var service = BuildService(secureBlobClientCreator: secureBlobClientCreator.Object);

            var result = await service.CreateSecureBlobClient(
                blobServiceClient: blobServiceClient.Object,
                token: originalToken
            );

            secureBlobClientCreator.Verify();

            var returnedClient = result.AssertRight();

            Assert.Same(secureClient.Object, returnedClient);
        }
    }

    private BlobSasService BuildService(
        DateTimeProvider? dateTimeProvider = null,
        ISecureBlobClientCreator? secureBlobClientCreator = null
    )
    {
        return new BlobSasService(
            dateTimeProvider: dateTimeProvider ?? new DateTimeProvider(),
            logger: Mock.Of<ILogger<BlobSasService>>(),
            secureBlobClientCreator: secureBlobClientCreator
        );
    }
}
