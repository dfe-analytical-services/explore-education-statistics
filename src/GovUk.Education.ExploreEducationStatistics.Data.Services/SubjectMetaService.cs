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
        private readonly ILogger _logger;
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
            return new SubjectMetaViewModel
            {
                Filters = GetFilters(subject.Id),
                Indicators = GetIndicators(subject.Id),
                Locations = GetObservationalUnits(subject.Id),
                TimePeriod = GetTimePeriods(subject.Id)
            };
        }

        private SubjectMetaViewModel GetSubjectMetaViewModelFromQuery(SubjectMetaQueryContext query)
        {
            var observations = _observationService.FindObservations(query).AsQueryable();
            var locations = new Dictionary<string, ObservationalUnitsMetaViewModel>();
            var timePeriods = new TimePeriodsMetaViewModel();
            var filters = new Dictionary<string, FilterMetaViewModel>();
            var indicators = new Dictionary<string, IndicatorsMetaViewModel>();
            
            var stopwatch = Stopwatch.StartNew();
            stopwatch.Start();

            if (query.Locations == null)
            {
                locations = GetObservationalUnits(observations);
                
                _logger.LogTrace("Got Observational Units in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                stopwatch.Restart();
            }
            
            if (query.TimePeriod == null && query.Locations != null)
            {
                timePeriods = GetTimePeriods(observations);

                _logger.LogTrace("Got Time Periods in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                stopwatch.Restart();
            }
            
            if (query.TimePeriod != null)
            {
                filters = GetFilters(query.SubjectId, observations, false);

                _logger.LogTrace("Got Filters in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                stopwatch.Restart();

                indicators = GetIndicators(query.SubjectId);

                _logger.LogTrace("Got Indicators in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
            }

            stopwatch.Stop();
            
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
            var locationLabelValues = _locationRepository.GetLocationsLabelValue(subjectId);
            return BuildObservationalUnitsViewModels(locationLabelValues);
        }

        private Dictionary<string, ObservationalUnitsMetaViewModel> GetObservationalUnits(
            IQueryable<Observation> observations)
        {
            var locationLabelValues = _locationRepository.GetLocationsLabelValue(observations);
            return BuildObservationalUnitsViewModels(locationLabelValues);
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
            Dictionary<GeographicLevel, IEnumerable<LabelValue>> observationalUnits)
        {
            var viewModels = observationalUnits
                .OrderBy(pair => pair.Key.GetEnumLabel())
                .ToDictionary(
                    pair => pair.Key.ToString().CamelCase(),
                    pair => new ObservationalUnitsMetaViewModel
                    {
                        Hint = "",
                        Legend = pair.Key.GetEnumLabel(),
                        Options = pair.Value,
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
