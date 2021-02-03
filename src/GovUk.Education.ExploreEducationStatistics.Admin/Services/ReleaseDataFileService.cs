using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStorageUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseDataFileService : IReleaseDataFileService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IDataArchiveValidationService _dataArchiveValidationService;
        private readonly IFileUploadsValidatorService _fileUploadsValidatorService;
        private readonly IFileRepository _fileRepository;
        private readonly IReleaseRepository _releaseRepository;
        private readonly IReleaseFileRepository _releaseFileRepository;
        private readonly IImportService _importService;
        private readonly IUserService _userService;

        public ReleaseDataFileService(ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IBlobStorageService blobStorageService,
            IDataArchiveValidationService dataArchiveValidationService,
            IFileUploadsValidatorService fileUploadsValidatorService,
            IFileRepository fileRepository,
            IReleaseRepository releaseRepository,
            IReleaseFileRepository releaseFileRepository,
            IImportService importService,
            IUserService userService)
        {
            _contentDbContext = contentDbContext;
            _statisticsDbContext = statisticsDbContext;
            _persistenceHelper = persistenceHelper;
            _blobStorageService = blobStorageService;
            _dataArchiveValidationService = dataArchiveValidationService;
            _fileUploadsValidatorService = fileUploadsValidatorService;
            _fileRepository = fileRepository;
            _releaseRepository = releaseRepository;
            _releaseFileRepository = releaseFileRepository;
            _importService = importService;
            _userService = userService;
        }

        public async Task<Either<ActionResult, Unit>> Delete(Guid releaseId,
            Guid id,
            bool forceDelete = false)
        {
            return await Delete(releaseId, new List<Guid>
            {
                id
            }, forceDelete: forceDelete);
        }

        public async Task<Either<ActionResult, Unit>> Delete(Guid releaseId,
            IEnumerable<Guid> ids,
            bool forceDelete = false)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(async release => await _userService.CheckCanUpdateRelease(release, ignoreCheck: forceDelete))
                .OnSuccess(async release =>
                    await ids.Select(id => _releaseFileRepository.CheckFileExists(releaseId, id, FileType.Data))
                        .OnSuccessAll())
                .OnSuccessVoid(async files =>
                {
                    foreach (var file in files)
                    {
                        var metaFile =
                            await GetAssociatedReleaseFileReference(file, Metadata);

                        if (await _releaseFileRepository.FileIsLinkedToOtherReleases(releaseId, file.Id))
                        {
                            await _releaseFileRepository.Delete(releaseId, file.Id);
                            await _releaseFileRepository.Delete(releaseId, metaFile.Id);
                        }
                        else
                        {
                            await _importService.DeleteImport(file.Id);
                            await _blobStorageService.DeleteBlob(
                                PrivateFilesContainerName,
                                file.Path()
                            );
                            await _blobStorageService.DeleteBlob(
                                PrivateFilesContainerName,
                                metaFile.Path()
                            );

                            // If this is a replacement then unlink it from the original
                            if (file.ReplacingId.HasValue)
                            {
                                var originalFile = await _fileRepository.Get(file.ReplacingId.Value);
                                originalFile.ReplacedById = null;
                                _contentDbContext.Update(originalFile);
                                await _contentDbContext.SaveChangesAsync();
                            }

                            await _releaseFileRepository.Delete(releaseId, file.Id);
                            await _releaseFileRepository.Delete(releaseId, metaFile.Id);

                            await _fileRepository.Delete(file.Id);
                            await _fileRepository.Delete(metaFile.Id);

                            if (file.SourceId.HasValue)
                            {
                                var zipFile = await _fileRepository.Get(file.SourceId.Value);
                                await _blobStorageService.DeleteBlob(
                                    PrivateFilesContainerName,
                                    zipFile.Path()
                                );
                                // N.B. No ReleaseFiles row for source links
                                await _fileRepository.Delete(zipFile.Id);
                            }
                        }

                        await DeleteBatchFiles(releaseId, file);
                    }
                });
        }

        public async Task<Either<ActionResult, Unit>> DeleteAll(Guid releaseId, bool forceDelete = false)
        {
            var releaseFiles = await _releaseFileRepository.GetByFileType(releaseId, FileType.Data);

            return await Delete(releaseId,
                releaseFiles.Select(releaseFile => releaseFile.File.Id),
                forceDelete: forceDelete);
        }

        public async Task<Either<ActionResult, DataFileInfo>> GetInfo(Guid releaseId, Guid id)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(
                    async release => await _persistenceHelper
                        .CheckEntityExists<ReleaseFile>(
                            q => q.Include(rf => rf.File)
                                .Where(
                                    rf => rf.ReleaseId == release.Id
                                          && rf.File.Type == FileType.Data
                                          && rf.FileId == id
                                )
                        )
                )
                .OnSuccess(async file => await GetDataFileInfo(releaseId, file.File));
        }

        public async Task<Either<ActionResult, IEnumerable<DataFileInfo>>> ListAll(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async _ =>
                {
                    var fileList = new List<DataFileInfo>();
                    var files = await _releaseFileRepository.GetByFileType(releaseId, FileType.Data);

                    // Exclude files that are replacements in progress
                    var filesExcludingReplacements = files.Where(file =>
                        !file.File.ReplacingId.HasValue);

                    await filesExcludingReplacements.ForEachAsync(async file =>
                        fileList.Add(await GetDataFileInfo(releaseId, file.File)));

                    return fileList
                        .OrderBy(file => file.Name)
                        .AsEnumerable();
                });
        }

        public async Task<Either<ActionResult, DataFileInfo>> Upload(Guid releaseId,
            IFormFile dataFormFile,
            IFormFile metaFormFile,
            string userName,
            Guid? replacingFileId = null,
            string subjectName = null)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async release =>
                {
                    return await _persistenceHelper.CheckOptionalEntityExists<File>(replacingFileId)
                        .OnSuccess(async replacingFile =>
                        {
                            return await ValidateSubjectName(releaseId, subjectName, replacingFile)
                                .OnSuccess(validSubjectName => _fileUploadsValidatorService
                                    .ValidateDataFilesForUpload(releaseId, dataFormFile, metaFormFile)
                                    // First, create with status uploading to prevent other users uploading the same datafile
                                    .OnSuccess(async () =>
                                    {
                                        var subjectId = await _releaseRepository.CreateReleaseAndSubjectHierarchy(
                                            releaseId,
                                            dataFormFile.FileName.ToLower(),
                                            validSubjectName);

                                        var dataFile = await _fileRepository.CreateDataOrMetadata(
                                            releaseId: releaseId,
                                            subjectId: subjectId,
                                            filename: dataFormFile.FileName.ToLower(),
                                            type: FileType.Data,
                                            replacingFile: replacingFile);

                                        var metaFile = await _fileRepository.CreateDataOrMetadata(
                                            releaseId: releaseId,
                                            subjectId: subjectId,
                                            filename: metaFormFile.FileName.ToLower(),
                                            type: Metadata);

                                        var dataInfo = GetDataFileMetaValues(
                                            name: validSubjectName,
                                            metaFileName: metaFile.Filename,
                                            userName: userName,
                                            numberOfRows: CalculateNumberOfRows(dataFormFile.OpenReadStream())
                                        );

                                        var metaDataInfo = GetMetaDataFileMetaValues(
                                            dataFileName: dataFile.Filename
                                        );

                                        await UploadFileToStorage(dataFile, dataFormFile, dataInfo);
                                        await UploadFileToStorage(metaFile, metaFormFile, metaDataInfo);

                                        await _importService.Import(
                                            subjectId: subjectId,
                                            dataFile: dataFile,
                                            metaFile: metaFile,
                                            formFile: dataFormFile);

                                        var blob = await _blobStorageService.GetBlob(
                                            PrivateFilesContainerName,
                                            dataFile.Path()
                                        );

                                        return new DataFileInfo
                                        {
                                            Id = dataFile.Id,
                                            FileName = dataFile.Filename,
                                            Name = validSubjectName,
                                            Path = blob.Path,
                                            Size = blob.Size,
                                            MetaFileId = metaFile.Id,
                                            MetaFileName = blob.GetMetaFileName(),
                                            Rows = blob.GetNumberOfRows(),
                                            UserName = blob.GetUserName(),
                                            Status = ImportStatus.QUEUED,
                                            Created = blob.Created,
                                            Permissions = await _userService.GetDataFilePermissions(dataFile)
                                        };
                                    }));
                        });
                });
        }

        public async Task<Either<ActionResult, DataFileInfo>> UploadAsZip(Guid releaseId,
            IFormFile zipFormFile,
            string userName,
            Guid? replacingFileId = null,
            string subjectName = null)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async release =>
                {
                    return await _persistenceHelper.CheckOptionalEntityExists<File>(replacingFileId)
                        .OnSuccess(async replacingFile =>
                        {
                            return await ValidateSubjectName(releaseId, subjectName, replacingFile)
                                .OnSuccess(validSubjectName =>
                                    _dataArchiveValidationService.ValidateDataArchiveFile(releaseId, zipFormFile)
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
                                                {
                                                    var subjectId = await _releaseRepository.CreateReleaseAndSubjectHierarchy(
                                                        releaseId,
                                                        archiveFile.DataFileName.ToLower(),
                                                        validSubjectName);

                                                    var zipFile = await _fileRepository.CreateZip(
                                                        filename: zipFormFile.FileName.ToLower(),
                                                        releaseId: releaseId);

                                                    var dataFile = await _fileRepository.CreateDataOrMetadata(
                                                        releaseId: releaseId,
                                                        subjectId: subjectId,
                                                        filename: archiveFile.DataFileName,
                                                        type: FileType.Data,
                                                        replacingFile: replacingFile,
                                                        source: zipFile);

                                                    var metaFile = await _fileRepository.CreateDataOrMetadata(
                                                        releaseId: releaseId,
                                                        subjectId: subjectId,
                                                        filename: archiveFile.MetaFileName,
                                                        type: Metadata,
                                                        source: zipFile);

                                                    await UploadFileToStorage(zipFile, zipFormFile, dataInfo);

                                                    await _importService.ImportZip(
                                                        subjectId: subjectId,
                                                        dataFile: dataFile,
                                                        metaFile: metaFile,
                                                        zipFile: zipFile);

                                                    var blob = await _blobStorageService.GetBlob(
                                                        PrivateFilesContainerName,
                                                        zipFile.Path()
                                                    );

                                                    return new DataFileInfo
                                                    {
                                                        // TODO size and rows are for zip file but they need to be for
                                                        // the datafile which isn't extracted yet
                                                        Id = dataFile.Id,
                                                        FileName = dataFile.Filename,
                                                        Name = validSubjectName,
                                                        Path = dataFile.Filename,
                                                        Size = blob.Size,
                                                        MetaFileId = metaFile.Id,
                                                        MetaFileName = metaFile.Filename,
                                                        Rows = blob.GetNumberOfRows(),
                                                        UserName = blob.GetUserName(),
                                                        Status = ImportStatus.QUEUED,
                                                        Created = blob.Created,
                                                        Permissions = await _userService.GetDataFilePermissions(dataFile)
                                                    };
                                                });
                                        }));
                        });
                });
        }

        private async Task<DataFileInfo> GetDataFileInfo(Guid releaseId, File dataFile)
        {
            // Files should exists in storage but if not then allow user to delete
            var blobExists =
                await _blobStorageService.CheckBlobExists(PrivateFilesContainerName, dataFile.Path());

            if (!blobExists)
            {
                return await GetFallbackDataFileInfo(dataFile);
            }

            var blob = await _blobStorageService.GetBlob(PrivateFilesContainerName, dataFile.Path());

            // If the file does exist then it could possibly be
            // partially uploaded so make sure meta data exists for it
            if (string.IsNullOrEmpty(blob.GetUserName()))
            {
                return await GetFallbackDataFileInfo(dataFile);
            }

            var metaFile = await GetAssociatedReleaseFileReference(dataFile, Metadata);

            return new DataFileInfo
            {
                Id = dataFile.Id,
                FileName = dataFile.Filename,
                Name = dataFile.SubjectId.HasValue ? await GetSubjectName(dataFile) : blob.Name,
                Path = blob.Path,
                Size = blob.Size,
                MetaFileId = metaFile.Id,
                MetaFileName = blob.GetMetaFileName(),
                ReplacedBy = dataFile.ReplacedById,
                Rows = blob.GetNumberOfRows(),
                UserName = blob.GetUserName(),
                Status = await _importService.GetStatus(dataFile.Id),
                Created = blob.Created,
                Permissions = await _userService.GetDataFilePermissions(dataFile)
            };
        }

        private async Task<DataFileInfo> GetFallbackDataFileInfo(File file)
        {
            // Try to get the name from the zip file if existing
            if (file.SourceId != null)
            {
                var source = await _fileRepository.Get(file.SourceId.Value);

                if (await _blobStorageService.CheckBlobExists(PrivateFilesContainerName, source.Path()))
                {
                    var zipBlob = await _blobStorageService.GetBlob(PrivateFilesContainerName, source.Path());

                    return new DataFileInfo
                    {
                        Id = file.Id,
                        FileName = file.Filename,
                        Name = zipBlob.Name,
                        Path = file.Filename,
                        Size = zipBlob.Size,
                        MetaFileId = null,
                        MetaFileName =
                            file.Type == FileType.Data
                                ? zipBlob.GetMetaFileName()
                                : string.Empty,
                        Rows = 0,
                        UserName = zipBlob.GetUserName(),
                        Status = await _importService.GetStatus(file.Id),
                        Created = zipBlob.Created,
                        Permissions = await _userService.GetDataFilePermissions(file)
                    };
                }
            }

            var metaFileReference = file.Type == FileType.Data
                ? await GetAssociatedReleaseFileReference(file, Metadata)
                : null;

            return new DataFileInfo
            {
                Id = file.Id,
                FileName = file.Filename,
                Name = await GetSubjectName(file),
                Path = file.Filename,
                Size = "0.00 B",
                MetaFileId = metaFileReference?.Id,
                MetaFileName = metaFileReference?.Filename ?? "",
                Rows = 0,
                UserName = "",
                Status = await _importService.GetStatus(file.Id),
                Permissions = await _userService.GetDataFilePermissions(file)
            };
        }

        private async Task<string> GetSubjectName(File file)
        {
            if (file.SubjectId.HasValue)
            {
                var subject = await _statisticsDbContext.Subject.FindAsync(file.SubjectId.Value);
                return subject.Name;
            }

            return "Unknown";
        }

        private async Task<Either<ActionResult, string>> ValidateSubjectName(Guid releaseId,
            string subjectName, File replacingFile)
        {
            if (replacingFile == null)
            {
                return await _fileUploadsValidatorService.ValidateSubjectName(releaseId, subjectName)
                    .OnSuccess(async () => await Task.FromResult(subjectName));
            }

            return await GetSubjectName(replacingFile);
        }

        private async Task UploadFileToStorage(
            File file,
            IFormFile formFile,
            IDictionary<string, string> metaValues)
        {
            await _blobStorageService.UploadFile(
                containerName: PrivateFilesContainerName,
                path: file.Path(),
                file: formFile,
                options: new IBlobStorageService.UploadFileOptions
                {
                    MetaValues = metaValues
                }
            );
        }

        private async Task<File> GetAssociatedReleaseFileReference(
            File file,
            FileType associatedType)
        {
            return await _contentDbContext.Files
                .FirstAsync(f => f.ReleaseId == file.ReleaseId
                                   && f.Type == associatedType
                                   && f.SubjectId == file.SubjectId);
        }

        private async Task<IEnumerable<BlobInfo>> GetBatchFilesForDataFile(Guid releaseId, string originalDataFileName)
        {
            var blobs = await _blobStorageService.ListBlobs(
                PrivateFilesContainerName,
                AdminReleaseBatchesDirectoryPath(releaseId)
            );

            return blobs.Where(blob => IsBatchFileForDataFile(releaseId, originalDataFileName, blob.Path));
        }

        private async Task DeleteBatchFiles(Guid releaseId, File dataFile)
        {
            var batchFiles = await GetBatchFilesForDataFile(releaseId, dataFile.Filename);
            await batchFiles.ForEachAsync(async batchFile =>
            {
                await _blobStorageService.DeleteBlob(
                    PrivateFilesContainerName,
                    AdminReleaseBatchesDirectoryPath(releaseId) + batchFile.FileName
                );
            });
        }
    }
}
