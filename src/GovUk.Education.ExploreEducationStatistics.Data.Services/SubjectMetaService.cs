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
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.Security.DataSecurityPolicies;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class SubjectMetaService : AbstractSubjectMetaService, ISubjectMetaService
    {
        private readonly IFilterRepository _filterRepository;
        private readonly IIndicatorGroupRepository _indicatorGroupRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly ILogger _logger;
        private readonly IObservationService _observationService;
        private readonly IPersistenceHelper<StatisticsDbContext> _persistenceHelper;
        private readonly ITimePeriodService _timePeriodService;
        private readonly IUserService _userService;
        private readonly LocationsOptions _locationOptions;

        public SubjectMetaService(
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
            _filterRepository = filterRepository;
            _indicatorGroupRepository = indicatorGroupRepository;
            _locationRepository = locationRepository;
            _logger = logger;
            _observationService = observationService;
            _persistenceHelper = persistenceHelper;
            _timePeriodService = timePeriodService;
            _userService = userService;
            _locationOptions = locationOptions.Value;
        }

        public Task<Either<ActionResult, SubjectMetaViewModel>> GetSubjectMeta(Guid subjectId)
        {
            return _persistenceHelper.CheckEntityExists<Subject>(subjectId)
                .OnSuccess(GetSubjectMetaViewModel);
        }

        public async Task<Either<ActionResult, SubjectMetaViewModel>> GetSubjectMetaRestricted(Guid subjectId)
        {
            return await _persistenceHelper.CheckEntityExists<Subject>(subjectId)
                .OnSuccess(CheckCanViewSubjectData)
                .OnSuccess(GetSubjectMetaViewModel);
        }

        public Task<Either<ActionResult, SubjectMetaViewModel>> GetSubjectMeta(
            ObservationQueryContext query)
        {
            return _persistenceHelper.CheckEntityExists<Subject>(query.SubjectId)
                .OnSuccess(_ => GetSubjectMetaViewModelFromQuery(query));
        }

        public Task<Either<ActionResult, SubjectMetaViewModel>> GetSubjectMetaRestricted(
            ObservationQueryContext query)
        {
            return _persistenceHelper.CheckEntityExists<Subject>(query.SubjectId)
                .OnSuccess(CheckCanViewSubjectData)
                .OnSuccess(_ => GetSubjectMetaViewModelFromQuery(query));
        }

        private async Task<SubjectMetaViewModel> GetSubjectMetaViewModel(Subject subject)
        {
            return new()
            {
                Filters = GetFilters(subject.Id),
                Indicators = GetIndicators(subject.Id),
                Locations = await GetLocations(subject.Id),
                TimePeriod = GetTimePeriods(subject.Id)
            };
        }

        private async Task<SubjectMetaViewModel> GetSubjectMetaViewModelFromQuery(ObservationQueryContext query)
        {
            var observations = _observationService.FindObservations(query).AsQueryable();
            var locations = new Dictionary<string, LocationsMetaViewModel>();
            var timePeriods = new TimePeriodsMetaViewModel();
            var filters = new Dictionary<string, FilterMetaViewModel>();
            var indicators = new Dictionary<string, IndicatorsMetaViewModel>();

            var stopwatch = Stopwatch.StartNew();
            stopwatch.Start();

            if (query.Locations == null)
            {
                locations = await GetLocations(observations);

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
                var filterItems = _filterItemRepository.GetFilterItemsFromObservationQuery(query.SubjectId, observations);
                filters = BuildFilterHierarchy(filterItems);

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
                await _locationRepository.GetLocationAttributesHierarchicalByObservationsQuery(
                    observations,
                    _locationOptions.Hierarchies);
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

        private static Dictionary<string, LocationsMetaViewModel> BuildLocationAttributeViewModels(
            Dictionary<GeographicLevel, List<LocationAttributeNode>> locationAttributes)
        {
            return locationAttributes
                .ToDictionary(
                    pair => pair.Key.ToString().CamelCase(),
                    pair => new LocationsMetaViewModel
                    {
                        Legend = pair.Key.GetEnumLabel(),
                        Options = DeduplicateLocationViewModels(
                                pair.Value
                                    .OrderBy(OrderLocationAttributes)
                                    .Select(BuildLocationAttributeViewModel)
                            )
                            .ToList()
                    }
                );
        }

        private static LocationAttributeViewModel BuildLocationAttributeViewModel(
            LocationAttributeNode locationAttributeNode)
        {
            return locationAttributeNode.IsLeaf
                ? new LocationAttributeViewModel
                {
                    Label = locationAttributeNode.Attribute.Name ?? string.Empty,
                    Value = locationAttributeNode.Attribute.GetCodeOrFallback()
                }
                : new LocationAttributeViewModel
                {
                    Label = locationAttributeNode.Attribute.Name ?? string.Empty,
                    Level = locationAttributeNode.Attribute.GetType().Name.CamelCase(),
                    Value = locationAttributeNode.Attribute.GetCodeOrFallback(),
                    Options = DeduplicateLocationViewModels(
                            locationAttributeNode.Children
                                .OrderBy(OrderLocationAttributes)
                                .Select(BuildLocationAttributeViewModel)
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
