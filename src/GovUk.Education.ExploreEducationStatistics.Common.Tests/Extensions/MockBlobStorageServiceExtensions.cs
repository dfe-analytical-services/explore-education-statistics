#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Language.Flow;
using Newtonsoft.Json;

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
            string content,
            bool decompress = true,
            CancellationToken cancellationToken = default)
        {
            return service.SetupDownloadToStream(
                container,
                path,
                Encoding.UTF8.GetBytes(content),
                decompress,
                cancellationToken
            );
        }

        public static IReturnsResult<IBlobStorageService> SetupDownloadToStream(
            this Mock<IBlobStorageService> service,
            IBlobContainer container,
            string path,
            byte[] content,
            bool decompress = true,
            CancellationToken cancellationToken = default)
        {
            return service.Setup(
                    s =>
                        s.DownloadToStream(container, path, It.IsAny<Stream>(), decompress, cancellationToken)
                )
                .Callback<IBlobContainer, string, Stream, bool, CancellationToken?>(
                    (_, _, stream, _, _) =>
                    {
                        stream.Write(content, 0, content.Length);

                        if (stream.CanSeek)
                        {
                            stream.Position = 0;
                        }
                    }
                )
                .ReturnsAsync((IBlobContainer _, string _, Stream stream, bool _, CancellationToken? _) => stream);
        }

        public static IReturnsResult<IBlobStorageService> SetupDownloadToStreamNotFound(
            this Mock<IBlobStorageService> service,
            IBlobContainer container,
            string path,
            bool decompress = true,
            CancellationToken cancellationToken = default)
        {
            return service.Setup(
                    s =>
                        s.DownloadToStream(container, path, It.IsAny<Stream>(), decompress, cancellationToken)
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
                .ReturnsAsync(new NotFoundResult());
        }

        public static IReturnsResult<IBlobStorageService> SetupGetDeserializedJson(
            this Mock<IBlobStorageService> service,
            IBlobContainer container,
            string path,
            object? value,
            Type type,
            JsonSerializerSettings? settings = null,
            CancellationToken cancellationToken = default)
        {
            return service.Setup(s =>
                    s.GetDeserializedJson(container, path, type, settings, cancellationToken))
                .ReturnsAsync(value);
        }

        public static IReturnsResult<IBlobStorageService> SetupGetDeserializedJson<T>(
            this Mock<IBlobStorageService> service,
            IBlobContainer container,
            string path,
            T value,
            JsonSerializerSettings? settings = null,
            CancellationToken cancellationToken = default) where T : class
        {
            return service.Setup(s =>
                    s.GetDeserializedJson<T>(container, path, settings, cancellationToken))
                .ReturnsAsync(value);
        }

        public static IReturnsResult<IBlobStorageService> SetupGetDeserializedJsonNotFound(
            this Mock<IBlobStorageService> service,
            IBlobContainer container,
            string path,
            Type type,
            JsonSerializerSettings? settings = null,
            CancellationToken cancellationToken = default)
        {
            return service.Setup(s =>
                    s.GetDeserializedJson(container, path, type, settings, cancellationToken))
                .ReturnsAsync(new NotFoundResult());
        }

        public static IReturnsResult<IBlobStorageService> SetupGetDeserializedJsonNotFound<T>(
            this Mock<IBlobStorageService> service,
            IBlobContainer container,
            string path,
            JsonSerializerSettings? settings = null,
            CancellationToken cancellationToken = default) where T : class
        {
            return service.Setup(s =>
                    s.GetDeserializedJson<T>(container, path, settings, cancellationToken))
                .ReturnsAsync(new NotFoundResult());
        }

        public static IReturnsResult<IBlobStorageService> SetupGetDeserializedJsonThrows(
            this Mock<IBlobStorageService> service,
            IBlobContainer container,
            string path,
            Type type,
            Exception exception,
            JsonSerializerSettings? settings = null,
            CancellationToken cancellationToken = default)
        {
            return service.Setup(s =>
                    s.GetDeserializedJson(container, path, type, settings, cancellationToken))
                .ThrowsAsync(exception);
        }

        public static IReturnsResult<IBlobStorageService> SetupUploadAsJson<T>(
            this Mock<IBlobStorageService> service,
            IBlobContainer container,
            string path,
            T content,
            string? contentEncoding = null,
            JsonSerializerSettings? settings = null,
            CancellationToken cancellationToken = default)
        {
            return service.Setup(s =>
                    s.UploadAsJson(container, path, content, contentEncoding, settings, cancellationToken))
                .Returns(Task.CompletedTask);
        }
    }
}
