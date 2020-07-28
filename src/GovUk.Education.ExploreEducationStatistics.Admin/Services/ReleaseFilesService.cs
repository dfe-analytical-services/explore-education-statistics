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
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.FileInfo;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseFilesService : IReleaseFilesService
    {
        private readonly string _storageConnectionString;

        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;
        private readonly ContentDbContext _context;
        private readonly IImportService _importService;
        private readonly IFileUploadsValidatorService _fileUploadsValidatorService;
        
        private const string NameKey = "name";

        public ReleaseFilesService(IConfiguration config, IUserService userService,
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
                    FileStorageUtils.ListPublicFilesPreview(_storageConnectionString, PrivateFilesContainerName,
                        referencedReleaseVersions));
        }

        public Task<Either<ActionResult, bool>> UploadDataFilesAsync(Guid releaseId,
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

                    return await _fileUploadsValidatorService
                        .ValidateDataFilesForUpload(releaseId, dataFile, metadataFile, name)
                        // First, create with status uploading to prevent other users uploading the same datafile
                        .OnSuccess(async () => await _importService.CreateImportTableRow(releaseId, dataFile.FileName.ToLower()))
                        .OnSuccess(async () =>
                        {
                            await CreateBasicFileLink(dataFile.FileName.ToLower(), releaseId, ReleaseFileTypes.Data);
                            await CreateBasicFileLink(metadataFile.FileName.ToLower(), releaseId,ReleaseFileTypes.Metadata);
                            await _context.SaveChangesAsync();
                            await UploadFileToStorageAsync(blobContainer, releaseId, dataFile, ReleaseFileTypes.Data, dataInfo);
                            await UploadFileToStorageAsync(blobContainer, releaseId, metadataFile,ReleaseFileTypes.Metadata, metaDataInfo);
                            await _importService.Import(dataFile.FileName.ToLower(), metadataFile.FileName.ToLower(),
                                    releaseId, dataFile);
                            return true;
                        });
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

                    var files = await _context.ReleaseFiles
                        .Include(f => f.ReleaseFileReference)
                        .Where(f => f.ReleaseId == releaseId && ReleaseFileTypes.Chart == f.ReleaseFileReference.ReleaseFileType)
                        .ToListAsync();

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

        public Task<Either<ActionResult, bool>> DeleteDataFilesAsync(Guid releaseId, string dataFileName)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async () =>
                {
                    var blobContainer = await GetCloudBlobContainer();
                    var metaFilename = await GetFilenameAssociatedToType(releaseId, dataFileName, ReleaseFileTypes.Data, ReleaseFileTypes.Metadata);

                    if (await DeletionWillOrphanFileAsync(releaseId, dataFileName, ReleaseFileTypes.Data))
                    {
                        await _importService.RemoveImportTableRowIfExists(releaseId, dataFileName);
                        await DeleteFileFromStorageAsync(blobContainer, AdminReleasePath(releaseId, ReleaseFileTypes.Data, dataFileName));
                        await DeleteFileFromStorageAsync(blobContainer, AdminReleasePath(releaseId, ReleaseFileTypes.Metadata, metaFilename));
                        await DeleteFileReference(releaseId, dataFileName, ReleaseFileTypes.Data);
                        await DeleteFileReference(releaseId, metaFilename, ReleaseFileTypes.Metadata);
                    }
                    else
                    {
                        await DeleteFileLink(releaseId, dataFileName, ReleaseFileTypes.Data);
                        await DeleteFileLink(releaseId, metaFilename, ReleaseFileTypes.Metadata);
                    }

                    await _context.SaveChangesAsync();
                    return true;
                });
        }

        public Task<Either<ActionResult, FileInfo>> UploadFileAsync(Guid releaseId,
            IFormFile file, string name, ReleaseFileTypes type, bool overwrite)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async () => await _fileUploadsValidatorService.ValidateUploadFileType(file, type))
                .OnSuccess(async release =>
                {
                    var blobContainer = await GetCloudBlobContainer();
                    var info = new Dictionary<string, string> {{NameKey, name.ToLower()}};
                    return await _fileUploadsValidatorService.ValidateFileForUpload(releaseId, file, type, overwrite)
                            .OnSuccess(async () =>
                            {
                                await CreateBasicFileLink(file.FileName.ToLower(), releaseId, type);
                                await _context.SaveChangesAsync();
                                await UploadFileToStorageAsync(blobContainer, releaseId, file, type, info);
                                var blob = blobContainer.GetBlobReference(AdminReleasePath(releaseId, type, file.FileName.ToLower()));
                                return GetFileInfo(blob);
                            });
                });
        }

        public Task<Either<ActionResult, FileInfo>> UploadChartFileAsync(Guid releaseId, IFormFile file)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async () => await _fileUploadsValidatorService.ValidateUploadFileType(file, ReleaseFileTypes.Chart))
                .OnSuccess(async release =>
                {
                    var blobContainer = await GetCloudBlobContainer();
                    var info = new Dictionary<string, string> {{NameKey, file.FileName.ToLower()}};
                    return await _fileUploadsValidatorService.ValidateFileForUpload(releaseId, file, ReleaseFileTypes.Chart, true)
                            .OnSuccess(async () =>
                            {
                                await CreateBasicFileLink(file.FileName.ToLower(), releaseId, ReleaseFileTypes.Chart);
                                await UploadFileToStorageAsync(blobContainer, releaseId, file,ReleaseFileTypes.Chart, info);
                                await _context.SaveChangesAsync();
                                var blob = blobContainer.GetBlobReference(AdminReleasePath(releaseId,ReleaseFileTypes.Chart, file.FileName.ToLower()));
                                return GetFileInfo(blob);
                            });
                });
        }

        public async Task<Either<ActionResult, bool>> DeleteNonDataFileAsync(Guid releaseId,
            ReleaseFileTypes type, string fileName)
        {
            return await DeleteNonDataFilesAsync(releaseId, type, new List<string>(){fileName});
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
                        if (await DeletionWillOrphanFileAsync(releaseId, fileName, type))
                        {
                            await DeleteFileFromStorageAsync(await GetCloudBlobContainer(), AdminReleasePath(releaseId, type, fileName));
                            await DeleteFileReference(releaseId, fileName, type);
                        }
                        else
                        {
                            await DeleteFileLink(releaseId, fileName, type);
                        }
                    }

                    await _context.SaveChangesAsync();
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

                    var files = await _context
                        .ReleaseFiles
                        .Include(f => f.ReleaseFileReference)
                        .Where(f => f.ReleaseId == releaseId && types.Contains(f.ReleaseFileReference.ReleaseFileType))
                        .ToListAsync();

                    var filesWithMetadata = files
                        .Select(async fileLink =>
                        {
                            var fileReference = fileLink.ReleaseFileReference;
                            var blobPath = AdminReleasePathWithFileReference(fileReference);

                            // Files should exists in storage but if not then allow user to delete
                            if (!blobContainer.GetBlockBlobReference(blobPath).Exists())
                            {
                                // Fail the import if this was a datafile upload
                                if (fileReference.ReleaseFileType == ReleaseFileTypes.Data ||
                                    fileReference.ReleaseFileType == ReleaseFileTypes.Metadata)
                                {
                                    await _importService.FailImport(releaseId,
                                        fileReference.ReleaseFileType == ReleaseFileTypes.Data
                                            ? fileReference.Filename
                                            : await GetFilenameAssociatedToType(releaseId, fileReference.Filename,
                                                ReleaseFileTypes.Metadata, ReleaseFileTypes.Data),
                                        new List<ValidationError>
                                                {
                                                    new ValidationError("Files not uploaded correctly. Please delete and retry")
                                                }.AsEnumerable());
                                }
                                
                                return new Models.FileInfo
                                {
                                    Extension = Path.GetExtension(fileReference.Filename),
                                    Name = "Unknown",
                                    Path = fileReference.Filename,
                                    Size = "0.00 B",
                                    MetaFileName = fileReference.ReleaseFileType == ReleaseFileTypes.Data ? 
                                        await GetFilenameAssociatedToType(releaseId, fileReference.Filename, ReleaseFileTypes.Data, ReleaseFileTypes.Metadata)
                                        : "",
                                    Rows = 0,
                                    UserName = "",
                                    Created = DateTimeOffset.UtcNow,
                                };
                            }
                            
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
                    var blob = blobContainer.GetBlockBlobReference(AdminReleasePathWithFileReference(fileLink.ReleaseFileReference));

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

        private async Task CreateBasicFileLink(string filename, Guid releaseId, ReleaseFileTypes type)
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
        }

        private async Task DeleteFileLink(Guid releaseId, string filename, ReleaseFileTypes type)
        {
            var fileLink = await GetReleaseFileLinkAsync(releaseId, filename, type);
            _context.ReleaseFiles.Remove(fileLink);
        }

        private async Task<ReleaseFile> GetReleaseFileLinkAsync(Guid releaseId, string filename, ReleaseFileTypes type)
        {
            var releaseFileLinks = await _context
                .ReleaseFiles
                .Include(f => f.ReleaseFileReference)
                .Where(f => f.ReleaseId == releaseId && f.ReleaseFileReference.ReleaseFileType == type)
                .ToListAsync();
            
            // Make sure the filename predicate is case sensitive by executing in memory rather than in the db
            return releaseFileLinks.FirstOrDefault(file => file.ReleaseFileReference.Filename == filename);
        }

        private async Task DeleteFileReference(Guid releaseId, string filename,
            ReleaseFileTypes type)
        {
            var fileLink = await GetReleaseFileLinkAsync(releaseId, filename, type);
            _context.ReleaseFileReferences.Remove(fileLink.ReleaseFileReference);
        }

        private static async Task UploadFileToStorageAsync(CloudBlobContainer blobContainer,
            Guid releaseId, IFormFile file, ReleaseFileTypes type, IDictionary<string, string> metaValues)
        {
            var blob = blobContainer.GetBlockBlobReference(AdminReleasePath(releaseId, type, file.FileName.ToLower()));
            blob.Properties.ContentType = file.ContentType;
            var path = await UploadToTemporaryFile(file);
            await blob.UploadFromFileAsync(path);
            metaValues["NumberOfRows"] = CalculateNumberOfRows(file.OpenReadStream()).ToString();
            await AddMetaValuesAsync(blob, metaValues);
        }

        private static async Task DeleteFileFromStorageAsync(CloudBlobContainer blobContainer, string path)
        {
            await blobContainer.GetBlockBlobReference(path).DeleteIfExistsAsync();
        }

        private async Task<CloudBlobContainer> GetCloudBlobContainer()
        {
            return await GetCloudBlobContainerAsync(_storageConnectionString, PrivateFilesContainerName);
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

        private async Task<bool> DeletionWillOrphanFileAsync(Guid releaseId, string filename,
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

        private async Task<string> GetFilenameAssociatedToType(Guid releaseId, string filename, ReleaseFileTypes type, ReleaseFileTypes associatedType)
        {
            var releaseDataFileLink = await GetReleaseFileLinkAsync(releaseId, filename, type);

            var associatedFileRef = await _context.ReleaseFileReferences
                .FirstAsync(rfr => rfr.ReleaseId == releaseDataFileLink.ReleaseFileReference.ReleaseId
                              && rfr.ReleaseFileType == associatedType
                              && rfr.SubjectId == releaseDataFileLink.ReleaseFileReference.SubjectId);
            return associatedFileRef.Filename;
        }
    }
}