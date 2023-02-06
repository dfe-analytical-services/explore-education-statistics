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
    private readonly IBlobStorageService _blobStorageService;

    public PublicReleaseFileBlobService(IBlobStorageService blobStorageService)
    {
        _blobStorageService = blobStorageService;
    }

    public Task<bool> CheckBlobExists(ReleaseFile releaseFile)
    {
        return _blobStorageService.CheckBlobExists(PublicReleaseFiles, releaseFile.PublicPath());
    }

    public Task<BlobInfo> GetBlob(ReleaseFile releaseFile)
    {
        return _blobStorageService.GetBlob(PublicReleaseFiles, releaseFile.PublicPath());
    }

    public Task<BlobInfo?> FindBlob(ReleaseFile releaseFile)
    {
        return _blobStorageService.FindBlob(PublicReleaseFiles, releaseFile.PublicPath());
    }

    public Task DeleteFile(ReleaseFile releaseFile)
    {
        return _blobStorageService.DeleteBlob(PublicReleaseFiles, releaseFile.PublicPath());
    }

    public Task<bool> MoveBlob(ReleaseFile releaseFile, string destinationPath)
    {
        return _blobStorageService.MoveBlob(
            containerName: PublicReleaseFiles,
            sourcePath: releaseFile.PublicPath(),
            destinationPath: destinationPath);
    }

    public Task<Stream> StreamBlob(
        ReleaseFile releaseFile,
        int? bufferSize = null,
        CancellationToken cancellationToken = default)
    {
        return _blobStorageService.StreamBlob(
            containerName: PublicReleaseFiles,
            path: releaseFile.PublicPath(),
            cancellationToken: cancellationToken
        );
    }

    public Task<string> DownloadBlobText(
        ReleaseFile releaseFile,
        CancellationToken cancellationToken = default)
    {
        return _blobStorageService.DownloadBlobText(
            containerName: PublicReleaseFiles,
            path: releaseFile.PublicPath(),
            cancellationToken: cancellationToken
        );
    }

    public Task<Either<ActionResult, Stream>> DownloadToStream(
        ReleaseFile releaseFile,
        Stream stream,
        CancellationToken? cancellationToken = null)
    {
        return _blobStorageService.DownloadToStream(
            containerName: PublicReleaseFiles,
            path: releaseFile.PublicPath(),
            stream: stream,
            cancellationToken: cancellationToken
        );
    }

    public Task<object?> GetDeserializedJson(ReleaseFile releaseFile, Type target)
    {
        return _blobStorageService.GetDeserializedJson(
            containerName: PublicReleaseFiles,
            path: releaseFile.PublicPath(),
            target: target);
    }

    public Task<T?> GetDeserializedJson<T>(ReleaseFile releaseFile) where T : class
    {
        return _blobStorageService.GetDeserializedJson<T>(PublicReleaseFiles, releaseFile.PublicPath());
    }

    public Task UploadFile(ReleaseFile releaseFile, IFormFile file)
    {
        return _blobStorageService.UploadFile(
            containerName: PublicReleaseFiles,
            path: releaseFile.PublicPath(),
            file: file);
    }

    public Task UploadStream(ReleaseFile releaseFile, Stream stream, string contentType)
    {
        return _blobStorageService.UploadStream(
            containerName: PublicReleaseFiles,
            path: releaseFile.PublicPath(),
            stream: stream,
            contentType: contentType);
    }

    public Task UploadAsJson<T>(ReleaseFile releaseFile, T content, JsonSerializerSettings? settings = null)
    {
        return _blobStorageService.UploadAsJson(
            containerName: PublicReleaseFiles,
            path: releaseFile.PublicPath(),
            content: content,
            settings: settings);
    }
}
