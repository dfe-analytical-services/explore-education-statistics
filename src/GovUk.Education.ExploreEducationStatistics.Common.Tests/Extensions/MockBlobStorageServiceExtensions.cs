#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Moq;
using Moq.Language.Flow;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions
{
    public static class MockBlobStorageServiceExtensions
    {
        public static IReturnsResult<IBlobStorageService> SetupFindBlob(
            this Mock<IBlobStorageService> service,
            IBlobContainer container,
            string path,
            BlobInfo? blob)
        {
            return service.Setup(s => s.FindBlob(container, path))
                .ReturnsAsync(blob);
        }

        public static IReturnsResult<IBlobStorageService> SetupStreamBlob(
            this Mock<IBlobStorageService> service,
            IBlobContainer container,
            string expectedBlobPath,
            string filePathToStream)
        {
            return service.Setup(s => s.StreamBlob(container, expectedBlobPath, null, default))
                .ReturnsAsync(() => File.OpenRead(filePathToStream));
        }
        
        public static IReturnsResult<IBlobStorageService> SetupListBlobs(
            this Mock<IBlobStorageService> service,
            IBlobContainer container,
            string expectedBlobPath,
            List<BlobInfo> blobList)
        {
            return service.Setup(s => s.ListBlobs(container, expectedBlobPath))
                .ReturnsAsync(blobList);
        }
        
        public static IReturnsResult<IBlobStorageService> SetupListBlobs(
            this Mock<IBlobStorageService> service,
            IBlobContainer container,
            string expectedBlobPath,
            params BlobInfo[] blobs)
        {
            return SetupListBlobs(service, container, expectedBlobPath, blobs.ToList());
        }

        public static IReturnsResult<IBlobStorageService> SetupCheckBlobExists(
            this Mock<IBlobStorageService> service,
            IBlobContainer container,
            string path,
            bool exists)
        {
            return service.Setup(s => s.CheckBlobExists(container, path))
                .ReturnsAsync(exists);
        }

        public static IReturnsResult<IBlobStorageService> SetupDeleteBlob(
            this Mock<IBlobStorageService> service,
            IBlobContainer container,
            string path)
        {
            return service.Setup(s => s.DeleteBlob(container, path))
                .Returns(Task.CompletedTask);
        }

        public static IReturnsResult<IBlobStorageService> SetupDownloadToStream(
            this Mock<IBlobStorageService> service,
            IBlobContainer container,
            string path,
            string blobText,
            CancellationToken? cancellationToken = null)
        {
            return service.Setup(
                    s =>
                        s.DownloadToStream(container, path, It.IsAny<Stream>(), cancellationToken)
                )
                .Callback<IBlobContainer, string, Stream, CancellationToken?>(
                    (_, _, stream, _) =>
                    {
                        stream.WriteText(blobText);

                        if (stream.CanSeek)
                        {
                            stream.Position = 0;
                        }
                    }
                )
                .ReturnsAsync(blobText.ToStream());
        }

        public static IReturnsResult<IBlobStorageService> SetupDownloadToStreamNotFound(
            this Mock<IBlobStorageService> service,
            IBlobContainer container,
            string path,
            CancellationToken? cancellationToken = null)
        {
            return service.Setup(
                    s =>
                        s.DownloadToStream(container, path, It.IsAny<Stream>(), cancellationToken)
                )
                .ReturnsAsync(new NotFoundResult());
        }

        public static IReturnsResult<IBlobStorageService> SetupDownloadBlobText(
            this Mock<IBlobStorageService> service,
            IBlobContainer container,
            string path,
            string blobText,
            CancellationToken cancellationToken = default)
        {
            return service.Setup(s =>
                    s.DownloadBlobText(container, path, cancellationToken))
                .ReturnsAsync(blobText);
        }

        public static IReturnsResult<IBlobStorageService> SetupDownloadBlobTextNotFound(
            this Mock<IBlobStorageService> service,
            IBlobContainer container,
            string path,
            CancellationToken cancellationToken = default)
        {
            return service.Setup(s =>
                    s.DownloadBlobText(container, path, cancellationToken))
                .ThrowsAsync(new FileNotFoundException("Could not find blob"));
        }
    }
}
