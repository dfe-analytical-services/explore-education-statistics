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
    public async Task<Either<ActionResult, (Guid dataSetId, Guid dataSetVersionId)>> CreateDataSetVersion(
        InitialDataSetVersionCreateRequest request,
        Guid instanceId,
        CancellationToken cancellationToken = default)
    {
        return await GetReleaseFile(request.ReleaseFileId, cancellationToken)
            .OnSuccess(async releaseFile => await ValidateReleaseFile(releaseFile, cancellationToken)
                .OnSuccess(async () => await CreateDataSet(releaseFile, cancellationToken))
                .OnSuccess(async dataSet =>
                    await CreateDataSetVersion(dataSet, releaseFile, cancellationToken))
                .OnSuccessDo(async dataSetVersion =>
                    await CreateDataSetVersionImport(dataSetVersion, instanceId, cancellationToken))
                .OnSuccessDo(async dataSetVersion =>
                    await UpdateFilePublicDataSetVersionId(releaseFile, dataSetVersion, cancellationToken))
                .OnSuccess(dataSetVersion => (dataSetId: dataSetVersion.DataSetId, dataSetVersionId: dataSetVersion.Id)));
    }

    private async Task<Either<ActionResult, ReleaseFile>> GetReleaseFile(
        Guid releaseFileId,
        CancellationToken cancellationToken)
    {
        return await contentDbContext.ReleaseFiles
            .Include(rf => rf.File)
            .Include(rf => rf.ReleaseVersion)
            .FirstOrNotFoundAsync(rf => rf.Id == releaseFileId, cancellationToken);
    }

    private async Task<Either<ActionResult, Unit>> ValidateReleaseFile(
        ReleaseFile releaseFile,
        CancellationToken cancellationToken)
    {
        List<ErrorViewModel> errors = [];

        // ReleaseFile must relate to a ReleaseVersion in Draft approval status
        if (releaseFile.ReleaseVersion.ApprovalStatus != ReleaseApprovalStatus.Draft)
        {
            errors.Add(CreateError(
                code: ValidationMessages.FileReleaseVersionNotDraft.Code,
                message: ValidationMessages.FileReleaseVersionNotDraft.Message,
                releaseFileId: releaseFile.Id));
        }

        // ReleaseFile must relate to a File of type Data
        if (releaseFile.File.Type != FileType.Data)
        {
            errors.Add(CreateError(
                code: ValidationMessages.FileTypeNotData.Code,
                message: ValidationMessages.FileTypeNotData.Message,
                releaseFileId: releaseFile.Id));
        }

        // There must be a ReleaseFile related to the same ReleaseVersion and Subject with File of type Metadata
        if (!await contentDbContext.ReleaseFiles
                .Where(rf => rf.ReleaseVersionId == releaseFile.ReleaseVersionId)
                .Where(rf => rf.File.SubjectId == releaseFile.File.SubjectId)
                .Where(rf => rf.File.Type == FileType.Metadata)
                .AnyAsync(cancellationToken: cancellationToken))
        {
            errors.Add(CreateError(
                code: ValidationMessages.NoMetadataFile.Code,
                message: ValidationMessages.NoMetadataFile.Message,
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

    private static ErrorViewModel CreateError(
        string code,
        string message,
        Guid releaseFileId)
    {
        return new ErrorViewModel
        {
            Code = code,
            Message = message,
            Path = nameof(InitialDataSetVersionCreateRequest.ReleaseFileId).ToLowerFirst(),
            Detail = new InvalidErrorDetail<Guid>(releaseFileId)
        };
    }
}
