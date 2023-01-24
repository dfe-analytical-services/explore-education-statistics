#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.ValidationErrorMessages;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class TableBuilderService : ITableBuilderService
    {
        private readonly StatisticsDbContext _context;
        private readonly IFilterItemRepository _filterItemRepository;
        private readonly IObservationService _observationService;
        private readonly IPersistenceHelper<StatisticsDbContext> _statisticsPersistenceHelper;
        private readonly IResultSubjectMetaService _resultSubjectMetaService;
        private readonly ISubjectRepository _subjectRepository;
        private readonly IUserService _userService;
        private readonly IResultBuilder<Observation, ObservationViewModel> _resultBuilder;
        private readonly IReleaseRepository _releaseRepository;
        private readonly TableBuilderOptions _options;

        public TableBuilderService(
            StatisticsDbContext context,
            IFilterItemRepository filterItemRepository,
            IObservationService observationService,
            IPersistenceHelper<StatisticsDbContext> statisticsPersistenceHelper,
            IResultSubjectMetaService resultSubjectMetaService,
            ISubjectRepository subjectRepository,
            IUserService userService,
            IResultBuilder<Observation, ObservationViewModel> resultBuilder,
            IReleaseRepository releaseRepository,
            IOptions<TableBuilderOptions> options)
        {
            _context = context;
            _filterItemRepository = filterItemRepository;
            _observationService = observationService;
            _statisticsPersistenceHelper = statisticsPersistenceHelper;
            _resultSubjectMetaService = resultSubjectMetaService;
            _subjectRepository = subjectRepository;
            _userService = userService;
            _resultBuilder = resultBuilder;
            _releaseRepository = releaseRepository;
            _options = options.Value;
        }

        public async Task<Either<ActionResult, TableBuilderResultViewModel>> Query(
            ObservationQueryContext queryContext,
            CancellationToken cancellationToken = default)
        {
            return await FindLatestPublishedReleaseId(queryContext.SubjectId)
                .OnSuccess(releaseId => Query(releaseId, queryContext, cancellationToken));
        }

        public async Task<Either<ActionResult, TableBuilderResultViewModel>> Query(
            Guid releaseId,
            ObservationQueryContext queryContext,
            CancellationToken cancellationToken = default)
        {
            return await CheckReleaseSubjectExists(queryContext.SubjectId, releaseId)
                .OnSuccess(_userService.CheckCanViewSubjectData)
                .OnSuccess(async () =>
                {
                    if (await GetMaximumTableCellCount(queryContext) > _options.MaxTableCellsAllowed)
                    {
                        return ValidationUtils.ValidationResult(QueryExceedsMaxAllowableTableSize);
                    }

                    var matchedObservationIds =
                        (await _observationService.GetMatchedObservations(queryContext, cancellationToken))
                        .Select(row => row.Id);

                    var observations = await _context
                        .Observation
                        .AsNoTracking()
                        .Include(o => o.Location)
                        .Include(o => o.FilterItems)
                        .Where(o => matchedObservationIds.Contains(o.Id))
                        .ToListAsync(cancellationToken);

                    if (!observations.Any())
                    {
                        return new TableBuilderResultViewModel();
                    }

                    return await _resultSubjectMetaService
                        .GetSubjectMeta(
                            releaseId,
                            queryContext,
                            observations)
                        .OnSuccess(subjectMetaViewModel =>
                        {
                            return new TableBuilderResultViewModel
                            {
                                SubjectMeta = subjectMetaViewModel,
                                Results = observations.Select(observation =>
                                    _resultBuilder.BuildResult(observation, queryContext.Indicators))
                            };
                        });
                });
        }

        private async Task<int> GetMaximumTableCellCount(ObservationQueryContext queryContext)
        {
            var filterItemIds = queryContext.Filters;
            var countsOfFilterItemsByFilter = filterItemIds == null
                ? new List<int>()
                : (await _filterItemRepository.CountFilterItemsByFilter(filterItemIds))
                .Select(pair =>
                {
                    var (_, count) = pair;
                    return count;
                })
                .ToList();

            // TODO Accessing time periods for the Subject by altering the Importer to store them would improve accuracy
            // here rather than assuming the Subject has all time periods between the start and end range.

            return TableBuilderUtils.MaximumTableCellCount(
                countOfIndicators: queryContext.Indicators.Count(),
                countOfLocations: queryContext.LocationIds.Count,
                countOfTimePeriods: TimePeriodUtil.Range(queryContext.TimePeriod).Count,
                countsOfFilterItemsByFilter: countsOfFilterItemsByFilter
            );
        }

        private async Task<Either<ActionResult, Guid>> FindLatestPublishedReleaseId(Guid subjectId)
        {
            return await _subjectRepository.FindPublicationIdForSubject(subjectId)
                .OrNotFound()
                .OnSuccess(publicationId => _releaseRepository.GetLatestPublishedRelease(publicationId))
                .OnSuccess(release => release.Id);
        }

        private Task<Either<ActionResult, ReleaseSubject>> CheckReleaseSubjectExists(Guid subjectId, Guid releaseId)
        {
            return _statisticsPersistenceHelper.CheckEntityExists<ReleaseSubject>(
                query => query
                    .Where(rs => rs.ReleaseId == releaseId && rs.SubjectId == subjectId)
            );
        }
    }

    public class TableBuilderOptions
    {
        public const string TableBuilder = "TableBuilder";

        public int MaxTableCellsAllowed { get; set; }
    }
}
