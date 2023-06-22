#nullable enable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Services;

public class PublicReleaseFileBlobService : IReleaseFileBlobService
{
    private readonly IPublicBlobStorageService _publicBlobStorageService;

    public PublicReleaseFileBlobService(IPublicBlobStorageService publicBlobStorageService)
    {
        _publicBlobStorageService = publicBlobStorageService;
    }

    public Task<bool> CheckBlobExists(ReleaseFile releaseFile)
    {
        return _publicBlobStorageService.CheckBlobExists(PublicReleaseFiles, releaseFile.PublicPath());
    }

    public Task<BlobInfo> GetBlob(ReleaseFile releaseFile)
    {
        return _publicBlobStorageService.GetBlob(PublicReleaseFiles, releaseFile.PublicPath());
    }

    public Task<BlobInfo?> FindBlob(ReleaseFile releaseFile)
    {
        return _publicBlobStorageService.FindBlob(PublicReleaseFiles, releaseFile.PublicPath());
    }

    public Task DeleteFile(ReleaseFile releaseFile)
    {
        return _publicBlobStorageService.DeleteBlob(PublicReleaseFiles, releaseFile.PublicPath());
    }

    public Task<bool> MoveBlob(ReleaseFile releaseFile, string destinationPath)
    {
        return _publicBlobStorageService.MoveBlob(
            containerName: PublicReleaseFiles,
            sourcePath: releaseFile.PublicPath(),
            destinationPath: destinationPath);
    }

    public Task<Stream> StreamBlob(
        ReleaseFile releaseFile,
        int? bufferSize = null,
        CancellationToken cancellationToken = default)
    {
        return _publicBlobStorageService.StreamBlob(
            containerName: PublicReleaseFiles,
            path: releaseFile.PublicPath(),
            cancellationToken: cancellationToken
        );
    }

    public Task<Either<ActionResult, string>> DownloadBlobText(
        ReleaseFile releaseFile,
        CancellationToken cancellationToken = default)
    {
        return _publicBlobStorageService.DownloadBlobText(
            containerName: PublicReleaseFiles,
            path: releaseFile.PublicPath(),
            cancellationToken: cancellationToken
        );
    }

    public Task<Either<ActionResult, Stream>> DownloadToStream(
        ReleaseFile releaseFile,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        return _publicBlobStorageService.DownloadToStream(
            containerName: PublicReleaseFiles,
            path: releaseFile.PublicPath(),
            stream: stream,
            cancellationToken: cancellationToken
        );
    }

    public Task<Either<ActionResult, object?>> GetDeserializedJson(ReleaseFile releaseFile, Type type)
    {
        return _publicBlobStorageService.GetDeserializedJson(
            containerName: PublicReleaseFiles,
            path: releaseFile.PublicPath(),
            type: type);
    }

    public Task<Either<ActionResult, T?>> GetDeserializedJson<T>(ReleaseFile releaseFile) where T : class
    {
        return _publicBlobStorageService.GetDeserializedJson<T>(PublicReleaseFiles, releaseFile.PublicPath());
    }

    public Task UploadFile(ReleaseFile releaseFile, IFormFile file)
    {
        return _publicBlobStorageService.UploadFile(
            containerName: PublicReleaseFiles,
            path: releaseFile.PublicPath(),
            file: file);
    }

    public Task UploadStream(ReleaseFile releaseFile, Stream stream, string contentType)
    {
        return _publicBlobStorageService.UploadStream(
            containerName: PublicReleaseFiles,
            path: releaseFile.PublicPath(),
            stream: stream,
            contentType: contentType);
    }

    public Task UploadAsJson<T>(ReleaseFile releaseFile, T content, JsonSerializerSettings? settings = null)
    {
        return _publicBlobStorageService.UploadAsJson(
            containerName: PublicReleaseFiles,
            path: releaseFile.PublicPath(),
            content: content,
            settings: settings);
    }
}
