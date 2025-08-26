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
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
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
                        && rf.ReleaseVersionId == releaseSubject.ReleaseVersionId,
                    cancellationToken: cancellationToken
                );

            return await _releaseFileBlobService.StreamBlob(releaseFile, cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                message:
                "Could not get file for release subject (ReleaseVersionId = {ReleaseVersionId}, SubjectId = {SubjectId})",
                releaseSubject.ReleaseVersionId, releaseSubject.SubjectId);

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

        var filterGroupCsvColumns = filters
            .Select(kvp => kvp.Value.GroupCsvColumn)
            .WhereNotNull()
            .ToList();

        filteredHeaders.AddRange(allLocationCols.Where(locationCols.Contains));
        filteredHeaders.AddRange(filterGroupCsvColumns);
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

        var filterGroupCsvColumns = filters
            .Select(kvp => kvp.Value.GroupCsvColumn)
            .WhereNotNull()
            .ToList();

        filteredHeaders.AddRange(headers.Where(locationCols.Contains));
        filteredHeaders.AddRange(headers.Where(filterGroupCsvColumns.Contains));
        filteredHeaders.AddRange(headers.Where(filters.ContainsKey));
        filteredHeaders.AddRange(headers.Where(indicators.ContainsKey));

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
            GeographicLevel.LocalSkillsImprovementPlanArea =>
                new LocalSkillsImprovementPlanArea(viewModel.Value, viewModel.Label),
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

    private record LocationAttributePath(Guid Id, IList<LocationAttributeViewModel> Path);
}
