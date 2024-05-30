#nullable enable
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValidationMessages = GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests.Validators.ValidationMessages;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;

public class DataSetVersionService(
    ContentDbContext contentDbContext,
    PublicDataDbContext publicDataDbContext,
    IProcessorClient processorClient,
    IUserService userService)
    : IDataSetVersionService
{
    public async Task<List<DataSetVersionStatusSummary>> GetStatusesForReleaseVersion(Guid releaseVersionId)
    {
        var releaseFileIds = await contentDbContext
            .ReleaseFiles
            .Where(rf => rf.ReleaseVersionId == releaseVersionId && rf.File.Type == FileType.Data)
            .Select(rf => rf.Id)
            .ToListAsync();

        return await publicDataDbContext
            .DataSetVersions
            .Where(dataSetVersion => releaseFileIds.Contains(dataSetVersion.ReleaseFileId))
            .Include(dataSetVersion => dataSetVersion.DataSet)
            .Select(dataSetVersion => new DataSetVersionStatusSummary(
                dataSetVersion.Id,
                dataSetVersion.DataSet.Title,
                dataSetVersion.Status)
            )
            .ToListAsync();
    }

    public async Task<Either<ActionResult, Unit>> DeleteVersion(Guid dataSetVersionId, CancellationToken cancellationToken = default)
    {
        return await userService.CheckIsBauUser()
            .OnSuccess(async () => await GetDataSetVersion(dataSetVersionId, cancellationToken))
            .OnSuccessDo(CheckCanDeleteDataSetVersion)
            .OnSuccessVoid(async () => await processorClient.DeleteDataSetVersion(dataSetVersionId, cancellationToken));
    }

    public async Task<bool> FileHasVersion(
        Guid releaseFileId,
        CancellationToken cancellationToken = default)
    {
        return await publicDataDbContext.DataSetVersions
            .AsNoTracking()
            .AnyAsync(dsv => dsv.ReleaseFileId == releaseFileId, cancellationToken);
    }

    private async Task<Either<ActionResult, DataSetVersion>> GetDataSetVersion(Guid dataSetVersionId, CancellationToken cancellationToken)
    {
        return await publicDataDbContext.DataSetVersions
           .AsNoTracking()
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
            Detail = new InvalidErrorDetail<Guid>(dataSetVersion.Id)
        });
    }
}

public record DataSetVersionStatusSummary(Guid Id, string Title, DataSetVersionStatus Status);
