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

    public async Task<Either<ActionResult, PermalinkCsvMetaViewModel>> GetCsvMeta(
        LegacyPermalink permalink,
        CancellationToken cancellationToken = default)
    {
        var releaseSubject = await _releaseSubjectService
            .FindForLatestPublishedVersion(subjectId: permalink.Query.SubjectId);

        var csvStream = releaseSubject is not null
            ? await GetCsvStream(releaseSubject, cancellationToken)
            : null;

        var subjectMeta = permalink.FullTable.SubjectMeta;

        var locations = await GetLocations(subjectMeta.LocationsHierarchical);

        var filters = subjectMeta.Filters.Values.ToDictionary(
            filter => filter.Name,
            filter => new FilterCsvMetaViewModel(filter)
        );
        var indicators = subjectMeta.Indicators.ToDictionary(
            indicator => indicator.Name,
            indicator => new IndicatorCsvMetaViewModel(indicator)
        );

        var headers = await ListCsvHeaders(csvStream, filters, indicators, locations);

        return new PermalinkCsvMetaViewModel
        {
            Locations = locations,
            Filters = filters,
            Indicators = indicators,
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
