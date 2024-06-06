using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Transactions;
using ValidationMessages =
    GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests.Validators.ValidationMessages;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services;

internal class DataSetVersionService(
    ContentDbContext contentDbContext,
    PublicDataDbContext publicDataDbContext,
    IDataSetVersionPathResolver dataSetVersionPathResolver
) : IDataSetVersionService
{
    public async Task<Either<ActionResult, Unit>> DeleteVersion(Guid dataSetVersionId,
        CancellationToken cancellationToken = default)
    {
        var strategy = contentDbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(
            async () =>
            {
                using var transactionScope = new TransactionScope(
                    TransactionScopeOption.Required,
                    new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                    TransactionScopeAsyncFlowOption.Enabled);

                return await GetDataSetVersion(dataSetVersionId, cancellationToken)
                    .OnSuccessDo(CheckCanDeleteDataSetVersion)
                    .OnSuccessDo(async dataSetVersion => await GetReleaseFile(dataSetVersion, cancellationToken)
                        .OnSuccessVoid(async releaseFile =>
                            await UpdateFilePublicApiDataSetId(releaseFile, cancellationToken))
                        .OnFailureDo(_ =>
                            throw new KeyNotFoundException(
                                $"The expected 'ReleaseFile', with ID '{dataSetVersion.ReleaseFileId}', was not found.")))
                    .OnSuccessDo(async dataSetVersion => await DeleteDataSetVersion(dataSetVersion, cancellationToken))
                    .OnSuccessVoid(DeleteParquetFiles)
                    .OnSuccessVoid(transactionScope.Complete);
            });
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

    private Either<ActionResult, Unit> CheckCanDeleteDataSetVersion(DataSetVersion dataSetVersion)
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
}
