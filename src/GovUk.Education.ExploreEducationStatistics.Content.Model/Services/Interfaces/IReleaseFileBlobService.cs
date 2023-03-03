#nullable enable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;

public interface IReleaseFileBlobService
{
    Task<bool> CheckBlobExists(ReleaseFile releaseFile);

    Task<BlobInfo> GetBlob(ReleaseFile releaseFile);

    Task<BlobInfo?> FindBlob(ReleaseFile releaseFile);

    Task DeleteFile(ReleaseFile releaseFile);

    Task<bool> MoveBlob(ReleaseFile releaseFile, string destinationPath);

    Task<Stream> StreamBlob(
        ReleaseFile releaseFile,
        int? bufferSize = null,
        CancellationToken cancellationToken = default);

    Task<string> DownloadBlobText(
        ReleaseFile releaseFile,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, Stream>> DownloadToStream(
        ReleaseFile releaseFile,
        Stream stream,
        CancellationToken? cancellationToken = null);

    Task<object?> GetDeserializedJson(ReleaseFile releaseFile, Type target);

    Task<T?> GetDeserializedJson<T>(ReleaseFile releaseFile) where T : class;

    Task UploadFile(
        ReleaseFile releaseFile,
        IFormFile file);

    Task UploadStream(
        ReleaseFile releaseFile,
        Stream stream,
        string contentType);

    Task UploadAsJson<T>(
        ReleaseFile releaseFile,
        T content,
        JsonSerializerSettings? settings = null);

}
