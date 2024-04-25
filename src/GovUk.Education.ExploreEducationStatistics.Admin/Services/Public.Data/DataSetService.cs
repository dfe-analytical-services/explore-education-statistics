#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    public async Task<Either<ActionResult, DataSetSummaryViewModel>> GetDataSet(
        Guid dataSetId,
        CancellationToken cancellationToken = default)
    {
        return await CheckDataSetExists(dataSetId, cancellationToken)
            .OnSuccessDo(dataSet => CheckPublicationExists(dataSet.PublicationId, cancellationToken)
                    .OnSuccess(userService.CheckCanViewPublication)
            )
            .OnSuccessCombineWith(async dataSet => await GetReleaseFilesByDataSetVersionId(dataSet, cancellationToken))
            .OnSuccess(combinedDataSetAndReleaseFiles =>
            {
                var (dataSet, releaseFilesByDataSetVersionId) = combinedDataSetAndReleaseFiles;

                return MapDataSet(dataSet, releaseFilesByDataSetVersionId);
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

    private static DataSetSummaryViewModel MapDataSet(DataSet dataSet, IReadOnlyDictionary<Guid, ReleaseFile> releaseFilesByDataSetVersionId)
    {
        var draftVersion = dataSet.LatestDraftVersion is null
            ? null
            : MapDataSetVersion(dataSet.LatestDraftVersion, releaseFilesByDataSetVersionId[dataSet.LatestDraftVersionId!.Value]);

        var latestLiveVersion = dataSet.LatestLiveVersion is null
            ? null
            : MapDataSetVersion(dataSet.LatestLiveVersion, releaseFilesByDataSetVersionId[dataSet.LatestLiveVersionId!.Value]);

        return new DataSetSummaryViewModel
        {
            Id = dataSet.Id,
            Title = dataSet.Title,
            Summary = dataSet.Summary,
            Status = dataSet.Status,
            DraftVersion = draftVersion,
            LatestLiveVersion = latestLiveVersion,
        };
    }

    private static DataSetVersionSummaryViewModel MapDataSetVersion(DataSetVersion dataSetVersion, ReleaseFile releaseFile)
    {
        return new DataSetVersionSummaryViewModel
        {
            Id = dataSetVersion.Id,
            Version = dataSetVersion.Version,
            Status = dataSetVersion.Status,
            Type = dataSetVersion.VersionType,
            DataSetFileId = releaseFile.File.DataSetFileId!.Value,
            ReleaseVersion = MapReleaseVersion(releaseFile.ReleaseVersion),
        };
    }

    private static IdTitleViewModel MapReleaseVersion(ReleaseVersion releaseVersion)
    {
        return new IdTitleViewModel
        {
            Id = releaseVersion.Id,
            Title = releaseVersion.Title,
        };
    }

    private async Task<Either<ActionResult, Publication>> CheckPublicationExists(Guid publicationId,
        CancellationToken cancellationToken)
    {
        return await contentDbContext.Publications
            .AsNoTracking()
            .FirstOrNotFoundAsync(p => p.Id == publicationId, cancellationToken: cancellationToken);
    }

    private async Task<Either<ActionResult, DataSet>> CheckDataSetExists(
        Guid dataSetId,
        CancellationToken cancellationToken)
    {
        return await publicDataDbContext.DataSets
            .AsNoTracking()
            .Include(ds => ds.LatestDraftVersion)
            .Include(ds => ds.LatestLiveVersion)
            .Where(ds => ds.Id == dataSetId)
            .SingleOrNotFoundAsync(cancellationToken);
    }

    private async Task<Either<ActionResult, IReadOnlyDictionary<Guid, ReleaseFile>>> GetReleaseFilesByDataSetVersionId(
        DataSet dataSet,
        CancellationToken cancellationToken)
    {
        if (dataSet.LatestDraftVersion is null && dataSet.LatestLiveVersion is null)
        {
            return new Dictionary<Guid, ReleaseFile>();
        }

        var dataSetVersionIdsByReleaseFileId = new Dictionary<Guid, Guid>();

        if (dataSet.LatestDraftVersion is not null)
        {
            dataSetVersionIdsByReleaseFileId.Add(dataSet.LatestDraftVersion.ReleaseFileId, dataSet.LatestDraftVersionId!.Value);
        }

        if (dataSet.LatestLiveVersion is not null)
        {
            dataSetVersionIdsByReleaseFileId.Add(dataSet.LatestLiveVersion.ReleaseFileId, dataSet.LatestLiveVersionId!.Value);
        }

        return await contentDbContext.ReleaseFiles
            .AsNoTracking()
            .Where(rf => dataSetVersionIdsByReleaseFileId.Keys.Contains(rf.Id))
            .Include(rf => rf.ReleaseVersion)
            .Include(rf => rf.File)
            .ToDictionaryAsync(rf => dataSetVersionIdsByReleaseFileId[rf.Id], cancellationToken);
    }
}
