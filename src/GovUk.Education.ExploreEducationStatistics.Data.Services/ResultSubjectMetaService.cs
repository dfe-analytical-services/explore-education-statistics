#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.Security.DataSecurityPolicies;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class ResultSubjectMetaService : AbstractSubjectMetaService, IResultSubjectMetaService
    {
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
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IReleaseDataFileRepository _releaseDataFileRepository;

        public ResultSubjectMetaService(ContentDbContext contentDbContext,
            IBoundaryLevelRepository boundaryLevelRepository,
            IFilterItemRepository filterItemRepository,
            IFootnoteRepository footnoteRepository,
            IGeoJsonRepository geoJsonRepository,
            IIndicatorRepository indicatorRepository,
            ILocationRepository locationRepository,
            IPersistenceHelper<StatisticsDbContext> persistenceHelper,
            ITimePeriodService timePeriodService,
            IUserService userService,
            ISubjectRepository subjectRepository,
            ILogger<ResultSubjectMetaService> logger,
            IMapper mapper,
            IReleaseDataFileRepository releaseDataFileRepository) : base(filterItemRepository)
        {
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
            _logger = logger;
            _mapper = mapper;
            _releaseDataFileRepository = releaseDataFileRepository;
        }

        public Task<Either<ActionResult, ResultSubjectMetaViewModel>> GetSubjectMeta(Guid releaseId, 
            SubjectMetaQueryContext query, IQueryable<Observation> observations)
        {
            return _persistenceHelper.CheckEntityExists<Subject>(query.SubjectId)
                .OnSuccess(CheckCanViewSubjectData)
                .OnSuccess(async subject =>
                {
                    var stopwatch = Stopwatch.StartNew();
                    stopwatch.Start();

                    var locations = await _locationRepository.GetLocationAttributes(observations);
                    _logger.LogTrace("Got Locations in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var filterViewModels = GetFilters(query.SubjectId, observations, true);
                    _logger.LogTrace("Got Filters in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var footnoteViewModels = GetFilteredFootnoteViewModels(releaseId, observations, query);
                    _logger.LogTrace("Got Footnotes in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var geographicLevels = locations.Keys.ToList();

                    var geoJsonAvailable = HasBoundaryLevelDataForAnyGeographicLevel(geographicLevels);
                    _logger.LogTrace("Got GeoJsonAvailable in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var boundaryLevelViewModels = GetBoundaryLevelViewModels(
                        query.BoundaryLevel,
                        locations.Keys);

                    var indicatorViewModels = GetIndicatorViewModels(query);
                    _logger.LogTrace("Got Indicators in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var locationViewModels = GetLegacyLocationViewModels(
                        locations,
                        query.IncludeGeoJson ?? false,
                        query.BoundaryLevel);
                    _logger.LogTrace("Got Location view models in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var timePeriodViewModels = GetTimePeriodViewModels(observations);
                    _logger.LogTrace("Got Time Periods in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Stop();

                    var publicationId = await _subjectRepository.GetPublicationIdForSubject(subject.Id);
                    var publicationTitle = (await _contentDbContext.Publications.FindAsync(publicationId)).Title;

                    var releaseFile = await _releaseDataFileRepository.GetBySubject(releaseId, subject.Id);
                    var subjectName = releaseFile.Name;
                    
                    return new ResultSubjectMetaViewModel
                    {
                        Filters = filterViewModels,
                        Footnotes = footnoteViewModels,
                        GeoJsonAvailable = geoJsonAvailable,
                        Indicators = indicatorViewModels,
                        Locations = locationViewModels,
                        BoundaryLevels = boundaryLevelViewModels,
                        PublicationName = publicationTitle,
                        SubjectName = subjectName,
                        TimePeriodRange = timePeriodViewModels
                    };
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

        [ObsoleteAttribute("TODO EES-2902 - Remove with SOW8 after EES-2777", false)]
        private IEnumerable<ObservationalUnitMetaViewModel> GetLegacyLocationViewModels(
            Dictionary<GeographicLevel, IEnumerable<ILocationAttribute>> locationAttributes,
            bool geoJsonRequested,
            long? boundaryLevelId)
        {
            var viewModels = locationAttributes.SelectMany(pair =>
                GetLegacyLocationAttributeViewModels(
                    pair.Key,
                    pair.Value.ToList(),
                    geoJsonRequested,
                    boundaryLevelId));

            return TransformDuplicateLocationAttributesWithUniqueLabels(viewModels)
                .OrderBy(model => model.Level.ToString())
                .ThenBy(model => model.Label);
        }

        private IEnumerable<BoundaryLevelIdLabel> GetBoundaryLevelViewModels(
            long? boundaryLevelId,
            IEnumerable<GeographicLevel> geographicLevels)
        {
            var boundaryLevels = boundaryLevelId.HasValue
                ? _boundaryLevelRepository.FindRelatedByBoundaryLevel(boundaryLevelId.Value)
                : _boundaryLevelRepository.FindByGeographicLevels(geographicLevels);
            return boundaryLevels.Select(level => _mapper.Map<BoundaryLevelIdLabel>(level));
        }

        private IEnumerable<IndicatorMetaViewModel> GetIndicatorViewModels(SubjectMetaQueryContext query)
        {
            var indicators = _indicatorRepository.GetIndicators(query.SubjectId, query.Indicators);
            return BuildIndicatorViewModels(indicators);
        }

        private IEnumerable<FootnoteViewModel> GetFilteredFootnoteViewModels(
            Guid releaseId,
            IQueryable<Observation> observations,
            SubjectMetaQueryContext queryContext)
        {
            return _footnoteRepository.GetFilteredFootnotes(releaseId, queryContext.SubjectId, observations, queryContext.Indicators)
                .Select(footnote => new FootnoteViewModel
                {
                    Id = footnote.Id,
                    Label = footnote.Content
                });
        }

        [ObsoleteAttribute("TODO EES-2902 - Remove with SOW8 after EES-2777", false)]
        private IEnumerable<ObservationalUnitMetaViewModel> GetLegacyLocationAttributeViewModels(
            GeographicLevel geographicLevel,
            ICollection<ILocationAttribute> locationAttributes,
            bool geoJsonRequested,
            long? boundaryLevelId)
        {
            var geoJsonByCode = new Dictionary<string, GeoJson>();

            if (geoJsonRequested)
            {
                var boundaryLevel = boundaryLevelId ?? GetBoundaryLevel(geographicLevel)?.Id;
                if (boundaryLevel.HasValue)
                {
                    var codes = locationAttributes.Select(
                        locationAttribute =>
                            locationAttribute is LocalAuthority localAuthority ?
                                localAuthority.GetCodeOrOldCodeIfEmpty()
                                : locationAttribute.Code);
                    geoJsonByCode = _geoJsonRepository.Find(boundaryLevel.Value, codes).ToDictionary(g => g.Code);
                }
            }

            return locationAttributes.Select(locationAttribute =>
            {
                var value = locationAttribute is LocalAuthority localAuthority
                    ? localAuthority.GetCodeOrOldCodeIfEmpty()
                    : locationAttribute.Code;

                var serializedGeoJson = geoJsonByCode.GetValueOrDefault(value);
                var geoJson = DeserializeGeoJson(serializedGeoJson);

                return new ObservationalUnitMetaViewModel
                {
                    GeoJson = geoJson,
                    Label = locationAttribute.Name,
                    Level = geographicLevel,
                    Value = value
                };
            });
        }

        private static dynamic? DeserializeGeoJson(GeoJson? geoJson)
        {
            return geoJson == null ? null : JsonConvert.DeserializeObject(geoJson.Value);
        }

        private BoundaryLevel? GetBoundaryLevel(GeographicLevel geographicLevel)
        {
            return _boundaryLevelRepository.FindLatestByGeographicLevel(geographicLevel);
        }

        private IEnumerable<TimePeriodMetaViewModel> GetTimePeriodViewModels(IQueryable<Observation> observations)
        {
            return _timePeriodService.GetTimePeriodRange(observations).Select(tuple =>
                new TimePeriodMetaViewModel(tuple.Year, tuple.TimeIdentifier));
        }

        private bool HasBoundaryLevelDataForAnyGeographicLevel(IEnumerable<GeographicLevel> geographicLevels)
        {
            return geographicLevels.Any(level =>
                _boundaryLevelRepository.FindLatestByGeographicLevel(level) != null);
        }
    }
}
