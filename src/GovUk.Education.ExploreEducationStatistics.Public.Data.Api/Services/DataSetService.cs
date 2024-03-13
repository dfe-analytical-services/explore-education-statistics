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

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

internal class DataSetService : IDataSetService
{
    private readonly PublicDataDbContext _publicDataDbContext;
    private readonly IUserService _userService;

    public DataSetService(
        PublicDataDbContext publicDataDbContext,
        IUserService userService)
    {
        _publicDataDbContext = publicDataDbContext;
        _userService = userService;
    }

    public async Task<Either<ActionResult, DataSetViewModel>> GetDataSet(Guid dataSetId)
    {
        return await _publicDataDbContext.DataSets
            .Include(ds => ds.LatestVersion)
            .SingleOrNotFound(ds => ds.Id == dataSetId)
            .OnSuccessDo(_userService.CheckCanViewDataSet)
            .OnSuccess(MapDataSet);
    }

    public async Task<Either<ActionResult, DataSetPaginatedListViewModel>> ListDataSets(
        int page,
        int pageSize,
        Guid publicationId)
    {
        var queryable = _publicDataDbContext.DataSets
            .Include(ds => ds.LatestVersion)
            .Where(ds => ds.PublicationId == publicationId)
            .WherePublicStatus();

        var totalResults = await queryable.CountAsync();

        var dataSets = (await queryable
            .OrderByDescending(ds => ds.LatestVersion!.Published)
            .ThenBy(ds => ds.Title)
            .ThenBy(ds => ds.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync())
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
        string dataSetVersion)
    {
        return await CheckVersionExists(dataSetId, dataSetVersion)
            .OnSuccessDo(_userService.CheckCanViewDataSetVersion)
            .OnSuccess(MapDataSetVersion);
    }

    public async Task<Either<ActionResult, DataSetVersionPaginatedListViewModel>> ListVersions(
        Guid dataSetId,
        int page,
        int pageSize)
    {
        return await _publicDataDbContext.DataSets
            .SingleOrNotFound(ds => ds.Id == dataSetId)
            .OnSuccessDo(_userService.CheckCanViewDataSet)
            .OnSuccess(dataSet => ListPaginatedVersions(dataSet: dataSet, page: page, pageSize: pageSize));
    }

    private async Task<Either<ActionResult, DataSetVersionPaginatedListViewModel>> ListPaginatedVersions(
        DataSet dataSet,
        int page,
        int pageSize)
    {
        var queryable = _publicDataDbContext.DataSetVersions
            .Where(ds => ds.DataSetId == dataSet.Id)
            .WherePublicStatus();

        var totalResults = await queryable.CountAsync();

        var dataSetVersions = (await queryable
                .OrderByDescending(dsv => dsv.VersionMajor)
                .ThenByDescending(dsv => dsv.VersionMinor)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync())
            .Select(MapDataSetVersion)
            .ToList();

        return new DataSetVersionPaginatedListViewModel
        {
            Results = dataSetVersions,
            Paging = new PagingViewModel(page: page, pageSize: pageSize, totalResults: totalResults)
        };
    }

    public async Task<Either<ActionResult, DataSetMetaViewModel>> GetMeta(Guid dataSetId, string? dataSetVersion = null)
    {
        return await GetVersionWithMeta(dataSetId, dataSetVersion)
            .OnSuccessDo(_userService.CheckCanViewDataSetVersion)
            .OnSuccess(MapVersionMeta);
    }

    private static DataSetViewModel MapDataSet(DataSet dataSet)
    {
        return new DataSetViewModel
        {
            Id = dataSet.Id,
            Title = dataSet.Title,
            Summary = dataSet.Summary,
            Status = dataSet.Status,
            LatestVersion = MapLatestVersion(dataSet.LatestVersion!),
            SupersedingDataSetId = dataSet.SupersedingDataSetId,
        };
    }

    private async Task<Either<ActionResult, DataSetVersion>> CheckVersionExists(
        Guid dataSetId,
        string dataSetVersion)
    {
        if (!VersionUtils.TryParse(dataSetVersion, out var version))
        {
            return new NotFoundResult();
        }

        return await _publicDataDbContext.DataSetVersions
            .Where(dsv => dsv.DataSetId == dataSetId)
            .Where(dsv => dsv.VersionMajor == version.Major)
            .Where(dsv => dsv.VersionMinor == version.Minor)
            .SingleOrNotFound();
    }

    private static DataSetLatestVersionViewModel MapLatestVersion(DataSetVersion latestVersion)
    {
        return new DataSetLatestVersionViewModel
        {
            Number = latestVersion.Version,
            Published = latestVersion.Published!.Value,
            TotalResults = latestVersion.TotalResults,
            TimePeriods = MapTimePeriods(latestVersion.MetaSummary.TimePeriodRange),
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
            Number = dataSetVersion.Version,
            Type = dataSetVersion.VersionType,
            Status = dataSetVersion.Status,
            Published = dataSetVersion.Published!.Value,
            Withdrawn = dataSetVersion.Withdrawn,
            Notes = dataSetVersion.Notes,
            TotalResults = dataSetVersion.TotalResults,
            TimePeriods = MapTimePeriods(dataSetVersion.MetaSummary.TimePeriodRange),
            GeographicLevels = dataSetVersion.MetaSummary.GeographicLevels,
            Filters = dataSetVersion.MetaSummary.Filters,
            Indicators = dataSetVersion.MetaSummary.Indicators,
        };
    }

    private async Task<Either<ActionResult, DataSetVersion>> GetVersionWithMeta(
        Guid dataSetId,
        string? dataSetVersion = null)
    {
        if (dataSetVersion is null)
        {
            return await GetVersionWithMeta(dataSetId)
                .OrderByDescending(dsv => dsv.VersionMajor)
                .ThenByDescending(dsv => dsv.VersionMinor)
                .FirstOrNotFound();
        }

        if (!VersionUtils.TryParse(dataSetVersion, out var version))
        {
            return new NotFoundResult();
        }

        return await GetVersionWithMeta(dataSetId)
            .Where(dsv => dsv.VersionMajor == version.Major)
            .Where(dsv => dsv.VersionMinor == version.Minor)
            .SingleOrNotFound();
    }

    private IQueryable<DataSetVersion> GetVersionWithMeta(Guid dataSetId)
    {
        return _publicDataDbContext.DataSetVersions
            .AsNoTracking()
            .Include(dsv => dsv.FilterMetas)
            .ThenInclude(fm => fm.Options)
            .ThenInclude(fom => fom.MetaLinks)
            .Include(dsv => dsv.LocationMetas)
            .ThenInclude(lm => lm.Options)
            .ThenInclude(lom => lom.MetaLinks)
            .Include(dsv => dsv.IndicatorMetas)
            .Include(dsv => dsv.TimePeriodMetas)
            .AsSplitQuery()
            .Where(dsv => dsv.DataSetId == dataSetId);
    }

    private DataSetMetaViewModel MapVersionMeta(DataSetVersion dataSetVersion)
    {
        return new DataSetMetaViewModel
        {
            Filters = dataSetVersion.FilterMetas.Select(MapFilterMeta).ToList(),
            Indicators = dataSetVersion.IndicatorMetas.Select(MapIndicatorMeta).ToList(),
            GeographicLevels = dataSetVersion.LocationMetas.Select(lm => lm.Level).ToList(),
            Locations = dataSetVersion.LocationMetas.Select(MapLocationMeta).ToList(),
            TimePeriods = dataSetVersion.TimePeriodMetas.Select(MapTimePeriod).ToList(),
        };
    }

    private static FilterMetaViewModel MapFilterMeta(FilterMeta filterMeta)
    {
        return new FilterMetaViewModel
        {
            Id = filterMeta.PublicId,
            Hint = filterMeta.Hint,
            Label = filterMeta.Label,
            Options = filterMeta.Options.Select(fom => MapFilterOptionMeta(filterMeta, fom)).ToList(),
        };
    }

    private static FilterOptionMetaViewModel MapFilterOptionMeta(
        FilterMeta filterMeta, 
        FilterOptionMeta filterOptionMeta)
    {
        var publicId = filterOptionMeta.MetaLinks.Single(ml => ml.MetaId == filterMeta.Id).PublicId;

        return new FilterOptionMetaViewModel
        {
            Id = SqidEncoder.Encode(publicId),
            Label = filterOptionMeta.Label,
            IsAggregate = filterOptionMeta.IsAggregate,
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
        return new LocationLevelMetaViewModel
        {
            Level = locationMeta.Level,
            Options = locationMeta.Options.Select(lom => MapLocationOptionMeta(locationMeta, lom)).ToList(),
        };
    }

    private static LocationOptionMetaViewModel MapLocationOptionMeta(
        LocationMeta locationMeta, 
        LocationOptionMeta locationOptionMeta)
    {
        var publicId = locationOptionMeta.MetaLinks.Single(ml => ml.MetaId == locationMeta.Id).PublicId;

        var encodedId = SqidEncoder.Encode(publicId);

        switch (locationOptionMeta)
        {
            case LocationCodedOptionMeta locationCodedOptionMeta:
                return new LocationCodedOptionMetaViewModel
                {
                    Id = encodedId,
                    Label = locationCodedOptionMeta.Label,
                    Code = locationCodedOptionMeta.Code,
                };
            case LocationLocalAuthorityOptionMeta locationLocalAuthorityOptionMeta:
                return new LocationLocalAuthorityOptionMetaViewModel
                {
                    Id = encodedId,
                    Label = locationLocalAuthorityOptionMeta.Label,
                    Code = locationLocalAuthorityOptionMeta.Code,
                    OldCode = locationLocalAuthorityOptionMeta.OldCode,
                };
            case LocationProviderOptionMeta locationProviderOptionMeta:
                return new LocationProviderOptionMetaViewModel
                {
                    Id = encodedId,
                    Label = locationProviderOptionMeta.Label,
                    Ukprn = locationProviderOptionMeta.Ukprn,
                };
            case LocationRscRegionOptionMeta locationRscRegionOptionMeta:
                return new LocationRscRegionOptionMetaViewModel
                {
                    Id = encodedId,
                    Label = locationRscRegionOptionMeta.Label,
                };
            case LocationSchoolOptionMeta locationSchoolOptionMeta:
                return new LocationSchoolOptionMetaViewModel
                {
                    Id = encodedId,
                    Label = locationSchoolOptionMeta.Label,
                    Urn = locationSchoolOptionMeta.Urn,
                    LaEstab = locationSchoolOptionMeta.LaEstab,
                };
            default:
                throw new NotImplementedException();
        }
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
