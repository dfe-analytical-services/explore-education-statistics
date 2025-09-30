using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using Moq;
using Blob = GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

internal class AzureBlobStorageClientMockBuilder
{
    private readonly Mock<IAzureBlobStorageClient> _mock = new(MockBehavior.Strict);
    private string? _deleteBlobFailsForBlobName;
    private bool _deleteBlobIsSuccessful = true;

    public IAzureBlobStorageClient Build()
    {
        _mock
            .Setup(mock =>
                mock.DeleteBlobIfExists(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(_deleteBlobIsSuccessful);

        if (!string.IsNullOrWhiteSpace(_deleteBlobFailsForBlobName))
        {
            _mock
                .Setup(mock =>
                    mock.DeleteBlobIfExists(
                        It.IsAny<string>(),
                        _deleteBlobFailsForBlobName,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(false);
        }

        _mock
            .Setup(mock =>
                mock.UploadBlob(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Blob>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);

        _mock
            .Setup(mock =>
                mock.DeleteAllBlobsFromContainer(It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .Returns(Task.CompletedTask);

        return _mock.Object;
    }

    public AzureBlobStorageClientMockBuilder WhereDeleteBlobFails()
    {
        _deleteBlobIsSuccessful = false;
        return this;
    }

    public AzureBlobStorageClientMockBuilder WhereDeleteBlobFailsFor(string blobName)
    {
        _deleteBlobFailsForBlobName = blobName;
        return this;
    }

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IAzureBlobStorageClient> mock)
    {
        public void BlobWasDeleted(string? containerName = null, string? blobName = null)
        {
            mock.Verify(
                m =>
                    m.DeleteBlobIfExists(
                        It.Is<string>(actualContainerName =>
                            containerName == null || actualContainerName == containerName
                        ),
                        It.Is<string>(actualBlobName =>
                            blobName == null || actualBlobName == blobName
                        ),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }

        public void NoBlobsDeleted()
        {
            mock.Verify(
                m =>
                    m.DeleteBlobIfExists(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );
        }

        public void BlobWasUploaded(
            string? containerName = null,
            string? blobName = null,
            string? contentType = null,
            string? contentEncoding = null,
            Func<Blob, bool>? whereBlob = null
        )
        {
            mock.Verify(
                m =>
                    m.UploadBlob(
                        It.Is<string>(actualContainerName =>
                            containerName == null || actualContainerName == containerName
                        ),
                        It.Is<string>(actualBlobName =>
                            blobName == null || actualBlobName == blobName
                        ),
                        It.Is<Blob>(blob => whereBlob == null || whereBlob(blob)),
                        It.Is<string>(actualContentType =>
                            contentType == null || actualContentType == contentType
                        ),
                        It.Is<string?>(actualContentEncoding =>
                            contentEncoding == null || actualContentEncoding == contentEncoding
                        ),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }

        public void AllBlobsWereDeleted(string? containerName)
        {
            mock.Verify(
                m =>
                    m.DeleteAllBlobsFromContainer(
                        It.Is<string>(actualContainerName =>
                            containerName == null || actualContainerName == containerName
                        ),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }
    }
}
