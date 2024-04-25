using System.Collections.Immutable;
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
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
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
    IUserService userService,
    IDataSetQueryParser dataSetQueryParser,
    IParquetDataRepository dataRepository,
    IParquetFilterRepository filterRepository,
    IParquetIndicatorRepository indicatorRepository,
    IParquetLocationRepository locationRepository,
    IParquetTimePeriodRepository timePeriodRepository)
    : IDataSetQueryService
{
    private static readonly Dictionary<string, string> ReservedSorts = new()
    {
        { "timePeriod", DataTable.Cols.TimePeriodId },
        { "geographicLevel", DataTable.Cols.GeographicLevel },
    };

    private static readonly ImmutableHashSet<string> ReservedColumns =
    [
        DataTable.Cols.Id,
        DataTable.Cols.GeographicLevel,
        DataTable.Cols.TimePeriodId
    ];

    private static string LocationsSortField(GeographicLevel level) => $"locations|{level.GetEnumValue()}";

    public async Task<Either<ActionResult, DataSetQueryPaginatedResultsViewModel>> Query(
        Guid dataSetId,
        DataSetGetQueryRequest request,
        string? dataSetVersion = null,
        CancellationToken cancellationToken = default)
    {
        using var _ = MiniProfiler.Current
            .Step($"{nameof(DataSetQueryService)}.{nameof(Query)}");

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
        };

        return await FindDataSetVersion(dataSetId, dataSetVersion, cancellationToken)
            .OnSuccessDo(userService.CheckCanQueryDataSetVersion)
            .OnSuccess(dsv => RunQuery(
                dataSetVersion: dsv,
                request: query,
                page: request.Page,
                pageSize: request.PageSize,
                debug: request.Debug,
                cancellationToken: cancellationToken
            ))
            .OnSuccess(results => results with
            {
                Warnings = results.Warnings.Select(MapGetQueryWarning).ToList()
            })
            .OnFailureFailWith(result =>
                result is not BadRequestObjectResult { Value: ValidationProblemViewModel validationProblem }
                ? result
                : ValidationUtils.ValidationResult(validationProblem.Errors.Select(MapGetQueryError))
            );
    }

    private async Task<Either<ActionResult, DataSetVersion>> FindDataSetVersion(
        Guid dataSetId,
        string? dataSetVersion,
        CancellationToken cancellationToken)
    {
        using var _ = MiniProfiler.Current
            .Step($"{nameof(DataSetQueryService)}.{nameof(FindDataSetVersion)}");

        if (dataSetVersion is null)
        {
            return await publicDataDbContext.DataSets
                .AsNoTracking()
                .Include(ds => ds.LatestLiveVersion)
                .Where(ds => ds.Id == dataSetId)
                .Select(ds => ds.LatestLiveVersion!)
                .SingleOrNotFoundAsync(cancellationToken);
        }

        if (!VersionUtils.TryParse(dataSetVersion, out var version))
        {
            return new NotFoundResult();
        }

        return await publicDataDbContext.DataSetVersions
            .AsNoTracking()
            .Where(dsv => dsv.DataSetId == dataSetId)
            .Where(dsv => dsv.VersionMajor == version.Major)
            .Where(dsv => dsv.VersionMinor == version.Minor)
            .SingleOrNotFoundAsync(cancellationToken);
    }

    private async Task<Either<ActionResult, DataSetQueryPaginatedResultsViewModel>> RunQuery(
        DataSetVersion dataSetVersion,
        DataSetQueryRequest request,
        int page,
        int pageSize,
        bool debug,
        CancellationToken cancellationToken)
    {
        using var _ = MiniProfiler.Current
            .Step($"{nameof(DataSetQueryService)}.{nameof(RunQuery)}");

        var queryState = new QueryState();

        var whereBuilder = new DuckDbSqlBuilder();

        if (request.Criteria is not null)
        {
            whereBuilder += await dataSetQueryParser.ParseCriteria(
                request.Criteria,
                dataSetVersion,
                queryState,
                cancellationToken
            );
        }

        var columnsTask = dataRepository.ListColumns(dataSetVersion, cancellationToken);
        var indicatorsTask = indicatorRepository.ListIds(dataSetVersion, cancellationToken);

        await Task.WhenAll(columnsTask, indicatorsTask);

        var indicators = GetIndicators(request, indicatorsTask.Result, queryState);

        var sorts = GetSorts(
            request: request,
            columns: columnsTask.Result,
            indicators: indicators,
            queryState: queryState);

        if (queryState.Errors.Count != 0)
        {
            return ValidationUtils.ValidationResult(queryState.Errors);
        }

        var whereSql = whereBuilder.Build();

        var columnsByType = GetColumnsByType(
            columns: columnsTask.Result,
            allIndicators: indicatorsTask.Result,
            selectedIndicators: indicators);

        var countTask = dataRepository.CountRows(
            dataSetVersion: dataSetVersion,
            where: whereSql,
            cancellationToken: cancellationToken);

        var rowsTask = dataRepository.ListRows(
            dataSetVersion: dataSetVersion,
            columns: [
                DataTable.Cols.TimePeriodId,
                DataTable.Cols.GeographicLevel,
                ..columnsByType.All
            ],
            where: whereSql,
            sorts: sorts,
            page: page,
            pageSize: pageSize,
            cancellationToken: cancellationToken);

        await Task.WhenAll(countTask, rowsTask);

        if (debug)
        {
            queryState.Warnings.Add(new WarningViewModel
            {
                Code = ValidationMessages.DebugEnabled.Code,
                Message = ValidationMessages.DebugEnabled.Message,
            });
        }

        if (countTask.Result == 0)
        {
            queryState.Warnings.Add(new WarningViewModel
            {
                Code = ValidationMessages.QueryNoResults.Code,
                Message = ValidationMessages.QueryNoResults.Message,
            });
        }

        return new DataSetQueryPaginatedResultsViewModel
        {
            Paging = new PagingViewModel(page, pageSize, (int)countTask.Result),
            Results = await MapQueryResults(
                rows: rowsTask.Result,
                dataSetVersion: dataSetVersion,
                columnsByType: columnsByType,
                debug: debug,
                cancellationToken: cancellationToken),
            Warnings = queryState.Warnings
        };
    }

    private static HashSet<string> GetIndicators(
        DataSetQueryRequest request,
        ISet<string> allowedIndicators,
        QueryState queryState)
    {
        var validIndicators = new HashSet<string>();
        var invalidIndicators = new HashSet<string>();

        foreach (var indicator in request.Indicators)
        {
            if (!allowedIndicators.Contains(indicator))
            {
                invalidIndicators.Add(indicator);
            }
            else
            {
                validIndicators.Add(indicator);
            }
        }

        if (invalidIndicators.Count != 0)
        {
            queryState.Errors.Add(new ErrorViewModel
            {
                Code = ValidationMessages.IndicatorsNotFound.Code,
                Message = ValidationMessages.IndicatorsNotFound.Message,
                Path = "indicators",
                Detail = new NotFoundItemsErrorDetail<string>(invalidIndicators)
            });
        }

        return validIndicators;
    }

    private static List<Sort> GetSorts(
        DataSetQueryRequest request,
        IEnumerable<string> columns,
        IEnumerable<string> indicators,
        QueryState queryState)
    {
        if (request.Sorts is null)
        {
            return [new Sort(Field: DataTable.Cols.TimePeriodId, Direction: SortDirection.Desc)];
        }

        var locationLevels = GeographicLevelUtils.Levels
            .Where(level => columns.Contains(DataTable.Cols.LocationId(level)))
            .ToHashSet();

        var fieldColumnMap = new Dictionary<string, string>(ReservedSorts);

        foreach (var locationLevel in locationLevels)
        {
            fieldColumnMap[LocationsSortField(locationLevel)] = DataTable.Cols.LocationId(locationLevel);
        }

        var otherFields = columns
            .Except([
                DataTable.Cols.Id,
                ..ReservedSorts.Keys,
                ..locationLevels.Select(DataTable.Cols.LocationId),
            ])
            .Concat(indicators)
            .ToHashSet();

        var invalidSorts = new HashSet<DataSetQuerySort>();
        var sorts = new List<Sort>();

        foreach (var sort in request.Sorts)
        {
            if (fieldColumnMap.TryGetValue(sort.Field, out var field))
            {
                sorts.Add(new Sort(Field: field, Direction: sort.ParsedDirection()));
                continue;
            }

            if (otherFields.Contains(sort.Field))
            {
                sorts.Add(new Sort(Field: sort.Field, Direction: sort.ParsedDirection()));
                continue;
            }

            invalidSorts.Add(sort);
        }

        if (invalidSorts.Count != 0)
        {
            queryState.Errors.Add(new ErrorViewModel
            {
                Message = ValidationMessages.SortFieldsNotFound.Message,
                Code = ValidationMessages.SortFieldsNotFound.Code,
                Path = "sorts",
                Detail = new NotFoundItemsErrorDetail<DataSetQuerySort>(invalidSorts)
            });
        }

        return sorts;
    }

    private static ColumnsByType GetColumnsByType(
        ISet<string> columns,
        ISet<string> allIndicators,
        ISet<string> selectedIndicators)
    {
        var locationLevels = GeographicLevelUtils.Levels
            .Where(level => columns.Contains(DataTable.Cols.LocationId(level)))
            .ToHashSet();

        var filterColumns = columns
            .Except([
                ..ReservedColumns,
                ..locationLevels.Select(DataTable.Cols.LocationId),
                ..allIndicators
            ])
            .ToHashSet();

        return new ColumnsByType
        {
            Filters = filterColumns,
            LocationLevels = locationLevels,
            Indicators = selectedIndicators
        };
    }

    private async Task<List<DataSetQueryResultViewModel>> MapQueryResults(
        IList<IDictionary<string, object?>> rows,
        DataSetVersion dataSetVersion,
        ColumnsByType columnsByType,
        bool debug,
        CancellationToken cancellationToken)
    {
        using var _ = MiniProfiler.Current.Step(
            $"{nameof(DataSetQueryService)}.{nameof(MapQueryResults)}");

        var locationColumnsByCode = columnsByType.LocationLevels
            .ToDictionary(
                level => level.GetEnumValue(),
                DataTable.Cols.LocationId
            );

        var timePeriodIds = new HashSet<int>();
        var locationOptionIds = new HashSet<int>();
        var filterOptionIds = new HashSet<int>();

        foreach (var row in rows)
        {
            timePeriodIds.Add((int)row[DataTable.Cols.TimePeriodId]!);

            foreach (var (_, locationColumn) in locationColumnsByCode)
            {
                if (row[locationColumn] is int locationOptionId)
                {
                    locationOptionIds.Add(locationOptionId);
                }
            }

            foreach (var filterColumn in columnsByType.Filters)
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
        var filterOptionsById =
            GetFilterOptionsById(dataSetVersion, filterOptionIds, debug, cancellationToken);
        var locationOptionsById =
            GetLocationOptionsById(dataSetVersion, locationOptionIds, debug, cancellationToken);
        var timePeriodsById = 
            GetTimePeriodsById(dataSetVersion, timePeriodIds, cancellationToken);

        await Task.WhenAll(filterOptionsById, locationOptionsById, timePeriodsById);

        return rows.Select(row => new DataSetQueryResultViewModel
        {
            GeographicLevel = EnumUtil.GetFromEnumLabel<GeographicLevel>((string)row[DataTable.Cols.GeographicLevel]!),
            TimePeriod = timePeriodsById.Result[(int)row[DataTable.Cols.TimePeriodId]!],
            Filters = columnsByType.Filters
                .Where(filter => row[filter] is int and not 0)
                .ToDictionary(
                    filter => filter,
                    filter => filterOptionsById.Result[(int)row[filter]!]
                ),
            Locations = locationColumnsByCode
                .Where(kv => row[kv.Value] is int and not 0)
                .ToDictionary(
                    kv => kv.Key,
                    kv => locationOptionsById.Result[(int)row[kv.Value]!]
                ),
            Values = columnsByType.Indicators.ToDictionary(
                indicator => indicator,
                indicator => row[indicator] as string ?? string.Empty
            )
        })
        .ToList();
    }

    private static WarningViewModel MapGetQueryWarning(WarningViewModel warning)
    {
        if (warning.Code == ValidationMessages.LocationsNotFound.Code
            && warning.Detail is NotFoundItemsErrorDetail<DataSetQueryLocation> notFoundLocations)
        {
            return warning with
            {
                Detail = new NotFoundItemsErrorDetail<string>(
                    notFoundLocations.Items.Select(item => item.ToLocationString())
                )
            };
        }

        if (warning.Code == ValidationMessages.TimePeriodsNotFound.Code
            && warning.Detail is NotFoundItemsErrorDetail<DataSetQueryTimePeriod> notFoundTimePeriods)
        {
            return warning with
            {
                Detail = new NotFoundItemsErrorDetail<string>(
                    notFoundTimePeriods.Items.Select(item => item.ToTimePeriodString())
                )
            };
        }

        return warning;
    }

    private static ErrorViewModel MapGetQueryError(ErrorViewModel error)
    {
        if (error.Code == ValidationMessages.SortFieldsNotFound.Code
            && error.Detail is NotFoundItemsErrorDetail<DataSetQuerySort> notFoundSortFields)
        {
            return error with
            {
                Detail = new NotFoundItemsErrorDetail<string>(
                    notFoundSortFields.Items.Select(item => item.ToSortString())
                )
            };
        }

        return error;
    }

    private async Task<Dictionary<int, string>> GetFilterOptionsById(
        DataSetVersion dataSetVersion,
        IEnumerable<int> filterOptionIds,
        bool debug,
        CancellationToken cancellationToken)
    {
        if (debug)
        {
            var options =
                await filterRepository.ListOptions(dataSetVersion, filterOptionIds, cancellationToken);

            return options.ToDictionary(
                option => option.Id,
                option => $"{option.PublicId} :: {option.Label}"
            );
        }

        var publicIds =
            await filterRepository.ListOptionPublicIds(dataSetVersion, filterOptionIds, cancellationToken);

        return publicIds.ToDictionary(
            option => option.Id,
            option => option.PublicId
        );
    }

    private async Task<Dictionary<int, string>> GetLocationOptionsById(
        DataSetVersion dataSetVersion,
        IEnumerable<int> locationOptionIds,
        bool debug,
        CancellationToken cancellationToken)
    {
        if (debug)
        {
            var options =
                await locationRepository.ListOptions(dataSetVersion, locationOptionIds, cancellationToken);

            return options.ToDictionary(
                o => o.Id,
                GetLocationOptionDebugLabel);
        }

        var publicIds =
            await locationRepository.ListOptionPublicIds(dataSetVersion, locationOptionIds, cancellationToken);

        return publicIds.ToDictionary(
            option => option.Id,
            option => option.PublicId
        );
    }

    private async Task<Dictionary<int, TimePeriodViewModel>> GetTimePeriodsById(
        DataSetVersion dataSetVersion,
        IEnumerable<int> timePeriodIds,
        CancellationToken cancellationToken)
    {
        var timePeriods =
            await timePeriodRepository.List(dataSetVersion, timePeriodIds, cancellationToken);

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
            GeographicLevel.Provider =>
                $"{option.PublicId} :: {option.Label} (ukprn = {option.Ukprn})",
            GeographicLevel.RscRegion =>
                $"{option.PublicId} :: {option.Label}",
            GeographicLevel.School =>
                $"{option.PublicId} :: {option.Label} (urn = {option.Urn}, laEstab = {option.LaEstab})",
            _ => $"{option.PublicId} :: {option.Label} (code = {option.Code})"
        };
    }

    private class ColumnsByType
    {
        public required IEnumerable<GeographicLevel> LocationLevels { get; init; } = [];

        public required IEnumerable<string> Filters { get; init; } = [];

        public required IEnumerable<string> Indicators { get; init; } = [];

        public IEnumerable<string> All =>
        [
            ..LocationLevels.Select(DataTable.Cols.LocationId),
            ..Filters,
            ..Indicators
        ];
    }
}
