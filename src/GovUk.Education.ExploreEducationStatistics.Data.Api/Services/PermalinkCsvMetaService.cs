#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services;

public class PermalinkCsvMetaService : IPermalinkCsvMetaService
{
    private readonly ILogger<PermalinkCsvMetaService> _logger;
    private readonly ContentDbContext _contentDbContext;
    private readonly IReleaseSubjectService _releaseSubjectService;
    private readonly IReleaseFileBlobService _releaseFileBlobService;

    public PermalinkCsvMetaService(
        ILogger<PermalinkCsvMetaService> logger,
        ContentDbContext contentDbContext,
        IReleaseSubjectService releaseSubjectService,
        IReleaseFileBlobService releaseFileBlobService
    )
    {
        _logger = logger;
        _contentDbContext = contentDbContext;
        _releaseSubjectService = releaseSubjectService;
        _releaseFileBlobService = releaseFileBlobService;
    }

    public async Task<Either<ActionResult, PermalinkCsvMetaViewModel>> GetCsvMeta(
        Guid subjectId,
        SubjectResultMetaViewModel tableResultMeta,
        IReadOnlyList<Location> locations,
        CancellationToken cancellationToken = default
    )
    {
        var releaseSubject = await _releaseSubjectService.FindForLatestPublishedVersion(subjectId: subjectId);

        var csvStream = releaseSubject is not null ? await GetCsvStream(releaseSubject, cancellationToken) : null;

        var csvLocations = locations.ToDictionary(location => location.Id, location => location.GetCsvValues());

        var csvFilters = tableResultMeta.Filters.Values.ToDictionary(
            filter => filter.Name,
            filter => new FilterCsvMetaViewModel(filter)
        );

        var csvIndicators = tableResultMeta.Indicators.ToDictionary(
            indicator => indicator.Name,
            indicator => new IndicatorCsvMetaViewModel(indicator)
        );

        // Prefer using the data file stream over the table meta locations to generate the headers
        var headers = csvStream is not null
            ? await ListCsvHeaders(csvStream, csvFilters, csvIndicators)
            : ListCsvHeaders(csvFilters, csvIndicators, csvLocations);

        return new PermalinkCsvMetaViewModel
        {
            Locations = csvLocations,
            Filters = csvFilters,
            Indicators = csvIndicators,
            Headers = headers,
        };
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
}
