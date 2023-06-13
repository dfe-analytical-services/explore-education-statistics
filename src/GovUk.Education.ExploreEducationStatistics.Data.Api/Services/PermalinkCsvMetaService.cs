#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services;

public class PermalinkCsvMetaService : IPermalinkCsvMetaService
{
    private readonly ILogger<PermalinkCsvMetaService> _logger;
    private readonly ContentDbContext _contentDbContext;
    private readonly StatisticsDbContext _statisticsDbContext;
    private readonly IReleaseSubjectService _releaseSubjectService;
    private readonly IReleaseFileBlobService _releaseFileBlobService;

    public PermalinkCsvMetaService(
        ILogger<PermalinkCsvMetaService> logger,
        ContentDbContext contentDbContext,
        StatisticsDbContext statisticsDbContext,
        IReleaseSubjectService releaseSubjectService,
        IReleaseFileBlobService releaseFileBlobService)
    {
        _logger = logger;
        _contentDbContext = contentDbContext;
        _statisticsDbContext = statisticsDbContext;
        _releaseSubjectService = releaseSubjectService;
        _releaseFileBlobService = releaseFileBlobService;
    }

    // TODO EES-3755 Remove after Permalink snapshot work is complete
    public async Task<Either<ActionResult, PermalinkCsvMetaViewModel>> GetCsvMeta(
        LegacyPermalink permalink,
        CancellationToken cancellationToken = default)
    {
        // The method of generating the csv is conditional on whether location id's are present in the permalink.
        // A change made to add location id's to the observations took place first in EES-3203 followed by a change to
        // add id's to the subject meta locations in EES-2955. These changes happened in Feb/March 2022.

        var observationsHaveLocationIds =
            permalink.FullTable.Results[0].LocationId !=
            Guid.Empty; // LocationId is not nullable but it is possible for the value to be missing

        var tableSubjectMetaLocationsHaveIds = LocationAttributeHasLeafNodeWithLocationId(
            permalink.FullTable.SubjectMeta.LocationsHierarchical.First().Value.First());

        var hasLocationIds = observationsHaveLocationIds && tableSubjectMetaLocationsHaveIds;
        if (hasLocationIds)
        {
            // This method will prefer to use locations looked up from the database by location id because they are
            // complete in terms of the attributes they contain from the original data set, compared to the locations
            // hierarchies present in the table subject meta.

            // It has a fallback to use the subject meta locations when they aren't found in the database.

            // Locations are returned here in the csv meta keyed by location id for the purpose of writing the csv
            // where they will be looked up using the location id's found in each observation.
            return await GetCsvMeta(permalink.Query.SubjectId,
                permalink.FullTable.SubjectMeta.AsSubjectResultMetaViewModel(),
                cancellationToken);
        }

        // Permalinks without location id's need to be handled with a workaround.
        // Writing the csv will use the location objects from each of the observations and ignore
        // the table subject meta locations completely.

        // A benefit to doing this is that we can generate a complete set of location attributes from the original
        // data set because historically all the location values were encoded into each observation.

        // If we were to use the table subject meta locations we would only get the location attributes that were
        // relevant to the result.

        // For example, recognising that an observation is for a local authority and pairing it with a local authority
        // in the table subject meta wouldn't give us any more attributes beyond the local authority,
        // or at best the local authority and region if generated after the LA-region location hierarchy was introduced.
        // By using the location object within the observation we will also get the country.

        var tableResultMeta = permalink.FullTable.SubjectMeta;
        var releaseSubject = await _releaseSubjectService.FindForLatestPublishedVersion(permalink.Query.SubjectId);

        var csvStream = releaseSubject is not null
            ? await GetCsvStream(releaseSubject, cancellationToken)
            : null;

        var locationCols = GetLocationsColumns(permalink.FullTable.Results);

        var csvFilters = tableResultMeta.Filters.Values.ToDictionary(
            filter => filter.Name ?? filter.Legend.SnakeCase(),
            filter => new FilterCsvMetaViewModel(filter)
        );

        var csvIndicators = tableResultMeta.Indicators.ToDictionary(
            indicator => indicator.Name ?? indicator.Label.SnakeCase(),
            indicator => new IndicatorCsvMetaViewModel(indicator)
        );

        // Prefer using the data file stream over the observations locations to generate the headers
        var headers = csvStream is not null
            ? await ListCsvHeaders(csvStream, csvFilters, csvIndicators)
            : ListCsvHeaders(csvFilters, csvIndicators, locationCols);

        return new PermalinkCsvMetaViewModel
        {
            Filters = csvFilters,
            Indicators = csvIndicators,
            Headers = headers
            // Locations are ignored here because they will be converted from each observation when writing the csv
        };
    }

    public async Task<Either<ActionResult, PermalinkCsvMetaViewModel>> GetCsvMeta(
        Guid subjectId,
        SubjectResultMetaViewModel tableResultMeta,
        CancellationToken cancellationToken = default)
    {
        var releaseSubject = await _releaseSubjectService
            .FindForLatestPublishedVersion(subjectId: subjectId);

        var csvStream = releaseSubject is not null
            ? await GetCsvStream(releaseSubject, cancellationToken)
            : null;

        var locations = await GetLocations(tableResultMeta.Locations);

        var csvFilters = tableResultMeta.Filters.Values.ToDictionary(
            // TODO EES-3755 Remove fallback after Permalink snapshot work is complete
            filter => filter.Name ?? filter.Legend.SnakeCase(),
            filter => new FilterCsvMetaViewModel(filter)
        );

        var csvIndicators = tableResultMeta.Indicators.ToDictionary(
            // TODO EES-3755 Remove fallback after Permalink snapshot work is complete
            indicator => indicator.Name ?? indicator.Label.SnakeCase(),
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
            Headers = headers
        };
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
                        && rf.File.Type == FileType.Data
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
                releaseSubject.ReleaseId, releaseSubject.SubjectId);

            return null;
        }
    }

    private static List<string> ListCsvHeaders(
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

        return filteredHeaders;
    }

    private static async Task<List<string>> ListCsvHeaders(
        Stream csvStream,
        IDictionary<string, FilterCsvMetaViewModel> filters,
        IDictionary<string, IndicatorCsvMetaViewModel> indicators)
    {
        var filteredHeaders = new List<string>
        {
            "time_period",
            "time_identifier",
            "geographic_level"
        };

        var headers = await CsvUtils.GetCsvHeaders(csvStream);

        var locationCols = LocationCsvUtils.AllCsvColumns().ToHashSet();

        filteredHeaders.AddRange(headers.Where(locationCols.Contains));
        filteredHeaders.AddRange(headers.Where(filters.ContainsKey));
        filteredHeaders.AddRange(headers.Where(indicators.ContainsKey));

        return filteredHeaders;
    }

    private static List<string> ListCsvHeaders(
        IDictionary<string, FilterCsvMetaViewModel> filters,
        IDictionary<string, IndicatorCsvMetaViewModel> indicators,
        IReadOnlySet<string> locationCols)
    {
        var filteredHeaders = new List<string>
        {
            "time_period",
            "time_identifier",
            "geographic_level"
        };

        var allLocationCols = LocationCsvUtils.AllCsvColumns();

        filteredHeaders.AddRange(allLocationCols.Where(locationCols.Contains));
        filteredHeaders.AddRange(filters.Keys);
        filteredHeaders.AddRange(indicators.Keys);

        return filteredHeaders;
    }

    private async Task<Dictionary<Guid, Dictionary<string, string>>> GetLocations(
        Dictionary<string, List<LocationAttributeViewModel>> locationsHierarchy)
    {
        var locationAttributePaths = locationsHierarchy
            .SelectMany(pair => GetLocationAttributePaths(
                attributes: pair.Value,
                rootLevel: Enum.Parse<GeographicLevel>(pair.Key, ignoreCase: true)
            ))
            .ToList();

        var locationIds = locationAttributePaths
            .Select(location => location.Id)
            .ToHashSet();

        // If possible, use the locations from the database as these contain
        // all the attributes from when it was originally created.
        // Locations in the database are immutable, but they may have been deleted
        // if they have been orphaned from any subject for too long.
        var locations = await _statisticsDbContext.Location
            .Where(location => locationIds.Contains(location.Id))
            .ToDictionaryAsync(location => location.Id);

        // For any locations that no longer exist in the database, fallback to using the
        // permalink's location hierarchy metadata. It should be noted that this meta
        // doesn't provide the full set of location attributes so the location will
        // most likely be missing columns that existed in the original CSV.
        return locationAttributePaths
            .ToDictionary(
                location => location.Id,
                location => locations.ContainsKey(location.Id)
                    ? locations[location.Id].GetCsvValues()
                    : location.Path
                        .SelectMany(node => ParseLocationAttribute(node).CsvValues)
                        .ToDictionary(colValue => colValue.Key, colValue => colValue.Value)
            );
    }

    private static IReadOnlySet<string> GetLocationsColumns(List<ObservationViewModel> observations)
    {
        return observations
            .Select(observation =>
            {
                // We've gone down this path because we detected the permalink has no location id's.
                // Instead of a location id we expect there to be a location object.
                // This existed historically before the location id's was added to ObservationViewModel.
                if (observation.Location == null)
                {
                    throw new InvalidOperationException("Observation has no location");
                }

                return observation.Location;
            })
            .Distinct()
            .SelectMany(locationViewModel => locationViewModel.GetCsvValues())
            .Where(pair => !pair.Value.IsNullOrEmpty())
            .Select(pair => pair.Key)
            .ToHashSet();
    }

    private static List<LocationAttributePath> GetLocationAttributePaths(
        List<LocationAttributeViewModel> attributes,
        GeographicLevel rootLevel,
        List<LocationAttributePath>? leafPaths = null,
        ImmutableList<LocationAttributeViewModel>? currentPath = null)
    {
        leafPaths ??= new List<LocationAttributePath>();
        currentPath ??= new List<LocationAttributeViewModel>().ToImmutableList();

        foreach (var attribute in attributes)
        {
            var nextPath = currentPath.Add(attribute with
            {
                Level = attribute.Level ?? rootLevel
            });

            if (attribute.Options is null)
            {
                if (!attribute.Id.HasValue)
                {
                    throw new Exception(
                        $"Leaf attribute has missing property {nameof(attribute.Id)}"
                    );
                }

                // This is a leaf node, so we can terminate the path.
                leafPaths.Add(new LocationAttributePath(attribute.Id.Value, nextPath));
                continue;
            }

            GetLocationAttributePaths(attribute.Options, rootLevel, leafPaths, nextPath);
        }

        return leafPaths;
    }

    private static LocationAttribute ParseLocationAttribute(LocationAttributeViewModel viewModel)
    {
        return viewModel.Level switch
        {
            GeographicLevel.Country =>
                new Country(viewModel.Value, viewModel.Label),
            GeographicLevel.EnglishDevolvedArea =>
                new EnglishDevolvedArea(viewModel.Value, viewModel.Label),
            GeographicLevel.LocalAuthority =>
                new LocalAuthority(viewModel.Value, null, viewModel.Label),
            GeographicLevel.LocalAuthorityDistrict =>
                new LocalAuthorityDistrict(viewModel.Value, viewModel.Label),
            GeographicLevel.LocalEnterprisePartnership =>
                new LocalEnterprisePartnership(viewModel.Value, viewModel.Label),
            GeographicLevel.Institution =>
                new Institution(viewModel.Value, viewModel.Label),
            GeographicLevel.MayoralCombinedAuthority =>
                new MayoralCombinedAuthority(viewModel.Value, viewModel.Label),
            GeographicLevel.MultiAcademyTrust =>
                new MultiAcademyTrust(viewModel.Value, viewModel.Label),
            GeographicLevel.OpportunityArea =>
                new OpportunityArea(viewModel.Value, viewModel.Label),
            GeographicLevel.ParliamentaryConstituency =>
                new ParliamentaryConstituency(viewModel.Value, viewModel.Label),
            GeographicLevel.PlanningArea =>
                new PlanningArea(viewModel.Value, viewModel.Label),
            GeographicLevel.Provider =>
                new Provider(viewModel.Value, viewModel.Label),
            GeographicLevel.Region =>
                new Region(viewModel.Value, viewModel.Label),
            GeographicLevel.RscRegion =>
                new RscRegion(viewModel.Value),
            GeographicLevel.School =>
                new School(viewModel.Value, viewModel.Label),
            GeographicLevel.Sponsor =>
                new Sponsor(viewModel.Value, viewModel.Label),
            GeographicLevel.Ward =>
                new Ward(viewModel.Value, viewModel.Label),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static bool LocationAttributeHasLeafNodeWithLocationId(LocationAttributeViewModel attribute)
    {
        if (attribute.Options is null)
        {
            // This is a leaf node
            return attribute.Id.HasValue;
        }

        // Check the first child node recursively until we find a leaf node
        return LocationAttributeHasLeafNodeWithLocationId(attribute.Options[0]);
    }

    private record LocationAttributePath(Guid Id, IList<LocationAttributeViewModel> Path);
}
