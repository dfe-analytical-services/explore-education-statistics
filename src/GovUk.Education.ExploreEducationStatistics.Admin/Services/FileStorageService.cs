using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using static System.StringComparison;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStorageUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Admin.Models.FileInfo;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _storageConnectionString;

        private readonly ISubjectService _subjectService;

        private const string ContainerName = "releases";

        private const string NameKey = "name";

        public FileStorageService(IConfiguration config, ISubjectService subjectService)
        {
            _storageConnectionString = config.GetConnectionString("CoreStorage");
            _subjectService = subjectService;
        }

        public async Task<IEnumerable<FileInfo>> ListPublicFilesPreviewAsync(Guid releaseId)
        {
            var files = new List<FileInfo>();

            files.AddRange(await ListFilesAsync(releaseId,ReleaseFileTypes.Data));
            files.AddRange(await ListFilesAsync(releaseId, ReleaseFileTypes.Ancillary));

            return files.OrderBy(f => f.Name);
        }
        
        public async Task<IEnumerable<FileInfo>> ListFilesAsync(Guid releaseId, ReleaseFileTypes type)
        {
            return await ListFilesAsync(releaseId.ToString(), type);
        }

        public async Task<Either<ValidationResult, IEnumerable<FileInfo>>> UploadDataFilesAsync(Guid releaseId,
            IFormFile dataFile, IFormFile metadataFile, string name, bool overwrite, string userName)
        {
            var blobContainer = await GetCloudBlobContainer();
            var dataInfo = new Dictionary<string, string>
                {{NameKey, name}, {MetaFileKey, metadataFile.FileName}, {UserName, userName}};
            var metaDataInfo = new Dictionary<string, string> {{DataFileKey, dataFile.FileName}, {UserName, userName}};
            return await ValidateDataFilesForUpload(blobContainer, releaseId, dataFile, metadataFile, name, overwrite)
                .OnSuccess(() =>
                    UploadFileAsync(blobContainer, releaseId, dataFile, ReleaseFileTypes.Data, dataInfo, overwrite))
                .OnSuccess(() => UploadFileAsync(blobContainer, releaseId, metadataFile, ReleaseFileTypes.Data,
                    metaDataInfo, overwrite))
                .OnSuccess(() => ListFilesAsync(releaseId, ReleaseFileTypes.Data));
        }

        // We cannot rely on the normal upload validation as we want this to be an atomic operation for both files.
        private async Task<Either<ValidationResult, bool>> ValidateDataFilesForUpload(CloudBlobContainer blobContainer,
            Guid releaseId, IFormFile dataFile, IFormFile metaFile, string name, bool overwrite)
        {
            if (string.Equals(dataFile.FileName, metaFile.FileName, OrdinalIgnoreCase))
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

            if (!IsCsvFile(dataFilePath, dataFile.OpenReadStream()))
            {
                return ValidationResult(DataFileMustBeCsvFile);
            }

            if (!IsCsvFile(metadataFilePath, metaFile.OpenReadStream()))
            {
                return ValidationResult(MetaFileMustBeCsvFile);
            }

            if (!overwrite && blobContainer.GetBlockBlobReference(dataFilePath).Exists())
            {
                return ValidationResult(CannotOverwriteDataFile);
            }

            if (!overwrite && blobContainer.GetBlockBlobReference(metadataFilePath).Exists())
            {
                return ValidationResult(CannotOverwriteMetadataFile);
            }

            if (_subjectService.Exists(releaseId, name))
            {
                return ValidationResult(SubjectTitleMustBeUnique);
            }

            return true;
        }

        public async Task<Either<ValidationResult, IEnumerable<FileInfo>>> DeleteDataFileAsync(Guid releaseId,
            string fileName)
        {
            var blobContainer = await GetCloudBlobContainer();
            // Get the paths of the files to delete
            return await DataPathsForDeletion(blobContainer, releaseId, fileName)
                .OnSuccess((path) =>
                {
                    // Delete the data file
                    return DeleteFileAsync(blobContainer, path.dataFilePath)
                        // and the metadata file
                        .OnSuccess(() => DeleteFileAsync(blobContainer, path.metadataFilePath))
                        // and return the remaining files
                        .OnSuccess(() => ListFilesAsync(releaseId, ReleaseFileTypes.Data));
                });
        }

        private async static Task<Either<ValidationResult, (string dataFilePath, string metadataFilePath)>>
            DataPathsForDeletion(CloudBlobContainer blobContainer, Guid releaseId, string fileName)
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

            return (dataFilePath: dataFilePath, metadataFilePath: metadataFilePath);
        }

        public async Task<Either<ValidationResult, IEnumerable<FileInfo>>> UploadFilesAsync(Guid releaseId,
            IFormFile file, string name, ReleaseFileTypes type, bool overwrite)
        {
            if (type == ReleaseFileTypes.Data)
            {
                return ValidationResult(CannotUseGenericFunctionToAddDataFile);
            }

            var blobContainer = await GetCloudBlobContainer();
            var info = new Dictionary<string, string> {{NameKey, name}};
            return await UploadFileAsync(blobContainer, releaseId, file, type, info, overwrite)
                .OnSuccess(() => ListFilesAsync(releaseId, type));
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
                .OnSuccess(() => ListFilesAsync(releaseId, type));
        }

        private async Task<IEnumerable<FileInfo>> ListFilesAsync(string releaseId, ReleaseFileTypes type)
        {
            var blobContainer = await GetCloudBlobContainer();

            return blobContainer
                .ListBlobs(AdminReleaseDirectoryPath(releaseId, type), true, BlobListingDetails.Metadata)
                .OfType<CloudBlockBlob>()
                .Where(blob => !IsBatchedFile(blob, releaseId) && !IsMetaDataFile(blob))
                .Select(file => new FileInfo
                {
                    Extension = GetExtension(file),
                    Name = GetName(file),
                    Path = file.Name,
                    Size = GetSize(file),
                    MetaFileName = GetMetaFileName(file),
                    Rows = GetNumberOfRows(file),
                    UserName = GetUserName(file),
                    Created = file.Properties.Created
                })
                .OrderBy(info => info.Name);
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

            metaValues["NumberOfRows"] = CalculateNumberOfRows(file.OpenReadStream()).ToString();

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
            return await FileStorageUtils.GetCloudBlobContainerAsync(_storageConnectionString, ContainerName);
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

        private static string GetMetaFileName(CloudBlob blob)
        {
            return blob.Metadata.TryGetValue(MetaFileKey, out var name) ? name : "";
        }

        private static int GetNumberOfRows(CloudBlob blob)
        {
            return
                blob.Metadata.TryGetValue(NumberOfRows, out var numberOfRows) &&
                int.TryParse(numberOfRows, out var numberOfRowsValue)
                    ? numberOfRowsValue
                    : 0;
        }

        private static string GetUserName(CloudBlob blob)
        {
            return blob.Metadata.TryGetValue(UserName, out var name) ? name : "";
        }

        private static bool IsBatchedFile(IListBlobItem blobItem, string releaseId)
        {
            return blobItem.Parent.Prefix.Equals(AdminReleaseBatchesDirectoryPath(releaseId));
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

        public async Task<Either<ValidationResult, FileStreamResult>> StreamFile(Guid releaseId,
            ReleaseFileTypes type, string fileName)
        {
            var blobContainer = await GetCloudBlobContainer();
            var blob = blobContainer.GetBlockBlobReference(AdminReleasePath(releaseId, type, fileName));

            if (!blob.Exists())
            {
                return ValidationResult(FileNotFound);
            }

            var stream = new MemoryStream();

            await blob.DownloadToStreamAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return new FileStreamResult(stream, blob.Properties.ContentType)
            {
                FileDownloadName = fileName
            };
        }

        private static bool IsCsvFile(string filePath, Stream fileStream)
        {
            if (!filePath.EndsWith(".csv"))
            {
                return false;
            }

            using var reader = new StreamReader(fileStream);
            return reader.BaseStream.IsTextFile();
        }

        private static int CalculateNumberOfRows(Stream fileStream)
        {
            using (var reader = new StreamReader(fileStream))
            {
                var numberOfLines = 0;
                while (reader.ReadLine() != null)
                {
                    ++numberOfLines;
                }

                return numberOfLines;
            }
        }
    }
}