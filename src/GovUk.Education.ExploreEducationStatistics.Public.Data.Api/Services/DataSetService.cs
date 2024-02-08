using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

internal class DataSetService : IDataSetService
{
    private readonly PublicDataDbContext _publicDataDbContext;

    public DataSetService(PublicDataDbContext publicDataDbContext)
    {
        _publicDataDbContext = publicDataDbContext;
    }

    public async Task<Either<ActionResult, DataSetViewModel>> GetDataSet(Guid dataSetId)
    {
        return await CheckDataSetExists(dataSetId)
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
            .Where(ds => ds.Status == DataSetStatus.Published);

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

        return new DataSetPaginatedListViewModel(dataSets, totalResults, page, pageSize);
    }

    private async Task<Either<ActionResult, DataSet>> CheckDataSetExists(Guid dataSetId)
    {
        var dataSet = await _publicDataDbContext.DataSets
            .Include(ds => ds.LatestVersion)
            .Where(ds => ds.Id == dataSetId)
            .Where(ds => ds.Status == DataSetStatus.Published)
            .SingleOrDefaultAsync();

        return dataSet is null
            ? new NotFoundResult()
            : dataSet;
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
            Start = TimePeriodFormatter.Format(timePeriodRange.Start.Year, timePeriodRange.Start.Code),
            End = TimePeriodFormatter.Format(timePeriodRange.End.Year, timePeriodRange.End.Code),
        };
    }
}
