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
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using IReleaseRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseRepository;
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
        private readonly IReleaseFileService _releaseFileService;
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
            IReleaseFileService releaseFileService,
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
            _releaseFileService = releaseFileService;
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
                .OnSuccess(async _ =>
                    await ids.Select(id => _releaseFileService.CheckFileExists(releaseId, id, FileType.Data))
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

        public async Task<Either<ActionResult, DataFileInfo>> GetInfo(Guid releaseId, Guid fileId)
        {
            return await _persistenceHelper
                .CheckEntityExists<ReleaseFile>(q => q.Where(
                        rf => rf.ReleaseId == releaseId
                              && rf.File.Type == FileType.Data
                              && rf.FileId == fileId)
                    .Include(rf => rf.Release))
                .OnSuccessDo(rf => _userService.CheckCanViewRelease(rf.Release))
                .OnSuccess(BuildDataFileViewModel);
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
                    var filesExcludingReplacements = files
                        .Where(releaseFile => !releaseFile.File.ReplacingId.HasValue)
                        .OrderBy(releaseFile => releaseFile.Order)
                        .ThenBy(releaseFile => releaseFile.Name) // For subjects existing before ordering was added
                        .ToList();

                    return await BuildDataFileViewModels(filesExcludingReplacements);
                });
        }

        public async Task<Either<ActionResult, List<DataFileInfo>>> ReorderDataFiles(
            Guid releaseId,
            List<Guid> fileIds)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(release => _userService.CheckCanUpdateRelease(release))
                .OnSuccess(async _ =>
                {
                    if (fileIds.Distinct().Count() != fileIds.Count)
                    {
                        return ValidationActionResult(FileIdsShouldBeDistinct);
                    }

                    var releaseFiles = await _contentDbContext.ReleaseFiles
                        .Include(releaseFile => releaseFile.File)
                        .Where(releaseFile =>
                            releaseFile.File.Type == FileType.Data
                            && releaseFile.ReleaseId == releaseId
                            && !releaseFile.File.ReplacingId.HasValue)
                        .ToDictionaryAsync(releaseFile => releaseFile.File.Id);

                    if (releaseFiles.Count != fileIds.Count)
                    {
                        return ValidationActionResult(IncorrectNumberOfFileIds);
                    }

                    fileIds.ForEach((fileId, order) =>
                    {
                        if (!releaseFiles.TryGetValue(fileId, out var matchingReleaseFile))
                        {
                            throw new ArgumentException(
                                $"fileId {fileId} not found in db as non-replacement related data file attached to the release {releaseId}");
                        }

                        matchingReleaseFile.Order = order;
                        _contentDbContext.Update(matchingReleaseFile);

                        if (matchingReleaseFile.File.ReplacedById != null)
                        {
                            var replacingReleaseFile = _contentDbContext.ReleaseFiles
                                .Single(releaseFile => releaseFile.FileId == matchingReleaseFile.File.ReplacedById);
                            replacingReleaseFile.Order = order;
                            _contentDbContext.Update(replacingReleaseFile);
                        }
                    });

                    await _contentDbContext.SaveChangesAsync();

                    return await ListAll(releaseId);
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
                .OnSuccess(async _ =>
                {
                    return await _persistenceHelper
                        .CheckOptionalEntityExists<File>(replacingFileId)
                        .OnSuccessDo(replacingFile =>
                            _fileUploadsValidatorService.ValidateDataFilesForUpload(releaseId, dataFormFile,
                                metaFormFile, replacingFile))
                        .OnSuccessCombineWith(replacingFile =>
                            ValidateSubjectName(releaseId, subjectName, replacingFile))
                        .OnSuccess(async replacingFileAndSubjectName =>
                        {
                            var (replacingFile, validSubjectName) = replacingFileAndSubjectName;

                            var subjectId =
                                await _releaseRepository
                                    .CreateStatisticsDbReleaseAndSubjectHierarchy(releaseId);

                            var releaseDataFileOrder = await GetNextDataFileOrder(replacingFile, releaseId);

                            var dataFile = await _releaseDataFileRepository.Create(
                                releaseId: releaseId,
                                subjectId: subjectId,
                                filename: dataFormFile.FileName.ToLower(),
                                contentLength: dataFormFile.Length,
                                type: FileType.Data,
                                createdById: _userService.GetUserId(),
                                name: validSubjectName,
                                replacingFile: replacingFile,
                                order: releaseDataFileOrder);

                            var metaFile = await _releaseDataFileRepository.Create(
                                releaseId: releaseId,
                                subjectId: subjectId,
                                filename: metaFormFile.FileName.ToLower(),
                                contentLength: metaFormFile.Length,
                                type: Metadata,
                                createdById: _userService.GetUserId());

                            await UploadFileToStorage(dataFile, dataFormFile);
                            await UploadFileToStorage(metaFile, metaFormFile);

                            var dataImport = await _dataImportService.Import(
                                subjectId: subjectId,
                                dataFile: dataFile,
                                metaFile: metaFile);

                            var permissions = await _userService.GetDataFilePermissions(dataFile);

                            return BuildDataFileViewModel(dataFile: dataFile,
                                metaFile: metaFile,
                                validSubjectName,
                                dataImport.TotalRows,
                                dataImport.Status,
                                permissions);
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
                .OnSuccess(async _ =>
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
                                                .ValidateDataArchiveEntriesForUpload(releaseId,
                                                    archiveFile,
                                                    replacingFile)
                                                .OnSuccess(async () =>
                                                {
                                                    var subjectId = await _releaseRepository
                                                        .CreateStatisticsDbReleaseAndSubjectHierarchy(releaseId);

                                                    var zipFile = await _releaseDataFileRepository.CreateZip(
                                                        filename: zipFormFile.FileName.ToLower(),
                                                        contentLength: zipFormFile.Length,
                                                        contentType: zipFormFile.ContentType,
                                                        releaseId: releaseId,
                                                        createdById: _userService.GetUserId());

                                                    var releaseDataFileOrder = await GetNextDataFileOrder(replacingFile, releaseId);

                                                    var dataFile = await _releaseDataFileRepository.Create(
                                                        releaseId: releaseId,
                                                        subjectId: subjectId,
                                                        filename: archiveFile.DataFileName,
                                                        contentLength: archiveFile.DataFileSize,
                                                        type: FileType.Data,
                                                        createdById: _userService.GetUserId(),
                                                        name: validSubjectName,
                                                        replacingFile: replacingFile,
                                                        source: zipFile,
                                                        order: releaseDataFileOrder);

                                                    var metaFile = await _releaseDataFileRepository.Create(
                                                        releaseId: releaseId,
                                                        subjectId: subjectId,
                                                        filename: archiveFile.MetaFileName,
                                                        contentLength: archiveFile.MetaFileSize,
                                                        type: Metadata,
                                                        createdById: _userService.GetUserId(),
                                                        source: zipFile);

                                                    await UploadFileToStorage(zipFile, zipFormFile);

                                                    var dataImport = await _dataImportService.ImportZip(
                                                        subjectId: subjectId,
                                                        dataFile: dataFile,
                                                        metaFile: metaFile,
                                                        zipFile: zipFile);

                                                    var permissions = await _userService.GetDataFilePermissions(dataFile);

                                                    return BuildDataFileViewModel(dataFile: dataFile,
                                                        metaFile: metaFile,
                                                        validSubjectName,
                                                        dataImport.TotalRows,
                                                        dataImport.Status,
                                                        permissions);
                                                });
                                        }));
                        });
                });
        }

        private async Task<DataFileInfo> BuildDataFileViewModel(ReleaseFile releaseFile)
        {
            var dataImport = await _contentDbContext.DataImports
                .AsSplitQuery()
                .Include(di => di.File)
                .ThenInclude(f => f.CreatedBy)
                .Include(di => di.MetaFile)
                .SingleAsync(di => di.FileId == releaseFile.FileId);

            var permissions = await _userService.GetDataFilePermissions(dataImport.File);

            return BuildDataFileViewModel(dataFile: dataImport.File,
                metaFile: dataImport.MetaFile,
                releaseFile.Name,
                dataImport.TotalRows,
                dataImport.Status,
                permissions);
        }

        private async Task<List<DataFileInfo>> BuildDataFileViewModels(List<ReleaseFile> releaseFiles)
        {
            var fileIds = releaseFiles.Select(rf => rf.FileId).ToList();

            var dataImports = await _contentDbContext.DataImports
                .AsSplitQuery()
                .Include(di => di.File)
                .ThenInclude(f => f.CreatedBy)
                .Include(di => di.MetaFile)
                .Where(di => fileIds.Contains(di.FileId))
                .ToDictionaryAsync(di => di.FileId);

            var subjectNames = releaseFiles.ToDictionary(rf => rf.FileId, rf => rf.Name);

            // TODO Optimise GetDataFilePermissions here instead of potentially making several db queries
            // Work out if the user has permission to cancel any import which Bau users can.
            // Combine it with the import status (already known) to evaluate whether a particular import can be cancelled
            var permissions = await releaseFiles
                .ToAsyncEnumerable()
                .ToDictionaryAwaitAsync(rf => ValueTask.FromResult(rf.FileId),
                    async rf => await _userService.GetDataFilePermissions(rf.File));

            return fileIds.Select(fileId =>
            {
                var dataImport = dataImports[fileId];
                return BuildDataFileViewModel(dataFile: dataImport.File,
                    metaFile: dataImport.MetaFile,
                    subjectNames[fileId],
                    dataImport.TotalRows,
                    dataImport.Status,
                    permissions[fileId]);
            }).ToList();
        }

        private static DataFileInfo BuildDataFileViewModel(File dataFile,
            File metaFile,
            string? subjectName,
            int? totalRows,
            DataImportStatus importStatus,
            DataFilePermissions permissions)
        {
            return new DataFileInfo
            {
                Id = dataFile.Id,
                FileName = dataFile.Filename,
                Name = subjectName ?? "Unknown",
                Size = dataFile.DisplaySize(),
                MetaFileId = metaFile.Id,
                MetaFileName = metaFile.Filename,
                ReplacedBy = dataFile.ReplacedById,
                Rows = totalRows,
                UserName = dataFile.CreatedBy?.Email ?? "",
                Status = importStatus,
                Created = dataFile.Created,
                Permissions = permissions
            };
        }

        private async Task<Either<ActionResult, string>> ValidateSubjectName(Guid releaseId,
            string? subjectName,
            File? replacingFile)
        {
            if (replacingFile != null)
            {
                if (replacingFile.Type != FileType.Data)
                {
                    throw new ArgumentException("replacingFile.Type should equal FileType.Data");
                }

                return (await _releaseFileRepository.Find(releaseId, replacingFile.Id))?.Name ?? "Unknown";
            }

            if (subjectName == null)
            {
                throw new ArgumentException("Original subjects cannot have null subject name");
            }

            return await _fileUploadsValidatorService.ValidateSubjectName(releaseId, subjectName)
                .OnSuccess(async () => await Task.FromResult(subjectName));
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

        private async Task<int> GetNextDataFileOrder(File? replacingFile, Guid releaseId)
        {
            if (replacingFile != null)
            {
                var replacedReleaseDataFile = await _contentDbContext.ReleaseFiles
                    .Include(rf => rf.File)
                    .SingleAsync(rf =>
                        rf.ReleaseId == releaseId
                        && rf.File.Type == FileType.Data
                        && rf.File.Id == replacingFile.Id);
                return replacedReleaseDataFile.Order;
            }

            var maxOrder = await _contentDbContext.ReleaseFiles
                .Include(releaseFile => releaseFile.File)
                .Where(releaseFile => releaseFile.ReleaseId == releaseId
                                      && releaseFile.File.Type == FileType.Data
                                      && releaseFile.File.ReplacingId == null)
                .MaxAsync(releaseFile => (int?)releaseFile.Order);

            return maxOrder.HasValue ? maxOrder.Value + 1 : 0;
        }
    }
}
