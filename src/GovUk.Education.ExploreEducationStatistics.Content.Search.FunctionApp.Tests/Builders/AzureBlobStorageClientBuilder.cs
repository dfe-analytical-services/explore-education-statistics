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
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<Blob>(), 
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
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
                It.Is<string>(actualContainerName => containerName == null || actualContainerName == containerName),
                It.Is<string>(actualBlobName => blobName == null || actualBlobName == blobName),
                It.Is<Blob>(blob => whereBlob == null || whereBlob(blob)), 
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
