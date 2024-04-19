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
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Data.ViewModels.LocationViewModelBuilder;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class SubjectMetaService(
        StatisticsDbContext statisticsDbContext,
        ContentDbContext contentDbContext,
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
        : ISubjectMetaService
    {
        private readonly LocationsOptions _locationOptions = locationOptions.Value;

        private enum SubjectMetaQueryStep
        {
            GetTimePeriods,
            GetFilterItems
        }

        public async Task<Either<ActionResult, SubjectMetaViewModel>> GetSubjectMeta(Guid releaseVersionId,
            Guid subjectId)
        {
            return await releaseSubjectService.Find(subjectId: subjectId,
                    releaseVersionId: releaseVersionId)
                .OnSuccess(GetSubjectMeta);
        }

        public async Task<Either<ActionResult, SubjectMetaViewModel>> GetSubjectMeta(ReleaseSubject releaseSubject)
        {
            return await userService.CheckCanViewSubjectData(releaseSubject)
                .OnSuccess(async rs =>
                {
                    var releaseFile = await contentDbContext.ReleaseFiles
                        .Where(rf => rf.ReleaseVersionId == rs.ReleaseVersionId
                                     && rf.File.SubjectId == rs.SubjectId
                                     && rf.File.Type == FileType.Data)
                        .SingleAsync();

                    return new SubjectMetaViewModel
                    {
                        Filters = await GetFilters(releaseSubject.SubjectId, releaseFile.FilterSequence),
                        Indicators = await GetIndicators(releaseSubject.SubjectId, releaseFile.IndicatorSequence),
                        Locations = await GetLocations(releaseSubject.SubjectId),
                        TimePeriod = await GetTimePeriods(releaseSubject.SubjectId)
                    };
                });
        }

        public async Task<Either<ActionResult, SubjectMetaViewModel>> FilterSubjectMeta(Guid? releaseVersionId,
            ObservationQueryContext query,
            CancellationToken cancellationToken)
        {
            return await releaseSubjectService.Find(subjectId: query.SubjectId,
                    releaseVersionId: releaseVersionId)
                .OnSuccess(userService.CheckCanViewSubjectData)
                .OnSuccess(releaseSubject =>
                    GetSubjectMetaViewModelFromQuery(query, releaseSubject, cancellationToken));
        }

        public async Task<Either<ActionResult, Unit>> UpdateSubjectFilters(
            Guid releaseVersionId,
            Guid subjectId,
            List<FilterUpdateViewModel> request)
        {
            return await contentDbContext.ReleaseFiles.SingleOrNotFoundAsync(rf =>
                    rf.ReleaseVersionId == releaseVersionId
                    && rf.File.SubjectId == subjectId
                    && rf.File.Type == FileType.Data)
                .OnSuccessDo(() => ValidateFiltersForSubject(subjectId, request))
                .OnSuccessVoid(async releaseFile =>
                {
                    // Set the sequence based on the order of filters, filter groups and indicators observed
                    // in the request
                    releaseFile.FilterSequence = request.Select(filter =>
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
                    await contentDbContext.SaveChangesAsync();
                    await InvalidateCachedReleaseSubjectMetadata(releaseVersionId: releaseVersionId,
                        subjectId: subjectId);
                });
        }

        public async Task<Either<ActionResult, Unit>> UpdateSubjectIndicators(
            Guid releaseVersionId,
            Guid subjectId,
            List<IndicatorGroupUpdateViewModel> request)
        {
            return await contentDbContext.ReleaseFiles.SingleOrNotFoundAsync(rf =>
                    rf.ReleaseVersionId == releaseVersionId
                    && rf.File.SubjectId == subjectId
                    && rf.File.Type == FileType.Data)
                .OnSuccessDo(() => ValidateIndicatorGroupsForSubject(subjectId, request))
                .OnSuccessVoid(async releaseFile =>
                {
                    // Set the sequence based on the order of indicator groups and indicators observed
                    // in the request
                    releaseFile.IndicatorSequence = request.Select(indicatorGroup =>
                            new IndicatorGroupSequenceEntry(
                                indicatorGroup.Id,
                                indicatorGroup.Indicators
                            ))
                        .ToList();
                    await contentDbContext.SaveChangesAsync();

                    await InvalidateCachedReleaseSubjectMetadata(
                        releaseVersionId: releaseVersionId,
                        subjectId: subjectId);
                });
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

                    var observations = statisticsDbContext
                        .Observation
                        .AsNoTracking()
                        .Where(o => o.SubjectId == query.SubjectId && query.LocationIds.Contains(o.LocationId));

                    var timePeriods = await GetTimePeriods(observations);

                    logger.LogTrace("Got Time Periods in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);

                    return new SubjectMetaViewModel
                    {
                        TimePeriod = timePeriods
                    };
                }

                case SubjectMetaQueryStep.GetFilterItems:
                {
                    var stopwatch = Stopwatch.StartNew();

                    var releaseFile = await contentDbContext.ReleaseFiles
                        .Where(rf => rf.ReleaseVersionId == releaseSubject.ReleaseVersionId
                                     && rf.File.SubjectId == releaseSubject.SubjectId
                                     && rf.File.Type == FileType.Data)
                        .SingleAsync(cancellationToken: cancellationToken);

                    var observations =
                        await observationService.GetMatchedObservations(query, cancellationToken);
                    logger.LogTrace("Got Observations in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var filterItems = await
                        filterItemRepository.GetFilterItemsFromMatchedObservationIds(query.SubjectId, observations);
                    var filters =
                        FiltersMetaViewModelBuilder.BuildFiltersFromFilterItems(
                            filterItems, releaseFile.FilterSequence);
                    logger.LogTrace("Got Filters in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var indicators = await GetIndicators(
                        releaseSubject.SubjectId, releaseFile.IndicatorSequence);
                    logger.LogTrace("Got Indicators in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);

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

        private async Task<Dictionary<string, FilterMetaViewModel>> GetFilters(
            Guid subjectId, List<FilterSequenceEntry>? filterSequence)
        {
            var filters = await filterRepository.GetFiltersIncludingItems(subjectId);
            return FiltersMetaViewModelBuilder.BuildFilters(filters, filterSequence);
        }

        private async Task<TimePeriodsMetaViewModel> GetTimePeriods(Guid subjectId)
        {
            var timePeriods = await timePeriodService.GetTimePeriods(subjectId);
            return BuildTimePeriodsViewModels(timePeriods);
        }

        private async Task<TimePeriodsMetaViewModel> GetTimePeriods(IQueryable<Observation> observations)
        {
            var timePeriods = await timePeriodService.GetTimePeriods(observations);
            return BuildTimePeriodsViewModels(timePeriods);
        }

        private async Task<Dictionary<string, LocationsMetaViewModel>> GetLocations(Guid subjectId)
        {
            var locations = await locationRepository.GetDistinctForSubject(subjectId);
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

        private async Task<Dictionary<string, IndicatorGroupMetaViewModel>> GetIndicators(
            Guid subjectId, List<IndicatorGroupSequenceEntry>? indicatorSequence)
        {
            var indicators = await indicatorGroupRepository.GetIndicatorGroups(subjectId);
            return IndicatorsMetaViewModelBuilder.BuildIndicatorGroups(indicators,
                indicatorSequence);
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

        private Task InvalidateCachedReleaseSubjectMetadata(Guid releaseVersionId, Guid subjectId)
        {
            return cacheService.DeleteItemAsync(new PrivateSubjectMetaCacheKey(releaseVersionId, subjectId));
        }

        private async Task<Either<ActionResult, Unit>> ValidateFiltersForSubject(
            Guid subjectId,
            List<FilterUpdateViewModel> requestFilters)
        {
            var filters = await filterRepository.GetFiltersIncludingItems(subjectId);
            return AssertCollectionsAreSameIgnoringOrder(filters,
                requestFilters,
                filter => filter.Id,
                requestFilter => requestFilter.Id,
                FiltersDifferFromSubject).OnSuccess(_ =>
            {
                var requestMap = requestFilters.ToDictionary(filter => filter.Id);
                return filters
                    .Select(filter =>
                        ValidateFilterGroupsForSubject(filter, requestMap[filter.Id].FilterGroups))
                    .OnSuccessAllReturnVoid();
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
                    return filter
                        .FilterGroups
                        .Select(filterGroup =>
                            AssertCollectionsAreSameIgnoringOrder(
                                filterGroup.FilterItems.Select(filterItem => filterItem.Id),
                                requestMap[filterGroup.Id].FilterItems,
                                FilterItemsDifferFromSubject))
                        .OnSuccessAllReturnVoid();
                });
        }

        private async Task<Either<ActionResult, Unit>> ValidateIndicatorGroupsForSubject(
            Guid subjectId,
            List<IndicatorGroupUpdateViewModel> requestIndicatorGroups)
        {
            var indicatorGroups = await indicatorGroupRepository.GetIndicatorGroups(subjectId);
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
                        .OnSuccessAllReturnVoid();
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
