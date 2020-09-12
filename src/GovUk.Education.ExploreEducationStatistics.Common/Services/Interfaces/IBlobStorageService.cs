using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Storage.DataMovement;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces
{
    public interface IBlobStorageService
    {
        public Task<IEnumerable<BlobInfo>> ListBlobs(string containerName, string path = null);

        public Task<bool> CheckBlobExists(string containerName, string path);

        public Task<BlobInfo> GetBlob(string containerName, string path);

        public Task DeleteBlobs(string containerName, string directoryPath, string excludePattern = null);

        public Task DeleteBlob(string containerName, string path);

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

        public Task<bool> IsAppendSupported(string containerName, string path);

        public class AppendTextOptions
        {
            public IDictionary<string, string> MetaValues { get; set; }
        }

        public Task AppendText(
            string containerName,
            string path,
            string content,
            string contentType = null,
            AppendTextOptions options = null);

        public Task<Stream> StreamBlob(string containerName, string path);

        public Task<string> DownloadBlobText(string containerName, string path);

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