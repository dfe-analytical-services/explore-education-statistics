#nullable enable
using System.Text;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Language.Flow;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class MockBlobStorageServiceExtensions
{
    public static IReturnsResult<T> SetupFindBlob<T>(
        this Mock<T> service,
        IBlobContainer container,
        string path,
        BlobInfo? blob
    )
        where T : class, IBlobStorageService
    {
        return service.Setup(s => s.FindBlob(container, path)).ReturnsAsync(blob);
    }

    public static IReturnsResult<T> SetupCheckBlobExists<T>(
        this Mock<T> service,
        IBlobContainer container,
        string path,
        bool exists
    )
        where T : class, IBlobStorageService
    {
        return service.Setup(s => s.CheckBlobExists(container, path)).ReturnsAsync(exists);
    }

    public static IReturnsResult<T> SetupDeleteBlob<T>(
        this Mock<T> service,
        IBlobContainer container,
        string path
    )
        where T : class, IBlobStorageService
    {
        return service.Setup(s => s.DeleteBlob(container, path)).Returns(Task.CompletedTask);
    }

    public static IReturnsResult<T> SetupGetDownloadStreamWithFilePath<T>(
        this Mock<T> service,
        IBlobContainer container,
        string path,
        string filePathToStream,
        bool decompress = true,
        CancellationToken cancellationToken = default,
        Action? callback = null
    )
        where T : class, IBlobStorageService
    {
        return service
            .Setup(s => s.GetDownloadStream(container, path, decompress, cancellationToken))
            .Callback(callback ?? (() => { }))
            .ReturnsAsync(() => File.OpenRead(filePathToStream));
    }

    public static IReturnsResult<T> SetupGetDownloadStream<T>(
        this Mock<T> service,
        IBlobContainer container,
        string path,
        string content,
        bool decompress = true,
        CancellationToken cancellationToken = default,
        Action? callback = null
    )
        where T : class, IBlobStorageService
    {
        return service.SetupGetDownloadStream(
            container,
            path,
            Encoding.UTF8.GetBytes(content),
            decompress,
            cancellationToken,
            callback
        );
    }

    public static IReturnsResult<T> SetupGetDownloadStream<T>(
        this Mock<T> service,
        IBlobContainer container,
        string path,
        byte[] content,
        bool decompress = true,
        CancellationToken cancellationToken = default,
        Action? callback = null
    )
        where T : class, IBlobStorageService
    {
        return service
            .Setup(s => s.GetDownloadStream(container, path, decompress, cancellationToken))
            .Callback(callback ?? (() => { }))
            .ReturnsAsync(() =>
            {
                var stream = new MemoryStream(content);
                stream.SeekToBeginning();
                return stream;
            });
    }

    public static IReturnsResult<T> SetupGetDownloadStreamNotFound<T>(
        this Mock<T> service,
        IBlobContainer container,
        string path,
        bool decompress = true,
        CancellationToken cancellationToken = default
    )
        where T : class, IBlobStorageService
    {
        return service
            .Setup(s => s.GetDownloadStream(container, path, decompress, cancellationToken))
            .ReturnsAsync(new NotFoundResult());
    }

    public static IReturnsResult<T> SetupGetDownloadToken<T>(
        this Mock<T> service,
        IBlobContainer container,
        string filename,
        string path,
        string contentType,
        CancellationToken cancellationToken = default
    )
        where T : class, IBlobStorageService
    {
        var token = new BlobDownloadToken(
            Token: "token",
            ContainerName: container.Name,
            Path: path,
            Filename: filename,
            ContentType: contentType
        );

        return service
            .Setup(s => s.GetBlobDownloadToken(container, filename, path, cancellationToken))
            .ReturnsAsync(new Either<ActionResult, BlobDownloadToken>(token));
    }

    public static IReturnsResult<T> SetupGetDownloadTokenNotFound<T>(
        this Mock<T> service,
        IBlobContainer container,
        string filename,
        string path,
        CancellationToken cancellationToken = default
    )
        where T : class, IBlobStorageService
    {
        return service
            .Setup(s => s.GetBlobDownloadToken(container, filename, path, cancellationToken))
            .ReturnsAsync(new Either<ActionResult, BlobDownloadToken>(new NotFoundResult()));
    }

    public static IReturnsResult<T> SetupGetDeserializedJson<T>(
        this Mock<T> service,
        IBlobContainer container,
        string path,
        object? value,
        Type type,
        JsonSerializerSettings? settings = null,
        CancellationToken cancellationToken = default
    )
        where T : class, IBlobStorageService
    {
        return service
            .Setup(s => s.GetDeserializedJson(container, path, type, settings, cancellationToken))
            .ReturnsAsync(value);
    }

    public static IReturnsResult<T1> SetupGetDeserializedJson<T1, T2>(
        this Mock<T1> service,
        IBlobContainer container,
        string path,
        T2 value,
        JsonSerializerSettings? settings = null,
        CancellationToken cancellationToken = default
    )
        where T1 : class, IBlobStorageService
        where T2 : class
    {
        return service
            .Setup(s => s.GetDeserializedJson<T2>(container, path, settings, cancellationToken))
            .ReturnsAsync(value);
    }

    public static IReturnsResult<T> SetupGetDeserializedJsonNotFound<T>(
        this Mock<T> service,
        IBlobContainer container,
        string path,
        Type type,
        JsonSerializerSettings? settings = null,
        CancellationToken cancellationToken = default
    )
        where T : class, IBlobStorageService
    {
        return service
            .Setup(s => s.GetDeserializedJson(container, path, type, settings, cancellationToken))
            .ReturnsAsync(new NotFoundResult());
    }

    public static IReturnsResult<T1> SetupGetDeserializedJsonNotFound<T1, T2>(
        this Mock<T1> service,
        IBlobContainer container,
        string path,
        JsonSerializerSettings? settings = null,
        CancellationToken cancellationToken = default
    )
        where T1 : class, IBlobStorageService
        where T2 : class
    {
        return service
            .Setup(s => s.GetDeserializedJson<T2>(container, path, settings, cancellationToken))
            .ReturnsAsync(new NotFoundResult());
    }

    public static IReturnsResult<T> SetupGetDeserializedJsonThrows<T>(
        this Mock<T> service,
        IBlobContainer container,
        string path,
        Type type,
        Exception exception,
        JsonSerializerSettings? settings = null,
        CancellationToken cancellationToken = default
    )
        where T : class, IBlobStorageService
    {
        return service
            .Setup(s => s.GetDeserializedJson(container, path, type, settings, cancellationToken))
            .ThrowsAsync(exception);
    }

    public static IReturnsResult<T1> SetupUploadAsJson<T1, T2>(
        this Mock<T1> service,
        IBlobContainer container,
        string path,
        T2 content,
        string? contentEncoding = null,
        JsonSerializerSettings? settings = null,
        CancellationToken cancellationToken = default
    )
        where T1 : class, IBlobStorageService
    {
        return service
            .Setup(s =>
                s.UploadAsJson(
                    container,
                    path,
                    content,
                    contentEncoding,
                    settings,
                    cancellationToken
                )
            )
            .Returns(Task.CompletedTask);
    }
}
