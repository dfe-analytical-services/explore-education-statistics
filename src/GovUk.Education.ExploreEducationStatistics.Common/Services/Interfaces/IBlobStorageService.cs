#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.DataMovement;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces
{
    public interface IBlobStorageService
    {
        public Task<List<BlobInfo>> ListBlobs(IBlobContainer containerName, string? path = null);

        public Task<bool> CheckBlobExists(IBlobContainer containerName, string path);

        public Task<BlobInfo> GetBlob(IBlobContainer containerName, string path);

        public Task<BlobInfo?> FindBlob(IBlobContainer containerName, string path);

        public record DeleteBlobsOptions
        {
            public Regex? ExcludeRegex { get; set; }
            public Regex? IncludeRegex { get; set; }
        }

        public Task DeleteBlobs(
            IBlobContainer containerName,
            string? directoryPath = null,
            DeleteBlobsOptions? options = null);

        public Task DeleteBlob(IBlobContainer containerName, string path);

        public Task<bool> MoveBlob(IBlobContainer containerName, string sourcePath, string destinationPath);

        public Task UploadFile(
            IBlobContainer containerName,
            string path,
            IFormFile file);

        public Task UploadStream(
            IBlobContainer containerName,
            string path,
            Stream stream,
            string contentType);

        public Task UploadAsJson<T>(
            IBlobContainer containerName,
            string path,
            T content,
            JsonSerializerSettings? settings = null);

        /// <summary>
        /// Download the entirety of a blob to a target stream.
        /// </summary>
        /// <param name="containerName">name of the blob container</param>
        /// <param name="path">path to the blob within the container</param>
        /// <param name="stream">stream to output blob to</param>
        /// <param name="cancellationToken">used to cancel the download</param>
        /// <returns>the stream that the blob has been output to</returns>
        public Task<Either<ActionResult, Stream>> DownloadToStream(
            IBlobContainer containerName,
            string path,
            Stream stream,
            CancellationToken? cancellationToken = null);

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
        /// <param name="bufferSize">Size of the stream buffer</param>
        /// <param name="cancellationToken">Token to cancel the request</param>
        /// <returns>The chunked blob stream</returns>
        public Task<Stream> StreamBlob(
            IBlobContainer containerName,
            string path,
            int? bufferSize = null,
            CancellationToken cancellationToken = default);

        public Task<string> DownloadBlobText(
            IBlobContainer containerName,
            string path,
            CancellationToken cancellationToken = default);

        Task<object?> GetDeserializedJson(
            IBlobContainer containerName,
            string path,
            Type target,
            CancellationToken cancellationToken = default);

        Task<T?> GetDeserializedJson<T>(
            IBlobContainer containerName,
            string path,
            CancellationToken cancellationToken = default)
            where T : class;

        public class CopyDirectoryOptions
        {
            public string? DestinationConnectionString { get; set; }
            public SetAttributesCallbackAsync? SetAttributesCallbackAsync { get; set; }
            public ShouldTransferCallbackAsync? ShouldTransferCallbackAsync { get; set; }
            public ShouldOverwriteCallbackAsync? ShouldOverwriteCallbackAsync { get; set; }
        }

        public Task<List<BlobInfo>> CopyDirectory(
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

        public Task MoveDirectory(
            IBlobContainer sourceContainerName,
            string sourceDirectoryPath,
            IBlobContainer destinationContainerName,
            string destinationDirectoryPath,
            MoveDirectoryOptions? options = null);
    }
}
