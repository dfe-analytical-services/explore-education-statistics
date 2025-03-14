#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Options;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class SubjectResultMetaService : ISubjectResultMetaService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<StatisticsDbContext> _persistenceHelper;
        private readonly IBoundaryLevelRepository _boundaryLevelRepository;
        private readonly IFilterItemRepository _filterItemRepository;
        private readonly IFootnoteRepository _footnoteRepository;
        private readonly IIndicatorRepository _indicatorRepository;
        private readonly ILocationService _locationService;
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
            IIndicatorRepository indicatorRepository,
            ILocationService locationService,
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
            _indicatorRepository = indicatorRepository;
            _locationService = locationService;
            _timePeriodService = timePeriodService;
            _userService = userService;
            _subjectRepository = subjectRepository;
            _releaseDataFileRepository = releaseDataFileRepository;
            _locationOptions = locationOptions.Value;
            _logger = logger;
        }

        public async Task<Either<ActionResult, SubjectResultMetaViewModel>> GetSubjectMeta(
            Guid releaseVersionId,
            FullTableQuery query,
            IList<Observation> observations)
        {
            return await CheckReleaseSubjectExists(releaseVersionId: releaseVersionId,
                    subjectId: query.SubjectId)
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

                    var releaseFile =
                        await _releaseDataFileRepository.GetBySubject(releaseVersionId: releaseVersionId,
                            subjectId: releaseSubject.SubjectId);

                    var filterItems =
                        await _filterItemRepository.GetFilterItemsFromObservations(observations);
                    var filterViewModels = FiltersMetaViewModelBuilder
                        .BuildFiltersFromFilterItems(filterItems, releaseFile.FilterSequence);
                    _logger.LogTrace("Got Filters in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var indicatorViewModels = GetIndicatorViewModels(
                        query, releaseFile.IndicatorSequence);
                    _logger.LogTrace("Got Indicators in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
                    stopwatch.Restart();

                    var footnoteViewModels = await GetFootnoteViewModels(
                        releaseVersionId: releaseVersionId,
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

                    var locationViewModels =
                        await _locationService.GetLocationViewModels(locations, _locationOptions.Hierarchies);
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
                        SubjectName = releaseFile.Name!,
                        TimePeriodRange = timePeriodViewModels
                    };
                });
        }

        private Task<Either<ActionResult, ReleaseSubject>> CheckReleaseSubjectExists(Guid releaseVersionId,
            Guid subjectId)
        {
            return _persistenceHelper.CheckEntityExists<ReleaseSubject>(
                query => query
                    .Include(rs => rs.Subject)
                    .Where(rs => rs.ReleaseVersionId == releaseVersionId
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

        private List<IndicatorMetaViewModel> GetIndicatorViewModels(FullTableQuery query,
            List<IndicatorGroupSequenceEntry>? indicatorSequence)
        {
            var indicators = _indicatorRepository.GetIndicators(query.SubjectId, query.Indicators);

            // Flatten the indicator sequence so that it can be used to sequence all the indicators since they have
            // been fetched without groups
            var indicatorsOrdering = indicatorSequence?
                .SelectMany(groupOrdering => groupOrdering.ChildSequence)
                .ToList();

            return IndicatorsMetaViewModelBuilder.BuildIndicators(indicators, indicatorsOrdering);
        }

        private async Task<List<FootnoteViewModel>> GetFootnoteViewModels(
            Guid releaseVersionId,
            Guid subjectId,
            IEnumerable<Guid> filterItemIds,
            IEnumerable<Guid> indicatorIds)
        {
            var footnotes = await _footnoteRepository
                .GetFilteredFootnotes(
                    releaseVersionId: releaseVersionId,
                    subjectId: subjectId,
                    filterItemIds: filterItemIds,
                    indicatorIds: indicatorIds);

            return FootnotesViewModelBuilder.BuildFootnotes(footnotes);
        }

        private List<TimePeriodMetaViewModel> GetTimePeriodViewModels(IList<Observation> observations)
        {
            return _timePeriodService
                .GetTimePeriodRange(observations)
                .Select(tuple => new TimePeriodMetaViewModel(tuple.Year, tuple.TimeIdentifier))
                .ToList();
        }
    }
}
