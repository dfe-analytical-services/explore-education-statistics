using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using MimeTypes;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Admin.Models.FileInfo;
using ReleaseId = System.Guid;

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

        public async Task<IEnumerable<FileInfo>> UploadDataFilesAsync(Guid releaseId, IFormFile dataFile, IFormFile metaFile,
            string name)
        {
            var blobContainer = await GetCloudBlobContainer();
            var result = await UploadFileAsync(blobContainer, releaseId, dataFile, ReleaseFileTypes.Data, new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("name", name),
                new KeyValuePair<string, string>("metafile", metaFile.FileName)
            });
            
            await UploadFileAsync(blobContainer, releaseId, metaFile, ReleaseFileTypes.Data, new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("datafile", dataFile.FileName)
            });
            return await ListFilesAsync(releaseId, ReleaseFileTypes.Data);
        }
        
        public async Task<IEnumerable<FileInfo>> UploadFilesAsync(Guid releaseId, IFormFile dataFile, string name, ReleaseFileTypes type)
        {
            var blobContainer = await GetCloudBlobContainer();
            await UploadFileAsync(blobContainer, releaseId, dataFile, type, new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("name", name)
            });
            return await ListFilesAsync(releaseId, type);
        }

        private async Task<CloudBlobContainer> GetCloudBlobContainer()
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(ContainerName);
            await blobContainer.CreateIfNotExistsAsync();
            return blobContainer;
        }

        private static async Task<Either<ValidationResult, bool>> UploadFileAsync(CloudBlobContainer blobContainer, Guid releaseId,
            IFormFile file, ReleaseFileTypes type, IEnumerable<KeyValuePair<string, string>> metaValues)
        {
            var blob = blobContainer.GetBlockBlobReference(ReleasePath(releaseId, type) + $"/{file.FileName}");
            blob.Properties.ContentType = file.ContentType;
            var path = await UploadToTemporaryFile(file);
            await blob.UploadFromFileAsync(path);
            await AddMetaValuesAsync(blob, metaValues);
            return true;
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

        public async Task<IEnumerable<FileInfo>> ListFilesAsync(Guid releaseId, ReleaseFileTypes type)
        {
            return await ListFilesAsync(releaseId.ToString(), type);
        }
        
        public async Task<IEnumerable<FileInfo>> ListFilesAsync(string releaseId, ReleaseFileTypes type)
        {
            var blobContainer = await GetCloudBlobContainer();

            return blobContainer.ListBlobs(ReleasePath(releaseId, type), true, BlobListingDetails.Metadata)
                .OfType<CloudBlockBlob>()
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

        private static string ReleasePath(string releaseId, ReleaseFileTypes type)
        {
            return $"{releaseId}/{type.GetEnumLabel()}";
        }

        private static string ReleasePath(ReleaseId releaseId, ReleaseFileTypes type) =>
            ReleasePath(releaseId.ToString(), type);

        private static string GetExtension(CloudBlob blob)
        {
            return MimeTypeMap.GetExtension(blob.Properties.ContentType).TrimStart('.');
        }

        private static string GetName(CloudBlob blob)
        {
            return blob.Metadata.TryGetValue("name", out var name) ? name : "";
        }
        
        private static string GetMetaFileName(CloudBlob blob)
        {
            return blob.Metadata.TryGetValue("metafile", out var name) ? name : "";
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