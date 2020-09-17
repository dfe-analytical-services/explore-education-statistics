using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.Security.DataSecurityPolicies;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class SubjectMetaService : AbstractSubjectMetaService,
        ISubjectMetaService
    {
        private readonly IFilterService _filterService;
        private readonly IFilterItemService _filterItemService;
        private readonly IIndicatorGroupService _indicatorGroupService;
        private readonly ILocationService _locationService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IObservationService _observationService;
        private readonly IPersistenceHelper<StatisticsDbContext> _persistenceHelper;
        private readonly ITimePeriodService _timePeriodService;
        private readonly IUserService _userService;

        public SubjectMetaService(IBoundaryLevelService boundaryLevelService,
            IFilterService filterService,
            IFilterItemService filterItemService,
            IGeoJsonService geoJsonService,
            IIndicatorGroupService indicatorGroupService,
            ILocationService locationService,
            ILogger<SubjectMetaService> logger,
            IMapper mapper,
            IObservationService observationService,
            IPersistenceHelper<StatisticsDbContext> persistenceHelper,
            ITimePeriodService timePeriodService,
            IUserService userService) : base(boundaryLevelService, filterItemService, geoJsonService)
        {
            _filterService = filterService;
            _filterItemService = filterItemService;
            _indicatorGroupService = indicatorGroupService;
            _locationService = locationService;
            _logger = logger;
            _mapper = mapper;
            _observationService = observationService;
            _persistenceHelper = persistenceHelper;
            _timePeriodService = timePeriodService;
            _userService = userService;
        }

        public Task<Either<ActionResult, SubjectMetaViewModel>> GetSubjectMeta(Guid subjectId)
        {
            return _persistenceHelper.CheckEntityExists<Subject>(subjectId)
                .OnSuccess(GetSubjectMetaViewModel);
        }
        
        public Task<Either<ActionResult, SubjectMetaViewModel>> GetSubjectMetaRestricted(Guid subjectId)
        {
            return _persistenceHelper.CheckEntityExists<Subject>(subjectId)
                .OnSuccess(CheckCanViewSubjectData)
                .OnSuccess(GetSubjectMetaViewModel);
        }

        public Task<Either<ActionResult, SubjectMetaViewModel>> GetSubjectMeta(
            SubjectMetaQueryContext query)
        {
            return _persistenceHelper.CheckEntityExists<Subject>(query.SubjectId)
                .OnSuccess(subject => GetSubjectMetaViewModelFromQuery(subject, query));
        }
        
        public Task<Either<ActionResult, SubjectMetaViewModel>> GetSubjectMetaRestricted(
            SubjectMetaQueryContext query)
        {
            return _persistenceHelper.CheckEntityExists<Subject>(query.SubjectId)
                .OnSuccess(CheckCanViewSubjectData)
                .OnSuccess(subject => GetSubjectMetaViewModelFromQuery(subject, query));
        }
        
        private SubjectMetaViewModel GetSubjectMetaViewModel(Subject subject)
        {
            return new SubjectMetaViewModel
            {
                Filters = GetFilters(subject.Id),
                Indicators = GetIndicators(subject.Id),
                Locations = GetObservationalUnits(subject.Id),
                TimePeriod = GetTimePeriods(subject.Id)
            };
        }

        private SubjectMetaViewModel GetSubjectMetaViewModelFromQuery(Subject subject, SubjectMetaQueryContext query)
        {
            var observations = _observationService.FindObservations(query).AsQueryable();

            var stopwatch = Stopwatch.StartNew();
            stopwatch.Start();

            var filters = query.TimePeriod != null ? GetFilters(subject.Id, observations, false) : new Dictionary<string, FilterMetaViewModel>();

            _logger.LogTrace("Got Filters in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
            stopwatch.Restart();

            var indicators = query.TimePeriod != null ? GetIndicators(subject.Id) : new Dictionary<string, IndicatorsMetaViewModel>();

            _logger.LogTrace("Got Indicators in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
            stopwatch.Restart();

            var locations = GetObservationalUnits(observations);

            _logger.LogTrace("Got Observational Units in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
            stopwatch.Restart();

            var timePeriods = GetTimePeriods(observations);

            _logger.LogTrace("Got Time Periods in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
            stopwatch.Stop();

            return new SubjectMetaViewModel
            {
                Filters = filters,
                Indicators = indicators,
                Locations = locations,
                TimePeriod = timePeriods
            };
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
                        Options = filter.FilterGroups
                            .OrderBy(filterGroup => filterGroup.Label, LabelComparer)
                            .ToDictionary(
                            filterGroup => filterGroup.Label.PascalCase(),
                            filterGroup => BuildFilterItemsViewModel(filterGroup, filterGroup.FilterItems)),
                        TotalValue = GetTotalValue(filter)
                    });
        }

        private TimePeriodsMetaViewModel GetTimePeriods(Guid subjectId)
        {
            var timePeriods = _timePeriodService.GetTimePeriods(subjectId);
            return BuildTimePeriodsViewModels(timePeriods);
        }

        private TimePeriodsMetaViewModel GetTimePeriods(IQueryable<Observation> observations)
        {
            var timePeriods = _timePeriodService.GetTimePeriods(observations);
            return BuildTimePeriodsViewModels(timePeriods);
        }

        private Dictionary<string, ObservationalUnitsMetaViewModel> GetObservationalUnits(Guid subjectId)
        {
            var observationalUnits = _locationService.GetObservationalUnits(subjectId);
            return BuildObservationalUnitsViewModels(observationalUnits);
        }

        private Dictionary<string, ObservationalUnitsMetaViewModel> GetObservationalUnits(
            IQueryable<Observation> observations)
        {
            var observationalUnits = _locationService.GetObservationalUnits(observations);
            return BuildObservationalUnitsViewModels(observationalUnits);
        }

        private Dictionary<string, IndicatorsMetaViewModel> GetIndicators(Guid subjectId)
        {
            return _indicatorGroupService.GetIndicatorGroups(subjectId)
                .OrderBy(group => group.Label, LabelComparer)
                .ToDictionary(
                    group => group.Label.PascalCase(),
                    group => new IndicatorsMetaViewModel
                    {
                        Label = group.Label,
                        Options = BuildIndicatorViewModels(group.Indicators)
                    }
                );
        }

        private static Dictionary<string, ObservationalUnitsMetaViewModel> BuildObservationalUnitsViewModels(
            Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>> observationalUnits)
        {
            var viewModels = observationalUnits
                .OrderBy(pair => pair.Key.GetEnumLabel())
                .ToDictionary(
                    pair => pair.Key.ToString().CamelCase(),
                    pair => new ObservationalUnitsMetaViewModel
                    {
                        Hint = "",
                        Legend = pair.Key.GetEnumLabel(),
                        Options = pair.Value.Select(MapObservationalUnitToLabelValue)
                    });

            foreach (var (_, viewModel) in viewModels)
            {
                viewModel.Options = TransformDuplicateObservationalUnitsWithUniqueLabels(viewModel.Options)
                    .OrderBy(value => value.Label);
            }

            return viewModels;
        }

        private static TimePeriodsMetaViewModel BuildTimePeriodsViewModels(
            IEnumerable<(int Year, TimeIdentifier TimeIdentifier)> timePeriods)
        {
            var options = timePeriods.Select(tuple => new TimePeriodMetaViewModel(tuple.Year, tuple.TimeIdentifier));
            return new TimePeriodsMetaViewModel
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

        private async Task<Either<ActionResult, Subject>> CheckCanViewSubjectData(Subject subject)
        {
            if (await _userService.MatchesPolicy(subject, CanViewSubjectData))
            {
                return subject;
            }

            return new ForbidResult();
        }
    }
}