﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
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
using FileInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.FileInfo;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class FileStorageService : IFileStorageService
    {
        private static readonly Regex[] CsvMimeTypes = {
            new Regex(@"^(application|text)/csv$"),
            new Regex(@"text/plain$")
        };
        
        private static readonly string[] CsvEncodingTypes = {
            "us-ascii",
            "utf-8"
        };
        
        private readonly string _storageConnectionString;

        private readonly ISubjectService _subjectService;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;
        private readonly IFileTypeService _fileTypeService;

        private const string ContainerName = "releases";

        private const string NameKey = "name";

        public FileStorageService(IConfiguration config, ISubjectService subjectService, IUserService userService, 
            IPersistenceHelper<ContentDbContext> persistenceHelper, IFileTypeService fileTypeService)
        {
            _storageConnectionString = config.GetConnectionString("CoreStorage");
            _subjectService = subjectService;
            _userService = userService;
            _persistenceHelper = persistenceHelper;
            _fileTypeService = fileTypeService;
        }

        public async Task<Either<ActionResult, IEnumerable<FileInfo>>> ListPublicFilesPreview(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(release => 
                    FileStorageUtils.ListPublicFilesPreview(_storageConnectionString, ContainerName, releaseId));
        }

        public Task<Either<ActionResult, IEnumerable<Models.FileInfo>>> UploadDataFilesAsync(Guid releaseId,
            IFormFile dataFile, IFormFile metadataFile, string name, bool overwrite, string userName)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async release =>
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
                });
        }

        public Task<Either<ActionResult, IEnumerable<Models.FileInfo>>> DeleteDataFileAsync(Guid releaseId,
            string fileName)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async release =>
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
                });
        }

        public Task<Either<ActionResult, IEnumerable<Models.FileInfo>>> UploadFilesAsync(Guid releaseId,
            IFormFile file, string name, ReleaseFileTypes type, bool overwrite, IEnumerable<Regex> allowedMimeTypes)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccessDo(() => ValidateUploadFileType(file, allowedMimeTypes))
                .OnSuccess(async release =>
                {
                    if (type == ReleaseFileTypes.Data)
                    {
                        return ValidationActionResult(CannotUseGenericFunctionToAddDataFile);
                    }

                    var blobContainer = await GetCloudBlobContainer();
                    var info = new Dictionary<string, string> {{NameKey, name}};
                    return await UploadFileAsync(blobContainer, releaseId, file, type, info, overwrite)
                        .OnSuccess(() => ListFilesAsync(releaseId, type));
                });
        }

        public Task<Either<ActionResult, IEnumerable<Models.FileInfo>>> DeleteFileAsync(Guid releaseId,
            ReleaseFileTypes type, string fileName)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async release =>
                {
                    if (type == ReleaseFileTypes.Data)
                    {
                        return ValidationActionResult(CannotUseGenericFunctionToDeleteDataFile);
                    }

                    return await DeleteFileAsync(await GetCloudBlobContainer(),
                            AdminReleasePath(releaseId, type, fileName))
                        .OnSuccess(() => ListFilesAsync(releaseId, type));
                });
        }

        public async Task<Either<ActionResult, IEnumerable<Models.FileInfo>>> ListFilesAsync(Guid releaseId, ReleaseFileTypes type)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async _ =>
                {
                    var blobContainer = await GetCloudBlobContainer();

                    IEnumerable<Models.FileInfo> files = blobContainer
                        .ListBlobs(AdminReleaseDirectoryPath(releaseId, type), true, BlobListingDetails.Metadata)
                        .Where(blob => !IsBatchedFile(blob, releaseId))
                        .OfType<CloudBlockBlob>()
                        .Select(file => new Models.FileInfo
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

                    return files;
                });
        }

        public async Task<Either<ActionResult, FileStreamResult>> StreamFile(Guid releaseId,
            ReleaseFileTypes type, string fileName)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async _ =>
                {
                    var blobContainer = await GetCloudBlobContainer();
                    var blob = blobContainer.GetBlockBlobReference(AdminReleasePath(releaseId, type, fileName));

                    if (!blob.Exists())
                    {
                        return ValidationActionResult<FileStreamResult>(FileNotFound);
                    }

                    var stream = new MemoryStream();

                    await blob.DownloadToStreamAsync(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return new FileStreamResult(stream, blob.Properties.ContentType)
                    {
                        FileDownloadName = fileName
                    };
                });
        }

        private async static Task<Either<ActionResult, (string dataFilePath, string metadataFilePath)>>
            DataPathsForDeletion(CloudBlobContainer blobContainer, Guid releaseId, string fileName)
        {
            var dataFilePath = AdminReleasePath(releaseId, ReleaseFileTypes.Data, fileName);
            var dataBlob = blobContainer.GetBlockBlobReference(dataFilePath);
            if (!dataBlob.Exists())
            {
                return ValidationActionResult(FileNotFound);
            }

            dataBlob.FetchAttributes();
            if (!dataBlob.Metadata.ContainsKey(MetaFileKey))
            {
                return ValidationActionResult(UnableToFindMetadataFileToDelete);
            }

            var metaFileName = dataBlob.Metadata[MetaFileKey];
            var metadataFilePath = AdminReleasePath(releaseId, ReleaseFileTypes.Data, metaFileName);
            var metaBlob = blobContainer.GetBlockBlobReference(metadataFilePath);
            if (!metaBlob.Exists())
            {
                return ValidationActionResult(UnableToFindMetadataFileToDelete);
            }

            return (dataFilePath: dataFilePath, metadataFilePath: metadataFilePath);
        }

        // We cannot rely on the normal upload validation as we want this to be an atomic operation for both files.
        private async Task<Either<ActionResult, bool>> ValidateDataFilesForUpload(CloudBlobContainer blobContainer,
            Guid releaseId, IFormFile dataFile, IFormFile metaFile, string name, bool overwrite)
        {
            if (string.Equals(dataFile.FileName, metaFile.FileName, OrdinalIgnoreCase))
            {
                return ValidationActionResult(DataAndMetadataFilesCannotHaveTheSameName);
            }

            if (dataFile.Length == 0)
            {
                return ValidationActionResult(DataFileCannotBeEmpty);
            }

            if (metaFile.Length == 0)
            {
                return ValidationActionResult(MetadataFileCannotBeEmpty);
            }

            var dataFilePath = AdminReleasePath(releaseId, ReleaseFileTypes.Data, dataFile.FileName);
            var metadataFilePath = AdminReleasePath(releaseId, ReleaseFileTypes.Data, metaFile.FileName);

            if (!IsCsvFile(dataFilePath, dataFile))
            {
                return ValidationActionResult(DataFileMustBeCsvFile);
            }

            if (!IsCsvFile(metadataFilePath, metaFile))
            {
                return ValidationActionResult(MetaFileMustBeCsvFile);
            }

            if (!overwrite && blobContainer.GetBlockBlobReference(dataFilePath).Exists())
            {
                return ValidationActionResult(CannotOverwriteDataFile);
            }

            if (!overwrite && blobContainer.GetBlockBlobReference(metadataFilePath).Exists())
            {
                return ValidationActionResult(CannotOverwriteMetadataFile);
            }

            if (_subjectService.Exists(releaseId, name))
            {
                return ValidationActionResult(SubjectTitleMustBeUnique);
            }

            return true;
        }
        
        // We cannot rely on the normal upload validation as we want this to be an atomic operation for both files.
        private async Task<Either<ActionResult, bool>> ValidateUploadFileType(
            IFormFile file, IEnumerable<Regex> allowedMimeTypes)
        {
            if (!_fileTypeService.HasMatchingMimeType(file, allowedMimeTypes))
            {
                return ValidationActionResult(FileTypeInvalid);
            }

            return true;
        }

        private static async Task<Either<ActionResult, bool>> UploadFileAsync(CloudBlobContainer blobContainer,
            Guid releaseId, IFormFile file, ReleaseFileTypes type, IDictionary<string, string> metaValues,
            bool overwrite)
        {
            var blob = blobContainer.GetBlockBlobReference(AdminReleasePath(releaseId, type, file.FileName));
            if (!overwrite && blob.Exists())
            {
                return ValidationActionResult(CannotOverwriteFile);
            }

            // Check that it is not an empty file because this causes issues downstream
            if (file.Length == 0)
            {
                return ValidationActionResult(FileCannotBeEmpty);
            }

            blob.Properties.ContentType = file.ContentType;
            var path = await UploadToTemporaryFile(file);
            await blob.UploadFromFileAsync(path);

            metaValues["NumberOfRows"] = CalculateNumberOfRows(file.OpenReadStream()).ToString();

            await AddMetaValuesAsync(blob, metaValues);
            return true;
        }

        private static async Task<Either<ActionResult, bool>> DeleteFileAsync(CloudBlobContainer blobContainer,
            string path)
        {
            var blob = blobContainer.GetBlockBlobReference(path);
            if (!blob.Exists())
            {
                return ValidationActionResult(FileNotFound);
            }

            await blob.DeleteAsync();
            return true;
        }

        private async Task<CloudBlobContainer> GetCloudBlobContainer()
        {
            return await GetCloudBlobContainerAsync(_storageConnectionString, ContainerName);
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

        private static bool IsBatchedFile(IListBlobItem blobItem, Guid releaseId)
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

        private bool IsCsvFile(string filePath, IFormFile file)
        {
            if (!filePath.EndsWith(".csv"))
            {
                return false;
            }
            
            return _fileTypeService.HasMatchingMimeType(file, CsvMimeTypes) && _fileTypeService.HasMatchingEncodingType(file, CsvEncodingTypes);
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