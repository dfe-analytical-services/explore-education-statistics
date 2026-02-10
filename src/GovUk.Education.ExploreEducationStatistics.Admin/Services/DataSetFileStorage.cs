#nullable enable
using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using DataSet = GovUk.Education.ExploreEducationStatistics.Admin.Models.DataSet;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseVersionRepository;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

// TODO - EES-6480.
public class DataSetFileStorage(
    ContentDbContext contentDbContext,
    IPrivateBlobStorageService privateBlobStorageService,
    IReleaseVersionRepository releaseVersionRepository,
    IReleaseDataFileRepository releaseDataFileRepository,
    IDataSetUploadRepository dataSetUploadRepository,
    IDataImportService dataImportService,
    IUserService userService,
    IDataSetVersionService dataSetVersionService,
    IDataSetService dataSetService,
    ILogger<DataSetFileStorage> logger
) : IDataSetFileStorage
{
    private async Task CreateDraftDataSetVersion(
        Guid dataReleaseFileId,
        ReleaseFile replacedReleaseDataFile,
        CancellationToken cancellationToken
    )
    {
        var dataSetId = replacedReleaseDataFile.PublicApiDataSetId;

        await dataSetVersionService
            .GetDataSetVersion(dataSetId!.Value, replacedReleaseDataFile.PublicApiDataSetVersion!, cancellationToken)
            .OnFailureDo(_ =>
            {
                var errorMessage =
                    "Failed to find the data set version expected to be linked to the release file that is being replaced for the "
                    + $"data set id: {dataSetId} and the data set version number: {replacedReleaseDataFile.PublicApiDataSetVersionString}. This has occured when creating the next draft version.";
                logger.LogError(errorMessage);
                throw new InvalidOperationException(
                    "Failed to find the associated API data set version for the release file."
                );
            })
            .OnSuccessDo(async dataSetVersion =>
            {
                if (dataSetVersion.IsFirstVersion && dataSetVersion.Status != DataSetVersionStatus.Published)
                {
                    await DeleteAndRecreateInitialDataSetVersion(
                        dataReleaseFileId,
                        cancellationToken,
                        dataSetVersion,
                        dataSetId
                    );
                }
                else if (dataSetVersion.Status != DataSetVersionStatus.Published)
                {
                    await DeleteAndRecreateDataSetVersion(
                        dataReleaseFileId,
                        replacedReleaseDataFile,
                        cancellationToken,
                        dataSetVersion,
                        dataSetId
                    );
                }
                else
                {
                    await CreateNextDataSetVersion(
                        dataReleaseFileId,
                        replacedReleaseDataFile,
                        cancellationToken,
                        dataSetVersion,
                        dataSetId
                    );
                }
            });
    }

    private async Task CreateNextDataSetVersion(
        Guid dataReleaseFileId,
        ReleaseFile replacedReleaseDataFile,
        CancellationToken cancellationToken,
        DataSetVersion dataSetVersion,
        [DisallowNull] Guid? dataSetId
    )
    {
        await dataSetVersionService
            .CreateNextVersion(
                releaseFileId: dataReleaseFileId,
                dataSetId: replacedReleaseDataFile.PublicApiDataSetId!.Value,
                dataSetVersionToReplaceId: dataSetVersion.Id,
                cancellationToken
            )
            .OnFailureDo(_ =>
            {
                var errorMessage =
                    $"Failed whilst creating the next draft version for the data set id: {dataSetId} and the data set version number: {replacedReleaseDataFile.PublicApiDataSetVersionString}.";
                logger.LogError(errorMessage);
                throw new InvalidOperationException(
                    "Failure detected when creating the next draft version for the data file uploaded."
                );
            });
    }

    private async Task DeleteAndRecreateDataSetVersion(
        Guid dataReleaseFileId,
        ReleaseFile replacedReleaseDataFile,
        CancellationToken cancellationToken,
        DataSetVersion dataSetVersion,
        [DisallowNull] Guid? dataSetId
    )
    {
        await dataSetVersionService
            .DeleteVersion(dataSetVersion.Id, cancellationToken)
            .OnSuccessVoid(async _ =>
            {
                await dataSetVersionService
                    .CreateNextVersion(
                        releaseFileId: dataReleaseFileId,
                        dataSetId: replacedReleaseDataFile.PublicApiDataSetId!.Value,
                        dataSetVersionToReplaceId: null,
                        cancellationToken
                    )
                    .OnFailureDo(_ =>
                    {
                        var errorMessage =
                            $"Failed whilst creating the next draft version for the data set id: {dataSetId} and the data set version number: {replacedReleaseDataFile.PublicApiDataSetVersionString}.";
                        logger.LogError(errorMessage);
                        throw new InvalidOperationException(
                            "Failure detected when creating the next draft version for the data file uploaded."
                        );
                    });
            })
            .OnFailureVoid(_ =>
            {
                var errorMessage = $"Failed whilst deleting the draft version for the data set id: {dataSetId}.";
                logger.LogError(errorMessage);
                throw new InvalidOperationException(
                    "Failure detected when deleting the draft version for the data file uploaded."
                );
            });
    }

    private async Task DeleteAndRecreateInitialDataSetVersion(
        Guid dataReleaseFileId,
        CancellationToken cancellationToken,
        DataSetVersion dataSetVersion,
        [DisallowNull] Guid? dataSetId
    )
    {
        await dataSetVersionService
            .DeleteVersion(dataSetVersion.Id, cancellationToken)
            .OnSuccessVoid(async _ =>
            {
                await dataSetService
                    .CreateDataSet(dataReleaseFileId, cancellationToken)
                    .OnFailureDo(_ =>
                    {
                        var errorMessage =
                            $"Failed whilst creating the initial draft version for the data set id: {dataSetId}";
                        logger.LogError(errorMessage);
                        throw new InvalidOperationException("Failed whilst creating the initial draft version ");
                    });
            })
            .OnFailureVoid(_ =>
            {
                var errorMessage =
                    $"Failed whilst deleting the initial draft version for the data set id: {dataSetId}.";
                logger.LogError(errorMessage);
                throw new InvalidOperationException("Failed whilst deleting initial the draft version.");
            });
    }

    public async Task<List<DataSetUpload>> UploadDataSetsToTemporaryStorage(
        Guid releaseVersionId,
        List<DataSet> dataSets,
        CancellationToken cancellationToken
    )
    {
        var uploadTasks = dataSets.Select(dataSet =>
            UploadDataSetToTemporaryStorage(releaseVersionId, dataSet, cancellationToken)
        );

        var uploads = await Task.WhenAll(uploadTasks);

        return [.. uploads];
    }

    // TODO - EES-6480.
    public async Task<Either<ActionResult, BlobDownloadToken>> GetTemporaryFileDownloadToken(
        Guid releaseVersionId,
        Guid dataSetUploadId,
        FileType fileType,
        CancellationToken cancellationToken
    )
    {
        return await ValidateFileTypeForTemporaryDataSetUploadFile(fileType)
            .OnSuccess(async () =>
                await contentDbContext.DataSetUploads.SingleOrNotFoundAsync(
                    upload => upload.Id == dataSetUploadId,
                    cancellationToken: cancellationToken
                )
            )
            .OnSuccess(upload =>
            {
                var fileDetails = GetTemporaryDataUploadFileDetails(
                    releaseVersionId: releaseVersionId,
                    fileType: fileType,
                    upload: upload
                );

                return privateBlobStorageService.GetBlobDownloadToken(
                    container: PrivateReleaseTempFiles,
                    filename: fileDetails.FileName,
                    path: fileDetails.FilePath,
                    cancellationToken: cancellationToken
                );
            });
    }

    /// <summary>
    /// Upload the supplied data set files to temporary blob storage.
    /// </summary>
    /// <returns>An entity representing the uploaded files. The IDs are used to locate the files in virtual storage.</returns>
    private async Task<DataSetUpload> UploadDataSetToTemporaryStorage(
        Guid releaseVersionId,
        DataSet dataSet,
        CancellationToken cancellationToken
    )
    {
        var dataFileId = Guid.NewGuid();
        var metaFileId = Guid.NewGuid();
        var dataFilePath = $"{FileStoragePathUtils.FilesPath(releaseVersionId, FileType.Data)}{dataFileId}";
        var metaFilePath = $"{FileStoragePathUtils.FilesPath(releaseVersionId, FileType.Metadata)}{metaFileId}";

        await privateBlobStorageService.UploadStream(
            containerName: PrivateReleaseTempFiles,
            path: dataFilePath,
            sourceStream: dataSet.DataFile.FileStreamProvider(),
            contentType: ContentTypes.Csv,
            contentEncoding: ContentEncodings.Gzip,
            cancellationToken: cancellationToken
        );

        await privateBlobStorageService.UploadStream(
            containerName: PrivateReleaseTempFiles,
            path: metaFilePath,
            sourceStream: dataSet.MetaFile.FileStreamProvider(),
            contentType: ContentTypes.Csv,
            contentEncoding: ContentEncodings.Gzip,
            cancellationToken: cancellationToken
        );

        return new DataSetUpload
        {
            ReleaseVersionId = releaseVersionId,
            DataSetTitle = dataSet.Title,
            DataFileId = dataFileId,
            DataFileName = dataSet.DataFile.FileName,
            DataFileSizeInBytes = dataSet.DataFile.FileSize,
            MetaFileId = metaFileId,
            MetaFileName = dataSet.MetaFile.FileName,
            MetaFileSizeInBytes = dataSet.MetaFile.FileSize,
            Status = DataSetUploadStatus.SCREENING,
            UploadedBy = userService.GetProfileFromClaims().Email.ToLower(),
            ReplacingFileId = dataSet.ReplacingFile?.Id,
        };
    }

    public async Task<DataSetUpload> CreateOrReplaceExistingDataSetUpload(
        Guid releaseVersionId,
        DataSetUpload dataSetUpload,
        CancellationToken cancellationToken
    )
    {
        var existingUpload = await contentDbContext.DataSetUploads.SingleOrDefaultAsync(
            existingUpload =>
                existingUpload.ReleaseVersionId == releaseVersionId
                && (
                    (existingUpload.DataSetTitle == dataSetUpload.DataSetTitle)
                    || (
                        existingUpload.DataFileName == dataSetUpload.DataFileName
                        && existingUpload.MetaFileName == dataSetUpload.MetaFileName
                    )
                ),
            cancellationToken
        );

        if (existingUpload is not null)
        {
            await dataSetUploadRepository.Delete(releaseVersionId, existingUpload.Id, cancellationToken);
        }

        await contentDbContext.DataSetUploads.AddAsync(dataSetUpload, cancellationToken);
        await contentDbContext.SaveChangesAsync(cancellationToken);

        return dataSetUpload;
    }

    public async Task UpdateDataSetUpload(
        Guid dataSetUploadId,
        DataSetScreenerResponse? screenerResult,
        CancellationToken cancellationToken
    )
    {
        var upload = await contentDbContext.DataSetUploads.SingleAsync(
            upload => upload.Id == dataSetUploadId,
            cancellationToken
        );

        if (screenerResult is null)
        {
            upload.Status = DataSetUploadStatus.SCREENER_ERROR;
        }
        else
        {
            upload.ScreenerResult = screenerResult;

            var hasWarnings = screenerResult.TestResults.Any(test => test.Result == TestResult.WARNING);
            var hasFailures = screenerResult.TestResults.Any(test => test.Result == TestResult.FAIL);

            if (hasWarnings)
            {
                upload.Status = DataSetUploadStatus.PENDING_REVIEW;
            }
            else if (hasFailures)
            {
                upload.Status = DataSetUploadStatus.FAILED_SCREENING;
            }
            else
            {
                upload.Status = DataSetUploadStatus.PENDING_IMPORT;
            }
        }

        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<ReleaseFile>> MoveDataSetsToPermanentStorage(
        Guid releaseVersionId,
        List<DataSetUpload> dataSetUploads,
        CancellationToken cancellationToken
    )
    {
        var releaseFiles = new List<ReleaseFile>();

        foreach (var dataSetUpload in dataSetUploads)
        {
            var subjectId = await releaseVersionRepository.CreateStatisticsDbReleaseAndSubjectHierarchy(
                releaseVersionId
            );

            var replacingFile = dataSetUpload.ReplacingFileId is null
                ? null
                : await contentDbContext.Files.FirstAsync(
                    f => f.Id == dataSetUpload.ReplacingFileId,
                    cancellationToken
                );

            ReleaseFile? replacedReleaseDataFile = null;

            if (replacingFile is not null)
            {
                replacedReleaseDataFile = await GetReplacedReleaseFile(releaseVersionId, replacingFile.Id);
            }

            var releaseDataFileOrder = await GetNextDataFileOrder(releaseVersionId, replacedReleaseDataFile?.File.Id);

            var dataFile = await releaseDataFileRepository.Create(
                releaseVersionId,
                subjectId,
                dataSetUpload.DataFileName,
                contentLength: dataSetUpload.DataFileSizeInBytes,
                type: FileType.Data,
                apiCompatible: dataSetUpload.ScreenerResult?.PublicApiCompatible,
                createdById: userService.GetUserId(),
                name: dataSetUpload.DataSetTitle,
                replacingDataFile: replacingFile,
                order: releaseDataFileOrder
            );

            var sourceDataFilePath = FileExtensions.Path(releaseVersionId, FileType.Data, dataSetUpload.DataFileId);
            var destinationDataFilePath = FileExtensions.Path(releaseVersionId, FileType.Data, dataFile.Id); // Same path, but a new ID has been generated by the creation step above

            var dataReleaseFile = await contentDbContext
                .ReleaseFiles.Include(rf => rf.File.CreatedBy)
                .SingleAsync(
                    rf => rf.ReleaseVersionId == releaseVersionId && rf.FileId == dataFile.Id,
                    cancellationToken
                );

            releaseFiles.Add(dataReleaseFile);

            var metaFile = await releaseDataFileRepository.Create(
                releaseVersionId,
                subjectId,
                dataSetUpload.MetaFileName,
                dataSetUpload.MetaFileSizeInBytes,
                type: FileType.Metadata,
                createdById: userService.GetUserId(),
                apiCompatible: dataSetUpload.ScreenerResult?.PublicApiCompatible
            );

            var sourceMetaFilePath = FileExtensions.Path(releaseVersionId, FileType.Metadata, dataSetUpload.MetaFileId);
            var destinationMetaFilePath = FileExtensions.Path(releaseVersionId, FileType.Metadata, metaFile.Id); // Same path, but a new ID has been generated by the creation step above

            await privateBlobStorageService.MoveBlob(
                PrivateReleaseTempFiles,
                sourceDataFilePath,
                destinationDataFilePath,
                PrivateReleaseFiles
            );
            await privateBlobStorageService.MoveBlob(
                PrivateReleaseTempFiles,
                sourceMetaFilePath,
                destinationMetaFilePath,
                PrivateReleaseFiles
            );

            if (replacingFile is not null && replacedReleaseDataFile!.PublicApiDataSetId != null)
            {
                await CreateDraftDataSetVersion(dataReleaseFile.Id, replacedReleaseDataFile, cancellationToken);
            }

            await dataImportService.Import(subjectId, dataFile, metaFile);
        }

        return releaseFiles;
    }

    private async Task<int> GetNextDataFileOrder(Guid releaseVersionId, Guid? fileToBeReplaceId = null)
    {
        if (fileToBeReplaceId is not null)
        {
            var fileToBeReplacedReleaseFile = await contentDbContext
                .ReleaseFiles.Include(rf => rf.File)
                .SingleAsync(rf =>
                    rf.ReleaseVersionId == releaseVersionId
                    && rf.File.Type == FileType.Data
                    && rf.File.Id == fileToBeReplaceId
                );

            return fileToBeReplacedReleaseFile.Order;
        }

        var currentMaxOrder = await contentDbContext
            .ReleaseFiles.Include(releaseFile => releaseFile.File)
            .Where(releaseFile =>
                releaseFile.ReleaseVersionId == releaseVersionId
                && releaseFile.File.Type == FileType.Data
                && releaseFile.File.ReplacingId == null
            )
            .MaxAsync(releaseFile => (int?)releaseFile.Order);

        return currentMaxOrder.HasValue ? currentMaxOrder.Value + 1 : 0;
    }

    private async Task<ReleaseFile?> GetReplacedReleaseFile(Guid releaseVersionId, Guid fileToBeReplacedId)
    {
        return await contentDbContext
            .ReleaseFiles.Include(rf => rf.File)
            .SingleAsync(rf =>
                rf.ReleaseVersionId == releaseVersionId
                && rf.File.Type == FileType.Data
                && rf.File.Id == fileToBeReplacedId
            );
    }

    private static Either<ActionResult, Unit> ValidateFileTypeForTemporaryDataSetUploadFile(FileType fileType)
    {
        return fileType is FileType.Data or FileType.Metadata
            ? Unit.Instance
            : ValidationUtils.ValidationResult(
                new ErrorViewModel
                {
                    Message = $"Invalid {nameof(fileType)} value {fileType} for temporary data set upload file type",
                    Path = nameof(fileType),
                    Detail = new InvalidErrorDetail<FileType>(fileType),
                }
            );
    }

    private static (string FilePath, string FileName) GetTemporaryDataUploadFileDetails(
        Guid releaseVersionId,
        FileType fileType,
        DataSetUpload upload
    )
    {
        if (fileType == FileType.Data)
        {
            return (
                FilePath: $"{FileStoragePathUtils.FilesPath(releaseVersionId, FileType.Data)}{upload.DataFileId}",
                FileName: upload.DataFileName
            );
        }

        return (
            FilePath: $"{FileStoragePathUtils.FilesPath(releaseVersionId, FileType.Metadata)}{upload.MetaFileId}",
            FileName: upload.MetaFileName
        );
    }
}
