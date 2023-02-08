#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.LocationViewModelBuilder;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class SubjectMetaService : ISubjectMetaService
    {
        private enum SubjectMetaQueryStep
        {
            GetTimePeriods,
            GetFilterItems
        }

        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly IBlobCacheService _cacheService;
        private readonly IReleaseSubjectService _releaseSubjectService;
        private readonly IFilterRepository _filterRepository;
        private readonly IFilterItemRepository _filterItemRepository;
        private readonly IIndicatorGroupRepository _indicatorGroupRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly ILogger _logger;
        private readonly IObservationService _observationService;
        private readonly ITimePeriodService _timePeriodService;
        private readonly IUserService _userService;
        private readonly LocationsOptions _locationOptions;

        public SubjectMetaService(
            StatisticsDbContext statisticsDbContext,
            IBlobCacheService cacheService,
            IReleaseSubjectService releaseSubjectService,
            IFilterRepository filterRepository,
            IFilterItemRepository filterItemRepository,
            IIndicatorGroupRepository indicatorGroupRepository,
            ILocationRepository locationRepository,
            ILogger<SubjectMetaService> logger,
            IObservationService observationService,
            ITimePeriodService timePeriodService,
            IUserService userService,
            IOptions<LocationsOptions> locationOptions)
        {
            _statisticsDbContext = statisticsDbContext;
            _cacheService = cacheService;
            _releaseSubjectService = releaseSubjectService;
            _filterRepository = filterRepository;
            _filterItemRepository = filterItemRepository;
            _indicatorGroupRepository = indicatorGroupRepository;
            _locationRepository = locationRepository;
            _logger = logger;
            _observationService = observationService;
            _timePeriodService = timePeriodService;
            _userService = userService;
            _locationOptions = locationOptions.Value;
        }

        public async Task<Either<ActionResult, SubjectMetaViewModel>> GetSubjectMeta(Guid releaseId, Guid subjectId)
        {
            return await _releaseSubjectService.Find(subjectId: subjectId, releaseId: releaseId)
                .OnSuccess(GetSubjectMeta);
        }

        public async Task<Either<ActionResult, SubjectMetaViewModel>> GetSubjectMeta(ReleaseSubject releaseSubject)
        {
            return await _userService.CheckCanViewSubjectData(releaseSubject)
                .OnSuccess(GetSubjectMetaViewModel);
        }

        public async Task<Either<ActionResult, SubjectMetaViewModel>> FilterSubjectMeta(Guid? releaseId,
            ObservationQueryContext query,
            CancellationToken cancellationToken)
        {
            return await _releaseSubjectService.Find(subjectId: query.SubjectId, releaseId: releaseId)
                .OnSuccess(_userService.CheckCanViewSubjectData)
                .OnSuccess(releaseSubject =>
                    GetSubjectMetaViewModelFromQuery(query, releaseSubject, cancellationToken));
        }

        public async Task<Either<ActionResult, Unit>> UpdateSubjectFilters(
            Guid releaseId,
            Guid subjectId,
            List<FilterUpdateViewModel> request)
        {
            return await _releaseSubjectService.Find(subjectId: subjectId, releaseId: releaseId)
                .OnSuccessDo(() => ValidateFiltersForSubject(subjectId, request))
                .OnSuccessVoid(async rs =>
                {
                    // Set the sequence based on the order of filters, filter groups and indicators observed
                    // in the request
                    rs.FilterSequence = request.Select(filter =>
                            new FilterSequenceEntry(
                                filter.Id,
                                filter.FilterGroups.Select(filterGroup =>
                                        new FilterGroupSequenceEntry(
                                            filterGroup.Id,
                                            filterGroup.FilterItems
                                        ))
                                    .ToList()
                            ))
                        .ToList();
                    await _statisticsDbContext.SaveChangesAsync();
                    await InvalidateCachedReleaseSubjectMetadata(releaseId, subjectId);
                });
        }

        public async Task<Either<ActionResult, Unit>> UpdateSubjectIndicators(
            Guid releaseId,
            Guid subjectId,
            List<IndicatorGroupUpdateViewModel> request)
        {
            return await _releaseSubjectService.Find(subjectId: subjectId, releaseId: releaseId)
                .OnSuccessDo(() => ValidateIndicatorGroupsForSubject(subjectId, request))
                .OnSuccessVoid(async releaseSubject =>
                {
                    // Set the sequence based on the order of indicator groups and indicators observed
                    // in the request
                    releaseSubject.IndicatorSequence = request.Select(indicatorGroup =>
                            new IndicatorGroupSequenceEntry(
                                indicatorGroup.Id,
                                indicatorGroup.Indicators
                            ))
                        .ToList();
                    await _statisticsDbContext.SaveChangesAsync();
                    await InvalidateCachedReleaseSubjectMetadata(releaseId, subjectId);
                });
        }

        private async Task<SubjectMetaViewModel> GetSubjectMetaViewModel(ReleaseSubject releaseSubject)
        {
            return new SubjectMetaViewModel
            {
                Filters = await GetFilters(releaseSubject),
                Indicators = await GetIndicators(releaseSubject),
                Locations = await GetLocations(releaseSubject.SubjectId),
                TimePeriod = await GetTimePeriods(releaseSubject.SubjectId)
            };
        }

        private async Task<SubjectMetaViewModel> GetSubjectMetaViewModelFromQuery(
            ObservationQueryContext query,
            ReleaseSubject releaseSubject,
            CancellationToken cancellationToken)
        {
            SubjectMetaQueryStep? subjectMetaStep = null;
            if (!query.LocationIds.IsNullOrEmpty() && query.TimePeriod == null)
            {
                subjectMetaStep = SubjectMetaQueryStep.GetTimePeriods;
            }
            else if (query.TimePeriod != null && query.Filters.IsNullOrEmpty())
            {
                subjectMetaStep = SubjectMetaQueryStep.GetFilterItems;
            }

            // Only data relevant to the step being executed in the table tool needs to be returned, so only the
            // minimum requisite DB calls for the task are performed.
            switch (subjectMetaStep)
            {
                case SubjectMetaQueryStep.GetTimePeriods:
                {
                    var stopwatch = Stopwatch.StartNew();

                    var observations = _statisticsDbContext
                        .Observation
                        .AsNoTracking()
                        .Where(o => o.SubjectId == query.SubjectId && query.LocationIds.Contains(o.LocationId));

                    var timePeriods = await GetTimePeriods(observations);

                    _logger.LogTrace("Got Time Periods in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);

                    return new SubjectMetaViewModel
                    {
                        TimePeriod = timePeriods
                    };
                }

                case SubjectMetaQueryStep.GetFilterItems:
                {
                    var stopwatch = Stopwatch.StartNew();

                    var observations =
                        await _observationService.GetMatchedObservations(query, cancellationToken);
                    _logger.LogTrace("Got Observations in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var filterItems = await
                        _filterItemRepository.GetFilterItemsFromMatchedObservationIds(query.SubjectId, observations);
                    var filters =
                        FiltersMetaViewModelBuilder.BuildFiltersFromFilterItems(filterItems,
                            releaseSubject.FilterSequence);
                    _logger.LogTrace("Got Filters in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var indicators = await GetIndicators(releaseSubject);
                    _logger.LogTrace("Got Indicators in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);

                    return new SubjectMetaViewModel
                    {
                        Filters = filters,
                        Indicators = indicators,
                    };
                }
                default:
                    throw new ArgumentOutOfRangeException($"{nameof(subjectMetaStep)}",
                        "Unable to determine which SubjectMeta information has requested");
            }
        }

        private async Task<Dictionary<string, FilterMetaViewModel>> GetFilters(ReleaseSubject releaseSubject)
        {
            var filters = await _filterRepository.GetFiltersIncludingItems(releaseSubject.SubjectId);
            return FiltersMetaViewModelBuilder.BuildFilters(filters, releaseSubject.FilterSequence);
        }

        private async Task<TimePeriodsMetaViewModel> GetTimePeriods(Guid subjectId)
        {
            var timePeriods = await _timePeriodService.GetTimePeriods(subjectId);
            return BuildTimePeriodsViewModels(timePeriods);
        }

        private async Task<TimePeriodsMetaViewModel> GetTimePeriods(IQueryable<Observation> observations)
        {
            var timePeriods = await _timePeriodService.GetTimePeriods(observations);
            return BuildTimePeriodsViewModels(timePeriods);
        }

        private async Task<Dictionary<string, LocationsMetaViewModel>> GetLocations(Guid subjectId)
        {
            var locations = await _locationRepository.GetDistinctForSubject(subjectId);
            var locationViewModels = BuildLocationAttributeViewModels(locations,
                _locationOptions.Hierarchies);

            return locationViewModels
                .ToDictionary(
                    pair => pair.Key.ToString().CamelCase(),
                    pair => new LocationsMetaViewModel
                    {
                        Legend = pair.Key.GetEnumLabel(),
                        Options = pair.Value
                    }
                );
        }

        private async Task<Dictionary<string, IndicatorGroupMetaViewModel>> GetIndicators(ReleaseSubject releaseSubject)
        {
            var indicators = await _indicatorGroupRepository.GetIndicatorGroups(releaseSubject.SubjectId);
            return IndicatorsMetaViewModelBuilder.BuildIndicatorGroups(indicators,
                releaseSubject.IndicatorSequence);
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

        private Task InvalidateCachedReleaseSubjectMetadata(Guid releaseId, Guid subjectId)
        {
            return _cacheService.DeleteItem(new PrivateSubjectMetaCacheKey(releaseId, subjectId));
        }

        private async Task<Either<ActionResult, Unit>> ValidateFiltersForSubject(
            Guid subjectId,
            List<FilterUpdateViewModel> requestFilters)
        {
            var filters = await _filterRepository.GetFiltersIncludingItems(subjectId);
            return AssertCollectionsAreSameIgnoringOrder(filters,
                requestFilters,
                filter => filter.Id,
                requestFilter => requestFilter.Id,
                FiltersDifferFromSubject).OnSuccess(_ =>
            {
                var requestMap = requestFilters.ToDictionary(filter => filter.Id);
                return filters.Select(filter =>
                        ValidateFilterGroupsForSubject(filter, requestMap[filter.Id].FilterGroups))
                    .OnSuccessAll()
                    .OnSuccessVoid();
            });
        }

        private static Either<ActionResult, Unit> ValidateFilterGroupsForSubject(
            Filter filter,
            List<FilterGroupUpdateViewModel> requestFilterGroups)
        {
            return AssertCollectionsAreSameIgnoringOrder(filter.FilterGroups,
                    requestFilterGroups,
                    filterGroup => filterGroup.Id,
                    requestFilterGroup => requestFilterGroup.Id,
                    FilterGroupsDifferFromSubject)
                .OnSuccess(_ =>
                {
                    var requestMap = requestFilterGroups.ToDictionary(filterGroup => filterGroup.Id);
                    return filter.FilterGroups.Select(filterGroup =>
                            AssertCollectionsAreSameIgnoringOrder(
                                filterGroup.FilterItems.Select(filterItem => filterItem.Id),
                                requestMap[filterGroup.Id].FilterItems,
                                FilterItemsDifferFromSubject))
                        .OnSuccessAll()
                        .OnSuccessVoid();
                });
        }

        private async Task<Either<ActionResult, Unit>> ValidateIndicatorGroupsForSubject(
            Guid subjectId,
            List<IndicatorGroupUpdateViewModel> requestIndicatorGroups)
        {
            var indicatorGroups = await _indicatorGroupRepository.GetIndicatorGroups(subjectId);
            return AssertCollectionsAreSameIgnoringOrder(indicatorGroups,
                    requestIndicatorGroups,
                    indicatorGroup => indicatorGroup.Id,
                    requestIndicatorGroup => requestIndicatorGroup.Id,
                    IndicatorGroupsDifferFromSubject)
                .OnSuccess(_ =>
                {
                    var requestMap = requestIndicatorGroups.ToDictionary(indicatorGroup => indicatorGroup.Id);
                    return indicatorGroups.Select(indicatorGroup =>
                            AssertCollectionsAreSameIgnoringOrder(
                                indicatorGroup.Indicators.Select(indicator => indicator.Id),
                                requestMap[indicatorGroup.Id].Indicators,
                                IndicatorsDifferFromSubject))
                        .OnSuccessAll()
                        .OnSuccessVoid();
                });
        }

        private static Either<ActionResult, Unit> AssertCollectionsAreSameIgnoringOrder<TFirst, TSecond, TId>(
            IEnumerable<TFirst> first,
            IEnumerable<TSecond> second,
            Func<TFirst, TId> firstIdSelector,
            Func<TSecond, TId> secondIdSelector,
            ValidationErrorMessages error) where TId : IComparable
        {
            var firstIdList = first.Select(firstIdSelector);
            var secondIdList = second.Select(secondIdSelector);
            return AssertCollectionsAreSameIgnoringOrder(firstIdList, secondIdList, error);
        }

        private static Either<ActionResult, Unit> AssertCollectionsAreSameIgnoringOrder<T>(IEnumerable<T> first,
            IEnumerable<T> second,
            ValidationErrorMessages error) where T : IComparable
        {
            if(ComparerUtils.SequencesAreEqualIgnoringOrder(first, second))
            {
                return Unit.Instance;
            }

            return ValidationResult(error);
        }
    }
}
