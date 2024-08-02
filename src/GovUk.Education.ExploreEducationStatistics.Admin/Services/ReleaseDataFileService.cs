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
using IReleaseVersionRepository =
    GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseVersionRepository;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseDataFileService : IReleaseDataFileService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IPrivateBlobStorageService _privateBlobStorageService;
        private readonly IDataArchiveValidationService _dataArchiveValidationService;
        private readonly IFileUploadsValidatorService _fileUploadsValidatorService;
        private readonly IFileRepository _fileRepository;
        private readonly IReleaseVersionRepository _releaseVersionRepository;
        private readonly IReleaseFileRepository _releaseFileRepository;
        private readonly IReleaseFileService _releaseFileService;
        private readonly IReleaseDataFileRepository _releaseDataFileRepository;
        private readonly IDataImportService _dataImportService;
        private readonly IUserService _userService;

        public ReleaseDataFileService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IPrivateBlobStorageService privateBlobStorageService,
            IDataArchiveValidationService dataArchiveValidationService,
            IFileUploadsValidatorService fileUploadsValidatorService,
            IFileRepository fileRepository,
            IReleaseVersionRepository releaseVersionRepository,
            IReleaseFileRepository releaseFileRepository,
            IReleaseFileService releaseFileService,
            IReleaseDataFileRepository releaseDataFileRepository,
            IDataImportService dataImportService,
            IUserService userService)
        {
            _contentDbContext = contentDbContext;
            _persistenceHelper = persistenceHelper;
            _privateBlobStorageService = privateBlobStorageService;
            _dataArchiveValidationService = dataArchiveValidationService;
            _fileUploadsValidatorService = fileUploadsValidatorService;
            _fileRepository = fileRepository;
            _releaseVersionRepository = releaseVersionRepository;
            _releaseFileRepository = releaseFileRepository;
            _releaseFileService = releaseFileService;
            _releaseDataFileRepository = releaseDataFileRepository;
            _dataImportService = dataImportService;
            _userService = userService;
        }

        public async Task<Either<ActionResult, Unit>> Delete(Guid releaseVersionId,
            Guid fileId,
            bool forceDelete = false)
        {
            return await Delete(releaseVersionId, [ fileId ], forceDelete: forceDelete);
        }

        public async Task<Either<ActionResult, Unit>> Delete(Guid releaseVersionId,
            IEnumerable<Guid> fileIds,
            bool forceDelete = false)
        {
            return await _persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(async releaseVersion =>
                    await _userService.CheckCanUpdateReleaseVersion(releaseVersion, ignoreCheck: forceDelete))
                .OnSuccess(async _ =>
                    await fileIds.Select(fileId => _releaseFileService.CheckFileExists(
                            releaseVersionId: releaseVersionId,
                            fileId: fileId,
                            FileType.Data))
                        .OnSuccessAll())
                .OnSuccessVoid(async files =>
                {
                    foreach (var file in files)
                    {
                        var metaFile = await GetAssociatedMetaFile(releaseVersionId, file);

                        if (await _releaseFileRepository.FileIsLinkedToOtherReleases(releaseVersionId, file.Id))
                        {
                            await _releaseFileRepository.Delete(releaseVersionId, file.Id);
                            await _releaseFileRepository.Delete(releaseVersionId, metaFile.Id);
                        }
                        else
                        {
                            await _dataImportService.DeleteImport(file.Id);
                            await _privateBlobStorageService.DeleteBlob(
                                PrivateReleaseFiles,
                                file.Path()
                            );
                            await _privateBlobStorageService.DeleteBlob(
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

                            await _releaseFileRepository.Delete(releaseVersionId, file.Id);
                            await _releaseFileRepository.Delete(releaseVersionId, metaFile.Id);

                            await _fileRepository.Delete(file.Id);
                            await _fileRepository.Delete(metaFile.Id);

                            if (file.SourceId.HasValue
                                // A bulk upload zip may be linked to multiple files - only delete zip if all have been removed
                                && !await _contentDbContext.Files.AnyAsync(f => f.SourceId == file.SourceId.Value))
                            {
                                var zipFile = await _fileRepository.Get(file.SourceId.Value);
                                await _privateBlobStorageService.DeleteBlob(
                                    PrivateReleaseFiles,
                                    zipFile.Path()
                                );
                                // N.B. No ReleaseFiles row for source links
                                await _fileRepository.Delete(zipFile.Id);
                            }
                        }
                    }
                });
        }

        public async Task<Either<ActionResult, Unit>> DeleteAll(Guid releaseVersionId,
            bool forceDelete = false)
        {
            var releaseFiles = await _releaseFileRepository.GetByFileType(releaseVersionId, types: FileType.Data);

            return await Delete(releaseVersionId,
                releaseFiles.Select(releaseFile => releaseFile.File.Id),
                forceDelete: forceDelete);
        }

        public async Task<Either<ActionResult, DataFileInfo>> GetInfo(Guid releaseVersionId,
            Guid fileId)
        {
            return await _persistenceHelper
                .CheckEntityExists<ReleaseFile>(q => q.Where(
                        rf => rf.ReleaseVersionId == releaseVersionId
                              && rf.File.Type == FileType.Data
                              && rf.FileId == fileId)
                    .Include(rf => rf.ReleaseVersion))
                .OnSuccessDo(rf => _userService.CheckCanViewReleaseVersion(rf.ReleaseVersion))
                .OnSuccess(BuildDataFileViewModel);
        }

        public async Task<Either<ActionResult, List<DataFileInfo>>> ListAll(Guid releaseVersionId)
        {
            return await _persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(_userService.CheckCanViewReleaseVersion)
                .OnSuccess(async () =>
                {
                    var files = await _releaseFileRepository.GetByFileType(releaseVersionId, types: FileType.Data);

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
            Guid releaseVersionId,
            List<Guid> fileIds)
        {
            return await _persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
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
                            && releaseFile.ReleaseVersionId == releaseVersionId
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
                                $"fileId {fileId} not found in db as non-replacement related data file attached to the release version {releaseVersionId}");
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

                    return await ListAll(releaseVersionId);
                });
        }

        public async Task<Either<ActionResult, DataFileInfo>> Upload(Guid releaseVersionId,
            IFormFile dataFormFile,
            IFormFile metaFormFile,
            string? dataSetTitle,
            Guid? replacingFileId)
        {
            return await _persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
                .OnSuccess(async _ =>
                {
                    return await _persistenceHelper
                        .CheckOptionalEntityExists<File>(replacingFileId)
                        .OnSuccessCombineWith(async replacingFile =>
                        {
                            var newDataSetTitle = await GetReleaseVersionDataSetTitle(
                                releaseVersionId, dataSetTitle, replacingFile);

                            var errors = await _fileUploadsValidatorService
                                .ValidateDataSetFilesForUpload(
                                    releaseVersionId,
                                    newDataSetTitle,
                                    dataFormFile,
                                    metaFormFile,
                                    replacingFile);

                            if (errors.Count > 0)
                            {
                                return new Either<ActionResult, string>(
                                    Common.Validators.ValidationUtils.ValidationResult(errors));
                            }

                            return newDataSetTitle;
                        })
                        .OnSuccess(async tuple => {

                            var (replacingFile, newDataSetTitle) = tuple;

                            var subjectId =
                                await _releaseVersionRepository
                                    .CreateStatisticsDbReleaseAndSubjectHierarchy(releaseVersionId);

                            var releaseDataFileOrder = await GetNextDataFileOrder(releaseVersionId, replacingFile);

                            var dataFile = await _releaseDataFileRepository.Create(
                                releaseVersionId: releaseVersionId,
                                subjectId: subjectId,
                                filename: dataFormFile.FileName.ToLower(),
                                contentLength: dataFormFile.Length,
                                type: FileType.Data,
                                createdById: _userService.GetUserId(),
                                name: newDataSetTitle,
                                replacingDataFile: replacingFile,
                                order: releaseDataFileOrder);

                            var dataReleaseFile = await _contentDbContext.ReleaseFiles
                                .Include(rf => rf.File)
                                .SingleAsync(rf =>
                                    rf.ReleaseVersionId == releaseVersionId
                                    && rf.FileId == dataFile.Id);

                            var metaFile = await _releaseDataFileRepository.Create(
                                releaseVersionId: releaseVersionId,
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

                            return BuildDataFileViewModel(
                                dataReleaseFile: dataReleaseFile,
                                metaFile: metaFile,
                                newDataSetTitle,
                                dataImport.TotalRows,
                                dataImport.Status,
                                permissions);
                        });
                });
        }

        public async Task<Either<ActionResult, DataFileInfo>> UploadAsZip(Guid releaseVersionId,
            IFormFile zipFormFile,
            string? dataSetTitle,
            Guid? replacingFileId)
        {
            return await _persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
                .OnSuccess(async _ =>
                {
                    return await _persistenceHelper.CheckOptionalEntityExists<File>(replacingFileId)
                        .OnSuccessCombineWith(async replacingFile =>
                        {
                            var newDataSetTitle = await GetReleaseVersionDataSetTitle(
                                releaseVersionId, dataSetTitle, replacingFile);

                            return await _dataArchiveValidationService
                                .ValidateDataArchiveFile(
                                    releaseVersionId,
                                    newDataSetTitle,
                                    zipFormFile,
                                    replacingFile);
                        })
                        .OnSuccess(async tuple =>
                        {
                            var (replacingFile, archiveDataSet) = tuple;

                            var zipFile = new File
                            {
                                CreatedById = _userService.GetUserId(),
                                RootPath = releaseVersionId,
                                ContentLength = zipFormFile.Length,
                                ContentType = zipFormFile.ContentType,
                                Filename = zipFormFile.FileName.ToLower(),
                                Type = DataZip,
                            };
                            _contentDbContext.Files.Add(zipFile);
                            await _contentDbContext.SaveChangesAsync();

                            var subjectId = await _releaseVersionRepository
                                .CreateStatisticsDbReleaseAndSubjectHierarchy(releaseVersionId);

                            var releaseDataFileOrder =
                                await GetNextDataFileOrder(releaseVersionId, replacingFile);

                            var dataFile = await _releaseDataFileRepository.Create(
                                releaseVersionId: releaseVersionId,
                                subjectId: subjectId,
                                filename: archiveDataSet.DataFilename,
                                contentLength: archiveDataSet.DataFileSize,
                                type: FileType.Data,
                                createdById: _userService.GetUserId(),
                                name: archiveDataSet.Title,
                                replacingDataFile: replacingFile,
                                source: zipFile,
                                order: releaseDataFileOrder);

                            var dataReleaseFile = await _contentDbContext.ReleaseFiles
                                .Include(rf => rf.File)
                                .SingleAsync(rf =>
                                    rf.ReleaseVersionId == releaseVersionId
                                    && rf.FileId == dataFile.Id);

                            var metaFile = await _releaseDataFileRepository.Create(
                                releaseVersionId: releaseVersionId,
                                subjectId: subjectId,
                                filename: archiveDataSet.MetaFilename,
                                contentLength: archiveDataSet.MetaFileSize,
                                type: Metadata,
                                createdById: _userService.GetUserId(),
                                source: zipFile);

                            // data/meta files are extracted to blob storage by _dataImportService.Import
                            await UploadFileToStorage(zipFile, zipFormFile);

                            var dataImport = await _dataImportService.Import(
                                subjectId: subjectId,
                                dataFile: dataFile,
                                metaFile: metaFile,
                                sourceZipFile: zipFile);

                            var permissions =
                                await _userService.GetDataFilePermissions(dataFile);

                            return BuildDataFileViewModel(
                                dataReleaseFile: dataReleaseFile,
                                metaFile: metaFile,
                                archiveDataSet.Title,
                                dataImport.TotalRows,
                                dataImport.Status,
                                permissions);
                        });
                });
        }

        public async Task<Either<ActionResult, List<DataFileInfo>>> UploadAsBulkZip(
            Guid releaseVersionId,
            IFormFile bulkZipFormFile)
        {
            return await _persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
                .OnSuccess(_ => _dataArchiveValidationService.ValidateBulkDataArchiveFile(
                        releaseVersionId, bulkZipFormFile)
                .OnSuccess(async dataArchiveFiles =>
                {
                    var bulkZipFile = new File
                    {
                        CreatedById = _userService.GetUserId(),
                        RootPath = releaseVersionId,
                        ContentLength = bulkZipFormFile.Length,
                        ContentType = bulkZipFormFile.ContentType,
                        Filename = bulkZipFormFile.FileName.ToLower(),
                        Type = BulkDataZip,
                    };
                    _contentDbContext.Files.Add(bulkZipFile);
                    await _contentDbContext.SaveChangesAsync();

                    // each data/meta file pair are extracted to blob storage by _dataImportService.Import
                    await UploadFileToStorage(bulkZipFile, bulkZipFormFile);

                        var results = new List<DataFileInfo>();

                        foreach (var archiveFile in dataArchiveFiles)
                        {
                            var subjectId = await _releaseVersionRepository
                                .CreateStatisticsDbReleaseAndSubjectHierarchy(releaseVersionId);

                            var releaseDataFileOrder = await GetNextDataFileOrder(releaseVersionId);

                            var dataFile = await _releaseDataFileRepository.Create(
                                releaseVersionId: releaseVersionId,
                                subjectId: subjectId,
                                filename: archiveFile.DataFilename,
                                contentLength: archiveFile.DataFileSize,
                                type: FileType.Data,
                                createdById: _userService.GetUserId(),
                                name: archiveFile.Title,
                                source: bulkZipFile,
                                order: releaseDataFileOrder);

                            var dataReleaseFile = await _contentDbContext.ReleaseFiles
                                .Include(rf => rf.File)
                                .SingleAsync(rf =>
                                    rf.ReleaseVersionId == releaseVersionId
                                    && rf.FileId == dataFile.Id);

                            var metaFile = await _releaseDataFileRepository.Create(
                                releaseVersionId: releaseVersionId,
                                subjectId: subjectId,
                                filename: archiveFile.MetaFilename,
                                contentLength: archiveFile.MetaFileSize,
                                type: Metadata,
                                createdById: _userService.GetUserId(),
                                source: bulkZipFile);

                            var dataImport = await _dataImportService.Import(
                                subjectId: subjectId,
                                dataFile: dataFile,
                                metaFile: metaFile,
                                sourceZipFile: bulkZipFile);

                            var permissions = await _userService.GetDataFilePermissions(dataFile);

                            results.Add(
                                BuildDataFileViewModel(
                                    dataReleaseFile: dataReleaseFile,
                                    metaFile: metaFile,
                                    archiveFile.Title,
                                    dataImport.TotalRows,
                                    dataImport.Status,
                                    permissions));
                        }

                        return results;
                    }));
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

            return BuildDataFileViewModel(
                dataReleaseFile: releaseFile,
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

            return releaseFiles.Select(releaseFile =>
            {
                var dataImport = dataImports[releaseFile.FileId];
                return BuildDataFileViewModel(
                    dataReleaseFile: releaseFile,
                    metaFile: dataImport.MetaFile,
                    subjectNames[releaseFile.FileId],
                    dataImport.TotalRows,
                    dataImport.Status,
                    permissions[releaseFile.FileId]);
            }).ToList();
        }

        private static DataFileInfo BuildDataFileViewModel(
            ReleaseFile dataReleaseFile,
            File metaFile,
            string? subjectName,
            int? totalRows,
            DataImportStatus importStatus,
            DataFilePermissions permissions)
        {
            return new DataFileInfo
            {
                Id = dataReleaseFile.FileId,
                FileName = dataReleaseFile.File.Filename,
                Name = subjectName ?? "Unknown",
                Size = dataReleaseFile.File.DisplaySize(),
                MetaFileId = metaFile.Id,
                MetaFileName = metaFile.Filename,
                ReplacedBy = dataReleaseFile.File.ReplacedById,
                Rows = totalRows,
                UserName = dataReleaseFile.File.CreatedBy?.Email ?? "",
                Status = importStatus,
                Created = dataReleaseFile.File.Created,
                Permissions = permissions,
                PublicApiDataSetId = dataReleaseFile.PublicApiDataSetId,
                PublicApiDataSetVersion = dataReleaseFile.PublicApiDataSetVersionString,
            };
        }

        // This method fetches the data set file title based on whether it's an original import or a replacement.
        // Replacement's use the title from the data set being replaced.
        private async Task<string> GetReleaseVersionDataSetTitle(Guid releaseVersionId, string? dataSetTitle = null,
            File? replacingFile = null)
        {
            if (replacingFile != null)
            {
                if (dataSetTitle != null)
                {
                    throw new ArgumentException("subjectName and replacingFile shouldn't both be provided");
                }

                if (replacingFile.Type != FileType.Data)
                {
                    throw new ArgumentException("replacingFile.Type should equal FileType.Data");
                }

                // No need to validate the data set title if a replacement is occurring and we're using the replacement title
                return (await _releaseFileRepository.Find(releaseVersionId: releaseVersionId,
                    fileId: replacingFile.Id))?.Name ?? "Unknown";
            }

            if (dataSetTitle == null)
            {
                throw new ArgumentException("New data set files cannot have a null data set title");
            }

            return dataSetTitle;
        }

        private async Task UploadFileToStorage(
            File file,
            IFormFile formFile)
        {
            await _privateBlobStorageService.UploadFile(
                containerName: PrivateReleaseFiles,
                path: file.Path(),
                file: formFile
            );
        }

        private async Task<File> GetAssociatedMetaFile(Guid releaseVersionId,
            File dataFile)
        {
            return await _contentDbContext.ReleaseFiles
                .Include(rf => rf.File)
                .Where(rf => rf.ReleaseVersionId == releaseVersionId
                             && rf.File.Type == Metadata
                             && rf.File.SubjectId == dataFile.SubjectId)
                .Select(rf => rf.File)
                .SingleAsync();
        }

        private async Task<int> GetNextDataFileOrder(
            Guid releaseVersionId,
            File? replacingFile = null)
        {
            if (replacingFile != null)
            {
                var replacedReleaseDataFile = await _contentDbContext.ReleaseFiles
                    .Include(rf => rf.File)
                    .SingleAsync(rf =>
                        rf.ReleaseVersionId == releaseVersionId
                        && rf.File.Type == FileType.Data
                        && rf.File.Id == replacingFile.Id);
                return replacedReleaseDataFile.Order;
            }

            var currentMaxOrder = await _contentDbContext.ReleaseFiles
                .Include(releaseFile => releaseFile.File)
                .Where(releaseFile => releaseFile.ReleaseVersionId == releaseVersionId
                                      && releaseFile.File.Type == FileType.Data
                                      && releaseFile.File.ReplacingId == null)
                .MaxAsync(releaseFile => (int?)releaseFile.Order);

            return currentMaxOrder.HasValue ? currentMaxOrder.Value + 1 : 0;
        }
    }
}
