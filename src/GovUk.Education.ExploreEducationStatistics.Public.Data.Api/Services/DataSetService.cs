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
                .OrderByDescending(ds => ds.VersionMajor)
                .ThenByDescending(ds => ds.VersionMinor)
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
            Unpublished = dataSetVersion.Withdrawn,
            Notes = dataSetVersion.Notes,
            TotalResults = dataSetVersion.TotalResults,
            TimePeriods = MapTimePeriods(dataSetVersion.MetaSummary.TimePeriodRange),
            GeographicLevels = dataSetVersion.MetaSummary.GeographicLevels,
            Filters = dataSetVersion.MetaSummary.Filters,
            Indicators = dataSetVersion.MetaSummary.Indicators,
        };
    }
}
