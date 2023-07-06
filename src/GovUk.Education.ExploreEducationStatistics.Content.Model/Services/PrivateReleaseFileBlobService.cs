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

public class PrivateReleaseFileBlobService : IReleaseFileBlobService
{
    private readonly IPrivateBlobStorageService _privateBlobStorageService;

    public PrivateReleaseFileBlobService(IPrivateBlobStorageService privateBlobStorageService)
    {
        _privateBlobStorageService = privateBlobStorageService;
    }

    public Task<bool> CheckBlobExists(ReleaseFile releaseFile)
    {
        return _privateBlobStorageService.CheckBlobExists(PrivateReleaseFiles, releaseFile.Path());
    }

    public Task<BlobInfo> GetBlob(ReleaseFile releaseFile)
    {
        return _privateBlobStorageService.GetBlob(PrivateReleaseFiles, releaseFile.Path());
    }

    public Task<BlobInfo?> FindBlob(ReleaseFile releaseFile)
    {
        return _privateBlobStorageService.FindBlob(PrivateReleaseFiles, releaseFile.Path());
    }

    public Task DeleteFile(ReleaseFile releaseFile)
    {
        return _privateBlobStorageService.DeleteBlob(PrivateReleaseFiles, releaseFile.Path());
    }

    public Task<bool> MoveBlob(ReleaseFile releaseFile, string destinationPath)
    {
        return _privateBlobStorageService.MoveBlob(
            containerName: PrivateReleaseFiles,
            sourcePath: releaseFile.Path(),
            destinationPath: destinationPath
        );
    }

    public Task<Stream> StreamBlob(
        ReleaseFile releaseFile,
        int? bufferSize = null,
        CancellationToken cancellationToken = default)
    {
        return _privateBlobStorageService.StreamBlob(
            containerName: PrivateReleaseFiles,
            path: releaseFile.Path(),
            cancellationToken: cancellationToken
        );
    }

    public Task<Either<ActionResult, string>> DownloadBlobText(
        ReleaseFile releaseFile,
        CancellationToken cancellationToken = default)
    {
        return _privateBlobStorageService.DownloadBlobText(
            containerName: PrivateReleaseFiles,
            path: releaseFile.Path(),
            cancellationToken: cancellationToken
        );
    }

    public Task<Either<ActionResult, Stream>> DownloadToStream(
        ReleaseFile releaseFile,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        return _privateBlobStorageService.DownloadToStream(
            containerName: PrivateReleaseFiles,
            path: releaseFile.Path(),
            stream: stream,
            cancellationToken: cancellationToken
        );
    }

    public Task<Either<ActionResult, object?>> GetDeserializedJson(ReleaseFile releaseFile, Type type)
    {
        return _privateBlobStorageService.GetDeserializedJson(
            containerName: PrivateReleaseFiles,
            path: releaseFile.Path(),
            type: type
        );
    }

    public Task<Either<ActionResult, T?>> GetDeserializedJson<T>(ReleaseFile releaseFile) where T : class
    {
        return _privateBlobStorageService.GetDeserializedJson<T>(PrivateReleaseFiles, releaseFile.Path());
    }

    public Task UploadFile(ReleaseFile releaseFile, IFormFile file)
    {
        return _privateBlobStorageService.UploadFile(
            containerName: PrivateReleaseFiles,
            path: releaseFile.Path(),
            file: file
        );
    }

    public Task UploadStream(ReleaseFile releaseFile, Stream stream, string contentType)
    {
        return _privateBlobStorageService.UploadStream(
            containerName: PrivateReleaseFiles,
            path: releaseFile.Path(),
            stream: stream,
            contentType: contentType
        );
    }

    public Task UploadAsJson<T>(ReleaseFile releaseFile, T content, JsonSerializerSettings? settings = null)
    {
        return _privateBlobStorageService.UploadAsJson(
            containerName: PrivateReleaseFiles,
            path: releaseFile.Path(),
            content: content,
            settings: settings
        );
    }
}
