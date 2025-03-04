using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using Moq;
using Blob = GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

internal class AzureBlobStorageClientBuilder
{
    private readonly Mock<IAzureBlobStorageClient> _mock = new(MockBehavior.Strict);

    public AzureBlobStorageClientBuilder()
    {
        Assert = new(_mock);

        _mock.Setup(mock => mock.UploadBlob(
                It.IsAny<UploadBlobRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new UploadBlobResponse.Success());
    }

    public IAzureBlobStorageClient Build() => _mock.Object;

    public Asserter Assert { get; }

    public class Asserter(Mock<IAzureBlobStorageClient> mock)
    {
        public void BlobWasUploaded(
            string? containerName = null,
            string? blobName = null,
            Func<Blob, bool>? whereBlob = null)
        {
            mock.Verify(m => m.UploadBlob(
                It.Is<UploadBlobRequest>(request =>
                    (containerName == null || request.ContainerName == containerName) &&
                    (blobName == null || request.BlobName == blobName) &&
                    (whereBlob == null || whereBlob(request.Blob))),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
