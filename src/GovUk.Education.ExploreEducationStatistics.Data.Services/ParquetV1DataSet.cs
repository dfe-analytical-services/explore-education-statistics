#nullable enable
using System.Diagnostics;
using Dapper;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.Extensions.Logging;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services;

/// <summary>
/// Answers table tool queries from the Parquet copy of a data set's CSV file rather than from the Observation
/// tables in the statistics database. The Parquet file holds the raw CSV values, so results are mapped back onto
/// the Location, Filter Item and Indicator rows of the statistics database that the table tool refers to by ID.
/// Everything other than the observations, filter items and time periods matching a query is read from
/// <paramref name="statisticsDbDataSet" />.
/// </summary>
public class ParquetV1DataSet(
    File dataFile,
    IStorageDataSet statisticsDbDataSet,
    IDataFilesPathResolver dataFilesPathResolver,
    ILogger<ParquetV1DataSet> logger
) : IStorageDataSet
{
    private const string TimePeriodColumn = "time_period";
    private const string TimeIdentifierColumn = "time_identifier";
    private const string GeographicLevelColumn = "geographic_level";
    private const char KeySeparator = '\u001f';

    private static readonly EnumToEnumLabelConverter<TimeIdentifier> TimeIdentifierLookup = new();

    public Guid SubjectId => statisticsDbDataSet.SubjectId;

    public async Task<List<Observation>> ListObservations(
        FullTableQuery query,
        CancellationToken cancellationToken = default
    )
    {
        ValidateQuery(query);

        var stopwatch = Stopwatch.StartNew();

        var locations = await ListLocations(query.LocationIds, cancellationToken);

        if (locations.Count == 0)
        {
            return [];
        }

        var filters = await ListFilters(cancellationToken);
        var filterItemLookup = BuildFilterItemLookup(filters);
        var requestedFilterItems = await ListFilterItems(query.GetFilterItemIds(), cancellationToken);
        var indicators = await ListIndicators(query.Indicators, cancellationToken);

        await using var parquet = await OpenParquetFile(cancellationToken);

        var locationsByKey = new Dictionary<string, Location>();
        foreach (var location in locations)
        {
            if (!locationsByKey.TryAdd(LocationKey(parquet, location), location))
            {
                logger.LogWarning(
                    "Location {LocationId} has the same attributes as another requested Location and will be ignored",
                    location.Id
                );
            }
        }

        var columns = new[] { GeographicLevelColumn, TimePeriodColumn, TimeIdentifierColumn }
            .Concat(parquet.LocationColumns)
            .Concat(filters.SelectMany(FilterColumns))
            .Concat(indicators.Select(indicator => indicator.Name))
            .Where(parquet.Columns.Contains)
            .Distinct()
            .ToList();

        var predicates =
            LocationsPredicate(parquet, locations)
            + TimePeriodPredicate(query.TimePeriod)
            + FiltersPredicate(parquet, requestedFilterItems);

        var rows = await parquet.Query(
            $"""
            SELECT {columns.Select(Quote).JoinToString(", ")}
            FROM {parquet.Source}
            WHERE {predicates}
            """,
            cancellationToken
        );

        var observations = rows.Select(row =>
            {
                var locationKey = LocationKey(parquet, row);

                if (!locationsByKey.TryGetValue(locationKey, out var location))
                {
                    throw new InvalidOperationException(
                        $"No Location matches Parquet row with attributes '{locationKey.Replace(KeySeparator, ',')}'"
                    );
                }

                var (year, timeIdentifier) = GetTimePeriod(row);
                var observationId = Guid.NewGuid();

                return new Observation
                {
                    Id = observationId,
                    SubjectId = SubjectId,
                    Location = location,
                    LocationId = location.Id,
                    Year = year,
                    TimeIdentifier = timeIdentifier,
                    Measures = indicators.ToDictionary(
                        indicator => indicator.Id,
                        indicator => GetString(row, indicator.Name)
                    ),
                    FilterItems = filters
                        .Select(filter => new ObservationFilterItem
                        {
                            ObservationId = observationId,
                            FilterId = filter.Id,
                            FilterItemId = GetFilterItem(row, filter, filterItemLookup).Id,
                        })
                        .ToList(),
                };
            })
            .ToList();

        logger.LogTrace(
            "Fetched {ObservationCount} Observations from Parquet in {Milliseconds} ms",
            observations.Count,
            stopwatch.Elapsed.TotalMilliseconds
        );

        return observations;
    }

    public IAsyncEnumerable<IReadOnlyList<Observation>> ListObservationBatches(
        FullTableQuery query,
        int batchSize,
        CancellationToken cancellationToken = default
    )
    {
        return statisticsDbDataSet.ListObservationBatches(query, batchSize, cancellationToken);
    }

    public async Task<List<FilterItem>> ListFilterItemsForQuery(
        FullTableQuery query,
        CancellationToken cancellationToken = default
    )
    {
        ValidateQuery(query);

        var locations = await ListLocations(query.LocationIds, cancellationToken);
        var filters = await ListFilters(cancellationToken);

        if (locations.Count == 0 || filters.Count == 0)
        {
            return [];
        }

        var filterItemLookup = BuildFilterItemLookup(filters);

        await using var parquet = await OpenParquetFile(cancellationToken);

        var columns = filters.SelectMany(FilterColumns).Where(parquet.Columns.Contains).Distinct().ToList();

        var rows = await parquet.Query(
            $"""
            SELECT DISTINCT {(columns.Count > 0 ? columns.Select(Quote).JoinToString(", ") : "1")}
            FROM {parquet.Source}
            WHERE {LocationsPredicate(parquet, locations)}{TimePeriodPredicate(query.TimePeriod)}
            """,
            cancellationToken
        );

        return rows.SelectMany(row => filters.Select(filter => GetFilterItem(row, filter, filterItemLookup)))
            .Distinct()
            .ToList();
    }

    public Task<List<FilterItem>> ListFilterItems(
        IEnumerable<Guid> filterItemIds,
        CancellationToken cancellationToken = default
    )
    {
        return statisticsDbDataSet.ListFilterItems(filterItemIds, cancellationToken);
    }

    public Task<Dictionary<Guid, int>> CountFilterItemsByFilter(
        IEnumerable<Guid> filterItemIds,
        CancellationToken cancellationToken = default
    )
    {
        return statisticsDbDataSet.CountFilterItemsByFilter(filterItemIds, cancellationToken);
    }

    public Task<List<Filter>> ListFilters(CancellationToken cancellationToken = default)
    {
        return statisticsDbDataSet.ListFilters(cancellationToken);
    }

    public Task<List<Indicator>> ListIndicators(CancellationToken cancellationToken = default)
    {
        return statisticsDbDataSet.ListIndicators(cancellationToken);
    }

    public Task<List<Indicator>> ListIndicators(
        IEnumerable<Guid> indicatorIds,
        CancellationToken cancellationToken = default
    )
    {
        return statisticsDbDataSet.ListIndicators(indicatorIds, cancellationToken);
    }

    public Task<List<IndicatorGroup>> ListIndicatorGroups(CancellationToken cancellationToken = default)
    {
        return statisticsDbDataSet.ListIndicatorGroups(cancellationToken);
    }

    public Task<List<Location>> ListLocations(CancellationToken cancellationToken = default)
    {
        return statisticsDbDataSet.ListLocations(cancellationToken);
    }

    public Task<List<Location>> ListLocations(
        IEnumerable<Guid> locationIds,
        CancellationToken cancellationToken = default
    )
    {
        return statisticsDbDataSet.ListLocations(locationIds, cancellationToken);
    }

    public Task<List<(int Year, TimeIdentifier TimeIdentifier)>> ListTimePeriods(
        CancellationToken cancellationToken = default
    )
    {
        return statisticsDbDataSet.ListTimePeriods(cancellationToken);
    }

    public async Task<List<(int Year, TimeIdentifier TimeIdentifier)>> ListTimePeriods(
        IEnumerable<Guid> locationIds,
        CancellationToken cancellationToken = default
    )
    {
        var locations = await ListLocations(locationIds, cancellationToken);

        if (locations.Count == 0)
        {
            return [];
        }

        await using var parquet = await OpenParquetFile(cancellationToken);

        var yearColumn = $"substr({Quote(TimePeriodColumn)}, 1, 4) AS {Quote(TimePeriodColumn)}";

        var rows = await parquet.Query(
            $"""
            SELECT DISTINCT {yearColumn}, {Quote(TimeIdentifierColumn)}
            FROM {parquet.Source}
            WHERE {LocationsPredicate(parquet, locations)}
            """,
            cancellationToken
        );

        return rows.Select(GetTimePeriod)
            .Distinct()
            .OrderBy(tuple => tuple.Year)
            .ThenBy(tuple => tuple.TimeIdentifier)
            .ToList();
    }

    private void ValidateQuery(FullTableQuery query)
    {
        if (query.SubjectId != SubjectId)
        {
            throw new ArgumentException(
                $"Query SubjectId {query.SubjectId} does not match data set SubjectId {SubjectId}",
                nameof(query)
            );
        }
    }

    private async Task<ParquetFile> OpenParquetFile(CancellationToken cancellationToken)
    {
        var path = dataFilesPathResolver.ParquetV1Path(dataFile);

        var connection = new DuckDbConnection();
        await connection.OpenAsync(cancellationToken);

        var source = Literal(path);

        var columns = await connection.QueryAsync<string>(
            new CommandDefinition(
                $"SELECT column_name FROM (DESCRIBE SELECT * FROM {source})",
                cancellationToken: cancellationToken
            )
        );

        return new ParquetFile(connection, source, columns.ToHashSet());
    }

    private static string LocationsPredicate(ParquetFile parquet, IEnumerable<Location> locations)
    {
        var columnExpressions = new[] { $"lower({Quote(GeographicLevelColumn)})" }.Concat(
            parquet.LocationColumns.Select(column => $"coalesce(trim({Quote(column)}), '')")
        );

        var values = locations.Select(location =>
            $"({LocationKeyValues(parquet, location).Select(Literal).JoinToString(", ")})"
        );

        return $"({columnExpressions.JoinToString(", ")}) IN ({values.JoinToString(", ")})";
    }

    private static string TimePeriodPredicate(TimePeriodQuery? timePeriod)
    {
        if (timePeriod == null)
        {
            return string.Empty;
        }

        var values = TimePeriodUtil
            .Range(timePeriod)
            .Select(tuple =>
                $"({Literal(tuple.Year.ToString())}, {Literal(tuple.TimeIdentifier.GetEnumLabel().ToLower())})"
            );

        var columns = $"(substr({Quote(TimePeriodColumn)}, 1, 4), lower({Quote(TimeIdentifierColumn)}))";

        return $"\nAND {columns} IN ({values.JoinToString(", ")})";
    }

    private static string FiltersPredicate(ParquetFile parquet, IEnumerable<FilterItem> filterItems)
    {
        return filterItems
            .GroupBy(filterItem => filterItem.FilterGroup.Filter, Filter.IdComparer)
            .Select(grouping =>
            {
                var filter = grouping.Key;

                var values = grouping.Select(filterItem =>
                    $"({Literal(filterItem.FilterGroup.Label.ToLower())}, {Literal(filterItem.Label.ToLower())})"
                );

                var columns =
                    $"({FilterGroupLabelExpression(parquet, filter)}, {FilterItemLabelExpression(parquet, filter)})";

                return $"\nAND {columns} IN ({values.JoinToString(", ")})";
            })
            .JoinToString(string.Empty);
    }

    private static string FilterGroupLabelExpression(ParquetFile parquet, Filter filter)
    {
        return LabelExpression(parquet, filter.GroupCsvColumn, FilterGroup.NotSpecifiedFilterGroupLabel);
    }

    private static string FilterItemLabelExpression(ParquetFile parquet, Filter filter)
    {
        return LabelExpression(parquet, filter.Name, FilterItem.NotSpecifiedFilterItemLabel);
    }

    private static string LabelExpression(ParquetFile parquet, string? column, string defaultLabel)
    {
        return column != null && parquet.Columns.Contains(column)
            ? $"lower(coalesce(nullif(trim({Quote(column)}), ''), {Literal(defaultLabel)}))"
            : Literal(defaultLabel.ToLower());
    }

    private static IEnumerable<string> FilterColumns(Filter filter)
    {
        return new[] { filter.Name, filter.GroupCsvColumn }.WhereNotNull();
    }

    private static Dictionary<string, FilterItem> BuildFilterItemLookup(IEnumerable<Filter> filters)
    {
        return filters
            .SelectMany(filter =>
                filter.FilterGroups.SelectMany(filterGroup =>
                    filterGroup.FilterItems.Select(filterItem =>
                    {
                        filterGroup.Filter = filter;
                        filterItem.FilterGroup = filterGroup;
                        return (
                            Key: FilterItemKey(filter, filterGroup.Label, filterItem.Label),
                            FilterItem: filterItem
                        );
                    })
                )
            )
            .ToDictionary(tuple => tuple.Key, tuple => tuple.FilterItem);
    }

    private static FilterItem GetFilterItem(
        IDictionary<string, object?> row,
        Filter filter,
        Dictionary<string, FilterItem> filterItemLookup
    )
    {
        var filterGroupLabel = GetLabel(row, filter.GroupCsvColumn, FilterGroup.NotSpecifiedFilterGroupLabel);
        var filterItemLabel = GetLabel(row, filter.Name, FilterItem.NotSpecifiedFilterItemLabel);

        if (filterItemLookup.TryGetValue(FilterItemKey(filter, filterGroupLabel, filterItemLabel), out var filterItem))
        {
            return filterItem;
        }

        throw new InvalidOperationException(
            $"No Filter Item with label '{filterItemLabel}' in group '{filterGroupLabel}' exists for '{filter.Name}'"
        );
    }

    private static string FilterItemKey(Filter filter, string filterGroupLabel, string filterItemLabel)
    {
        return $"{filter.Id}{KeySeparator}{filterGroupLabel.ToLower()}{KeySeparator}{filterItemLabel.ToLower()}";
    }

    private static string GetLabel(IDictionary<string, object?> row, string? column, string defaultLabel)
    {
        return column != null && row.TryGetValue(column, out var value)
            ? value?.ToString()?.Trim().NullIfWhiteSpace() ?? defaultLabel
            : defaultLabel;
    }

    private static (int Year, TimeIdentifier TimeIdentifier) GetTimePeriod(IDictionary<string, object?> row)
    {
        var year = int.Parse(GetString(row, TimePeriodColumn)[..4]);
        var timeIdentifier = (TimeIdentifier)
            TimeIdentifierLookup.ConvertFromProvider.Invoke(GetString(row, TimeIdentifierColumn))!;

        return (year, timeIdentifier);
    }

    private static string LocationKey(ParquetFile parquet, Location location)
    {
        return LocationKeyValues(parquet, location).JoinToString(KeySeparator);
    }

    private static string LocationKey(ParquetFile parquet, IDictionary<string, object?> row)
    {
        return new[] { GetString(row, GeographicLevelColumn).ToLower() }
            .Concat(parquet.LocationColumns.Select(column => GetString(row, column).Trim()))
            .JoinToString(KeySeparator);
    }

    private static List<string> LocationKeyValues(ParquetFile parquet, Location location)
    {
        var csvValues = location.GetCsvValues();

        return
        [
            location.GeographicLevel.GetEnumLabel().ToLower(),
            .. parquet.LocationColumns.Select(column => csvValues.GetValueOrDefault(column, string.Empty).Trim()),
        ];
    }

    private static string GetString(IDictionary<string, object?> row, string column)
    {
        return row.TryGetValue(column, out var value) ? value?.ToString() ?? string.Empty : string.Empty;
    }

    private static string Quote(string identifier)
    {
        return $"\"{identifier.Replace("\"", "\"\"")}\"";
    }

    private static string Literal(string value)
    {
        return $"'{value.Replace("'", "''")}'";
    }

    private sealed class ParquetFile(DuckDbConnection connection, string source, HashSet<string> columns)
        : IAsyncDisposable
    {
        public string Source { get; } = source;

        public HashSet<string> Columns { get; } = columns;

        public List<string> LocationColumns { get; } =
            LocationCsvUtils.AllCsvColumns().Where(columns.Contains).ToList();

        public async Task<List<IDictionary<string, object?>>> Query(string sql, CancellationToken cancellationToken)
        {
            var rows = await connection.QueryAsync(new CommandDefinition(sql, cancellationToken: cancellationToken));
            return rows.Cast<IDictionary<string, object?>>().ToList();
        }

        public ValueTask DisposeAsync()
        {
            return connection.DisposeAsync();
        }
    }
}
