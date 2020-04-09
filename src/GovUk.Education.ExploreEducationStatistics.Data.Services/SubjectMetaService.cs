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
using GovUk.Education.ExploreEducationStatistics.Data.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.Security.DataSecurityPolicies;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class SubjectMetaService : AbstractTableBuilderSubjectMetaService, ISubjectMetaService
    {
        private readonly IBoundaryLevelService _boundaryLevelService;
        private readonly IIndicatorService _indicatorService;
        private readonly ILocationService _locationService;
        private readonly IPersistenceHelper<StatisticsDbContext> _persistenceHelper;
        private readonly ITimePeriodService _timePeriodService;
        private readonly IFootnoteService _footnoteService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public SubjectMetaService(IBoundaryLevelService boundaryLevelService,
            IFilterItemService filterItemService,
            IGeoJsonService geoJsonService,
            IIndicatorService indicatorService,
            ILocationService locationService,
            IPersistenceHelper<StatisticsDbContext> persistenceHelper,
            IMapper mapper,
            ITimePeriodService timePeriodService,
            IFootnoteService footnoteService,
            IUserService userService,
            ILogger<SubjectMetaService> logger
        ) : base(boundaryLevelService, filterItemService, geoJsonService)
        {
            _boundaryLevelService = boundaryLevelService;
            _indicatorService = indicatorService;
            _locationService = locationService;
            _persistenceHelper = persistenceHelper;
            _mapper = mapper;
            _timePeriodService = timePeriodService;
            _footnoteService = footnoteService;
            _userService = userService;
            _logger = logger;
        }

        public Task<Either<ActionResult, SubjectMetaViewModel>> GetSubjectMeta(
            SubjectMetaQueryContext query,
            IQueryable<Observation> observations)
        {
            return _persistenceHelper.CheckEntityExists<Subject>(query.SubjectId, HydrateSubject)
                .OnSuccess(CheckCanViewSubjectData)
                .OnSuccess(subject =>
                {
                    var stopwatch = Stopwatch.StartNew();
                    stopwatch.Start();

                    var observationalUnits = _locationService.GetObservationalUnits(observations);

                    _logger.LogTrace("Got Observational Units in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var filters = GetFilters(observations);

                    _logger.LogTrace("Got Filters in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var indicators = GetIndicators(query);

                    _logger.LogTrace("Got Indicators in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var locations = GetGeoJsonObservationalUnits(observationalUnits, query.IncludeGeoJson ?? true,
                        query.BoundaryLevel);
                    var geoJsonAvailable = HasBoundaryLevelDataForAnyObservationalUnits(observationalUnits);

                    _logger.LogTrace("Got GeoJson in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var boundaryLevels = GetBoundaryLevelOptions(query.BoundaryLevel, observationalUnits.Keys);

                    _logger.LogTrace("Got Boundary Level Options in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var timePeriod = GetTimePeriod(observations);

                    _logger.LogTrace("Got Time Periods in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var footnotes = GetFootnotes(observations, query);

                    _logger.LogTrace("Got Footnotes in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Stop();

                    return new SubjectMetaViewModel
                    {
                        Filters = filters,
                        Indicators = indicators,
                        Locations = locations,
                        BoundaryLevels = boundaryLevels,
                        PublicationName = subject.Release.Publication.Title,
                        SubjectName = subject.Name,
                        TimePeriod = timePeriod,
                        Footnotes = footnotes,
                        GeoJsonAvailable = geoJsonAvailable
                    };
                });
        }

        private Dictionary<string, IndicatorMetaViewModel> GetIndicators(SubjectMetaQueryContext query)
        {
            var indicatorList = _indicatorService.GetIndicators(query.SubjectId, query.Indicators);
            return indicatorList.ToDictionary(
                indicator => indicator.Id.ToString(),
                indicator => _mapper.Map<IndicatorMetaViewModel>(indicator));
        }

        private Dictionary<string, ObservationalUnitMetaViewModel> GetGeoJsonObservationalUnits(
            Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>> observationalUnits,
            bool geoJsonRequested,
            long? boundaryLevelId = null)
        {
            var observationalUnitMetaViewModels = observationalUnits.SelectMany(pair =>
                BuildObservationalUnitMetaViewModelsWithGeoJsonIfAvailable(
                    pair.Key,
                    pair.Value.ToList(),
                    geoJsonRequested,
                    boundaryLevelId));

            return observationalUnitMetaViewModels.ToDictionary(
                model => model.Value,
                model => model);
        }

        private IEnumerable<BoundaryLevelIdLabel> GetBoundaryLevelOptions(long? boundaryLevelId,
            IEnumerable<GeographicLevel> geographicLevels)
        {
            var boundaryLevels = boundaryLevelId.HasValue
                ? _boundaryLevelService.FindRelatedByBoundaryLevel(boundaryLevelId.Value)
                : _boundaryLevelService.FindByGeographicLevels(geographicLevels);
            return boundaryLevels.Select(level => _mapper.Map<BoundaryLevelIdLabel>(level));
        }

        private Dictionary<string, TimePeriodMetaViewModel> GetTimePeriod(IQueryable<Observation> observations)
        {
            return _timePeriodService.GetTimePeriods(observations).ToDictionary(
                tuple => tuple.GetTimePeriod(),
                tuple => new TimePeriodMetaViewModel(tuple.Year, tuple.TimeIdentifier));
        }

        private async Task<Either<ActionResult, Subject>> CheckCanViewSubjectData(Subject subject)
        {
            if (await _userService.MatchesPolicy(subject.Release, CanViewSubjectDataForRelease))
            {
                return subject;
            }

            return new ForbidResult();
        }

        private IEnumerable<FootnoteViewModel> GetFootnotes(IQueryable<Observation> observations,
            SubjectMetaQueryContext queryContext)
        {
            return _footnoteService.GetFootnotes(queryContext.SubjectId, observations, queryContext.Indicators)
                .Select(footnote => new FootnoteViewModel
                {
                    Id = footnote.Id,
                    Label = footnote.Content
                });
        }

        private static IQueryable<Subject> HydrateSubject(IQueryable<Subject> queryable)
        {
            return queryable.Include(subject => subject.Release)
                .ThenInclude(release => release.Publication);
        }
    }
}