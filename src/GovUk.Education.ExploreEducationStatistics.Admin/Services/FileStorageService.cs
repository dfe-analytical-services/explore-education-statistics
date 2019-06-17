using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using MimeTypes;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _storageConnectionString;
        private readonly ILogger _logger;

        private const string containerName = "releases";

        private enum FileSizeUnit : byte
        {
            B,
            Kb,
            Mb,
            Gb,
            Tb
        }

        public FileStorageService(ILogger<FileStorageService> logger,
            IConfiguration config)
        {
            _logger = logger;
            _storageConnectionString = config.GetConnectionString("AzureStorage");
        }

        public async Task UploadFileAsync(string publication, string release, string filename, string path, string name)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);

            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(containerName);
            await blobContainer.CreateIfNotExistsAsync();

            var permissions = new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            };
            await blobContainer.SetPermissionsAsync(permissions);

            var blob = blobContainer.GetBlockBlobReference($"{publication}/{release}/{filename}");
            await blob.UploadFromFileAsync(path);
            await AddNameMetadataAsync(blob, name);
        }

        public IEnumerable<FileInfo> ListFiles(string publication, string release)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);

            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(containerName);
            blobContainer.CreateIfNotExists();

            return blobContainer.ListBlobs($"{publication}/{release}", true, BlobListingDetails.Metadata)
                .OfType<CloudBlockBlob>()
                .Select(file => new FileInfo
                {
                    Extension = GetExtension(file),
                    Name = GetName(file),
                    Path = file.Name,
                    Size = GetSize(file)
                })
                .OrderBy(info => info.Name);
        }

        private static string GetExtension(CloudBlob blob)
        {
            return MimeTypeMap.GetExtension(blob.Properties.ContentType).TrimStart('.');
        }

        private static string GetName(CloudBlob blob)
        {
            return blob.Metadata.TryGetValue("name", out var name) ? name : "Unknown file name";
        }

        private static string GetSize(CloudBlob blob)
        {
            var fileSize = blob.Properties.Length;
            var unit = FileSizeUnit.B;
            while (fileSize >= 1024 && unit < FileSizeUnit.Tb)
            {
                fileSize /= 1024;
                unit++;
            }

            return $"{fileSize:0.##} {unit}";
        }

        private static async Task AddNameMetadataAsync(CloudBlob blob, string name)
        {
            blob.Metadata.Add("name", name);
            await blob.SetMetadataAsync();
        }
    }
}