using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
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
    public async Task<Either<ActionResult, Unit>> DeleteVersion(
        Guid dataSetVersionId,
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
                    .OnSuccessDo(async dataSetVersion => await UpdateReleaseFiles(dataSetVersion, cancellationToken))
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
