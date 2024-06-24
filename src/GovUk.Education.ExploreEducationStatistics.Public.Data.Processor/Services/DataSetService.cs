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
    public async Task<Either<ActionResult, (Guid dataSetId, Guid dataSetVersionId)>> CreateDataSet(
        DataSetCreateRequest request,
        Guid instanceId,
        CancellationToken cancellationToken = default)
    {
        var strategy = contentDbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using var transactionScope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.ReadCommitted
                },
                TransactionScopeAsyncFlowOption.Enabled);

            return await GetReleaseFile(request.ReleaseFileId, cancellationToken)
                .OnSuccess(async releaseFile => await ValidateReleaseFile(releaseFile, cancellationToken)
                    .OnSuccess(async () => await CreateDataSet(releaseFile, cancellationToken))
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
            DataSetVersionId = dataSetVersion.Id,
            InstanceId = instanceId,
            Stage = DataSetVersionImportStage.Pending
        };

        publicDataDbContext.DataSetVersionImports.Add(dataSetVersionImport);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task UpdateFilePublicDataSetVersionId(
        ReleaseFile releaseFile,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken)
    {
        releaseFile.PublicApiDataSetId = dataSetVersion.DataSetId;
        releaseFile.PublicApiDataSetVersion = dataSetVersion.FullSemanticVersion();
        await contentDbContext.SaveChangesAsync(cancellationToken);
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
}
