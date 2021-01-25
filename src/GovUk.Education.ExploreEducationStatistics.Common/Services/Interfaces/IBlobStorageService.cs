using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Storage.DataMovement;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces
{
    public interface IBlobStorageService
    {
        public Task<IEnumerable<BlobInfo>> ListBlobs(string containerName, string path = null);

        public Task<bool> CheckBlobExists(string containerName, string path);

        public Task<BlobInfo> GetBlob(string containerName, string path);

        public Task DeleteBlobs(string containerName, string directoryPath, string excludePattern = null);

        public Task DeleteBlob(string containerName, string path);

        public Task<bool> MoveBlob(string containerName, string sourcePath, string destinationPath);

        public class UploadFileOptions
        {
            public IDictionary<string, string> MetaValues { get; set; }
        }

        public Task UploadFile(
            string containerName,
            string path,
            IFormFile file,
            UploadFileOptions options = null);

        public class UploadStreamOptions
        {
            public IDictionary<string, string> MetaValues { get; set; }
        }

        public Task UploadStream(
            string containerName,
            string path,
            Stream stream,
            string contentType,
            UploadStreamOptions options = null);

        public class UploadTextOptions
        {
            public IDictionary<string, string> MetaValues { get; set; }
        }

        public Task UploadText(
            string containerName,
            string path,
            string content,
            string contentType,
            UploadTextOptions options = null);

        public Task UploadAsJson<T>(
            string containerName,
            string path,
            T content,
            JsonSerializerSettings settings = null);

        public Task<bool> IsAppendSupported(string containerName, string path);

        public Task AppendText(string containerName, string path, string content);

        /// <summary>
        /// Download the entirety of a blob to a target stream.
        /// </summary>
        /// <param name="containerName">name of the blob container</param>
        /// <param name="path">path to the blob within the container</param>
        /// <param name="stream">stream to output blob to</param>
        /// <returns>the blob stream</returns>
        public Task<Stream> DownloadToStream(string containerName, string path, Stream stream);

        public Task SetMetadata(string containerName, string path, IDictionary<string, string> metadata);

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
        public Task<Stream> StreamBlob(string containerName, string path, int? bufferSize = null);

        public Task<string> DownloadBlobText(string containerName, string path);

        Task<T> GetDeserializedJson<T>(string containerName, string path);

        public class CopyDirectoryOptions
        {
            public string DestinationConnectionString { get; set; }
            public SetAttributesCallbackAsync SetAttributesCallbackAsync { get; set; }
            public ShouldTransferCallbackAsync ShouldTransferCallbackAsync { get; set; }
            public ShouldOverwriteCallbackAsync ShouldOverwriteCallbackAsync { get; set; }
        }

        public Task<List<BlobInfo>> CopyDirectory(
            string sourceContainerName,
            string sourceDirectoryPath,
            string destinationContainerName,
            string destinationDirectoryPath,
            CopyDirectoryOptions options = null);

        public class MoveDirectoryOptions
        {
            public string DestinationConnectionString { get; set; }
            public SetAttributesCallbackAsync SetAttributesCallbackAsync { get; set; }
            public ShouldTransferCallbackAsync ShouldTransferCallbackAsync { get; set; }
            public ShouldOverwriteCallbackAsync ShouldOverwriteCallbackAsync { get; set; }
        }

        public Task MoveDirectory(
            string sourceContainerName,
            string sourceDirectoryPath,
            string destinationContainerName,
            string destinationDirectoryPath,
            MoveDirectoryOptions options = null);
    }
}