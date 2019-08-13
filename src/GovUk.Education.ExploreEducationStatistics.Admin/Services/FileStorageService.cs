using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using MimeTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Admin.Models.FileInfo;
using ReleaseId = System.Guid;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _storageConnectionString;

        private const string ContainerName = "releases";

        private const string NameKey = "name";

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

        public async Task<Either<ValidationResult, IEnumerable<FileInfo>>> UploadDataFilesAsync(Guid releaseId,
            IFormFile dataFile, IFormFile metadataFile, string name)
        {
            var blobContainer = await GetCloudBlobContainer();
            var dataInfo = new Dictionary<string, string> {{NameKey, name}, {MetaFileKey, metadataFile.FileName}};
            var metaDataInfo = new Dictionary<string, string> {{DataFileKey, dataFile.FileName}};
            return await ValidateDataFilesForUpload(blobContainer, releaseId, dataFile, metadataFile)
                .OnSuccess(async () => await UploadFileAsync(blobContainer, releaseId, dataFile, ReleaseFileTypes.Data, dataInfo))
                .OnSuccess(async () => await UploadFileAsync(blobContainer, releaseId, metadataFile, ReleaseFileTypes.Data, metaDataInfo))
                .OnSuccess(async () => await ListFilesAsync(releaseId, ReleaseFileTypes.Data));
        }

        // We cannot rely on the normal upload validation as we want this to be an atomic operation for both files.
        private static async Task<Either<ValidationResult,bool>> ValidateDataFilesForUpload(CloudBlobContainer blobContainer, Guid releaseId,
            IFormFile dataFile, IFormFile metaFile)
        {
            if (string.Equals(dataFile.Name, metaFile.Name, StringComparison.OrdinalIgnoreCase))
            {
                return ValidationResult()
            }
            if (dataFile.Length == 0 || metaFile.Length == 0)
            {
                return ValidationResult(FileCannotBeEmpty);
            }
            var dataFilePath = AdminReleasePath(releaseId, ReleaseFileTypes.Data, dataFile.FileName);
            var metadataFilePath = AdminReleasePath(releaseId, ReleaseFileTypes.Data, metaFile.FileName);
            if (blobContainer.GetBlockBlobReference(dataFilePath).Exists())
            {
                return ValidationResult(CannotOverwriteFile);
            }

            if (blobContainer.GetBlockBlobReference(metadataFilePath).Exists())
            {
                return ValidationResult(CannotOverwriteFile);
            }
            return true;
        }
        
        public async Task<Either<ValidationResult, IEnumerable<FileInfo>>> DeleteDataFileAsync(Guid releaseId,
            string fileName)
        {
            // TODO what are the conditions in which we allow deletion?
            var blobContainer = await GetCloudBlobContainer();
            // Get the paths of the files to delete
            return await DataPathsForDeletion(blobContainer, releaseId, fileName)
                .OnSuccess(async (path) =>
                {
                    // Delete the data file
                    return await DeleteFileAsync(blobContainer, path.dataFilePath)
                        // and the metadata file
                        .OnSuccess(async () => await DeleteFileAsync(blobContainer, path.metadataFilePath))
                        // and return the remaining files
                        .OnSuccess(async () => await ListFilesAsync(releaseId, ReleaseFileTypes.Data));
                });
        }
        
        private async static Task<Either<ValidationResult, (string dataFilePath, string metadataFilePath)>> DataPathsForDeletion(CloudBlobContainer blobContainer, Guid releaseId, string fileName)
        {
            var dataFilePath = AdminReleasePath(releaseId, ReleaseFileTypes.Data, fileName);
            var dataBlob = blobContainer.GetBlockBlobReference(dataFilePath);
            if (!dataBlob.Exists())
            {
                return ValidationResult(FileNotFound);
            }

            dataBlob.FetchAttributes();
            if (!dataBlob.Metadata.ContainsKey(MetaFileKey))
            {
                return ValidationResult(UnableToFindMetadataFileToDelete);
            }

            var metaFileName = dataBlob.Metadata[MetaFileKey];
            var metadataFilePath = AdminReleasePath(releaseId, ReleaseFileTypes.Data, metaFileName);
            var metaBlob = blobContainer.GetBlockBlobReference(metadataFilePath);
            if (!metaBlob.Exists())
            {
                return ValidationResult(UnableToFindMetadataFileToDelete);
            }

            return (dataFilePath : dataFilePath, metadataFilePath : metadataFilePath);
        }

        public async Task<Either<ValidationResult, IEnumerable<FileInfo>>> UploadFilesAsync(Guid releaseId,
            IFormFile file, string name, ReleaseFileTypes type)
        {
            if (type == ReleaseFileTypes.Data)
            {
                return ValidationResult(CannotUseGenericFunctionToDeleteDataFile);
            }
            var blobContainer = await GetCloudBlobContainer();
            var info = new Dictionary<string, string> {{NameKey, name}};
            return await UploadFileAsync(blobContainer, releaseId, file, type, info)
                .OnSuccess(async () => await ListFilesAsync(releaseId, type));
        }

        public async Task<Either<ValidationResult, IEnumerable<FileInfo>>> DeleteFileAsync(Guid releaseId,
            ReleaseFileTypes type, string fileName)
        {
            // TODO Are there conditions in which we would not allow deletion?
            if (type == ReleaseFileTypes.Data)
            {
                return ValidationResult(CannotUseGenericFunctionToDeleteDataFile);
            }

            return await DeleteFileAsync(await GetCloudBlobContainer(), AdminReleasePath(releaseId, type, fileName))
                .OnSuccess(async () => await ListFilesAsync(releaseId, type));
        }

        private async Task<IEnumerable<FileInfo>> ListFilesAsync(string releaseId, ReleaseFileTypes type)
        {
            var blobContainer = await GetCloudBlobContainer();

            return blobContainer
                .ListBlobs(AdminReleaseDirectoryPath(releaseId, type), true, BlobListingDetails.Metadata)
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

        private static async Task<Either<ValidationResult, bool>> UploadFileAsync(CloudBlobContainer blobContainer,
            Guid releaseId, IFormFile file, ReleaseFileTypes type, IDictionary<string, string> metaValues)
        {
            var blob = blobContainer.GetBlockBlobReference(AdminReleasePath(releaseId, type, file.FileName));
            if (blob.Exists())
            {
                return ValidationResult(CannotOverwriteFile);
            }

            // Check that it is not an empty file because this causes issues downstream
            if (file.Length == 0)
            {
                return ValidationResult(FileCannotBeEmpty);
            }

            blob.Properties.ContentType = file.ContentType;
            var path = await UploadToTemporaryFile(file);
            await blob.UploadFromFileAsync(path);
            await AddMetaValuesAsync(blob, metaValues);
            return true;
        }

        private static async Task<Either<ValidationResult, bool>> DeleteFileAsync(CloudBlobContainer blobContainer,
            string path)
        {
            var blob = blobContainer.GetBlockBlobReference(path);
            if (!blob.Exists())
            {
                return ValidationResult(FileNotFound);
            }

            await blob.DeleteAsync();
            return true;
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

        private static string GetExtension(CloudBlob blob)
        {
            return MimeTypeMap.GetExtension(blob.Properties.ContentType).TrimStart('.');
        }

        private static string GetName(CloudBlob blob)
        {
            return blob.Metadata.TryGetValue(NameKey, out var name) ? name : "";
        }

        private static string GetMetaFileName(CloudBlob blob)
        {
            return blob.Metadata.TryGetValue(MetaFileKey, out var name) ? name : "";
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