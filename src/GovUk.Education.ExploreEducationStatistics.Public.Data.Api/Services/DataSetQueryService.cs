using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Query;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Profiling;
using PagingViewModel = GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels.PagingViewModel;
using ValidationMessages = GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators.ValidationMessages;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

internal class DataSetQueryService(
    PublicDataDbContext publicDataDbContext,
    IDuckDbConnection duckDbConnection,
    IUserService userService,
    IDataSetQueryParser dataSetQueryParser,
    IParquetDataRepository dataRepository,
    IParquetFilterRepository filterRepository,
    IParquetIndicatorRepository indicatorRepository,
    IParquetLocationRepository locationRepository,
    IParquetTimePeriodRepository timePeriodRepository,
    IAnalyticsService analyticsService,
    TimeProvider timeProvider,
    ILogger<DataSetQueryService> logger
) : IDataSetQueryService
{
    private static readonly Dictionary<string, string> ReservedSorts = new()
    {
        { "timePeriod", DataTable.Cols.TimePeriodId },
        { "geographicLevel", DataTable.Cols.GeographicLevel },
    };

    private const string SortTypeFilter = "filter";
    private const string SortTypeIndicator = "indicator";
    private const string SortTypeLocation = "location";

    public async Task<Either<ActionResult, DataSetQueryPaginatedResultsViewModel>> Query(
        Guid dataSetId,
        DataSetGetQueryRequest request,
        string? dataSetVersion = null,
        CancellationToken cancellationToken = default
    )
    {
        using var _ = MiniProfiler.Current.Step($"{nameof(DataSetQueryService)}.{nameof(Query)}");

        var query = new DataSetQueryRequest
        {
            Criteria = new DataSetQueryCriteriaFacets
            {
                Locations = request.Locations?.ToCriteria(),
                Filters = request.Filters?.ToCriteria(),
                GeographicLevels = request.GeographicLevels?.ToCriteria(),
                TimePeriods = request.TimePeriods?.ToCriteria(),
            },
            Indicators = request.Indicators,
            Sorts = request.Sorts?.Select(DataSetQuerySort.Parse).ToList(),
            Debug = request.Debug,
            Page = request.Page,
            PageSize = request.PageSize,
        };

        return await FindDataSetVersion(dataSetId, dataSetVersion, cancellationToken)
            .OnSuccessDo(userService.CheckCanQueryDataSetVersion)
            .OnSuccess(dsv =>
                RunQueryWithAnalytics(
                    dataSetVersion: dsv,
                    requestedDataSetVersion: dataSetVersion,
                    query: query,
                    cancellationToken: cancellationToken
                )
            )
            .OnSuccess(results => results with { Warnings = results.Warnings.Select(MapGetQueryWarning).ToList() })
            .OnFailureFailWith(result =>
                result is not BadRequestObjectResult { Value: ValidationProblemViewModel validationProblem }
                    ? result
                    : ValidationUtils.ValidationResult(validationProblem.Errors.Select(MapGetQueryError))
            );
    }

    public async Task<Either<ActionResult, DataSetQueryPaginatedResultsViewModel>> Query(
        Guid dataSetId,
        DataSetQueryRequest request,
        string? dataSetVersion,
        CancellationToken cancellationToken = default
    )
    {
        return await FindDataSetVersion(dataSetId, dataSetVersion, cancellationToken)
            .OnSuccessDo(userService.CheckCanQueryDataSetVersion)
            .OnSuccess(dsv =>
                RunQueryWithAnalytics(
                    dataSetVersion: dsv,
                    requestedDataSetVersion: dataSetVersion,
                    query: request,
                    cancellationToken: cancellationToken,
                    baseCriteriaPath: "criteria"
                )
            );
    }

    private async Task<Either<ActionResult, DataSetVersion>> FindDataSetVersion(
        Guid dataSetId,
        string? dataSetVersion,
        CancellationToken cancellationToken
    )
    {
        using var _ = MiniProfiler.Current.Step($"{nameof(DataSetQueryService)}.{nameof(FindDataSetVersion)}");

        return dataSetVersion is null or "*"
            ? await publicDataDbContext
                .DataSets.AsNoTracking()
                .Include(ds => ds.LatestLiveVersion)
                    .ThenInclude(dsv => dsv != null ? dsv.DataSet : null)
                .Where(ds => ds.Id == dataSetId)
                .Select(ds => ds.LatestLiveVersion!)
                .SingleOrNotFoundAsync(cancellationToken)
            : await publicDataDbContext
                .DataSetVersions.AsNoTracking()
                .Include(dsv => dsv.DataSet)
                .FindByVersion(dataSetId: dataSetId, version: dataSetVersion, cancellationToken);
    }

    private async Task<Either<ActionResult, DataSetQueryPaginatedResultsViewModel>> RunQueryWithAnalytics(
        DataSetVersion dataSetVersion,
        string? requestedDataSetVersion,
        DataSetQueryRequest query,
        CancellationToken cancellationToken,
        string baseCriteriaPath = ""
    )
    {
        var startTime = timeProvider.GetUtcNow().UtcDateTime;

        return await RunQuery(
                dataSetVersion: dataSetVersion,
                query: query,
                cancellationToken: cancellationToken,
                baseCriteriaPath: baseCriteriaPath
            )
            .OnSuccessDo(async results =>
            {
                try
                {
                    await analyticsService.CaptureDataSetVersionQuery(
                        dataSetVersion: dataSetVersion,
                        requestedDataSetVersion: requestedDataSetVersion,
                        query: query,
                        results: results,
                        startTime: startTime,
                        cancellationToken: cancellationToken
                    );
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error capturing query in analytics");
                }
            });
    }

    private async Task<Either<ActionResult, DataSetQueryPaginatedResultsViewModel>> RunQuery(
        DataSetVersion dataSetVersion,
        DataSetQueryRequest query,
        CancellationToken cancellationToken,
        string baseCriteriaPath = ""
    )
    {
        duckDbConnection.Open();

        using var _ = MiniProfiler.Current.Step($"{nameof(DataSetQueryService)}.{nameof(RunQuery)}");

        var queryState = new QueryState();

        var whereBuilder = new DuckDbSqlBuilder();

        if (query.Criteria is not null)
        {
            whereBuilder += await dataSetQueryParser.ParseCriteria(
                criteria: query.Criteria,
                dataSetVersion: dataSetVersion,
                queryState: queryState,
                basePath: baseCriteriaPath,
                cancellationToken: cancellationToken
            );
        }

        var columnsTask = dataRepository.ListColumns(dataSetVersion, cancellationToken);
        var filterColumnsByIdTask = filterRepository.GetFilterColumnsById(dataSetVersion, cancellationToken);
        var indicatorColumnsByIdTask = indicatorRepository.GetColumnsById(dataSetVersion, cancellationToken);

        await Task.WhenAll(columnsTask, filterColumnsByIdTask, indicatorColumnsByIdTask);

        var indicators = GetIndicators(query, indicatorColumnsByIdTask.Result, queryState);

        var columnMappings = GetColumnMappings(
            columns: columnsTask.Result,
            filterColumnsById: filterColumnsByIdTask.Result,
            indicatorColumnsById: indicatorColumnsByIdTask.Result,
            selectedIndicators: indicators
        );

        var sorts = GetSorts(request: query, columnMappings: columnMappings, queryState: queryState);

        if (queryState.Errors.Count != 0)
        {
            return ValidationUtils.ValidationResult(queryState.Errors);
        }

        var whereSql = whereBuilder.Build();

        var countTask = dataRepository.CountRows(
            dataSetVersion: dataSetVersion,
            where: whereSql,
            cancellationToken: cancellationToken
        );

        var rowsTask = dataRepository.ListRows(
            dataSetVersion: dataSetVersion,
            columns: [DataTable.Cols.TimePeriodId, DataTable.Cols.GeographicLevel, .. columnMappings.Columns],
            where: whereSql,
            sorts: sorts,
            page: query.Page,
            pageSize: query.PageSize,
            cancellationToken: cancellationToken
        );

        await Task.WhenAll(countTask, rowsTask);

        if (query.Debug)
        {
            queryState.Warnings.Add(
                new WarningViewModel
                {
                    Code = ValidationMessages.DebugEnabled.Code,
                    Message = ValidationMessages.DebugEnabled.Message,
                }
            );
        }

        if (countTask.Result == 0)
        {
            queryState.Warnings.Add(
                new WarningViewModel
                {
                    Code = ValidationMessages.QueryNoResults.Code,
                    Message = ValidationMessages.QueryNoResults.Message,
                }
            );
        }

        var results = await MapQueryResults(
            rows: rowsTask.Result,
            dataSetVersion: dataSetVersion,
            columnMappings: columnMappings,
            debug: query.Debug,
            cancellationToken: cancellationToken
        );

        duckDbConnection.Close();

        return new DataSetQueryPaginatedResultsViewModel
        {
            Paging = new PagingViewModel(
                page: query.Page,
                pageSize: query.PageSize,
                totalResults: (int)countTask.Result
            ),
            Results = results,
            Warnings = queryState.Warnings,
        };
    }

    private static HashSet<string> GetIndicators(
        DataSetQueryRequest request,
        Dictionary<string, string> indicatorColumnsById,
        QueryState queryState
    )
    {
        if (request.Indicators == null)
        {
            return [.. indicatorColumnsById.Values];
        }

        var validIndicatorColumns = new HashSet<string>();
        var invalidIndicatorIds = new HashSet<string>();

        foreach (var indicatorId in request.Indicators)
        {
            if (indicatorColumnsById.TryGetValue(indicatorId, out var value))
            {
                validIndicatorColumns.Add(value);
            }
            else
            {
                invalidIndicatorIds.Add(indicatorId);
            }
        }

        if (invalidIndicatorIds.Count != 0)
        {
            queryState.Errors.Add(
                new ErrorViewModel
                {
                    Code = ValidationMessages.IndicatorsNotFound.Code,
                    Message = ValidationMessages.IndicatorsNotFound.Message,
                    Path = "indicators",
                    Detail = new NotFoundItemsErrorDetail<string>(invalidIndicatorIds),
                }
            );
        }

        return validIndicatorColumns;
    }

    private static List<Sort> GetSorts(
        DataSetQueryRequest request,
        ColumnMappings columnMappings,
        QueryState queryState
    )
    {
        var sorts = new List<Sort>();

        if (request.Sorts is not null)
        {
            var invalidSorts = new HashSet<DataSetQuerySort>();

            foreach (var sort in request.Sorts)
            {
                if (TryGetSortColumn(sort, columnMappings, out var column))
                {
                    sorts.Add(new Sort(Field: column, Direction: sort.ParsedDirection()));
                    continue;
                }

                invalidSorts.Add(sort);
            }

            if (invalidSorts.Count != 0)
            {
                queryState.Errors.Add(
                    new ErrorViewModel
                    {
                        Message = ValidationMessages.SortFieldsNotFound.Message,
                        Code = ValidationMessages.SortFieldsNotFound.Code,
                        Path = "sorts",
                        Detail = new NotFoundItemsErrorDetail<DataSetQuerySort>(invalidSorts),
                    }
                );
            }
        }

        if (sorts.Count == 0)
        {
            // Default to sorting by time period in descending order if no sorts.
            sorts.Add(new Sort(Field: DataTable.Ref().TimePeriodId, Direction: SortDirection.Desc));
        }

        // Need to append ID sort to ensure there is always a tie-break to ensure results are
        // returned in a deterministic order (otherwise ordering can vary when queries are re-ran).
        sorts.Add(new Sort(Field: DataTable.Ref().Id, Direction: SortDirection.Asc));

        return sorts;
    }

    private static bool TryGetSortColumn(
        DataSetQuerySort sort,
        ColumnMappings columnMappings,
        [NotNullWhen(true)] out string? column
    )
    {
        column = null;

        if (ReservedSorts.TryGetValue(sort.Field, out column))
        {
            return true;
        }

        var fieldParts = sort.Field.Split('|', 2);

        if (fieldParts.Length != 2)
        {
            return false;
        }

        var fieldType = fieldParts[0];
        var fieldId = fieldParts[1];

        return fieldType switch
        {
            SortTypeFilter => columnMappings.Filters.TryGetValue(fieldId, out column),
            SortTypeIndicator => columnMappings.Indicators.TryGetValue(fieldId, out column),
            SortTypeLocation => columnMappings.LocationLevels.TryGetValue(fieldId, out column),
            _ => false,
        };
    }

    private static ColumnMappings GetColumnMappings(
        ISet<string> columns,
        Dictionary<string, string> filterColumnsById,
        Dictionary<string, string> indicatorColumnsById,
        ISet<string> selectedIndicators
    )
    {
        var locationLevels = GeographicLevelUtils
            .Levels.Where(level => columns.Contains(DataTable.Cols.LocationId(level)))
            .ToDictionary(level => level.GetEnumValue(), DataTable.Cols.LocationId);

        var indicators = indicatorColumnsById.Where(kv => selectedIndicators.Contains(kv.Value)).ToDictionary();

        return new ColumnMappings
        {
            Filters = filterColumnsById,
            LocationLevels = locationLevels,
            Indicators = indicators,
        };
    }

    private async Task<List<DataSetQueryResultViewModel>> MapQueryResults(
        IList<IDictionary<string, object?>> rows,
        DataSetVersion dataSetVersion,
        ColumnMappings columnMappings,
        bool debug,
        CancellationToken cancellationToken
    )
    {
        using var _ = MiniProfiler.Current.Step($"{nameof(DataSetQueryService)}.{nameof(MapQueryResults)}");

        var timePeriodIds = new HashSet<int>();
        var locationOptionIds = new HashSet<int>();
        var filterOptionIds = new HashSet<int>();

        foreach (var row in rows)
        {
            timePeriodIds.Add((int)row[DataTable.Cols.TimePeriodId]!);

            foreach (var (_, locationColumn) in columnMappings.LocationLevels)
            {
                if (row[locationColumn] is int locationOptionId)
                {
                    locationOptionIds.Add(locationOptionId);
                }
            }

            foreach (var filterColumn in columnMappings.Filters.Values)
            {
                if (row[filterColumn] is int filterOptionId)
                {
                    filterOptionIds.Add(filterOptionId);
                }
            }
        }

        // Fetch options in parallel. We split out separate queries and merge back into the results
        // instead of joining everything in one big data query as joins on larger data sets are
        // quite expensive. This also avoids excessive data transfer over the wire and
        // should be more efficient overall (despite the additional requests).
        var filterOptionsById = GetFilterOptionsById(dataSetVersion, filterOptionIds, debug, cancellationToken);
        var locationOptionsById = GetLocationOptionsById(dataSetVersion, locationOptionIds, debug, cancellationToken);
        var timePeriodsById = GetTimePeriodsById(dataSetVersion, timePeriodIds, cancellationToken);

        await Task.WhenAll(filterOptionsById, locationOptionsById, timePeriodsById);

        return rows.Select(row => new DataSetQueryResultViewModel
            {
                GeographicLevel = EnumUtil.GetFromEnumLabel<GeographicLevel>(
                    (string)row[DataTable.Cols.GeographicLevel]!
                ),
                TimePeriod = timePeriodsById.Result[(int)row[DataTable.Cols.TimePeriodId]!],
                Filters = columnMappings
                    .Filters.Where(kv => row[kv.Value] is int and not 0)
                    .ToDictionary(
                        kv => debug ? $"{kv.Key} :: {kv.Value}" : kv.Key,
                        kv => filterOptionsById.Result[(int)row[kv.Value]!]
                    ),
                Locations = columnMappings
                    .LocationLevels.Where(kv => row[kv.Value] is int and not 0)
                    .ToDictionary(kv => kv.Key, kv => locationOptionsById.Result[(int)row[kv.Value]!]),
                Values = columnMappings.Indicators.ToDictionary(
                    kv => debug ? $"{kv.Key} :: {kv.Value}" : kv.Key,
                    kv => row[kv.Value] as string ?? string.Empty
                ),
            })
            .ToList();
    }

    private static WarningViewModel MapGetQueryWarning(WarningViewModel warning)
    {
        if (
            warning.Code == ValidationMessages.LocationsNotFound.Code
            && warning.Detail is NotFoundItemsErrorDetail<IDataSetQueryLocation> notFoundLocations
        )
        {
            return warning with
            {
                Detail = new NotFoundItemsErrorDetail<string>(
                    notFoundLocations.Items.Select(item => item.ToLocationString())
                ),
            };
        }

        if (
            warning.Code == ValidationMessages.TimePeriodsNotFound.Code
            && warning.Detail is NotFoundItemsErrorDetail<DataSetQueryTimePeriod> notFoundTimePeriods
        )
        {
            return warning with
            {
                Detail = new NotFoundItemsErrorDetail<string>(
                    notFoundTimePeriods.Items.Select(item => item.ToTimePeriodString())
                ),
            };
        }

        return warning;
    }

    private static ErrorViewModel MapGetQueryError(ErrorViewModel error)
    {
        if (
            error.Code == ValidationMessages.SortFieldsNotFound.Code
            && error.Detail is NotFoundItemsErrorDetail<DataSetQuerySort> notFoundSortFields
        )
        {
            return error with
            {
                Detail = new NotFoundItemsErrorDetail<string>(
                    notFoundSortFields.Items.Select(item => item.ToSortString())
                ),
            };
        }

        return error;
    }

    private async Task<Dictionary<int, string>> GetFilterOptionsById(
        DataSetVersion dataSetVersion,
        IEnumerable<int> filterOptionIds,
        bool debug,
        CancellationToken cancellationToken
    )
    {
        if (debug)
        {
            var options = await filterRepository.ListOptions(dataSetVersion, filterOptionIds, cancellationToken);

            return options.ToDictionary(option => option.Id, option => $"{option.PublicId} :: {option.Label}");
        }

        var publicIds = await filterRepository.ListOptionPublicIds(dataSetVersion, filterOptionIds, cancellationToken);

        return publicIds.ToDictionary(option => option.Id, option => option.PublicId);
    }

    private async Task<Dictionary<int, string>> GetLocationOptionsById(
        DataSetVersion dataSetVersion,
        IEnumerable<int> locationOptionIds,
        bool debug,
        CancellationToken cancellationToken
    )
    {
        if (debug)
        {
            var options = await locationRepository.ListOptions(dataSetVersion, locationOptionIds, cancellationToken);

            return options.ToDictionary(o => o.Id, GetLocationOptionDebugLabel);
        }

        var publicIds = await locationRepository.ListOptionPublicIds(
            dataSetVersion,
            locationOptionIds,
            cancellationToken
        );

        return publicIds.ToDictionary(option => option.Id, option => option.PublicId);
    }

    private async Task<Dictionary<int, TimePeriodViewModel>> GetTimePeriodsById(
        DataSetVersion dataSetVersion,
        IEnumerable<int> timePeriodIds,
        CancellationToken cancellationToken
    )
    {
        var timePeriods = await timePeriodRepository.List(dataSetVersion, timePeriodIds, cancellationToken);

        return timePeriods.ToDictionary(
            timePeriod => timePeriod.Id,
            timePeriod => new TimePeriodViewModel
            {
                Code = EnumUtil.GetFromEnumLabel<TimeIdentifier>(timePeriod.Identifier),
                Period = TimePeriodFormatter.FormatFromCsv(timePeriod.Period),
            }
        );
    }

    private static string GetLocationOptionDebugLabel(ParquetLocationOption option)
    {
        var level = EnumUtil.GetFromEnumValue<GeographicLevel>(option.Level);

        return level switch
        {
            GeographicLevel.LocalAuthority =>
                $"{option.PublicId} :: {option.Label} (code = {option.Code}, oldCode = {option.OldCode})",
            GeographicLevel.Provider => $"{option.PublicId} :: {option.Label} (ukprn = {option.Ukprn})",
            GeographicLevel.RscRegion => $"{option.PublicId} :: {option.Label}",
            GeographicLevel.School =>
                $"{option.PublicId} :: {option.Label} (urn = {option.Urn}, laEstab = {option.LaEstab})",
            _ => $"{option.PublicId} :: {option.Label} (code = {option.Code})",
        };
    }

    private class ColumnMappings
    {
        /// <summary>
        /// Key is level code, value is the column.
        /// </summary>
        public required Dictionary<string, string> LocationLevels { get; init; } = [];

        /// <summary>
        /// Key is the filter ID, value is the column.
        /// </summary>
        public required Dictionary<string, string> Filters { get; init; } = [];

        /// <summary>
        /// Key is the indicator ID, value is the column.
        /// </summary>
        public required Dictionary<string, string> Indicators { get; init; } = [];

        public IEnumerable<string> Columns => [.. LocationLevels.Values, .. Filters.Values, .. Indicators.Values];
    }
}
