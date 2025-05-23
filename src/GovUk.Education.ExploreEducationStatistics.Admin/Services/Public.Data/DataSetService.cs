#nullable enable
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;

internal class DataSetService(
    ContentDbContext contentDbContext,
    PublicDataDbContext publicDataDbContext,
    IProcessorClient processorClient,
    IDataSetVersionMappingService dataSetVersionMappingService,
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
                    .Include(ds => ds.Versions)
                    .Include(ds => ds.LatestDraftVersion)
                    .Include(ds => ds.LatestLiveVersion)
                    .Where(ds => ds.PublicationId == publicationId);

                var dataSets = await dataSetsQueryable
                    .OrderByDescending(ds =>
                        ds.LatestLiveVersion != null ? ds.LatestLiveVersion.Published : DateTimeOffset.MinValue)
                    .ThenBy(ds => ds.Title)
                    .ThenBy(ds => ds.Id)
                    .Paginate(page: page, pageSize: pageSize)
                    .ToListAsync(cancellationToken);

                var dataSetReleaseVersions =
                    await GetDataSetReleaseVersions(dataSets, cancellationToken);

                var results = dataSets
                    .Select(ds => MapDataSetSummary(ds, dataSetReleaseVersions[ds.Id].ReleaseFilesByDataSetVersionId))
                    .ToList();

                return new PaginatedListViewModel<DataSetSummaryViewModel>(
                    results,
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
            .OnSuccess(async _ => await processorClient.CreateDataSet(
                releaseFileId: releaseFileId,
                cancellationToken: cancellationToken))
            .OnSuccess(async processorResponse => await QueryDataSet(processorResponse.DataSetId)
                .SingleAsync(cancellationToken))
            .OnSuccess(async dataSet => await MapDataSet(dataSet, cancellationToken));
    }

    private DataSetSummaryViewModel MapDataSetSummary(
        DataSet dataSet,
        IReadOnlyDictionary<Guid, ReleaseFile> releaseFilesByDataSetVersionId)
    {
        var previousReleaseIds = releaseFilesByDataSetVersionId
            .Select(r => r.Value.ReleaseVersion.ReleaseId)
            .ToList();

        return new DataSetSummaryViewModel
        {
            Id = dataSet.Id,
            Title = dataSet.Title,
            Summary = dataSet.Summary,
            Status = dataSet.Status,
            SupersedingDataSetId = dataSet.SupersedingDataSetId,
            DraftVersion = MapDraftSummaryVersion(dataSet.LatestDraftVersion, releaseFilesByDataSetVersionId),
            LatestLiveVersion = MapLiveSummaryVersion(dataSet.LatestLiveVersion, releaseFilesByDataSetVersionId),
            PreviousReleaseIds = previousReleaseIds
        };
    }

    private static DataSetVersionSummaryViewModel? MapDraftSummaryVersion(
        DataSetVersion? dataSetVersion,
        IReadOnlyDictionary<Guid, ReleaseFile> releaseFilesByDataSetVersionId)
    {
        return dataSetVersion != null
            ? new DataSetVersionSummaryViewModel
            {
                Id = dataSetVersion.Id,
                Version = dataSetVersion.PublicVersion,
                Status = dataSetVersion.Status,
                Type = dataSetVersion.VersionType,
                ReleaseVersion = MapReleaseVersion(releaseFilesByDataSetVersionId[dataSetVersion.Id].ReleaseVersion),
                File = MapVersionFile(releaseFilesByDataSetVersionId[dataSetVersion.Id])
            }
            : null;
    }

    private static DataSetLiveVersionSummaryViewModel? MapLiveSummaryVersion(
        DataSetVersion? dataSetVersion,
        IReadOnlyDictionary<Guid, ReleaseFile> releaseFilesByDataSetVersionId)
    {
        return dataSetVersion != null
            ? new DataSetLiveVersionSummaryViewModel
            {
                Id = dataSetVersion.Id,
                Version = dataSetVersion.PublicVersion,
                Published = dataSetVersion.Published!.Value,
                Status = dataSetVersion.Status,
                Type = dataSetVersion.VersionType,
                ReleaseVersion = MapReleaseVersion(releaseFilesByDataSetVersionId[dataSetVersion.Id].ReleaseVersion),
                File = MapVersionFile(releaseFilesByDataSetVersionId[dataSetVersion.Id])
            }
            : null;
    }

    private async Task<DataSetViewModel> MapDataSet(DataSet dataSet, CancellationToken cancellationToken)
    {
        var releaseFilesByDataSetVersionId = await GetReleaseFilesByDataSetVersionId(dataSet, cancellationToken);

        var draftVersion = dataSet.LatestDraftVersion is null
            ? null
            : MapDraftVersion(
                dataSetVersion: dataSet.LatestDraftVersion,
                mappingStatus: await dataSetVersionMappingService.GetMappingStatus(
                    nextDataSetVersionId: dataSet.LatestDraftVersion.Id,
                    cancellationToken),
                releaseFilesByDataSetVersionId[dataSet.LatestDraftVersion.Id]
            );

        var latestLiveVersion = dataSet.LatestLiveVersion is null
            ? null
            : MapLiveVersion(
                dataSet.LatestLiveVersion,
                releaseFilesByDataSetVersionId[dataSet.LatestLiveVersion.Id]
            );

        var allReleaseIds = releaseFilesByDataSetVersionId
            .Select(r => r.Value.ReleaseVersion.ReleaseId)
            .ToList();

        return new DataSetViewModel
        {
            Id = dataSet.Id,
            Title = dataSet.Title,
            Summary = dataSet.Summary,
            Status = dataSet.Status,
            SupersedingDataSetId = dataSet.SupersedingDataSetId,
            DraftVersion = draftVersion,
            LatestLiveVersion = latestLiveVersion,
            PreviousReleaseIds = allReleaseIds
        };
    }

    private async Task<IReadOnlyDictionary<Guid, DataSetReleaseVersions>> GetDataSetReleaseVersions(
        IReadOnlyList<DataSet> dataSets,
        CancellationToken cancellationToken)
    {
        var dataSetsByPreviousReleaseFileId = dataSets
            .SelectMany(ds => ds.Versions)
            .ToDictionary(dsv => dsv.Release.ReleaseFileId, dsv => dsv.DataSet);

        var releaseFiles = await contentDbContext
            .ReleaseFiles
            .Include(rf => rf.ReleaseVersion)
            .ThenInclude(rv => rv.Release)
            .Include(rf => rf.File)
            .Where(rf => dataSetsByPreviousReleaseFileId.Keys.Contains(rf.Id))
            .ToListAsync(cancellationToken);

        return releaseFiles
            .GroupBy(rf => dataSetsByPreviousReleaseFileId[rf.Id])
            .ToDictionary(
                group => group.Key.Id,
                group => new DataSetReleaseVersions
                {
                    ReleaseFilesByDataSetVersionId = group
                        .ToDictionary(
                            rf => group.Key.Versions.Single(v => v.Release.ReleaseFileId == rf.Id).Id,
                            rf => rf
                        )
                }
            );
    }

    private async Task<IReadOnlyDictionary<Guid, ReleaseFile>> GetReleaseFilesByDataSetVersionId(
        DataSet dataSet,
        CancellationToken cancellationToken)
    {
        var dataSetVersionsByReleaseFileId = dataSet
            .Versions
            .ToDictionary(dsv => dsv.Release.ReleaseFileId);

        return await contentDbContext
            .ReleaseFiles
            .Include(rf => rf.ReleaseVersion)
            .ThenInclude(rv => rv.Release)
            .Include(rf => rf.File)
            .Where(releaseFile => dataSetVersionsByReleaseFileId.Keys.Contains(releaseFile.Id))
            .ToDictionaryAsync(
                rf => dataSetVersionsByReleaseFileId[rf.Id].Id,
                cancellationToken);
    }

    private static DataSetDraftVersionViewModel MapDraftVersion(
        DataSetVersion dataSetVersion,
        MappingStatusViewModel? mappingStatus,
        ReleaseFile releaseFile)
    {
        return new DataSetDraftVersionViewModel
        {
            Id = dataSetVersion.Id,
            Version = dataSetVersion.PublicVersion,
            Status = dataSetVersion.Status,
            Type = dataSetVersion.VersionType,
            File = MapVersionFile(releaseFile),
            ReleaseVersion = MapReleaseVersion(releaseFile.ReleaseVersion),
            TotalResults = dataSetVersion.TotalResults,
            Notes = dataSetVersion.Notes,
            GeographicLevels = dataSetVersion.MetaSummary?.GeographicLevels
                .Select(l => l.GetEnumLabel())
                .ToList() ?? null,
            TimePeriods = dataSetVersion.MetaSummary?.TimePeriodRange is not null
                ? TimePeriodRangeViewModel.Create(dataSetVersion.MetaSummary.TimePeriodRange)
                : null,
            Filters = dataSetVersion.MetaSummary?.Filters ?? null,
            Indicators = dataSetVersion.MetaSummary?.Indicators ?? null,
            MappingStatus = mappingStatus
        };
    }
    
    private static DataSetLiveVersionViewModel MapLiveVersion(
        DataSetVersion dataSetVersion,
        ReleaseFile releaseFile)
    {
        return new DataSetLiveVersionViewModel
        {
            Id = dataSetVersion.Id,
            Version = dataSetVersion.PublicVersion,
            Status = dataSetVersion.Status,
            Type = dataSetVersion.VersionType,
            File = MapVersionFile(releaseFile),
            Published = dataSetVersion.Published!.Value,
            TotalResults = dataSetVersion.TotalResults,
            Notes = dataSetVersion.Notes,
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
            Title = releaseVersion.Release.Title,
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

    private async Task<Either<ActionResult, Publication>> CheckPublicationExists(
        Guid publicationId,
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
            .Include(ds => ds.Versions)
            .Include(ds => ds.LatestDraftVersion)
            .Include(ds => ds.LatestLiveVersion);
    }

    private record DataSetReleaseVersions
    {
        public IReadOnlyDictionary<Guid, ReleaseFile> ReleaseFilesByDataSetVersionId { get; init; }
            = new Dictionary<Guid, ReleaseFile>();
    }
}
