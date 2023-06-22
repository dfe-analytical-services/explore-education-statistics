#nullable enable
using System;
using System.IO;
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
        public static IReturnsResult<T> SetupFindBlob<T>(
            this Mock<T> service,
            IBlobContainer container,
            string path,
            BlobInfo? blob) where T : class, IBlobStorageService
        {
            return service.Setup(s => s.FindBlob(container, path))
                .ReturnsAsync(blob);
        }

        public static IReturnsResult<T> SetupStreamBlob<T>(
            this Mock<T> service,
            IBlobContainer container,
            string expectedBlobPath,
            string filePathToStream) where T : class, IBlobStorageService
        {
            return service.Setup(s => s.StreamBlob(container, expectedBlobPath, null, default))
                .ReturnsAsync(() => File.OpenRead(filePathToStream));
        }

        public static IReturnsResult<T> SetupCheckBlobExists<T>(
            this Mock<T> service,
            IBlobContainer container,
            string path,
            bool exists) where T : class, IBlobStorageService
        {
            return service.Setup(s => s.CheckBlobExists(container, path))
                .ReturnsAsync(exists);
        }

        public static IReturnsResult<T> SetupDeleteBlob<T>(
            this Mock<T> service,
            IBlobContainer container,
            string path) where T : class, IBlobStorageService
        {
            return service.Setup(s => s.DeleteBlob(container, path))
                .Returns(Task.CompletedTask);
        }

        public static IReturnsResult<T> SetupDownloadToStream<T>(
            this Mock<T> service,
            IBlobContainer container,
            string path,
            string content,
            bool decompress = true,
            CancellationToken cancellationToken = default) where T : class, IBlobStorageService
        {
            return service.SetupDownloadToStream(
                container,
                path,
                Encoding.UTF8.GetBytes(content),
                decompress,
                cancellationToken
            );
        }

        public static IReturnsResult<T> SetupDownloadToStream<T>(
            this Mock<T> service,
            IBlobContainer container,
            string path,
            byte[] content,
            bool decompress = true,
            CancellationToken cancellationToken = default) where T : class, IBlobStorageService
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

        public static IReturnsResult<T> SetupDownloadToStreamNotFound<T>(
            this Mock<T> service,
            IBlobContainer container,
            string path,
            bool decompress = true,
            CancellationToken cancellationToken = default) where T : class, IBlobStorageService
        {
            return service.Setup(
                    s =>
                        s.DownloadToStream(container, path, It.IsAny<Stream>(), decompress, cancellationToken)
                )
                .ReturnsAsync(new NotFoundResult());
        }

        public static IReturnsResult<T> SetupGetDeserializedJson<T>(
            this Mock<T> service,
            IBlobContainer container,
            string path,
            object? value,
            Type type,
            JsonSerializerSettings? settings = null,
            CancellationToken cancellationToken = default) where T : class, IBlobStorageService
        {
            return service.Setup(s =>
                    s.GetDeserializedJson(container, path, type, settings, cancellationToken))
                .ReturnsAsync(value);
        }

        public static IReturnsResult<T1> SetupGetDeserializedJson<T1, T2>(
            this Mock<T1> service,
            IBlobContainer container,
            string path,
            T2 value,
            JsonSerializerSettings? settings = null,
            CancellationToken cancellationToken = default)
        where T1 : class, IBlobStorageService
        where T2 : class
        {
            return service.Setup(s =>
                    s.GetDeserializedJson<T2>(container, path, settings, cancellationToken))
                .ReturnsAsync(value);
        }

        public static IReturnsResult<T> SetupGetDeserializedJsonNotFound<T>(
            this Mock<T> service,
            IBlobContainer container,
            string path,
            Type type,
            JsonSerializerSettings? settings = null,
            CancellationToken cancellationToken = default) where T : class, IBlobStorageService
        {
            return service.Setup(s =>
                    s.GetDeserializedJson(container, path, type, settings, cancellationToken))
                .ReturnsAsync(new NotFoundResult());
        }

        public static IReturnsResult<T1> SetupGetDeserializedJsonNotFound<T1, T2>(
            this Mock<T1> service,
            IBlobContainer container,
            string path,
            JsonSerializerSettings? settings = null,
            CancellationToken cancellationToken = default)
        where T1 : class, IBlobStorageService
        where T2 : class
        {
            return service.Setup(s =>
                    s.GetDeserializedJson<T2>(container, path, settings, cancellationToken))
                .ReturnsAsync(new NotFoundResult());
        }

        public static IReturnsResult<T> SetupGetDeserializedJsonThrows<T>(
            this Mock<T> service,
            IBlobContainer container,
            string path,
            Type type,
            Exception exception,
            JsonSerializerSettings? settings = null,
            CancellationToken cancellationToken = default) where T : class, IBlobStorageService
        {
            return service.Setup(s =>
                    s.GetDeserializedJson(container, path, type, settings, cancellationToken))
                .ThrowsAsync(exception);
        }

        public static IReturnsResult<T1> SetupUploadAsJson<T1, T2>(
            this Mock<T1> service,
            IBlobContainer container,
            string path,
            T2 content,
            string? contentEncoding = null,
            JsonSerializerSettings? settings = null,
            CancellationToken cancellationToken = default) where T1 : class, IBlobStorageService
        {
            return service.Setup(s =>
                    s.UploadAsJson(container, path, content, contentEncoding, settings, cancellationToken))
                .Returns(Task.CompletedTask);
        }
    }
}
