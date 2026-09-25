#nullable enable
using System.Collections.Immutable;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
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
    private readonly IStorageDataSetResolver _storageDataSetResolver;
    private readonly IReleaseSubjectService _releaseSubjectService;
    private readonly IReleaseFileBlobService _releaseFileBlobService;

    public PermalinkCsvMetaService(
        ILogger<PermalinkCsvMetaService> logger,
        ContentDbContext contentDbContext,
        IStorageDataSetResolver storageDataSetResolver,
        IReleaseSubjectService releaseSubjectService,
        IReleaseFileBlobService releaseFileBlobService
    )
    {
        _logger = logger;
        _contentDbContext = contentDbContext;
        _storageDataSetResolver = storageDataSetResolver;
        _releaseSubjectService = releaseSubjectService;
        _releaseFileBlobService = releaseFileBlobService;
    }

    public async Task<Either<ActionResult, PermalinkCsvMetaViewModel>> GetCsvMeta(
        Guid subjectId,
        SubjectResultMetaViewModel tableResultMeta,
        CancellationToken cancellationToken = default
    )
    {
        var releaseSubject = await _releaseSubjectService.FindForLatestPublishedVersion(subjectId: subjectId);

        var csvStream = releaseSubject is not null ? await GetCsvStream(releaseSubject, cancellationToken) : null;

        var dataSet = await _storageDataSetResolver.TryResolve(subjectId, cancellationToken);
        var locations = await GetLocations(dataSet, tableResultMeta.Locations, cancellationToken);

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
            : ListCsvHeaders(csvFilters, csvIndicators, locations);

        return new PermalinkCsvMetaViewModel
        {
            Locations = locations,
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

    private static async Task<Dictionary<Guid, Dictionary<string, string>>> GetLocations(
        IStorageDataSet? dataSet,
        Dictionary<string, List<LocationAttributeViewModel>> locationsHierarchy,
        CancellationToken cancellationToken
    )
    {
        var locationAttributePaths = locationsHierarchy
            .SelectMany(pair =>
                GetLocationAttributePaths(
                    attributes: pair.Value,
                    rootLevel: Enum.Parse<GeographicLevel>(pair.Key, ignoreCase: true)
                )
            )
            .ToList();

        var locationIds = locationAttributePaths.Select(location => location.Id).ToHashSet();

        if (dataSet is null)
        {
            return new Dictionary<Guid, Dictionary<string, string>>();
        }

        return (await dataSet.ListLocations(locationIds, cancellationToken)).ToDictionary(
            location => location.Id,
            location => location.GetCsvValues()
        );
    }

    private static List<LocationAttributePath> GetLocationAttributePaths(
        List<LocationAttributeViewModel> attributes,
        GeographicLevel rootLevel,
        List<LocationAttributePath>? leafPaths = null,
        ImmutableList<LocationAttributeViewModel>? currentPath = null
    )
    {
        leafPaths ??= new List<LocationAttributePath>();
        currentPath ??= new List<LocationAttributeViewModel>().ToImmutableList();

        foreach (var attribute in attributes)
        {
            var nextPath = currentPath.Add(attribute with { Level = attribute.Level ?? rootLevel });

            if (attribute.Options is null)
            {
                if (!attribute.Id.HasValue)
                {
                    throw new Exception($"Leaf attribute has missing property {nameof(attribute.Id)}");
                }

                // This is a leaf node, so we can terminate the path.
                leafPaths.Add(new LocationAttributePath(attribute.Id.Value, nextPath));
                continue;
            }

            GetLocationAttributePaths(attribute.Options, rootLevel, leafPaths, nextPath);
        }

        return leafPaths;
    }

    private record LocationAttributePath(Guid Id, IList<LocationAttributeViewModel> Path);
}
