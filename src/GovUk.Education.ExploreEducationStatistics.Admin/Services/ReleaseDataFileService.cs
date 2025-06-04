#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
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
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseDataFileService(
        ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext> persistenceHelper,
        IPrivateBlobStorageService privateBlobStorageService,
        IDataSetValidator dataSetValidator,
        IFileRepository fileRepository,
        IReleaseFileRepository releaseFileRepository,
        IReleaseFileService releaseFileService,
        IDataImportService dataImportService,
        IUserService userService,
        IDataSetFileStorage dataSetFileStorage,
        IDataSetScreenerClient dataSetScreenerClient,
        IDataSetUploadRepository dataSetUploadRepository) : IReleaseDataFileService
    {
        public async Task<Either<ActionResult, Unit>> Delete(
            Guid releaseVersionId,
            Guid fileId,
            bool forceDelete = false)
        {
            return await Delete(releaseVersionId, [fileId], forceDelete);
        }

        public async Task<Either<ActionResult, Unit>> Delete(
            Guid releaseVersionId,
            IEnumerable<Guid> fileIds,
            bool forceDelete = false)
        {
            return await persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(async releaseVersion =>
                    await userService.CheckCanUpdateReleaseVersion(releaseVersion, ignoreCheck: forceDelete))
                .OnSuccess(async _ =>
                    await fileIds.Select(fileId => releaseFileService.CheckFileExists(
                        releaseVersionId,
                        fileId,
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

        public async Task<Either<ActionResult, Unit>> DeleteAll(
            Guid releaseVersionId,
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
                    rf => rf.ReleaseVersionId == releaseVersionId &&
                    rf.File.Type == FileType.Data &&
                    rf.FileId == fileId)
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
                })
                .OnSuccessCombineWith(async files
                    => await dataSetUploadRepository.ListAll(releaseVersionId))
                .OnSuccess(dataFilesAndUploads =>
                {
                    var (files, uploads) = dataFilesAndUploads;

                    var filesFromUploads = new List<DataFileInfo>();
                    foreach (var upload in uploads)
                    {
                        filesFromUploads.Add(new DataFileInfo
                        {
                            Id = upload.Id,
                            FileName = upload.DataFileName,
                            Name = upload.DataSetTitle,
                            Size = "0kb", // TODO: Map the file size from elsewhere
                            MetaFileName = upload.MetaFileName,
                        });
                    }

                    files.AddRange(filesFromUploads);

                    return files;
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
                            releaseFile.File.Type == FileType.Data &&
                            releaseFile.ReleaseVersionId == releaseVersionId &&
                            !releaseFile.File.ReplacingId.HasValue)
                        .ToDictionaryAsync(releaseFile => releaseFile.File.Id);

                    if (releaseFiles.Count != fileIds.Count)
                    {
                        return ValidationActionResult(IncorrectNumberOfFileIds);
                    }

                    fileIds.ForEach((fileId, order) =>
                    {
                        if (!releaseFiles.TryGetValue(fileId, out var matchingReleaseFile))
                        {
                            throw new ArgumentException($"fileId {fileId} not found in db as non-replacement related data file attached to the release version {releaseVersionId}");
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

        public async Task<Either<ActionResult, List<DataSetUploadViewModel>>> Upload(
            Guid releaseVersionId,
            IFormFile dataFormFile,
            IFormFile metaFormFile,
            string dataSetTitle,
            Guid? replacingFileId,
            CancellationToken cancellationToken)
        {
            return await persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(userService.CheckCanUpdateReleaseVersion)
                .OnSuccess(async _ =>
                {
                    return await persistenceHelper
                        .CheckOptionalEntityExists<File>(replacingFileId)
                        .OnSuccess(async replacingFile
                            => await ValidateDataSetCsvPair(releaseVersionId, dataFormFile, metaFormFile, dataSetTitle, replacingFile))
                        .OnSuccess(async dataSet
                            => await dataSetFileStorage.UploadDataSetsToTemporaryStorage(releaseVersionId, [dataSet], cancellationToken))
                        .OnSuccess(async dataSetUploads
                            => await ScreenDataSetUploads(dataSetUploads, cancellationToken));
                });
        }

        // TODO (EES-6176): Remove once manual replacement process has been consolidated to use Upload
        public async Task<Either<ActionResult, DataFileInfo>> UploadForReplacement(
            Guid releaseVersionId,
            IFormFile dataFormFile,
            IFormFile metaFormFile,
            string dataSetTitle,
            Guid? replacingFileId,
            CancellationToken cancellationToken)
        {
            return await persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(userService.CheckCanUpdateReleaseVersion)
                .OnSuccess(async _ =>
                {
                    return await persistenceHelper
                        .CheckOptionalEntityExists<File>(replacingFileId)
                        .OnSuccess(async replacingFile
                            => await ValidateDataSetCsvPair(releaseVersionId, dataFormFile, metaFormFile, dataSetTitle, replacingFile, performAutoReplacement: true))
                        .OnSuccess(async dataSet
                            => await dataSetFileStorage.UploadDataSet(releaseVersionId, dataSet, cancellationToken));
                });
        }

        public async Task<Either<ActionResult, List<DataSetUploadViewModel>>> UploadFromZip(
            Guid releaseVersionId,
            IFormFile zipFormFile,
            string dataSetTitle,
            Guid? replacingFileId,
            CancellationToken cancellationToken)
        {
            return await persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(userService.CheckCanUpdateReleaseVersion)
                .OnSuccess(async _ =>
                {
                    return await persistenceHelper
                        .CheckOptionalEntityExists<File>(replacingFileId)
                        .OnSuccess(async replacingFile
                            => await ValidateDataSetZip(releaseVersionId, zipFormFile, dataSetTitle, replacingFile))
                        .OnSuccess(async dataSet
                            => await dataSetFileStorage.UploadDataSetsToTemporaryStorage(releaseVersionId, [dataSet], cancellationToken))
                        .OnSuccess(async dataSetUploads
                            => await ScreenDataSetUploads(dataSetUploads, cancellationToken));
                });
        }

        // TODO (EES-6176): Remove once manual replacement process has been consolidated to use UploadFromZip
        public async Task<Either<ActionResult, DataFileInfo>> UploadFromZipForReplacement(
            Guid releaseVersionId,
            IFormFile zipFormFile,
            string dataSetTitle,
            Guid? replacingFileId,
            CancellationToken cancellationToken)
        {
            return await persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(userService.CheckCanUpdateReleaseVersion)
                .OnSuccess(async _ =>
                {
                    return await persistenceHelper
                        .CheckOptionalEntityExists<File>(replacingFileId)
                        .OnSuccess(async replacingFile
                            => await ValidateDataSetZip(releaseVersionId, zipFormFile, dataSetTitle, replacingFile, performAutoReplacement: true))
                        .OnSuccess(async dataSet
                            => await dataSetFileStorage.UploadDataSet(releaseVersionId, dataSet, cancellationToken));
                });
        }

        public async Task<Either<ActionResult, List<DataSetUploadViewModel>>> UploadFromBulkZip(
            Guid releaseVersionId,
            IFormFile zipFormFile,
            CancellationToken cancellationToken)
        {
            return await persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(userService.CheckCanUpdateReleaseVersion)
                .OnSuccess(async _
                    => await ValidateBulkDataSetZip(releaseVersionId, zipFormFile))
                .OnSuccess(async dataSets
                    => await dataSetFileStorage.UploadDataSetsToTemporaryStorage(releaseVersionId, dataSets, cancellationToken))
                .OnSuccess(async dataSetUploads
                    => await ScreenDataSetUploads(dataSetUploads, cancellationToken));
        }

        private async Task<List<DataSetUploadViewModel>> ScreenDataSetUploads(
            List<DataSetUpload> dataSetUploads,
            CancellationToken cancellationToken)
        {
            var tasks = dataSetUploads.Select(async dataSetUpload =>
            {
                var request = BuildScreenerRequest(dataSetUpload);
                var result = await dataSetScreenerClient.ScreenDataSet(request, cancellationToken);

                await dataSetFileStorage.AddScreenerResultToUpload(dataSetUpload.Id, result, cancellationToken);

                return BuildUploadViewModel(dataSetUpload, result);
            });

            return [.. await Task.WhenAll(tasks)];
        }

        private static DataSetUploadViewModel BuildUploadViewModel(
            DataSetUpload dataSetUpload,
            DataSetScreenerResult screenerResult)
        {
            return new DataSetUploadViewModel
            {
                Id = dataSetUpload.Id,
                DataFileName = dataSetUpload.DataFileName,
                DataSetTitle = dataSetUpload.DataSetTitle,
                MetaFileName = dataSetUpload.MetaFileName,
                Status = GetDataSetUploadStatus(screenerResult),
                ScreenerResult = screenerResult,
            };
        }

        private static DataSetUploadStatus GetDataSetUploadStatus(DataSetScreenerResult screenerResult)
        {
            return screenerResult.Result switch
            {
                ScreenerResult.Passed => screenerResult.TestResults.Any(test => test.TestResult == TestResult.WARNING)
                    ? DataSetUploadStatus.CompletePendingReview
                    : DataSetUploadStatus.CompletePendingImport,
                ScreenerResult.Failed => DataSetUploadStatus.Failed,
                _ => throw new ArgumentOutOfRangeException(nameof(screenerResult), screenerResult, null),
            };
        }

        private static Requests.DataSetScreenerRequest BuildScreenerRequest(DataSetUpload dataSet)
        {
            return new()
            {
                // TODO: Update screener API to accept a container name rather than hard-coded
                StorageContainerName = "releases-temp",
                DataFileName = dataSet.DataFileName,
                DataFilePath = dataSet.DataFilePath,
                MetaFileName = dataSet.MetaFileName,
                MetaFilePath = dataSet.MetaFilePath,
            };
        }

        private async Task<Either<ActionResult, DataSet>> ValidateDataSetCsvPair(
            Guid releaseVersionId,
            IFormFile dataFormFile,
            IFormFile metaFormFile,
            string dataSetTitle,
            File? replacingFile,
            bool performAutoReplacement = false)
        {
            var dataFile = await BuildDataSetFile(dataFormFile);
            var metaFile = await BuildDataSetFile(metaFormFile);
            return await ValidateAndBuildDataSet(releaseVersionId, dataFile, metaFile, dataSetTitle, replacingFile, performAutoReplacement);
        }

        private async Task<Either<ActionResult, DataSet>> ValidateDataSetZip(
            Guid releaseVersionId,
            IFormFile zipFormFile,
            string dataSetTitle,
            File? replacingFile,
            bool performAutoReplacement = false)
        {
            var dataSetFiles = await ExtractDataSetZipFile(zipFormFile);

            var dataFile = dataSetFiles.FirstOrDefault(file => !file.FileName.EndsWith(".meta.csv"));
            var metaFile = dataSetFiles.FirstOrDefault(file => file.FileName.EndsWith(".meta.csv"));

            return await ValidateAndBuildDataSet(releaseVersionId, dataFile, metaFile, dataSetTitle, replacingFile, performAutoReplacement);
        }

        private async Task<Either<ActionResult, DataSet>> ValidateAndBuildDataSet(
            Guid releaseVersionId,
            FileDto dataFile,
            FileDto metaFile,
            string dataSetTitle,
            File? replacingFile,
            bool performAutoReplacement = false)
        {
            var newDataSetTitle = await GetReleaseVersionDataSetTitle(releaseVersionId, dataSetTitle, replacingFile);

            var dataSet = new DataSetDto
            {
                ReleaseVersionId = releaseVersionId,
                Title = newDataSetTitle,
                DataFile = dataFile,
                MetaFile = metaFile,
                ReplacingFile = replacingFile,
            };

            var validationResult = await dataSetValidator.ValidateDataSet(dataSet, performAutoReplacement);

            return validationResult.IsLeft
                ? Common.Validators.ValidationUtils.ValidationResult(validationResult.Left)
                : validationResult.Right;
        }

        private async Task<Either<ActionResult, List<DataSet>>> ValidateBulkDataSetZip(
            Guid releaseVersionId,
            IFormFile zipFormFile)
        {
            var dataSetFiles = await ExtractDataSetZipFile(zipFormFile);

            var indexFile = dataSetFiles.FirstOrDefault(dsf => dsf.FileName == "dataset_names.csv");
            if (indexFile is null)
            {
                return Common.Validators.ValidationUtils.ValidationResult(ValidationMessages.GenerateErrorBulkDataZipMustContainDataSetNamesCsv());
            }

            dataSetFiles.Remove(indexFile);

            var dataSetDtos = new List<DataSetDto>();
            var errors = new List<ErrorViewModel>();

            await dataSetValidator.ValidateBulkDataZipIndexFile(releaseVersionId, indexFile, dataSetFiles)
                .OnSuccessDo(dataSetIndex => dataSetDtos.AddRange(BuildDataSetsFromBulkZip(dataSetIndex, dataSetFiles)))
                .OnFailureDo(errors.AddRange);

            if (errors.Count > 0)
            {
                return Common.Validators.ValidationUtils.ValidationResult(errors);
            }

            var dataSets = new List<DataSet>();
            foreach (var dataSetDto in dataSetDtos)
            {
                await dataSetValidator.ValidateDataSet(dataSetDto, performAutoReplacement: true)
                    .OnSuccessDo(dataSets.Add)
                    .OnFailureDo(errors.AddRange);
            }

            return errors.Count > 0
                ? Common.Validators.ValidationUtils.ValidationResult(errors)
                : dataSets;
        }

        private static List<DataSetDto> BuildDataSetsFromBulkZip(
            DataSetIndex dataSetIndex,
            List<FileDto> dataSetFiles)
        {
            var dataSets = new List<DataSetDto>();
            foreach (var dataSet in dataSetIndex.DataSetIndexItems)
            {
                dataSets.Add(new()
                {
                    ReleaseVersionId = dataSetIndex.ReleaseVersionId,
                    Title = dataSet.DataSetTitle,
                    DataFile = dataSetFiles.FirstOrDefault(file => file.FileName == dataSet.DataFileName),
                    MetaFile = dataSetFiles.FirstOrDefault(file => file.FileName == dataSet.MetaFileName),
                    ReplacingFile = dataSet.ReplacingFile,
                });
            }

            return dataSets;
        }

        private static async Task<List<FileDto>> ExtractDataSetZipFile(IFormFile zipFile)
        {
            await using var stream = zipFile.OpenReadStream();
            using var archive = new ZipArchive(stream);

            var files = new List<FileDto>();

            foreach (var entry in archive.Entries)
            {
                using var fileStream = entry.Open();
                files.Add(await BuildDataSetFile(fileStream, entry.Name));
            }

            return files;
        }

        private static async Task<FileDto> BuildDataSetFile(IFormFile formFile)
        {
            using var fileStream = formFile.OpenReadStream();
            return await BuildDataSetFile(fileStream, formFile.FileName);
        }

        private static async Task<FileDto> BuildDataSetFile(
            System.IO.Stream fileStream,
            string fileName)
        {
            var memoryStream = new System.IO.MemoryStream();
            await fileStream.CopyToAsync(memoryStream);

            memoryStream.SeekToBeginning();

            return new FileDto
            {
                FileName = fileName,
                FileSize = memoryStream.Length,
                FileStream = memoryStream,
            };
        }

        public async Task<Either<ActionResult, List<DataFileInfo>>> SaveDataSetsFromTemporaryBlobStorage(
            Guid releaseVersionId,
            List<Guid> dataSetUploadIds,
            CancellationToken cancellationToken)
        {
            return await persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(userService.CheckCanUpdateReleaseVersion)
                .OnSuccess(() => dataSetUploadIds
                    .Select(dataSetUploadId => ValidateTempDataSetFileExistence(releaseVersionId, dataSetUploadId, cancellationToken))
                    .OnSuccessAll())
                .OnSuccess(async dataSetUploads =>
                {
                    var releaseFiles = await dataSetFileStorage.MoveDataSetsToPermanentStorage(releaseVersionId, dataSetUploads, cancellationToken);
                    return await BuildDataFileViewModels(releaseFiles);
                });
        }

        private async Task<Either<ActionResult, DataSetUpload>> ValidateTempDataSetFileExistence(
            Guid releaseVersionId,
            Guid dataSetUploadId,
            CancellationToken cancellationToken)
        {
            // TODO: Replace both exceptions with proper error response
            var dataSetUpload = await contentDbContext.DataSetUploads.FirstOrDefaultAsync(upload =>
                dataSetUploadId == upload.Id &&
                upload.ReleaseVersionId == releaseVersionId,
                cancellationToken)
                    ?? throw new Exception("Data set upload not found");

            var dataBlobExists = await privateBlobStorageService.CheckBlobExists(PrivateReleaseTempFiles, dataSetUpload.DataFilePath);
            var metaBlobExists = await privateBlobStorageService.CheckBlobExists(PrivateReleaseTempFiles, dataSetUpload.MetaFilePath);

            return !dataBlobExists || !metaBlobExists
                ? throw new Exception("Unable to locate temporary files at the locations specified")
                : dataSetUpload;
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

            return new DataFileInfo
            {
                Id = releaseFile.FileId,
                FileName = releaseFile.File.Filename,
                Name = releaseFile.Name ?? "Unknown",
                Size = releaseFile.File.DisplaySize(),
                MetaFileId = dataImport.MetaFile.Id,
                MetaFileName = dataImport.MetaFile.Filename,
                ReplacedBy = releaseFile.File.ReplacedById,
                Rows = dataImport.TotalRows,
                UserName = releaseFile.File.CreatedBy?.Email ?? "",
                Status = dataImport.Status,
                Created = releaseFile.File.Created,
                Permissions = permissions,
                PublicApiDataSetId = releaseFile.PublicApiDataSetId,
                PublicApiDataSetVersion = releaseFile.PublicApiDataSetVersionString,
            };
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
                .ToDictionaryAwaitAsync(
                    rf => ValueTask.FromResult(rf.FileId),
                    async rf => await userService.GetDataFilePermissions(rf.File));

            return releaseFiles.Select(releaseFile =>
            {
                var dataImport = dataImports[releaseFile.FileId];

                return new DataFileInfo
                {
                    Id = releaseFile.FileId,
                    FileName = releaseFile.File.Filename,
                    Name = subjectNames[releaseFile.FileId] ?? "",
                    Size = releaseFile.File.DisplaySize(),
                    MetaFileId = dataImport.MetaFile.Id,
                    MetaFileName = dataImport.MetaFile.Filename,
                    ReplacedBy = releaseFile.File.ReplacedById,
                    Rows = dataImport.TotalRows,
                    UserName = releaseFile.File.CreatedBy?.Email ?? "",
                    Status = dataImport.Status,
                    Created = releaseFile.File.Created,
                    Permissions = permissions[releaseFile.FileId],
                    PublicApiDataSetId = releaseFile.PublicApiDataSetId,
                    PublicApiDataSetVersion = releaseFile.PublicApiDataSetVersionString,
                };
            }).ToList();
        }

        /// <summary>
        /// Determine the title which should be used for a data set.
        /// </summary>
        /// <remarks>
        /// Data replacements use the title of the original data set upload, regardless of whether a new one has been provided during a subsequent upload.
        /// </remarks>
        private async Task<string> GetReleaseVersionDataSetTitle(
            Guid releaseVersionId,
            string dataSetTitle,
            File? replacingFile)
        {
            if (replacingFile is not null)
            {
                var originalDataSetTitle = await releaseFileRepository.Find(releaseVersionId, replacingFile.Id);
                dataSetTitle = originalDataSetTitle?.Name ?? dataSetTitle;
            }

            return dataSetTitle;
        }

        private async Task<File> GetAssociatedMetaFile(
            Guid releaseVersionId,
            File dataFile)
        {
            return await contentDbContext.ReleaseFiles
                .Include(rf => rf.File)
                .Where(
                    rf => rf.ReleaseVersionId == releaseVersionId &&
                    rf.File.Type == FileType.Metadata &&
                    rf.File.SubjectId == dataFile.SubjectId)
                .Select(rf => rf.File)
                .SingleAsync();
        }
    }
}
