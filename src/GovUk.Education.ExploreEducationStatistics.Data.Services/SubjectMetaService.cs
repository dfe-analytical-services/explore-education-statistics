#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.FilterAndIndicatorViewModelBuilders;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class SubjectMetaService : AbstractSubjectMetaService, ISubjectMetaService
    {
        private enum SubjectMetaQueryStep
        {
            GetTimePeriods,
            GetFilterItems
        }

        private readonly ContentDbContext _contentDbContext;
        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly IFilterRepository _filterRepository;
        private readonly IFilterItemRepository _filterItemRepository;
        private readonly IIndicatorGroupRepository _indicatorGroupRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly ILogger _logger;
        private readonly IObservationService _observationService;
        private readonly IReleaseSubjectRepository _releaseSubjectRepository;
        private readonly ITimePeriodService _timePeriodService;
        private readonly LocationsOptions _locationOptions;

        public SubjectMetaService(
            ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext,
            IFilterRepository filterRepository,
            IFilterItemRepository filterItemRepository,
            IIndicatorGroupRepository indicatorGroupRepository,
            ILocationRepository locationRepository,
            ILogger<SubjectMetaService> logger,
            IObservationService observationService,
            IReleaseSubjectRepository releaseSubjectRepository,
            IPersistenceHelper<StatisticsDbContext> persistenceHelper,
            ITimePeriodService timePeriodService,
            IUserService userService,
            IOptions<LocationsOptions> locationOptions) : base(
            persistenceHelper,
            userService)
        {
            _contentDbContext = contentDbContext;
            _statisticsDbContext = statisticsDbContext;
            _filterRepository = filterRepository;
            _filterItemRepository = filterItemRepository;
            _indicatorGroupRepository = indicatorGroupRepository;
            _locationRepository = locationRepository;
            _logger = logger;
            _observationService = observationService;
            _releaseSubjectRepository = releaseSubjectRepository;
            _timePeriodService = timePeriodService;
            _locationOptions = locationOptions.Value;
        }

        public async Task<Either<ActionResult, SubjectMetaViewModel>> GetCachedSubjectMeta(Guid subjectId)
        {
            return await CheckSubjectExistsOnLatestPublishedVersion(subjectId)
                .OnSuccessCombineWith(CreateCacheKeyForSubjectMeta)
                .OnSuccess(releaseSubjectAndCacheKey =>
                {
                    var (releaseSubject, cacheKey) = releaseSubjectAndCacheKey;
                    return GetCachedSubjectMeta(releaseSubject, cacheKey);
                });
        }

        [BlobCache(typeof(SubjectMetaCacheKey))]
        private Task<SubjectMetaViewModel> GetCachedSubjectMeta(ReleaseSubject releaseSubject,
            SubjectMetaCacheKey cacheKey)
        {
            return GetSubjectMetaViewModel(releaseSubject);
        }

        public async Task<Either<ActionResult, SubjectMetaViewModel>> GetSubjectMetaRestricted(
            Guid releaseId,
            Guid subjectId)
        {
            return await CheckReleaseSubjectExists(releaseId, subjectId)
                .OnSuccess(CheckCanViewSubjectData)
                .OnSuccess(GetSubjectMetaViewModel);
        }

        public Task<Either<ActionResult, SubjectMetaViewModel>> GetSubjectMeta(
            ObservationQueryContext query,
            CancellationToken cancellationToken)
        {
            return CheckSubjectExistsOnLatestPublishedVersion(query.SubjectId)
                .OnSuccess(releaseSubject =>
                    GetSubjectMetaViewModelFromQuery(query, releaseSubject, cancellationToken));
        }

        public Task<Either<ActionResult, SubjectMetaViewModel>> GetSubjectMetaRestricted(
            Guid releaseId,
            ObservationQueryContext query,
            CancellationToken cancellationToken)
        {
            return CheckReleaseSubjectExists(releaseId, query.SubjectId)
                .OnSuccess(CheckCanViewSubjectData)
                .OnSuccess(releaseSubject =>
                    GetSubjectMetaViewModelFromQuery(query, releaseSubject, cancellationToken));
        }

        public async Task<Either<ActionResult, Unit>> UpdateSubjectFilters(
            Guid releaseId,
            Guid subjectId,
            List<FilterUpdateViewModel> request)
        {
            return await CheckReleaseSubjectExists(releaseId, subjectId)
                .OnSuccessVoid(async rs =>
                {
                    // TODO EES-3345 Validate all entries are present and only entries that relate to the subject

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
                });
        }

        public async Task<Either<ActionResult, Unit>> UpdateSubjectIndicators(
            Guid releaseId,
            Guid subjectId,
            List<IndicatorGroupUpdateViewModel> request)
        {
            return await CheckReleaseSubjectExists(releaseId, subjectId)
                .OnSuccessVoid(async releaseSubject =>
                {
                    // TODO EES-3345 Validate all entries are present and only entries that relate to the subject

                    // Set the sequence based on the order of indicator groups and indicators observed
                    // in the request
                    releaseSubject.IndicatorSequence = request.Select(indicatorGroup =>
                            new IndicatorGroupSequenceEntry(
                                indicatorGroup.Id,
                                indicatorGroup.Indicators
                            ))
                        .ToList();
                    await _statisticsDbContext.SaveChangesAsync();
                });
        }

        private async Task<SubjectMetaViewModel> GetSubjectMetaViewModel(ReleaseSubject releaseSubject)
        {
            return new SubjectMetaViewModel
            {
                Filters = GetFilters(releaseSubject),
                Indicators = GetIndicators(releaseSubject),
                Locations = await GetLocations(releaseSubject.SubjectId),
                TimePeriod = GetTimePeriods(releaseSubject.SubjectId)
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
            else if (query.TimePeriod != null && query.Filters == null)
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

                    var timePeriods = GetTimePeriods(observations);

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
                        FiltersViewModelBuilder.BuildFiltersFromFilterItems(filterItems,
                            releaseSubject.FilterSequence);
                    _logger.LogTrace("Got Filters in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var indicators = GetIndicators(releaseSubject);
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

        private Dictionary<string, FilterMetaViewModel> GetFilters(ReleaseSubject releaseSubject)
        {
            var filters = _filterRepository.GetFiltersIncludingItems(releaseSubject.SubjectId);
            return FiltersViewModelBuilder.BuildFilters(filters, releaseSubject.FilterSequence);
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
            var locations = await _locationRepository.GetDistinctForSubject(subjectId);
            var locationsHierarchical =
                locations.GetLocationAttributesHierarchical(_locationOptions.Hierarchies);
            return BuildLocationAttributeViewModels(locationsHierarchical);
        }

        private Dictionary<string, IndicatorGroupMetaViewModel> GetIndicators(ReleaseSubject releaseSubject)
        {
            var indicators = _indicatorGroupRepository.GetIndicatorGroups(releaseSubject.SubjectId);
            return IndicatorsViewModelBuilder.BuildIndicatorGroups(indicators,
                releaseSubject.IndicatorSequence);
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
                    Id = locationAttributeNode.LocationId.Value,
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

        private async Task<Either<ActionResult, ReleaseSubject>> CheckSubjectExistsOnLatestPublishedVersion(
            Guid subjectId)
        {
            return await _releaseSubjectRepository.GetReleaseSubjectForLatestPublishedVersion(subjectId) ??
                   new Either<ActionResult, ReleaseSubject>(new NotFoundResult());
        }

        private async Task<Either<ActionResult, SubjectMetaCacheKey>> CreateCacheKeyForSubjectMeta(
            ReleaseSubject releaseSubject)
        {
            var release = await _contentDbContext
                .Releases
                .Include(release => release.Publication)
                .SingleAsync(release => release.Id == releaseSubject.ReleaseId);

            return new SubjectMetaCacheKey(release.Publication.Slug, release.Slug, releaseSubject.SubjectId);
        }
    }
}
