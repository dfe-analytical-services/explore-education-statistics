#nullable enable
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
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Options;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using IReleaseVersionRepository =
    GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseVersionRepository;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseDataFileService(
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
        IUserService userService,
        IDataSetVersionService dataSetVersionService,
        IOptions<FeatureFlags> featureFlags)
        : IReleaseDataFileService
    {
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
            return await persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(async releaseVersion =>
                    await userService.CheckCanUpdateReleaseVersion(releaseVersion, ignoreCheck: forceDelete))
                .OnSuccess(async _ =>
                    await fileIds.Select(fileId => releaseFileService.CheckFileExists(
                            releaseVersionId: releaseVersionId,
                            fileId: fileId,
                            FileType.Data))
                        .OnSuccessAll())
                .OnSuccessVoid(async files =>
                {
                    foreach (var file in files)
                    {
                        var metaFile = await GetAssociatedMetaFile(releaseVersionId, file);

                        if (await releaseFileRepository.FileIsLinkedToOtherReleases(releaseVersionId, file.Id))
                        {
                            await releaseFileRepository.Delete(releaseVersionId, file.Id);
                            await releaseFileRepository.Delete(releaseVersionId, metaFile.Id);
                        }
                        else
                        {
                            await dataImportService.DeleteImport(file.Id);
                            await privateBlobStorageService.DeleteBlob(
                                PrivateReleaseFiles,
                                file.Path()
                            );
                            await privateBlobStorageService.DeleteBlob(
                                PrivateReleaseFiles,
                                metaFile.Path()
                            );

                            // If this is a replacement then unlink it from the original
                            if (file.ReplacingId.HasValue)
                            {
                                var originalFile = await fileRepository.Get(file.ReplacingId.Value);
                                originalFile.ReplacedById = null;
                                contentDbContext.Update(originalFile);
                                await contentDbContext.SaveChangesAsync();
                            }

                            await releaseFileRepository.Delete(releaseVersionId, file.Id);
                            await releaseFileRepository.Delete(releaseVersionId, metaFile.Id);

                            await fileRepository.Delete(file.Id);
                            await fileRepository.Delete(metaFile.Id);

                            if (file.SourceId.HasValue
                                // A bulk upload zip may be linked to multiple files - only delete zip if all have been removed
                                && !await contentDbContext.Files.AnyAsync(f => f.SourceId == file.SourceId.Value))
                            {
                                var zipFile = await fileRepository.Get(file.SourceId.Value);
                                await privateBlobStorageService.DeleteBlob(
                                    PrivateReleaseFiles,
                                    zipFile.Path()
                                );
                                // N.B. No ReleaseFiles row for source links
                                await fileRepository.Delete(zipFile.Id);
                            }
                        }
                    }
                });
        }

        public async Task<Either<ActionResult, Unit>> DeleteAll(Guid releaseVersionId,
            bool forceDelete = false)
        {
            var releaseFiles = await releaseFileRepository.GetByFileType(releaseVersionId, types: FileType.Data);

            return await Delete(releaseVersionId,
                releaseFiles.Select(releaseFile => releaseFile.File.Id),
                forceDelete: forceDelete);
        }

        public async Task<Either<ActionResult, DataFileInfo>> GetInfo(Guid releaseVersionId,
            Guid fileId)
        {
            return await persistenceHelper
                .CheckEntityExists<ReleaseFile>(q => q.Where(
                        rf => rf.ReleaseVersionId == releaseVersionId
                              && rf.File.Type == FileType.Data
                              && rf.FileId == fileId)
                    .Include(rf => rf.ReleaseVersion))
                .OnSuccessDo(rf => userService.CheckCanViewReleaseVersion(rf.ReleaseVersion))
                .OnSuccess(BuildDataFileViewModel);
        }

        public async Task<Either<ActionResult, List<DataFileInfo>>> ListAll(Guid releaseVersionId)
        {
            return await persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(userService.CheckCanViewReleaseVersion)
                .OnSuccess(async () =>
                {
                    var files = await releaseFileRepository.GetByFileType(releaseVersionId, types: FileType.Data);

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
            return await persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(userService.CheckCanUpdateReleaseVersion)
                .OnSuccess(async _ =>
                {
                    if (fileIds.Distinct().Count() != fileIds.Count)
                    {
                        return ValidationActionResult(FileIdsShouldBeDistinct);
                    }

                    var releaseFiles = await contentDbContext.ReleaseFiles
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
                        contentDbContext.Update(matchingReleaseFile);

                        if (matchingReleaseFile.File.ReplacedById != null)
                        {
                            var replacingReleaseFile = contentDbContext.ReleaseFiles
                                .Single(releaseFile => releaseFile.FileId == matchingReleaseFile.File.ReplacedById);
                            replacingReleaseFile.Order = order;
                            contentDbContext.Update(replacingReleaseFile);
                        }
                    });

                    await contentDbContext.SaveChangesAsync();

                    return await ListAll(releaseVersionId);
                });
        }

        public async Task<Either<ActionResult, DataFileInfo>> Upload(Guid releaseVersionId,
            IFormFile dataFormFile,
            IFormFile metaFormFile,
            string? dataSetTitle,
            Guid? replacingFileId)
        {
            return await persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(userService.CheckCanUpdateReleaseVersion)
                .OnSuccess(async _ =>
                {
                    return await persistenceHelper
                        .CheckOptionalEntityExists<File>(replacingFileId)
                        .OnSuccessCombineWith(async replacingFile =>
                        {
                            var newDataSetTitle = await GetReleaseVersionDataSetTitle(
                                releaseVersionId, dataSetTitle, replacingFile);

                            var errors = await fileUploadsValidatorService
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
                        .OnSuccess(async tuple =>
                        {
                            var (replacingFile, newDataSetTitle) = tuple;
                            var subjectId = 
                                await releaseVersionRepository
                                    .CreateStatisticsDbReleaseAndSubjectHierarchy(releaseVersionId);
                            ReleaseFile? replacedReleaseDataFile = null;

                            if (replacingFile is not null)
                            {
                                replacedReleaseDataFile = await GetReplacedReleaseFile(releaseVersionId, replacingFile);
                            }
                            
                            var releaseDataFileOrder = await GetNextDataFileOrder(releaseVersionId, replacedReleaseDataFile);

                            var dataFile = await releaseDataFileRepository.Create(
                                releaseVersionId: releaseVersionId,
                                subjectId: subjectId,
                                filename: dataFormFile.FileName.ToLower(),
                                contentLength: dataFormFile.Length,
                                type: FileType.Data,
                                createdById: userService.GetUserId(),
                                name: newDataSetTitle,
                                replacingDataFile: replacingFile,
                                order: releaseDataFileOrder);

                            var dataReleaseFile = await contentDbContext.ReleaseFiles
                                .Include(rf => rf.File)
                                .SingleAsync(rf =>
                                    rf.ReleaseVersionId == releaseVersionId
                                    && rf.FileId == dataFile.Id);

                            var metaFile = await releaseDataFileRepository.Create(
                                releaseVersionId: releaseVersionId,
                                subjectId: subjectId,
                                filename: metaFormFile.FileName.ToLower(),
                                contentLength: metaFormFile.Length,
                                type: FileType.Metadata,
                                createdById: userService.GetUserId());

                            await UploadFileToStorage(dataFile, dataFormFile);
                            await UploadFileToStorage(metaFile, metaFormFile);

                            if (featureFlags.Value.EnableReplacementOfPublicApiDataSets)
                            {
                                if (replacingFile is not null && contentDbContext.ReleaseFiles
                                        .Any(rf =>
                                            rf.ReleaseVersionId == releaseVersionId
                                            && rf.Name == newDataSetTitle
                                            && rf.PublicApiDataSetId != null))
                                {
                                    // Creates a new data set version to enable replacement. 
                                    await dataSetVersionService.CreateNextVersion(
                                        dataReleaseFile.Id,
                                        (Guid)replacedReleaseDataFile?.PublicApiDataSetId!,
                                        replacedReleaseDataFile?.PublicApiDataSetVersion
                                    );
                                }
                            }

                            var dataImport = await dataImportService.Import(
                                subjectId: subjectId,
                                dataFile: dataFile,
                                metaFile: metaFile);

                            var permissions = await userService.GetDataFilePermissions(dataFile);

                            return BuildDataFileViewModel(
                                dataReleaseFile: dataReleaseFile,
                                metaFile: metaFile,
                                newDataSetTitle,
                                dataImport!.TotalRows,
                                dataImport!.Status,
                                permissions);
                        });
                });
        }

        public async Task<Either<ActionResult, DataFileInfo>> UploadAsZip(Guid releaseVersionId,
            IFormFile zipFormFile,
            string? dataSetTitle,
            Guid? replacingFileId)
        {
            return await persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(userService.CheckCanUpdateReleaseVersion)
                .OnSuccess(async _ =>
                {
                    return await persistenceHelper.CheckOptionalEntityExists<File>(replacingFileId)
                        .OnSuccessCombineWith(async replacingFile =>
                        {
                            var newDataSetTitle = await GetReleaseVersionDataSetTitle(
                                releaseVersionId, dataSetTitle, replacingFile);

                            return await dataArchiveValidationService
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
                                CreatedById = userService.GetUserId(),
                                RootPath = releaseVersionId,
                                ContentLength = zipFormFile.Length,
                                ContentType = zipFormFile.ContentType,
                                Filename = zipFormFile.FileName.ToLower(),
                                Type = FileType.DataZip,
                            };
                            contentDbContext.Files.Add(zipFile);
                            await contentDbContext.SaveChangesAsync();

                            var subjectId = await releaseVersionRepository
                                .CreateStatisticsDbReleaseAndSubjectHierarchy(releaseVersionId);
                            ReleaseFile? replacedReleaseDataFile = null;

                            if (replacingFile is not null)
                            {
                                replacedReleaseDataFile = await GetReplacedReleaseFile(releaseVersionId, replacingFile);
                            }
                            var releaseDataFileOrder = await 
                                GetNextDataFileOrder(releaseVersionId, replacedReleaseDataFile);

                            var dataFile = await releaseDataFileRepository.Create(
                                releaseVersionId: releaseVersionId,
                                subjectId: subjectId,
                                filename: archiveDataSet.DataFilename,
                                contentLength: archiveDataSet.DataFileSize,
                                type: FileType.Data,
                                createdById: userService.GetUserId(),
                                name: archiveDataSet.Title,
                                replacingDataFile: replacingFile,
                                source: zipFile,
                                order: releaseDataFileOrder);

                            var dataReleaseFile = await contentDbContext.ReleaseFiles
                                .Include(rf => rf.File)
                                .SingleAsync(rf =>
                                    rf.ReleaseVersionId == releaseVersionId
                                    && rf.FileId == dataFile.Id);

                            var metaFile = await releaseDataFileRepository.Create(
                                releaseVersionId: releaseVersionId,
                                subjectId: subjectId,
                                filename: archiveDataSet.MetaFilename,
                                contentLength: archiveDataSet.MetaFileSize,
                                type: FileType.Metadata,
                                createdById: userService.GetUserId(),
                                source: zipFile);

                            // data/meta files are extracted to blob storage by _dataImportService.Import
                            await UploadFileToStorage(zipFile, zipFormFile);

                            var dataImport = await dataImportService.Import(
                                subjectId: subjectId,
                                dataFile: dataFile,
                                metaFile: metaFile,
                                sourceZipFile: zipFile);

                            var permissions =
                                await userService.GetDataFilePermissions(dataFile);

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

        public async Task<Either<ActionResult, List<ArchiveDataSetFileViewModel>>> ValidateAndUploadBulkZip(
            Guid releaseVersionId,
            IFormFile zipFile,
            CancellationToken cancellationToken)
        {
            return await persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(userService.CheckCanUpdateReleaseVersion)
                .OnSuccessVoid(async _ =>
                {
                    var errors = await dataArchiveValidationService.IsValidZipFile(zipFile);
                    if (errors.Count > 0)
                    {
                        return new Either<ActionResult, Unit>(Common.Validators.ValidationUtils.ValidationResult(errors));
                    }

                    return Unit.Instance;
                })
                .OnSuccess(async _ => await dataArchiveValidationService.ValidateBulkDataArchiveFiles(releaseVersionId, zipFile))
                .OnSuccess(async dataSetFiles =>
                {
                    await using var stream = zipFile.OpenReadStream();
                    var archive = new ZipArchive(stream);

                    var viewModels = new List<ArchiveDataSetFileViewModel>();

                    foreach (var file in dataSetFiles)
                    {
                        var dataFileId = await UploadFileToTempStorage(releaseVersionId, archive.GetEntry(file.DataFilename)!, FileType.Data, cancellationToken);
                        var metaFileId = await UploadFileToTempStorage(releaseVersionId, archive.GetEntry(file.MetaFilename)!, FileType.Metadata, cancellationToken);

                        viewModels.Add(new ArchiveDataSetFileViewModel
                        {
                            Title = file.Title,
                            DataFilename = file.DataFilename,
                            MetaFilename = file.MetaFilename,
                            DataFileId = dataFileId,
                            MetaFileId = metaFileId,
                            DataFileSize = file.DataFileSize,
                            MetaFileSize = file.MetaFileSize,
                            ReplacingFileId = file.ReplacingFile?.Id,
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
            return await persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(userService.CheckCanUpdateReleaseVersion)
                .OnSuccess(() => archiveDataSetFiles
                    .Select(importRequest => ValidateTempDataSetFileExistence(releaseVersionId, importRequest))
                    .OnSuccessAll())
                .OnSuccess(async _ =>
                {
                    var releaseFiles = new List<ReleaseFile>();

                    foreach (var archiveDataSetFile in archiveDataSetFiles)
                    {
                        var subjectId = await releaseVersionRepository.CreateStatisticsDbReleaseAndSubjectHierarchy(releaseVersionId);

                        var replacingFile = archiveDataSetFile.ReplacingFileId is null
                            ? null
                            : await contentDbContext.Files.FirstAsync(f => f.Id == archiveDataSetFile.ReplacingFileId, cancellationToken: cancellationToken);
                        ReleaseFile? replacedReleaseDataFile = null;

                        if (replacingFile is not null)
                        {
                            replacedReleaseDataFile = await GetReplacedReleaseFile(releaseVersionId, replacingFile);
                        }

                        var releaseDataFileOrder = await GetNextDataFileOrder(releaseVersionId, replacedReleaseDataFile);

                        var dataFile = await releaseDataFileRepository.Create(
                            releaseVersionId: releaseVersionId,
                            subjectId: subjectId,
                            filename: archiveDataSetFile.DataFilename,
                            contentLength: archiveDataSetFile.DataFileSize,
                            type: FileType.Data,
                            createdById: userService.GetUserId(),
                            name: archiveDataSetFile.Title,
                            replacingDataFile: replacingFile,
                            order: releaseDataFileOrder);

                        var sourceDataFilePath = FileExtensions.Path(releaseVersionId, FileType.Data, archiveDataSetFile.DataFileId);
                        var destinationDataFilePath = FileExtensions.Path(releaseVersionId, FileType.Data, dataFile.Id); // Same path, but a new ID has been generated by the creation step above

                        var dataReleaseFile = await contentDbContext.ReleaseFiles
                            .Include(rf => rf.File)
                            .SingleAsync(rf =>
                                rf.ReleaseVersionId == releaseVersionId
                                && rf.FileId == dataFile.Id, 
                                cancellationToken);

                        releaseFiles.Add(dataReleaseFile);

                        var metaFile = await releaseDataFileRepository.Create(
                            releaseVersionId: releaseVersionId,
                            subjectId: subjectId,
                            filename: archiveDataSetFile.MetaFilename,
                            contentLength: archiveDataSetFile.MetaFileSize,
                            type: FileType.Metadata,
                            createdById: userService.GetUserId());

                        var sourceMetaFilePath = FileExtensions.Path(releaseVersionId, FileType.Metadata, archiveDataSetFile.MetaFileId);
                        var destinationMetaFilePath = FileExtensions.Path(releaseVersionId, FileType.Metadata, metaFile.Id); // Same path, but a new ID has been generated by the creation step above

                        await dataImportService.Import(
                            subjectId: subjectId,
                            dataFile: dataFile,
                            metaFile: metaFile);

                        await privateBlobStorageService.MoveBlob(PrivateReleaseTempFiles, sourceDataFilePath, destinationDataFilePath, PrivateReleaseFiles);
                        await privateBlobStorageService.MoveBlob(PrivateReleaseTempFiles, sourceMetaFilePath, destinationMetaFilePath, PrivateReleaseFiles);
                    }

                    return await BuildDataFileViewModels(releaseFiles);
                });
        }

        private async Task<Either<ActionResult, Unit>> ValidateTempDataSetFileExistence(
            Guid releaseVersionId,
            ArchiveDataSetFileViewModel fileImportRequest)
        {
            var dataBlobExists = await privateBlobStorageService.CheckBlobExists(PrivateReleaseTempFiles, $"{FileExtensions.Path(releaseVersionId, FileType.Data, fileImportRequest.DataFileId)}");
            var metaBlobExists = await privateBlobStorageService.CheckBlobExists(PrivateReleaseTempFiles, $"{FileExtensions.Path(releaseVersionId, FileType.Metadata, fileImportRequest.MetaFileId)}");

            if (!dataBlobExists || !metaBlobExists)
            {
                throw new Exception("Unable to locate temporary files at the locations specified");
            }

            return Unit.Instance;
        }

        private async Task<DataFileInfo> BuildDataFileViewModel(ReleaseFile releaseFile)
        {
            var dataImport = await contentDbContext.DataImports
                .AsSplitQuery()
                .Include(di => di.File)
                .ThenInclude(f => f.CreatedBy)
                .Include(di => di.MetaFile)
                .SingleAsync(di => di.FileId == releaseFile.FileId);

            var permissions = await userService.GetDataFilePermissions(dataImport.File);

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

            var dataImports = await contentDbContext.DataImports
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
                    async rf => await userService.GetDataFilePermissions(rf.File));

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
                return (await releaseFileRepository.Find(releaseVersionId: releaseVersionId,
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
            await privateBlobStorageService.UploadFile(
                containerName: PrivateReleaseFiles,
                path: file.Path(),
                file: formFile
            );
        }

        private async Task<Guid> UploadFileToTempStorage(
            Guid releaseVersionId,
            ZipArchiveEntry archiveEntry,
            FileType fileType,
            CancellationToken cancellationToken)
        {
            var fileId = Guid.NewGuid();
            var path = $"{FileStoragePathUtils.FilesPath(releaseVersionId, fileType)}{fileId}";

            await privateBlobStorageService.UploadStream(
                containerName: PrivateReleaseTempFiles,
                path: path,
                stream: archiveEntry.Open(),
                contentType: ContentTypes.Csv,
                cancellationToken: cancellationToken);

            return fileId;
        }

        private async Task<File> GetAssociatedMetaFile(Guid releaseVersionId,
            File dataFile)
        {
            return await contentDbContext.ReleaseFiles
                .Include(rf => rf.File)
                .Where(rf => rf.ReleaseVersionId == releaseVersionId
                    && rf.File.Type == FileType.Metadata
                    && rf.File.SubjectId == dataFile.SubjectId)
                .Select(rf => rf.File)
                .SingleAsync();
        }

        private async Task<int> GetNextDataFileOrder(
            Guid releaseVersionId,
            ReleaseFile? replacedReleaseDataFile = null)
        {
            if (replacedReleaseDataFile != null)
            {
                return replacedReleaseDataFile.Order;
            }

            var currentMaxOrder = await contentDbContext.ReleaseFiles
                .Include(releaseFile => releaseFile.File)
                .Where(releaseFile => releaseFile.ReleaseVersionId == releaseVersionId
                                      && releaseFile.File.Type == FileType.Data
                                      && releaseFile.File.ReplacingId == null)
                .MaxAsync(releaseFile => (int?)releaseFile.Order);

            return currentMaxOrder.HasValue ? currentMaxOrder.Value + 1 : 0;
        }
        private async Task<ReleaseFile?> GetReplacedReleaseFile(Guid releaseVersionId, File replacingFile)
        {
            return await contentDbContext.ReleaseFiles
                .Include(rf => rf.File)
                .SingleAsync(rf =>
                    rf.ReleaseVersionId == releaseVersionId
                    && rf.File.Type == FileType.Data
                    && rf.File.Id == replacingFile.Id);
        }
    }
}
