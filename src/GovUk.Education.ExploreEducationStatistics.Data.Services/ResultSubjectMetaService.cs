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
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.Security.DataSecurityPolicies;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class ResultSubjectMetaService : AbstractSubjectMetaService, IResultSubjectMetaService
    {
        private readonly IBoundaryLevelService _boundaryLevelService;
        private readonly IFootnoteRepository _footnoteRepository;
        private readonly IIndicatorService _indicatorService;
        private readonly ILocationService _locationService;
        private readonly IPersistenceHelper<StatisticsDbContext> _persistenceHelper;
        private readonly ITimePeriodService _timePeriodService;
        private readonly IUserService _userService;
        private readonly ISubjectService _subjectService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public ResultSubjectMetaService(IBoundaryLevelService boundaryLevelService,
            IFilterItemService filterItemService,
            IFootnoteRepository footnoteRepository,
            IGeoJsonService geoJsonService,
            IIndicatorService indicatorService,
            ILocationService locationService,
            IPersistenceHelper<StatisticsDbContext> persistenceHelper,
            ITimePeriodService timePeriodService,
            IUserService userService,
            ISubjectService subjectService,
            ILogger<ResultSubjectMetaService> logger,
            IMapper mapper) : base(boundaryLevelService, filterItemService, geoJsonService)
        {
            _boundaryLevelService = boundaryLevelService;
            _footnoteRepository = footnoteRepository;
            _indicatorService = indicatorService;
            _locationService = locationService;
            _persistenceHelper = persistenceHelper;
            _timePeriodService = timePeriodService;
            _userService = userService;
            _subjectService = subjectService;
            _logger = logger;
            _mapper = mapper;
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

                    var observationalUnits = _locationService.GetObservationalUnits(observations);
                    _logger.LogTrace("Got Observational Units in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var filters = GetFilters(query.SubjectId, observations, true);
                    _logger.LogTrace("Got Filters in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var footnotes = GetFilteredFootnotes(releaseId, observations, query);
                    _logger.LogTrace("Got Footnotes in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var geoJsonAvailable = HasBoundaryLevelDataForAnyObservationalUnits(observationalUnits);
                    _logger.LogTrace("Got GeoJsonAvailable in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var boundaryLevels = GetBoundaryLevelOptions(query.BoundaryLevel, observationalUnits.Keys);

                    var indicators = GetIndicators(query);
                    _logger.LogTrace("Got Indicators in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var locations = GetGeoJsonObservationalUnits(observationalUnits, query.IncludeGeoJson ?? false,
                        query.BoundaryLevel);
                    _logger.LogTrace("Got Observational Units in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var timePeriodRange = GetTimePeriodRange(observations);
                    _logger.LogTrace("Got Time Periods in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Stop();

                    var publication = await _subjectService.GetPublicationForSubject(subject.Id);

                    var subjectName = (await _persistenceHelper
                        .CheckEntityExists<ReleaseSubject>(
                            q => q
                                .Where(rs => rs.ReleaseId == releaseId
                                             && rs.SubjectId == subject.Id)
                        )
                        .OnSuccess(rs => rs.SubjectName)).Right;
                    
                    return new ResultSubjectMetaViewModel
                    {
                        Filters = filters,
                        Footnotes = footnotes,
                        GeoJsonAvailable = geoJsonAvailable,
                        Indicators = indicators,
                        Locations = locations,
                        BoundaryLevels = boundaryLevels,
                        PublicationName = publication.Title,
                        SubjectName = subjectName,
                        TimePeriodRange = timePeriodRange
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

        private IEnumerable<ObservationalUnitMetaViewModel> GetGeoJsonObservationalUnits(
            Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>> observationalUnits,
            bool geoJsonRequested,
            long? boundaryLevelId)
        {
            var viewModels = observationalUnits.SelectMany(pair =>
                BuildObservationalUnitMetaViewModelsWithGeoJsonIfAvailable(
                    pair.Key,
                    pair.Value.ToList(),
                    geoJsonRequested,
                    boundaryLevelId));

            return TransformDuplicateObservationalUnitsWithUniqueLabels(viewModels)
                .OrderBy(model => model.Level.ToString())
                .ThenBy(model => model.Label);
        }

        private IEnumerable<BoundaryLevelIdLabel> GetBoundaryLevelOptions(long? boundaryLevelId,
            IEnumerable<GeographicLevel> geographicLevels)
        {
            var boundaryLevels = boundaryLevelId.HasValue
                ? _boundaryLevelService.FindRelatedByBoundaryLevel(boundaryLevelId.Value)
                : _boundaryLevelService.FindByGeographicLevels(geographicLevels);
            return boundaryLevels.Select(level => _mapper.Map<BoundaryLevelIdLabel>(level));
        }

        private IEnumerable<IndicatorMetaViewModel> GetIndicators(SubjectMetaQueryContext query)
        {
            var indicators = _indicatorService.GetIndicators(query.SubjectId, query.Indicators);
            return BuildIndicatorViewModels(indicators);
        }

        private IEnumerable<FootnoteViewModel> GetFilteredFootnotes(Guid releaseId, IQueryable<Observation> observations,
            SubjectMetaQueryContext queryContext)
        {
            return _footnoteRepository.GetFilteredFootnotes(releaseId, queryContext.SubjectId, observations, queryContext.Indicators)
                .Select(footnote => new FootnoteViewModel
                {
                    Id = footnote.Id,
                    Label = footnote.Content
                });
        }

        private IEnumerable<TimePeriodMetaViewModel> GetTimePeriodRange(IQueryable<Observation> observations)
        {
            return _timePeriodService.GetTimePeriodRange(observations).Select(tuple =>
                new TimePeriodMetaViewModel(tuple.Year, tuple.TimeIdentifier));
        }
    }
}
