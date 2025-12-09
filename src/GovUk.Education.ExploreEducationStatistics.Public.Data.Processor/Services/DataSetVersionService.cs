using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Options;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Semver;
using Release = GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Release;
using ValidationMessages = GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests.Validators.ValidationMessages;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services;

internal class DataSetVersionService(
    ContentDbContext contentDbContext,
    PublicDataDbContext publicDataDbContext,
    IReleaseFileRepository releaseFileRepository,
    IDataSetVersionPathResolver dataSetVersionPathResolver,
    IOptions<AppOptions> options
) : IDataSetVersionService
{
    public async Task<Either<ActionResult, Guid>> CreateInitialVersion(
        Guid dataSetId,
        Guid releaseFileId,
        Guid instanceId,
        CancellationToken cancellationToken = default
    )
    {
        return await GetDataSet(dataSetId, cancellationToken)
            .OnSuccess(ValidateInitialDataSet)
            .OnSuccess(dataSet =>
                CreateDataSetVersion(
                    releaseFileId: releaseFileId,
                    instanceId: instanceId,
                    dataSet: dataSet,
                    cancellationToken: cancellationToken
                )
            )
            .OnSuccess(dataSetVersion => dataSetVersion.Id);
    }

    public async Task<Either<ActionResult, Guid>> CreateNextVersion(
        Guid dataSetId,
        Guid releaseFileId,
        Guid instanceId,
        Guid? dataSetVersionToReplaceId = null,
        CancellationToken cancellationToken = default
    )
    {
        return await GetDataSet(dataSetId, cancellationToken)
            .OnSuccess(ds => ValidateCanCreateNextDataSetVersion(ds, dataSetVersionToReplaceId))
            .OnSuccess(dataSet =>
                (
                    dataSetVersionToReplaceId is not null
                        ? ValidateDataSetVersionToReplace(dataSet, dataSetVersionToReplaceId.Value)
                        : (DataSetVersion?)null
                ).OnSuccess(previousDataSetVersionToReplace => (dataSet, previousDataSetVersionToReplace))
            )
            .OnSuccess(dataSetAndDataSetVersion =>
                CreateDataSetVersion(
                    releaseFileId: releaseFileId,
                    instanceId: instanceId,
                    dataSet: dataSetAndDataSetVersion.dataSet,
                    previousDataSetVersionToReplace: dataSetAndDataSetVersion.previousDataSetVersionToReplace,
                    cancellationToken: cancellationToken
                )
            )
            .OnSuccess(dataSetVersion => dataSetVersion.Id);
    }

    public async Task<Either<ActionResult, Unit>> BulkDeleteVersions(
        Guid releaseVersionId,
        bool forceDeleteAll = false,
        CancellationToken cancellationToken = default
    )
    {
        return await publicDataDbContext.RequireTransaction(() =>
            GetReleaseFiles(releaseVersionId, cancellationToken)
                .OnSuccess(async releaseFiles =>
                    await GetDataSetVersions(releaseFiles, cancellationToken)
                        .OnSuccess(dataSetVersions => (releaseFiles, dataSetVersions))
                        .OnSuccess(releaseFilesAndDataSetVersions =>
                            (
                                releaseFilesAndDataSetVersions.releaseFiles,
                                releaseFilesAndDataSetVersions.dataSetVersions
                            )
                        )
                )
                .OnSuccessDo(releaseFilesAndDataSetVersions =>
                    CheckCanDeleteDataSetVersions(releaseFilesAndDataSetVersions.dataSetVersions, forceDeleteAll)
                )
                .OnSuccessDo(async releaseFilesAndDataSetVersions =>
                    await UnlinkReleaseFilesFromApiDataSets(
                        releaseFilesAndDataSetVersions.releaseFiles,
                        cancellationToken
                    )
                )
                .OnSuccessDo(async releaseFilesAndDataSetVersions =>
                    await DeleteDataSetVersions(releaseFilesAndDataSetVersions.dataSetVersions, cancellationToken)
                )
                .OnSuccessDo(releaseFilesAndDataSetVersions =>
                    DeleteDuckDbFiles(releaseFilesAndDataSetVersions.dataSetVersions)
                )
                .OnSuccessVoid()
        );
    }

    public async Task<Either<ActionResult, Unit>> DeleteVersion(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default
    )
    {
        return await publicDataDbContext.RequireTransaction(() =>
            GetDataSetVersion(dataSetVersionId, cancellationToken)
                .OnSuccessDo(dataSetVersion =>
                    CheckCanDeleteDataSetVersion(dataSetVersion: dataSetVersion, forceDeleteAll: false)
                )
                .OnSuccessDo(async dataSetVersion => await UpdateReleaseFiles(dataSetVersion, cancellationToken))
                .OnSuccessDo(async dataSetVersion => await DeleteDataSetVersion(dataSetVersion, cancellationToken))
                .OnSuccessDo(DeleteDuckDbFiles)
                .OnSuccessVoid()
        );
    }

    private async Task<Either<ActionResult, IReadOnlyList<DataSetVersion>>> GetDataSetVersions(
        IReadOnlyList<ReleaseFile> releaseFiles,
        CancellationToken cancellationToken
    )
    {
        var releaseFileIds = releaseFiles.Select(rf => rf.Id).ToList();

        return await publicDataDbContext
            .DataSetVersions.AsNoTracking()
            .Include(dsv => dsv.DataSet)
            .Where(dsv => releaseFileIds.Contains(dsv.Release.ReleaseFileId))
            .ToListAsync(cancellationToken);
    }

    private async Task<Either<ActionResult, DataSetVersion>> GetDataSetVersion(
        Guid dataSetVersionId,
        CancellationToken cancellationToken
    )
    {
        return await publicDataDbContext
            .DataSetVersions.AsNoTracking()
            .Include(dsv => dsv.DataSet)
            .Where(dsv => dsv.Id == dataSetVersionId)
            .SingleOrNotFoundAsync(cancellationToken);
    }

    private async Task<Either<ActionResult, Unit>> CheckCanDeleteDataSetVersions(
        IReadOnlyList<DataSetVersion> dataSetVersions,
        bool forceDeleteAll = false
    )
    {
        var versionsWhichCanNotBeDeleted = await dataSetVersions
            .ToAsyncEnumerable()
            .WhereAwait(async dsv =>
                !await CanDeleteDataSetVersion(dataSetVersion: dsv, forceDeleteAll: forceDeleteAll)
            )
            .Select(dsv => dsv.Id)
            .ToListAsync();

        if (!versionsWhichCanNotBeDeleted.Any())
        {
            return Unit.Instance;
        }

        return ValidationUtils.ValidationResult(
            new ErrorViewModel
            {
                Code = ValidationMessages.OneOrMoreDataSetVersionsCanNotBeDeleted.Code,
                Message = ValidationMessages.OneOrMoreDataSetVersionsCanNotBeDeleted.Message,
                Detail = new InvalidErrorDetail<IReadOnlyList<Guid>>(versionsWhichCanNotBeDeleted),
                Path = "releaseVersionId",
            }
        );
    }

    private async Task<bool> CanDeleteDataSetVersion(DataSetVersion dataSetVersion, bool forceDeleteAll = false)
    {
        if (dataSetVersion.CanBeDeleted)
        {
            return true;
        }

        if (!forceDeleteAll || !options.Value.EnableThemeDeletion)
        {
            return false;
        }

        var releaseFile = await contentDbContext
            .ReleaseFiles.AsNoTracking()
            .Include(releaseFile => releaseFile.ReleaseVersion.Publication.Theme)
            .SingleAsync(releaseFile => releaseFile.Id == dataSetVersion.Release.ReleaseFileId);

        return releaseFile.ReleaseVersion.Publication.Theme.IsTestOrSeedTheme();
    }

    private async Task<Either<ActionResult, Unit>> CheckCanDeleteDataSetVersion(
        DataSetVersion dataSetVersion,
        bool forceDeleteAll = false
    )
    {
        var canDelete = await CanDeleteDataSetVersion(dataSetVersion: dataSetVersion, forceDeleteAll);

        if (canDelete)
        {
            return Unit.Instance;
        }

        return ValidationUtils.ValidationResult(
            new ErrorViewModel
            {
                Code = ValidationMessages.DataSetVersionCanNotBeDeleted.Code,
                Message = ValidationMessages.DataSetVersionCanNotBeDeleted.Message,
                Detail = new InvalidErrorDetail<Guid>(dataSetVersion.Id),
                Path = "dataSetVersionId",
            }
        );
    }

    private async Task UpdateReleaseFiles(DataSetVersion dataSetVersion, CancellationToken cancellationToken)
    {
        var releaseFiles = await contentDbContext
            .ReleaseFiles.Where(rf => rf.PublicApiDataSetId == dataSetVersion.DataSetId)
            .Where(rf => rf.PublicApiDataSetVersion == dataSetVersion.SemVersion())
            .ToListAsync(cancellationToken);

        await UnlinkReleaseFilesFromApiDataSets(releaseFiles, cancellationToken);
    }

    private async Task DeleteDataSetVersions(
        IReadOnlyList<DataSetVersion> dataSetVersions,
        CancellationToken cancellationToken
    )
    {
        var dataSetsWithNoOtherVersions = dataSetVersions.Where(dsv => dsv.IsFirstVersion).Select(dsv => dsv.DataSet);

        publicDataDbContext.DataSetVersions.RemoveRange(dataSetVersions);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);

        publicDataDbContext.DataSets.RemoveRange(dataSetsWithNoOtherVersions);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task DeleteDataSetVersion(DataSetVersion dataSetVersion, CancellationToken cancellationToken)
    {
        publicDataDbContext.DataSetVersions.Remove(dataSetVersion);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);

        if (dataSetVersion.IsFirstVersion)
        {
            publicDataDbContext.DataSets.Remove(dataSetVersion.DataSet);
            await publicDataDbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<Either<ActionResult, IReadOnlyList<ReleaseFile>>> GetReleaseFiles(
        Guid releaseVersionId,
        CancellationToken cancellationToken
    )
    {
        return await releaseFileRepository.GetByFileType(
            releaseVersionId: releaseVersionId,
            cancellationToken: cancellationToken,
            types: FileType.Data
        );
    }

    private async Task UnlinkReleaseFilesFromApiDataSets(
        IReadOnlyList<ReleaseFile> releaseFiles,
        CancellationToken cancellationToken
    )
    {
        foreach (var releaseFile in releaseFiles)
        {
            UnlinkReleaseFileFromApiDataSet(releaseFile);
        }

        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    private static void UnlinkReleaseFileFromApiDataSet(ReleaseFile releaseFile)
    {
        releaseFile.PublicApiDataSetId = null;
        releaseFile.PublicApiDataSetVersion = null;
    }

    private void DeleteDuckDbFiles(IReadOnlyList<DataSetVersion> dataSetVersions)
    {
        foreach (var dataSetVersion in dataSetVersions)
        {
            DeleteDuckDbFiles(dataSetVersion);
        }
    }

    private void DeleteDuckDbFiles(DataSetVersion dataSetVersion)
    {
        if (dataSetVersion.IsFirstVersion)
        {
            DeleteDataSetDirectory(dataSetVersion);

            return;
        }

        DeleteDataSetVersionDirectory(dataSetVersion);
    }

    private void DeleteDataSetDirectory(DataSetVersion dataSetVersion)
    {
        var dataSetDirectory = GetDataSetDirectory(dataSetVersion);

        DeleteDirectoryIfExists(dataSetDirectory);
    }

    private void DeleteDataSetVersionDirectory(DataSetVersion dataSetVersion)
    {
        var directory = dataSetVersionPathResolver.DirectoryPath(dataSetVersion);

        DeleteDirectoryIfExists(directory);
    }

    private string GetDataSetDirectory(DataSetVersion dataSetVersion)
    {
        var dataSetVersionDirectory = dataSetVersionPathResolver.DirectoryPath(dataSetVersion);
        return Directory.GetParent(dataSetVersionDirectory)!.FullName;
    }

    private static void DeleteDirectoryIfExists(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return;
        }

        Directory.Delete(directory, true);
    }

    private async Task<Either<ActionResult, DataSet>> GetDataSet(
        Guid dataSetId,
        CancellationToken cancellationToken = default
    )
    {
        var dataSet = await publicDataDbContext
            .DataSets.Include(dataSet => dataSet.LatestLiveVersion)
            .Include(dataSet => dataSet.Versions)
            .FirstOrDefaultAsync(dataSet => dataSet.Id == dataSetId, cancellationToken);

        return dataSet is null
            ? ValidationUtils.NotFoundResult<DataSet, Guid>(
                id: dataSetId,
                path: nameof(NextDataSetVersionMappingsCreateRequest.DataSetId).ToLowerFirst()
            )
            : dataSet;
    }

    private Either<ActionResult, DataSet> ValidateInitialDataSet(DataSet dataSet)
    {
        if (dataSet.Versions.Count > 0)
        {
            return ValidationUtils.ValidationResult(
                CreateDataSetIdError(
                    message: ValidationMessages.DataSetMustHaveNoExistingVersions,
                    dataSetId: dataSet.Id
                )
            );
        }

        return dataSet;
    }

    private static Either<ActionResult, DataSet> ValidateCanCreateNextDataSetVersion(
        DataSet dataSet,
        Guid? dataSetVersionToReplaceId
    )
    {
        var isNotReplacingAndIsMissingLiveVersion =
            dataSetVersionToReplaceId is null && dataSet.LatestLiveVersionId is null;

        //TODO: Action for EES-5996: Reword the validation error message to be more appropriate when EES-5779 is LIVE.
        return isNotReplacingAndIsMissingLiveVersion
            ? ValidationUtils.ValidationResult(
                CreateDataSetIdError(message: ValidationMessages.DataSetNoLiveVersion, dataSetId: dataSet.Id)
            )
            : dataSet;
    }

    private Either<ActionResult, DataSetVersion?> ValidateDataSetVersionToReplace(
        DataSet dataSet,
        Guid dataSetVersionToReplaceId
    )
    {
        var previousVersion = dataSet.Versions.FirstOrDefault(dv => dataSetVersionToReplaceId == dv.Id);

        return previousVersion is null
            ? ValidationUtils.ValidationResult(
                CreateDataSetIdError(message: ValidationMessages.NextDataSetVersionNotFound, dataSetId: dataSet.Id)
            )
            : previousVersion;
    }

    private async Task<Either<ActionResult, DataSetVersion>> CreateDataSetVersion(
        Guid releaseFileId,
        Guid instanceId,
        DataSet dataSet,
        DataSetVersion? previousDataSetVersionToReplace = null,
        CancellationToken cancellationToken = default
    )
    {
        return await publicDataDbContext.RequireTransaction(async () =>
            await GetReleaseFile(releaseFileId, cancellationToken)
                .OnSuccess(async releaseFile =>
                    await ValidateReleaseFileAndDataSet(
                            releaseFile,
                            dataSet,
                            previousDataSetVersionToReplace?.PublicVersion,
                            cancellationToken
                        )
                        .OnSuccess(async () =>
                            await CreateDataSetVersion(
                                dataSet,
                                releaseFile,
                                previousDataSetVersionToReplace,
                                cancellationToken
                            )
                        )
                        .OnSuccessDo(async dataSetVersion =>
                            await CreateDataSetVersionImport(
                                dataSetVersion,
                                instanceId,
                                previousDataSetVersionToReplace,
                                cancellationToken
                            )
                        )
                        .OnSuccessDo(async dataSetVersion =>
                            await UpdateReleaseFilePublicDataSetVersionId(
                                releaseFile,
                                dataSetVersion,
                                cancellationToken
                            )
                        )
                )
        );
    }

    private async Task<Either<ActionResult, ReleaseFile>> GetReleaseFile(
        Guid releaseFileId,
        CancellationToken cancellationToken
    )
    {
        var releaseFile = await contentDbContext
            .ReleaseFiles.Include(rf => rf.File)
            .Include(rf => rf.ReleaseVersion)
                .ThenInclude(r => r.Release)
            .FirstOrDefaultAsync(rf => rf.Id == releaseFileId, cancellationToken);

        return releaseFile is null
            ? ValidationUtils.NotFoundResult<ReleaseFile, Guid>(
                id: releaseFileId,
                path: nameof(NextDataSetVersionMappingsCreateRequest.ReleaseFileId).ToLowerFirst()
            )
            : releaseFile;
    }

    private async Task<Either<ActionResult, Unit>> ValidateReleaseFileAndDataSet(
        ReleaseFile releaseFile,
        DataSet dataSet,
        string? dataSetVersionToReplace = null,
        CancellationToken cancellationToken = default
    )
    {
        // ReleaseFile must not already have a DataSetVersion
        if (
            await publicDataDbContext.DataSetVersions.AnyAsync(
                dsv => dsv.Release.ReleaseFileId == releaseFile.Id,
                cancellationToken: cancellationToken
            )
        )
        {
            return ValidationUtils.ValidationResult([
                CreateReleaseFileIdError(
                    message: ValidationMessages.FileHasApiDataSetVersion,
                    releaseFileId: releaseFile.Id
                ),
            ]);
        }

        // ReleaseFile must relate to a ReleaseVersion in Draft approval status
        if (releaseFile.ReleaseVersion.ApprovalStatus != ReleaseApprovalStatus.Draft)
        {
            return ValidationUtils.ValidationResult([
                CreateReleaseFileIdError(
                    message: ValidationMessages.FileReleaseVersionNotDraft,
                    releaseFileId: releaseFile.Id
                ),
            ]);
        }

        List<ErrorViewModel> errors = [];

        // ReleaseFile must relate to a File of type Data
        if (releaseFile.File.Type != FileType.Data)
        {
            errors.Add(
                CreateReleaseFileIdError(message: ValidationMessages.FileTypeNotData, releaseFileId: releaseFile.Id)
            );
        }

        // There must be a ReleaseFile related to the same ReleaseVersion and Subject with File of type Metadata
        if (
            !await contentDbContext
                .ReleaseFiles.Where(rf => rf.ReleaseVersionId == releaseFile.ReleaseVersionId)
                .Where(rf => rf.File.SubjectId == releaseFile.File.SubjectId)
                .Where(rf => rf.File.Type == FileType.Metadata)
                .AnyAsync(cancellationToken: cancellationToken)
        )
        {
            errors.Add(
                CreateReleaseFileIdError(message: ValidationMessages.NoMetadataFile, releaseFileId: releaseFile.Id)
            );
        }

        if (releaseFile.ReleaseVersion.PublicationId != dataSet.PublicationId)
        {
            errors.Add(
                CreateReleaseFileIdError(
                    message: ValidationMessages.FileNotInDataSetPublication,
                    releaseFileId: releaseFile.Id
                )
            );
        }

        var previousReleaseFileIds = dataSet.Versions.Select(version => version.Release.ReleaseFileId).ToList();

        if (dataSetVersionToReplace is not null)
        {
            //`dataSetVersionToReplace` gets initiated when in an amendment.
            //When in an amendment, we are modifying one of the previous release ID and so the
            //selected release ID will be found because it will attempt to 'patch/replace' existing data.
            //Therefore, we can skip validation that ensures data file is in a different release to current release.
            return ValidationResult();
        }

        var previousReleaseIds = await GetReleaseIdsForReleaseFiles(
            contentDbContext,
            previousReleaseFileIds,
            cancellationToken
        );

        var selectedReleaseFileReleaseId = (
            await GetReleaseIdsForReleaseFiles(contentDbContext, [releaseFile.Id], cancellationToken)
        ).Single();

        if (previousReleaseIds.Contains(selectedReleaseFileReleaseId))
        {
            errors.Add(
                CreateReleaseFileIdError(
                    message: ValidationMessages.FileMustBeInDifferentRelease,
                    releaseFileId: releaseFile.Id
                )
            );
        }

        return ValidationResult();

        Either<ActionResult, Unit> ValidationResult() =>
            errors.Count == 0 ? Unit.Instance : ValidationUtils.ValidationResult(errors);
    }

    private async Task<DataSetVersion> CreateDataSetVersion(
        DataSet dataSet,
        ReleaseFile releaseFile,
        DataSetVersion? previousVersionToPatch,
        CancellationToken cancellationToken
    )
    {
        var nextVersion = previousVersionToPatch is not null
            ? previousVersionToPatch.NextPatchVersion()
            : dataSet.LatestLiveVersion?.DefaultNextVersion() ?? new SemVersion(major: 1, minor: 0, patch: 0);

        var dataSetVersion = new DataSetVersion
        {
            DataSetId = dataSet.Id,
            Status = DataSetVersionStatus.Processing,
            Release = new Release
            {
                DataSetFileId =
                    releaseFile.File.DataSetFileId ?? throw new NullReferenceException("DataSetFileId cannot be null"),
                ReleaseFileId = releaseFile.Id,
                Slug = releaseFile.ReleaseVersion.Release.Slug,
                Title = releaseFile.ReleaseVersion.Release.Title,
            },
            Notes = "",
            VersionMajor = nextVersion.Major,
            VersionMinor = nextVersion.Minor,
            VersionPatch = nextVersion.Patch,
        };

        dataSet.Versions.Add(dataSetVersion);
        dataSet.LatestDraftVersion = dataSetVersion;

        publicDataDbContext.DataSets.Update(dataSet);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);

        return dataSetVersion;
    }

    private async Task CreateDataSetVersionImport(
        DataSetVersion dataSetVersion,
        Guid instanceId,
        DataSetVersion? dataSetVersionToReplace = null,
        CancellationToken cancellationToken = default
    )
    {
        var dataSetVersionImport = new DataSetVersionImport
        {
            DataSetVersionId = dataSetVersion.Id,
            InstanceId = instanceId,
            Stage = DataSetVersionImportStage.Pending,
            DataSetVersionToReplaceId = dataSetVersionToReplace?.Id,
        };

        publicDataDbContext.DataSetVersionImports.Add(dataSetVersionImport);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task UpdateReleaseFilePublicDataSetVersionId(
        ReleaseFile releaseFile,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken
    )
    {
        releaseFile.PublicApiDataSetId = dataSetVersion.DataSetId;
        releaseFile.PublicApiDataSetVersion = dataSetVersion.SemVersion();
        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<List<Guid>> GetReleaseIdsForReleaseFiles(
        ContentDbContext contentDbContext,
        List<Guid> releaseFileIds,
        CancellationToken cancellationToken
    )
    {
        return await contentDbContext
            .ReleaseFiles.Include(releaseFile => releaseFile.ReleaseVersion)
            .Where(releaseFile => releaseFileIds.Contains(releaseFile.Id))
            .Select(releaseFile => releaseFile.ReleaseVersion.ReleaseId)
            .ToListAsync(cancellationToken);
    }

    private static ErrorViewModel CreateReleaseFileIdError(LocalizableMessage message, Guid releaseFileId)
    {
        return new ErrorViewModel
        {
            Code = message.Code,
            Message = message.Message,
            Path = nameof(DataSetCreateRequest.ReleaseFileId).ToLowerFirst(),
            Detail = new InvalidErrorDetail<Guid>(releaseFileId),
        };
    }

    private static ErrorViewModel CreateDataSetIdError(LocalizableMessage message, Guid dataSetId)
    {
        return new ErrorViewModel
        {
            Code = message.Code,
            Message = message.Message,
            Path = nameof(NextDataSetVersionMappingsCreateRequest.DataSetId).ToLowerFirst(),
            Detail = new InvalidErrorDetail<Guid>(dataSetId),
        };
    }
}
