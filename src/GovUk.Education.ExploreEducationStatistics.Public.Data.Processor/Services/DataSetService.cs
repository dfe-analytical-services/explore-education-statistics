using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services;

public class DataSetService(
    ContentDbContext contentDbContext,
    PublicDataDbContext publicDataDbContext
) : IDataSetService
{
    public async Task<Either<ActionResult, Guid>> CreateDataSetVersion(Guid releaseFileId,
        Guid? dataSetId = null,
        CancellationToken cancellationToken = default)
    {
        return await ValidateDataSet(dataSetId, cancellationToken)
            .OnSuccess(async () =>
                await ValidateReleaseFile(releaseFileId: releaseFileId, dataSetId: dataSetId, cancellationToken))
            .OnSuccess(async () =>
                await GetOrCreateDataSet(dataSetId: dataSetId, releaseFileId: releaseFileId, cancellationToken))
            .OnSuccess(async dataSet =>
                await CreateDataSetVersion(dataSet, releaseFileId, cancellationToken))
            .OnSuccessDo(async dataSetVersion =>
                await UpdateFilePublicDataSetVersionId(releaseFileId: releaseFileId,
                    dataSetVersionId: dataSetVersion.Id,
                    cancellationToken))
            .OnSuccess(dataSetVersion => dataSetVersion.Id);
    }

    private async Task<Either<ActionResult, Unit>> ValidateDataSet(Guid? dataSetId,
        CancellationToken cancellationToken)
    {
        return dataSetId.HasValue
            ? await publicDataDbContext.DataSets
                .FirstOrNotFoundAsync(ds => ds.Id == dataSetId, cancellationToken)
                .OnSuccess(ValidateDataSet)
            : Unit.Instance;
    }

    private static Either<ActionResult, Unit> ValidateDataSet(DataSet dataSet)
    {
        List<ErrorViewModel> errors = [];

        // Dataset must not have a draft DataSetVersion
        if (dataSet.LatestDraftVersionId != null)
        {
            errors.Add(CreateError(code: "DataSetHasDraftVersion",
                message: "Dataset already has a draft version",
                dataSetId: dataSet.Id));
        }

        return errors.Count == 0 ? Unit.Instance : ValidationUtils.ValidationResult(errors);
    }

    private async Task<Either<ActionResult, Unit>> ValidateReleaseFile(Guid releaseFileId,
        Guid? dataSetId,
        CancellationToken cancellationToken)
    {
        return await contentDbContext.ReleaseFiles
            .Include(rf => rf.File)
            .Include(rf => rf.ReleaseVersion)
            .FirstOrNotFoundAsync(rf => rf.Id == releaseFileId, cancellationToken)
            .OnSuccess(releaseFile => ValidateReleaseFile(releaseFile, dataSetId, cancellationToken));
    }

    private async Task<Either<ActionResult, Unit>> ValidateReleaseFile(ReleaseFile releaseFile,
        Guid? dataSetId,
        CancellationToken cancellationToken)
    {
        List<ErrorViewModel> errors = [];

        // ReleaseFile must relate to a ReleaseVersion in Draft approval status
        if (releaseFile.ReleaseVersion.ApprovalStatus != ReleaseApprovalStatus.Draft)
        {
            errors.Add(CreateError(
                code: "ReleaseVersionNotDraft",
                message: "ReleaseVersion is not in Draft approval status",
                releaseFileId: releaseFile.Id));
        }

        // ReleaseFile must relate to a File of type Data
        if (releaseFile.File.Type != FileType.Data)
        {
            errors.Add(CreateError(
                code: "ReleaseFileNotTypeData",
                message: "ReleaseFile is not of type Data",
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
                code: "NoMetadataFile",
                message: "ReleaseFile does not have a corresponding ReleaseFile of type Metadata",
                releaseFileId: releaseFile.Id));
        }

        if (dataSetId.HasValue)
        {
            var dataSetPublicationId = await publicDataDbContext.DataSets
                .Where(ds => ds.Id == dataSetId)
                .Select(ds => ds.PublicationId)
                .FirstAsync(cancellationToken);

            // Dataset and ReleaseFile must relate to the same Publication
            if (dataSetPublicationId != releaseFile.ReleaseVersion.PublicationId)
            {
                errors.Add(CreateError(
                    code: "PublicationMismatch",
                    message: "Dataset and ReleaseFile do not belong to the same Publication",
                    dataSetId: dataSetId,
                    releaseFileId: releaseFile.Id));
            }
        }

        return errors.Count == 0 ? Unit.Instance : ValidationUtils.ValidationResult(errors);
    }

    private async Task<DataSet> CreateDataSet(Guid releaseFileId,
        CancellationToken cancellationToken)
    {
        var dataSet = await contentDbContext
            .ReleaseFiles
            .Where(rf => rf.Id == releaseFileId)
            .Select(rf => new DataSet
            {
                Status = DataSetStatus.Draft,
                Title = rf.Name!,
                Summary = rf.Summary ?? "",
                PublicationId = rf.ReleaseVersion.PublicationId,
            })
            .FirstAsync(cancellationToken);

        publicDataDbContext.Add(dataSet);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);

        return dataSet;
    }

    private async Task<DataSetVersion> CreateDataSetVersion(DataSet dataSet,
        Guid releaseFileId,
        CancellationToken cancellationToken)
    {
        var dataSetVersion = new DataSetVersion
        {
            ReleaseFileId = releaseFileId,
            DataSetId = dataSet.Id,
            Status = DataSetVersionStatus.Processing,
            Imports =
            [
                new DataSetVersionImport
                {
                    InstanceId = Guid.NewGuid(), Stage = DataSetVersionImportStage.Created
                }
            ],
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

    private async Task<Either<ActionResult, DataSet>> GetOrCreateDataSet(Guid? dataSetId,
        Guid releaseFileId,
        CancellationToken cancellationToken)
    {
        return dataSetId.HasValue
            ? await GetDataSet(dataSetId.Value, cancellationToken)
            : await CreateDataSet(releaseFileId, cancellationToken);
    }

    private async Task<Either<ActionResult, DataSet>> GetDataSet(Guid dataSetId,
        CancellationToken cancellationToken)
    {
        return await publicDataDbContext.DataSets
            .Include(ds => ds.LatestLiveVersion)
            .FirstOrNotFoundAsync(ds => ds.Id == dataSetId, cancellationToken);
    }

    private async Task UpdateFilePublicDataSetVersionId(Guid releaseFileId,
        Guid dataSetVersionId,
        CancellationToken cancellationToken)
    {
        var file = await contentDbContext.ReleaseFiles
            .Where(rf => rf.Id == releaseFileId)
            .Select(rf => rf.File)
            .FirstAsync(cancellationToken);

        file.PublicDataSetVersionId = dataSetVersionId;

        await contentDbContext.SaveChangesAsync(cancellationToken);
    }

    private static ErrorViewModel CreateError(string code,
        string message,
        Guid? dataSetId = null,
        Guid? releaseFileId = null)
    {
        return new ErrorViewModel
        {
            Code = code,
            Message = message,
            Detail = new Dictionary<string, Guid?>
            {
                {
                    "dataSetId", dataSetId
                },
                {
                    "releaseFileId", releaseFileId
                }
            }.Filter(valuePair => valuePair.Value.HasValue)
        };
    }
}
