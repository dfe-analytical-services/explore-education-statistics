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
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta.TableBuilder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.Security.DataSecurityPolicies;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class TableBuilderResultSubjectMetaService : AbstractTableBuilderSubjectMetaService,
        ITableBuilderResultSubjectMetaService
    {
        private readonly IFootnoteService _footnoteService;
        private readonly IIndicatorService _indicatorService;
        private readonly ILocationService _locationService;
        private readonly IPersistenceHelper<StatisticsDbContext> _persistenceHelper;
        private readonly ITimePeriodService _timePeriodService;
        private readonly IUserService _userService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public TableBuilderResultSubjectMetaService(IFilterItemService filterItemService,
            IFootnoteService footnoteService,
            IIndicatorService indicatorService,
            ILocationService locationService,
            IPersistenceHelper<StatisticsDbContext> persistenceHelper,
            ITimePeriodService timePeriodService,
            IUserService userService,
            ILogger<TableBuilderResultSubjectMetaService> logger,
            IMapper mapper) : base(filterItemService)
        {
            _footnoteService = footnoteService;
            _indicatorService = indicatorService;
            _locationService = locationService;
            _persistenceHelper = persistenceHelper;
            _timePeriodService = timePeriodService;
            _userService = userService;
            _logger = logger;
            _mapper = mapper;
        }

        public Task<Either<ActionResult, TableBuilderResultSubjectMetaViewModel>> GetSubjectMeta(
            SubjectMetaQueryContext query, IQueryable<Observation> observations)
        {
            return _persistenceHelper.CheckEntityExists<Subject>(query.SubjectId, HydrateSubject)
                .OnSuccess(CheckCanViewSubjectData)
                .OnSuccess(subject =>
                {
                    var stopwatch = Stopwatch.StartNew();
                    stopwatch.Start();

                    var filters = GetFilters(observations);

                    _logger.LogTrace("Got Filters in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var footnotes = GetFootnotes(observations, query);

                    _logger.LogTrace("Got Footnotes in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var indicators = GetIndicators(query);

                    _logger.LogTrace("Got Indicators in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var locations = GetObservationalUnits(observations);

                    _logger.LogTrace("Got Observational Units in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var timePeriodRange = GetTimePeriodRange(observations);

                    _logger.LogTrace("Got Time Periods in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Stop();

                    return new TableBuilderResultSubjectMetaViewModel
                    {
                        Filters = filters,
                        Footnotes = footnotes,
                        Indicators = indicators,
                        Locations = locations,
                        PublicationName = subject.Release.Publication.Title,
                        SubjectName = subject.Name,
                        TimePeriodRange = timePeriodRange
                    };
                });
        }

        private async Task<Either<ActionResult, Subject>> CheckCanViewSubjectData(Subject subject)
        {
            var result = subject.Release.Live || await _userService.MatchesPolicy(subject, CanViewSubjectData);
            return result ? new Either<ActionResult, Subject>(subject) : new ForbidResult();
        }

        private IEnumerable<ObservationalUnitMetaViewModel> GetObservationalUnits(IQueryable<Observation> observations)
        {
            var observationalUnits = _locationService.GetObservationalUnits(observations);

            var viewModels = observationalUnits.SelectMany(pair =>
                pair.Value.Select(observationalUnit =>
                    BuildObservationalUnitMetaViewModel(pair.Key, observationalUnit)));

            return TransformDuplicateObservationalUnitsWithUniqueLabels(viewModels);
        }

        private IEnumerable<IndicatorMetaViewModel> GetIndicators(SubjectMetaQueryContext query)
        {
            return _mapper.Map<IEnumerable<IndicatorMetaViewModel>>(
                _indicatorService.GetIndicators(query.SubjectId, query.Indicators));
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

        private IEnumerable<TimePeriodMetaViewModel> GetTimePeriodRange(IQueryable<Observation> observations)
        {
            return _timePeriodService.GetTimePeriodRange(observations).Select(tuple =>
                new TimePeriodMetaViewModel(tuple.Year, tuple.TimeIdentifier));
        }

        private static ObservationalUnitMetaViewModel BuildObservationalUnitMetaViewModel(GeographicLevel level,
            IObservationalUnit unit)
        {
            var value = unit is LocalAuthority localAuthority ? localAuthority.GetCodeOrOldCodeIfEmpty() : unit.Code;
            return new ObservationalUnitMetaViewModel
            {
                Label = unit.Name,
                Level = level,
                Value = value
            };
        }

        private static IQueryable<Subject> HydrateSubject(IQueryable<Subject> queryable)
        {
            return queryable.Include(subject => subject.Release)
                .ThenInclude(release => release.Publication);
        }
    }
}