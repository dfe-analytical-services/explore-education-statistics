using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Blob;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStorageUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.FileInfo;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _storageConnectionString;

        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;
        private readonly ContentDbContext _context;
        private readonly IImportService _importService;
        private readonly IFileUploadsValidatorService _fileUploadsValidatorService;

        private const string ContainerName = "releases";
        private const string NameKey = "name";

        public FileStorageService(IConfiguration config, IUserService userService,
            IPersistenceHelper<ContentDbContext> persistenceHelper, ContentDbContext context,
            IImportService importService, IFileUploadsValidatorService fileUploadsValidatorService)
        {
            _storageConnectionString = config.GetValue<string>("CoreStorage");
            _userService = userService;
            _persistenceHelper = persistenceHelper;
            _context = context;
            _importService = importService;
            _fileUploadsValidatorService = fileUploadsValidatorService;
        }

        public async Task<Either<ActionResult, IEnumerable<FileInfo>>> ListPublicFilesPreview(Guid releaseId,
            IEnumerable<Guid> referencedReleaseVersions)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(release =>
                    FileStorageUtils.ListPublicFilesPreview(_storageConnectionString, ContainerName,
                        referencedReleaseVersions));
        }

        public Task<Either<ActionResult, IEnumerable<Models.FileInfo>>> UploadDataFilesAsync(Guid releaseId,
            IFormFile dataFile, IFormFile metadataFile, string name, string userName)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async release =>
                {
                    var blobContainer = await GetCloudBlobContainer();
                    var dataInfo = new Dictionary<string, string>
                        {{NameKey, name}, {MetaFileKey, metadataFile.FileName.ToLower()}, {UserName, userName}};
                    var metaDataInfo = new Dictionary<string, string>
                        {{DataFileKey, dataFile.FileName.ToLower()}, {UserName, userName}};
                    return await _fileUploadsValidatorService.ValidateDataFilesForUpload(releaseId, dataFile,metadataFile, name)
                            .OnSuccess(() => _importService.CreateImportTableRow(releaseId, dataFile.FileName.ToLower()))
                            .OnSuccess(() => UploadFileAsync(blobContainer, releaseId, dataFile, ReleaseFileTypes.Data, dataInfo))
                            .OnSuccess(() => CreateBasicFileLink(dataFile.FileName.ToLower(), releaseId, ReleaseFileTypes.Data))
                            .OnSuccess(() => UploadFileAsync(blobContainer, releaseId, metadataFile,ReleaseFileTypes.Metadata, metaDataInfo))
                            .OnSuccess(() => CreateBasicFileLink(metadataFile.FileName.ToLower(), releaseId, ReleaseFileTypes.Metadata))
                            // add message to queue to process these files
                            .OnSuccessDo(() => _importService.Import(dataFile.FileName.ToLower(), metadataFile.FileName.ToLower(), releaseId, dataFile))
                            .OnSuccess(() => ListFilesAsync(releaseId, ReleaseFileTypes.Data, ReleaseFileTypes.Metadata));
                });
        }

        public async Task<Either<ActionResult, IEnumerable<FileInfo>>> ListChartFilesAsync(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async _ =>
                {
                    var blobContainer = await GetCloudBlobContainer();

                    var files = _context.ReleaseFiles
                        .Include(f => f.ReleaseFileReference)
                        .Where(f => f.ReleaseId == releaseId && ReleaseFileTypes.Chart == f.ReleaseFileReference.ReleaseFileType)
                        .ToList();

                    return files.Select(fileLink =>
                        {
                            var fileReference = fileLink.ReleaseFileReference;
                            var blobPath = AdminReleasePathWithFileReference(fileReference);
                            var blob = blobContainer.GetBlockBlobReference(blobPath);
                            return GetFileInfo(blob);
                        })
                        .OrderBy(file => file.Name)
                        .AsEnumerable();
                });
        }

        public Task<Either<ActionResult, IEnumerable<Models.FileInfo>>> RemoveDataFileReleaseLinkAsync(Guid releaseId,
            string dataFileName)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(GetCloudBlobContainer)
                .OnSuccess(async blobContainer =>
                {
                    if (await CheckFileDeletionWillOrphanFileAsync(releaseId, dataFileName, ReleaseFileTypes.Data))
                    {
                        return
                            await DataPathsForDeletion(blobContainer, releaseId, dataFileName)
                                .OnSuccess(fileNames =>
                                    DeleteDataFilesAsync(releaseId, blobContainer, fileNames)
                                        .OnSuccess(() => DeleteFileReference(releaseId, fileNames.dataFileName,
                                            ReleaseFileTypes.Data))
                                        .OnSuccess(() => DeleteFileReference(releaseId, fileNames.metadataFileName,
                                            ReleaseFileTypes.Metadata))
                                        .OnSuccess(() => ListFilesAsync(releaseId, ReleaseFileTypes.Data)));
                    }

                    var metaFilename = await GetAssociatedMetaFilename(releaseId, dataFileName);

                    return await DeleteFileLink(releaseId, dataFileName, ReleaseFileTypes.Data)
                        .OnSuccess(() => DeleteFileLink(releaseId, metaFilename, ReleaseFileTypes.Metadata))
                        .OnSuccess(() => ListFilesAsync(releaseId, ReleaseFileTypes.Data));
                });
        }

        public Task<Either<ActionResult, IEnumerable<Models.FileInfo>>> UploadFilesAsync(Guid releaseId,
            IFormFile file, string name, ReleaseFileTypes type, bool overwrite)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccessDo(() => _fileUploadsValidatorService.ValidateUploadFileType(file, type))
                .OnSuccess(async release =>
                {
                    var blobContainer = await GetCloudBlobContainer();
                    var info = new Dictionary<string, string> {{NameKey, name.ToLower()}};
                    return await
                        _fileUploadsValidatorService
                            .ValidateFileForUpload(releaseId, file, type, overwrite)
                            .OnSuccess(() => UploadFileAsync(blobContainer, releaseId, file, type, info))
                            .OnSuccess(() => CreateBasicFileLink(file.FileName.ToLower(), releaseId, type))
                            .OnSuccess(() => ListFilesAsync(releaseId, type));
                });
        }

        public Task<Either<ActionResult, FileInfo>> UploadChartFileAsync(Guid releaseId, IFormFile file)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccessDo(() => _fileUploadsValidatorService.ValidateUploadFileType(file, ReleaseFileTypes.Chart))
                .OnSuccess(async release =>
                {
                    var blobContainer = await GetCloudBlobContainer();
                    var info = new Dictionary<string, string> {{NameKey, file.FileName.ToLower()}};
                    return await
                        _fileUploadsValidatorService.ValidateFileForUpload(releaseId, file, ReleaseFileTypes.Chart, true)
                            .OnSuccess(() => UploadFileAsync(blobContainer, releaseId, file, ReleaseFileTypes.Chart, info))
                            .OnSuccess(() => CreateBasicFileLink(file.FileName.ToLower(), releaseId, ReleaseFileTypes.Chart))
                            .OnSuccess(_ =>
                            {
                                var blob = blobContainer.GetBlobReference(AdminReleasePath(releaseId,
                                    ReleaseFileTypes.Chart, file.FileName.ToLower()));
                                return GetFileInfo(blob);
                            });
                });
        }

        public async Task<Either<ActionResult, IEnumerable<Models.FileInfo>>> DeleteNonDataFileAsync(Guid releaseId,
            ReleaseFileTypes type, string fileName)
        {
            return await DeleteNonDataFilesAsync(releaseId, type, new List<string>(){fileName})
                .OnSuccess(() => ListFilesAsync(releaseId, type));
        }
        
        public async Task<Either<ActionResult, bool>> DeleteNonDataFilesAsync(Guid releaseId, ReleaseFileTypes type, IEnumerable<string> fileNames)
        {
            if (type == ReleaseFileTypes.Data || type == ReleaseFileTypes.Metadata)
            {
                return ValidationActionResult(CannotUseGenericFunctionToDeleteDataFile);
            }
            
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async () =>
                {
                    foreach (var fileName in fileNames)
                    {
                        if (await CheckFileDeletionWillOrphanFileAsync(releaseId, fileName, type))
                        {
                            await DeleteFileAsync(await GetCloudBlobContainer(), AdminReleasePath(releaseId, type, fileName))
                                .OnSuccess(() => DeleteFileReference(releaseId, fileName, type));
                        }
                        else
                        {
                            await DeleteFileLink(releaseId, fileName, type);
                        }
                    }

                    return true;
                });
        }

        public async Task<Either<ActionResult, IEnumerable<Models.FileInfo>>> ListFilesAsync(Guid releaseId,
            params ReleaseFileTypes[] types)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async _ =>
                {
                    var blobContainer = await GetCloudBlobContainer();

                    var files = _context
                        .ReleaseFiles
                        .Include(f => f.ReleaseFileReference)
                        .Where(f => f.ReleaseId == releaseId && types.Contains(f.ReleaseFileReference.ReleaseFileType))
                        .ToList();

                    var filesWithMetadata = files
                        .Select(async fileLink =>
                        {
                            var fileReference = fileLink.ReleaseFileReference;
                            var blobPath = AdminReleasePathWithFileReference(fileReference);
                            var file = blobContainer.GetBlockBlobReference(blobPath);
                            await file.FetchAttributesAsync();
                            return new Models.FileInfo
                            {
                                Extension = GetExtension(file),
                                Name = GetName(file),
                                Path = file.Name,
                                Size = GetSize(file),
                                MetaFileName = GetMetaFileName(file),
                                Rows = GetNumberOfRows(file),
                                UserName = GetUserName(file),
                                Created = file.Properties.Created
                            };
                        });

                    return (await Task.WhenAll(filesWithMetadata))
                        .OrderBy(file => file.Name)
                        .AsEnumerable();
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
                    var fileLink = await GetReleaseFileLinkAsync(releaseId, fileName, type);

                    var blobContainer = await GetCloudBlobContainer();
                    var blob = blobContainer.GetBlockBlobReference(
                        AdminReleasePathWithFileReference(fileLink.ReleaseFileReference));

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

        private async static Task<Either<ActionResult, (string dataFileName, string metadataFileName)>>
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
            var metadataFilePath = AdminReleasePath(releaseId, ReleaseFileTypes.Metadata, metaFileName);
            var metaBlob = blobContainer.GetBlockBlobReference(metadataFilePath);
            if (!metaBlob.Exists())
            {
                return ValidationActionResult(UnableToFindMetadataFileToDelete);
            }

            return (fileName, metaFileName);
        }

        private async Task<Either<ActionResult, bool>> CreateBasicFileLink(string filename, Guid releaseId,
            ReleaseFileTypes type)
        {
            var fileLink = new ReleaseFile
            {
                ReleaseId = releaseId,
                ReleaseFileReference = new ReleaseFileReference
                {
                    ReleaseId = releaseId,
                    Filename = filename,
                    ReleaseFileType = type
                }
            };

            await _context.ReleaseFiles.AddAsync(fileLink);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<Either<ActionResult, bool>> DeleteFileLink(Guid releaseId, string filename, ReleaseFileTypes type)
        {
            var fileLink = await GetReleaseFileLinkAsync(releaseId, filename, type);

            _context.ReleaseFiles.Remove(fileLink);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<ReleaseFile> GetReleaseFileLinkAsync(Guid releaseId, string filename, ReleaseFileTypes type)
        {
            var releaseFileLinks = _context
                .ReleaseFiles
                .Include(f => f.ReleaseFileReference)
                .Where(f => f.ReleaseId == releaseId &&
                            f.ReleaseFileReference.ReleaseFileType == type);
            
            // Make sure the filename predicate is case sensitive by executing in memory rather than in the db
            return releaseFileLinks.ToList()
                .FirstOrDefault(file => file.ReleaseFileReference.Filename == filename);
        }

        private async Task<Either<ActionResult, bool>> DeleteFileReference(Guid releaseId, string filename,
            ReleaseFileTypes type)
        {
            var fileLink = await GetReleaseFileLinkAsync(releaseId, filename, type);

            _context.ReleaseFileReferences.Remove(fileLink.ReleaseFileReference);
            await _context.SaveChangesAsync();
            return true;
        }

        private static async Task<Either<ActionResult, bool>> UploadFileAsync(CloudBlobContainer blobContainer,
            Guid releaseId, IFormFile file, ReleaseFileTypes type, IDictionary<string, string> metaValues)
        {
            var blob = blobContainer.GetBlockBlobReference(AdminReleasePath(releaseId, type, file.FileName.ToLower()));
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

        private static async Task<Either<ActionResult, bool>> DeleteDataFilesAsync(Guid releaseId,
            CloudBlobContainer blobContainer, (string, string) fileNames)
        {
            await DeleteFileAsync(blobContainer, AdminReleasePath(releaseId, ReleaseFileTypes.Data, fileNames.Item1));
            await DeleteFileAsync(blobContainer, AdminReleasePath(releaseId, ReleaseFileTypes.Metadata, fileNames.Item2));
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
                await using var stream = new FileStream(path, FileMode.Create);
                await file.CopyToAsync(stream);
            }

            return path;
        }

        private static string GetMetaFileName(CloudBlob blob)
        {
            return blob.Metadata.TryGetValue(MetaFileKey, out var name) ? name : "";
        }

        private static int GetNumberOfRows(CloudBlob blob)
        {
            return blob.Metadata.TryGetValue(NumberOfRows, out var numberOfRows) &&
                   int.TryParse(numberOfRows, out var numberOfRowsValue) ? numberOfRowsValue : 0;
        }

        private static string GetUserName(CloudBlob blob)
        {
            return blob.Metadata.TryGetValue(UserName, out var name) ? name : "";
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

        private async Task<bool> CheckFileDeletionWillOrphanFileAsync(Guid releaseId, string filename,
            ReleaseFileTypes type)
        {
            var fileLink = await GetReleaseFileLinkAsync(releaseId, filename, type);

            var otherFileReferences = await _context
                .ReleaseFiles
                .CountAsync(f => f.ReleaseFileReferenceId == fileLink.ReleaseFileReferenceId && f.Id != fileLink.Id);

            return otherFileReferences == 0;
        }

        private string AdminReleasePathWithFileReference(ReleaseFileReference fileReference)
        {
            return AdminReleasePath(fileReference.ReleaseId, fileReference.ReleaseFileType, fileReference.Filename);
        }

        private async Task<string> GetAssociatedMetaFilename(Guid releaseId, string dataFileName)
        {
            var releaseDataFileLink = await GetReleaseFileLinkAsync(releaseId, dataFileName, ReleaseFileTypes.Data);

            return _context.ReleaseFileReferences
                .First(rfr => rfr.ReleaseId == releaseDataFileLink.ReleaseFileReference.ReleaseId
                              && rfr.ReleaseFileType == ReleaseFileTypes.Metadata
                              && rfr.SubjectId == releaseDataFileLink.ReleaseFileReference.SubjectId).Filename;
        }
    }
}