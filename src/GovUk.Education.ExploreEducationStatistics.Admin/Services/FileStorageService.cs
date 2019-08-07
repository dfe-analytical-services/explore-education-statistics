using System;
using System.Collections.Generic;
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
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Admin.Models.FileInfo;
using ReleaseId = System.Guid;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

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

        public async Task<IEnumerable<FileInfo>> ListFilesAsync(Guid releaseId, ReleaseFileTypes type)
        {
            return await ListFilesAsync(releaseId.ToString(), type);
        }

        public async Task<Either<ValidationResult, IEnumerable<FileInfo>>> UploadDataFilesAsync(Guid releaseId, IFormFile dataFile, IFormFile metaFile, string name)
        {
            var blobContainer = await GetCloudBlobContainer();
            var dataInfo = new Dictionary<string, string> {{"name", name}, {"metafile", metaFile.FileName}};
            var metaDataInfo = new Dictionary<string, string> {{"datafile", dataFile.FileName}};
            return await UploadFileAsync(blobContainer, releaseId, dataFile, ReleaseFileTypes.Data, dataInfo)
                .OnSuccess(async () =>
                    await UploadFileAsync(blobContainer, releaseId, metaFile, ReleaseFileTypes.Data, metaDataInfo))
                .OnSuccess(async () =>
                    await ListFilesAsync(releaseId, ReleaseFileTypes.Data));
        }

        public async Task<IEnumerable<FileInfo>> DeleteDataFileAsync(Guid releaseId, string fileName)
        {
            // TODO what are the conditions in which we allow deletion?
            var blobContainer = await GetCloudBlobContainer();
            var dataBlob = blobContainer.GetBlockBlobReference(ReleasePath(releaseId, ReleaseFileTypes.Data, fileName));
            var metaFileName = dataBlob.Metadata["metafile"];

            // Delete the data and the metadata
            await DeleteFileAsync(blobContainer, ReleasePath(releaseId, ReleaseFileTypes.Data, fileName));
            await DeleteFileAsync(blobContainer, ReleasePath(releaseId, ReleaseFileTypes.Data, metaFileName));
            return await ListFilesAsync(releaseId, ReleaseFileTypes.Data);
        }

        public async Task<Either<ValidationResult, IEnumerable<FileInfo>>> UploadFilesAsync(Guid releaseId, IFormFile file, string name, ReleaseFileTypes type)
        {
            var blobContainer = await GetCloudBlobContainer();
            var info = new Dictionary<string, string> {{"name", name}};
            return await UploadFileAsync(blobContainer, releaseId, file, type, info, true)
                .OnSuccess(async () => await ListFilesAsync(releaseId, type));
        }

        public async Task<IEnumerable<FileInfo>> DeleteFileAsync(Guid releaseId, ReleaseFileTypes type, string fileName)
        {
            // TODO Are there conditions in which we would not allow deletion?
            if (type == ReleaseFileTypes.Data)
            {
                throw new ArgumentException("Deleting data files required calling DeleteDataFilesAsync");
            }

            await DeleteFileAsync(await GetCloudBlobContainer(), ReleasePath(releaseId, type, fileName));
            return await ListFilesAsync(releaseId, type);
        }

        private async Task<IEnumerable<FileInfo>> ListFilesAsync(string releaseId, ReleaseFileTypes type)
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

        private static async Task<Either<ValidationResult, bool>> UploadFileAsync(CloudBlobContainer blobContainer, Guid releaseId, IFormFile file, ReleaseFileTypes type, IDictionary<string, string> metaValues, bool overwrite = false)
        {
            var blob = blobContainer.GetBlockBlobReference(ReleasePath(releaseId, type, file.FileName));
            if (blob.Exists() && !overwrite)
            {
                return ValidationResult("File name", "Cannot overwrite");
            }

            blob.Properties.ContentType = file.ContentType;
            var path = await UploadToTemporaryFile(file);
            await blob.UploadFromFileAsync(path);
            await AddMetaValuesAsync(blob, metaValues);
            return true;
        }

        private static async Task DeleteFileAsync(CloudBlobContainer blobContainer, string path)
        {
            var blob = blobContainer.GetBlockBlobReference(path);
            await blob.DeleteIfExistsAsync();
        }

        private async Task<CloudBlobContainer> GetCloudBlobContainer()
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(ContainerName);
            await blobContainer.CreateIfNotExistsAsync();
            return blobContainer;
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

        private static string ReleasePath(string releaseId, ReleaseFileTypes type)
        {
            return $"{releaseId}/{type.GetEnumLabel()}";
        }

        private static string ReleasePath(string releaseId, ReleaseFileTypes type, string fileName)
        {
            return ReleasePath(releaseId, type) + $"/{fileName}";
        }

        private static string ReleasePath(Guid releaseId, ReleaseFileTypes type, string fileName)
            => ReleasePath(releaseId.ToString(), type, fileName);

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

        private static async Task AddMetaValuesAsync(CloudBlob blob, IDictionary<string, string> values)
        {
            foreach (var (key, value) in values)
            {
                if (blob.Metadata.ContainsKey(key))
                {
                    blob.Metadata.Remove(key);
                }

                blob.Metadata.Add(key, value);
            }

            await blob.SetMetadataAsync();
        }
    }
}