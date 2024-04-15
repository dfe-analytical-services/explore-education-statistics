#nullable enable
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Release = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;

public class DataSetService(
    ContentDbContext contentDbContext,
    PublicDataDbContext publicDataDbContext,
    IUserService userService)
    : IDataSetService
{
    public async Task<Either<ActionResult, PaginatedListViewModel<DataSetViewModel>>> ListDataSets(
        int page,
        int pageSize,
        Guid publicationId,
        CancellationToken cancellationToken = default)
    {
        return await CheckPublicationExists(publicationId, cancellationToken)
            .OnSuccess(userService.CheckCanViewPublication)
            .OnSuccess(async () =>
            {
                var dataSetsQueryable = publicDataDbContext.DataSets
                    .AsNoTracking()
                    .Include(ds => ds.LatestDraftVersion)
                    .Include(ds => ds.LatestLiveVersion)
                    .Where(ds => ds.PublicationId == publicationId);

                var dataSets = (await dataSetsQueryable
                        .OrderByDescending(ds =>
                            ds.LatestLiveVersion != null ? ds.LatestLiveVersion.Published : DateTimeOffset.MinValue)
                        .ThenBy(ds => ds.Title)
                        .ThenBy(ds => ds.Id)
                        .Paginate(page: page, pageSize: pageSize)
                        .ToListAsync(cancellationToken: cancellationToken)
                    )
                    .Select(MapDataSet)
                    .ToList();

                return new PaginatedListViewModel<DataSetViewModel>(
                    dataSets,
                    totalResults: await dataSetsQueryable.CountAsync(cancellationToken: cancellationToken),
                    page: page,
                    pageSize: pageSize);
            });
    }

    public async Task<Either<ActionResult, DataSetVersionSummaryViewModel>> GetVersion(
        Guid releaseVersionId,
        Guid dataSetId,
        string dataSetVersion,
        CancellationToken cancellationToken = default)
    {
        return await CheckReleaseVersionExists(releaseVersionId, cancellationToken)
            .OnSuccess(userService.CheckCanViewReleaseVersion)
            .OnSuccessCombineWith(async _ => await CheckVersionExists(dataSetId, dataSetVersion))
            .OnSuccessDo(async combinedReleaseAndDataSetVersion =>
            {
                var (releaseVersion, dataSetVersion) = combinedReleaseAndDataSetVersion;

                return await CheckDataSetVersionBelongsToReleaseVersion(dataSetVersion, releaseVersion);
            })
            .OnSuccess(combinedReleaseAndDataSetVersion =>
            {
                var (releaseVersion, dataSetVersion) = combinedReleaseAndDataSetVersion;

                return MapDataSetVersion(dataSetVersion, releaseVersion);
            });
    }

    private static DataSetViewModel MapDataSet(DataSet dataSet)
    {
        return new DataSetViewModel
        {
            Id = dataSet.Id,
            Title = dataSet.Title,
            Summary = dataSet.Summary,
            Status = dataSet.Status,
            DraftVersion = MapDraftVersion(dataSet.LatestDraftVersion),
            LatestLiveVersion = MapLiveVersion(dataSet.LatestLiveVersion),
            SupersedingDataSetId = dataSet.SupersedingDataSetId,
        };
    }

    private static DataSetVersionViewModel? MapDraftVersion(DataSetVersion? dataSetVersion)
    {
        return dataSetVersion != null
            ? new DataSetVersionViewModel
            {
                Id = dataSetVersion.Id,
                Version = dataSetVersion.Version,
                Status = dataSetVersion.Status,
                Type = dataSetVersion.VersionType,
            }
            : null;
    }

    private static DataSetLiveVersionViewModel? MapLiveVersion(DataSetVersion? dataSetVersion)
    {
        return dataSetVersion != null
            ? new DataSetLiveVersionViewModel
            {
                Id = dataSetVersion.Id,
                Version = dataSetVersion.Version,
                Published = dataSetVersion.Published!.Value,
                Status = dataSetVersion.Status,
                Type = dataSetVersion.VersionType,
            }
            : null;
    }

    private async Task<Either<ActionResult, Publication>> CheckPublicationExists(Guid publicationId,
        CancellationToken cancellationToken)
    {
        return await contentDbContext.Publications
            .AsNoTracking()
            .FirstOrNotFoundAsync(p => p.Id == publicationId, cancellationToken: cancellationToken);
    }

    private async Task<Either<ActionResult, ReleaseVersion>> CheckReleaseVersionExists(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default)
    {
        return await contentDbContext.ReleaseVersions
            .AsNoTracking()
            .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId, cancellationToken: cancellationToken);
    }

    private async Task<Either<ActionResult, DataSetVersion>> CheckVersionExists(
        Guid dataSetId,
        string dataSetVersion,
        CancellationToken cancellationToken = default)
    {
        if (!VersionUtils.TryParse(dataSetVersion, out var version))
        {
            return new NotFoundResult();
        }

        return await publicDataDbContext.DataSetVersions
            .AsNoTracking()
            .Include(dsv => dsv.DataSet)
            .Where(dsv => dsv.DataSetId == dataSetId)
            .Where(dsv => dsv.VersionMajor == version.Major)
            .Where(dsv => dsv.VersionMinor == version.Minor)
            .SingleOrNotFoundAsync(cancellationToken);
    }

    private async Task<Either<ActionResult, Unit>> CheckDataSetVersionBelongsToReleaseVersion(
        DataSetVersion dataSetVersion,
        ReleaseVersion releaseVersion)
    {
        return await contentDbContext.ReleaseFiles
            .AsNoTracking()
            .Where(rf => rf.ReleaseVersionId == releaseVersion.Id)
            .Where(rf => rf.FileId == dataSetVersion.CsvFileId)
            .SingleOrNotFoundAsync()
            .OnSuccessVoid();
    }

    private DataSetVersionSummaryViewModel MapDataSetVersion(DataSetVersion dataSetVersion, ReleaseVersion releaseVersion)
    {
        return new DataSetVersionSummaryViewModel
        {
            Title = dataSetVersion.DataSet.Title,
            Release = MapReleaseVersion(releaseVersion),
            Version = dataSetVersion.Version,
            Type = dataSetVersion.VersionType,
            Status = dataSetVersion.Status,
            DataSetFileId = dataSetVersion.CsvFileId,
        };
    }

    private Release MapReleaseVersion(ReleaseVersion releaseVersion)
    {
        return new Release
        {
            Id = releaseVersion.ReleaseId,
            Title = releaseVersion.Title,
        };
    }
}
