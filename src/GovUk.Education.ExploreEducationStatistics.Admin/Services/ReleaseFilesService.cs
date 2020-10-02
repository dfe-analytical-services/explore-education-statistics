using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStorageUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;
using static GovUk.Education.ExploreEducationStatistics.Common.Extensions.BlobInfoExtensions;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.FileInfo;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseFilesService : IReleaseFilesService
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;
        private readonly ContentDbContext _context;
        private readonly IImportService _importService;
        private readonly IFileUploadsValidatorService _fileUploadsValidatorService;
        private readonly ISubjectService _subjectService;
        private readonly IDataArchiveValidationService _dataArchiveValidationService;
        private readonly IImportStatusService _importStatusService;

        public ReleaseFilesService(
            IBlobStorageService blobStorageService,
            IUserService userService,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            ContentDbContext context,
            IImportService importService,
            IFileUploadsValidatorService fileUploadsValidatorService,
            ISubjectService subjectService,
            IDataArchiveValidationService dataArchiveValidationService,
            IImportStatusService importStatusService)
        {
            _blobStorageService = blobStorageService;
            _userService = userService;
            _persistenceHelper = persistenceHelper;
            _context = context;
            _importService = importService;
            _fileUploadsValidatorService = fileUploadsValidatorService;
            _subjectService = subjectService;
            _dataArchiveValidationService = dataArchiveValidationService;
            _importStatusService = importStatusService;
        }

        public async Task<Either<ActionResult, IEnumerable<FileInfo>>> ListPublicFilesPreview(
            Guid releaseId,
            IEnumerable<Guid> referencedReleaseVersions)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async release =>
                    {
                        var files = new List<BlobInfo>();

                        foreach (var version in referencedReleaseVersions)
                        {
                            files.AddRange(
                                await _blobStorageService.ListBlobs(
                                    PrivateFilesContainerName,
                                    AdminReleaseDirectoryPath(version, ReleaseFileTypes.Data)
                                )
                            );
                            files.AddRange(
                                await _blobStorageService.ListBlobs(
                                    PrivateFilesContainerName,
                                    AdminReleaseDirectoryPath(version, ReleaseFileTypes.Ancillary)
                                )
                            );
                        }

                        return files
                            .Where(blob => !blob.IsMetaDataFile())
                            .Select(blob => blob.ToFileInfo())
                            .OrderBy(f => f.Name)
                            .AsEnumerable();
                    }
                );
        }

        public Task<Either<ActionResult, DataFileInfo>> UploadDataFiles(Guid releaseId,
            IFormFile dataFile,
            IFormFile metadataFile,
            string userName,
            Guid? replacingFileId = null,
            string subjectName = null)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async release =>
                {
                    return await _persistenceHelper.CheckOptionalEntityExists<ReleaseFileReference>(replacingFileId)
                        .OnSuccess(async replacingFile =>
                        {
                            return await ValidateSubjectName(releaseId, subjectName, replacingFile)
                                .OnSuccess(validSubjectName => _fileUploadsValidatorService
                                    .ValidateDataFilesForUpload(releaseId, dataFile, metadataFile)
                                    // First, create with status uploading to prevent other users uploading the same datafile
                                    .OnSuccess(async () =>
                                        await _importService.CreateImportTableRow(releaseId,
                                            dataFile.FileName.ToLower()))
                                    .OnSuccess(async () =>
                                    {
                                        var fileReference = await CreateOrUpdateFileReference(
                                            filename: dataFile.FileName.ToLower(),
                                            releaseId: releaseId,
                                            type: ReleaseFileTypes.Data,
                                            replacingFile: replacingFile);

                                        var metaFileReference = await CreateOrUpdateFileReference(
                                            filename: metadataFile.FileName.ToLower(),
                                            releaseId: releaseId,
                                            ReleaseFileTypes.Metadata);

                                        await _context.SaveChangesAsync();

                                        var dataInfo = GetDataFileMetaValues(
                                            name: validSubjectName,
                                            metaFileName: metadataFile.FileName,
                                            userName: userName,
                                            numberOfRows: CalculateNumberOfRows(dataFile.OpenReadStream())
                                        );
                                        var metaDataInfo = GetMetaDataFileMetaValues(
                                            dataFileName: dataFile.FileName,
                                            userName: userName,
                                            numberOfRows: CalculateNumberOfRows(metadataFile.OpenReadStream())
                                        );

                                        await UploadFileToStorage(releaseId, dataFile, ReleaseFileTypes.Data, dataInfo);
                                        await UploadFileToStorage(releaseId, metadataFile, ReleaseFileTypes.Metadata,
                                            metaDataInfo);

                                        await _importService.Import(releaseId, dataFile.FileName.ToLower(),
                                            metadataFile.FileName.ToLower(), dataFile, false);

                                        var blob = await _blobStorageService.GetBlob(
                                            PrivateFilesContainerName,
                                            AdminReleasePathWithFileReference(fileReference)
                                        );

                                        return new DataFileInfo
                                        {
                                            Id = fileReference.Id,
                                            Extension = blob.Extension,
                                            Name = blob.Name,
                                            Path = blob.Path,
                                            Size = blob.Size,
                                            MetaFileId = metaFileReference.Id,
                                            MetaFileName = blob.GetMetaFileName(),
                                            Rows = blob.GetNumberOfRows(),
                                            UserName = blob.GetUserName(),
                                            Status = IStatus.QUEUED,
                                            Created = blob.Created
                                        };
                                    }));
                        });
                });
        }

        public Task<Either<ActionResult, DataFileInfo>> UploadDataFilesAsZip(Guid releaseId,
            IFormFile zipFile,
            string userName,
            Guid? replacingFileId = null,
            string subjectName = null)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async release =>
                {
                    return await _persistenceHelper.CheckOptionalEntityExists<ReleaseFileReference>(replacingFileId)
                        .OnSuccess(async replacingFile =>
                        {
                            return await ValidateSubjectName(releaseId, subjectName, replacingFile)
                                .OnSuccess(validSubjectName =>
                                    _dataArchiveValidationService.ValidateDataArchiveFile(releaseId, zipFile)
                                .OnSuccess(async archiveFile =>
                                {
                                    var dataInfo = GetDataFileMetaValues(
                                        name: validSubjectName,
                                        metaFileName: archiveFile.MetaFileName,
                                        userName: userName,
                                        numberOfRows: 0
                                    );

                                    return await _fileUploadsValidatorService
                                        .ValidateDataArchiveEntriesForUpload(releaseId, archiveFile)
                                        .OnSuccess(async () =>
                                            await _importService.CreateImportTableRow(
                                                releaseId,
                                                archiveFile.DataFileName))
                                        .OnSuccess(async () =>
                                        {
                                            var source = await CreateOrUpdateFileReference(
                                                filename: zipFile.FileName.ToLower(),
                                                releaseId: releaseId,
                                                type: ReleaseFileTypes.DataZip);

                                            var dataFileReference = await CreateOrUpdateFileReference(
                                                filename: archiveFile.DataFileName,
                                                releaseId: releaseId,
                                                type: ReleaseFileTypes.Data,
                                                id: null,
                                                replacingFile: replacingFile,
                                                source: source);

                                            var metaFileReference = await CreateOrUpdateFileReference(
                                                filename: archiveFile.MetaFileName,
                                                releaseId: releaseId,
                                                type: ReleaseFileTypes.Metadata,
                                                id: null,
                                                source: source);

                                            await _context.SaveChangesAsync();

                                            await UploadFileToStorage(releaseId, zipFile, ReleaseFileTypes.DataZip,
                                                dataInfo);

                                            await _importService.Import(
                                                releaseId,
                                                dataFileName: archiveFile.DataFileName,
                                                metaFileName: archiveFile.MetaFileName,
                                                zipFile,
                                                true);

                                            var blob = await _blobStorageService.GetBlob(
                                                PrivateFilesContainerName,
                                                AdminReleasePathWithFileReference(source)
                                            );

                                            return new DataFileInfo
                                            {
                                                // TODO size and rows are for zip file but they need to be for
                                                // the datafile which isn't extracted yet
                                                Id = dataFileReference.Id,
                                                Extension = dataFileReference.Extension,
                                                Name = validSubjectName,
                                                Path = dataFileReference.Filename,
                                                Size = blob.Size,
                                                MetaFileId = metaFileReference.Id,
                                                MetaFileName = blob.GetMetaFileName(),
                                                Rows = blob.GetNumberOfRows(),
                                                UserName = blob.GetUserName(),
                                                Status = IStatus.QUEUED,
                                                Created = blob.Created
                                            };
                                        });
                                }));
                        });
                });
        }

        public Task<Either<ActionResult, Unit>> DeleteDataFiles(Guid releaseId, Guid fileId, bool forceDelete = false)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(async release => await CheckCanDeleteReleaseFile(release, forceDelete))
                .OnSuccess(() => CheckReleaseFileReferenceExists(fileId))
                .OnSuccessVoid(async releaseFileReference =>
                {
                    var metaReleaseFileReference = await GetAssociatedReleaseFileReference(releaseFileReference, ReleaseFileTypes.Metadata);

                    if (await DeletionWillOrphanFileAsync(releaseId, fileId))
                    {
                        await _importService.RemoveImportTableRowIfExists(releaseId, releaseFileReference.Filename);
                        await _blobStorageService.DeleteBlob(
                            PrivateFilesContainerName,
                            AdminReleasePath(releaseId, ReleaseFileTypes.Data, releaseFileReference.Filename)
                        );
                        await _blobStorageService.DeleteBlob(
                            PrivateFilesContainerName,
                            AdminReleasePath(releaseId, ReleaseFileTypes.Metadata, metaReleaseFileReference.Filename)
                        );

                        await DeleteFileLink(releaseId, releaseFileReference.Id);
                        await DeleteFileLink(releaseId, metaReleaseFileReference.Id);

                        _context.ReleaseFileReferences.Remove(releaseFileReference);
                        _context.ReleaseFileReferences.Remove(metaReleaseFileReference);

                        if (releaseFileReference.SourceId.HasValue)
                        {
                            var sourceRef = await GetReleaseFileReference(releaseFileReference.SourceId.Value);
                            await _blobStorageService.DeleteBlob(
                                PrivateFilesContainerName,
                                AdminReleasePath(releaseId, ReleaseFileTypes.DataZip, sourceRef.Filename)
                            );
                            // N.B. No ReleaseFiles row for source links
                            _context.ReleaseFileReferences.Remove(sourceRef);
                        }
                    }
                    else
                    {
                        await DeleteFileLink(releaseId, releaseFileReference.Id);
                        await DeleteFileLink(releaseId, metaReleaseFileReference.Id);
                    }

                    await _context.SaveChangesAsync();
                });
        }

        public Task<Either<ActionResult, FileInfo>> UploadFile(Guid releaseId,
            IFormFile file, string name, ReleaseFileTypes type, bool overwrite)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async () => await _fileUploadsValidatorService.ValidateUploadFileType(file, type))
                .OnSuccess(async release =>
                {
                    var info = new Dictionary<string, string>
                    {
                        {NameKey, name.ToLower()}
                    };

                    return await _fileUploadsValidatorService.ValidateFileForUpload(releaseId, file, type, overwrite)
                            .OnSuccess(async () => await _fileUploadsValidatorService.ValidateFileUploadName(name))
                            .OnSuccess(async () =>
                            {
                                var releaseFileReference = await CreateOrUpdateFileReference(file.FileName.ToLower(), releaseId, type);
                                await _context.SaveChangesAsync();

                                await UploadFileToStorage(releaseId, file, type, info);

                                var blob = await _blobStorageService.GetBlob(
                                    PrivateFilesContainerName,
                                    AdminReleasePath(releaseId, type, file.FileName.ToLower())
                                );

                                return blob.ToFileInfo(releaseFileReference.Id);
                            });
                });
        }

        public Task<Either<ActionResult, FileInfo>> UploadChartFile(Guid releaseId, IFormFile file, Guid? id = null)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async () => await _fileUploadsValidatorService.ValidateUploadFileType(file, ReleaseFileTypes.Chart))
                .OnSuccess(async release =>
                {
                    var info = new Dictionary<string, string>
                    {
                        {NameKey, file.FileName.ToLower()}
                    };

                    return await _fileUploadsValidatorService.ValidateFileForUpload(releaseId, file, ReleaseFileTypes.Chart, true)
                            .OnSuccess(async () =>
                            {
                                var releaseFileReference = await CreateOrUpdateFileReference(file.FileName.ToLower(), releaseId, ReleaseFileTypes.Chart, id);
                                await _context.SaveChangesAsync();

                                await UploadChartFileToStorage(releaseId, file, info, releaseFileReference.Id);

                                var blob = await _blobStorageService.GetBlob(
                                    PrivateFilesContainerName,
                                    AdminReleasePath(releaseId,ReleaseFileTypes.Chart, releaseFileReference.Id.ToString())
                                );

                                return blob.ToFileInfo(releaseFileReference.Id);
                            });
                });
        }

        public async Task<Either<ActionResult, Unit>> DeleteChartFiles(
            Guid releaseId,
            IEnumerable<Guid> fileIds,
            bool forceDelete = false)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(async release => await CheckCanDeleteReleaseFile(release, forceDelete))
                .OnSuccessVoid(async () =>
                {
                    foreach (var id in fileIds)
                    {
                        if (await DeletionWillOrphanFileAsync(releaseId, id))
                        {
                            var fileReference = await _context.ReleaseFileReferences.SingleAsync(rfr => rfr.Id == id);

                            await _blobStorageService.DeleteBlob(
                                PrivateFilesContainerName,
                                AdminReleasePathWithFileReference(fileReference)
                            );

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
                });
        }

        public async Task<Either<ActionResult, Unit>> DeleteChartFile(Guid releaseId, Guid id, bool forceDelete = false)
        {
            return await DeleteChartFiles(releaseId, new List<Guid>() {id}, forceDelete: forceDelete);
        }

        public async Task<Either<ActionResult, Unit>> DeleteNonDataFile(
            Guid releaseId,
            ReleaseFileTypes type,
            string fileName,
            bool forceDelete = false)
        {
            if (type == ReleaseFileTypes.Data || type == ReleaseFileTypes.Metadata)
            {
                return ValidationActionResult(CannotUseGenericFunctionToDeleteDataFile);
            }

            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(async release => await CheckCanDeleteReleaseFile(release, forceDelete))
                .OnSuccessVoid(async () =>
                {
                    if (await DeletionWillOrphanFileAsync(releaseId, fileName, type))
                    {
                        await _blobStorageService.DeleteBlob(
                            PrivateFilesContainerName,
                            AdminReleasePath(releaseId, type, fileName)
                        );

                        await DeleteFileReference(releaseId, fileName, type);
                    }
                    else
                    {
                        await DeleteFileLink(releaseId, fileName, type);
                    }

                    await _context.SaveChangesAsync();
                });
        }

        public async Task<Either<ActionResult, IEnumerable<FileInfo>>> ListFiles(Guid releaseId,
            params ReleaseFileTypes[] types)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async _ =>
                {
                    var files = await GetReleaseFiles(releaseId, types);

                    var filesWithMetadata = files
                        .Select(async fileLink =>
                        {
                            var fileReference = fileLink.ReleaseFileReference;
                            var blobPath = AdminReleasePathWithFileReference(fileReference);

                            // Files should exists in storage but if not then allow user to delete
                            var exists = await _blobStorageService.CheckBlobExists(PrivateFilesContainerName, blobPath);

                            if (!exists)
                            {
                                return new FileInfo
                                {
                                    Id = fileReference.Id,
                                    Extension = fileReference.Extension,
                                    Name = "Unknown",
                                    Path = fileReference.Filename,
                                    Size = "0.00 B"
                                };
                            }

                            var blob = await _blobStorageService.GetBlob(PrivateFilesContainerName, blobPath);

                            return blob.ToFileInfo(fileReference.Id);
                        });

                    return (await Task.WhenAll(filesWithMetadata))
                        .OrderBy(file => file.Name)
                        .AsEnumerable();
                });
        }

        public async Task<Either<ActionResult, DataFileInfo>> GetDataFile(Guid releaseId, Guid fileReferenceId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async release => await _persistenceHelper.CheckEntityExists<ReleaseFileReference>(fileReferenceId))
                .OnSuccess(async fileReference => await GetDataFileInfo(releaseId, fileReference));
        }

        public async Task<Either<ActionResult, IEnumerable<DataFileInfo>>> ListDataFiles(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async _ =>
                {
                    var fileList = new List<DataFileInfo>();
                    var files = await GetReleaseFiles(releaseId, ReleaseFileTypes.Data);

                    // Exclude files that are replacements in progress
                    var filesExcludingReplacements = files.Where(file =>
                        !file.ReleaseFileReference.ReplacingId.HasValue);

                    await filesExcludingReplacements.ForEachAsync(async file =>
                        fileList.Add(await GetDataFileInfo(releaseId, file.ReleaseFileReference)));

                    return fileList
                        .OrderBy(file => file.Name)
                        .AsEnumerable();
                });
        }

        public async Task<Either<ActionResult, Unit>> DeleteAllFiles(Guid releaseId, bool forceDelete = false)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(async release => await CheckCanDeleteReleaseFile(release, forceDelete))
                .OnSuccessVoid(
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
                                    await DeleteChartFile(release.Id, file.Id, forceDelete: forceDelete);
                                    break;
                                case ReleaseFileTypes.Data:
                                    await DeleteDataFiles(release.Id, file.Id, forceDelete: forceDelete);
                                    break;
                                default:
                                    await DeleteNonDataFile(
                                        release.Id,
                                        file.ReleaseFileType,
                                        file.Filename,
                                        forceDelete: forceDelete
                                    );
                                    break;
                            }
                        }
                    }
                );
        }

        private async Task<Either<ActionResult, Release>> CheckCanDeleteReleaseFile(Release release, bool forceDelete)
        {
            if (forceDelete)
            {
                return await Task.FromResult(release);
            }

            return await _userService.CheckCanUpdateRelease(release);
        }

        public async Task<Either<ActionResult, FileStreamResult>> StreamFile(Guid releaseId, Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async _ =>
                {
                    var fileLink = await GetReleaseFileLink(releaseId, id);
                    return await GetStreamedFile(fileLink);
                });
        }

        private async Task<Either<ActionResult, ReleaseFileReference>> CheckReleaseFileReferenceExists(Guid id)
        {
            return await _persistenceHelper.CheckEntityExists<ReleaseFileReference>(id)
                .OnSuccess(releaseFileReference => releaseFileReference.ReleaseFileType != ReleaseFileTypes.Data
                    ? new Either<ActionResult, ReleaseFileReference>(
                        ValidationActionResult(FileTypeMustBeData))
                    : releaseFileReference);
        }

        private async Task<Either<ActionResult, FileStreamResult>> GetStreamedFile(ReleaseFile fileLink)
        {
            var path = AdminReleasePathWithFileReference(fileLink.ReleaseFileReference);
            var blob = await _blobStorageService.GetBlob(PrivateFilesContainerName, path);

            var stream = new MemoryStream();
            await _blobStorageService.DownloadToStream(PrivateFilesContainerName, path, stream);

            return new FileStreamResult(stream, blob.ContentType)
            {
                FileDownloadName = fileLink.ReleaseFileReference.Filename,
            };
        }

        private async Task<ReleaseFileReference> CreateOrUpdateFileReference(string filename,
            Guid releaseId,
            ReleaseFileTypes type,
            Guid? id = null,
            ReleaseFileReference replacingFile = null,
            ReleaseFileReference source = null)
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
                Replacing = replacingFile,
                Source = source
            };

            var entry = await _context.ReleaseFileReferences.AddAsync(releaseFileReference);

            if (replacingFile != null)
            {
                _context.Update(replacingFile);
                replacingFile.ReplacedBy = entry.Entity;
            }

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

        private async Task<DataFileInfo> GetDataFileInfo(Guid releaseId, ReleaseFileReference dataFileReference)
        {
            var blobPath = AdminReleasePathWithFileReference(dataFileReference);

            // Files should exists in storage but if not then allow user to delete
            var blobExists = await _blobStorageService.CheckBlobExists(PrivateFilesContainerName, blobPath);

            if (!blobExists)
            {
                return await GetFallbackDataFileInfo(releaseId, dataFileReference);
            }

            var blob = await _blobStorageService.GetBlob(PrivateFilesContainerName, blobPath);

            // If the file does exist then it could possibly be
            // partially uploaded so make sure meta data exists for it
            if (blob.GetUserName().IsNullOrEmpty())
            {
                return await GetFallbackDataFileInfo(releaseId, dataFileReference);
            }

            var metaFileReference =
                await GetAssociatedReleaseFileReference(dataFileReference, ReleaseFileTypes.Metadata);

            var importStatus = await _importStatusService.GetImportStatus(releaseId, blob.FileName);

            return new DataFileInfo
            {
                Id = dataFileReference.Id,
                Extension = blob.Extension,
                Name = blob.Name,
                Path = blob.Path,
                Size = blob.Size,
                MetaFileId = metaFileReference.Id,
                MetaFileName = blob.GetMetaFileName(),
                ReplacedBy = dataFileReference.ReplacedById,
                Rows = blob.GetNumberOfRows(),
                UserName = blob.GetUserName(),
                Status = importStatus.Status,
                Created = blob.Created
            };
        }

        private async Task<DataFileInfo> GetFallbackDataFileInfo(Guid releaseId, ReleaseFileReference fileReference)
        {
            // Try to get the name from the zip file if existing
            if (fileReference.SourceId != null)
            {
                var source = await GetReleaseFileReference(fileReference.SourceId.Value);
                var sourceBlobPath = AdminReleasePathWithFileReference(source);

                if (await _blobStorageService.CheckBlobExists(PrivateFilesContainerName, sourceBlobPath))
                {
                    var zipBlob = await _blobStorageService.GetBlob(PrivateFilesContainerName, sourceBlobPath);
                    var importStatus = await _importStatusService.GetImportStatus(releaseId, fileReference.Filename);

                    return new DataFileInfo
                    {
                        Id = fileReference.Id,
                        Extension = fileReference.Extension,
                        Name = zipBlob.Name,
                        Path = fileReference.Filename,
                        Size = zipBlob.Size,
                        MetaFileId = null,
                        MetaFileName =
                            fileReference.ReleaseFileType == ReleaseFileTypes.Data
                                ? zipBlob.GetMetaFileName()
                                : string.Empty,
                        Rows = 0,
                        UserName = zipBlob.GetUserName(),
                        Status = importStatus.Status,
                        Created = zipBlob.Created
                    };
                }
            }

            var dataFileName = fileReference.ReleaseFileType == ReleaseFileTypes.Data
                ? fileReference.Filename
                : (await GetAssociatedReleaseFileReference(fileReference, ReleaseFileTypes.Data)).Filename;

            // Fail the import if this was a datafile upload
            await _importService.FailImport(
                releaseId,
                dataFileName,
                new List<ValidationError>
                {
                    new ValidationError("Files not uploaded correctly. Please delete and retry")
                }.AsEnumerable()
            );

            var metaFileReference = fileReference.ReleaseFileType == ReleaseFileTypes.Data
                ? await GetAssociatedReleaseFileReference(fileReference, ReleaseFileTypes.Metadata)
                : null;

            return new DataFileInfo
            {
                Id = fileReference.Id,
                Name = await GetSubjectName(fileReference),
                Extension = fileReference.Extension,
                Path = fileReference.Filename,
                Size = "0.00 B",
                MetaFileId = metaFileReference?.Id,
                MetaFileName = metaFileReference?.Filename ?? "",
                Rows = 0,
                UserName = "",
                Status = IStatus.NOT_FOUND
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

        private async Task DeleteFileLink(Guid releaseId, Guid releaseFileReferenceId)
        {
            var fileLink = await GetReleaseFileLink(releaseId, releaseFileReferenceId);
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

        private async Task<ReleaseFile> GetReleaseFileLink(Guid releaseId, Guid releaseFileReferenceId)
        {
            return await _context
                .ReleaseFiles
                .Include(f => f.ReleaseFileReference)
                .Where(f => f.ReleaseId == releaseId && f.ReleaseFileReferenceId == releaseFileReferenceId)
                .SingleAsync();
        }

        private async Task DeleteFileReference(Guid releaseId, string filename,
            ReleaseFileTypes type)
        {
            var fileLink = await GetReleaseFileLinkAsync(releaseId, filename, type);
            _context.ReleaseFileReferences.Remove(fileLink.ReleaseFileReference);
        }

        private async Task<Either<ActionResult, string>> ValidateSubjectName(Guid releaseId,
            string subjectName, ReleaseFileReference replacingFile)
        {
            if (replacingFile == null)
            {
                return await _fileUploadsValidatorService.ValidateSubjectName(releaseId, subjectName)
                    .OnSuccess(async () => subjectName);
            }
            return await GetSubjectName(replacingFile);
        }

        private async Task UploadFileToStorage(
            Guid releaseId,
            IFormFile file,
            ReleaseFileTypes type,
            IDictionary<string, string> metaValues)
        {
            await _blobStorageService.UploadFile(
                containerName: PrivateFilesContainerName,
                path: AdminReleasePath(releaseId, type, file.FileName.ToLower()),
                file: file,
                options: new IBlobStorageService.UploadFileOptions
                {
                    MetaValues = metaValues
                }
            );
        }

        private async Task UploadChartFileToStorage(
            Guid releaseId,
            IFormFile file,
            IDictionary<string, string> metaValues,
            Guid id)
        {
            await _blobStorageService.UploadFile(
                containerName: PrivateFilesContainerName,
                path: AdminReleasePath(releaseId, ReleaseFileTypes.Chart, id.ToString()),
                file: file,
                options: new IBlobStorageService.UploadFileOptions
                {
                    MetaValues = metaValues
                }
            );
        }

        private async Task<bool> DeletionWillOrphanFileAsync(Guid releaseId, string filename,
            ReleaseFileTypes type)
        {
            var fileLink = await GetReleaseFileLinkAsync(releaseId, filename, type);

            return !await _context.ReleaseFiles.AnyAsync(f => 
                f.ReleaseFileReferenceId == fileLink.ReleaseFileReferenceId && f.Id != fileLink.Id);
        }

        private async Task<bool> DeletionWillOrphanFileAsync(Guid releaseId, Guid id)
        {
            var fileLink = await GetReleaseFileLink(releaseId, id);

            return !await _context.ReleaseFiles.AnyAsync(f =>
                f.ReleaseFileReferenceId == fileLink.ReleaseFileReferenceId && f.Id != fileLink.Id);
        }

        private string AdminReleasePathWithFileReference(ReleaseFileReference fileReference)
        {
            return AdminReleasePath(
                fileReference.ReleaseId,
                fileReference.ReleaseFileType,
                fileReference.BlobStorageName);
        }

        private async Task<ReleaseFileReference> GetAssociatedReleaseFileReference(ReleaseFileReference releaseFileReference, ReleaseFileTypes associatedType)
        {
            return await _context.ReleaseFileReferences
                .FirstAsync(rfr => rfr.ReleaseId == releaseFileReference.ReleaseId
                                   && rfr.ReleaseFileType == associatedType
                                   && rfr.SubjectId == releaseFileReference.SubjectId);
        }

        private async Task<string> GetSubjectName(ReleaseFileReference releaseFileReference)
        {
            if (releaseFileReference.SubjectId.HasValue)
            {
                var subject = await _subjectService.GetAsync(releaseFileReference.SubjectId.Value);
                return subject.Name;
            }

            return "Unknown";
        }
    }
}