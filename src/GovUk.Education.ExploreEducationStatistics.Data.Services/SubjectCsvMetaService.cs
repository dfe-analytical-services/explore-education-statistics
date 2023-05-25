#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
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
        IReleaseFileBlobService releaseFileBlobService)
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
        ObservationQueryContext query,
        IList<Observation> observations,
        CancellationToken cancellationToken = default)
    {
        return await _userService.CheckCanViewSubjectData(releaseSubject)
            .OnSuccess(() => GetCsvStream(releaseSubject, cancellationToken))
            .OnSuccess(
                async csvStream =>
                {
                    var locations = GetLocations(observations);
                    var filters = await GetFilters(observations);
                    var indicators = await GetIndicators(query);
                    var headers = await ListCsvHeaders(csvStream, filters, indicators, locations);

                    return new SubjectCsvMetaViewModel
                    {
                        Filters = filters,
                        Locations = locations,
                        Indicators = indicators,
                        Headers = headers
                    };
                }
            );
    }

    private async Task<Stream?> GetCsvStream(
        ReleaseSubject releaseSubject,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var releaseFile = await _contentDbContext.ReleaseFiles
                .Include(rf => rf.File)
                .SingleAsync(
                    predicate: rf =>
                        rf.File.SubjectId == releaseSubject.SubjectId
                        && rf.ReleaseId == releaseSubject.ReleaseId,
                    cancellationToken: cancellationToken
                );

            return await _releaseFileBlobService.StreamBlob(releaseFile, cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                message: "Could not get file for release subject (ReleaseId = {ReleaseId}, SubjectId = {SubjectId})",
                releaseSubject.ReleaseId, releaseSubject.Subject);

            return null;
        }
    }

    private static async Task<List<string>> ListCsvHeaders(
        Stream? csvStream,
        IDictionary<string, FilterCsvMetaViewModel> filters,
        IDictionary<string, IndicatorCsvMetaViewModel> indicators,
        IDictionary<Guid, Dictionary<string, string>> locations)
    {
        var filteredHeaders = new List<string>
        {
            "time_period",
            "time_identifier",
            "geographic_level"
        };

        if (csvStream is null)
        {
            var allLocationCols = LocationCsvUtils.AllCsvColumns();

            // Strip out any location columns that may be completely empty.
            var locationCols = locations
                .SelectMany(location => location.Value)
                .Where(attribute => !attribute.Value.IsNullOrEmpty())
                .Select(attribute => attribute.Key)
                .ToHashSet();

            filteredHeaders.AddRange(allLocationCols.Where(locationCols.Contains));
            filteredHeaders.AddRange(filters.Keys);
            filteredHeaders.AddRange(indicators.Keys);
        }
        else
        {
            var headers = await CsvUtils.GetCsvHeaders(csvStream);

            var locationCols = LocationCsvUtils.AllCsvColumns().ToHashSet();

            filteredHeaders.AddRange(headers.Where(locationCols.Contains));
            filteredHeaders.AddRange(headers.Where(filters.ContainsKey));
            filteredHeaders.AddRange(headers.Where(indicators.ContainsKey));
        }

        return filteredHeaders;
    }

    private static Dictionary<Guid, Dictionary<string, string>> GetLocations(
        IEnumerable<Observation> observations)
    {
        return observations
            .Select(observation => observation.Location)
            .Distinct()
            .ToDictionary(
                location => location.Id,
                location => location.GetCsvValues()
            );
    }

    private async Task<Dictionary<string, FilterCsvMetaViewModel>> GetFilters(
        IEnumerable<Observation> observations)
    {
        var filterItems = await _filterItemRepository
            .GetFilterItemsFromObservations(observations);

        return FiltersMetaViewModelBuilder.BuildCsvFiltersFromFilterItems(filterItems);
    }

    private async Task<Dictionary<string, IndicatorCsvMetaViewModel>> GetIndicators(
        ObservationQueryContext query)
    {
        return await _statisticsDbContext.Indicator
            .AsNoTracking()
            .Where(indicator => query.Indicators.Contains(indicator.Id))
            .ToDictionaryAsync(
                indicator => indicator.Name,
                indicator => new IndicatorCsvMetaViewModel(indicator)
            );
    }
}
