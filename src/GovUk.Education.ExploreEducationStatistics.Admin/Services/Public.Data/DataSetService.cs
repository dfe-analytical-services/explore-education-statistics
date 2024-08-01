#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
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

                var releasesByDataSetVersionByDataSet =
                    await GetPreviousReleasesByVersionByDataSet(dataSets, cancellationToken);

                var results = dataSets
                    .Select(ds => MapDataSetSummary(ds, releasesByDataSetVersionByDataSet[ds.Id]))
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
        IReadOnlyDictionary<Guid, ReleaseVersion> releasesByDataSetVersion)
    {
        var previousReleaseIds = releasesByDataSetVersion
            .Select(r => r.Value.Id)
            .ToList();

        return new DataSetSummaryViewModel
        {
            Id = dataSet.Id,
            Title = dataSet.Title,
            Summary = dataSet.Summary,
            Status = dataSet.Status,
            SupersedingDataSetId = dataSet.SupersedingDataSetId,
            DraftVersion = MapDraftSummaryVersion(dataSet.LatestDraftVersion, releasesByDataSetVersion),
            LatestLiveVersion = MapLiveSummaryVersion(dataSet.LatestLiveVersion, releasesByDataSetVersion),
            PreviousReleaseIds = previousReleaseIds
        };
    }

    private static DataSetVersionSummaryViewModel? MapDraftSummaryVersion(
        DataSetVersion? dataSetVersion,
        IReadOnlyDictionary<Guid, ReleaseVersion> releasesByDataSetVersion)
    {
        return dataSetVersion != null
            ? new DataSetVersionSummaryViewModel
            {
                Id = dataSetVersion.Id,
                Version = dataSetVersion.Version,
                Status = dataSetVersion.Status,
                Type = dataSetVersion.VersionType,
                Release = MapReleaseVersion(releasesByDataSetVersion[dataSetVersion.Id])
            }
            : null;
    }

    private static DataSetLiveVersionSummaryViewModel? MapLiveSummaryVersion(
        DataSetVersion? dataSetVersion,
        IReadOnlyDictionary<Guid, ReleaseVersion> releasesByDataSetVersion)
    {
        return dataSetVersion != null
            ? new DataSetLiveVersionSummaryViewModel
            {
                Id = dataSetVersion.Id,
                Version = dataSetVersion.Version,
                Published = dataSetVersion.Published!.Value,
                Status = dataSetVersion.Status,
                Type = dataSetVersion.VersionType,
                Release = MapReleaseVersion(releasesByDataSetVersion[dataSetVersion.Id])
            }
            : null;
    }

    private async Task<DataSetViewModel> MapDataSet(DataSet dataSet, CancellationToken cancellationToken)
    {
        var releaseFilesByVersion =
            await GetReleaseFilesByDataSetVersion(dataSet, cancellationToken);

        var draftVersion = dataSet.LatestDraftVersion is null
            ? null
            : MapDraftVersion(
                dataSetVersion: dataSet.LatestDraftVersion,
                mappingStatus: await GetMappingStatus(
                    nextDataSetVersionId: dataSet.LatestDraftVersion.Id,
                    cancellationToken),
                releaseFilesByVersion[dataSet.LatestDraftVersion]
            );

        var latestLiveVersion = dataSet.LatestLiveVersion is null
            ? null
            : MapLiveVersion(
                dataSet.LatestLiveVersion,
                releaseFilesByVersion[dataSet.LatestLiveVersion]
            );

        var previousReleasesByVersion = await GetDataSetPreviousReleasesByVersion(dataSet, cancellationToken);
        var previousReleaseIds = previousReleasesByVersion
            .Select(r => r.Value.Id)
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
            PreviousReleaseIds = previousReleaseIds
        };
    }

    private async Task<Dictionary<Guid, Dictionary<Guid, ReleaseVersion>>> GetPreviousReleasesByVersionByDataSet(
        IReadOnlyList<DataSet> dataSets, 
        CancellationToken cancellationToken)
    {
        var dataSetsByPreviousReleaseFileId = dataSets
            .SelectMany(ds => ds.Versions)
            .ToDictionary(dsv => dsv.ReleaseFileId, dsv => dsv.DataSet);

        var releasesByReleaseFileId = await contentDbContext
            .ReleaseFiles
            .Where(releaseFile => dataSetsByPreviousReleaseFileId.Keys.Contains(releaseFile.Id))
            .ToDictionaryAsync(
                rf => rf.Id,
                rf => rf.ReleaseVersion,
                cancellationToken);

        return releasesByReleaseFileId
            .Select(d => new
            {
                ReleaseFileId = d.Key,
                ReleaseVersion = d.Value,
                DataSet = dataSetsByPreviousReleaseFileId[d.Key]
            })
            .GroupBy(a => a.DataSet)
            .ToDictionary(
                g => g.Key.Id,
                g => g.ToDictionary(
                        a => a.DataSet.Versions.Single(v => v.ReleaseFileId == a.ReleaseFileId).Id,
                        a => a.ReleaseVersion
                    )
            );
    }

    private async Task<IReadOnlyDictionary<Guid, ReleaseVersion>> GetDataSetPreviousReleasesByVersion(DataSet dataSet, CancellationToken cancellationToken)
    {
        var dataSetVersionsByPreviousReleaseFileId = dataSet
            .Versions
            .ToDictionary(dsv => dsv.ReleaseFileId);

        var releasesByReleaseFileId = await contentDbContext
            .ReleaseFiles
            .Where(releaseFile => dataSetVersionsByPreviousReleaseFileId.Keys.Contains(releaseFile.Id))
            .ToDictionaryAsync(
                rf => rf.Id, 
                rf => rf.ReleaseVersion, 
                cancellationToken);

        return releasesByReleaseFileId
            .ToDictionary(
                d => dataSetVersionsByPreviousReleaseFileId[d.Key].Id,
                d => d.Value
            );
    }

    private async Task<IReadOnlyDictionary<DataSetVersion, ReleaseFile>> GetReleaseFilesByDataSetVersion(
        DataSet dataSet,
        CancellationToken cancellationToken)
    {
        if (dataSet.LatestDraftVersion is null && dataSet.LatestLiveVersion is null)
        {
            return new Dictionary<DataSetVersion, ReleaseFile>();
        }

        var dataSetVersions = new List<DataSetVersion>();
        var predicate = PredicateBuilder.False<ReleaseFile>();

        if (dataSet.LatestDraftVersion is not null)
        {
            dataSetVersions.Add(dataSet.LatestDraftVersion);

            predicate = predicate.Or(rf => rf.Id == dataSet.LatestDraftVersion.ReleaseFileId);
        }

        if (dataSet.LatestLiveVersion is not null)
        {
            dataSetVersions.Add(dataSet.LatestLiveVersion);

            predicate = predicate.Or(rf => rf.Id == dataSet.LatestLiveVersion.ReleaseFileId);
        }

        return await contentDbContext.ReleaseFiles
            .AsNoTracking()
            .Where(predicate)
            .Include(rf => rf.ReleaseVersion)
            .Include(rf => rf.File)
            .ToDictionaryAsync(
                rf => dataSetVersions.Single(dsv => dsv.ReleaseFileId == rf.Id),
                cancellationToken: cancellationToken
            );
    }

    private static DataSetDraftVersionViewModel MapDraftVersion(
        DataSetVersion dataSetVersion,
        MappingStatusViewModel? mappingStatus,
        ReleaseFile releaseFile)
    {   
        return new DataSetDraftVersionViewModel
        {
            Id = dataSetVersion.Id,
            Version = dataSetVersion.Version,
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

    private async Task<MappingStatusViewModel?> GetMappingStatus(
        Guid nextDataSetVersionId,
        CancellationToken cancellationToken)
    {
        return await publicDataDbContext
            .DataSetVersionMappings
            .Where(mapping => mapping.TargetDataSetVersionId == nextDataSetVersionId)
            .Select(mapping => new MappingStatusViewModel
            {
                LocationsComplete = mapping.LocationMappingsComplete,
                FiltersComplete = mapping.FilterMappingsComplete
            })
            .SingleOrDefaultAsync(cancellationToken);
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
            .Include(ds => ds.Versions)
            .Include(ds => ds.LatestDraftVersion)
            .Include(ds => ds.LatestLiveVersion);
    }
}
