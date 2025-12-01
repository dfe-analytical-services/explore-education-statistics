#nullable enable
using System.Dynamic;
using System.Globalization;
using CsvHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Options;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services;

public class TableBuilderService : ITableBuilderService
{
    private readonly StatisticsDbContext _statisticsDbContext;
    private readonly ContentDbContext _contentDbContext;
    private readonly ILocationService _locationService;
    private readonly IObservationService _observationService;
    private readonly IPersistenceHelper<StatisticsDbContext> _statisticsPersistenceHelper;
    private readonly ISubjectResultMetaService _subjectResultMetaService;
    private readonly ISubjectCsvMetaService _subjectCsvMetaService;
    private readonly ISubjectRepository _subjectRepository;
    private readonly ITableBuilderQueryOptimiser _tableBuilderQueryOptimiser;
    private readonly IUserService _userService;
    private readonly TableBuilderOptions _options;
    private readonly LocationsOptions _locationOptions;

    public TableBuilderService(
        StatisticsDbContext statisticsDbContext,
        ContentDbContext contentDbContext,
        ILocationService locationService,
        IObservationService observationService,
        IPersistenceHelper<StatisticsDbContext> statisticsPersistenceHelper,
        ISubjectResultMetaService subjectResultMetaService,
        ISubjectCsvMetaService subjectCsvMetaService,
        ISubjectRepository subjectRepository,
        ITableBuilderQueryOptimiser tableBuilderQueryOptimiser,
        IUserService userService,
        IOptions<TableBuilderOptions> options,
        IOptions<LocationsOptions> locationOptions
    )
    {
        _statisticsDbContext = statisticsDbContext;
        _contentDbContext = contentDbContext;
        _locationService = locationService;
        _observationService = observationService;
        _statisticsPersistenceHelper = statisticsPersistenceHelper;
        _subjectResultMetaService = subjectResultMetaService;
        _subjectCsvMetaService = subjectCsvMetaService;
        _subjectRepository = subjectRepository;
        _tableBuilderQueryOptimiser = tableBuilderQueryOptimiser;
        _userService = userService;
        _options = options.Value;
        _locationOptions = locationOptions.Value;
    }

    public async Task<Either<ActionResult, TableBuilderResultViewModel>> Query(
        FullTableQuery query,
        CancellationToken cancellationToken = default
    )
    {
        return await FindLatestPublishedReleaseVersionId(query.SubjectId)
            .OnSuccess(releaseVersionId => Query(releaseVersionId, query, cancellationToken));
    }

    public async Task<Either<ActionResult, TableBuilderResultViewModel>> Query(
        Guid releaseVersionId,
        FullTableQuery query,
        CancellationToken cancellationToken = default
    )
    {
        return await CheckReleaseSubjectExists(subjectId: query.SubjectId, releaseVersionId: releaseVersionId)
            .OnSuccess(_userService.CheckCanViewSubjectData)
            .OnSuccess(() => ListQueryObservations(query, cancellationToken))
            .OnSuccess(async queryObservations =>
            {
                var (observations, isCroppedTable) = queryObservations;

                if (!observations.Any())
                {
                    return new TableBuilderResultViewModel();
                }

                return await _subjectResultMetaService
                    .GetSubjectMeta(releaseVersionId, query, observations, isCroppedTable)
                    .OnSuccess(subjectMetaViewModel =>
                    {
                        return new TableBuilderResultViewModel
                        {
                            SubjectMeta = subjectMetaViewModel,
                            Results = observations.Select(observation =>
                                ObservationViewModelBuilder.BuildObservation(observation, query.Indicators)
                            ),
                        };
                    });
            });
    }

    public async Task<Either<ActionResult, Dictionary<string, List<LocationAttributeViewModel>>>> QueryForBoundaryLevel(
        Guid releaseVersionId,
        FullTableQuery query,
        long boundaryLevelId,
        CancellationToken cancellationToken = default
    )
    {
        return await CheckReleaseSubjectExists(subjectId: query.SubjectId, releaseVersionId: releaseVersionId)
            .OnSuccess(_userService.CheckCanViewSubjectData)
            .OnSuccess(() => ListQueryObservations(query, cancellationToken))
            .OnSuccess(async queryObservations =>
            {
                var (observations, isCroppedTable) = queryObservations;

                if (observations.Count == 0)
                {
                    return [];
                }

                var locations = observations.Select(o => o.Location).Distinct().ToList();

                return await _locationService.GetLocationViewModels(
                    locations,
                    _locationOptions.Hierarchies,
                    boundaryLevelId
                );
            });
    }

    public async Task<Either<ActionResult, Unit>> QueryToCsvStream(
        FullTableQuery query,
        Stream stream,
        CancellationToken cancellationToken = default
    )
    {
        return await FindLatestPublishedReleaseVersionId(query.SubjectId)
            .OnSuccessVoid(releaseVersionId => QueryToCsvStream(releaseVersionId, query, stream, cancellationToken));
    }

    public async Task<Either<ActionResult, Unit>> QueryToCsvStream(
        Guid releaseVersionId,
        FullTableQuery query,
        Stream stream,
        CancellationToken cancellationToken = default
    )
    {
        return await CheckReleaseSubjectExists(query.SubjectId, releaseVersionId)
            .OnSuccess(_userService.CheckCanViewSubjectData)
            .OnSuccessVoid(async releaseSubject =>
            {
                var (observations, _) = await ListQueryObservations(query, cancellationToken);

                await _subjectCsvMetaService
                    .GetSubjectCsvMeta(releaseSubject, query, observations, cancellationToken)
                    .OnSuccessVoid(async meta =>
                    {
                        await using var writer = new StreamWriter(stream, leaveOpen: true);
                        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture, leaveOpen: true);

                        await WriteCsvHeaderRow(csv, meta);
                        await WriteCsvRows(csv, observations, meta, cancellationToken);
                    });
            });
    }

    private async Task<(List<Observation>, bool)> ListQueryObservations(
        FullTableQuery query,
        CancellationToken cancellationToken
    )
    {
        var requiresCropping = await _tableBuilderQueryOptimiser.IsCroppingRequired(query);

        if (requiresCropping)
        {
            query = await _tableBuilderQueryOptimiser.CropQuery(query, cancellationToken);
        }

        await _observationService.GetMatchedObservations(query, cancellationToken);

        var matchedObservationIds = _statisticsDbContext.MatchedObservations.Select(o => o.Id);

        var results = await _statisticsDbContext
            .Observation.AsNoTracking()
            .Include(o => o.Location)
            .Include(o => o.FilterItems)
            .Where(o => matchedObservationIds.Contains(o.Id))
            .Take(_options.CroppedTableMaxRows)
            .ToListAsync(cancellationToken);

        return (results, requiresCropping);
    }

    private async Task<Either<ActionResult, Guid>> FindLatestPublishedReleaseVersionId(Guid subjectId)
    {
        return await _subjectRepository
            .FindPublicationIdForSubject(subjectId)
            .OrNotFound()
            .OnSuccess(async publicationId =>
                await _contentDbContext
                    .Publications.Where(p => p.Id == publicationId)
                    .Select(p => p.LatestPublishedReleaseVersionId)
                    .SingleOrDefaultAsync()
                    .OrNotFound()
            );
    }

    private Task<Either<ActionResult, ReleaseSubject>> CheckReleaseSubjectExists(Guid subjectId, Guid releaseVersionId)
    {
        return _statisticsPersistenceHelper.CheckEntityExists<ReleaseSubject>(query =>
            query.Where(rs => rs.ReleaseVersionId == releaseVersionId && rs.SubjectId == subjectId)
        );
    }

    private async Task WriteCsvHeaderRow(CsvWriter csv, SubjectCsvMetaViewModel meta)
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
        CancellationToken cancellationToken
    )
    {
        var locationHeaders = meta
            .Headers.Where(header => LocationCsvUtils.AllCsvColumns().Contains(header))
            .ToHashSet();

        var rows = observations.Select(observation =>
            MapCsvRow(observation: observation, meta: meta, locationHeaders: locationHeaders)
        );

        await csv.WriteRecordsAsync(rows, cancellationToken);
    }

    private object MapCsvRow(Observation observation, SubjectCsvMetaViewModel meta, HashSet<string> locationHeaders)
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
        IReadOnlySet<string> locationHeaders
    )
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

        if (meta.FiltersByGroupingColumn.TryGetValue(header, out var filterFoundFromGroupHeader))
        {
            var match = observation.FilterItems.FirstOrDefault(fi =>
                filterFoundFromGroupHeader.Items.ContainsKey(fi.FilterItemId)
            );

            if (match is not null)
            {
                return filterFoundFromGroupHeader.Items[match.FilterItemId].GroupLabel;
            }
        }

        if (meta.Filters.TryGetValue(header, out var filter))
        {
            var match = observation.FilterItems.FirstOrDefault(fi => filter.Items.ContainsKey(fi.FilterItemId));

            if (match is not null)
            {
                return filter.Items[match.FilterItemId].Label;
            }
        }

        if (meta.Indicators.TryGetValue(header, out var indicator))
        {
            var match = observation.Measures.First(measure => measure.Key == indicator.Id);

            return match.Value;
        }

        return string.Empty;
    }
}
