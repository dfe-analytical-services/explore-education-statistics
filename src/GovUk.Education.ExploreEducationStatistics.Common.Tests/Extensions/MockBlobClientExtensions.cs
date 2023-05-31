#nullable enable
using System;
using System.IO;
using System.Threading;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Moq;
using Moq.Language.Flow;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class MockBlobClientExtensions
{
    public static IReturnsResult<BlobClient> SetupDownloadContentAsync(
        this Mock<BlobClient> blobClient,
        string content,
        string? contentEncoding = null,
        CancellationToken cancellationToken = default)
    {
        var downloadDetails = BlobsModelFactory.BlobDownloadDetails(
            lastModified: default,
            metadata: default,
            contentRange: default,
            contentEncoding: contentEncoding,
            cacheControl: default,
            contentDisposition: default,
            contentLanguage: default,
            blobSequenceNumber: default,
            copyCompletedOn: default,
            copyStatusDescription: default,
            copyId: default,
            copyProgress: default,
            copySource: default,
            copyStatus: default,
            leaseDuration: default,
            leaseState: default,
            leaseStatus: default,
            acceptRanges: default,
            blobCommittedBlockCount: default,
            isServerEncrypted: default,
            encryptionKeySha256: default,
            encryptionScope: default,
            blobContentHash: default,
            tagCount: default,
            versionId: default,
            isSealed: default,
            objectReplicationSourceProperties: default,
            objectReplicationDestinationPolicy: default
        );

        var blobDownloadResult = BlobsModelFactory.BlobDownloadResult(
            content: BinaryData.FromString(content),
            details: downloadDetails);

        var response = new Mock<Response<BlobDownloadResult>>(MockBehavior.Strict);
        response.SetupGet(r => r.Value)
            .Returns(blobDownloadResult);

        return blobClient.Setup(client => client.DownloadContentAsync(cancellationToken))
            .ReturnsAsync(response.Object);
    }

    public static IReturnsResult<BlobClient> SetupDownloadContentAsyncNotFound(
        this Mock<BlobClient> blobClient,
        CancellationToken cancellationToken = default)
    {
        return blobClient.Setup(client => client.DownloadContentAsync(cancellationToken))
            .ThrowsAsync(new RequestFailedException(status: 404, message: "Blob not found"));
    }

    public static IReturnsResult<BlobClient> SetupDownloadToAsync(
        this Mock<BlobClient> blobClient,
        string content,
        CancellationToken cancellationToken = default)
    {
        return blobClient.SetupDownloadToAsync(content.ToStream(), cancellationToken);
    }

    public static IReturnsResult<BlobClient> SetupDownloadToAsync(
        this Mock<BlobClient> blobClient,
        Stream sourceStream,
        CancellationToken cancellationToken = default)
    {
        return blobClient.Setup(client =>
                client.DownloadToAsync(It.IsAny<Stream>(), cancellationToken))
            .Callback<Stream, CancellationToken>(
                (destinationStream, token) =>
                    sourceStream.CopyToAsync(destinationStream, token))
            .ReturnsAsync(new Mock<Response>().Object);
    }
}
