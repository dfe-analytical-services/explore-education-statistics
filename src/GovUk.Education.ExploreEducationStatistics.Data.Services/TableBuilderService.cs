#nullable enable
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.Utils.TableBuilderUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.ValidationErrorMessages;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class TableBuilderService : ITableBuilderService
    {
        private readonly StatisticsDbContext _context;
        private readonly IFilterItemRepository _filterItemRepository;
        private readonly IObservationService _observationService;
        private readonly IPersistenceHelper<StatisticsDbContext> _statisticsPersistenceHelper;
        private readonly ISubjectResultMetaService _subjectResultMetaService;
        private readonly ISubjectCsvMetaService _subjectCsvMetaService;
        private readonly ISubjectRepository _subjectRepository;
        private readonly IUserService _userService;
        private readonly IReleaseRepository _releaseRepository;
        private readonly TableBuilderOptions _options;

        public TableBuilderService(
            StatisticsDbContext context,
            IFilterItemRepository filterItemRepository,
            IObservationService observationService,
            IPersistenceHelper<StatisticsDbContext> statisticsPersistenceHelper,
            ISubjectResultMetaService subjectResultMetaService,
            ISubjectCsvMetaService subjectCsvMetaService,
            ISubjectRepository subjectRepository,
            IUserService userService,
            IReleaseRepository releaseRepository,
            IOptions<TableBuilderOptions> options)
        {
            _context = context;
            _filterItemRepository = filterItemRepository;
            _observationService = observationService;
            _statisticsPersistenceHelper = statisticsPersistenceHelper;
            _subjectResultMetaService = subjectResultMetaService;
            _subjectCsvMetaService = subjectCsvMetaService;
            _subjectRepository = subjectRepository;
            _userService = userService;
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
                .OnSuccessDo(() => CheckQueryHasValidTableSize(queryContext))
                .OnSuccess(() => ListQueryObservations(queryContext, cancellationToken))
                .OnSuccess(async observations =>
                {
                    if (!observations.Any())
                    {
                        return new TableBuilderResultViewModel();
                    }

                    return await _subjectResultMetaService
                        .GetSubjectMeta(releaseId, queryContext, observations)
                        .OnSuccess(subjectMetaViewModel =>
                        {
                            return new TableBuilderResultViewModel
                            {
                                SubjectMeta = subjectMetaViewModel,
                                Results = observations.Select(observation =>
                                    ObservationViewModelBuilder.BuildObservation(observation, queryContext.Indicators))
                            };
                        });
                });
        }

        public async Task<Either<ActionResult, Unit>> QueryToCsvStream(
            ObservationQueryContext queryContext,
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            return await FindLatestPublishedReleaseId(queryContext.SubjectId)
                .OnSuccessVoid(releaseId => QueryToCsvStream(releaseId, queryContext, stream, cancellationToken));
        }

        public async Task<Either<ActionResult, Unit>> QueryToCsvStream(
            Guid releaseId,
            ObservationQueryContext queryContext,
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            return await CheckReleaseSubjectExists(queryContext.SubjectId, releaseId)
                .OnSuccess(_userService.CheckCanViewSubjectData)
                .OnSuccessDo(() => CheckQueryHasValidTableSize(queryContext))
                .OnSuccess(releaseSubject => ListQueryObservations(queryContext, cancellationToken)
                    .OnSuccessCombineWith(observations =>
                        _subjectCsvMetaService.GetSubjectCsvMeta(releaseSubject, queryContext, observations, cancellationToken))
                )
                .OnSuccessVoid(
                    async tuple =>
                    {
                        await using var writer = new StreamWriter(stream, leaveOpen: true);
                        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture, leaveOpen: true);

                        var (observations, meta) = tuple;

                        await WriteCsvHeaderRow(csv, meta);
                        await WriteCsvRows(csv, observations, meta, cancellationToken);
                    }
                );
        }

        private async Task<Either<ActionResult, List<Observation>>> ListQueryObservations(
            ObservationQueryContext queryContext,
            CancellationToken cancellationToken)
        {
            var matchedObservationIds =
                (await _observationService.GetMatchedObservations(queryContext, cancellationToken))
                .Select(row => row.Id);

            return await _context
                .Observation
                .AsNoTracking()
                .Include(o => o.Location)
                .Include(o => o.FilterItems)
                .Where(o => matchedObservationIds.Contains(o.Id))
                .ToListAsync(cancellationToken);
        }

        private async Task<Either<ActionResult, Unit>> CheckQueryHasValidTableSize(ObservationQueryContext queryContext)
        {
            if (await GetMaximumTableCellCount(queryContext) > _options.MaxTableCellsAllowed)
            {
                return ValidationUtils.ValidationResult(QueryExceedsMaxAllowableTableSize);
            }

            return Unit.Instance;
        }

        private async Task<int> GetMaximumTableCellCount(ObservationQueryContext queryContext)
        {
            var filterItemIds = queryContext.Filters.ToList();
            var countsOfFilterItemsByFilter = filterItemIds.Count == 0
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

            return MaximumTableCellCount(
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

        private async Task WriteCsvHeaderRow(
            CsvWriter csv,
            SubjectCsvMetaViewModel meta)
        {
            var headerRow = new ExpandoObject() as IDictionary<string, object>;

            foreach (var header in meta.Headers)
            {
                headerRow[header] = string.Empty;
            }

            csv.WriteDynamicHeader(headerRow as dynamic);

            await csv.NextRecordAsync();
        }

        private async Task WriteCsvRows(
            IWriter csv,
            List<Observation> observations,
            SubjectCsvMetaViewModel meta,
            CancellationToken cancellationToken)
        {
            var locationHeaders = meta.Headers
                .Where(header => LocationCsvUtils.AllCsvColumns().Contains(header))
                .ToHashSet();

            var rows = observations
                .Select(
                    observation => MapCsvRow(
                        observation: observation,
                        meta: meta,
                        locationHeaders: locationHeaders
                    )
                );

            await csv.WriteRecordsAsync(rows, cancellationToken);
        }

        private object MapCsvRow(
            Observation observation,
            SubjectCsvMetaViewModel meta,
            HashSet<string> locationHeaders)
        {
            var row = new ExpandoObject() as IDictionary<string, object>;

            foreach (var header in meta.Headers)
            {
                row[header] = GetCsvRowValue(header, observation, meta, locationHeaders);
            }

            return row;
        }

        private string GetCsvRowValue(
            string header,
            Observation observation,
            SubjectCsvMetaViewModel meta,
            IReadOnlySet<string> locationHeaders)
        {
            if (header == "time_period")
            {
                return TimePeriodLabelFormatter.FormatCsvYear(observation.Year, observation.TimeIdentifier);
            }

            if (header == "time_identifier")
            {
                return observation.TimeIdentifier.GetEnumLabel();
            }

            if (header == "geographic_level")
            {
                return observation.Location.GeographicLevel.GetEnumLabel();
            }

            if (locationHeaders.Contains(header))
            {
                var location = meta.Locations[observation.Location.Id];

                if (location.ContainsKey(header))
                {
                    return location[header];
                }
            }

            if (meta.Filters.TryGetValue(header, out var filter))
            {
                var match = observation.FilterItems
                    .FirstOrDefault(fi => filter.Items.ContainsKey(fi.FilterItemId));

                if (match is not null)
                {
                    return filter.Items[match.FilterItemId].Label;
                }
            }

            if (meta.Indicators.TryGetValue(header, out var indicator))
            {
                var match = observation.Measures
                    .First(measure => measure.Key == indicator.Id);

                return match.Value;
            }

            return string.Empty;
        }
    }

    public class TableBuilderOptions
    {
        public const string TableBuilder = "TableBuilder";

        public int MaxTableCellsAllowed { get; set; }
    }
}
