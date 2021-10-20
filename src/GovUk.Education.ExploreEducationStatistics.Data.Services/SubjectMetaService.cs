using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
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
        private readonly IFilterRepository _filterRepository;
        private readonly IFilterItemRepository _filterItemRepository;
        private readonly IIndicatorGroupRepository _indicatorGroupRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly ILogger<SubjectMetaService> _logger;
        private readonly IObservationService _observationService;
        private readonly IPersistenceHelper<StatisticsDbContext> _persistenceHelper;
        private readonly ITimePeriodService _timePeriodService;
        private readonly IUserService _userService;

        public SubjectMetaService(IBoundaryLevelRepository boundaryLevelRepository,
            IFilterRepository filterRepository,
            IFilterItemRepository filterItemRepository,
            IGeoJsonRepository geoJsonRepository,
            IIndicatorGroupRepository indicatorGroupRepository,
            ILocationRepository locationRepository,
            ILogger<SubjectMetaService> logger,
            IObservationService observationService,
            IPersistenceHelper<StatisticsDbContext> persistenceHelper,
            ITimePeriodService timePeriodService,
            IUserService userService) : base(boundaryLevelRepository, filterItemRepository, geoJsonRepository)
        {
            _filterRepository = filterRepository;
            _filterItemRepository = filterItemRepository;
            _indicatorGroupRepository = indicatorGroupRepository;
            _locationRepository = locationRepository;
            _logger = logger;
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
                .OnSuccess(subject => GetSubjectMetaViewModelFromQuery(query));
        }
        
        public Task<Either<ActionResult, SubjectMetaViewModel>> GetSubjectMetaRestricted(
            SubjectMetaQueryContext query)
        {
            return _persistenceHelper.CheckEntityExists<Subject>(query.SubjectId)
                .OnSuccess(CheckCanViewSubjectData)
                .OnSuccess(subject => GetSubjectMetaViewModelFromQuery(query));
        }
        
        private SubjectMetaViewModel GetSubjectMetaViewModel(Subject subject)
        {
            var filters = _logger.TraceTime(
                () => GetFilters(subject.Id), "Getting Filters");
            
            var indicators = _logger.TraceTime(
                () => GetIndicators(subject.Id), "Getting Indicators");
            
            var observationalUnits = _logger.TraceTime(
                () => GetObservationalUnits(subject.Id), "Getting Observational Units");
            
            var timePeriods = _logger.TraceTime(
                () => GetTimePeriods(subject.Id), 
                "Getting Time Periods");

            return new SubjectMetaViewModel
            {
                Filters = filters,
                Indicators = indicators,
                Locations = observationalUnits,
                TimePeriod = timePeriods
            };
        }

        private SubjectMetaViewModel GetSubjectMetaViewModelFromQuery(SubjectMetaQueryContext query)
        {
            var observations = _observationService
                .FindObservations(query)
                .AsQueryable();
            
            var locations = new Dictionary<string, ObservationalUnitsMetaViewModel>();
            var timePeriods = new TimePeriodsMetaViewModel();
            var filters = new Dictionary<string, FilterMetaViewModel>();
            var indicators = new Dictionary<string, IndicatorsMetaViewModel>();
            
            if (query.TimePeriod != null)
            {
                filters = _logger.TraceTime(
                    () => GetFilters(query.SubjectId, observations, false), 
                    "Getting Filters");

                indicators = _logger.TraceTime(() => 
                        GetIndicators(query.SubjectId), 
                    "Getting Indicators");
            }
            
            if (query.Locations == null)
            {
                locations = _logger.TraceTime(
                    () => GetObservationalUnits(observations), 
                        "Getting Observational Units");
            }
            
            if (query.TimePeriod == null && query.Locations != null)
            {
                timePeriods = _logger.TraceTime(
                    () => GetTimePeriods(observations),
                        "Getting Time Periods");
            }
            
            // Only data relevant to the step being executed in the table tool needs to be returned hence the 
            // null checks above so only the minimum requisite DB calls for the task are performed.
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
            return _filterRepository.GetFiltersIncludingItems(subjectId)
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
            var observationalUnits = _locationRepository.GetObservationalUnits(subjectId);
            return BuildObservationalUnitsViewModels(observationalUnits);
        }

        private Dictionary<string, ObservationalUnitsMetaViewModel> GetObservationalUnits(
            IQueryable<Observation> observations)
        {
            var observationalUnits = _locationRepository.GetObservationalUnits(observations);
            return BuildObservationalUnitsViewModels(observationalUnits);
        }

        private Dictionary<string, IndicatorsMetaViewModel> GetIndicators(Guid subjectId)
        {
            return _indicatorGroupRepository.GetIndicatorGroups(subjectId)
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
            Dictionary<GeographicLevel, IEnumerable<ObservationalUnit>> observationalUnits)
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
            return _filterItemRepository.GetTotal(filter)?.Id.ToString() ?? string.Empty;
        }

        private static LabelValue MapObservationalUnitToLabelValue(ObservationalUnit unit)
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
