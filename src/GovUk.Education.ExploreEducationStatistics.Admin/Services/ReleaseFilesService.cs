using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
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
        private readonly ISubjectService _subjectService;
        private readonly IDataZipArchiveService _dataZipArchiveService;

        private const string NameKey = "name";

        public ReleaseFilesService(IConfiguration config, IUserService userService,
            IPersistenceHelper<ContentDbContext> persistenceHelper, ContentDbContext context,
            IImportService importService, IFileUploadsValidatorService fileUploadsValidatorService,
            ISubjectService subjectService, IDataZipArchiveService dataZipArchiveService)
        {
            _storageConnectionString = config.GetValue<string>("CoreStorage");
            _userService = userService;
            _persistenceHelper = persistenceHelper;
            _context = context;
            _importService = importService;
            _fileUploadsValidatorService = fileUploadsValidatorService;
            _subjectService = subjectService;
            _dataZipArchiveService = dataZipArchiveService;
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

        public Task<Either<ActionResult, DataFileInfo>> UploadDataFilesAsync(Guid releaseId,
            IFormFile dataFile, IFormFile metadataFile, string name, string userName)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async release =>
                {
                    var blobContainer = await GetCloudBlobContainer();
                    var dataInfo = new Dictionary<string, string>{{NameKey, name}, {MetaFileKey, metadataFile.FileName.ToLower()}, {UserName, userName}};
                    var metaDataInfo = new Dictionary<string, string>{{DataFileKey, dataFile.FileName.ToLower()}, {UserName, userName}};

                    return await _fileUploadsValidatorService
                        .ValidateDataFilesForUpload(releaseId, dataFile, metadataFile, name)
                        // First, create with status uploading to prevent other users uploading the same datafile
                        .OnSuccess(async () => await _importService.CreateImportTableRow(releaseId, dataFile.FileName.ToLower()))
                        .OnSuccess(async () =>
                        {
                            var fileReference =  await CreateOrUpdateFileReference(dataFile.FileName.ToLower(), releaseId, ReleaseFileTypes.Data);
                            await CreateOrUpdateFileReference(metadataFile.FileName.ToLower(), releaseId,ReleaseFileTypes.Metadata);
                            await _context.SaveChangesAsync();
                            await UploadFileToStorageAsync(blobContainer, releaseId, dataFile, ReleaseFileTypes.Data, dataInfo);
                            await UploadFileToStorageAsync(blobContainer, releaseId, metadataFile,ReleaseFileTypes.Metadata, metaDataInfo);
                            await _importService.Import(releaseId, dataFile.FileName.ToLower(),
                                metadataFile.FileName.ToLower(), dataFile, false);

                            var file = blobContainer.GetBlockBlobReference(
                                AdminReleasePathWithFileReference(fileReference)
                            );
                            await file.FetchAttributesAsync();

                            return new DataFileInfo
                            {
                                Id = fileReference.Id,
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
                });
        }

        public Task<Either<ActionResult, bool>> UploadDataFilesAsZipAsync(Guid releaseId,
            IFormFile zipFile, string name, string userName)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async release =>
                {
                    var blobContainer = await GetCloudBlobContainer();

                    return await _dataZipArchiveService.GetArchiveEntries(blobContainer, releaseId, zipFile)
                        .OnSuccess(async dataFiles =>
                        {
                            var dataFile = dataFiles.Item1;
                            var metadataFile = dataFiles.Item2;
                            var dataInfo = new Dictionary<string, string>{{NameKey, name}, {MetaFileKey, metadataFile.Name.ToLower()}, {UserName, userName}};

                            return await _fileUploadsValidatorService.ValidateZippedDataFileForUpload(releaseId, dataFile, metadataFile, name)
                                .OnSuccess(async () => await _importService.CreateImportTableRow(releaseId, dataFile.Name.ToLower()))
                                .OnSuccess(async () =>
                                {
                                    var source = await CreateOrUpdateFileReference(zipFile.FileName.ToLower(), releaseId, ReleaseFileTypes.DataZip);
                                    await CreateOrUpdateFileReference(dataFile.Name.ToLower(), releaseId, ReleaseFileTypes.Data, null, source);
                                    await CreateOrUpdateFileReference(metadataFile.Name.ToLower(), releaseId,ReleaseFileTypes.Metadata, null ,source);
                                    await _context.SaveChangesAsync();
                                    await UploadFileToStorageAsync(blobContainer, releaseId, zipFile, ReleaseFileTypes.DataZip, dataInfo);
                                    await _importService.Import(releaseId, dataFile.Name.ToLower(),
                                        metadataFile.Name.ToLower(), zipFile, true);                                    return true;
                                });
                        });
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
                                var releaseFileReference = await CreateOrUpdateFileReference(file.FileName.ToLower(), releaseId, type);
                                await _context.SaveChangesAsync();
                                await UploadFileToStorageAsync(blobContainer, releaseId, file, type, info);
                                var blob = blobContainer.GetBlobReference(AdminReleasePath(releaseId, type, file.FileName.ToLower()));
                                return GetFileInfo(releaseFileReference.Id, blob);
                            });
                });
        }

        public Task<Either<ActionResult, FileInfo>> UploadChartFileAsync(Guid releaseId, IFormFile file, Guid? id = null)
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
                                var releaseFileReference = await CreateOrUpdateFileReference(file.FileName.ToLower(), releaseId, ReleaseFileTypes.Chart, id);
                                await _context.SaveChangesAsync();
                                await UploadChartFileToStorageAsync(blobContainer, releaseId, file, info, releaseFileReference.Id);
                                var blob = blobContainer.GetBlobReference(AdminReleasePath(releaseId,ReleaseFileTypes.Chart, releaseFileReference.Id.ToString()));
                                return GetFileInfo(releaseFileReference.Id, blob);
                            });
                });
        }

        public async Task<Either<ActionResult, bool>> DeleteChartFilesAsync(Guid releaseId, IEnumerable<Guid> fileIds)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async () =>
                {
                    foreach (var id in fileIds)
                    {
                        if (await DeletionWillOrphanFileAsync(releaseId, id))
                        {
                            var fileReference = await _context.ReleaseFileReferences.SingleAsync(rfr => rfr.Id == id);
                            await DeleteFileFromStorageAsync(await GetCloudBlobContainer(),
                                AdminReleasePathWithFileReference(fileReference));
                            _context.ReleaseFileReferences.Remove(fileReference);
                        }
                        else
                        {
                            var releaseFile = await _context.ReleaseFiles.SingleAsync(rf =>
                                rf.ReleaseId == releaseId && rf.ReleaseFileReferenceId == id);
                            _context.ReleaseFiles.Remove(releaseFile);
                        }
                    }

                    await _context.SaveChangesAsync();
                    return true;
                });
        }

        public async Task<Either<ActionResult, bool>> DeleteChartFileAsync(Guid releaseId, Guid id)
        {
            return await DeleteChartFilesAsync(releaseId, new List<Guid>() {id});
        }

        public async Task<Either<ActionResult, bool>> DeleteNonDataFileAsync(Guid releaseId,
            ReleaseFileTypes type, string fileName)
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
                    if (await DeletionWillOrphanFileAsync(releaseId, fileName, type))
                    {
                        await DeleteFileFromStorageAsync(await GetCloudBlobContainer(), AdminReleasePath(releaseId, type, fileName));
                        await DeleteFileReference(releaseId, fileName, type);
                    }
                    else
                    {
                        await DeleteFileLink(releaseId, fileName, type);
                    }

                    await _context.SaveChangesAsync();
                    return true;
                });
        }

        public async Task<Either<ActionResult, IEnumerable<FileInfo>>> ListFilesAsync(Guid releaseId,
            params ReleaseFileTypes[] types)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async _ =>
                {
                    var blobContainer = await GetCloudBlobContainer();

                    var files = await GetReleaseFiles(releaseId, types);

                    var filesWithMetadata = files
                        .Select(async fileLink =>
                        {
                            var fileReference = fileLink.ReleaseFileReference;
                            var blobPath = AdminReleasePathWithFileReference(fileReference);

                            // Files should exists in storage but if not then allow user to delete
                            if (!blobContainer.GetBlockBlobReference(blobPath).Exists())
                            {
                                return new FileInfo
                                {
                                    Id = fileReference.Id,
                                    Extension = Path.GetExtension(fileReference.Filename),
                                    Name = "Unkown",
                                    Path = fileReference.Filename,
                                    Size = "0.00 B"
                                };
                            }

                            var file = blobContainer.GetBlockBlobReference(blobPath);
                            await file.FetchAttributesAsync();
                            return new FileInfo
                            {
                                Id = fileReference.Id,
                                Extension = GetExtension(file),
                                Name = GetName(file),
                                Path = file.Name,
                                Size = GetSize(file)
                            };
                        });

                    return (await Task.WhenAll(filesWithMetadata))
                        .OrderBy(file => file.Name)
                        .AsEnumerable();
                });
        }

        public async Task<Either<ActionResult, IEnumerable<DataFileInfo>>> ListDataFilesAsync(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async _ =>
                {
                    var blobContainer = await GetCloudBlobContainer();
                    var files = await GetReleaseFiles(releaseId, ReleaseFileTypes.Data, ReleaseFileTypes.Metadata);
                    var fileList = new List<DataFileInfo>();

                    foreach (var fileLink in files)
                    {
                        fileList.Add(await GetDataFileInfo(releaseId, fileLink, blobContainer));
                    }

                    return fileList
                        .OrderBy(file => file.Name)
                        .AsEnumerable();
                });
        }

        public async Task<Either<ActionResult, Unit>> DeleteAllFiles(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(
                    async release =>
                    {
                        // Exclude metadata as they'll be deleted alongside the data file
                        var files = (await GetAllReleaseFiles(release.Id))
                            .Where(file => file.ReleaseFileReference.ReleaseFileType != ReleaseFileTypes.Metadata)
                            .Select(file => file.ReleaseFileReference);

                        foreach (var file in files)
                        {
                            switch (file.ReleaseFileType)
                            {
                                case ReleaseFileTypes.Chart:
                                    await DeleteChartFileAsync(release.Id, file.Id);
                                    break;
                                case ReleaseFileTypes.Data:
                                    await DeleteDataFilesAsync(release.Id, file.Filename);
                                    break;
                                default:
                                    await DeleteNonDataFileAsync(
                                        release.Id,
                                        file.ReleaseFileType,
                                        file.Filename
                                    );
                                    break;
                            }
                        }
                    }
                );
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
                    return await GetStreamedFile(fileLink);
                });
        }

        public async Task<Either<ActionResult, FileStreamResult>> StreamFile(Guid releaseId,
            ReleaseFileTypes type, Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async _ =>
                {
                    var fileLink = await GetReleaseFileLinkAsync(releaseId, id);
                    return await GetStreamedFile(fileLink);
                });
        }

        private async Task<Either<ActionResult, FileStreamResult>> GetStreamedFile(ReleaseFile fileLink)
        {
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
                FileDownloadName = fileLink.ReleaseFileReference.Filename
            };
        }

        private async Task<ReleaseFileReference> CreateOrUpdateFileReference(string filename, Guid releaseId,
            ReleaseFileTypes type, Guid? id = null, ReleaseFileReference? source = null)
        {
            ReleaseFileReference releaseFileReference;
            
            // If updating existing then check if existing reference is for this release - if not then create new ref
            if (id != null)
            {
                releaseFileReference = await _context.ReleaseFileReferences.Where(rfr => rfr.Id == id).FirstAsync();
                if (releaseFileReference.ReleaseId == releaseId)
                {
                    return await UpdateFileReference(filename, id.Value);
                }
            }

            releaseFileReference = new ReleaseFileReference
            {
                ReleaseId = releaseId,
                Filename = filename,
                ReleaseFileType = type,
                Source = source
            };
            
            var entry = await _context.ReleaseFileReferences.AddAsync(releaseFileReference);  

            // No ReleaseFileLink required for the zip file source reference
            if (type != ReleaseFileTypes.DataZip)
            {
                var fileLink = new ReleaseFile
                {
                    ReleaseId = releaseId,
                    ReleaseFileReference = releaseFileReference
                };

                await _context.ReleaseFiles.AddAsync(fileLink);
            }
            
            return entry.Entity;
        }

        private async Task<DataFileInfo> GetDataFileInfo(Guid releaseId, ReleaseFile fileLink, CloudBlobContainer blobContainer)
        {
            var fileReference = fileLink.ReleaseFileReference;
            var blobPath = AdminReleasePathWithFileReference(fileReference);

            // Files should exists in storage but if not then allow user to delete
            if (!blobContainer.GetBlockBlobReference(blobPath).Exists())
            {
                // Try to get the name from the zip file if existing
                if (fileReference.SourceId != null)
                {
                    var source = await GetReleaseFileReference(fileReference.SourceId.Value);
                    blobPath = AdminReleasePathWithFileReference(source);
                    if (blobContainer.GetBlockBlobReference(blobPath).Exists())
                    {
                        var zipFile = blobContainer.GetBlockBlobReference(blobPath);
                        await zipFile.FetchAttributesAsync();
                        return new DataFileInfo
                        {
                            Id = fileReference.Id,
                            Extension = Path.GetExtension(fileReference.Filename),
                            Name = GetName(zipFile),
                            Path = fileLink.ReleaseFileReference.Filename,
                            Size = GetSize(zipFile),
                            MetaFileName = fileLink.ReleaseFileReference.ReleaseFileType == ReleaseFileTypes.Data ? GetMetaFileName(zipFile) : "",
                            Rows = 0,
                            UserName = GetUserName(zipFile),
                            Created = zipFile.Properties.Created
                        };
                    }
                }

                // Fail the import if this was a datafile upload
                await _importService.FailImport(releaseId,
                    fileReference.ReleaseFileType == ReleaseFileTypes.Data
                        ? fileReference.Filename
                        : await GetFilenameAssociatedToType(releaseId, fileReference.Filename,
                            ReleaseFileTypes.Metadata, ReleaseFileTypes.Data),
                    new List<ValidationError>
                    {
                        new ValidationError("Files not uploaded correctly. Please delete and retry")
                    }.AsEnumerable());

                return new DataFileInfo
                {
                    Id = fileReference.Id,
                    Extension = Path.GetExtension(fileReference.Filename),
                    Name = await GetSubjectName(releaseId, fileReference.Filename,
                        fileReference.ReleaseFileType),
                    Path = fileReference.Filename,
                    Size = "0.00 B",
                    MetaFileName = fileReference.ReleaseFileType == ReleaseFileTypes.Data
                        ? await GetFilenameAssociatedToType(releaseId, fileReference.Filename,
                            ReleaseFileTypes.Data, ReleaseFileTypes.Metadata)
                        : "",
                    Rows = 0,
                    UserName = "",
                    Created = DateTimeOffset.UtcNow,
                };
            }

            var file = blobContainer.GetBlockBlobReference(blobPath);
            await file.FetchAttributesAsync();
            return new DataFileInfo
            {
                Id = fileReference.Id,
                Extension = GetExtension(file),
                Name = GetName(file),
                Path = file.Name,
                Size = GetSize(file),
                MetaFileName = GetMetaFileName(file),
                Rows = GetNumberOfRows(file),
                UserName = GetUserName(file),
                Created = file.Properties.Created
            };
        }

        private async Task<ReleaseFileReference> UpdateFileReference(string filename, Guid id)
        {
            var releaseFileReference = await _context.ReleaseFileReferences.Where(rfr => rfr.Id == id).FirstAsync();
            releaseFileReference.Filename = filename;
            return _context.ReleaseFileReferences.Update(releaseFileReference).Entity;
        }

        private async Task DeleteFileLink(Guid releaseId, string filename, ReleaseFileTypes type)
        {
            var fileLink = await GetReleaseFileLinkAsync(releaseId, filename, type);
            _context.ReleaseFiles.Remove(fileLink);
        }

        private async Task<List<ReleaseFile>> GetAllReleaseFiles(Guid releaseId)
        {
            return await _context
                .ReleaseFiles
                .Include(f => f.ReleaseFileReference)
                .AsNoTracking()
                .Where(f => f.ReleaseId == releaseId)
                .ToListAsync();
        }

        private async Task<List<ReleaseFile>> GetReleaseFiles(Guid releaseId, params ReleaseFileTypes[] types)
        {
            return await _context
                .ReleaseFiles
                .Include(f => f.ReleaseFileReference)
                .Where(f => f.ReleaseId == releaseId && types.Contains(f.ReleaseFileReference.ReleaseFileType))
                .ToListAsync();
        }
        
        private async Task<ReleaseFileReference> GetReleaseFileReference(Guid id)
        {
            return await _context
                .ReleaseFileReferences
                .SingleAsync(rfr => rfr.Id == id);
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

        private async Task<ReleaseFile> GetReleaseFileLinkAsync(Guid releaseId, Guid id)
        {
            return await _context
                .ReleaseFiles
                .Include(f => f.ReleaseFileReference)
                .Where(f => f.ReleaseId == releaseId && f.ReleaseFileReferenceId == id)
                .SingleAsync();
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

        private static async Task UploadChartFileToStorageAsync(CloudBlobContainer blobContainer,
            Guid releaseId, IFormFile file, IDictionary<string, string> metaValues, Guid id)
        {
            var blob = blobContainer.GetBlockBlobReference(AdminReleasePath(releaseId, ReleaseFileTypes.Chart, id.ToString()));
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

        private async Task<bool> DeletionWillOrphanFileAsync(Guid releaseId, Guid id)
        {
            var fileLink = await GetReleaseFileLinkAsync(releaseId, id);

            var otherFileReferences = await _context
                .ReleaseFiles
                .CountAsync(f => f.ReleaseFileReferenceId == fileLink.ReleaseFileReferenceId && f.Id != fileLink.Id);

            return otherFileReferences == 0;
        }

        private string AdminReleasePathWithFileReference(ReleaseFileReference fileReference)
        {
            return AdminReleasePath(fileReference.ReleaseId, fileReference.ReleaseFileType,
                fileReference.ReleaseFileType == ReleaseFileTypes.Chart ? fileReference.Id.ToString() : fileReference.Filename);
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

        private async Task<string> GetSubjectName(Guid releaseId, string filename, ReleaseFileTypes type)
        {
            // TODO Need to get back to the originating subject to get the name which is used in the delete plan
            // Seems convoluted so flagging as future work
            var releaseDataFileLink = await GetReleaseFileLinkAsync(releaseId, filename, type);

            var associatedFileRef = await _context.ReleaseFileReferences
                .FirstAsync(rfr => rfr.ReleaseId == releaseDataFileLink.ReleaseFileReference.ReleaseId
                                   && rfr.ReleaseFileType == type
                                   && rfr.Filename == filename);

            if (associatedFileRef?.SubjectId != null)
            {
                var subject = await _subjectService.GetAsync(associatedFileRef.SubjectId.Value);
                return subject.Name;
            }

            return "Unknown";
        }
    }
}