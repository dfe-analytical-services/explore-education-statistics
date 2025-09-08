#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services;

public class SubjectCsvMetaService : ISubjectCsvMetaService
{
    private readonly ILogger<SubjectCsvMetaService> _logger;
    private readonly StatisticsDbContext _statisticsDbContext;
    private readonly ContentDbContext _contentDbContext;
    private readonly IUserService _userService;
    private readonly IFilterItemRepository _filterItemRepository;
    private readonly IReleaseFileBlobService _releaseFileBlobService;

    public SubjectCsvMetaService(
        ILogger<SubjectCsvMetaService> logger,
        StatisticsDbContext statisticsDbContext,
        ContentDbContext contentDbContext,
        IUserService userService,
        IFilterItemRepository filterItemRepository,
        IReleaseFileBlobService releaseFileBlobService
    )
    {
        _logger = logger;
        _statisticsDbContext = statisticsDbContext;
        _contentDbContext = contentDbContext;
        _userService = userService;
        _filterItemRepository = filterItemRepository;
        _releaseFileBlobService = releaseFileBlobService;
    }

    public async Task<Either<ActionResult, SubjectCsvMetaViewModel>> GetSubjectCsvMeta(
        ReleaseSubject releaseSubject,
        FullTableQuery query,
        IList<Observation> observations,
        CancellationToken cancellationToken = default
    )
    {
        return await _userService
            .CheckCanViewSubjectData(releaseSubject)
            .OnSuccess(() => GetCsvStream(releaseSubject, cancellationToken))
            .OnSuccess(async csvStream =>
            {
                var locations = GetLocations(observations);
                var filters = await GetFilters(observations);
                var indicators = await GetIndicators(query);
                var headers = csvStream is not null
                    ? await ListCsvHeaders(csvStream, filters, indicators)
                    : ListCsvHeaders(filters, indicators, locations);

                return new SubjectCsvMetaViewModel
                {
                    Filters = filters,
                    Locations = locations,
                    Indicators = indicators,
                    Headers = headers,
                };
            });
    }

    private async Task<Stream?> GetCsvStream(
        ReleaseSubject releaseSubject,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var releaseFile = await _contentDbContext
                .ReleaseFiles.Include(rf => rf.File)
                .SingleAsync(
                    predicate: rf =>
                        rf.File.SubjectId == releaseSubject.SubjectId
                        && rf.File.Type == FileType.Data
                        && rf.ReleaseVersionId == releaseSubject.ReleaseVersionId,
                    cancellationToken: cancellationToken
                );

            return (
                await _releaseFileBlobService.GetDownloadStream(releaseFile, cancellationToken: cancellationToken)
            ).Right;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                message: "Could not get file for release subject (ReleaseVersionId = {ReleaseVersionId}, SubjectId = {SubjectId})",
                releaseSubject.ReleaseVersionId,
                releaseSubject.SubjectId
            );

            return null;
        }
    }

    private static List<string> ListCsvHeaders(
        IDictionary<string, FilterCsvMetaViewModel> filters,
        IDictionary<string, IndicatorCsvMetaViewModel> indicators,
        IDictionary<Guid, Dictionary<string, string>> locations
    )
    {
        var filteredHeaders = new List<string> { "time_period", "time_identifier", "geographic_level" };

        var allLocationCols = LocationCsvUtils.AllCsvColumns();

        // Strip out any location columns that may be completely empty.
        var locationCols = locations
            .SelectMany(location => location.Value)
            .Where(attribute => !attribute.Value.IsNullOrEmpty())
            .Select(attribute => attribute.Key)
            .ToHashSet();

        var filterGroupCsvColumns = filters.Select(kvp => kvp.Value.GroupCsvColumn).WhereNotNull().ToList();

        filteredHeaders.AddRange(allLocationCols.Where(locationCols.Contains));
        filteredHeaders.AddRange(filterGroupCsvColumns);
        filteredHeaders.AddRange(filters.Keys);
        filteredHeaders.AddRange(indicators.Keys);

        return filteredHeaders;
    }

    private static async Task<List<string>> ListCsvHeaders(
        Stream csvStream,
        IDictionary<string, FilterCsvMetaViewModel> filters,
        IDictionary<string, IndicatorCsvMetaViewModel> indicators
    )
    {
        var filteredHeaders = new List<string> { "time_period", "time_identifier", "geographic_level" };

        var headers = await CsvUtils.GetCsvHeaders(csvStream);

        var locationCols = LocationCsvUtils.AllCsvColumns().ToHashSet();

        var filterGroupCsvColumns = filters.Select(kvp => kvp.Value.GroupCsvColumn).WhereNotNull().ToList();

        filteredHeaders.AddRange(headers.Where(locationCols.Contains));
        filteredHeaders.AddRange(headers.Where(filterGroupCsvColumns.Contains));
        filteredHeaders.AddRange(headers.Where(filters.ContainsKey));
        filteredHeaders.AddRange(headers.Where(indicators.ContainsKey));

        return filteredHeaders;
    }

    private static Dictionary<Guid, Dictionary<string, string>> GetLocations(IEnumerable<Observation> observations)
    {
        return observations
            .Select(observation => observation.Location)
            .Distinct()
            .ToDictionary(location => location.Id, location => location.GetCsvValues());
    }

    private async Task<Dictionary<string, FilterCsvMetaViewModel>> GetFilters(IEnumerable<Observation> observations)
    {
        var filterItems = await _filterItemRepository.GetFilterItemsFromObservations(observations);

        return FiltersMetaViewModelBuilder.BuildCsvFiltersFromFilterItems(filterItems);
    }

    private async Task<Dictionary<string, IndicatorCsvMetaViewModel>> GetIndicators(FullTableQuery query)
    {
        return await _statisticsDbContext
            .Indicator.AsNoTracking()
            .Where(indicator => query.Indicators.Contains(indicator.Id))
            .ToDictionaryAsync(indicator => indicator.Name, indicator => new IndicatorCsvMetaViewModel(indicator));
    }
}
