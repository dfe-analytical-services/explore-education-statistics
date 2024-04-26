using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

internal class DataSetService(
    PublicDataDbContext publicDataDbContext,
    IUserService userService)
    : IDataSetService
{
    public async Task<Either<ActionResult, DataSetViewModel>> GetDataSet(
        Guid dataSetId,
        CancellationToken cancellationToken = default)
    {
        return await publicDataDbContext.DataSets
            .AsNoTracking()
            .Include(ds => ds.LatestLiveVersion)
            .SingleOrNotFoundAsync(ds => ds.Id == dataSetId, cancellationToken: cancellationToken)
            .OnSuccessDo(userService.CheckCanViewDataSet)
            .OnSuccess(MapDataSet);
    }

    public async Task<Either<ActionResult, DataSetPaginatedListViewModel>> ListDataSets(
        int page,
        int pageSize,
        Guid publicationId,
        CancellationToken cancellationToken = default)
    {
        var queryable = publicDataDbContext.DataSets
            .AsNoTracking()
            .Include(ds => ds.LatestLiveVersion)
            .Where(ds => ds.PublicationId == publicationId)
            .WherePublicStatus();

        var totalResults = await queryable.CountAsync(cancellationToken: cancellationToken);

        var dataSets = (await queryable
                .OrderByDescending(ds => ds.LatestLiveVersion!.Published)
                .ThenBy(ds => ds.Title)
                .ThenBy(ds => ds.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken: cancellationToken)
            )
            .Select(MapDataSet)
            .ToList();

        return new DataSetPaginatedListViewModel
        {
            Results = dataSets,
            Paging = new PagingViewModel(page: page, pageSize: pageSize, totalResults: totalResults)
        };
    }

    public async Task<Either<ActionResult, DataSetVersionViewModel>> GetVersion(
        Guid dataSetId,
        string dataSetVersion,
        CancellationToken cancellationToken = default)
    {
        return await CheckVersionExists(
                dataSetId: dataSetId,
                dataSetVersion: dataSetVersion,
                cancellationToken: cancellationToken)
            .OnSuccessDo(userService.CheckCanViewDataSetVersion)
            .OnSuccess(MapDataSetVersion);
    }

    public async Task<Either<ActionResult, DataSetVersionPaginatedListViewModel>> ListVersions(
        Guid dataSetId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await publicDataDbContext.DataSets
            .AsNoTracking()
            .SingleOrNotFoundAsync(ds => ds.Id == dataSetId, cancellationToken: cancellationToken)
            .OnSuccessDo(userService.CheckCanViewDataSet)
            .OnSuccess(dataSet => ListPaginatedVersions(
                dataSet: dataSet,
                page: page,
                pageSize: pageSize,
                cancellationToken: cancellationToken));
    }

    public async Task<Either<ActionResult, DataSetMetaViewModel>> GetMeta(
        Guid dataSetId,
        string? dataSetVersion = null,
        IReadOnlySet<DataSetMetaType>? types = null,
        CancellationToken cancellationToken = default)
    {
        return await FindVersion(
                dataSetId: dataSetId,
                dataSetVersion: dataSetVersion,
                cancellationToken: cancellationToken)
            .OnSuccessDo(userService.CheckCanViewDataSetVersion)
            .OnSuccessDo(dsv => LoadMeta(dsv, types, cancellationToken))
            .OnSuccess(MapVersionMeta);
    }

    private async Task<Either<ActionResult, DataSetVersionPaginatedListViewModel>> ListPaginatedVersions(
        DataSet dataSet,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var queryable = publicDataDbContext.DataSetVersions
            .AsNoTracking()
            .Where(ds => ds.DataSetId == dataSet.Id)
            .WherePublicStatus();

        var totalResults = await queryable.CountAsync(cancellationToken: cancellationToken);

        var dataSetVersions = (await queryable
                .OrderByDescending(dsv => dsv.VersionMajor)
                .ThenByDescending(dsv => dsv.VersionMinor)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken: cancellationToken)
            )
            .Select(MapDataSetVersion)
            .ToList();

        return new DataSetVersionPaginatedListViewModel
        {
            Results = dataSetVersions,
            Paging = new PagingViewModel(page: page, pageSize: pageSize, totalResults: totalResults)
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
            LatestVersion = MapLatestVersion(dataSet.LatestLiveVersion!),
            SupersedingDataSetId = dataSet.SupersedingDataSetId,
        };
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
            .Where(dsv => dsv.DataSetId == dataSetId)
            .Where(dsv => dsv.VersionMajor == version.Major)
            .Where(dsv => dsv.VersionMinor == version.Minor)
            .SingleOrNotFoundAsync(cancellationToken);
    }

    private static DataSetLatestVersionViewModel MapLatestVersion(DataSetVersion latestVersion)
    {
        return new DataSetLatestVersionViewModel
        {
            Version = latestVersion.Version,
            Published = latestVersion.Published!.Value,
            TotalResults = latestVersion.TotalResults,
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
            Version = dataSetVersion.Version,
            Type = dataSetVersion.VersionType,
            Status = dataSetVersion.Status,
            Published = dataSetVersion.Published!.Value,
            Withdrawn = dataSetVersion.Withdrawn,
            Notes = dataSetVersion.Notes,
            TotalResults = dataSetVersion.TotalResults,
            TimePeriods = MapTimePeriods(dataSetVersion.MetaSummary!.TimePeriodRange),
            GeographicLevels = dataSetVersion.MetaSummary.GeographicLevels,
            Filters = dataSetVersion.MetaSummary.Filters,
            Indicators = dataSetVersion.MetaSummary.Indicators,
        };
    }

    private async Task<Either<ActionResult, DataSetVersion>> FindVersion(
        Guid dataSetId,
        string? dataSetVersion = null,
        CancellationToken cancellationToken = default)
    {
        if (dataSetVersion is null)
        {
            return await publicDataDbContext.DataSets
                .AsNoTracking()
                .Include(ds => ds.LatestLiveVersion)
                .Where(ds => ds.Id == dataSetId)
                .Select(ds => ds.LatestLiveVersion!)
                .SingleOrNotFoundAsync(cancellationToken);
        }

        return await CheckVersionExists(
            dataSetId: dataSetId,
            dataSetVersion: dataSetVersion,
            cancellationToken: cancellationToken);
    }

    private async Task LoadMeta(
        DataSetVersion dataSetVersion,
        IReadOnlySet<DataSetMetaType>? types = null, 
        CancellationToken cancellationToken = default)
    {
        types = types.IsNullOrEmpty() ? EnumUtil.GetEnums<DataSetMetaType>().ToHashSet() : types!;

        if (types.Contains(DataSetMetaType.Filters))
        {
            dataSetVersion.FilterMetas = await publicDataDbContext.FilterMetas
                .AsNoTracking()
                .Where(fm => fm.DataSetVersionId == dataSetVersion.Id)
                .Include(fm => fm.OptionLinks)
                .ThenInclude(fom => fom.Option)
                .ToListAsync(cancellationToken: cancellationToken);
        }

        if (types.Contains(DataSetMetaType.Locations))
        {
            dataSetVersion.LocationMetas = await publicDataDbContext.LocationMetas
                .AsNoTracking()
                .Where(lm => lm.DataSetVersionId == dataSetVersion.Id)
                .Include(lm => lm.Options)
                .ToListAsync(cancellationToken: cancellationToken);
        }

        if (types.Contains(DataSetMetaType.Indicators))
        {
            dataSetVersion.IndicatorMetas = await publicDataDbContext.IndicatorMetas
                .AsNoTracking()
                .Where(lm => lm.DataSetVersionId == dataSetVersion.Id)
                .ToListAsync(cancellationToken: cancellationToken);
        }

        if (types.Contains(DataSetMetaType.TimePeriods))
        {
            dataSetVersion.TimePeriodMetas = await publicDataDbContext.TimePeriodMetas
                .AsNoTracking()
                .Where(lm => lm.DataSetVersionId == dataSetVersion.Id)
                .ToListAsync(cancellationToken: cancellationToken);
        }
    }

    private static DataSetMetaViewModel MapVersionMeta(DataSetVersion dataSetVersion)
    {
        var filters = dataSetVersion.FilterMetas
            .Select(MapFilterMeta)
            .OrderBy(fm => fm.Label)
            .ToList();

        var indicators = dataSetVersion.IndicatorMetas
            .Select(MapIndicatorMeta)
            .OrderBy(im => im.Label)
            .ToList();

        var geographicLevels = dataSetVersion.LocationMetas
            .Select(lm => lm.Level)
            .ToList();

        var locations = dataSetVersion.LocationMetas
            .Select(MapLocationMeta)
            .ToList();

        var timePeriods = dataSetVersion.TimePeriodMetas
            .Select(MapTimePeriod)
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

    private static FilterMetaViewModel MapFilterMeta(FilterMeta filterMeta)
    {
        var options = filterMeta.OptionLinks
            .Select(MapFilterOptionMeta)
            .OrderBy(fom => fom.Label)
            .ToList();

        return new FilterMetaViewModel
        {
            Id = filterMeta.PublicId,
            Hint = filterMeta.Hint,
            Label = filterMeta.Label,
            Options = options,
        };
    }

    private static FilterOptionMetaViewModel MapFilterOptionMeta(FilterOptionMetaLink filterOptionMetaLink)
    {
        return new FilterOptionMetaViewModel
        {
            Id = filterOptionMetaLink.PublicId,
            Label = filterOptionMetaLink.Option.Label,
            IsAggregate = filterOptionMetaLink.Option.IsAggregate,
        };
    }

    private static IndicatorMetaViewModel MapIndicatorMeta(IndicatorMeta indicatorMeta)
    {
        return new IndicatorMetaViewModel
        {
            Id = indicatorMeta.PublicId,
            Label = indicatorMeta.Label,
            Unit = indicatorMeta.Unit,
            DecimalPlaces = indicatorMeta.DecimalPlaces,
        };
    }

    private static LocationLevelMetaViewModel MapLocationMeta(LocationMeta locationMeta)
    {
        var options = locationMeta.Options
            .Select(MapLocationOptionMeta)
            .OrderBy(lom => lom.Label)
            .ToList();

        return new LocationLevelMetaViewModel
        {
            Level = locationMeta.Level,
            Options = options,
        };
    }

    private static LocationOptionMetaViewModel MapLocationOptionMeta(LocationOptionMeta locationOptionMeta)
    {
        return locationOptionMeta switch
        {
            LocationCodedOptionMeta codedOption => new LocationCodedOptionMetaViewModel
            {
                Id = codedOption.PublicId,
                Label = codedOption.Label,
                Code = codedOption.Code,
            },
            LocationLocalAuthorityOptionMeta localAuthorityOption => new LocationLocalAuthorityOptionMetaViewModel
            {
                Id = localAuthorityOption.PublicId,
                Label = localAuthorityOption.Label,
                Code = localAuthorityOption.Code,
                OldCode = localAuthorityOption.OldCode,
            },
            LocationProviderOptionMeta providerOption => new LocationProviderOptionMetaViewModel
            {
                Id = providerOption.PublicId,
                Label = providerOption.Label,
                Ukprn = providerOption.Ukprn,
            },
            LocationRscRegionOptionMeta rscRegionOption => new LocationRscRegionOptionMetaViewModel
            {
                Id = rscRegionOption.PublicId,
                Label = rscRegionOption.Label,
            },
            LocationSchoolOptionMeta schoolOption => new LocationSchoolOptionMetaViewModel
            {
                Id = schoolOption.PublicId,
                Label = schoolOption.Label,
                Urn = schoolOption.Urn,
                LaEstab = schoolOption.LaEstab,
            },
            _ => throw new NotImplementedException()
        };
    }

    private static TimePeriodMetaViewModel MapTimePeriod(TimePeriodMeta timePeriodMeta)
    {
        return new TimePeriodMetaViewModel
        {
            Code = timePeriodMeta.Code,
            Period = timePeriodMeta.Period,
            Label = TimePeriodFormatter.FormatLabel(timePeriodMeta.Period, timePeriodMeta.Code),
        };
    }
}
