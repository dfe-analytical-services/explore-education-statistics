#nullable enable
using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.DataMovement;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

public interface IBlobStorageService
{
    Task<bool> CheckBlobExists(IBlobContainer containerName, string path);

    Task<BlobInfo> GetBlob(IBlobContainer containerName, string path);

    Task<BlobInfo?> FindBlob(IBlobContainer containerName, string path);

    public record DeleteBlobsOptions
    {
        public Regex? ExcludeRegex { get; set; }
        public Regex? IncludeRegex { get; set; }
    }

    Task DeleteBlobs(
        IBlobContainer containerName,
        string? directoryPath = null,
        DeleteBlobsOptions? options = null);

    Task DeleteBlob(IBlobContainer containerName, string path);

    Task<bool> MoveBlob(
        IBlobContainer sourceContainer,
        string sourcePath,
        string destinationPath,
        IBlobContainer? destinationContainer = null);

    Task UploadFile(
        IBlobContainer containerName,
        string path,
        IFormFile file);

    Task UploadStream(
        IBlobContainer containerName,
        string path,
        Stream sourceStream,
        string contentType,
        string? contentEncoding = null,
        CancellationToken cancellationToken = default);

    Task UploadAsJson<T>(
        IBlobContainer containerName,
        string path,
        T content,
        string? contentEncoding = null,
        JsonSerializerSettings? settings = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Download the entirety of a blob to a target stream.
    /// </summary>
    /// <param name="containerName">name of the blob container</param>
    /// <param name="path">path to the blob within the container</param>
    /// <param name="stream">stream to output blob to</param>
    /// <param name="decompress">if true, checks the content encoding and decompresses the blob if necessary</param>
    /// <param name="cancellationToken">used to cancel the download</param>
    /// <returns>the stream that the blob has been output to</returns>
    [Obsolete("Use GetDownloadStream instead to create a download stream with no intermediary Streams necessary.")]
    Task<Either<ActionResult, Stream>> DownloadToStream(
        IBlobContainer containerName,
        string path,
        Stream stream,
        bool decompress = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a download Stream to fetch the entirety of a blob, with no intermediary Streams necessary.
    /// </summary>
    /// <param name="containerName">name of the blob container</param>
    /// <param name="path">path to the blob within the container</param>
    /// <param name="decompress">if true, checks the content encoding and decompresses the blob if necessary</param>
    /// <param name="cancellationToken">used to cancel the download</param>
    /// <returns>the stream via which the blob will be downloaded</returns>
    Task<Either<ActionResult, Stream>> GetDownloadStream(
        IBlobContainer containerName,
        string path,
        bool decompress = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stream a blob in chunks.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This differs from <see cref="DownloadToStream">DownloadToStream</see> in
    /// that it does not download the entirety of the blob beforehand. This causes
    /// differences in how the outputted stream behaves that you may or may not want.
    /// </para>
    /// </remarks>
    /// <param name="containerName">Name of the blob container</param>
    /// <param name="path">Path to the blob within the container</param>
    /// <param name="cancellationToken">Token to cancel the request</param>
    /// <returns>The chunked blob stream</returns>
    Task<Stream> StreamBlob(
        IBlobContainer containerName,
        string path,
        CancellationToken cancellationToken = default);
    
    Task<Either<ActionResult, FileStreamResult>> StreamWithToken(
        BlobDownloadToken token,
        CancellationToken cancellationToken);
    
    Task<Either<ActionResult, BlobDownloadToken>> GetBlobDownloadToken(
        IBlobContainer containerName,
        string filename,
        string path);

    Task<Either<ActionResult, string>> DownloadBlobText(
        IBlobContainer containerName,
        string path,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, object?>> GetDeserializedJson(
        IBlobContainer containerName,
        string path,
        Type type,
        JsonSerializerSettings? settings = null,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, T?>> GetDeserializedJson<T>(
        IBlobContainer containerName,
        string path,
        JsonSerializerSettings? settings = null,
        CancellationToken cancellationToken = default)
        where T : class;

    public class CopyDirectoryOptions
    {
        public string? DestinationConnectionString { get; set; }
        public SetAttributesCallbackAsync? SetAttributesCallbackAsync { get; set; }
        public ShouldTransferCallbackAsync? ShouldTransferCallbackAsync { get; set; }
        public ShouldOverwriteCallbackAsync? ShouldOverwriteCallbackAsync { get; set; }
    }

    Task<List<BlobInfo>> CopyDirectory(
        IBlobContainer sourceContainerName,
        string sourceDirectoryPath,
        IBlobContainer destinationContainerName,
        string destinationDirectoryPath,
        CopyDirectoryOptions? options = null);

    public class MoveDirectoryOptions
    {
        public string? DestinationConnectionString { get; set; }
        public SetAttributesCallbackAsync? SetAttributesCallbackAsync { get; set; }
        public ShouldTransferCallbackAsync? ShouldTransferCallbackAsync { get; set; }
        public ShouldOverwriteCallbackAsync? ShouldOverwriteCallbackAsync { get; set; }
    }

    Task MoveDirectory(
        IBlobContainer sourceContainerName,
        string sourceDirectoryPath,
        IBlobContainer destinationContainerName,
        string destinationDirectoryPath,
        MoveDirectoryOptions? options = null);
}
