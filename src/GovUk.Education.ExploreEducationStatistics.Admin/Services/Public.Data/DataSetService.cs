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

internal class DataSetService(
    ContentDbContext contentDbContext,
    PublicDataDbContext publicDataDbContext,
    IProcessorClient processorClient,
    IUserService userService)
    : IDataSetService
{
    public async Task<Either<ActionResult, PaginatedListViewModel<DataSetSummaryViewModel>>> ListDataSets(
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
                    .Select(MapDataSetSummary)
                    .ToList();

                return new PaginatedListViewModel<DataSetSummaryViewModel>(
                    dataSets,
                    totalResults: await dataSetsQueryable.CountAsync(cancellationToken: cancellationToken),
                    page: page,
                    pageSize: pageSize);
            });
    }

    public async Task<Either<ActionResult, DataSetViewModel>> GetDataSet(
        Guid dataSetId,
        CancellationToken cancellationToken = default)
    {
        return await QueryDataSet(dataSetId)
            .SingleOrNotFoundAsync(cancellationToken)
            .OnSuccessDo(dataSet => CheckPublicationExists(dataSet.PublicationId, cancellationToken)
                .OnSuccess(userService.CheckCanViewPublication)
            )
            .OnSuccess(async dataSet => await MapDataSet(dataSet, cancellationToken));
    }

    public async Task<Either<ActionResult, DataSetViewModel>> CreateDataSet(
        Guid releaseFileId,
        CancellationToken cancellationToken = default)
    {
        return await userService.CheckIsBauUser()
            .OnSuccess(async _ => await processorClient.CreateInitialDataSetVersion(
                releaseFileId: releaseFileId,
                cancellationToken: cancellationToken))
            .OnSuccess(async processorResponse => await QueryDataSet(processorResponse.DataSetId)
                .SingleAsync(cancellationToken))
            .OnSuccess(async dataSet => await MapDataSet(dataSet, cancellationToken));
    }

    private static DataSetSummaryViewModel MapDataSetSummary(DataSet dataSet)
    {
        return new DataSetSummaryViewModel
        {
            Id = dataSet.Id,
            Title = dataSet.Title,
            Summary = dataSet.Summary,
            Status = dataSet.Status,
            SupersedingDataSetId = dataSet.SupersedingDataSetId,
            DraftVersion = MapDraftSummaryVersion(dataSet.LatestDraftVersion),
            LatestLiveVersion = MapLiveSummaryVersion(dataSet.LatestLiveVersion),
        };
    }

    private static DataSetVersionSummaryViewModel? MapDraftSummaryVersion(DataSetVersion? dataSetVersion)
    {
        return dataSetVersion != null
            ? new DataSetVersionSummaryViewModel
            {
                Id = dataSetVersion.Id,
                Version = dataSetVersion.Version,
                Status = dataSetVersion.Status,
                Type = dataSetVersion.VersionType,
            }
            : null;
    }

    private static DataSetLiveVersionSummaryViewModel? MapLiveSummaryVersion(DataSetVersion? dataSetVersion)
    {
        return dataSetVersion != null
            ? new DataSetLiveVersionSummaryViewModel
            {
                Id = dataSetVersion.Id,
                Version = dataSetVersion.Version,
                Published = dataSetVersion.Published!.Value,
                Status = dataSetVersion.Status,
                Type = dataSetVersion.VersionType,
            }
            : null;
    }

    private async Task<DataSetViewModel> MapDataSet(DataSet dataSet, CancellationToken cancellationToken)
    {
        var releaseFilesByDataSetVersionId =
            await GetReleaseFilesByDataSetVersionId(dataSet, cancellationToken);

        var draftVersion = dataSet.LatestDraftVersion is null
            ? null
            : MapDraftVersion(
                dataSet.LatestDraftVersion,
                releaseFilesByDataSetVersionId[dataSet.LatestDraftVersionId!.Value]
            );

        var latestLiveVersion = dataSet.LatestLiveVersion is null
            ? null
            : MapLiveVersion(
                dataSet.LatestLiveVersion,
                releaseFilesByDataSetVersionId[dataSet.LatestLiveVersionId!.Value]
            );

        return new DataSetViewModel
        {
            Id = dataSet.Id,
            Title = dataSet.Title,
            Summary = dataSet.Summary,
            Status = dataSet.Status,
            SupersedingDataSetId = dataSet.SupersedingDataSetId,
            DraftVersion = draftVersion,
            LatestLiveVersion = latestLiveVersion,
        };
    }

    private async Task<IReadOnlyDictionary<Guid, ReleaseFile>> GetReleaseFilesByDataSetVersionId(
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
            dataSetVersionIdsByReleaseFileId.Add(
                dataSet.LatestDraftVersion.ReleaseFileId,
                dataSet.LatestDraftVersionId!.Value
            );
        }

        if (dataSet.LatestLiveVersion is not null)
        {
            dataSetVersionIdsByReleaseFileId.Add(
                dataSet.LatestLiveVersion.ReleaseFileId,
                dataSet.LatestLiveVersionId!.Value
            );
        }

        return await contentDbContext.ReleaseFiles
            .AsNoTracking()
            .Where(rf => dataSetVersionIdsByReleaseFileId.Keys.Contains(rf.Id))
            .Include(rf => rf.ReleaseVersion)
            .Include(rf => rf.File)
            .ToDictionaryAsync(rf => dataSetVersionIdsByReleaseFileId[rf.Id], cancellationToken);
    }

    private static DataSetVersionViewModel MapDraftVersion(
        DataSetVersion dataSetVersion,
        ReleaseFile releaseFile)
    {
        return new DataSetVersionViewModel
        {
            Id = dataSetVersion.Id,
            Version = dataSetVersion.Version,
            Status = dataSetVersion.Status,
            Type = dataSetVersion.VersionType,
            File = MapVersionFile(releaseFile),
            ReleaseVersion = MapReleaseVersion(releaseFile.ReleaseVersion),
            TotalResults = dataSetVersion.TotalResults,
            GeographicLevels = dataSetVersion.MetaSummary?.GeographicLevels
                .Select(l => l.GetEnumLabel())
                .ToList() ?? null,
            TimePeriods = dataSetVersion.MetaSummary?.TimePeriodRange is not null
                ? TimePeriodRangeViewModel.Create(dataSetVersion.MetaSummary.TimePeriodRange)
                : null,
            Filters = dataSetVersion.MetaSummary?.Filters ?? null,
            Indicators = dataSetVersion.MetaSummary?.Indicators ?? null,
        };
    }

    private static DataSetLiveVersionViewModel MapLiveVersion(
        DataSetVersion dataSetVersion,
        ReleaseFile releaseFile)
    {
        return new DataSetLiveVersionViewModel
        {
            Id = dataSetVersion.Id,
            Version = dataSetVersion.Version,
            Status = dataSetVersion.Status,
            Type = dataSetVersion.VersionType,
            File = MapVersionFile(releaseFile),
            Published = dataSetVersion.Published!.Value,
            TotalResults = dataSetVersion.TotalResults,
            ReleaseVersion = MapReleaseVersion(releaseFile.ReleaseVersion),
            GeographicLevels = dataSetVersion.MetaSummary!.GeographicLevels
                .Select(l => l.GetEnumLabel())
                .ToList(),
            TimePeriods = TimePeriodRangeViewModel.Create(dataSetVersion.MetaSummary.TimePeriodRange),
            Filters = dataSetVersion.MetaSummary.Filters,
            Indicators = dataSetVersion.MetaSummary.Indicators,
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

    private static IdTitleViewModel MapVersionFile(ReleaseFile releaseFile)
    {
        return new IdTitleViewModel
        {
            Id = releaseFile.File.DataSetFileId!.Value,
            Title = releaseFile.Name ?? string.Empty,
        };
    }

    private async Task<Either<ActionResult, Publication>> CheckPublicationExists(Guid publicationId,
        CancellationToken cancellationToken)
    {
        return await contentDbContext.Publications
            .AsNoTracking()
            .FirstOrNotFoundAsync(p => p.Id == publicationId, cancellationToken: cancellationToken);
    }

    private IQueryable<DataSet> QueryDataSet(Guid dataSetId)
    {
        return publicDataDbContext.DataSets
            .AsNoTracking()
            .Where(ds => ds.Id == dataSetId)
            .Include(ds => ds.LatestDraftVersion)
            .Include(ds => ds.LatestLiveVersion);
    }
}
