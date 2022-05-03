#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.FilterAndIndicatorViewModelBuilders;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class ResultSubjectMetaService : AbstractSubjectMetaService, IResultSubjectMetaService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IBoundaryLevelRepository _boundaryLevelRepository;
        private readonly IFilterItemRepository _filterItemRepository;
        private readonly IFootnoteRepository _footnoteRepository;
        private readonly IGeoJsonRepository _geoJsonRepository;
        private readonly IIndicatorRepository _indicatorRepository;
        private readonly ITimePeriodService _timePeriodService;
        private readonly IUserService _userService;
        private readonly ISubjectRepository _subjectRepository;
        private readonly IReleaseDataFileRepository _releaseDataFileRepository;
        private readonly LocationsOptions _locationOptions;
        private readonly ILogger _logger;

        public ResultSubjectMetaService(
            ContentDbContext contentDbContext,
            IBoundaryLevelRepository boundaryLevelRepository,
            IFilterItemRepository filterItemRepository,
            IFootnoteRepository footnoteRepository,
            IGeoJsonRepository geoJsonRepository,
            IIndicatorRepository indicatorRepository,
            IPersistenceHelper<StatisticsDbContext> persistenceHelper,
            ITimePeriodService timePeriodService,
            IUserService userService,
            ISubjectRepository subjectRepository,
            IReleaseDataFileRepository releaseDataFileRepository,
            IOptions<LocationsOptions> locationOptions,
            ILogger<ResultSubjectMetaService> logger) : base(persistenceHelper)
        {
            _contentDbContext = contentDbContext;
            _boundaryLevelRepository = boundaryLevelRepository;
            _filterItemRepository = filterItemRepository;
            _footnoteRepository = footnoteRepository;
            _geoJsonRepository = geoJsonRepository;
            _indicatorRepository = indicatorRepository;
            _timePeriodService = timePeriodService;
            _userService = userService;
            _subjectRepository = subjectRepository;
            _releaseDataFileRepository = releaseDataFileRepository;
            _locationOptions = locationOptions.Value;
            _logger = logger;
        }

        public async Task<Either<ActionResult, ResultSubjectMetaViewModel>> GetSubjectMeta(
            Guid releaseId,
            ObservationQueryContext query,
            IList<Observation> observations)
        {
            return await CheckReleaseSubjectExists(releaseId, query.SubjectId)
                .OnSuccess(_userService.CheckCanViewSubjectData)
                .OnSuccess(async releaseSubject =>
                {
                    var stopwatch = Stopwatch.StartNew();
                    stopwatch.Start();

                    var locations = observations
                        .Select(o => o.Location)
                        .Distinct()
                        .ToList();

                    var locationAttributes =
                        locations.GetLocationAttributesHierarchical(_locationOptions.Hierarchies);
                    _logger.LogTrace("Got Location attributes in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var filterItems =
                        _filterItemRepository.GetFilterItemsFromObservationList(observations);
                    var filterViewModels = FiltersViewModelBuilder.BuildFiltersFromFilterItems(filterItems,
                        releaseSubject.FilterSequence);
                    _logger.LogTrace("Got Filters in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var footnoteViewModels =
                        GetFilteredFootnoteViewModels(releaseId, filterItems.Select(fi => fi.Id).ToList(), query);
                    _logger.LogTrace("Got Footnotes in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var indicatorViewModels = GetIndicatorViewModels(query, releaseSubject);
                    _logger.LogTrace("Got Indicators in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var timePeriodViewModels = GetTimePeriodViewModels(observations);
                    _logger.LogTrace("Got Time Periods in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var publicationId = await _subjectRepository.GetPublicationIdForSubject(releaseSubject.SubjectId);
                    var publicationTitle = (await _contentDbContext.Publications.FindAsync(publicationId))!.Title;

                    var releaseFile =
                        await _releaseDataFileRepository.GetBySubject(releaseId, releaseSubject.SubjectId);
                    var subjectName = releaseFile.Name!;

                    var locationsHelper = new LocationsQueryHelper(
                        locationAttributes,
                        query,
                        _boundaryLevelRepository,
                        _geoJsonRepository);

                    var locationViewModels = locationsHelper.GetLocationViewModels();
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
                });
        }

        private List<IndicatorMetaViewModel> GetIndicatorViewModels(ObservationQueryContext query,
            ReleaseSubject subject)
        {
            var indicators = _indicatorRepository.GetIndicators(query.SubjectId, query.Indicators);

            // Flatten the indicator sequence so that it can be used to sequence all the indicators since they have
            // been fetched without groups
            var indicatorsOrdering = subject.IndicatorSequence?
                .SelectMany(groupOrdering => groupOrdering.ChildSequence)
                .ToList();

            return IndicatorsViewModelBuilder.BuildIndicators(indicators, indicatorsOrdering);
        }

        private List<FootnoteViewModel> GetFilteredFootnoteViewModels(
            Guid releaseId,
            IEnumerable<Guid> filterItemIds,
            ObservationQueryContext queryContext)
        {
            return _footnoteRepository
                .GetFilteredFootnotes(releaseId, queryContext.SubjectId, filterItemIds, queryContext.Indicators)
                .Select(footnote => new FootnoteViewModel
                {
                    Id = footnote.Id,
                    Label = footnote.Content
                })
                .ToList();
        }

        private List<TimePeriodMetaViewModel> GetTimePeriodViewModels(IList<Observation> observations)
        {
            return _timePeriodService
                .GetTimePeriodRange(observations)
                .Select(tuple => new TimePeriodMetaViewModel(tuple.Year, tuple.TimeIdentifier))
                .ToList();
        }

        private class LocationsQueryHelper
        {
            private readonly Dictionary<GeographicLevel, List<LocationAttributeNode>> _locationAttributes;
            private readonly ObservationQueryContext _query;
            private readonly IBoundaryLevelRepository _boundaryLevelRepository;
            private readonly IGeoJsonRepository _geoJsonRepository;
            private readonly List<GeographicLevel> _geographicLevels;

            internal LocationsQueryHelper(
                Dictionary<GeographicLevel, List<LocationAttributeNode>> locationAttributes,
                ObservationQueryContext query,
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
                return _boundaryLevelRepository
                    .FindByGeographicLevels(_geographicLevels)
                    .Select(level => new BoundaryLevelViewModel(level.Id, level.Label))
                    .ToList();
            }

            public Dictionary<string, List<LocationAttributeViewModel>> GetLocationViewModels()
            {
                var allGeoJson = _query.BoundaryLevel != null
                    ? GetGeoJson(_query.BoundaryLevel.Value, _locationAttributes)
                    : new Dictionary<GeographicLevel, Dictionary<string, GeoJson>>();

                return _locationAttributes.ToDictionary(
                    pair => pair.Key.ToString().CamelCase(),
                    pair =>
                    {
                        var (geographicLevel, locationAttributes) = pair;
                        var geoJsonByCode = allGeoJson.GetValueOrDefault(geographicLevel);
                        return DeduplicateLocationViewModels(
                                locationAttributes
                                    .OrderBy(OrderLocationAttributes)
                                    .Select(locationAttribute =>
                                        GetLocationAttributeViewModel(locationAttribute, geoJsonByCode))
                            )
                            .ToList();
                    });
            }

            private static LocationAttributeViewModel GetLocationAttributeViewModel(
                LocationAttributeNode locationAttributeNode,
                IReadOnlyDictionary<string, GeoJson>? geoJsonByCode)
            {
                var locationAttribute = locationAttributeNode.Attribute;
                var code = locationAttribute.GetCodeOrFallback();

                if (locationAttributeNode.IsLeaf)
                {
                    var geoJson = code.IsNullOrEmpty()
                        ? null
                        : geoJsonByCode?.GetValueOrDefault(code)?.Deserialized;

                    return new LocationAttributeViewModel
                    {
                        Id = locationAttributeNode.LocationId!.Value,
                        GeoJson = geoJson,
                        Label = locationAttribute.Name ?? string.Empty,
                        Value = code
                    };
                }

                return new LocationAttributeViewModel
                {
                    Label = locationAttribute.Name ?? string.Empty,
                    Level = locationAttribute.GetType().Name.CamelCase(),
                    Value = code,
                    Options = DeduplicateLocationViewModels(
                            locationAttributeNode.Children
                                .OrderBy(OrderLocationAttributes)
                                .Select(child => GetLocationAttributeViewModel(child, geoJsonByCode))
                        )
                        .ToList()
                };
            }

            private static string OrderLocationAttributes(LocationAttributeNode node)
            {
                var locationAttribute = node.Attribute;

                return locationAttribute switch
                {
                    Region region => region.Code ?? string.Empty,
                    _ => locationAttribute.Name ?? string.Empty
                };
            }

            private Dictionary<GeographicLevel, Dictionary<string, GeoJson>> GetGeoJson(
                long boundaryLevelId,
                Dictionary<GeographicLevel, List<LocationAttributeNode>> locations)
            {
                var selectedBoundaryLevel = _boundaryLevelRepository
                    .FindOrNotFound(boundaryLevelId)
                    .OrElse(() => null!)
                    .Right;

                if (selectedBoundaryLevel == null || !locations.ContainsKey(selectedBoundaryLevel.Level))
                {
                    return new Dictionary<GeographicLevel, Dictionary<string, GeoJson>>();
                }

                return locations.ToDictionary(
                    pair => pair.Key,
                    pair =>
                    {
                        var (geographicLevel, locationAttributes) = pair;

                        if (geographicLevel != selectedBoundaryLevel.Level)
                        {
                            return new Dictionary<string, GeoJson>();
                        }

                        var locationAttributeCodes = locationAttributes
                            .SelectMany(locationAttribute => locationAttribute.GetLeafAttributes())
                            .Select(locationAttribute => locationAttribute.GetCodeOrFallback())
                            .WhereNotNull();

                        return _geoJsonRepository.FindByBoundaryLevelAndCodes(boundaryLevelId, locationAttributeCodes);
                    });
            }
        }
    }
}
