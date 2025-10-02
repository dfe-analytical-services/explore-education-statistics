using System.Net.Mime;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

internal class DataSetService(
    PublicDataDbContext publicDataDbContext,
    IDataSetVersionPathResolver dataSetVersionPathResolver,
    IUserService userService,
    IAnalyticsService analyticsService,
    IAuthorizationHandlerService authorizationHandlerService
) : IDataSetService
{
    public async Task<Either<ActionResult, DataSetViewModel>> GetDataSet(
        Guid dataSetId,
        CancellationToken cancellationToken = default
    )
    {
        return await publicDataDbContext
            .DataSets.AsNoTracking()
            .Include(ds => ds.LatestLiveVersion)
            .SingleOrNotFoundAsync(ds => ds.Id == dataSetId, cancellationToken: cancellationToken)
            .OnSuccessDo(userService.CheckCanViewDataSet)
            .OnSuccessDo(ds =>
                analyticsService.CaptureDataSetCall(
                    dataSetId: ds.Id,
                    type: DataSetCallType.GetSummary,
                    cancellationToken: cancellationToken
                )
            )
            .OnSuccess(MapDataSet);
    }

    public async Task<Either<ActionResult, FileStreamResult>> DownloadDataSet(
        Guid dataSetId,
        string? dataSetVersion,
        CancellationToken cancellationToken = default
    )
    {
        return await FindVersion(
                dataSetId: dataSetId,
                dataSetVersion: dataSetVersion,
                cancellationToken: cancellationToken
            )
            .OnSuccessDo(userService.CheckCanViewDataSetVersion)
            .OnSuccessDo(dsv =>
                analyticsService.CaptureDataSetVersionCall(
                    dataSetVersionId: dsv.Id,
                    type: DataSetVersionCallType.DownloadCsv,
                    requestedDataSetVersion: dataSetVersion,
                    cancellationToken: cancellationToken
                )
            )
            .OnSuccess(DownloadDataSetVersionToStream);
    }

    private FileStreamResult DownloadDataSetVersionToStream(DataSetVersion dataSetVersion)
    {
        var csvDataPath = dataSetVersionPathResolver.CsvDataPath(dataSetVersion);

        var fileStream = new FileStream(csvDataPath, FileMode.Open, FileAccess.Read, FileShare.Read);

        return new FileStreamResult(fileStream, MediaTypeNames.Text.Csv)
        {
            FileDownloadName = $"{dataSetVersion.DataSetId}_v{dataSetVersion.PublicVersion}.csv",
        };
    }

    public async Task<Either<ActionResult, DataSetPaginatedListViewModel>> ListDataSets(
        int page,
        int pageSize,
        Guid publicationId,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = publicDataDbContext
            .DataSets.AsNoTracking()
            .Include(ds => ds.LatestLiveVersion)
            .Where(ds => ds.PublicationId == publicationId)
            .WherePublicStatus();

        var totalResults = await queryable.CountAsync(cancellationToken: cancellationToken);

        var dataSets = (
            await queryable
                .OrderByDescending(ds => ds.LatestLiveVersion!.Published)
                .ThenBy(ds => ds.Title)
                .ThenBy(ds => ds.Id)
                .Paginate(page: page, pageSize: pageSize)
                .ToListAsync(cancellationToken: cancellationToken)
        )
            .Select(MapDataSet)
            .ToList();

        await analyticsService.CapturePublicationCall(
            publicationId: publicationId,
            type: PublicationCallType.GetDataSets,
            parameters: new PaginationParameters(Page: page, PageSize: pageSize),
            cancellationToken: cancellationToken
        );

        return new DataSetPaginatedListViewModel
        {
            Results = dataSets,
            Paging = new PagingViewModel(page: page, pageSize: pageSize, totalResults: totalResults),
        };
    }

    public async Task<Either<ActionResult, DataSetVersionViewModel>> GetVersion(
        Guid dataSetId,
        string dataSetVersion,
        CancellationToken cancellationToken = default
    )
    {
        return await publicDataDbContext
            .DataSetVersions.AsNoTracking()
            .FindByVersion(dataSetId: dataSetId, version: dataSetVersion, cancellationToken: cancellationToken)
            .OnSuccessDo(userService.CheckCanViewDataSetVersion)
            .OnSuccessDo(dsv =>
                analyticsService.CaptureDataSetVersionCall(
                    dataSetVersionId: dsv.Id,
                    type: DataSetVersionCallType.GetSummary,
                    requestedDataSetVersion: dataSetVersion,
                    cancellationToken: cancellationToken
                )
            )
            .OnSuccess(MapDataSetVersion);
    }

    public async Task<Either<ActionResult, DataSetVersionPaginatedListViewModel>> ListVersions(
        Guid dataSetId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        return await publicDataDbContext
            .DataSets.AsNoTracking()
            .SingleOrNotFoundAsync(ds => ds.Id == dataSetId, cancellationToken: cancellationToken)
            .OnSuccessDo(userService.CheckCanViewDataSet)
            .OnSuccessDo(ds =>
                analyticsService.CaptureDataSetCall(
                    dataSetId: ds.Id,
                    type: DataSetCallType.GetVersions,
                    parameters: new PaginationParameters(Page: page, PageSize: pageSize),
                    cancellationToken: cancellationToken
                )
            )
            .OnSuccess(async dataSet =>
            {
                var includeDraftVersion = await authorizationHandlerService.RequestHasValidPreviewToken(dataSet);

                return await ListPaginatedVersions(
                    dataSet: dataSet,
                    page: page,
                    pageSize: pageSize,
                    includeDraftVersion: includeDraftVersion,
                    cancellationToken: cancellationToken
                );
            });
    }

    public async Task<Either<ActionResult, DataSetMetaViewModel>> GetMeta(
        Guid dataSetId,
        string? dataSetVersion = null,
        IReadOnlySet<DataSetMetaType>? types = null,
        CancellationToken cancellationToken = default
    )
    {
        return await FindVersion(
                dataSetId: dataSetId,
                dataSetVersion: dataSetVersion,
                cancellationToken: cancellationToken
            )
            .OnSuccessDo(userService.CheckCanViewDataSetVersion)
            .OnSuccessDo(dsv =>
                analyticsService.CaptureDataSetVersionCall(
                    dataSetVersionId: dsv.Id,
                    type: DataSetVersionCallType.GetMetadata,
                    requestedDataSetVersion: dataSetVersion,
                    parameters: types != null ? new GetMetadataAnalyticsParameters(types) : null,
                    cancellationToken: cancellationToken
                )
            )
            .OnSuccessDo(dsv => LoadMeta(dsv, types, cancellationToken))
            .OnSuccess(MapVersionMeta);
    }

    private async Task<Either<ActionResult, DataSetVersionPaginatedListViewModel>> ListPaginatedVersions(
        DataSet dataSet,
        int page,
        int pageSize,
        bool includeDraftVersion = false,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = publicDataDbContext.DataSetVersions.AsNoTracking().Where(dsv => dsv.DataSetId == dataSet.Id);
        queryable = includeDraftVersion
            ? queryable.WherePublicStatusOrSpecifiedId(dataSet.LatestDraftVersionId!.Value)
            : queryable.WherePublicStatus();

        var totalResults = await queryable.CountAsync(cancellationToken: cancellationToken);

        var dataSetVersions = await queryable
            .OrderByDescending(dsv => dsv.VersionMajor)
            .ThenByDescending(dsv => dsv.VersionMinor)
            .Paginate(page: page, pageSize: pageSize)
            .ToListAsync(cancellationToken: cancellationToken);

        var results = dataSetVersions.Select(MapDataSetVersion).ToList();

        return new DataSetVersionPaginatedListViewModel
        {
            Results = results,
            Paging = new PagingViewModel(page: page, pageSize: pageSize, totalResults: totalResults),
        };
    }

    private static DataSetViewModel MapDataSet(DataSet dataSet)
    {
        return new DataSetViewModel
        {
            Id = dataSet.Id,
            Title = dataSet.Title,
            Summary = dataSet.Summary,
            Status = dataSet.Status,
            LatestVersion = dataSet.LatestLiveVersion != null ? MapLatestVersion(dataSet.LatestLiveVersion) : null,
            SupersedingDataSetId = dataSet.SupersedingDataSetId,
        };
    }

    private static DataSetLatestVersionViewModel MapLatestVersion(DataSetVersion latestVersion)
    {
        return new DataSetLatestVersionViewModel
        {
            Version = latestVersion.PublicVersion,
            Published = latestVersion.Published!.Value,
            TotalResults = latestVersion.TotalResults,
            File = MapFile(latestVersion),
            TimePeriods = MapTimePeriods(latestVersion.MetaSummary!.TimePeriodRange),
            GeographicLevels = latestVersion.MetaSummary.GeographicLevels,
            Filters = latestVersion.MetaSummary.Filters,
            Indicators = latestVersion.MetaSummary.Indicators,
        };
    }

    private static TimePeriodRangeViewModel MapTimePeriods(TimePeriodRange timePeriodRange)
    {
        return new TimePeriodRangeViewModel
        {
            Start = TimePeriodFormatter.FormatLabel(timePeriodRange.Start.Period, timePeriodRange.Start.Code),
            End = TimePeriodFormatter.FormatLabel(timePeriodRange.End.Period, timePeriodRange.End.Code),
        };
    }

    private static DataSetVersionViewModel MapDataSetVersion(DataSetVersion dataSetVersion)
    {
        return new DataSetVersionViewModel
        {
            Version = dataSetVersion.PublicVersion,
            Type = dataSetVersion.VersionType,
            Status = dataSetVersion.Status,
            Published = dataSetVersion.Published,
            Withdrawn = dataSetVersion.Withdrawn,
            Notes = dataSetVersion.Notes,
            TotalResults = dataSetVersion.TotalResults,
            File = MapFile(dataSetVersion),
            Release = MapRelease(dataSetVersion),
            TimePeriods = MapTimePeriods(dataSetVersion.MetaSummary!.TimePeriodRange),
            GeographicLevels = dataSetVersion.MetaSummary.GeographicLevels,
            Filters = dataSetVersion.MetaSummary.Filters,
            Indicators = dataSetVersion.MetaSummary.Indicators,
        };
    }

    private static DataSetVersionFileViewModel MapFile(DataSetVersion dataSetVersion)
    {
        return new DataSetVersionFileViewModel { Id = dataSetVersion.Release.DataSetFileId };
    }

    private static DataSetVersionReleaseViewModel MapRelease(DataSetVersion dataSetVersion)
    {
        return new DataSetVersionReleaseViewModel
        {
            Title = dataSetVersion.Release.Title,
            Slug = dataSetVersion.Release.Slug,
        };
    }

    private async Task<Either<ActionResult, DataSetVersion>> FindVersion(
        Guid dataSetId,
        string? dataSetVersion = null,
        CancellationToken cancellationToken = default
    )
    {
        if (dataSetVersion is null or "*")
        {
            return await publicDataDbContext
                .DataSets.AsNoTracking()
                .Include(ds => ds.LatestLiveVersion)
                .Where(ds => ds.Id == dataSetId)
                .Select(ds => ds.LatestLiveVersion!)
                .SingleOrNotFoundAsync(cancellationToken);
        }

        return dataSetVersion.Contains('*')
            ? await publicDataDbContext
                .DataSetVersions.AsNoTracking()
                .WherePublishedStatus()
                .FindByVersion(dataSetId: dataSetId, version: dataSetVersion, cancellationToken: cancellationToken)
            : await publicDataDbContext
                .DataSetVersions.AsNoTracking()
                .FindByVersion(dataSetId: dataSetId, version: dataSetVersion, cancellationToken: cancellationToken);
    }

    private async Task LoadMeta(
        DataSetVersion dataSetVersion,
        IReadOnlySet<DataSetMetaType>? types = null,
        CancellationToken cancellationToken = default
    )
    {
        types = types.IsNullOrEmpty() ? EnumUtil.GetEnums<DataSetMetaType>().ToHashSet() : types!;

        if (types.Contains(DataSetMetaType.Filters))
        {
            dataSetVersion.FilterMetas = await publicDataDbContext
                .FilterMetas.AsNoTracking()
                .Where(fm => fm.DataSetVersionId == dataSetVersion.Id)
                .Include(fm => fm.OptionLinks)
                .ThenInclude(fom => fom.Option)
                .ToListAsync(cancellationToken: cancellationToken);
        }

        if (types.Contains(DataSetMetaType.Locations))
        {
            dataSetVersion.GeographicLevelMeta = await publicDataDbContext
                .GeographicLevelMetas.AsNoTracking()
                .Where(lm => lm.DataSetVersionId == dataSetVersion.Id)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            dataSetVersion.LocationMetas = await publicDataDbContext
                .LocationMetas.AsNoTracking()
                .Where(lm => lm.DataSetVersionId == dataSetVersion.Id)
                .Include(lm => lm.OptionLinks)
                .ThenInclude(l => l.Option)
                .ToListAsync(cancellationToken: cancellationToken);
        }

        if (types.Contains(DataSetMetaType.Indicators))
        {
            dataSetVersion.IndicatorMetas = await publicDataDbContext
                .IndicatorMetas.AsNoTracking()
                .Where(lm => lm.DataSetVersionId == dataSetVersion.Id)
                .ToListAsync(cancellationToken: cancellationToken);
        }

        if (types.Contains(DataSetMetaType.TimePeriods))
        {
            dataSetVersion.TimePeriodMetas = await publicDataDbContext
                .TimePeriodMetas.AsNoTracking()
                .Where(lm => lm.DataSetVersionId == dataSetVersion.Id)
                .ToListAsync(cancellationToken: cancellationToken);
        }
    }

    private static DataSetMetaViewModel MapVersionMeta(DataSetVersion dataSetVersion)
    {
        var filters = dataSetVersion.FilterMetas.Select(MapFilterOptions).OrderBy(fm => fm.Label).ToList();

        var indicators = dataSetVersion
            .IndicatorMetas.Select(IndicatorViewModel.Create)
            .OrderBy(im => im.Label)
            .ToList();

        var geographicLevels =
            dataSetVersion.GeographicLevelMeta?.Levels.Select(GeographicLevelViewModel.Create).ToList() ?? [];

        var locations = dataSetVersion.LocationMetas.Select(MapLocationGroupOptions).ToList();

        var timePeriods = dataSetVersion
            .TimePeriodMetas.Select(TimePeriodOptionViewModel.Create)
            .OrderBy(tm => tm.Code.GetEnumValue())
            .ThenBy(tm => tm.Period)
            .ToList();

        return new DataSetMetaViewModel
        {
            Filters = filters,
            Indicators = indicators,
            GeographicLevels = geographicLevels,
            Locations = locations,
            TimePeriods = timePeriods,
        };
    }

    private static FilterOptionsViewModel MapFilterOptions(FilterMeta filterMeta)
    {
        var options = filterMeta.OptionLinks.Select(MapFilterOption).OrderBy(fom => fom.Label).ToList();

        return new FilterOptionsViewModel
        {
            Id = filterMeta.PublicId,
            Column = filterMeta.Column,
            Hint = filterMeta.Hint,
            Label = filterMeta.Label,
            Options = options,
        };
    }

    private static FilterOptionViewModel MapFilterOption(FilterOptionMetaLink filterOptionMetaLink)
    {
        return new FilterOptionViewModel
        {
            Id = filterOptionMetaLink.PublicId,
            Label = filterOptionMetaLink.Option.Label,
        };
    }

    private static LocationGroupOptionsViewModel MapLocationGroupOptions(LocationMeta locationMeta)
    {
        var options = locationMeta
            .OptionLinks.Select(LocationOptionViewModel.Create)
            .OrderBy(lom => lom.Label)
            .ToList();

        return new LocationGroupOptionsViewModel
        {
            Level = GeographicLevelViewModel.Create(locationMeta.Level),
            Options = options,
        };
    }
}
