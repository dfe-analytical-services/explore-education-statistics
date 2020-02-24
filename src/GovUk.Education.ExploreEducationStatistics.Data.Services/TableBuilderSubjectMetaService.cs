using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta.TableBuilder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.Security.DataSecurityPolicies;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class TableBuilderSubjectMetaService : AbstractTableBuilderSubjectMetaService,
        ITableBuilderSubjectMetaService
    {
        private readonly IFilterService _filterService;
        private readonly IFilterItemService _filterItemService;
        private readonly IIndicatorGroupService _indicatorGroupService;
        private readonly ILocationService _locationService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IObservationService _observationService;
        private readonly ISubjectService _subjectService;
        private readonly ITimePeriodService _timePeriodService;
        private readonly IUserService _userService;

        public TableBuilderSubjectMetaService(IFilterService filterService,
            IFilterItemService filterItemService,
            IIndicatorGroupService indicatorGroupService,
            ILocationService locationService,
            ILogger<TableBuilderSubjectMetaService> logger,
            IMapper mapper,
            IObservationService observationService,
            ISubjectService subjectService,
            ITimePeriodService timePeriodService,
            IUserService userService) : base(filterItemService)
        {
            _filterService = filterService;
            _filterItemService = filterItemService;
            _indicatorGroupService = indicatorGroupService;
            _locationService = locationService;
            _logger = logger;
            _mapper = mapper;
            _observationService = observationService;
            _subjectService = subjectService;
            _timePeriodService = timePeriodService;
            _userService = userService;
        }

        public Task<Either<ActionResult, TableBuilderSubjectMetaViewModel>> GetSubjectMeta(Guid subjectId)
        {
            return CheckSubjectExists(subjectId)
                .OnSuccess(CheckCanViewSubjectData)
                .OnSuccess(subject => new TableBuilderSubjectMetaViewModel
                {
                    Filters = GetFilters(subject.Id),
                    Indicators = GetIndicators(subject.Id),
                    Locations = GetObservationalUnits(subject.Id),
                    TimePeriod = GetTimePeriods(subject.Id)
                });
        }

        public Task<Either<ActionResult, TableBuilderSubjectMetaViewModel>> GetSubjectMeta(
            SubjectMetaQueryContext query)
        {
            return CheckSubjectExists(query.SubjectId)
                .OnSuccess(CheckCanViewSubjectData)
                .OnSuccess(subject =>
                {
                    var observations = _observationService.FindObservations(query).AsQueryable();

                    var stopwatch = Stopwatch.StartNew();
                    stopwatch.Start();

                    var filters = GetFilters(observations);

                    _logger.LogTrace("Got Filters in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var indicators = GetIndicators(subject.Id);

                    _logger.LogTrace("Got Indicators in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var locations = GetObservationalUnits(observations);

                    _logger.LogTrace("Got Observational Units in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var timePeriods = GetTimePeriods(observations);

                    _logger.LogTrace("Got Time Periods in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Stop();

                    return new TableBuilderSubjectMetaViewModel
                    {
                        Filters = filters,
                        Indicators = indicators,
                        Locations = locations,
                        TimePeriod = timePeriods
                    };
                });
        }

        private Dictionary<string, FilterMetaViewModel> GetFilters(Guid subjectId)
        {
            return _filterService.GetFiltersIncludingItems(subjectId)
                .ToDictionary(
                    filter => filter.Label.PascalCase(),
                    filter => new FilterMetaViewModel
                    {
                        Hint = filter.Hint,
                        Legend = filter.Label,
                        Options = filter.FilterGroups.ToDictionary(
                            filterGroup => filterGroup.Label.PascalCase(),
                            filterGroup => BuildFilterItemsViewModel(filterGroup, filterGroup.FilterItems)),
                        TotalValue = GetTotalValue(filter)
                    });
        }

        private TableBuilderTimePeriodsMetaViewModel GetTimePeriods(Guid subjectId)
        {
            var timePeriods = _timePeriodService.GetTimePeriods(subjectId);
            return BuildTimePeriodsViewModels(timePeriods);
        }

        private TableBuilderTimePeriodsMetaViewModel GetTimePeriods(IQueryable<Observation> observations)
        {
            var timePeriods = _timePeriodService.GetTimePeriods(observations);
            return BuildTimePeriodsViewModels(timePeriods);
        }

        private Dictionary<string, TableBuilderObservationalUnitsMetaViewModel> GetObservationalUnits(Guid subjectId)
        {
            var observationalUnits = _locationService.GetObservationalUnits(subjectId);
            return BuildObservationalUnitsViewModels(observationalUnits);
        }

        private Dictionary<string, TableBuilderObservationalUnitsMetaViewModel> GetObservationalUnits(
            IQueryable<Observation> observations)
        {
            var observationalUnits = _locationService.GetObservationalUnits(observations);
            return BuildObservationalUnitsViewModels(observationalUnits);
        }

        private Dictionary<string, TableBuilderIndicatorsMetaViewModel> GetIndicators(Guid subjectId)
        {
            return _indicatorGroupService.GetIndicatorGroups(subjectId).ToDictionary(
                group => group.Label.PascalCase(),
                group => new TableBuilderIndicatorsMetaViewModel
                {
                    Label = group.Label,
                    Options = _mapper.Map<IEnumerable<IndicatorMetaViewModel>>(group.Indicators)
                }
            );
        }

        private Dictionary<string, TableBuilderObservationalUnitsMetaViewModel> BuildObservationalUnitsViewModels(
            Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>> observationalUnits)
        {
            var viewModels = observationalUnits.ToDictionary(
                pair => pair.Key.ToString().CamelCase(),
                pair => new TableBuilderObservationalUnitsMetaViewModel
                {
                    Hint = "",
                    Legend = pair.Key.GetEnumLabel(),
                    Options = pair.Value.Select(MapObservationalUnitToLabelValue)
                });

            foreach (var (_, viewModel) in viewModels)
            {
                viewModel.Options = TransformDuplicateObservationalUnitsWithUniqueLabels(viewModel.Options);
            }

            return viewModels;
        }

        private static TableBuilderTimePeriodsMetaViewModel BuildTimePeriodsViewModels(
            IEnumerable<(int Year, TimeIdentifier TimeIdentifier)> timePeriods)
        {
            var options = timePeriods.Select(tuple => new TimePeriodMetaViewModel(tuple.Year, tuple.TimeIdentifier));
            return new TableBuilderTimePeriodsMetaViewModel
            {
                Hint = "Filter statistics by a given start and end date",
                Legend = "",
                Options = options
            };
        }

        private string GetTotalValue(Filter filter)
        {
            return _filterItemService.GetTotal(filter)?.Id.ToString() ?? string.Empty;
        }

        private static LabelValue MapObservationalUnitToLabelValue(IObservationalUnit unit)
        {
            var value = unit is LocalAuthority localAuthority ? localAuthority.GetCodeOrOldCodeIfEmpty() : unit.Code;
            return new LabelValue
            {
                Label = unit.Name,
                Value = value
            };
        }

        private async Task<Either<ActionResult, Subject>> CheckSubjectExists(Guid subjectId)
        {
            var subject = _subjectService.Find(subjectId, new List<Expression<Func<Subject, object>>>
            {
                s => s.Release
            });

            return subject == null
                ? new NotFoundResult()
                : new Either<ActionResult, Subject>(subject);
        }

        private async Task<Either<ActionResult, Subject>> CheckCanViewSubjectData(Subject subject)
        {
            var result = subject.Release.Live || await _userService.MatchesPolicy(subject, CanViewSubjectData);
            return result ? new Either<ActionResult, Subject>(subject) : new ForbidResult();
        }
    }
}