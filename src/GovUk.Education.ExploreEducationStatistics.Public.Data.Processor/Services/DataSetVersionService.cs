using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValidationMessages =
    GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests.Validators.ValidationMessages;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services;

internal class DataSetVersionService(
    ContentDbContext contentDbContext,
    PublicDataDbContext publicDataDbContext,
    IDataSetVersionPathResolver dataSetVersionPathResolver
) : IDataSetVersionService
{
    public async Task<Either<ActionResult, (Guid dataSetId, Guid dataSetVersionId)>> CreateInitialVersion(
        Guid dataSetId,
        Guid releaseFileId,
        Guid instanceId,
        CancellationToken cancellationToken = default)
    {
        return await GetDataSet(dataSetId, cancellationToken)
            .OnSuccess(ValidateInitialDataSet)
            .OnSuccess(dataSet => CreateDataSetVersion(
                releaseFileId: releaseFileId,
                instanceId: instanceId,
                dataSet: dataSet, 
                cancellationToken: cancellationToken));
    }

    public async Task<Either<ActionResult, (Guid dataSetId, Guid dataSetVersionId)>> CreateNextVersion(
        Guid dataSetId,
        Guid releaseFileId,
        Guid instanceId,
        CancellationToken cancellationToken = default)
    {
        return await GetDataSet(dataSetId, cancellationToken)
            .OnSuccess(ValidateNextDataSet)
            .OnSuccess(dataSet => CreateDataSetVersion(
                releaseFileId: releaseFileId,
                instanceId: instanceId,
                dataSet: dataSet, 
                cancellationToken: cancellationToken));
    }
    
    public async Task<Either<ActionResult, Unit>> DeleteVersion(Guid dataSetVersionId,
        CancellationToken cancellationToken = default)
    {
        return await publicDataDbContext.RequireTransaction(() => 
            await GetDataSetVersion(dataSetVersionId, cancellationToken)
                .OnSuccessDo(CheckCanDeleteDataSetVersion)
                .OnSuccessDo(async dataSetVersion => await UpdateReleaseFiles(dataSetVersion, cancellationToken))
                .OnSuccessDo(async dataSetVersion => await DeleteDataSetVersion(dataSetVersion, cancellationToken))
                .OnSuccessVoid(DeleteParquetFiles)
    }

    private async Task<Either<ActionResult, DataSetVersion>> GetDataSetVersion(Guid dataSetVersionId,
        CancellationToken cancellationToken)
    {
        return await publicDataDbContext.DataSetVersions
            .AsNoTracking()
            .Include(dsv => dsv.DataSet)
            .Where(dsv => dsv.Id == dataSetVersionId)
            .SingleOrNotFoundAsync(cancellationToken);
    }

    private static Either<ActionResult, Unit> CheckCanDeleteDataSetVersion(DataSetVersion dataSetVersion)
    {
        if (dataSetVersion.CanBeDeleted)
        {
            return Unit.Instance;
        }

        return ValidationUtils.ValidationResult(new ErrorViewModel
        {
            Code = ValidationMessages.DataSetVersionCanNotBeDeleted.Code,
            Message = ValidationMessages.DataSetVersionCanNotBeDeleted.Message,
            Detail = new InvalidErrorDetail<Guid>(dataSetVersion.Id),
            Path = "dataSetVersionId"
        });
    }

    private async Task UpdateReleaseFiles(
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken)
    {
        var releaseFiles = await contentDbContext.ReleaseFiles
            .Where(rf => rf.PublicApiDataSetId == dataSetVersion.DataSetId)
            .Where(rf => rf.PublicApiDataSetVersion == dataSetVersion.Version)
            .ToListAsync(cancellationToken);


        foreach (var releaseFile in releaseFiles)
        {
            releaseFile.PublicApiDataSetId = null;
            releaseFile.PublicApiDataSetVersion = null;
        }

        await contentDbContext.SaveChangesAsync(cancellationToken);
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

    private async Task<Either<ActionResult, ReleaseFile>> GetReleaseFile(DataSetVersion dataSetVersion,
        CancellationToken cancellationToken)
    {
        return await contentDbContext.ReleaseFiles
            .Include(rf => rf.File)
            .SingleOrNotFoundAsync(rf => rf.Id == dataSetVersion.ReleaseFileId, cancellationToken);
    }

    private async Task UpdateFilePublicApiDataSetId(ReleaseFile releaseFile, CancellationToken cancellationToken)
    {
        releaseFile.File.PublicApiDataSetId = null;
        releaseFile.File.PublicApiDataSetVersion = null;
        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    private void DeleteParquetFiles(DataSetVersion dataSetVersion)
    {
        var directory = dataSetVersionPathResolver.DirectoryPath(dataSetVersion);

        if (!Directory.Exists(directory))
        {
            return;
        }

        if (dataSetVersion.IsFirstVersion)
        {
            var dataSetDirectory = Directory.GetParent(directory)!.FullName;

            Directory.Delete(dataSetDirectory, true);

            return;
        }

        Directory.Delete(directory, true);
    }
    
    private async Task<Either<ActionResult, DataSet>> GetDataSet(
        Guid dataSetId,
        CancellationToken cancellationToken = default)
    {
        var dataSet = await publicDataDbContext
            .DataSets
            .Include(dataSet => dataSet.LatestLiveVersion)
            .Include(dataSet => dataSet.Versions)
            .FirstOrDefaultAsync(dataSet => dataSet.Id == dataSetId, cancellationToken);

        return dataSet is null
            ? ValidationUtils.ValidationResult(CreateDataSetIdError(
                message: ValidationMessages.FileNotFound,
                dataSetId: dataSetId
            ))
            : dataSet;
    }
    
    private Either<ActionResult, DataSet> ValidateInitialDataSet(DataSet dataSet)
    {
        if (dataSet.Versions.Count > 0)
        {
            return ValidationUtils.ValidationResult(CreateDataSetIdError(
                message: ValidationMessages.DataSetMustHaveNoExistingVersions,
                dataSetId: dataSet.Id));
        }

        return dataSet;
    }
    
    private Either<ActionResult, DataSet> ValidateNextDataSet(DataSet dataSet)
    {
        if (dataSet.LatestLiveVersionId is null)
        {
            return ValidationUtils.ValidationResult(CreateDataSetIdError(
                message: ValidationMessages.DataSetMustHaveLiveDataSetVersion,
                dataSetId: dataSet.Id));
        }

        return dataSet;
    }

    private async Task<Either<ActionResult, (Guid dataSetId, Guid dataSetVersionId)>> CreateDataSetVersion(
        Guid releaseFileId,
        Guid instanceId,
        DataSet dataSet,
        CancellationToken cancellationToken = default)
    {
        return await publicDataDbContext.RequireTransaction(async () => 
            await GetReleaseFile(releaseFileId, cancellationToken)
                .OnSuccess(async releaseFile => await ValidateReleaseFile(releaseFile, cancellationToken)
                    .OnSuccessDo(async () => 
                        await ValidateReleaseFileAndDataSet(releaseFile, dataSet, cancellationToken))
                    .OnSuccess(async () =>
                        await CreateDataSetVersion(dataSet, releaseFile, cancellationToken))
                    .OnSuccessDo(async dataSetVersion =>
                        await CreateDataSetVersionImport(dataSetVersion, instanceId, cancellationToken))
                    .OnSuccessDo(async dataSetVersion =>
                        await UpdateFilePublicDataSetVersionId(releaseFile, dataSetVersion, cancellationToken))
                    .OnSuccess(dataSetVersion =>
                        (dataSetId: dataSetVersion.DataSetId, dataSetVersionId: dataSetVersion.Id))));
    }

    private async Task<Either<ActionResult, ReleaseFile>> GetReleaseFile(
        Guid releaseFileId,
        CancellationToken cancellationToken)
    {
        var releaseFile = await contentDbContext.ReleaseFiles
            .Include(rf => rf.File)
            .Include(rf => rf.ReleaseVersion)
            .FirstOrDefaultAsync(rf => rf.Id == releaseFileId, cancellationToken);

        return releaseFile is null
            ? ValidationUtils.ValidationResult(CreateReleaseFileIdError(
                message: ValidationMessages.FileNotFound,
                releaseFileId: releaseFileId
            ))
            : releaseFile;
    }

    private async Task<Either<ActionResult, Unit>> ValidateReleaseFile(
        ReleaseFile releaseFile,
        CancellationToken cancellationToken)
    {
        // ReleaseFile must not already have a DataSetVersion
        if (await publicDataDbContext.DataSetVersions.AnyAsync(
                dsv => dsv.ReleaseFileId == releaseFile.Id,
                cancellationToken: cancellationToken))
        {
            return ValidationUtils.ValidationResult(
            [
                CreateReleaseFileIdError(
                    message: ValidationMessages.FileHasApiDataSetVersion,
                    releaseFileId: releaseFile.Id)
            ]);
        }

        // ReleaseFile must relate to a ReleaseVersion in Draft approval status
        if (releaseFile.ReleaseVersion.ApprovalStatus != ReleaseApprovalStatus.Draft)
        {
            return ValidationUtils.ValidationResult(
            [
                CreateReleaseFileIdError(
                    message: ValidationMessages.FileReleaseVersionNotDraft,
                    releaseFileId: releaseFile.Id)
            ]);
        }

        List<ErrorViewModel> errors = [];

        // ReleaseFile must relate to a File of type Data
        if (releaseFile.File.Type != FileType.Data)
        {
            errors.Add(CreateReleaseFileIdError(
                message: ValidationMessages.FileTypeNotData,
                releaseFileId: releaseFile.Id));
        }

        // There must be a ReleaseFile related to the same ReleaseVersion and Subject with File of type Metadata
        if (!await contentDbContext.ReleaseFiles
                .Where(rf => rf.ReleaseVersionId == releaseFile.ReleaseVersionId)
                .Where(rf => rf.File.SubjectId == releaseFile.File.SubjectId)
                .Where(rf => rf.File.Type == FileType.Metadata)
                .AnyAsync(cancellationToken: cancellationToken))
        {
            errors.Add(CreateReleaseFileIdError(
                message: ValidationMessages.NoMetadataFile,
                releaseFileId: releaseFile.Id));
        }

        return errors.Count == 0 ? Unit.Instance : ValidationUtils.ValidationResult(errors);
    }
    
    private async Task<Either<ActionResult, Unit>> ValidateReleaseFileAndDataSet(
        ReleaseFile releaseFile,
        DataSet dataSet,
        CancellationToken cancellationToken)
    {
        List<ErrorViewModel> errors = [];

        if (releaseFile.ReleaseVersion.PublicationId != dataSet.PublicationId)
        {
            errors.Add(CreateReleaseFileIdError(
                message: ValidationMessages.NextReleaseFileMustBeForSamePublicationAsDataSet,
                releaseFileId: releaseFile.Id));
        }

        var historicReleaseFileIds = dataSet
            .Versions
            .Select(version => version.ReleaseFileId)
            .ToList();

        var historicalReleaseIds = await GetReleaseIdsForReleaseFiles(
            contentDbContext,
            historicReleaseFileIds,
            cancellationToken);

        var selectedReleaseFileReleaseId = (await GetReleaseIdsForReleaseFiles(
                contentDbContext,
                [releaseFile.Id],
                cancellationToken))
            .Single();

        if (historicalReleaseIds.Contains(selectedReleaseFileReleaseId))
        {
            errors.Add(CreateReleaseFileIdError(
                message: ValidationMessages.ReleaseFileMustBeFromDifferentReleaseToHistoricalVersions,
                releaseFileId: releaseFile.Id));
        }
        
        return errors.Count == 0 ? Unit.Instance : ValidationUtils.ValidationResult(errors);
    }

    private async Task<DataSetVersion> CreateDataSetVersion(
        DataSet dataSet,
        ReleaseFile releaseFile,
        CancellationToken cancellationToken)
    {
        var dataSetVersion = new DataSetVersion
        {
            ReleaseFileId = releaseFile.Id,
            DataSetId = dataSet.Id,
            Status = DataSetVersionStatus.Processing,
            Notes = "",
            VersionMajor = dataSet.LatestLiveVersion?.VersionMajor ?? 1,
            VersionMinor = dataSet.LatestLiveVersion?.VersionMinor + 1 ?? 0
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
        CancellationToken cancellationToken)
    {
        var dataSetVersionImport = new DataSetVersionImport
        {
            DataSetVersionId = dataSetVersion.Id, InstanceId = instanceId, Stage = DataSetVersionImportStage.Pending
        };

        publicDataDbContext.DataSetVersionImports.Add(dataSetVersionImport);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task UpdateFilePublicDataSetVersionId(
        ReleaseFile releaseFile,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken)
    {
        releaseFile.File.PublicApiDataSetId = dataSetVersion.DataSetId;
        releaseFile.File.PublicApiDataSetVersion = dataSetVersion.FullSemanticVersion();
        await contentDbContext.SaveChangesAsync(cancellationToken);
    }        
    
    private static async Task<List<Guid>> GetReleaseIdsForReleaseFiles(
        ContentDbContext contentDbContext,
        List<Guid> releaseFileIds,
        CancellationToken cancellationToken)
    {
        return await contentDbContext
            .ReleaseFiles
            .Include(releaseFile => releaseFile.ReleaseVersion)
            .Where(releaseFile => releaseFileIds.Contains(releaseFile.Id))
            .Select(releaseFile => releaseFile.ReleaseVersion.ReleaseId)
            .ToListAsync(cancellationToken);
    }

    private static ErrorViewModel CreateReleaseFileIdError(
        LocalizableMessage message,
        Guid releaseFileId)
    {
        return new ErrorViewModel
        {
            Code = message.Code,
            Message = message.Message,
            Path = nameof(DataSetCreateRequest.ReleaseFileId).ToLowerFirst(),
            Detail = new InvalidErrorDetail<Guid>(releaseFileId)
        };
    }

    private static ErrorViewModel CreateDataSetIdError(
        LocalizableMessage message,
        Guid dataSetId)
    {
        return new ErrorViewModel
        {
            Code = message.Code,
            Message = message.Message,
            Path = nameof(NextDataSetVersionCreateRequest.DataSetId).ToLowerFirst(),
            Detail = new InvalidErrorDetail<Guid>(dataSetId)
        };
    }
}
