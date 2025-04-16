using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using Moq;
using Blob = GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

internal class AzureBlobStorageClientMockBuilder
{
    private readonly Mock<IAzureBlobStorageClient> _mock = new(MockBehavior.Strict);
    private bool _deleteBlobIfExistsIsSuccessful = true;

    public AzureBlobStorageClientMockBuilder()
    {
        Assert = new Asserter(_mock);

        _mock.Setup(mock => mock.DeleteBlobIfExists(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _deleteBlobIfExistsIsSuccessful);

        _mock.Setup(mock => mock.UploadBlob(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Blob>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    public IAzureBlobStorageClient Build() => _mock.Object;

    public AzureBlobStorageClientMockBuilder WhereDeleteBlobIfExistsIsSuccessful(bool isSuccessful)
    {
        _deleteBlobIfExistsIsSuccessful = isSuccessful;
        return this;
    }

    public Asserter Assert { get; }

    public class Asserter(Mock<IAzureBlobStorageClient> mock)
    {
        public void BlobWasDeletedIfExists(
            string? containerName = null,
            string? blobName = null)
        {
            mock.Verify(m => m.DeleteBlobIfExists(
                    It.Is<string>(actualContainerName => containerName == null || actualContainerName == containerName),
                    It.Is<string>(actualBlobName => blobName == null || actualBlobName == blobName),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        public void NoBlobsDeleted()
        {
            mock
                .Verify(m => m.DeleteBlobIfExists(
                            It.IsAny<string>(),
                            It.IsAny<string>(),
                            It.IsAny<CancellationToken>()),
                    Times.Never);
        }

        public void BlobWasUploaded(
            string? containerName = null,
            string? blobName = null,
            Func<Blob, bool>? whereBlob = null)
        {
            mock.Verify(m => m.UploadBlob(
                    It.Is<string>(actualContainerName => containerName == null || actualContainerName == containerName),
                    It.Is<string>(actualBlobName => blobName == null || actualBlobName == blobName),
                    It.Is<Blob>(blob => whereBlob == null || whereBlob(blob)),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
