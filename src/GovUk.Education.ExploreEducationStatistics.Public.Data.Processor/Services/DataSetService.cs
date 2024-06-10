using System.Transactions;
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValidationMessages =
    GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests.Validators.ValidationMessages;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services;

public class DataSetService(
    ContentDbContext contentDbContext,
    PublicDataDbContext publicDataDbContext
) : IDataSetService
{
    public async Task<Either<ActionResult, (Guid dataSetId, Guid dataSetVersionId)>> CreateInitialDataSetVersion(
        InitialDataSetVersionCreateRequest request,
        Guid instanceId,
        CancellationToken cancellationToken = default)
    {
        return await CreateDataSetVersion(
            releaseFileId: request.ReleaseFileId,
            instanceId: instanceId,
            dataSetSupplier: async releaseFile => await CreateDataSet(releaseFile, cancellationToken),
            cancellationToken: cancellationToken);
    }

    public async Task<Either<ActionResult, (Guid dataSetId, Guid dataSetVersionId)>> CreateNextDataSetVersion(
        NextDataSetVersionCreateRequest request,
        Guid instanceId,
        CancellationToken cancellationToken = default)
    {
        return await CreateDataSetVersion(
            releaseFileId: request.ReleaseFileId,
            instanceId: instanceId,
            dataSetSupplier: async _ => await publicDataDbContext
                .DataSets
                .Include(dataSet => dataSet.LatestLiveVersion)
                .Include(dataSet => dataSet.Versions)
                .SingleAsync(dataSet => dataSet.Id == request.DataSetId, cancellationToken),
            cancellationToken: cancellationToken);
    }

    private async Task<Either<ActionResult, (Guid dataSetId, Guid dataSetVersionId)>> CreateDataSetVersion(
        Guid releaseFileId,
        Guid instanceId,
        Func<ReleaseFile, Task<DataSet>> dataSetSupplier,
        CancellationToken cancellationToken = default)
    {
        var strategy = contentDbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using var transactionScope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions {IsolationLevel = IsolationLevel.ReadCommitted},
                TransactionScopeAsyncFlowOption.Enabled);

            return await GetReleaseFile(releaseFileId, cancellationToken)
                .OnSuccess(async releaseFile => await ValidateReleaseFile(releaseFile, cancellationToken)
                    .OnSuccess(() => dataSetSupplier.Invoke(releaseFile))
                    .OnSuccessDo(async dataSet => 
                        await ValidateReleaseFileAndDataSet(releaseFile, dataSet, cancellationToken))
                    .OnSuccess(async dataSet =>
                        await CreateDataSetVersion(dataSet, releaseFile, cancellationToken))
                    .OnSuccessDo(async dataSetVersion =>
                        await CreateDataSetVersionImport(dataSetVersion, instanceId, cancellationToken))
                    .OnSuccessDo(async dataSetVersion =>
                        await UpdateFilePublicDataSetVersionId(releaseFile, dataSetVersion, cancellationToken))
                    .OnSuccessDo(transactionScope.Complete)
                    .OnSuccess(dataSetVersion =>
                        (dataSetId: dataSetVersion.DataSetId, dataSetVersionId: dataSetVersion.Id)));
        });
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

        if (dataSet.PublicationId != releaseFile.ReleaseVersion.PublicationId)
        {
            errors.Add(CreatDataSetIdError(
                message: ValidationMessages.DataSetAndReleaseFileMustBeForSamePublication,
                dataSetId: dataSet.Id));
        }

        var firstVersion = dataSet.Versions.Count == 0;

        if (!firstVersion)
        {
            if (dataSet.LatestLiveVersionId is null)
            {
                errors.Add(CreatDataSetIdError(
                    message: ValidationMessages.DataSetMustHaveLiveDataSetVersion,
                    dataSetId: dataSet.Id));
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
        }
        
        return errors.Count == 0 ? Unit.Instance : ValidationUtils.ValidationResult(errors);
    }

    private async Task<DataSet> CreateDataSet(
        ReleaseFile releaseFile,
        CancellationToken cancellationToken)
    {
        var dataSet = new DataSet
        {
            Status = DataSetStatus.Draft,
            Title = releaseFile.Name!,
            Summary = releaseFile.Summary ?? "",
            PublicationId = releaseFile.ReleaseVersion.PublicationId
        };

        publicDataDbContext.Add(dataSet);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);

        return dataSet;
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
            Path = nameof(InitialDataSetVersionCreateRequest.ReleaseFileId).ToLowerFirst(),
            Detail = new InvalidErrorDetail<Guid>(releaseFileId)
        };
    }

    private static ErrorViewModel CreatDataSetIdError(
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
