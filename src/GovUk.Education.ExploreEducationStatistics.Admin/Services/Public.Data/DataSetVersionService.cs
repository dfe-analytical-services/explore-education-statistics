#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semver;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;

public class DataSetVersionService(
    ContentDbContext contentDbContext,
    PublicDataDbContext publicDataDbContext,
    IProcessorClient processorClient,
    IPublicDataApiClient publicDataApiClient,
    IUserService userService)
    : IDataSetVersionService
{
    public async Task<List<DataSetVersionStatusSummary>> GetStatusesForReleaseVersion(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default)
    {
        var releaseFileIds = await contentDbContext
            .ReleaseFiles
            .Where(rf => rf.ReleaseVersionId == releaseVersionId && rf.File.Type == FileType.Data)
            .Select(rf => rf.Id)
            .ToListAsync(cancellationToken);

        return await publicDataDbContext
            .DataSetVersions
            .Where(dataSetVersion => releaseFileIds.Contains(dataSetVersion.ReleaseFileId))
            .Include(dataSetVersion => dataSetVersion.DataSet)
            .Select(dataSetVersion => new DataSetVersionStatusSummary(
                dataSetVersion.Id,
                dataSetVersion.DataSet.Title,
                dataSetVersion.Status)
            )
            .ToListAsync(cancellationToken);
    }

    public async Task<Either<ActionResult, DataSetVersionSummaryViewModel>> CreateNextVersion(
        Guid releaseFileId,
        Guid dataSetId,
        CancellationToken cancellationToken = default)
    {
        return await userService.CheckIsBauUser()
            .OnSuccess(async _ => await processorClient.CreateNextDataSetVersion(
                dataSetId: dataSetId,
                releaseFileId: releaseFileId,
                cancellationToken: cancellationToken))
            .OnSuccess(async processorResponse => await publicDataDbContext
                .DataSetVersions
                .SingleAsync(
                    dataSetVersion => dataSetVersion.Id == processorResponse.DataSetVersionId,
                    cancellationToken))
            .OnSuccess(MapDraftSummaryVersion);
    }

    public async Task<Either<ActionResult, DataSetVersion>> GetDataSetVersion(
        Guid dataSetId,
        SemVersion version,
        CancellationToken cancellationToken = default)
    {
        return await publicDataDbContext.DataSetVersions
            .AsNoTracking()
            .Include(dsv => dsv.DataSet)
            .Where(dsv => dsv.DataSetId == dataSetId)
            .Where(dsv => dsv.VersionMajor == version.Major)
            .Where(dsv => dsv.VersionMinor == version.Minor)
            .SingleOrNotFoundAsync(cancellationToken);
    }

    public async Task<Either<ActionResult, Unit>> DeleteVersion(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default)
    {
        return await userService.CheckIsBauUser()
            .OnSuccessVoid(async () => await processorClient.DeleteDataSetVersion(
                dataSetVersionId: dataSetVersionId,
                cancellationToken: cancellationToken));
    }

    private static DataSetVersionSummaryViewModel MapDraftSummaryVersion(DataSetVersion dataSetVersion)
    {
        return new DataSetVersionSummaryViewModel
        {
            Id = dataSetVersion.Id,
            Version = dataSetVersion.Version,
            Status = dataSetVersion.Status,
            Type = dataSetVersion.VersionType,
        };
    }

    public async Task<Either<ActionResult, HttpResponseMessage>> GetVersionChanges(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default)
    {
        return await userService.CheckIsBauUser()
            .OnSuccess(() => publicDataDbContext.DataSetVersions
                .AsNoTracking()
                .Where(dsv => dsv.Id == dataSetVersionId)
                .SingleOrNotFoundAsync(cancellationToken: cancellationToken))
            .OnSuccess(dsv => publicDataApiClient.GetDataSetVersionChanges(
                dataSetId: dsv.DataSetId,
                dataSetVersion: dsv.Version,
                cancellationToken: cancellationToken
            ));
    }
}

public record DataSetVersionStatusSummary(Guid Id, string Title, DataSetVersionStatus Status);
