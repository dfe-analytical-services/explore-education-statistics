using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.DataMovement;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

public interface IBlobStorageService
{
    /// <summary>
    /// Retrieve a list of blob paths within the specified container, optionally filtered by a directory prefix.
    /// </summary>
    /// <param name="containerName"></param>
    /// <param name="prefixFilter">
    /// Filter out blobs from the result set whose keys do not start with this value. Where a blob key would
    /// ordinarily start with a GUID, for example with releases, this GUID is excluded from the matching function.
    /// </param>
    /// <param name="cancellationToken"></param>
    Task<List<string>> GetBlobs(
        IBlobContainer containerName,
        string? prefixFilter = null,
        CancellationToken cancellationToken = default
    );

    Task<bool> CheckBlobExists(IBlobContainer containerName, string path);

    Task<BlobInfo?> FindBlob(IBlobContainer containerName, string path);

    public record DeleteBlobsOptions
    {
        public Regex? ExcludeRegex { get; set; }
        public Regex? IncludeRegex { get; set; }
    }

    Task DeleteBlobs(IBlobContainer containerName, string? directoryPath = null, DeleteBlobsOptions? options = null);

    Task DeleteBlob(IBlobContainer containerName, string path);

    Task<bool> MoveBlob(
        IBlobContainer sourceContainer,
        string sourcePath,
        string destinationPath,
        IBlobContainer? destinationContainer = null
    );

    Task UploadFile(IBlobContainer containerName, string path, IFormFile file);

    Task UploadStream(
        IBlobContainer containerName,
        string path,
        Stream sourceStream,
        string contentType,
        string? contentEncoding = null,
        CancellationToken cancellationToken = default
    );

    Task UploadAsJson<T>(
        IBlobContainer containerName,
        string path,
        T content,
        string? contentEncoding = null,
        JsonSerializerSettings? settings = null,
        CancellationToken cancellationToken = default
    );

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
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Obtain a secure,short-lived download token for use with <see cref="StreamWithToken"/>.
    /// Any relevant permission checks for the current business process situation should be
    /// carried out prior to obtaining this token.
    /// </summary>
    /// <param name="container">The blob container</param>
    /// <param name="filename">The requested filename for the download token to include,
    /// for use when performing the actual download with <see cref="StreamWithToken"/>
    /// </param>
    /// <param name="path">Path to the blob within the container</param>
    /// <param name="cancellationToken">Token to cancel the request</param>
    /// <returns>
    /// A <see cref="BlobDownloadToken"/> that can be used with <see cref="StreamWithToken"/>
    /// to stream a file securely from Blob Storage using SAS.
    /// </returns>
    Task<Either<ActionResult, BlobDownloadToken>> GetBlobDownloadToken(
        IBlobContainer container,
        string filename,
        string path,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Use a secure, short-lived <see cref="BlobDownloadToken"/> obtained from
    /// <see cref="GetBlobDownloadToken"/> to stream a Blob from storage.
    /// </summary>
    /// <param name="token">
    /// A secure, short-lived <see cref="BlobDownloadToken"/> obtained from <see cref="GetBlobDownloadToken"/>
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Either<ActionResult, FileStreamResult>> StreamWithToken(
        BlobDownloadToken token,
        CancellationToken cancellationToken
    );

    Task<Either<ActionResult, string>> DownloadBlobText(
        IBlobContainer containerName,
        string path,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, object?>> GetDeserializedJson(
        IBlobContainer containerName,
        string path,
        Type type,
        JsonSerializerSettings? settings = null,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, T?>> GetDeserializedJson<T>(
        IBlobContainer containerName,
        string path,
        JsonSerializerSettings? settings = null,
        CancellationToken cancellationToken = default
    )
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
        CopyDirectoryOptions? options = null
    );

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
        MoveDirectoryOptions? options = null
    );
}
