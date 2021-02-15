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
        private readonly IDataImportService _dataImportService;
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
            IDataImportService dataImportService,
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
            _dataImportService = dataImportService;
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
                        var metaFile = await GetAssociatedMetaFile(releaseId, file);

                        if (await _releaseFileRepository.FileIsLinkedToOtherReleases(releaseId, file.Id))
                        {
                            await _releaseFileRepository.Delete(releaseId, file.Id);
                            await _releaseFileRepository.Delete(releaseId, metaFile.Id);
                        }
                        else
                        {
                            await _dataImportService.DeleteImport(file.Id);
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

                        await DeleteBatchFiles(file);
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

                                        await UploadFileToStorage(dataFile, dataFormFile, dataInfo);
                                        await UploadFileToStorage(metaFile, metaFormFile);

                                        await _dataImportService.Import(
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
                                            Status = DataImportStatus.QUEUED,
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

                                                    await _dataImportService.ImportZip(
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
                                                        Status = DataImportStatus.QUEUED,
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
                return await GetFallbackDataFileInfo(releaseId, dataFile);
            }

            var blob = await _blobStorageService.GetBlob(PrivateFilesContainerName, dataFile.Path());

            // If the file does exist then it could possibly be
            // partially uploaded so make sure meta data exists for it
            if (string.IsNullOrEmpty(blob.GetUserName()))
            {
                return await GetFallbackDataFileInfo(releaseId, dataFile);
            }

            var metaFile = await GetAssociatedMetaFile(releaseId, dataFile);

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
                Status = await _dataImportService.GetStatus(dataFile.Id),
                Created = blob.Created,
                Permissions = await _userService.GetDataFilePermissions(dataFile)
            };
        }

        private async Task<DataFileInfo> GetFallbackDataFileInfo(Guid releaseId, File dataFile)
        {
            // Try to get the name from the zip file if existing
            if (dataFile.SourceId != null)
            {
                var source = await _fileRepository.Get(dataFile.SourceId.Value);

                if (await _blobStorageService.CheckBlobExists(PrivateFilesContainerName, source.Path()))
                {
                    var zipBlob = await _blobStorageService.GetBlob(PrivateFilesContainerName, source.Path());

                    return new DataFileInfo
                    {
                        Id = dataFile.Id,
                        FileName = dataFile.Filename,
                        Name = zipBlob.Name,
                        Path = dataFile.Filename,
                        Size = zipBlob.Size,
                        MetaFileId = null,
                        MetaFileName = zipBlob.GetMetaFileName(),
                        Rows = 0,
                        UserName = zipBlob.GetUserName(),
                        Status = await _dataImportService.GetStatus(dataFile.Id),
                        Created = zipBlob.Created,
                        Permissions = await _userService.GetDataFilePermissions(dataFile)
                    };
                }
            }

            var metaFile = await GetAssociatedMetaFile(releaseId, dataFile);

            return new DataFileInfo
            {
                Id = dataFile.Id,
                FileName = dataFile.Filename,
                Name = await GetSubjectName(dataFile),
                Path = dataFile.Filename,
                Size = "0.00 B",
                MetaFileId = metaFile.Id,
                MetaFileName = metaFile.Filename ?? "",
                Rows = 0,
                UserName = "",
                Status = await _dataImportService.GetStatus(dataFile.Id),
                Permissions = await _userService.GetDataFilePermissions(dataFile)
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
            IDictionary<string, string> metadata = null)
        {
            await _blobStorageService.UploadFile(
                containerName: PrivateFilesContainerName,
                path: file.Path(),
                file: formFile,
                metadata: metadata
            );
        }

        private async Task<File> GetAssociatedMetaFile(Guid releaseId, File dataFile)
        {
            return await _contentDbContext.ReleaseFiles
                .Include(rf => rf.File)
                .Where(rf => rf.ReleaseId == releaseId
                             && rf.File.Type == Metadata
                             && rf.File.SubjectId == dataFile.SubjectId)
                .Select(rf => rf.File)
                .SingleAsync();
        }

        private async Task DeleteBatchFiles(File dataFile)
        {
            await _blobStorageService.DeleteBlobs(PrivateFilesContainerName,
                AdminDataFileBatchesDirectoryPath(dataFile.RootPath, dataFile.Id));
        }
    }
}
