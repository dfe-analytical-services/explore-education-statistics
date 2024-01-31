using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
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

    public async Task<Either<ActionResult, PaginatedDataSetViewModel>> ListDataSets(
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
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync())
            .Select(MapDataSet)
            .ToList();

        return new PaginatedDataSetViewModel(dataSets, totalResults, page, pageSize);
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
            Number = $"{latestVersion.VersionMajor}.{latestVersion.VersionMinor}",
            Published = latestVersion.Published!.Value,
            TotalResults = latestVersion.TotalResults,
            TimePeriods = MapTimePeriods(latestVersion.MetaSummary.TimePeriodRange),
            GeographicLevels = latestVersion.MetaSummary.GeographicLevels,
            Filters = latestVersion.MetaSummary.Filters,
            Indicators = latestVersion.MetaSummary.Filters,
        };
    }

    private static TimePeriodRangeViewModel MapTimePeriods(TimePeriodRange timePeriodRange)
    {
        return new TimePeriodRangeViewModel
        {
            Start = TimePeriodLabelFormatter.FormatYear(timePeriodRange.Start.Year, timePeriodRange.Start.Code),
            End = TimePeriodLabelFormatter.FormatYear(timePeriodRange.End.Year, timePeriodRange.End.Code),
        };
    }
}
