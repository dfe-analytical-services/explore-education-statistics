#nullable enable
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
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseDataFileService : IReleaseDataFileService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IDataArchiveValidationService _dataArchiveValidationService;
        private readonly IFileUploadsValidatorService _fileUploadsValidatorService;
        private readonly IFileRepository _fileRepository;
        private readonly IReleaseRepository _releaseRepository;
        private readonly IReleaseFileRepository _releaseFileRepository;
        private readonly IReleaseDataFileRepository _releaseDataFileRepository;
        private readonly IDataImportService _dataImportService;
        private readonly IUserService _userService;

        public ReleaseDataFileService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IBlobStorageService blobStorageService,
            IDataArchiveValidationService dataArchiveValidationService,
            IFileUploadsValidatorService fileUploadsValidatorService,
            IFileRepository fileRepository,
            IReleaseRepository releaseRepository,
            IReleaseFileRepository releaseFileRepository,
            IReleaseDataFileRepository releaseDataFileRepository,
            IDataImportService dataImportService,
            IUserService userService)
        {
            _contentDbContext = contentDbContext;
            _persistenceHelper = persistenceHelper;
            _blobStorageService = blobStorageService;
            _dataArchiveValidationService = dataArchiveValidationService;
            _fileUploadsValidatorService = fileUploadsValidatorService;
            _fileRepository = fileRepository;
            _releaseRepository = releaseRepository;
            _releaseFileRepository = releaseFileRepository;
            _releaseDataFileRepository = releaseDataFileRepository;
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
                                PrivateReleaseFiles,
                                file.Path()
                            );
                            await _blobStorageService.DeleteBlob(
                                PrivateReleaseFiles,
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
                                    PrivateReleaseFiles,
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

        public async Task<Either<ActionResult, List<DataFileInfo>>> ListAll(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async () =>
                {
                    var files = await _releaseFileRepository.GetByFileType(releaseId, FileType.Data);

                    // Exclude files that are replacements in progress
                    var filesExcludingReplacements = files.Where(file => !file.File.ReplacingId.HasValue);

                    return await filesExcludingReplacements
                        .ToAsyncEnumerable()
                        .SelectAwait(async file => await GetDataFileInfo(releaseId, file.File))
                        .OrderBy(file => file.Name.IsNullOrWhitespace())
                        .ThenBy(file => file.Name)
                        .ToListAsync();
                });
        }

        public async Task<Either<ActionResult, DataFileInfo>> Upload(Guid releaseId,
            IFormFile dataFormFile,
            IFormFile metaFormFile,
            Guid? replacingFileId = null,
            string? subjectName = null)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async release =>
                {
                    return await _persistenceHelper
                        .CheckOptionalEntityExists<File>(replacingFileId)
                        .OnSuccessDo(replacingFile =>
                            _fileUploadsValidatorService.ValidateDataFilesForUpload(releaseId, dataFormFile,
                                metaFormFile, replacingFile))
                        // First, create with status uploading to prevent other users uploading the same datafile
                        .OnSuccessCombineWith(replacingFile =>
                            ValidateSubjectName(releaseId, subjectName, replacingFile))
                        .OnSuccess(async replacingFileAndSubjectName =>
                        {
                            var (replacingFile, validSubjectName) = replacingFileAndSubjectName;

                            var subjectId =
                                await _releaseRepository
                                    .CreateStatisticsDbReleaseAndSubjectHierarchy(releaseId);

                            var dataFile = await _releaseDataFileRepository.Create(
                                releaseId: releaseId,
                                subjectId: subjectId,
                                filename: dataFormFile.FileName.ToLower(),
                                contentType: dataFormFile.ContentType,
                                size: dataFormFile.Length,
                                type: FileType.Data,
                                createdById: _userService.GetUserId(),
                                name: validSubjectName,
                                replacingFile: replacingFile);

                            var metaFile = await _releaseDataFileRepository.Create(
                                releaseId: releaseId,
                                subjectId: subjectId,
                                filename: metaFormFile.FileName.ToLower(),
                                contentType: metaFormFile.ContentType,
                                size: metaFormFile.Length,
                                type: Metadata,
                                createdById: _userService.GetUserId());

                            await UploadFileToStorage(dataFile, dataFormFile);
                            await UploadFileToStorage(metaFile, metaFormFile);

                            var dataImport = await _dataImportService.Import(
                                subjectId: subjectId,
                                dataFile: dataFile,
                                metaFile: metaFile,
                                formFile: dataFormFile);

                            await _contentDbContext.Entry(dataFile)
                                .Reference(f => f.CreatedBy)
                                .LoadAsync();

                            return new DataFileInfo
                            {
                                Id = dataFile.Id,
                                FileName = dataFile.Filename,
                                Name = validSubjectName,
                                Size = dataFile.DisplaySize(),
                                MetaFileId = metaFile.Id,
                                MetaFileName = metaFile.Filename,
                                Rows = dataImport.Rows,
                                UserName = dataFile.CreatedBy.Email,
                                Status = dataImport.Status,
                                Created = dataFile.Created,
                                Permissions = await _userService.GetDataFilePermissions(dataFile)
                            };
                        });
                });
        }

        public async Task<Either<ActionResult, DataFileInfo>> UploadAsZip(Guid releaseId,
            IFormFile zipFormFile,
            Guid? replacingFileId = null,
            string? subjectName = null)
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
                                    _dataArchiveValidationService.ValidateDataArchiveFile(zipFormFile)
                                        .OnSuccess(async archiveFile =>
                                        {
                                            return await _fileUploadsValidatorService
                                                .ValidateDataArchiveEntriesForUpload(releaseId, archiveFile)
                                                .OnSuccess(async () =>
                                                {
                                                    var subjectId = await _releaseRepository
                                                        .CreateStatisticsDbReleaseAndSubjectHierarchy(releaseId);

                                                    var zipFile = await _releaseDataFileRepository.CreateZip(
                                                        filename: zipFormFile.FileName.ToLower(),
                                                        contentType: zipFormFile.ContentType,
                                                        size: zipFormFile.Length,
                                                        releaseId: releaseId,
                                                        createdById: _userService.GetUserId());

                                                    var dataFile = await _releaseDataFileRepository.Create(
                                                        releaseId: releaseId,
                                                        subjectId: subjectId,
                                                        filename: archiveFile.DataFileName,
                                                        contentType: null,
                                                        size: archiveFile.DataFileSize,
                                                        type: FileType.Data,
                                                        createdById: _userService.GetUserId(),
                                                        name: validSubjectName,
                                                        replacingFile: replacingFile,
                                                        source: zipFile);

                                                    var metaFile = await _releaseDataFileRepository.Create(
                                                        releaseId: releaseId,
                                                        subjectId: subjectId,
                                                        filename: archiveFile.MetaFileName,
                                                        contentType: null,
                                                        size: archiveFile.MetaFileSize,
                                                        type: Metadata,
                                                        createdById: _userService.GetUserId(),
                                                        source: zipFile);

                                                    await UploadFileToStorage(zipFile, zipFormFile);

                                                    await _dataImportService.ImportZip(
                                                        subjectId: subjectId,
                                                        dataFile: dataFile,
                                                        metaFile: metaFile,
                                                        zipFile: zipFile);

                                                    await _contentDbContext.Entry(dataFile)
                                                        .Reference(f => f.CreatedBy)
                                                        .LoadAsync();

                                                    return new DataFileInfo
                                                    {
                                                        Id = dataFile.Id,
                                                        FileName = dataFile.Filename,
                                                        Name = validSubjectName,
                                                        Size = dataFile.DisplaySize(),
                                                        MetaFileId = metaFile.Id,
                                                        MetaFileName = metaFile.Filename,
                                                        Rows = null,
                                                        UserName = dataFile.CreatedBy.Email,
                                                        Status = DataImportStatus.QUEUED,
                                                        Created = dataFile.Created,
                                                        Permissions =
                                                            await _userService.GetDataFilePermissions(dataFile)
                                                    };
                                                });
                                        }));
                        });
                });
        }

        private async Task<DataFileInfo> GetDataFileInfo(Guid releaseId, File dataFile)
        {
            var metaFile = await GetAssociatedMetaFile(releaseId, dataFile);

            await _contentDbContext.Entry(dataFile)
                .Reference(f => f.CreatedBy)
                .LoadAsync();

            var dataImport = await _dataImportService.GetImport(dataFile.Id);

            return new DataFileInfo
            {
                Id = dataFile.Id,
                FileName = dataFile.Filename,
                Name = await GetSubjectName(releaseId, dataFile),
                Size = dataFile.DisplaySize(),
                MetaFileId = metaFile.Id,
                MetaFileName = metaFile.Filename,
                ReplacedBy = dataFile.ReplacedById,
                Rows = dataImport?.Rows ?? 0,
                UserName = dataFile.CreatedBy?.Email ?? "",
                Status = dataImport?.Status ?? DataImportStatus.NOT_FOUND,
                Created = dataFile.Created,
                Permissions = await _userService.GetDataFilePermissions(dataFile)
            };
        }

        private async Task<string> GetSubjectName(Guid releaseId, File file)
        {
            if (file.Type != FileType.Data)
            {
                throw new ArgumentException("file.Type should equal FileType.Data");
            }

            return (await _releaseFileRepository.Find(releaseId, file.Id))?.Name ?? "Unknown";
        }

        private async Task<Either<ActionResult, string>> ValidateSubjectName(Guid releaseId,
            string subjectName,
            File? replacingFile)
        {
            if (replacingFile == null)
            {
                return await _fileUploadsValidatorService.ValidateSubjectName(releaseId, subjectName)
                    .OnSuccess(async () => await Task.FromResult(subjectName));
            }

            return await GetSubjectName(releaseId, replacingFile);
        }

        private async Task UploadFileToStorage(
            File file,
            IFormFile formFile)
        {
            await _blobStorageService.UploadFile(
                containerName: PrivateReleaseFiles,
                path: file.Path(),
                file: formFile
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
            await _blobStorageService.DeleteBlobs(PrivateReleaseFiles, dataFile.BatchesPath());
        }
    }
}
