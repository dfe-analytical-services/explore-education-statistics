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
        public static IReturnsResult<IPublicBlobStorageService> SetupFindBlob(
            this Mock<IPublicBlobStorageService> service,
            IBlobContainer container,
            string path,
            BlobInfo? blob)
        {
            return service.Setup(s => s.FindBlob(container, path))
                .ReturnsAsync(blob);
        }

        public static IReturnsResult<IPrivateBlobStorageService> SetupStreamBlob(
            this Mock<IPrivateBlobStorageService> service,
            IBlobContainer container,
            string expectedBlobPath,
            string filePathToStream)
        {
            return service.Setup(s => s.StreamBlob(container, expectedBlobPath, null, default))
                .ReturnsAsync(() => File.OpenRead(filePathToStream));
        }

        public static IReturnsResult<IPublicBlobStorageService> SetupCheckBlobExists(
            this Mock<IPublicBlobStorageService> service,
            IBlobContainer container,
            string path,
            bool exists)
        {
            return service.Setup(s => s.CheckBlobExists(container, path))
                .ReturnsAsync(exists);
        }

        public static IReturnsResult<IPrivateBlobStorageService> SetupCheckBlobExists(
            this Mock<IPrivateBlobStorageService> service,
            IBlobContainer container,
            string path,
            bool exists)
        {
            return service.Setup(s => s.CheckBlobExists(container, path))
                .ReturnsAsync(exists);
        }

        public static IReturnsResult<IPublicBlobStorageService> SetupDeleteBlob(
            this Mock<IPublicBlobStorageService> service,
            IBlobContainer container,
            string path)
        {
            return service.Setup(s => s.DeleteBlob(container, path))
                .Returns(Task.CompletedTask);
        }

        public static IReturnsResult<IPrivateBlobStorageService> SetupDeleteBlob(
            this Mock<IPrivateBlobStorageService> service,
            IBlobContainer container,
            string path)
        {
            return service.Setup(s => s.DeleteBlob(container, path))
                .Returns(Task.CompletedTask);
        }

        public static IReturnsResult<IPublicBlobStorageService> SetupDownloadToStream(
            this Mock<IPublicBlobStorageService> service,
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

        public static IReturnsResult<IPrivateBlobStorageService> SetupDownloadToStream(
            this Mock<IPrivateBlobStorageService> service,
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

        public static IReturnsResult<IPublicBlobStorageService> SetupDownloadToStream(
            this Mock<IPublicBlobStorageService> service,
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

        public static IReturnsResult<IPrivateBlobStorageService> SetupDownloadToStream(
            this Mock<IPrivateBlobStorageService> service,
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

        public static IReturnsResult<IPublicBlobStorageService> SetupDownloadToStreamNotFound(
            this Mock<IPublicBlobStorageService> service,
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

        public static IReturnsResult<IPrivateBlobStorageService> SetupDownloadToStreamNotFound(
            this Mock<IPrivateBlobStorageService> service,
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

        public static IReturnsResult<IPublicBlobStorageService> SetupGetDeserializedJson(
            this Mock<IPublicBlobStorageService> service,
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

        public static IReturnsResult<IPublicBlobStorageService> SetupGetDeserializedJson<T>(
            this Mock<IPublicBlobStorageService> service,
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

        public static IReturnsResult<IPublicBlobStorageService> SetupGetDeserializedJsonNotFound(
            this Mock<IPublicBlobStorageService> service,
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

        public static IReturnsResult<IPublicBlobStorageService> SetupGetDeserializedJsonNotFound<T>(
            this Mock<IPublicBlobStorageService> service,
            IBlobContainer container,
            string path,
            JsonSerializerSettings? settings = null,
            CancellationToken cancellationToken = default) where T : class
        {
            return service.Setup(s =>
                    s.GetDeserializedJson<T>(container, path, settings, cancellationToken))
                .ReturnsAsync(new NotFoundResult());
        }

        public static IReturnsResult<IPublicBlobStorageService> SetupGetDeserializedJsonThrows(
            this Mock<IPublicBlobStorageService> service,
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

        public static IReturnsResult<IPublicBlobStorageService> SetupUploadAsJson<T>(
            this Mock<IPublicBlobStorageService> service,
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
