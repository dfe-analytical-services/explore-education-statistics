using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

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
            .Where(ds => ds.Status == DataSetStatus.Published 
                || ds.Status == DataSetStatus.Deprecated);

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

    public async Task<Either<ActionResult, DataSetVersionPaginatedListViewModel>> ListVersions(
        int page,
        int pageSize,
        Guid dataSetId)
    {
        var queryable = _publicDataDbContext.DataSetVersions
            .Where(ds => ds.DataSetId == dataSetId)
            .Where(ds => ds.Status == DataSetVersionStatus.Published
                || ds.Status == DataSetVersionStatus.Unpublished
                || ds.Status == DataSetVersionStatus.Deprecated);

        var totalResults = await queryable.CountAsync();

        var dataSetVersions = (await queryable
            .OrderByDescending(ds => ds.VersionMajor)
            .ThenByDescending(ds => ds.VersionMinor)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync())
            .Select(MapDataSetVersion)
            .ToList();

        return new DataSetVersionPaginatedListViewModel(dataSetVersions, totalResults, page, pageSize);
    }

    private async Task<Either<ActionResult, DataSet>> CheckDataSetExists(Guid dataSetId)
    {
        return await _publicDataDbContext.DataSets
            .Include(ds => ds.LatestVersion)
            .Where(ds => ds.Id == dataSetId)
            .Where(ds => ds.Status == DataSetStatus.Published
                || ds.Status == DataSetStatus.Deprecated)
            .SingleOrNotFound();
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

    private static DataSetVersionViewModel MapDataSetVersion(DataSetVersion dataSetVersion)
    {
        return new DataSetVersionViewModel
        {
            Number = dataSetVersion.Version,
            Type = dataSetVersion.VersionType,
            Status = dataSetVersion.Status,
            Published = dataSetVersion.Published!.Value,
            Unpublished = dataSetVersion.Unpublished,
            Notes = dataSetVersion.Notes,
            TotalResults = dataSetVersion.TotalResults,
            TimePeriods = MapTimePeriods(dataSetVersion.MetaSummary.TimePeriodRange),
            GeographicLevels = dataSetVersion.MetaSummary.GeographicLevels,
            Filters = dataSetVersion.MetaSummary.Filters,
            Indicators = dataSetVersion.MetaSummary.Indicators,
        };
    }
}