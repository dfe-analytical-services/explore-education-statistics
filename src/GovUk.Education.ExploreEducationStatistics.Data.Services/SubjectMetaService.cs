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
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.Security.DataSecurityPolicies;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class SubjectMetaService : AbstractSubjectMetaService, ISubjectMetaService
    {
        private readonly IFeatureManager _featureManager;
        private readonly IFilterRepository _filterRepository;
        private readonly IFilterItemRepository _filterItemRepository;
        private readonly IIndicatorGroupRepository _indicatorGroupRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly ILogger _logger;
        private readonly IObservationService _observationService;
        private readonly IPersistenceHelper<StatisticsDbContext> _persistenceHelper;
        private readonly ITimePeriodService _timePeriodService;
        private readonly IUserService _userService;
        private readonly LocationsOptions _locationOptions;

        public SubjectMetaService(
            IFeatureManager featureManager,
            IFilterRepository filterRepository,
            IFilterItemRepository filterItemRepository,
            IIndicatorGroupRepository indicatorGroupRepository,
            ILocationRepository locationRepository,
            ILogger<SubjectMetaService> logger,
            IObservationService observationService,
            IPersistenceHelper<StatisticsDbContext> persistenceHelper,
            ITimePeriodService timePeriodService,
            IUserService userService,
            IOptions<LocationsOptions> locationOptions) :
            base(filterItemRepository)
        {
            _featureManager = featureManager;
            _filterRepository = filterRepository;
            _filterItemRepository = filterItemRepository;
            _indicatorGroupRepository = indicatorGroupRepository;
            _locationRepository = locationRepository;
            _logger = logger;
            _observationService = observationService;
            _persistenceHelper = persistenceHelper;
            _timePeriodService = timePeriodService;
            _userService = userService;
            _locationOptions = locationOptions.Value;
        }

        public Task<Either<ActionResult, ISubjectMetaViewModel>> GetSubjectMeta(Guid subjectId)
        {
            return _persistenceHelper.CheckEntityExists<Subject>(subjectId)
                .OnSuccess(GetSubjectMetaViewModel);
        }

        public async Task<Either<ActionResult, ISubjectMetaViewModel>> GetSubjectMetaRestricted(Guid subjectId)
        {
            return await _persistenceHelper.CheckEntityExists<Subject>(subjectId)
                .OnSuccess(CheckCanViewSubjectData)
                .OnSuccess(GetSubjectMetaViewModel);
        }

        public Task<Either<ActionResult, ISubjectMetaViewModel>> GetSubjectMeta(
            SubjectMetaQueryContext query)
        {
            return _persistenceHelper.CheckEntityExists<Subject>(query.SubjectId)
                .OnSuccess(_ => GetSubjectMetaViewModelFromQuery(query));
        }

        public Task<Either<ActionResult, ISubjectMetaViewModel>> GetSubjectMetaRestricted(
            SubjectMetaQueryContext query)
        {
            return _persistenceHelper.CheckEntityExists<Subject>(query.SubjectId)
                .OnSuccess(CheckCanViewSubjectData)
                .OnSuccess(_ => GetSubjectMetaViewModelFromQuery(query));
        }

        private async Task<ISubjectMetaViewModel> GetSubjectMetaViewModel(Subject subject)
        {
            if (await _featureManager.IsEnabledAsync("LocationHierarchies"))
            {
                return new SubjectMetaViewModel
                {
                    Filters = GetFilters(subject.Id),
                    Indicators = GetIndicators(subject.Id),
                    Locations = await GetLocations(subject.Id),
                    TimePeriod = GetTimePeriods(subject.Id)
                };
            }

            return new LegacySubjectMetaViewModel
            {
                Filters = GetFilters(subject.Id),
                Indicators = GetIndicators(subject.Id),
                Locations = await GetLegacyLocations(subject.Id),
                TimePeriod = GetTimePeriods(subject.Id)
            };
        }

        private async Task<ISubjectMetaViewModel> GetSubjectMetaViewModelFromQuery(SubjectMetaQueryContext query)
        {
            var locationHierarchiesEnabled = await _featureManager.IsEnabledAsync("LocationHierarchies");

            var observations = _observationService.FindObservations(query).AsQueryable();
            var legacyLocations = new Dictionary<string, ObservationalUnitsMetaViewModel>();
            var locations = new Dictionary<string, LocationsMetaViewModel>();
            var timePeriods = new TimePeriodsMetaViewModel();
            var filters = new Dictionary<string, FilterMetaViewModel>();
            var indicators = new Dictionary<string, IndicatorsMetaViewModel>();

            var stopwatch = Stopwatch.StartNew();
            stopwatch.Start();

            if (query.Locations == null)
            {
                if (locationHierarchiesEnabled)
                {
                    locations = await GetLocations(observations);
                }
                else
                {
                    legacyLocations = await GetLegacyLocations(observations);
                }

                _logger.LogTrace("Got Locations in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
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

            if (locationHierarchiesEnabled)
            {
                return new SubjectMetaViewModel
                {
                    Filters = filters,
                    Indicators = indicators,
                    Locations = locations,
                    TimePeriod = timePeriods
                };
            }

            return new LegacySubjectMetaViewModel
            {
                Filters = filters,
                Indicators = indicators,
                Locations = legacyLocations,
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

        [ObsoleteAttribute("TODO EES-2902 - Remove with SOW8 after EES-2773", false)]
        private async Task<Dictionary<string, ObservationalUnitsMetaViewModel>> GetLegacyLocations(Guid subjectId)
        {
            var locations = await _locationRepository.GetLocationAttributes(subjectId);
            return BuildLegacyLocationAttributeViewModel(locations);
        }

        [ObsoleteAttribute("TODO EES-2902 - Remove with SOW8 after EES-2773", false)]
        private async Task<Dictionary<string, ObservationalUnitsMetaViewModel>> GetLegacyLocations(
            IQueryable<Observation> observations)
        {
            var locations = await _locationRepository.GetLocationAttributes(observations);
            return BuildLegacyLocationAttributeViewModel(locations);
        }

        private async Task<Dictionary<string, LocationsMetaViewModel>> GetLocations(Guid subjectId)
        {
            var locations =
                await _locationRepository.GetLocationAttributesHierarchical(subjectId, _locationOptions.Hierarchies);
            return BuildLocationAttributeViewModels(locations);
        }

        private async Task<Dictionary<string, LocationsMetaViewModel>> GetLocations(
            IQueryable<Observation> observations)
        {
            var locations =
                await _locationRepository.GetLocationAttributesHierarchical(observations, _locationOptions.Hierarchies);
            return BuildLocationAttributeViewModels(locations);
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

        [ObsoleteAttribute("TODO EES-2902 - Remove with SOW8 after EES-2773", false)]
        private static Dictionary<string, ObservationalUnitsMetaViewModel> BuildLegacyLocationAttributeViewModel(
            Dictionary<GeographicLevel, IEnumerable<ILocationAttribute>> locationAttributes)
        {
            var viewModels = locationAttributes
                .OrderBy(pair => pair.Key.GetEnumLabel())
                .ToDictionary(
                    pair => pair.Key.ToString().CamelCase(),
                    pair => new ObservationalUnitsMetaViewModel
                    {
                        Hint = "",
                        Legend = pair.Key.GetEnumLabel(),
                        Options = pair.Value.Select(MapLocationAttributeToLabelValue)
                    });

            foreach (var (_, viewModel) in viewModels)
            {
                viewModel.Options = TransformDuplicateLocationAttributesWithUniqueLabels(viewModel.Options)
                    .OrderBy(value => value.Label);
            }

            return viewModels;
        }

        private static Dictionary<string, LocationsMetaViewModel> BuildLocationAttributeViewModels(
            Dictionary<GeographicLevel, List<LocationAttributeNode>> locationAttributes)
        {
            return locationAttributes
                .ToDictionary(
                    pair => pair.Key.ToString().CamelCase(),
                    pair =>
                        new LocationsMetaViewModel
                        {
                            Legend = pair.Key.GetEnumLabel(),
                            Options = pair.Value.Select(BuildLocationAttributeViewModel).ToList()
                        });
        }

        private static LocationAttributeViewModel BuildLocationAttributeViewModel(
            LocationAttributeNode locationAttributeNode)
        {
            return locationAttributeNode.IsLeaf
                ? new LocationAttributeViewModel
                {
                    Label = locationAttributeNode.Attribute.Name ?? string.Empty,
                    Value = locationAttributeNode.Attribute.Code ?? string.Empty
                }
                : new LocationAttributeViewModel
                {
                    Label = locationAttributeNode.Attribute.Name ?? string.Empty,
                    Level = locationAttributeNode.Attribute.GetType().Name,
                    Value = locationAttributeNode.Attribute.Code ?? string.Empty,
                    Options = locationAttributeNode.Children.Select(BuildLocationAttributeViewModel).ToList()
                };
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

        private static LabelValue MapLocationAttributeToLabelValue(ILocationAttribute unit)
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

    public class LocationsOptions
    {
        public const string Locations = "Locations";

        /// <summary>
        /// Map of <c>GeographicLevel</c> to attribute names of the <c>Location</c> type.
        /// This is used to configure a hierarchy of location attributes for a geographic level.
        /// Example: For Local Authority level data where Country, Region and Local Authority attributes are provided,
        /// a hierarchy Country -> Region -> Local Authority can be configured for this level using configuration data
        /// <c>
        /// "Locations": {
        ///   "Hierarchies": {
        ///     "LocalAuthority": [
        ///       "Country",
        ///       "Region",
        ///       "LocalAuthority"
        ///     ]
        ///   }
        /// }
        /// </c>
        /// </summary>
        /// <remarks>
        /// Attributes of the <c>Location</c> type must be specified in order of the hierarchy from top to bottom.
        /// </remarks>
        public Dictionary<GeographicLevel, List<string>> Hierarchies { get; init; } = new();
    }
}
