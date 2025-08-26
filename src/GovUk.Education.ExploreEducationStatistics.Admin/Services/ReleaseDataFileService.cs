#nullable enable
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
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
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;
using ValidationUtils = GovUk.Education.ExploreEducationStatistics.Common.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

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
    IDataBlockService dataBlockService,
    IFootnoteRepository footnoteRepository,
    IDataSetScreenerClient dataSetScreenerClient,
    IReplacementPlanService replacementPlanService,
    IMapper mapper) : IReleaseDataFileService
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
            .CheckEntityExists<ReleaseFile>(q => q
                .Include(rf => rf.ReleaseVersion)
                .Include(rf => rf.File.CreatedBy)
                .Where(rf =>
                    rf.ReleaseVersionId == releaseVersionId &&
                rf.File.Type == FileType.Data &&
                    rf.FileId == fileId))
            .OnSuccessDo(rf => userService.CheckCanViewReleaseVersion(rf.ReleaseVersion))
            .OnSuccess(async releaseFile =>
            {
                var dataImport = await contentDbContext.DataImports
                    .AsSplitQuery()
                    .Include(di => di.File)
                    .Include(di => di.MetaFile)
                    .SingleAsync(di => di.FileId == releaseFile.FileId);

                var permissions = await userService.GetDataFilePermissions(dataImport.File);

                return new DataFileInfo(releaseFile, dataImport, permissions);
            });
    }

    public async Task<Either<ActionResult, DataSetAccoutrementsViewModel>> GetAccoutrementsSummary(
        Guid releaseVersionId,
        Guid fileId)
    {
        return await persistenceHelper
            .CheckEntityExists<ReleaseFile>(q => q
                .Include(releaseFile => releaseFile.File)
                .Include(releaseFile => releaseFile.ReleaseVersion)
                .Where(releaseFile =>
                    releaseFile.ReleaseVersionId == releaseVersionId
                    && releaseFile.FileId == fileId
                    && releaseFile.File.Type == FileType.Data))
            .OnSuccessDo(releaseFile => userService.CheckCanViewReleaseVersion(releaseFile.ReleaseVersion))
            .OnSuccess(async releaseFile =>
            {
                var dataBlocks = (await dataBlockService.ListDataBlocks(
                    releaseFile.ReleaseVersionId))
                    .Where(dataBlock => dataBlock.Query.SubjectId == releaseFile.File.SubjectId)
                    .ToList();

                var footnotes = await footnoteRepository.GetFootnotes(
                    releaseVersionId: releaseFile.ReleaseVersionId,
                    subjectId: releaseFile.File.SubjectId);

                return new DataSetAccoutrementsViewModel
                {
                    DataBlocks = dataBlocks.Select(db => new DataBlockAccoutrementViewModel
                    {
                        Id = db.Id,
                        Name = db.Name,
                    }).ToList(),
                    Footnotes = footnotes.Select(fn => new FootnoteAccoutrementViewModel
                    {
                        Id = fn.Id,
                        Content = fn.Content,
                    }).ToList(),
                };
            });
    }

    public async Task<Either<ActionResult, List<DataFileInfo>>> ListAll(Guid releaseVersionId)
    {
        return await persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(userService.CheckCanViewReleaseVersion)
            .OnSuccess(async () =>
            {
                var releaseFiles = await releaseFileRepository.GetByFileType(
                    releaseVersionId: releaseVersionId,
                    types: FileType.Data);
                var releaseFilesByFileId = releaseFiles
                    .ToDictionary(rf => rf.File.Id);

                // An in-progress data set replacement has two release files associated with it: the original and
                // the replacement. The frontend doesn't want to display two files for the in-progress replacement,
                // so we exclude the replacement file and keep the original file.
                var filesExcludingReplacements = releaseFiles
                    .Where(releaseFile => !releaseFile.File.ReplacingId.HasValue)
                    .OrderBy(releaseFile => releaseFile.Order)
                    .ThenBy(releaseFile => releaseFile.Name) // For subjects existing before ordering was added
                    .ToList();

                // But we still want the in-progress replacement files for generating view models
                // Get the replacing file in-progress if they're in this release version.
                // ReplacedById can be set if a replacement is in progress on a newer release version. Ignore those replacements here.
                var inProgressReplacementsInCurrentReleaseVersion = releaseFiles
                    .Where(rf => rf.File.ReplacedById.HasValue)
                    .Select(rf =>
                        releaseFilesByFileId.TryGetValue(rf.File.ReplacedById!.Value,
                            out var replacementReleaseFile)
                            ? replacementReleaseFile
                            : null
                    )
                    .WhereNotNull()
                    .ToList();

                return await BuildDataFileViewModels(filesExcludingReplacements, inProgressReplacementsInCurrentReleaseVersion);
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
        IManagedStreamFile dataFile,
        IManagedStreamFile metaFile,
        string dataSetTitle,
        CancellationToken cancellationToken)
    {
        return await persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async _ =>
            {
                return await ValidateDataSetCsvPair(
                    releaseVersionId,
                    dataFile,
                    metaFile,
                    dataSetTitle)
                    .OnSuccess(async dataSet
                        => await dataSetFileStorage.UploadDataSetsToTemporaryStorage(releaseVersionId, [dataSet], cancellationToken))
                    .OnSuccess(dataSetUploads
                        => dataSetUploads.SelectAsync(dataSetUpload
                            => dataSetFileStorage.CreateOrReplaceExistingDataSetUpload(releaseVersionId, dataSetUpload, cancellationToken)))
                    .OnSuccess(async dataSetUploads
                        => await ScreenDataSetUploads([.. dataSetUploads], cancellationToken));
            });
    }

    public async Task<Either<ActionResult, List<DataSetUploadViewModel>>> UploadFromZip(
        Guid releaseVersionId,
        IManagedStreamZipFile zipFile,
        string dataSetTitle,
        CancellationToken cancellationToken)
    {
        return await persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async _ =>
            {
                return await ValidateDataSetZip(
                    releaseVersionId,
                    zipFile,
                    dataSetTitle)
                    .OnSuccess(async dataSet
                        => await dataSetFileStorage.UploadDataSetsToTemporaryStorage(releaseVersionId, [dataSet], cancellationToken))
                    .OnSuccess(dataSetUploads
                        => dataSetUploads.SelectAsync(dataSetUpload
                            => dataSetFileStorage.CreateOrReplaceExistingDataSetUpload(releaseVersionId, dataSetUpload, cancellationToken)))
                    .OnSuccess(async dataSetUploads
                        => await ScreenDataSetUploads([.. dataSetUploads], cancellationToken));
            });
    }

    public async Task<Either<ActionResult, List<DataSetUploadViewModel>>> UploadFromBulkZip(
        Guid releaseVersionId,
        IManagedStreamZipFile zipFile,
        CancellationToken cancellationToken)
    {
        return await persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async _
                => await ValidateBulkDataSetZip(
                    releaseVersionId,
                    zipFile))
            .OnSuccess(async dataSets
                => await dataSetFileStorage.UploadDataSetsToTemporaryStorage(
                    releaseVersionId,
                    dataSets,
                    cancellationToken))
            .OnSuccess(dataSetUploads
                => dataSetUploads.SelectAsync(dataSetUpload
                    => dataSetFileStorage.CreateOrReplaceExistingDataSetUpload(releaseVersionId, dataSetUpload, cancellationToken)))
            .OnSuccess(async dataSetUploads
                => await ScreenDataSetUploads([.. dataSetUploads], cancellationToken));
    }

    private async Task<List<DataSetUploadViewModel>> ScreenDataSetUploads(
        List<DataSetUpload> dataSetUploads,
        CancellationToken cancellationToken)
    {
        return await dataSetUploads
            .ToAsyncEnumerable()
            .SelectAwait(async dataSetUpload =>
            {
                var request = mapper.Map<DataSetScreenerRequest>(dataSetUpload);
                var result = await dataSetScreenerClient.ScreenDataSet(request, cancellationToken);

                await dataSetFileStorage.AddScreenerResultToUpload(dataSetUpload.Id, result, cancellationToken);

                // TODO (EES-6334): Basic auto-import added as an initial step. Once the screener has been re-enabled,
                // this will later be refined to prevent auto-import when there are failures or warnings.
                await SaveDataSetsFromTemporaryBlobStorage(
                    dataSetUpload.ReleaseVersionId,
                    [dataSetUpload.Id],
                    cancellationToken);

                return mapper.Map<DataSetUploadViewModel>(dataSetUpload);
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<Either<ActionResult, DataSet>> ValidateDataSetCsvPair(
        Guid releaseVersionId,
        IManagedStreamFile dataFile,
        IManagedStreamFile metaFile,
        string dataSetTitle)
    {
        return await ValidateAndBuildDataSet(
            releaseVersionId,
            BuildDataSetFile(dataFile),
            BuildDataSetFile(metaFile),
            dataSetTitle);
    }

    private async Task<Either<ActionResult, DataSet>> ValidateDataSetZip(
        Guid releaseVersionId,
        IManagedStreamZipFile zipFile,
        string dataSetTitle)
    {
        var dataSetFiles = ExtractDataSetZipFile(zipFile);

        var dataFile = dataSetFiles.FirstOrDefault(file => !file.FileName.EndsWith(".meta.csv"));
        var metaFile = dataSetFiles.FirstOrDefault(file => file.FileName.EndsWith(".meta.csv"));

        return await ValidateAndBuildDataSet(releaseVersionId, dataFile, metaFile, dataSetTitle);
    }

    private async Task<Either<ActionResult, DataSet>> ValidateAndBuildDataSet(
        Guid releaseVersionId,
        FileDto dataFile,
        FileDto metaFile,
        string dataSetTitle)
    {
        var dataSet = new DataSetDto
        {
            ReleaseVersionId = releaseVersionId,
            Title = dataSetTitle,
            DataFile = dataFile,
            MetaFile = metaFile,
        };

        return (await dataSetValidator.ValidateDataSet(dataSet))
            .OnFailure<ActionResult>(errors => ValidationUtils.ValidationResult(errors))
            .OnSuccess(ds => ds);
    }

    private async Task<Either<ActionResult, List<DataSet>>> ValidateBulkDataSetZip(
        Guid releaseVersionId,
        IManagedStreamZipFile zipFile)
    {
        var dataSetFiles = ExtractDataSetZipFile(zipFile);

        var indexFile = dataSetFiles.FirstOrDefault(dsf => dsf.FileName == "dataset_names.csv");
        if (indexFile is null)
        {
            return ValidationUtils.ValidationResult(ValidationMessages
                .GenerateErrorBulkDataZipMustContainDataSetNamesCsv());
        }

        dataSetFiles.Remove(indexFile);

        var dataSetDtos = new List<DataSetDto>();
        var errors = new List<ErrorViewModel>();

        await dataSetValidator.ValidateBulkDataZipIndexFile(releaseVersionId, indexFile, dataSetFiles)
            .OnSuccessDo(dataSetIndex => dataSetDtos.AddRange(BuildDataSetsFromBulkZip(dataSetIndex, dataSetFiles)))
            .OnFailureDo(errors.AddRange);

        if (errors.Count > 0)
        {
            return ValidationUtils.ValidationResult(errors);
        }

        var dataSets = new List<DataSet>();
        foreach (var dataSetDto in dataSetDtos)
        {
            await dataSetValidator.ValidateDataSet(dataSetDto)
                .OnSuccessDo(dataSets.Add)
                .OnFailureDo(errors.AddRange);
        }

        return errors.Count > 0
            ? ValidationUtils.ValidationResult(errors)
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
            });
        }

        return dataSets;
    }

    private static List<FileDto> ExtractDataSetZipFile(IManagedStreamZipFile zipFile)
    {
        return zipFile
            .GetEntries()
            .Select(BuildDataSetFile)
            .ToList();
    }

    private static FileDto BuildDataSetFile(IManagedStreamFile file)
    {
        return BuildDataSetFile(
            file.GetStream,
            file.Name,
            file.Length);
    }

    private static FileDto BuildDataSetFile(
        Func<Stream> fileStreamProvider,
        string fileName,
        long fileSize)
    {
        return new FileDto
        {
            FileName = fileName,
            FileSize = fileSize,
            FileStreamProvider = fileStreamProvider,
        };
    }

    public async Task<Either<ActionResult, Unit>> SaveDataSetsFromTemporaryBlobStorage(
        Guid releaseVersionId,
        List<Guid> dataSetUploadIds,
        CancellationToken cancellationToken)
    {
        return await persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(_ => dataSetUploadIds
                .Select(dataSetUploadId => ValidateDataSetUploadExistence(releaseVersionId, dataSetUploadId, cancellationToken))
                .OnSuccessAll())
            .OnSuccess(dataSetUploads => dataSetUploads
                .Select(ValidateDataSetCanBeImported)
                .OnSuccessAll())
            .OnSuccessDo(async dataSetUploads =>
            {
                await dataSetFileStorage.MoveDataSetsToPermanentStorage(releaseVersionId, dataSetUploads, cancellationToken);

                // Upload records are no longer needed once the files have been queued for import
                contentDbContext.DataSetUploads.RemoveRange(dataSetUploads);
                await contentDbContext.SaveChangesAsync(cancellationToken);
            })
            .OnSuccessVoid();
    }

    private async Task<Either<ActionResult, DataSetUpload>> ValidateDataSetUploadExistence(
        Guid releaseVersionId,
        Guid dataSetUploadId,
        CancellationToken cancellationToken)
    {
        var dataSetUpload = await contentDbContext.DataSetUploads.FirstOrDefaultAsync(upload =>
            dataSetUploadId == upload.Id &&
            upload.ReleaseVersionId == releaseVersionId,
            cancellationToken);

        if (dataSetUpload is null)
        {
            return ValidationUtils.ValidationResult(ValidationMessages.GenerateErrorDataSetUploadNotFound());
        }

        var dataBlobExists = await privateBlobStorageService.CheckBlobExists(PrivateReleaseTempFiles, dataSetUpload.DataFilePath);
        var metaBlobExists = await privateBlobStorageService.CheckBlobExists(PrivateReleaseTempFiles, dataSetUpload.MetaFilePath);

        return !dataBlobExists || !metaBlobExists
            ? ValidationUtils.ValidationResult(ValidationMessages.GenerateErrorTemporaryFilesNotFound())
            : dataSetUpload;
    }

    private static Either<ActionResult, DataSetUpload> ValidateDataSetCanBeImported(DataSetUpload dataSetUpload)
    {
        return dataSetUpload.Status is not DataSetUploadStatus.PENDING_REVIEW and not DataSetUploadStatus.PENDING_IMPORT
            ? ValidationUtils.ValidationResult(ValidationMessages.GenerateErrorDataSetIsNotInAnImportableState())
            : dataSetUpload;
    }

    private async Task<List<DataFileInfo>> BuildDataFileViewModels(
        List<ReleaseFile> releaseFiles,
        List<ReleaseFile> inProgressReplacementsInCurrentReleaseVersion)
    {
        var files = releaseFiles
            .Concat(inProgressReplacementsInCurrentReleaseVersion)
            .Select(rf => rf.File)
            .ToList();

        var dataImportsDict = await contentDbContext.DataImports
            .AsSplitQuery()
            .Include(di => di.File.CreatedBy)
            .Include(di => di.MetaFile)
            .Where(di => files
                .Select(f => f.Id)
                .Contains(di.FileId))
            .ToDictionaryAsync(di => di.FileId);

        // TODO Optimise GetDataFilePermissions here instead of potentially making several db queries
        // Work out if the user has permission to cancel any import which Bau users can.
        // Combine it with the import status (already known) to evaluate whether a particular import can be cancelled
        var permissionsDict = await files
            .ToAsyncEnumerable()
            .ToDictionaryAwaitAsync(
                file => ValueTask.FromResult(file.Id),
                async file => await userService.GetDataFilePermissions(file));

        var result = await releaseFiles.SelectAsync(async releaseFile =>
        {
            var fileHasReplacementInCurrentReleaseVersion = releaseFile.File.ReplacedById.HasValue &&
                                                     inProgressReplacementsInCurrentReleaseVersion
                                                     .Any(rf => rf.FileId == releaseFile.File.ReplacedById);
            if (!fileHasReplacementInCurrentReleaseVersion)
            {
                return new DataFileInfo(
                    releaseFile,
                    dataImportsDict[releaseFile.FileId],
                    permissionsDict[releaseFile.FileId]);
            }
            var replacement = inProgressReplacementsInCurrentReleaseVersion.Single(rf =>
                rf.FileId == releaseFile.File.ReplacedById);

            var hasValidReplacementPlan = false;
            if (dataImportsDict[replacement.FileId].Status == DataImportStatus.COMPLETE)
            {
                hasValidReplacementPlan = await replacementPlanService.HasValidReplacementPlan(
                    releaseFile, replacement);
            }

            return new DataFileInfo(
                releaseFile,
                dataImportsDict[releaseFile.FileId],
                permissionsDict[releaseFile.FileId])
            {
                ReplacedByDataFile = new ReplacementDataFileInfo(
                    replacement,
                    dataImportsDict[replacement.FileId],
                    permissionsDict[replacement.FileId],
                    hasValidReplacementPlan),
            };
        });

        return result.ToList();
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
