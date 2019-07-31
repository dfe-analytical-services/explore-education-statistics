using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using MimeTypes;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Admin.Models.FileInfo;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _storageConnectionString;

        private const string ContainerName = "releases";

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private enum FileSizeUnit : byte
        {
            B,
            Kb,
            Mb,
            Gb,
            Tb
        }

        public FileStorageService(IConfiguration config)
        {
            _storageConnectionString = config.GetConnectionString("CoreStorage");
        }

        public async Task UploadFilesAsync(Guid releaseId, IFormFile dataFile, IFormFile metaFile,
            string name)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);

            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(ContainerName);
            await blobContainer.CreateIfNotExistsAsync();

            var permissions = new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            };
            await blobContainer.SetPermissionsAsync(permissions);

            await UploadFileAsync(blobContainer, releaseId, dataFile, new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("name", name),
                new KeyValuePair<string, string>("metafile", metaFile.FileName)
            });
            await UploadFileAsync(blobContainer, releaseId, metaFile, new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("datafile", dataFile.FileName)
            });
        }

        private static async Task UploadFileAsync(CloudBlobContainer blobContainer, Guid releaseId,
            IFormFile file, IEnumerable<KeyValuePair<string, string>> metaValues)
        {
            var blob = blobContainer.GetBlockBlobReference($"{releaseId.ToString()}/{file.FileName}");
            blob.Properties.ContentType = file.ContentType;
            var path = await UploadToTemporaryFile(file);
            await blob.UploadFromFileAsync(path);
            await AddMetaValuesAsync(blob, metaValues);
        }

        private static async Task<string> UploadToTemporaryFile(IFormFile file)
        {
            var path = Path.GetTempFileName();
            if (file.Length > 0)
            {
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }

            return path;
        }

        public IEnumerable<FileInfo> ListFiles(Guid releaseId)
        {
            return ListFiles(releaseId.ToString());
        }
        
        public IEnumerable<FileInfo> ListFiles(string releaseId)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);

            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(ContainerName);
            blobContainer.CreateIfNotExists();

            return blobContainer.ListBlobs($"{releaseId}", true, BlobListingDetails.Metadata)
                .OfType<CloudBlockBlob>()
                .Where(IsDataFile)
                .Select(file => new FileInfo
                {
                    Extension = GetExtension(file),
                    Name = GetName(file),
                    Path = file.Name,
                    Size = GetSize(file),
                    MetaFileName = GetMetaFileName(file)
                })
                .OrderBy(info => info.Name);
        }
        
        private static bool IsDataFile(CloudBlob blob)
        {
            // Data files are identified by a key in their metadata describing their corresponding meta file
            return blob.Metadata.ContainsKey("metafile");
        }
        
        private static string GetExtension(CloudBlob blob)
        {
            return MimeTypeMap.GetExtension(blob.Properties.ContentType).TrimStart('.');
        }

        private static string GetName(CloudBlob blob)
        {
            return blob.Metadata.TryGetValue("name", out var name) ? name : "Unknown file name";
        }
        
        private static string GetMetaFileName(CloudBlob blob)
        {
            return blob.Metadata.TryGetValue("metafile", out var name) ? name : "Unknown file name";
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

        private static async Task AddMetaValuesAsync(CloudBlob blob, IEnumerable<KeyValuePair<string, string>> values)
        {
            foreach (var value in values)
            {
                blob.Metadata.Add(value);
            }

            await blob.SetMetadataAsync();
        }
    }
}