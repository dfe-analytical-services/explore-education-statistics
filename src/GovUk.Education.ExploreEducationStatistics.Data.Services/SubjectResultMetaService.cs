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
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.LocationViewModelBuilder;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class SubjectResultMetaService : ISubjectResultMetaService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<StatisticsDbContext> _persistenceHelper;
        private readonly IBoundaryLevelRepository _boundaryLevelRepository;
        private readonly IFilterItemRepository _filterItemRepository;
        private readonly IFootnoteRepository _footnoteRepository;
        private readonly IGeoJsonRepository _geoJsonRepository;
        private readonly IIndicatorRepository _indicatorRepository;
        private readonly ITimePeriodService _timePeriodService;
        private readonly IUserService _userService;
        private readonly ISubjectRepository _subjectRepository;
        private readonly IReleaseDataFileRepository _releaseDataFileRepository;
        private readonly LocationsOptions _locationOptions;
        private readonly ILogger _logger;

        public SubjectResultMetaService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<StatisticsDbContext> persistenceHelper,
            IBoundaryLevelRepository boundaryLevelRepository,
            IFilterItemRepository filterItemRepository,
            IFootnoteRepository footnoteRepository,
            IGeoJsonRepository geoJsonRepository,
            IIndicatorRepository indicatorRepository,
            ITimePeriodService timePeriodService,
            IUserService userService,
            ISubjectRepository subjectRepository,
            IReleaseDataFileRepository releaseDataFileRepository,
            IOptions<LocationsOptions> locationOptions,
            ILogger<SubjectResultMetaService> logger)
        {
            _contentDbContext = contentDbContext;
            _persistenceHelper = persistenceHelper;
            _boundaryLevelRepository = boundaryLevelRepository;
            _filterItemRepository = filterItemRepository;
            _footnoteRepository = footnoteRepository;
            _geoJsonRepository = geoJsonRepository;
            _indicatorRepository = indicatorRepository;
            _timePeriodService = timePeriodService;
            _userService = userService;
            _subjectRepository = subjectRepository;
            _releaseDataFileRepository = releaseDataFileRepository;
            _locationOptions = locationOptions.Value;
            _logger = logger;
        }

        public async Task<Either<ActionResult, SubjectResultMetaViewModel>> GetSubjectMeta(
            Guid releaseId,
            ObservationQueryContext query,
            IList<Observation> observations)
        {
            return await CheckReleaseSubjectExists(releaseId, query.SubjectId)
                .OnSuccess(_userService.CheckCanViewSubjectData)
                .OnSuccess(async releaseSubject =>
                {
                    var stopwatch = Stopwatch.StartNew();
                    stopwatch.Start();

                    var locations = observations
                        .Select(o => o.Location)
                        .Distinct()
                        .ToList();

                    _logger.LogTrace("Got Location attributes in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var filterItems =
                        await _filterItemRepository.GetFilterItemsFromObservations(observations);
                    var filterViewModels = FiltersMetaViewModelBuilder.BuildFiltersFromFilterItems(filterItems,
                        releaseSubject.FilterSequence);
                    _logger.LogTrace("Got Filters in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var indicatorViewModels = GetIndicatorViewModels(query, releaseSubject);
                    _logger.LogTrace("Got Indicators in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var footnoteViewModels = await GetFootnoteViewModels(
                        releaseId: releaseId,
                        subjectId: query.SubjectId,
                        filterItemIds: filterItems.Select(fi => fi.Id).ToList(),
                        indicatorIds: indicatorViewModels.Select(i => i.Value).ToList());
                    _logger.LogTrace("Got Footnotes in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var timePeriodViewModels = GetTimePeriodViewModels(observations);
                    _logger.LogTrace("Got Time Periods in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var publicationId = await _subjectRepository.FindPublicationIdForSubject(releaseSubject.SubjectId);
                    var publicationTitle = (await _contentDbContext.Publications.FindAsync(publicationId))!.Title;

                    var releaseFile =
                        await _releaseDataFileRepository.GetBySubject(releaseId, releaseSubject.SubjectId);
                    var subjectName = releaseFile.Name!;

                    var locationViewModels =
                        await GetLocationViewModels(locations, query.BoundaryLevel, _locationOptions.Hierarchies);
                    _logger.LogTrace("Got Location view models in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Stop();

                    var geographicLevels = locations.Select(l => l.GeographicLevel).Distinct().ToList();
                    var boundaryLevelViewModels = GetBoundaryLevelViewModels(geographicLevels);

                    return new SubjectResultMetaViewModel
                    {
                        Filters = filterViewModels,
                        Footnotes = footnoteViewModels,
                        GeoJsonAvailable = boundaryLevelViewModels.Any(),
                        Indicators = indicatorViewModels,
                        Locations = locationViewModels,
                        BoundaryLevels = boundaryLevelViewModels,
                        PublicationName = publicationTitle,
                        SubjectName = subjectName,
                        TimePeriodRange = timePeriodViewModels
                    };
                });
        }

        private Task<Either<ActionResult, ReleaseSubject>> CheckReleaseSubjectExists(Guid releaseId, Guid subjectId)
        {
            return _persistenceHelper.CheckEntityExists<ReleaseSubject>(
                query => query
                    .Include(rs => rs.Subject)
                    .Where(rs => rs.ReleaseId == releaseId
                                 && rs.SubjectId == subjectId)
            );
        }

        private List<BoundaryLevelViewModel> GetBoundaryLevelViewModels(List<GeographicLevel> geographicLevels)
        {
            return _boundaryLevelRepository
                .FindByGeographicLevels(geographicLevels)
                .Select(level => new BoundaryLevelViewModel(level.Id, level.Label))
                .ToList();
        }

        private List<IndicatorMetaViewModel> GetIndicatorViewModels(ObservationQueryContext query,
            ReleaseSubject subject)
        {
            var indicators = _indicatorRepository.GetIndicators(query.SubjectId, query.Indicators);

            // Flatten the indicator sequence so that it can be used to sequence all the indicators since they have
            // been fetched without groups
            var indicatorsOrdering = subject.IndicatorSequence?
                .SelectMany(groupOrdering => groupOrdering.ChildSequence)
                .ToList();

            return IndicatorsMetaViewModelBuilder.BuildIndicators(indicators, indicatorsOrdering);
        }

        private async Task<List<FootnoteViewModel>> GetFootnoteViewModels(
            Guid releaseId,
            Guid subjectId,
            IEnumerable<Guid> filterItemIds,
            IEnumerable<Guid> indicatorIds)
        {
            var footnotes = await _footnoteRepository
                .GetFilteredFootnotes(
                    releaseId: releaseId,
                    subjectId: subjectId,
                    filterItemIds: filterItemIds,
                    indicatorIds: indicatorIds);

            return footnotes
                .Select(footnote => new FootnoteViewModel(footnote.Id, footnote.Content))
                .ToList();
        }

        private List<TimePeriodMetaViewModel> GetTimePeriodViewModels(IList<Observation> observations)
        {
            return _timePeriodService
                .GetTimePeriodRange(observations)
                .Select(tuple => new TimePeriodMetaViewModel(tuple.Year, tuple.TimeIdentifier))
                .ToList();
        }

        private async Task<Dictionary<string, List<LocationAttributeViewModel>>> GetLocationViewModels(
            List<Location> locations,
            long? boundaryLevelId,
            Dictionary<GeographicLevel, List<string>>? hierarchies)
        {
            var geoJson = await GetGeoJson(locations, boundaryLevelId);

            return BuildLocationAttributeViewModels(locations, hierarchies, geoJson)
                .ToDictionary(
                    pair => pair.Key.ToString().CamelCase(),
                    pair => pair.Value);
        }

        private async Task<Dictionary<GeographicLevel, Dictionary<string, GeoJson>>> GetGeoJson(
            List<Location> locations,
            long? boundaryLevelId)
        {
            if (!boundaryLevelId.HasValue)
            {
                return new Dictionary<GeographicLevel, Dictionary<string, GeoJson>>();
            }

            // TODO EES-3328 This could soon be irrelevant if boundary level is about to be removed from the query
            // but if not we should consider returning an error if this isn't found
            var boundaryLevel = await _boundaryLevelRepository.Get(boundaryLevelId.Value);
            if (boundaryLevel == null)
            {
                return new Dictionary<GeographicLevel, Dictionary<string, GeoJson>>();
            }

            var locationsMatchingLevel =
                locations.Where(location => location.GeographicLevel == boundaryLevel.Level);

            var codes = locationsMatchingLevel
                .Select(location => location.ToLocationAttribute().GetCodeOrFallback())
                .ToList();
            var geoJson = _geoJsonRepository.FindByBoundaryLevelAndCodes(boundaryLevelId.Value, codes);

            return new Dictionary<GeographicLevel, Dictionary<string, GeoJson>>
            {
                {
                    boundaryLevel.Level,
                    geoJson
                }
            };
        }
    }
}
