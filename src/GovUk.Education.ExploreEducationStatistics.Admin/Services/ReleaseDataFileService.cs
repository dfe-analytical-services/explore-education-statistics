#nullable enable
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
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
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseVersionRepository;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseDataFileService : IReleaseDataFileService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IPrivateBlobStorageService _privateBlobStorageService;
        private readonly IDataSetArchiveValidationService _dataArchiveValidationService;
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
            IDataSetArchiveValidationService dataArchiveValidationService,
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
            return await Delete(releaseVersionId, [fileId], forceDelete: forceDelete);
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

        public async Task<Either<ActionResult, DataFileInfo>> ValidateAndUpload(
            Guid releaseVersionId,
            IFormFile dataFormFile,
            IFormFile metaFormFile,
            string? dataSetTitle,
            Guid? replacingFileId,
            CancellationToken cancellationToken)
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
                            var newDataSetTitle = await GetReleaseVersionDataSetTitle(releaseVersionId, dataSetTitle, replacingFile);
                            var dataSetFiles = await BuildDataSetFiles(dataFormFile, metaFormFile);

                            var errors = await _fileUploadsValidatorService.ValidateDataSet(
                                    releaseVersionId,
                                    newDataSetTitle,
                                    dataSetFiles,
                                    replacingFile);

                            if (errors.Count > 0)
                            {
                                return new Either<ActionResult, DataSet>(Common.Validators.ValidationUtils.ValidationResult(errors));
                            }

                            var dataSet = new DataSet
                            {
                                Title = newDataSetTitle,
                                DataFile = dataSetFiles.FirstOrDefault(file => !file.FileName.EndsWith(".meta.csv")),
                                MetaFile = dataSetFiles.FirstOrDefault(file => file.FileName.EndsWith(".meta.csv")),
                            };

                            return dataSet;
                        })
                        .OnSuccess(async tuple =>
                        {
                            var (replacingFile, dataSet) = tuple;

                            var subjectId =
                                await _releaseVersionRepository
                                    .CreateStatisticsDbReleaseAndSubjectHierarchy(releaseVersionId);

                            var releaseDataFileOrder = await GetNextDataFileOrder(releaseVersionId, replacingFile);

                            var dataFileEntity = await _releaseDataFileRepository.Create(
                                releaseVersionId,
                                subjectId,
                                dataFormFile.FileName.ToLower(),
                                contentLength: dataFormFile.Length,
                                type: FileType.Data,
                                createdById: _userService.GetUserId(),
                                name: dataSet.Title,
                                replacingFile,
                                order: releaseDataFileOrder);

                            var dataReleaseFile = await _contentDbContext.ReleaseFiles
                                .Include(rf => rf.File)
                                .SingleAsync(rf =>
                                    rf.ReleaseVersionId == releaseVersionId
                                    && rf.FileId == dataFileEntity.Id);

                            var metaFileEntity = await _releaseDataFileRepository.Create(
                                releaseVersionId: releaseVersionId,
                                subjectId: subjectId,
                                filename: metaFormFile.FileName.ToLower(),
                                contentLength: metaFormFile.Length,
                                type: FileType.Metadata,
                                createdById: _userService.GetUserId());

                            // TODO: Add overload function to handle data set pair upload
                            await UploadFileToStorage(dataFileEntity, dataFormFile);
                            await UploadFileToStorage(metaFileEntity, metaFormFile);

                            var dataImport = await _dataImportService.Import(
                                subjectId,
                                dataFileEntity,
                                metaFileEntity);

                            var permissions = await _userService.GetDataFilePermissions(dataFileEntity);

                            return BuildDataFileViewModel(
                                dataReleaseFile: dataReleaseFile,
                                metaFile: metaFileEntity,
                                dataSet.Title,
                                dataImport.TotalRows,
                                dataImport.Status,
                                permissions);
                        });
                });
        }

        // TODO: See if this function can be split into separate validate and upload functions, called independently by the controller
        public async Task<Either<ActionResult, DataFileInfo>> ValidateAndUploadFromZip(
            Guid releaseVersionId,
            IFormFile zipFormFile,
            string? dataSetTitle,
            Guid? replacingFileId,
            CancellationToken cancellationToken)
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
                            var newDataSetTitle = await GetReleaseVersionDataSetTitle(releaseVersionId, dataSetTitle, replacingFile);
                            var dataSetFiles = await ExtractDataSetArchive(zipFormFile);

                            var errors = await _fileUploadsValidatorService.ValidateDataSet(
                                releaseVersionId,
                                newDataSetTitle,
                                dataSetFiles,
                                replacingFile);

                            if (errors.Count > 0)
                            {
                                return new Either<ActionResult, DataSet>(Common.Validators.ValidationUtils.ValidationResult(errors));
                            }

                            var dataSet = new DataSet
                            {
                                Title = newDataSetTitle,
                                DataFile = dataSetFiles.FirstOrDefault(file => !file.FileName.EndsWith(".meta.csv")),
                                MetaFile = dataSetFiles.FirstOrDefault(file => file.FileName.EndsWith(".meta.csv")),
                            };

                            return dataSet;
                        })
                        .OnSuccess(async tuple =>
                        {
                            var (replacingFile, dataSet) = tuple;

                            var subjectId = await _releaseVersionRepository.CreateStatisticsDbReleaseAndSubjectHierarchy(releaseVersionId);
                            var releaseDataFileOrder = await GetNextDataFileOrder(releaseVersionId, replacingFile);

                            // TODO: Put the entity creations/imports inside an OnSuccess?
                            var dataFileEntity = await _releaseDataFileRepository.Create(
                                releaseVersionId,
                                subjectId,
                                dataSet.DataFile!.FileName,
                                contentLength: dataSet.DataFile.FileStream.Length,
                                type: FileType.Data,
                                createdById: _userService.GetUserId(),
                                name: dataSet.Title,
                                replacingFile,
                                order: releaseDataFileOrder);

                            var dataReleaseFile = await _contentDbContext.ReleaseFiles
                                .Include(rf => rf.File)
                                .SingleAsync(rf =>
                                    rf.ReleaseVersionId == releaseVersionId
                                    && rf.FileId == dataFileEntity.Id);

                            var metaFileEntity = await _releaseDataFileRepository.Create(
                                releaseVersionId,
                                subjectId,
                                dataSet.MetaFile!.FileName,
                                contentLength: dataSet.MetaFile.FileStream.Length,
                                type: FileType.Metadata,
                                createdById: _userService.GetUserId());

                            // TODO: Add overload function to handle data set pair upload (similar to UploadDataSetToTempStorage)
                            await UploadFileToReleaseStorage(releaseVersionId, dataFileEntity.Id, dataSet.DataFile.FileStream, FileType.Data, cancellationToken);
                            await UploadFileToReleaseStorage(releaseVersionId, metaFileEntity.Id, dataSet.MetaFile.FileStream, FileType.Metadata, cancellationToken);

                            var dataImport = await _dataImportService.Import(
                                subjectId,
                                dataFileEntity,
                                metaFileEntity);

                            var permissions = await _userService.GetDataFilePermissions(dataFileEntity);

                            return BuildDataFileViewModel(
                                dataReleaseFile: dataReleaseFile,
                                metaFile: metaFileEntity,
                                dataSet.Title,
                                dataImport.TotalRows,
                                dataImport.Status,
                                permissions);
                        });
                });
        }

        // TODO: Extract these two methods to a separate DataDetBuilder class
        // Optionally combine/refactor to reduce duplication of stream reading logic
        private static async Task<List<DataSetFileDto>> BuildDataSetFiles(
            IFormFile dataFormFile,
            IFormFile metaFormFile)
        {
            var files = new List<DataSetFileDto>();

            using var dataFileStream = dataFormFile.OpenReadStream();
            var dataMemoryStream = new System.IO.MemoryStream();
            await dataFileStream.CopyToAsync(dataMemoryStream);

            files.Add(new DataSetFileDto
            {
                FileName = dataFormFile.FileName,
                FileSize = dataMemoryStream.Length,
                FileStream = dataMemoryStream,
            });

            using var metaFileStream = metaFormFile.OpenReadStream();
            var metaMemoryStream = new System.IO.MemoryStream();
            await metaFileStream.CopyToAsync(metaMemoryStream);

            files.Add(new DataSetFileDto
            {
                FileName = metaFormFile.FileName,
                FileSize = metaMemoryStream.Length,
                FileStream = metaMemoryStream,
            });

            dataMemoryStream.SeekToBeginning();
            metaMemoryStream.SeekToBeginning();

            return files;
        }

        private static async Task<List<DataSetFileDto>> ExtractDataSetArchive(IFormFile zipFile)
        {
            await using var stream = zipFile.OpenReadStream();
            using var archive = new ZipArchive(stream);

            var files = new List<DataSetFileDto>();

            foreach (var entry in archive.Entries)
            {
                using var fileStream = entry.Open();
                var memoryStream = new System.IO.MemoryStream();
                await fileStream.CopyToAsync(memoryStream);

                files.Add(new DataSetFileDto
                {
                    FileName = entry.Name,
                    FileSize = memoryStream.Length,
                    FileStream = memoryStream,
                });

                memoryStream.SeekToBeginning();
            }

            return files;
        }

        public async Task<Either<ActionResult, List<ArchiveDataSetFileViewModel>>> ValidateAndUploadFromBulkZip(
            Guid releaseVersionId,
            IFormFile zipFormFile,
            CancellationToken cancellationToken)
        {
            return await _persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
                .OnSuccess(async _ =>
                {
                    var dataSetFiles = await ExtractDataSetArchive(zipFormFile);
                    return await _dataArchiveValidationService.ValidateBulkDataArchiveFiles(releaseVersionId, dataSetFiles);
                }).OnSuccess(async validatedDataSets =>
                {
                    var viewModels = new List<ArchiveDataSetFileViewModel>();

                    foreach (var dataSet in validatedDataSets)
                    {
                        var uploadResult = await UploadDataSetToTempStorage(releaseVersionId, dataSet, cancellationToken);

                        viewModels.Add(new ArchiveDataSetFileViewModel
                        {
                            Title = dataSet.Title,
                            DataFileId = uploadResult.DataFileId,
                            DataFilename = dataSet.DataFile.FileName,
                            DataFileSize = dataSet.DataFile.FileSize,
                            MetaFileId = uploadResult.MetaFileId,
                            MetaFilename = dataSet.MetaFile.FileName,
                            MetaFileSize = dataSet.MetaFile.FileSize,
                            ReplacingFileId = dataSet.ReplacingFile?.Id,
                        });
                    }

                    return viewModels;
                });
        }

        public async Task<Either<ActionResult, List<DataFileInfo>>> SaveDataSetsFromTemporaryBlobStorage(
            Guid releaseVersionId,
            List<ArchiveDataSetFileViewModel> archiveDataSetFiles,
            CancellationToken cancellationToken)
        {
            return await _persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
                .OnSuccess(() => archiveDataSetFiles
                    .Select(importRequest => ValidateTempDataSetFileExistence(releaseVersionId, importRequest))
                    .OnSuccessAll())
                .OnSuccess(async _ =>
                {
                    var releaseFiles = new List<ReleaseFile>();

                    foreach (var archiveDataSetFile in archiveDataSetFiles)
                    {
                        var subjectId = await _releaseVersionRepository.CreateStatisticsDbReleaseAndSubjectHierarchy(releaseVersionId);

                        var replacingFile = archiveDataSetFile.ReplacingFileId is null
                            ? null
                            : await _contentDbContext.Files.FirstAsync(f => f.Id == archiveDataSetFile.ReplacingFileId);

                        var releaseDataFileOrder = await GetNextDataFileOrder(releaseVersionId, replacingFile);

                        var dataFile = await _releaseDataFileRepository.Create(
                            releaseVersionId: releaseVersionId,
                            subjectId: subjectId,
                            filename: archiveDataSetFile.DataFilename,
                            contentLength: archiveDataSetFile.DataFileSize,
                            type: FileType.Data,
                            createdById: _userService.GetUserId(),
                            name: archiveDataSetFile.Title,
                            replacingDataFile: replacingFile,
                            order: releaseDataFileOrder);

                        var sourceDataFilePath = FileExtensions.Path(releaseVersionId, FileType.Data, archiveDataSetFile.DataFileId);
                        var destinationDataFilePath = FileExtensions.Path(releaseVersionId, FileType.Data, dataFile.Id); // Same path, but a new ID has been generated by the creation step above

                        var dataReleaseFile = await _contentDbContext.ReleaseFiles
                            .Include(rf => rf.File)
                            .SingleAsync(rf =>
                                rf.ReleaseVersionId == releaseVersionId
                                && rf.FileId == dataFile.Id);

                        releaseFiles.Add(dataReleaseFile);

                        var metaFile = await _releaseDataFileRepository.Create(
                            releaseVersionId: releaseVersionId,
                            subjectId: subjectId,
                            filename: archiveDataSetFile.MetaFilename,
                            contentLength: archiveDataSetFile.MetaFileSize,
                            type: FileType.Metadata,
                            createdById: _userService.GetUserId());

                        var sourceMetaFilePath = FileExtensions.Path(releaseVersionId, FileType.Metadata, archiveDataSetFile.MetaFileId);
                        var destinationMetaFilePath = FileExtensions.Path(releaseVersionId, FileType.Metadata, metaFile.Id); // Same path, but a new ID has been generated by the creation step above

                        await _dataImportService.Import(
                            subjectId: subjectId,
                            dataFile: dataFile,
                            metaFile: metaFile);

                        await _privateBlobStorageService.MoveBlob(PrivateReleaseTempFiles, sourceDataFilePath, destinationDataFilePath, PrivateReleaseFiles);
                        await _privateBlobStorageService.MoveBlob(PrivateReleaseTempFiles, sourceMetaFilePath, destinationMetaFilePath, PrivateReleaseFiles);
                    }

                    return await BuildDataFileViewModels(releaseFiles);
                });
        }

        private async Task<Either<ActionResult, Unit>> ValidateTempDataSetFileExistence(
            Guid releaseVersionId,
            ArchiveDataSetFileViewModel fileImportRequest)
        {
            var dataBlobExists = await _privateBlobStorageService.CheckBlobExists(PrivateReleaseTempFiles, $"{FileExtensions.Path(releaseVersionId, FileType.Data, fileImportRequest.DataFileId)}");
            var metaBlobExists = await _privateBlobStorageService.CheckBlobExists(PrivateReleaseTempFiles, $"{FileExtensions.Path(releaseVersionId, FileType.Metadata, fileImportRequest.MetaFileId)}");

            if (!dataBlobExists || !metaBlobExists)
            {
                throw new Exception("Unable to locate temporary files at the locations specified");
            }

            return Unit.Instance;
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

        /// <summary>
        /// Upload the supplied data set files to temporary blob storage.
        /// </summary>
        /// <returns>An object consisting of newly generated IDs representing the uploaded files. The IDs are used to locate the files in virtual storage.</returns>
        private async Task<DataSetUploadResult> UploadDataSetToTempStorage(
            Guid releaseVersionId,
            ArchivedDataSet archivedDataSet,
            CancellationToken cancellationToken)
        {
            var dataFileId = Guid.NewGuid();
            var metaFileId = Guid.NewGuid();
            var dataFilePath = $"{FileStoragePathUtils.FilesPath(releaseVersionId, FileType.Data)}{dataFileId}";
            var metaFilePath = $"{FileStoragePathUtils.FilesPath(releaseVersionId, FileType.Metadata)}{metaFileId}";

            await _privateBlobStorageService.UploadStream(
                containerName: PrivateReleaseTempFiles,
                path: dataFilePath,
                stream: archivedDataSet.DataFile.FileStream,
                contentType: ContentTypes.Csv,
                cancellationToken: cancellationToken);

            await _privateBlobStorageService.UploadStream(
                containerName: PrivateReleaseTempFiles,
                path: metaFilePath,
                stream: archivedDataSet.MetaFile.FileStream,
                contentType: ContentTypes.Csv,
                cancellationToken: cancellationToken);

            await archivedDataSet.DataFile.FileStream.DisposeAsync();
            await archivedDataSet.MetaFile.FileStream.DisposeAsync();

            return new DataSetUploadResult
            {
                DataFileId = dataFileId,
                MetaFileId = metaFileId
            };
        }

        private async Task UploadFileToReleaseStorage(
            Guid releaseVersionId,
            Guid fileId,
            System.IO.Stream fileStream,
            FileType fileType,
            CancellationToken cancellationToken)
        {
            var path = $"{FileStoragePathUtils.FilesPath(releaseVersionId, fileType)}{fileId}";

            await _privateBlobStorageService.UploadStream(
                containerName: PrivateReleaseFiles,
                path,
                fileStream,
                contentType: ContentTypes.Csv,
                cancellationToken: cancellationToken);

            await fileStream.DisposeAsync();
        }

        private async Task<File> GetAssociatedMetaFile(Guid releaseVersionId,
            File dataFile)
        {
            return await _contentDbContext.ReleaseFiles
                .Include(rf => rf.File)
                .Where(rf => rf.ReleaseVersionId == releaseVersionId
                    && rf.File.Type == FileType.Metadata
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
