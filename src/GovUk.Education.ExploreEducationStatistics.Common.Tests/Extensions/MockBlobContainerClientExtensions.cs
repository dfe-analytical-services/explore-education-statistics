#nullable enable
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Moq;
using Moq.Language.Flow;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class MockBlobContainerClientExtensions
{
    public static IReturnsResult<BlobContainerClient> SetupCreateIfNotExistsAsync(
        this Mock<BlobContainerClient> blobContainerClient
    )
    {
        return blobContainerClient
            .Setup(client =>
                client.CreateIfNotExistsAsync(
                    It.IsAny<PublicAccessType>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<BlobContainerEncryptionScopeOptions>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Mock.Of<Response<BlobContainerInfo>>());
    }

    public static void VerifyCreateIfNotExistsAsync(this Mock<BlobContainerClient> blobContainerClient)
    {
        blobContainerClient.Verify(
            client =>
                client.CreateIfNotExistsAsync(
                    It.IsAny<PublicAccessType>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<BlobContainerEncryptionScopeOptions>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
