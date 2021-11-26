#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.Security.DataSecurityPolicies;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class ResultSubjectMetaService : AbstractSubjectMetaService, IResultSubjectMetaService
    {
        private readonly IFeatureManager _featureManager;
        private readonly ContentDbContext _contentDbContext;
        private readonly IBoundaryLevelRepository _boundaryLevelRepository;
        private readonly IFootnoteRepository _footnoteRepository;
        private readonly IGeoJsonRepository _geoJsonRepository;
        private readonly IIndicatorRepository _indicatorRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IPersistenceHelper<StatisticsDbContext> _persistenceHelper;
        private readonly ITimePeriodService _timePeriodService;
        private readonly IUserService _userService;
        private readonly ISubjectRepository _subjectRepository;
        private readonly IReleaseDataFileRepository _releaseDataFileRepository;
        private readonly LocationsOptions _locationOptions;
        private readonly ILogger _logger;

        public ResultSubjectMetaService(
            IFeatureManager featureManager,
            ContentDbContext contentDbContext,
            IFilterItemRepository filterItemRepository,
            IBoundaryLevelRepository boundaryLevelRepository,
            IFootnoteRepository footnoteRepository,
            IGeoJsonRepository geoJsonRepository,
            IIndicatorRepository indicatorRepository,
            ILocationRepository locationRepository,
            IPersistenceHelper<StatisticsDbContext> persistenceHelper,
            ITimePeriodService timePeriodService,
            IUserService userService,
            ISubjectRepository subjectRepository,
            IReleaseDataFileRepository releaseDataFileRepository,
            IOptions<LocationsOptions> locationOptions,
            ILogger<ResultSubjectMetaService> logger) : base(filterItemRepository)
        {
            _featureManager = featureManager;
            _contentDbContext = contentDbContext;
            _boundaryLevelRepository = boundaryLevelRepository;
            _footnoteRepository = footnoteRepository;
            _geoJsonRepository = geoJsonRepository;
            _indicatorRepository = indicatorRepository;
            _locationRepository = locationRepository;
            _persistenceHelper = persistenceHelper;
            _timePeriodService = timePeriodService;
            _userService = userService;
            _subjectRepository = subjectRepository;
            _releaseDataFileRepository = releaseDataFileRepository;
            _locationOptions = locationOptions.Value;
            _logger = logger;
        }

        public async Task<Either<ActionResult, ResultSubjectMetaViewModel>> GetSubjectMeta(
            Guid releaseId,
            SubjectMetaQueryContext query,
            IQueryable<Observation> observations)
        {
            return await _persistenceHelper.CheckEntityExists<Subject>(query.SubjectId)
                .OnSuccess(CheckCanViewSubjectData)
                .OnSuccess(async subject =>
                {
                    var stopwatch = Stopwatch.StartNew();
                    stopwatch.Start();

                    // TODO EES-2902 Remove the location hierarchies feature toggle after EES-2777
                    var locationHierarchiesEnabled = await _featureManager.IsEnabledAsync("LocationHierarchies");

                    // Uses the new GetLocationAttributesHierarchical to get the locations regardless of whether the
                    // feature is enabled or not.  If the feature is disabled, requests the locations without a hierarchy.
                    var locationAttributes = await _locationRepository.GetLocationAttributesHierarchical(
                        observations,
                        hierarchies: locationHierarchiesEnabled ? _locationOptions.Hierarchies : null);
                    _logger.LogTrace("Got Location attributes in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var filterViewModels = GetFilters(query.SubjectId, observations, true);
                    _logger.LogTrace("Got Filters in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var footnoteViewModels = GetFilteredFootnoteViewModels(releaseId, observations, query);
                    _logger.LogTrace("Got Footnotes in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var indicatorViewModels = GetIndicatorViewModels(query);
                    _logger.LogTrace("Got Indicators in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var timePeriodViewModels = GetTimePeriodViewModels(observations);
                    _logger.LogTrace("Got Time Periods in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var publicationId = await _subjectRepository.GetPublicationIdForSubject(subject.Id);
                    var publicationTitle = (await _contentDbContext.Publications.FindAsync(publicationId)).Title;

                    var releaseFile = await _releaseDataFileRepository.GetBySubject(releaseId, subject.Id);
                    var subjectName = releaseFile.Name ?? "";

                    var locationsHelper =
                        new LocationsQueryHelper(locationAttributes, query, _boundaryLevelRepository,
                            _geoJsonRepository);

                    if (locationHierarchiesEnabled)
                    {
                        var locationViewModels = locationsHelper.GetLocationViewModels();
                        _logger.LogTrace("Got Location view models in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                        stopwatch.Stop();

                        return new ResultSubjectMetaViewModel
                        {
                            Filters = filterViewModels,
                            Footnotes = footnoteViewModels,
                            GeoJsonAvailable = locationsHelper.GeoJsonAvailable,
                            Indicators = indicatorViewModels,
                            LocationsHierarchical = locationViewModels,
                            BoundaryLevels = locationsHelper.GetBoundaryLevelViewModels(),
                            PublicationName = publicationTitle,
                            SubjectName = subjectName,
                            TimePeriodRange = timePeriodViewModels
                        };
                    }
                    else
                    {
                        var locationViewModels = locationsHelper.GetLegacyLocationViewModels();
                        _logger.LogTrace("Got Location view models in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                        stopwatch.Stop();

                        return new ResultSubjectMetaViewModel
                        {
                            Filters = filterViewModels,
                            Footnotes = footnoteViewModels,
                            GeoJsonAvailable = locationsHelper.GeoJsonAvailable,
                            Indicators = indicatorViewModels,
                            Locations = locationViewModels,
                            BoundaryLevels = locationsHelper.GetBoundaryLevelViewModels(),
                            PublicationName = publicationTitle,
                            SubjectName = subjectName,
                            TimePeriodRange = timePeriodViewModels
                        };
                    }
                });
        }

        private async Task<Either<ActionResult, Subject>> CheckCanViewSubjectData(Subject subject)
        {
            if (await _userService.MatchesPolicy(subject, CanViewSubjectData))
            {
                return subject;
            }

            return new ForbidResult();
        }

        private List<IndicatorMetaViewModel> GetIndicatorViewModels(SubjectMetaQueryContext query)
        {
            var indicators = _indicatorRepository.GetIndicators(query.SubjectId, query.Indicators);
            return BuildIndicatorViewModels(indicators);
        }

        private List<FootnoteViewModel> GetFilteredFootnoteViewModels(
            Guid releaseId,
            IQueryable<Observation> observations,
            SubjectMetaQueryContext queryContext)
        {
            return _footnoteRepository
                .GetFilteredFootnotes(releaseId, queryContext.SubjectId, observations, queryContext.Indicators)
                .Select(footnote => new FootnoteViewModel
                {
                    Id = footnote.Id,
                    Label = footnote.Content
                })
                .ToList();
        }

        private List<TimePeriodMetaViewModel> GetTimePeriodViewModels(IQueryable<Observation> observations)
        {
            return _timePeriodService.GetTimePeriodRange(observations)
                .Select(tuple => new TimePeriodMetaViewModel(tuple.Year, tuple.TimeIdentifier))
                .ToList();
        }

        private class LocationsQueryHelper
        {
            private readonly Dictionary<GeographicLevel, List<LocationAttributeNode>> _locationAttributes;
            private readonly SubjectMetaQueryContext _query;
            private readonly IBoundaryLevelRepository _boundaryLevelRepository;
            private readonly IGeoJsonRepository _geoJsonRepository;
            private readonly List<GeographicLevel> _geographicLevels;

            internal LocationsQueryHelper(
                Dictionary<GeographicLevel, List<LocationAttributeNode>> locationAttributes,
                SubjectMetaQueryContext query,
                IBoundaryLevelRepository boundaryLevelRepository,
                IGeoJsonRepository geoJsonRepository)
            {
                _locationAttributes = locationAttributes;
                _query = query;
                _boundaryLevelRepository = boundaryLevelRepository;
                _geoJsonRepository = geoJsonRepository;
                _geographicLevels = locationAttributes.Keys.ToList();
            }

            public bool GeoJsonAvailable => _geographicLevels.Any(level =>
                _boundaryLevelRepository.FindLatestByGeographicLevel(level) != null);

            public List<BoundaryLevelViewModel> GetBoundaryLevelViewModels()
            {
                var boundaryLevels = _query.BoundaryLevel.HasValue
                    ? _boundaryLevelRepository.FindRelatedByBoundaryLevel(_query.BoundaryLevel.Value)
                    : _boundaryLevelRepository.FindByGeographicLevels(_geographicLevels);
                return boundaryLevels
                    .Select(level => new BoundaryLevelViewModel(level.Id, level.Label))
                    .ToList();
            }

            [ObsoleteAttribute("TODO EES-2902 - Remove with SOW8 after EES-2777", false)]
            public List<ObservationalUnitMetaViewModel> GetLegacyLocationViewModels()
            {
                var viewModels = _locationAttributes.SelectMany(pair =>
                {
                    var (geographicLevel, hierarchicalLocationAttributes) = pair;

                    // Location attributes should be flat because we retrieve the locations without specifying a hierarchy
                    // Throw an exception if this isn't true
                    if (hierarchicalLocationAttributes.Any(node => !node.IsLeaf))
                    {
                        throw new InvalidOperationException(
                            $"Expected flat list of location attributes building legacy location view model when locationHierarchies feature is disabled"
                        );
                    }

                    // Get the flat list of location attributes
                    var locationAttributes = hierarchicalLocationAttributes
                        .Select(node => node.Attribute)
                        .WhereNotNull()
                        .ToList();

                    return GetLegacyLocationAttributeViewModels(
                        geographicLevel,
                        locationAttributes);
                });

                return DeduplicateLocationViewModels(viewModels)
                    .OrderBy(model => model.Level.ToString())
                    .ThenBy(model => model.Label)
                    .ToList();
            }

            [ObsoleteAttribute("TODO EES-2902 - Remove with SOW8 after EES-2777", false)]
            private IEnumerable<ObservationalUnitMetaViewModel> GetLegacyLocationAttributeViewModels(
                GeographicLevel geographicLevel,
                IReadOnlyList<ILocationAttribute> locationAttributes)
            {
                var geoJsonByCode = GetGeoJson(geographicLevel, locationAttributes);

                return locationAttributes.Select(locationAttribute =>
                {
                    var code = GetLocationAttributeCode(locationAttribute);
                    var geoJson = code == null ? null : geoJsonByCode.GetValueOrDefault(code)?.Deserialized;

                    return new ObservationalUnitMetaViewModel
                    {
                        GeoJson = geoJson,
                        Label = locationAttribute.Name,
                        Level = geographicLevel,
                        Value = code
                    };
                });
            }

            public Dictionary<string, List<LocationAttributeViewModel>> GetLocationViewModels()
            {
                var allGeoJson = GetGeoJson(_locationAttributes);

                return _locationAttributes.ToDictionary(
                    pair => pair.Key.ToString().CamelCase(),
                    pair =>
                    {
                        var (geographicLevel, locationAttributes) = pair;
                        var geoJsonByCode = allGeoJson.GetValueOrDefault(geographicLevel);
                        return locationAttributes
                            .Select(locationAttribute =>
                                GetLocationAttributeViewModel(locationAttribute, geoJsonByCode))
                            .ToList();
                    });
            }

            private static LocationAttributeViewModel GetLocationAttributeViewModel(
                LocationAttributeNode locationAttributeNode,
                IReadOnlyDictionary<string, GeoJson>? geoJsonByCode)
            {
                var locationAttribute = locationAttributeNode.Attribute;

                if (locationAttributeNode.IsLeaf)
                {
                    var code = GetLocationAttributeCode(locationAttribute);
                    var geoJson = code == null ? null : geoJsonByCode?.GetValueOrDefault(code)?.Deserialized;

                    return new LocationAttributeViewModel
                    {
                        GeoJson = geoJson,
                        Label = locationAttribute.Name ?? string.Empty,
                        Value = locationAttribute.Code ?? string.Empty
                    };
                }

                return new LocationAttributeViewModel
                {
                    Label = locationAttribute.Name ?? string.Empty,
                    Level = locationAttribute.GetType().Name,
                    Value = locationAttribute.Code ?? string.Empty,
                    Options = locationAttributeNode.Children
                        .Select(child => GetLocationAttributeViewModel(child, geoJsonByCode))
                        .ToList()
                };
            }

            private long? GetLatestBoundaryLevelByGeographicLevel(GeographicLevel geographicLevel)
            {
                return _boundaryLevelRepository.FindLatestByGeographicLevel(geographicLevel)?.Id;
            }

            private Dictionary<GeographicLevel, Dictionary<string, GeoJson>> GetGeoJson(
                Dictionary<GeographicLevel, List<LocationAttributeNode>> locations)
            {
                if (!GeoJsonRequested)
                {
                    return new Dictionary<GeographicLevel, Dictionary<string, GeoJson>>();
                }

                return locations.ToDictionary(
                    pair => pair.Key,
                    pair =>
                    {
                        var (geographicLevel, locationAttributes) = pair;
                        return GetGeoJson(
                            geographicLevel,
                            locationAttributes
                                .SelectMany(node => node.GetLeafAttributes())
                                .ToList());
                    });
            }

            private Dictionary<string, GeoJson> GetGeoJson(
                GeographicLevel geographicLevel,
                IReadOnlyList<ILocationAttribute> locationAttributes)
            {
                if (!GeoJsonRequested)
                {
                    return new Dictionary<string, GeoJson>();
                }

                // If no boundary level is requested, get the latest boundary level id for the geographic level
                var boundaryLevelId = _query.BoundaryLevel ?? GetLatestBoundaryLevelByGeographicLevel(geographicLevel);

                // Not all geographic levels have boundary level data configured so expect to return an empty result
                if (boundaryLevelId == null)
                {
                    return new Dictionary<string, GeoJson>();
                }

                var locationAttributeCodes = locationAttributes
                    .Select(GetLocationAttributeCode)
                    .WhereNotNull();

                return _geoJsonRepository.FindByBoundaryLevelAndCodes(boundaryLevelId.Value, locationAttributeCodes);
            }

            private bool GeoJsonRequested => _query.IncludeGeoJson != null && _query.IncludeGeoJson.Value;

            private static string? GetLocationAttributeCode(ILocationAttribute locationAttribute)
            {
                return locationAttribute is LocalAuthority localAuthority
                    ? localAuthority.GetCodeOrOldCodeIfEmpty()
                    : locationAttribute.Code;
            }
        }
    }
}
