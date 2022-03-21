#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
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
            IFormFile file,
            IDictionary<string, string>? metadata = null);

        public Task UploadStream(
            IBlobContainer containerName,
            string path,
            Stream stream,
            string contentType,
            IDictionary<string, string>? metadata = null);

        public Task UploadText(
            IBlobContainer containerName,
            string path,
            string content,
            string contentType,
            IDictionary<string, string>? metadata = null);

        public Task UploadAsJson<T>(
            IBlobContainer containerName,
            string path,
            T content,
            JsonSerializerSettings? settings = null);

        public Task<bool> IsAppendSupported(IBlobContainer containerName, string path);

        public Task AppendText(IBlobContainer containerName, string path, string content);

        /// <summary>
        /// Download the entirety of a blob to a target stream.
        /// </summary>
        /// <param name="containerName">name of the blob container</param>
        /// <param name="path">path to the blob within the container</param>
        /// <param name="stream">stream to output blob to</param>
        /// <param name="cancellationToken">used to cancel the download</param>
        /// <returns>the stream that the blob has been output to</returns>
        public Task<Stream> DownloadToStream(
            IBlobContainer containerName,
            string path,
            Stream stream,
            CancellationToken? cancellationToken = null);

        public Task SetMetadata(IBlobContainer containerName, string path, IDictionary<string, string> metadata);

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
        /// <param name="containerName">name of the blob container</param>
        /// <param name="path">path to the blob within the container</param>
        /// <param name="bufferSize">size of the stream buffer</param>
        /// <returns>the chunked blob stream</returns>
        public Task<Stream> StreamBlob(IBlobContainer containerName, string path, int? bufferSize = null);

        public Task<string> DownloadBlobText(IBlobContainer containerName, string path);

        Task<object?> GetDeserializedJson(IBlobContainer containerName, string path, Type target);

        Task<T?> GetDeserializedJson<T>(IBlobContainer containerName, string path)
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
