#nullable enable
using System.Data.Common;
using System.Runtime.CompilerServices;
using Dapper;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Utils;
using InterpolatedSql.Dapper;
using static GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.ParquetSqlUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet;

/// <summary>
/// Reads a data set from parquet files written by <see cref="ParquetDataSetWriter" />.
/// </summary>
/// <remarks>
/// The data set's meta (filters, indicators, locations and time periods) is small relative to its observations,
/// so it's read in full the first time it's needed and then held for the lifetime of the instance.
/// </remarks>
public class ParquetV1DataSet(Guid subjectId, IParquetDataSetPathResolver pathResolver) : IStorageDataSet
{
    private List<FilterRow>? _filters;
    private List<FilterOptionRow>? _filterOptions;
    private List<IndicatorRow>? _indicators;
    private Dictionary<Guid, Location>? _locations;
    private Dictionary<int, TimePeriodRow>? _timePeriods;

    public Guid SubjectId { get; } = subjectId;

    public async Task<List<Observation>> ListObservations(
        IEnumerable<Guid> filterItemIds,
        IEnumerable<Guid> locationIds,
        TimePeriodQuery? timePeriod,
        CancellationToken cancellationToken = default
    )
    {
        var observations = new List<Observation>();

        await foreach (
            var batch in ListObservationBatches(
                filterItemIds,
                locationIds,
                timePeriod,
                batchSize: 10_000,
                cancellationToken
            )
        )
        {
            observations.AddRange(batch);
        }

        return observations;
    }

    public async IAsyncEnumerable<IReadOnlyList<Observation>> ListObservationBatches(
        IEnumerable<Guid> filterItemIds,
        IEnumerable<Guid> locationIds,
        TimePeriodQuery? timePeriod,
        int batchSize,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var filters = await GetFilters(cancellationToken);
        var filterOptions = (await GetFilterOptions(cancellationToken)).ToDictionary(option => option.Id);
        var indicators = await GetIndicators(cancellationToken);
        var locations = await GetLocations(cancellationToken);
        var timePeriods = await GetTimePeriods(cancellationToken);

        string[] columns =
        [
            $"d.{DataTable.Cols.ObservationId}",
            $"d.{DataTable.Cols.LocationId}",
            $"d.{DataTable.Cols.CsvRow}",
            $"d.{DataTable.Cols.TimePeriodId}",
            .. filters.Select(filter => $"d.{Quote(filter.Column)}"),
            .. indicators.Select(indicator => $"d.{Quote(indicator.Column)}"),
        ];

        await using var connection = OpenConnection();

        var builder = connection.SqlBuilder(
            $"SELECT {columns.JoinToString(", "):raw} FROM {Literal(pathResolver.DataPath(SubjectId)):raw} AS d"
        );

        await AppendObservationsWhereClause(builder, filterItemIds, locationIds, timePeriod, cancellationToken);

        builder.Append($" ORDER BY d.{DataTable.Cols.Id:raw}");

        await using var reader = (DbDataReader)await builder.ExecuteReaderAsync(cancellationToken: cancellationToken);

        var filtersOrdinal = 4;
        var indicatorsOrdinal = filtersOrdinal + filters.Count;

        var batch = new List<Observation>(batchSize);

        while (await reader.ReadAsync(cancellationToken))
        {
            var observationId = reader.GetGuid(0);
            var locationId = reader.GetGuid(1);
            var timePeriodRow = timePeriods[reader.GetInt32(3)];

            var observation = new Observation
            {
                Id = observationId,
                SubjectId = SubjectId,
                LocationId = locationId,
                Location = locations[locationId],
                CsvRow = reader.GetInt64(2),
                Year = timePeriodRow.Year,
                TimeIdentifier = timePeriodRow.GetTimeIdentifier(),
                Measures = indicators
                    .Select((indicator, index) => (indicator, index))
                    .ToDictionary(
                        tuple => tuple.indicator.IndicatorId,
                        tuple =>
                            reader.IsDBNull(indicatorsOrdinal + tuple.index)
                                ? null!
                                : reader.GetString(indicatorsOrdinal + tuple.index)
                    ),
            };

            for (var index = 0; index < filters.Count; index++)
            {
                var filterOptionId = reader.GetInt32(filtersOrdinal + index);

                if (filterOptionId != 0)
                {
                    var filterOption = filterOptions[filterOptionId];

                    observation.FilterItems.Add(
                        new ObservationFilterItem
                        {
                            ObservationId = observationId,
                            FilterItemId = filterOption.FilterItemId,
                            FilterId = filterOption.FilterId,
                        }
                    );
                }
            }

            batch.Add(observation);

            if (batch.Count == batchSize)
            {
                yield return batch;
                batch = new List<Observation>(batchSize);
            }
        }

        if (batch.Count > 0)
        {
            yield return batch;
        }
    }

    public async Task<List<FilterItem>> ListFilterItemsForQuery(
        IEnumerable<Guid> locationIds,
        TimePeriodQuery? timePeriod,
        CancellationToken cancellationToken = default
    )
    {
        var filters = await GetFilters(cancellationToken);

        if (filters.Count == 0)
        {
            return [];
        }

        var filterColumns = filters.Select(filter => $"d.{Quote(filter.Column)}");

        await using var connection = OpenConnection();

        var builder = connection.SqlBuilder(
            $"""
            SELECT DISTINCT UNNEST([{filterColumns.JoinToString(", "):raw}]) AS filter_option_id
            FROM {Literal(pathResolver.DataPath(SubjectId)):raw} AS d
            """
        );

        await AppendObservationsWhereClause(builder, filterItemIds: [], locationIds, timePeriod, cancellationToken);

        var filterOptionIds = (await builder.QueryAsync<int>(cancellationToken: cancellationToken)).ToHashSet();

        var filterOptions = await GetFilterOptions(cancellationToken);

        return BuildFilterItems(filterOptions.Where(option => filterOptionIds.Contains(option.Id)), filters);
    }

    public async Task<List<FilterItem>> ListFilterItems(
        IEnumerable<Guid> filterItemIds,
        CancellationToken cancellationToken = default
    )
    {
        var filterItemIdSet = filterItemIds.ToHashSet();

        var filters = await GetFilters(cancellationToken);
        var filterOptions = await GetFilterOptions(cancellationToken);

        return BuildFilterItems(filterOptions.Where(option => filterItemIdSet.Contains(option.FilterItemId)), filters);
    }

    public async Task<List<Filter>> ListFilters(
        bool includeItems = false,
        CancellationToken cancellationToken = default
    )
    {
        var filters = (await GetFilters(cancellationToken)).Select(BuildFilter).ToList();

        if (!includeItems)
        {
            return filters;
        }

        var filterOptions = await GetFilterOptions(cancellationToken);
        var filtersById = filters.ToDictionary(filter => filter.Id);

        foreach (var optionsByGroup in filterOptions.GroupBy(option => option.FilterGroupId))
        {
            var firstOption = optionsByGroup.First();
            var filter = filtersById[firstOption.FilterId];

            var group = BuildFilterGroup(firstOption, filter);
            group.FilterItems = optionsByGroup.Select(option => BuildFilterItem(option, group)).ToList();

            filter.FilterGroups.Add(group);
        }

        return filters;
    }

    public async Task<List<(Guid ParentFilterItemId, Guid ChildFilterItemId)>> ListFilterItemRelationships(
        Guid parentFilterId,
        Guid childFilterId,
        CancellationToken cancellationToken = default
    )
    {
        var filters = await GetFilters(cancellationToken);

        var parentFilter = filters.SingleOrDefault(filter => filter.Id == parentFilterId);
        var childFilter = filters.SingleOrDefault(filter => filter.Id == childFilterId);

        if (parentFilter is null || childFilter is null)
        {
            return [];
        }

        var filterOptions = (await GetFilterOptions(cancellationToken)).ToDictionary(option => option.Id);

        await using var connection = OpenConnection();

        var pairs = await connection
            .SqlBuilder(
                $"""
                SELECT DISTINCT
                    d.{Quote(parentFilter.Column):raw} AS ParentFilterOptionId,
                    d.{Quote(childFilter.Column):raw} AS ChildFilterOptionId
                FROM {Literal(pathResolver.DataPath(SubjectId)):raw} AS d
                WHERE d.{Quote(parentFilter.Column):raw} <> 0
                AND d.{Quote(childFilter.Column):raw} <> 0
                """
            )
            .QueryAsync<(int ParentFilterOptionId, int ChildFilterOptionId)>(cancellationToken: cancellationToken);

        return pairs
            .Select(pair =>
                (
                    filterOptions[pair.ParentFilterOptionId].FilterItemId,
                    filterOptions[pair.ChildFilterOptionId].FilterItemId
                )
            )
            .ToList();
    }

    public async Task<List<Indicator>> ListIndicators(
        IEnumerable<Guid>? indicatorIds = null,
        CancellationToken cancellationToken = default
    )
    {
        var indicators = await GetIndicators(cancellationToken);

        if (indicatorIds is not null)
        {
            var indicatorIdSet = indicatorIds.ToHashSet();
            indicators = indicators.Where(indicator => indicatorIdSet.Contains(indicator.IndicatorId)).ToList();
        }

        return indicators.Select(BuildIndicator).ToList();
    }

    public async Task<List<IndicatorGroup>> ListIndicatorGroups(CancellationToken cancellationToken = default)
    {
        var indicators = await GetIndicators(cancellationToken);

        return indicators
            .GroupBy(indicator => indicator.IndicatorGroupId)
            .Select(indicatorsByGroup =>
            {
                var group = new IndicatorGroup
                {
                    Id = indicatorsByGroup.Key,
                    Label = indicatorsByGroup.First().IndicatorGroupLabel,
                    SubjectId = SubjectId,
                };

                group.Indicators = indicatorsByGroup
                    .Select(row =>
                    {
                        var indicator = BuildIndicator(row);
                        indicator.IndicatorGroup = group;
                        return indicator;
                    })
                    .ToList();

                return group;
            })
            .ToList();
    }

    public async Task<List<Location>> ListLocations(
        IEnumerable<Guid>? locationIds = null,
        CancellationToken cancellationToken = default
    )
    {
        var locations = await GetLocations(cancellationToken);

        if (locationIds is null)
        {
            return locations.Values.ToList();
        }

        return locationIds
            .Distinct()
            .Select(locationId => locations.GetValueOrDefault(locationId))
            .WhereNotNull()
            .ToList();
    }

    public async Task<List<GeographicLevel>> ListGeographicLevels(CancellationToken cancellationToken = default)
    {
        await using var connection = OpenConnection();

        var geographicLevels = await connection
            .SqlBuilder(
                $"""
                SELECT DISTINCT {DataTable.Cols.GeographicLevel:raw}
                FROM {Literal(pathResolver.DataPath(SubjectId)):raw}
                """
            )
            .QueryAsync<string>(cancellationToken: cancellationToken);

        return geographicLevels.Select(EnumUtil.GetFromEnumLabel<GeographicLevel>).ToList();
    }

    public async Task<List<(int Year, TimeIdentifier TimeIdentifier)>> ListTimePeriods(
        IEnumerable<Guid>? locationIds = null,
        CancellationToken cancellationToken = default
    )
    {
        var timePeriods = await GetTimePeriods(cancellationToken);

        if (locationIds is null)
        {
            return TimePeriodQueryUtils.Order(
                timePeriods.Values.Select(timePeriod => (timePeriod.Year, timePeriod.GetTimeIdentifier()))
            );
        }

        var locationIdList = locationIds.ToList();

        if (locationIdList.Count == 0)
        {
            return [];
        }

        await using var connection = OpenConnection();

        var timePeriodIds = await connection
            .SqlBuilder(
                $"""
                SELECT DISTINCT {DataTable.Cols.TimePeriodId:raw}
                FROM {Literal(pathResolver.DataPath(SubjectId)):raw}
                WHERE {DataTable.Cols.LocationId:raw} IN ({locationIdList})
                """
            )
            .QueryAsync<int>(cancellationToken: cancellationToken);

        return TimePeriodQueryUtils.Order(
            timePeriodIds.Select(id => (timePeriods[id].Year, timePeriods[id].GetTimeIdentifier()))
        );
    }

    /// <summary>
    /// Appends a WHERE clause matching observations in the same way as the StatsDB matching observations query.
    /// Within a filter, observations need to match any of its selected filter items, whereas across filters they
    /// need to match every filter that has a selection. Filters with every item selected are ignored.
    /// </summary>
    private async Task AppendObservationsWhereClause(
        DuckDbDapperSqlBuilder builder,
        IEnumerable<Guid> filterItemIds,
        IEnumerable<Guid> locationIds,
        TimePeriodQuery? timePeriod,
        CancellationToken cancellationToken
    )
    {
        builder.Append($" WHERE TRUE");

        var filterItemIdSet = filterItemIds.ToHashSet();

        if (filterItemIdSet.Count > 0)
        {
            var filterOptions = await GetFilterOptions(cancellationToken);

            foreach (var optionsByFilter in filterOptions.GroupBy(option => option.FilterColumn))
            {
                var selectedOptionIds = optionsByFilter
                    .Where(option => filterItemIdSet.Contains(option.FilterItemId))
                    .Select(option => option.Id)
                    .ToList();

                if (selectedOptionIds.Count > 0 && selectedOptionIds.Count < optionsByFilter.Count())
                {
                    builder.Append($" AND d.{Quote(optionsByFilter.Key):raw} IN ({selectedOptionIds})");
                }
            }
        }

        var locationIdList = locationIds.ToList();

        if (locationIdList.Count > 0)
        {
            builder.Append($" AND d.{DataTable.Cols.LocationId:raw} IN ({locationIdList})");
        }

        if (timePeriod is not null)
        {
            var timePeriods = await GetTimePeriods(cancellationToken);
            var timePeriodsInRange = TimePeriodUtil.Range(timePeriod).ToHashSet();

            var timePeriodIds = timePeriods
                .Values.Where(row => timePeriodsInRange.Contains((row.Year, row.GetTimeIdentifier())))
                .Select(row => row.Id)
                .ToList();

            if (timePeriodIds.Count > 0)
            {
                builder.Append($" AND d.{DataTable.Cols.TimePeriodId:raw} IN ({timePeriodIds})");
            }
            else
            {
                builder.Append($" AND FALSE");
            }
        }
    }

    private List<FilterItem> BuildFilterItems(IEnumerable<FilterOptionRow> filterOptions, List<FilterRow> filterRows)
    {
        var filters = new Dictionary<Guid, Filter>();
        var groups = new Dictionary<Guid, FilterGroup>();

        return filterOptions
            .Select(option =>
            {
                if (!filters.TryGetValue(option.FilterId, out var filter))
                {
                    filter = BuildFilter(filterRows.Single(row => row.Id == option.FilterId));
                    filters[option.FilterId] = filter;
                }

                if (!groups.TryGetValue(option.FilterGroupId, out var group))
                {
                    group = BuildFilterGroup(option, filter);
                    groups[option.FilterGroupId] = group;
                    filter.FilterGroups.Add(group);
                }

                var item = BuildFilterItem(option, group);
                group.FilterItems.Add(item);
                return item;
            })
            .ToList();
    }

    private Filter BuildFilter(FilterRow row) =>
        new()
        {
            Id = row.Id,
            Label = row.Label,
            Name = row.Column,
            Hint = row.Hint,
            GroupCsvColumn = row.GroupCsvColumn,
            ParentFilter = row.ParentFilter,
            AutoSelectFilterItemId = row.AutoSelectFilterItemId,
            AutoSelectFilterItemLabel = row.AutoSelectFilterItemLabel,
            SubjectId = SubjectId,
        };

    private static FilterGroup BuildFilterGroup(FilterOptionRow option, Filter filter) =>
        new()
        {
            Id = option.FilterGroupId,
            Label = option.FilterGroupLabel,
            FilterId = filter.Id,
            Filter = filter,
        };

    private static FilterItem BuildFilterItem(FilterOptionRow option, FilterGroup group) =>
        new()
        {
            Id = option.FilterItemId,
            Label = option.Label,
            FilterGroupId = group.Id,
            FilterGroup = group,
        };

    private static Indicator BuildIndicator(IndicatorRow row) =>
        new()
        {
            Id = row.IndicatorId,
            Label = row.Label,
            Name = row.Column,
            Unit = EnumUtil.GetFromEnumLabel<IndicatorUnit>(row.Unit),
            DecimalPlaces = row.DecimalPlaces,
            IndicatorGroupId = row.IndicatorGroupId,
        };

    private async Task<List<FilterRow>> GetFilters(CancellationToken cancellationToken)
    {
        if (_filters is not null)
        {
            return _filters;
        }

        await using var connection = OpenConnection();

        _filters = (
            await connection
                .SqlBuilder(
                    $"""
                    SELECT
                        {FiltersTable.Cols.Id:raw} AS Id,
                        {FiltersTable.Cols.Column:raw} AS "Column",
                        {FiltersTable.Cols.Label:raw} AS Label,
                        {FiltersTable.Cols.Hint:raw} AS Hint,
                        {FiltersTable.Cols.GroupCsvColumn:raw} AS GroupCsvColumn,
                        {FiltersTable.Cols.ParentFilter:raw} AS ParentFilter,
                        {FiltersTable.Cols.AutoSelectFilterItemId:raw} AS AutoSelectFilterItemId,
                        {FiltersTable.Cols.AutoSelectFilterItemLabel:raw} AS AutoSelectFilterItemLabel
                    FROM {Literal(pathResolver.FiltersPath(SubjectId)):raw}
                    """
                )
                .QueryAsync<FilterRow>(cancellationToken: cancellationToken)
        ).AsList();

        return _filters;
    }

    private async Task<List<FilterOptionRow>> GetFilterOptions(CancellationToken cancellationToken)
    {
        if (_filterOptions is not null)
        {
            return _filterOptions;
        }

        await using var connection = OpenConnection();

        _filterOptions = (
            await connection
                .SqlBuilder(
                    $"""
                    SELECT
                        {FilterOptionsTable.Cols.Id:raw} AS Id,
                        {FilterOptionsTable.Cols.Label:raw} AS Label,
                        {FilterOptionsTable.Cols.FilterColumn:raw} AS FilterColumn,
                        {FilterOptionsTable.Cols.FilterItemId:raw} AS FilterItemId,
                        {FilterOptionsTable.Cols.FilterGroupId:raw} AS FilterGroupId,
                        {FilterOptionsTable.Cols.FilterGroupLabel:raw} AS FilterGroupLabel,
                        {FilterOptionsTable.Cols.EesFilterId:raw} AS FilterId
                    FROM {Literal(pathResolver.FilterOptionsPath(SubjectId)):raw}
                    ORDER BY {FilterOptionsTable.Cols.Id:raw}
                    """
                )
                .QueryAsync<FilterOptionRow>(cancellationToken: cancellationToken)
        ).AsList();

        return _filterOptions;
    }

    private async Task<List<IndicatorRow>> GetIndicators(CancellationToken cancellationToken)
    {
        if (_indicators is not null)
        {
            return _indicators;
        }

        await using var connection = OpenConnection();

        _indicators = (
            await connection
                .SqlBuilder(
                    $"""
                    SELECT
                        {IndicatorsTable.Cols.Column:raw} AS "Column",
                        {IndicatorsTable.Cols.Label:raw} AS Label,
                        {IndicatorsTable.Cols.Unit:raw} AS Unit,
                        CAST({IndicatorsTable.Cols.DecimalPlaces:raw} AS INTEGER) AS DecimalPlaces,
                        {IndicatorsTable.Cols.IndicatorId:raw} AS IndicatorId,
                        {IndicatorsTable.Cols.IndicatorGroupId:raw} AS IndicatorGroupId,
                        {IndicatorsTable.Cols.IndicatorGroupLabel:raw} AS IndicatorGroupLabel
                    FROM {Literal(pathResolver.IndicatorsPath(SubjectId)):raw}
                    """
                )
                .QueryAsync<IndicatorRow>(cancellationToken: cancellationToken)
        ).AsList();

        return _indicators;
    }

    private async Task<Dictionary<Guid, Location>> GetLocations(CancellationToken cancellationToken)
    {
        if (_locations is not null)
        {
            return _locations;
        }

        await using var connection = OpenConnection();

        string[] columns =
        [
            LocationsTable.Cols.Id,
            LocationsTable.Cols.GeographicLevel,
            .. LocationsTable.AttributeColumns.Select(column => column.Name),
        ];

        await using var reader = (DbDataReader)
            await connection
                .SqlBuilder(
                    $"""
                    SELECT {columns.JoinToString(", "):raw}
                    FROM {Literal(pathResolver.LocationsPath(SubjectId)):raw}
                    """
                )
                .ExecuteReaderAsync(cancellationToken: cancellationToken);

        var locations = new Dictionary<Guid, Location>();

        while (await reader.ReadAsync(cancellationToken))
        {
            var location = new Location
            {
                Id = reader.GetGuid(0),
                GeographicLevel = EnumUtil.GetFromEnumValue<GeographicLevel>(reader.GetString(1)),
            };

            for (var index = 0; index < LocationsTable.AttributeColumns.Count; index++)
            {
                var ordinal = index + 2;

                LocationsTable
                    .AttributeColumns[index]
                    .SetValue(location, reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal));
            }

            locations[location.Id] = location;
        }

        _locations = locations;
        return _locations;
    }

    private async Task<Dictionary<int, TimePeriodRow>> GetTimePeriods(CancellationToken cancellationToken)
    {
        if (_timePeriods is not null)
        {
            return _timePeriods;
        }

        await using var connection = OpenConnection();

        _timePeriods = (
            await connection
                .SqlBuilder(
                    $"""
                    SELECT
                        {TimePeriodsTable.Cols.Id:raw} AS Id,
                        {TimePeriodsTable.Cols.Year:raw} AS Year,
                        {TimePeriodsTable.Cols.TimeIdentifier:raw} AS TimeIdentifier
                    FROM {Literal(pathResolver.TimePeriodsPath(SubjectId)):raw}
                    """
                )
                .QueryAsync<TimePeriodRow>(cancellationToken: cancellationToken)
        ).ToDictionary(row => row.Id);

        return _timePeriods;
    }

    private static DuckDbConnection OpenConnection()
    {
        var connection = new DuckDbConnection();
        connection.Open();
        return connection;
    }

    private class FilterRow
    {
        public Guid Id { get; init; }
        public string Column { get; init; } = string.Empty;
        public string Label { get; init; } = string.Empty;
        public string? Hint { get; init; }
        public string? GroupCsvColumn { get; init; }
        public string? ParentFilter { get; init; }
        public Guid? AutoSelectFilterItemId { get; init; }
        public string? AutoSelectFilterItemLabel { get; init; }
    }

    private class FilterOptionRow
    {
        public int Id { get; init; }
        public string Label { get; init; } = string.Empty;
        public string FilterColumn { get; init; } = string.Empty;
        public Guid FilterItemId { get; init; }
        public Guid FilterGroupId { get; init; }
        public string FilterGroupLabel { get; init; } = string.Empty;
        public Guid FilterId { get; init; }
    }

    private class IndicatorRow
    {
        public string Column { get; init; } = string.Empty;
        public string Label { get; init; } = string.Empty;
        public string Unit { get; init; } = string.Empty;
        public int? DecimalPlaces { get; init; }
        public Guid IndicatorId { get; init; }
        public Guid IndicatorGroupId { get; init; }
        public string IndicatorGroupLabel { get; init; } = string.Empty;
    }

    private class TimePeriodRow
    {
        public int Id { get; init; }
        public int Year { get; init; }
        public string TimeIdentifier { get; init; } = string.Empty;

        public TimeIdentifier GetTimeIdentifier() =>
            EnumUtil.GetFromEnumValue<Common.Model.TimeIdentifier>(TimeIdentifier);
    }
}
