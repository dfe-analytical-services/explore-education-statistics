using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using static GovUk.Education.ExploreEducationStatistics.Data.Seed.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Data.Seed.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStorageUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Seed.Services
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

        public async Task<Either<ValidationResult, Either<ValidationResult, bool>>> UploadDataFilesAsync(Guid releaseId,
            IFormFile dataFile, IFormFile metadataFile, string name, bool overwrite)
        {
            var blobContainer =
                await FileStorageUtils.GetCloudBlobContainerAsync(_storageConnectionString, ContainerName);
            var dataInfo = new Dictionary<string, string> {{NameKey, name}, {MetaFileKey, metadataFile.FileName}};
            var metaDataInfo = new Dictionary<string, string> {{DataFileKey, dataFile.FileName}};
            return await ValidateDataFilesForUpload(blobContainer, releaseId, dataFile, metadataFile, overwrite)
                .OnSuccess(() =>
                    UploadFileAsync(blobContainer, releaseId, dataFile, ReleaseFileTypes.Data, dataInfo, overwrite))
                .OnSuccess(() => UploadFileAsync(blobContainer, releaseId, metadataFile, ReleaseFileTypes.Data,
                    metaDataInfo, overwrite));
        }

        // We cannot rely on the normal upload validation as we want this to be an atomic operation for both files.
        private static async Task<Either<ValidationResult, bool>> ValidateDataFilesForUpload(
            CloudBlobContainer blobContainer, Guid releaseId,
            IFormFile dataFile, IFormFile metaFile, bool overwrite)
        {
            if (string.Equals(dataFile.FileName, metaFile.FileName, StringComparison.OrdinalIgnoreCase))
            {
                return ValidationResult(DataAndMetadataFilesCannotHaveTheSameName);
            }

            if (dataFile.Length == 0)
            {
                return ValidationResult(DataFileCannotBeEmpty);
            }

            if (metaFile.Length == 0)
            {
                return ValidationResult(MetadataFileCannotBeEmpty);
            }

            var dataFilePath = AdminReleasePath(releaseId, ReleaseFileTypes.Data, dataFile.FileName);
            var metadataFilePath = AdminReleasePath(releaseId, ReleaseFileTypes.Data, metaFile.FileName);
            if (!overwrite && blobContainer.GetBlockBlobReference(dataFilePath).Exists())
            {
                return ValidationResult(CannotOverwriteDataFile);
            }

            if (!overwrite && blobContainer.GetBlockBlobReference(metadataFilePath).Exists())
            {
                return ValidationResult(CannotOverwriteMetadataFile);
            }

            return true;
        }

        private static async Task<Either<ValidationResult, bool>> UploadFileAsync(CloudBlobContainer blobContainer,
            Guid releaseId, IFormFile file, ReleaseFileTypes type, IDictionary<string, string> metaValues,
            bool overwrite)
        {
            var blob = blobContainer.GetBlockBlobReference(AdminReleasePath(releaseId, type, file.FileName));
            if (!overwrite && blob.Exists())
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