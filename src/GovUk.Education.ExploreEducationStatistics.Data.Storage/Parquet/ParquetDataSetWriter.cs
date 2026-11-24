#nullable enable
using Dapper;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using InterpolatedSql.Dapper;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.ParquetSqlUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet;

public class ParquetDataSetWriter(IParquetDataSetPathResolver pathResolver, ILogger<ParquetDataSetWriter> logger)
    : IParquetDataSetWriter
{
    private const string StagingTableName = "staging_observations";
    private const string LocationLinksTableName = "location_links";

    private static class StagingCols
    {
        public const string ObservationId = "observation_id";
        public const string CsvRow = "csv_row";
        public const string LocationId = "location_id";
        public const string Year = "year";
        public const string TimeIdentifier = "time_identifier";

        public static string Filter(Filter filter) => $"f_{filter.Id:N}";

        public static string Indicator(Indicator indicator) => $"i_{indicator.Id:N}";
    }

    public bool HasIndicators(Guid subjectId)
    {
        return File.Exists(pathResolver.IndicatorsPath(subjectId));
    }

    public Task WriteIndicators(
        Guid subjectId,
        IReadOnlyList<Indicator> indicators,
        CancellationToken cancellationToken = default
    )
    {
        return WriteTables(
            subjectId,
            new Dictionary<string, string> { { IndicatorsTable.TableName, pathResolver.IndicatorsPath(subjectId) } },
            connection => CreateIndicatorsTable(connection, indicators, cancellationToken),
            cancellationToken
        );
    }

    public bool HasFilters(Guid subjectId)
    {
        return File.Exists(pathResolver.FiltersPath(subjectId))
            && File.Exists(pathResolver.FilterOptionsPath(subjectId));
    }

    public Task WriteFilters(
        Guid subjectId,
        IReadOnlyList<Filter> filters,
        CancellationToken cancellationToken = default
    )
    {
        return WriteTables(
            subjectId,
            new Dictionary<string, string>
            {
                { FiltersTable.TableName, pathResolver.FiltersPath(subjectId) },
                { FilterOptionsTable.TableName, pathResolver.FilterOptionsPath(subjectId) },
            },
            connection => CreateFiltersTables(connection, filters, cancellationToken),
            cancellationToken
        );
    }

    public Task WriteLocations(
        Guid subjectId,
        IReadOnlyList<Location> locations,
        CancellationToken cancellationToken = default
    )
    {
        return WriteTables(
            subjectId,
            new Dictionary<string, string>
            {
                { LocationsTable.TableName, pathResolver.LocationsPath(subjectId) },
                { LocationOptionsTable.TableName, pathResolver.LocationOptionsPath(subjectId) },
            },
            connection => CreateLocationTables(connection, locations, cancellationToken),
            cancellationToken
        );
    }

    public async Task StartStaging(
        Guid subjectId,
        IReadOnlyList<Filter> filters,
        IReadOnlyList<Indicator> indicators,
        long? resumeFromCsvRow,
        CancellationToken cancellationToken = default
    )
    {
        ValidateColumnNames(filters, indicators);

        Directory.CreateDirectory(pathResolver.DirectoryPath(subjectId));

        if (resumeFromCsvRow is null)
        {
            DeleteStagingDb(subjectId);
        }

        string[] columns =
        [
            $"{StagingCols.ObservationId} UUID NOT NULL",
            $"{StagingCols.CsvRow} BIGINT NOT NULL",
            $"{StagingCols.LocationId} UUID NOT NULL",
            $"{StagingCols.Year} INTEGER NOT NULL",
            $"{StagingCols.TimeIdentifier} VARCHAR NOT NULL",
            .. OrderStagedFilters(filters).Select(filter => $"{StagingCols.Filter(filter)} UUID"),
            .. OrderStagedIndicators(indicators).Select(indicator => $"{StagingCols.Indicator(indicator)} VARCHAR"),
        ];

        await using var connection = OpenStagingDb(subjectId);

        await connection
            .SqlBuilder($"CREATE TABLE IF NOT EXISTS {StagingTableName:raw} ({columns.JoinToString(", "):raw})")
            .ExecuteAsync(cancellationToken: cancellationToken);

        if (resumeFromCsvRow is not null)
        {
            var deleted = await connection
                .SqlBuilder($"DELETE FROM {StagingTableName:raw} WHERE {StagingCols.CsvRow:raw} >= {resumeFromCsvRow}")
                .ExecuteAsync(cancellationToken: cancellationToken);

            logger.LogInformation(
                "Resuming staging of observations for subject {SubjectId} from CSV row {CsvRow}, removed {Deleted} "
                    + "previously staged observations",
                subjectId,
                resumeFromCsvRow,
                deleted
            );
        }
    }

    // @MarkFix This opens and closes a DuckDB file connection to the staging db for every batch (about 3,300 times for a
    // 10 million row import, each close flushing over the Azure Files mount), and the export path does the same every
    // 10,000 rows. Measure it, then hold one connection between StartStaging and Complete/CleanUp.
    public async Task AppendObservations(
        Guid subjectId,
        IReadOnlyList<Filter> filters,
        IReadOnlyList<Indicator> indicators,
        IEnumerable<Observation> observations,
        CancellationToken cancellationToken = default
    )
    {
        var orderedFilters = OrderStagedFilters(filters);
        var orderedIndicators = OrderStagedIndicators(indicators);

        await using var connection = OpenStagingDb(subjectId);
        using var appender = connection.CreateAppender(StagingTableName);

        foreach (var observation in observations)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var filterItemIdsByFilterId = observation
                .FilterItems.Where(item => item.FilterId.HasValue)
                .ToDictionary(item => item.FilterId!.Value, item => item.FilterItemId);

            var row = appender
                .CreateRow()
                .AppendValue(observation.Id)
                .AppendValue(observation.CsvRow)
                .AppendValue(observation.LocationId)
                .AppendValue(observation.Year)
                .AppendValue(observation.TimeIdentifier.GetEnumValue());

            foreach (var filter in orderedFilters)
            {
                row = filterItemIdsByFilterId.TryGetValue(filter.Id, out var filterItemId)
                    ? row.AppendValue(filterItemId)
                    : row.AppendNullValue();
            }

            foreach (var indicator in orderedIndicators)
            {
                row = row.AppendValue(observation.Measures.GetValueOrDefault(indicator.Id));
            }

            row.EndRow();
        }
    }

    public async Task<long> CountStagedObservations(Guid subjectId, CancellationToken cancellationToken = default)
    {
        await using var connection = OpenStagingDb(subjectId);

        return await connection
            .SqlBuilder($"SELECT COUNT(*) FROM {StagingTableName:raw}")
            .ExecuteScalarAsync<long>(cancellationToken: cancellationToken);
    }

    public async Task WriteTimePeriodsAndData(
        Guid subjectId,
        IReadOnlyList<Filter> filters,
        IReadOnlyList<Indicator> indicators,
        IReadOnlyList<Location> locations,
        CancellationToken cancellationToken = default
    )
    {
        ValidateColumnNames(filters, indicators);

        var tableFiles = new Dictionary<string, string>
        {
            { DataTable.TableName, pathResolver.DataPath(subjectId) },
            { TimePeriodsTable.TableName, pathResolver.TimePeriodsPath(subjectId) },
        };

        await using (var connection = OpenStagingDb(subjectId))
        {
            await CreateTimePeriodsTable(connection, cancellationToken);

            await connection
                .SqlBuilder(
                    $"""
                    CREATE OR REPLACE TABLE {FilterOptionsTable.TableName:raw} AS
                    SELECT * FROM {Literal(pathResolver.FilterOptionsPath(subjectId)):raw}
                    """
                )
                .ExecuteAsync(cancellationToken: cancellationToken);

            var locationOptionIds = await ReadLocationOptionIds(connection, subjectId, cancellationToken);
            var levels = await CreateLocationLinksTable(connection, locations, locationOptionIds, cancellationToken);

            await CreateDataTable(connection, filters, indicators, levels, cancellationToken);
            await ExportTables(connection, tableFiles, cancellationToken);
        }

        ReplaceFiles(tableFiles.Values);

        logger.LogInformation("Wrote data parquet files for subject {SubjectId}", subjectId);
    }

    private async Task WriteTables(
        Guid subjectId,
        Dictionary<string, string> tableFiles,
        Func<IDuckDbConnection, Task> createTables,
        CancellationToken cancellationToken
    )
    {
        Directory.CreateDirectory(pathResolver.DirectoryPath(subjectId));

        await using (var connection = new DuckDbConnection())
        {
            connection.Open();

            await createTables(connection);
            await ExportTables(connection, tableFiles, cancellationToken);
        }

        ReplaceFiles(tableFiles.Values);

        logger.LogInformation(
            "Wrote {Tables} parquet files for subject {SubjectId}",
            tableFiles.Keys.JoinToString(", "),
            subjectId
        );
    }

    private static async Task ExportTables(
        IDuckDbConnection connection,
        Dictionary<string, string> tableFiles,
        CancellationToken cancellationToken
    )
    {
        foreach (var (table, path) in tableFiles)
        {
            await connection
                .SqlBuilder($"COPY {table:raw} TO {Literal(TempPath(path)):raw} (FORMAT PARQUET, CODEC ZSTD)")
                .ExecuteAsync(cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// Only replaces the parquet files once every table has been exported, so that a failure part way through
    /// never leaves a mix of old and new files.
    ///
    /// Each table is exported to a temp path first and moved into place here, so a file only ever appears at its
    /// real path once it is complete. HasIndicators and HasFilters rely on this: if the processor restarts part way
    /// through an import, stage 2 is run again and skips any tables whose files exist. Exporting straight to the real
    /// path could leave a half-written file there, which would then be taken as already written.
    /// </summary>
    private static void ReplaceFiles(IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            File.Move(TempPath(path), path, overwrite: true);
        }
    }

    private static async Task CreateTimePeriodsTable(IDuckDbConnection connection, CancellationToken cancellationToken)
    {
        await connection
            .SqlBuilder(
                $"""
                CREATE OR REPLACE TABLE {TimePeriodsTable.TableName:raw}(
                    {TimePeriodsTable.Cols.Id:raw} INTEGER PRIMARY KEY,
                    {TimePeriodsTable.Cols.Period:raw} VARCHAR,
                    {TimePeriodsTable.Cols.Identifier:raw} VARCHAR,
                    {TimePeriodsTable.Cols.Year:raw} INTEGER,
                    {TimePeriodsTable.Cols.TimeIdentifier:raw} VARCHAR
                )
                """
            )
            .ExecuteAsync(cancellationToken: cancellationToken);

        var stagedTimePeriods = await connection
            .SqlBuilder(
                $"""
                SELECT DISTINCT
                    {StagingCols.Year:raw} AS Year,
                    {StagingCols.TimeIdentifier:raw} AS TimeIdentifier
                FROM {StagingTableName:raw}
                """
            )
            .QueryAsync<(int Year, string TimeIdentifier)>(cancellationToken: cancellationToken);

        // Order by the Public API's period format (e.g. 2020/2021) and then by identifier, as the
        // Public API does, so that ids can be compared to query time period ranges.
        var timePeriods = stagedTimePeriods
            .Select(timePeriod =>
            {
                var timeIdentifier = EnumUtil.GetFromEnumValue<TimeIdentifier>(timePeriod.TimeIdentifier);
                var csvPeriod = TimePeriodLabelFormatter.FormatCsvYear(timePeriod.Year, timeIdentifier);

                return new
                {
                    timePeriod.Year,
                    TimeIdentifier = timeIdentifier,
                    CsvPeriod = csvPeriod,
                    Period = TimePeriodFormatter.FormatFromCsv(csvPeriod),
                };
            })
            .OrderBy(timePeriod => timePeriod.Period)
            .ThenBy(timePeriod => timePeriod.TimeIdentifier);

        using var appender = connection.CreateAppender(TimePeriodsTable.TableName);

        var id = 1;

        foreach (var timePeriod in timePeriods)
        {
            appender
                .CreateRow()
                .AppendValue(id++)
                .AppendValue(timePeriod.CsvPeriod)
                .AppendValue(timePeriod.TimeIdentifier.GetEnumLabel())
                .AppendValue(timePeriod.Year)
                .AppendValue(timePeriod.TimeIdentifier.GetEnumValue())
                .EndRow();
        }
    }

    private static async Task CreateFiltersTables(
        IDuckDbConnection connection,
        IReadOnlyList<Filter> filters,
        CancellationToken cancellationToken
    )
    {
        await connection
            .SqlBuilder(
                $"""
                CREATE OR REPLACE TABLE {FiltersTable.TableName:raw}(
                    {FiltersTable.Cols.Id:raw} UUID PRIMARY KEY,
                    {FiltersTable.Cols.PublicId:raw} VARCHAR,
                    {FiltersTable.Cols.Column:raw} VARCHAR,
                    {FiltersTable.Cols.Label:raw} VARCHAR,
                    {FiltersTable.Cols.Hint:raw} VARCHAR,
                    {FiltersTable.Cols.GroupCsvColumn:raw} VARCHAR,
                    {FiltersTable.Cols.ParentFilter:raw} VARCHAR,
                    {FiltersTable.Cols.AutoSelectFilterItemId:raw} UUID,
                    {FiltersTable.Cols.AutoSelectFilterItemLabel:raw} VARCHAR
                )
                """
            )
            .ExecuteAsync(cancellationToken: cancellationToken);

        await connection
            .SqlBuilder(
                $"""
                CREATE OR REPLACE TABLE {FilterOptionsTable.TableName:raw}(
                    {FilterOptionsTable.Cols.Id:raw} INTEGER PRIMARY KEY,
                    {FilterOptionsTable.Cols.Label:raw} VARCHAR,
                    {FilterOptionsTable.Cols.PublicId:raw} VARCHAR,
                    {FilterOptionsTable.Cols.FilterId:raw} VARCHAR,
                    {FilterOptionsTable.Cols.FilterColumn:raw} VARCHAR,
                    {FilterOptionsTable.Cols.FilterItemId:raw} UUID,
                    {FilterOptionsTable.Cols.FilterGroupId:raw} UUID,
                    {FilterOptionsTable.Cols.FilterGroupLabel:raw} VARCHAR,
                    {FilterOptionsTable.Cols.EesFilterId:raw} UUID
                )
                """
            )
            .ExecuteAsync(cancellationToken: cancellationToken);

        using var filtersAppender = connection.CreateAppender(FiltersTable.TableName);
        using var filterOptionsAppender = connection.CreateAppender(FilterOptionsTable.TableName);

        var filterOrdinal = 1;
        var optionId = 1;

        foreach (var filter in filters.OrderBy(filter => filter.Name))
        {
            var filterPublicId = SqidEncoder.Encode(filterOrdinal++);

            filtersAppender
                .CreateRow()
                .AppendValue(filter.Id)
                .AppendValue(filterPublicId)
                .AppendValue(filter.Name)
                .AppendValue(filter.Label)
                .AppendValue(filter.Hint)
                .AppendValue(filter.GroupCsvColumn)
                .AppendValue(filter.ParentFilter)
                .AppendValue(filter.AutoSelectFilterItemId)
                .AppendValue(filter.AutoSelectFilterItemLabel)
                .EndRow();

            var items = filter
                .FilterGroups.SelectMany(group => group.FilterItems.Select(item => (Group: group, Item: item)))
                .OrderBy(tuple => tuple.Item.Label)
                .ThenBy(tuple => tuple.Group.Label);

            foreach (var (group, item) in items)
            {
                var id = optionId++;

                filterOptionsAppender
                    .CreateRow()
                    .AppendValue(id)
                    .AppendValue(item.Label)
                    .AppendValue(SqidEncoder.Encode(id))
                    .AppendValue(filterPublicId)
                    .AppendValue(filter.Name)
                    .AppendValue(item.Id)
                    .AppendValue(group.Id)
                    .AppendValue(group.Label)
                    .AppendValue(filter.Id)
                    .EndRow();
            }
        }
    }

    private static async Task CreateIndicatorsTable(
        IDuckDbConnection connection,
        IReadOnlyList<Indicator> indicators,
        CancellationToken cancellationToken
    )
    {
        await connection
            .SqlBuilder(
                $"""
                CREATE OR REPLACE TABLE {IndicatorsTable.TableName:raw}(
                    {IndicatorsTable.Cols.Id:raw} VARCHAR PRIMARY KEY,
                    {IndicatorsTable.Cols.Column:raw} VARCHAR,
                    {IndicatorsTable.Cols.Label:raw} VARCHAR,
                    {IndicatorsTable.Cols.Unit:raw} VARCHAR,
                    {IndicatorsTable.Cols.DecimalPlaces:raw} TINYINT,
                    {IndicatorsTable.Cols.IndicatorId:raw} UUID,
                    {IndicatorsTable.Cols.IndicatorGroupId:raw} UUID,
                    {IndicatorsTable.Cols.IndicatorGroupLabel:raw} VARCHAR
                )
                """
            )
            .ExecuteAsync(cancellationToken: cancellationToken);

        using var appender = connection.CreateAppender(IndicatorsTable.TableName);

        var ordinal = 1;

        foreach (var indicator in indicators.OrderBy(indicator => indicator.Name))
        {
            appender
                .CreateRow()
                .AppendValue(SqidEncoder.Encode(ordinal++))
                .AppendValue(indicator.Name)
                .AppendValue(indicator.Label)
                .AppendValue(indicator.Unit.GetEnumLabel())
                .AppendValue((sbyte?)indicator.DecimalPlaces)
                .AppendValue(indicator.Id)
                .AppendValue(indicator.IndicatorGroup.Id)
                .AppendValue(indicator.IndicatorGroup.Label)
                .EndRow();
        }
    }

    /// <summary>
    /// Creates the locations table and the Public API's location options table.
    /// </summary>
    private static async Task CreateLocationTables(
        IDuckDbConnection connection,
        IReadOnlyList<Location> locations,
        CancellationToken cancellationToken
    )
    {
        string[] locationColumns =
        [
            $"{LocationsTable.Cols.Id} UUID PRIMARY KEY",
            $"{LocationsTable.Cols.GeographicLevel} VARCHAR",
            .. LocationsTable.AttributeColumns.Select(column => $"{column.Name} VARCHAR"),
        ];

        await connection
            .SqlBuilder(
                $"CREATE OR REPLACE TABLE {LocationsTable.TableName:raw} ({locationColumns.JoinToString(", "):raw})"
            )
            .ExecuteAsync(cancellationToken: cancellationToken);

        using (var appender = connection.CreateAppender(LocationsTable.TableName))
        {
            foreach (var location in locations)
            {
                var row = appender
                    .CreateRow()
                    .AppendValue(location.Id)
                    .AppendValue(location.GeographicLevel.GetEnumValue());

                foreach (var column in LocationsTable.AttributeColumns)
                {
                    row = row.AppendValue(column.GetValue(location));
                }

                row.EndRow();
            }
        }

        var optionsByLevel = locations
            .SelectMany(location =>
                EnumUtil
                    .GetEnums<GeographicLevel>()
                    .Select(level => LocationOption.Create(location, level))
                    .WhereNotNull()
            )
            .Distinct()
            .GroupBy(option => option.Level)
            .OrderBy(group => group.Key.GetEnumLabel())
            .ToList();

        await connection
            .SqlBuilder(
                $"""
                CREATE OR REPLACE TABLE {LocationOptionsTable.TableName:raw}(
                    {LocationOptionsTable.Cols.Id:raw} INTEGER PRIMARY KEY,
                    {LocationOptionsTable.Cols.Label:raw} VARCHAR,
                    {LocationOptionsTable.Cols.Level:raw} VARCHAR,
                    {LocationOptionsTable.Cols.PublicId:raw} VARCHAR,
                    {LocationOptionsTable.Cols.Code:raw} VARCHAR,
                    {LocationOptionsTable.Cols.OldCode:raw} VARCHAR,
                    {LocationOptionsTable.Cols.Urn:raw} VARCHAR,
                    {LocationOptionsTable.Cols.LaEstab:raw} VARCHAR,
                    {LocationOptionsTable.Cols.Ukprn:raw} VARCHAR
                )
                """
            )
            .ExecuteAsync(cancellationToken: cancellationToken);

        using (var appender = connection.CreateAppender(LocationOptionsTable.TableName))
        {
            var id = 1;

            foreach (var option in optionsByLevel.SelectMany(group => group.OrderBy(option => option.Label)))
            {
                appender
                    .CreateRow()
                    .AppendValue(id)
                    .AppendValue(option.Label)
                    .AppendValue(option.Level.GetEnumValue())
                    .AppendValue(SqidEncoder.Encode(id))
                    .AppendValue(option.Code)
                    .AppendValue(option.OldCode)
                    .AppendValue(option.Urn)
                    .AppendNullValue()
                    .AppendValue(option.Ukprn)
                    .EndRow();

                id++;
            }
        }
    }

    /// <summary>
    /// Reads the ids of the location options written by <see cref="WriteLocations" />.
    /// </summary>
    private async Task<Dictionary<LocationOption, int>> ReadLocationOptionIds(
        IDuckDbConnection connection,
        Guid subjectId,
        CancellationToken cancellationToken
    )
    {
        var rows = await connection
            .SqlBuilder(
                $"""
                SELECT
                    {LocationOptionsTable.Cols.Id:raw} AS Id,
                    {LocationOptionsTable.Cols.Label:raw} AS Label,
                    {LocationOptionsTable.Cols.Level:raw} AS Level,
                    {LocationOptionsTable.Cols.Code:raw} AS Code,
                    {LocationOptionsTable.Cols.OldCode:raw} AS OldCode,
                    {LocationOptionsTable.Cols.Urn:raw} AS Urn,
                    {LocationOptionsTable.Cols.Ukprn:raw} AS Ukprn
                FROM {Literal(pathResolver.LocationOptionsPath(subjectId)):raw}
                """
            )
            .QueryAsync<LocationOptionRow>(cancellationToken: cancellationToken);

        return rows.ToDictionary(
            row => new LocationOption(
                EnumUtil.GetFromEnumValue<GeographicLevel>(row.Level),
                row.Label,
                Code: row.Code,
                OldCode: row.OldCode,
                Urn: row.Urn,
                Ukprn: row.Ukprn
            ),
            row => row.Id
        );
    }

    /// <summary>
    /// Creates a table linking each location to its location option at each geographic level.
    /// </summary>
    /// <returns>The geographic levels that have location options.</returns>
    private static async Task<List<GeographicLevel>> CreateLocationLinksTable(
        IDuckDbConnection connection,
        IReadOnlyList<Location> locations,
        Dictionary<LocationOption, int> optionIds,
        CancellationToken cancellationToken
    )
    {
        var levels = optionIds
            .Keys.Select(option => option.Level)
            .Distinct()
            .OrderBy(level => level.GetEnumLabel())
            .ToList();

        string[] linkColumns =
        [
            $"{StagingCols.LocationId} UUID PRIMARY KEY",
            $"{DataTable.Cols.GeographicLevel} VARCHAR NOT NULL",
            .. levels.Select(level => $"{DataTable.Cols.LocationOptionId(level)} INTEGER NOT NULL"),
        ];

        await connection
            .SqlBuilder($"CREATE OR REPLACE TABLE {LocationLinksTableName:raw} ({linkColumns.JoinToString(", "):raw})")
            .ExecuteAsync(cancellationToken: cancellationToken);

        using (var appender = connection.CreateAppender(LocationLinksTableName))
        {
            foreach (var location in locations)
            {
                var row = appender
                    .CreateRow()
                    .AppendValue(location.Id)
                    .AppendValue(location.GeographicLevel.GetEnumLabel());

                foreach (var level in levels)
                {
                    var option = LocationOption.Create(location, level);
                    row = row.AppendValue(option is not null ? optionIds[option] : 0);
                }

                row.EndRow();
            }
        }

        return levels;
    }

    private static async Task CreateDataTable(
        IDuckDbConnection connection,
        IReadOnlyList<Filter> filters,
        IReadOnlyList<Indicator> indicators,
        IReadOnlyList<GeographicLevel> levels,
        CancellationToken cancellationToken
    )
    {
        var orderedFilters = filters.OrderBy(filter => filter.Name).ToList();
        var orderedIndicators = indicators.OrderBy(indicator => indicator.Name).ToList();

        string[] columns =
        [
            $"{DataTable.Cols.Id} UINTEGER NOT NULL PRIMARY KEY",
            $"{DataTable.Cols.TimePeriodId} INTEGER NOT NULL",
            $"{DataTable.Cols.GeographicLevel} VARCHAR NOT NULL",
            .. levels.Select(level => $"{DataTable.Cols.LocationOptionId(level)} INTEGER NOT NULL"),
            .. orderedFilters.Select(filter => $"{Quote(filter.Name)} INTEGER NOT NULL"),
            .. orderedIndicators.Select(indicator => $"{Quote(indicator.Name)} VARCHAR"),
            $"{DataTable.Cols.ObservationId} UUID NOT NULL",
            $"{DataTable.Cols.LocationId} UUID NOT NULL",
            $"{DataTable.Cols.CsvRow} BIGINT NOT NULL",
        ];

        await connection
            .SqlBuilder($"CREATE OR REPLACE TABLE {DataTable.TableName:raw} ({columns.JoinToString(",\n"):raw})")
            .ExecuteAsync(cancellationToken: cancellationToken);

        // Rows are ordered in the same way as the Public API's data files.
        string[] selectColumns =
        [
            "row_number() OVER (ORDER BY ll.geographic_level ASC, tp.period DESC, s.csv_row ASC)",
            $"tp.{TimePeriodsTable.Cols.Id}",
            $"ll.{DataTable.Cols.GeographicLevel}",
            .. levels.Select(level => $"ll.{DataTable.Cols.LocationOptionId(level)}"),
            .. orderedFilters.Select((_, index) => $"COALESCE(fo_{index}.{FilterOptionsTable.Cols.Id}, 0)"),
            .. orderedIndicators.Select(indicator => $"s.{StagingCols.Indicator(indicator)}"),
            $"s.{StagingCols.ObservationId}",
            $"s.{StagingCols.LocationId}",
            $"s.{StagingCols.CsvRow}",
        ];

        string[] joins =
        [
            $"""
                JOIN {LocationLinksTableName} AS ll
                ON ll.{StagingCols.LocationId} = s.{StagingCols.LocationId}
                """,
            $"""
                JOIN {TimePeriodsTable.TableName} AS tp
                ON tp.{TimePeriodsTable.Cols.Year} = s.{StagingCols.Year}
                AND tp.{TimePeriodsTable.Cols.TimeIdentifier} = s.{StagingCols.TimeIdentifier}
                """,
            .. orderedFilters.Select(
                (filter, index) =>
                    $"""
                        LEFT JOIN {FilterOptionsTable.TableName} AS fo_{index}
                        ON fo_{index}.{FilterOptionsTable.Cols.FilterItemId} = s.{StagingCols.Filter(filter)}
                        """
            ),
        ];

        await connection
            .SqlBuilder(
                $"""
                INSERT INTO {DataTable.TableName:raw}
                SELECT {selectColumns.JoinToString(",\n"):raw}
                FROM {StagingTableName:raw} AS s
                {joins.JoinToString("\n"):raw}
                ORDER BY 1
                """
            )
            .ExecuteAsync(cancellationToken: cancellationToken);

        var stagedCount = await connection
            .SqlBuilder($"SELECT COUNT(*) FROM {StagingTableName:raw}")
            .ExecuteScalarAsync<long>(cancellationToken: cancellationToken);

        var dataCount = await connection
            .SqlBuilder($"SELECT COUNT(*) FROM {DataTable.TableName:raw}")
            .ExecuteScalarAsync<long>(cancellationToken: cancellationToken);

        if (stagedCount != dataCount)
        {
            throw new InvalidOperationException(
                $"Wrote {dataCount} rows to the {DataTable.TableName} table but {stagedCount} observations were "
                    + "staged. Check that every staged observation's location and time period could be found."
            );
        }
    }

    private static void ValidateColumnNames(IReadOnlyList<Filter> filters, IReadOnlyList<Indicator> indicators)
    {
        var columnNames = filters.Select(filter => filter.Name).Concat(indicators.Select(indicator => indicator.Name));

        var invalidColumnNames = columnNames
            .GroupBy(name => name, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1 || DataTable.IsReservedColumn(group.Key))
            .Select(group => group.Key)
            .ToList();

        if (invalidColumnNames.Count > 0)
        {
            throw new InvalidOperationException(
                "Filter and indicator columns must be unique and must not use a reserved column name. "
                    + $"Invalid columns: {invalidColumnNames.JoinToString(", ")}"
            );
        }
    }

    /// <summary>
    /// The staging table's columns are ordered by filter id so that the same order is used when resuming.
    /// </summary>
    private static List<Filter> OrderStagedFilters(IEnumerable<Filter> filters) =>
        filters.OrderBy(filter => filter.Id).ToList();

    /// <summary>
    /// The staging table's columns are ordered by indicator id so that the same order is used when resuming.
    /// </summary>
    private static List<Indicator> OrderStagedIndicators(IEnumerable<Indicator> indicators) =>
        indicators.OrderBy(indicator => indicator.Id).ToList();

    private DuckDbConnection OpenStagingDb(Guid subjectId)
    {
        var connection = DuckDbConnection.CreateFileConnection(pathResolver.StagingDbPath(subjectId));
        connection.Open();
        return connection;
    }

    public void DeleteStagingDb(Guid subjectId)
    {
        var stagingDbPath = pathResolver.StagingDbPath(subjectId);
        File.Delete(stagingDbPath);
        File.Delete($"{stagingDbPath}.wal");
    }

    private static string TempPath(string path) => $"{path}.tmp";

    private class LocationOptionRow
    {
        public int Id { get; init; }
        public string Label { get; init; } = string.Empty;
        public string Level { get; init; } = string.Empty;
        public string? Code { get; init; }
        public string? OldCode { get; init; }
        public string? Urn { get; init; }
        public string? Ukprn { get; init; }
    }

    /// <summary>
    /// A location option at a single geographic level, which is equivalent to a Public API location option.
    /// </summary>
    private record LocationOption(
        GeographicLevel Level,
        string Label,
        string? Code = null,
        string? OldCode = null,
        string? Urn = null,
        string? Ukprn = null
    )
    {
        public static LocationOption? Create(Location location, GeographicLevel level)
        {
            return level switch
            {
                GeographicLevel.LocalAuthority => location.LocalAuthority is { Name: { } name } la
                && (HasValue(la.Code) || HasValue(la.OldCode))
                && HasValue(name)
                    ? new LocationOption(
                        level,
                        name,
                        Code: la.Code.NullIfWhiteSpace(),
                        OldCode: la.OldCode.NullIfWhiteSpace()
                    )
                    : null,
                GeographicLevel.Provider => location.Provider is { Code: { } ukprn, Name: { } name }
                && HasValue(ukprn)
                && HasValue(name)
                    ? new LocationOption(level, name, Ukprn: ukprn)
                    : null,
                GeographicLevel.School => location.School is { Code: { } urn, Name: { } name }
                && HasValue(urn)
                && HasValue(name)
                    ? new LocationOption(level, name, Urn: urn)
                    : null,
                GeographicLevel.RscRegion => location.RscRegion is { Code: { } name } && HasValue(name)
                    ? new LocationOption(level, name)
                    : null,
                _ => GetAttribute(location, level) is { Code: { } code, Name: { } name }
                && HasValue(code)
                && HasValue(name)
                    ? new LocationOption(level, name, Code: code)
                    : null,
            };
        }

        private static bool HasValue(string? value) => !value.IsNullOrWhitespace();

        private static LocationAttribute? GetAttribute(Location location, GeographicLevel level) =>
            level switch
            {
                GeographicLevel.Country => location.Country,
                GeographicLevel.EnglishDevolvedArea => location.EnglishDevolvedArea,
                GeographicLevel.Institution => location.Institution,
                GeographicLevel.LocalAuthorityDistrict => location.LocalAuthorityDistrict,
                GeographicLevel.LocalEnterprisePartnership => location.LocalEnterprisePartnership,
                GeographicLevel.LocalSkillsImprovementPlanArea => location.LocalSkillsImprovementPlanArea,
                GeographicLevel.MayoralCombinedAuthority => location.MayoralCombinedAuthority,
                GeographicLevel.MultiAcademyTrust => location.MultiAcademyTrust,
                GeographicLevel.OpportunityArea => location.OpportunityArea,
                GeographicLevel.ParliamentaryConstituency => location.ParliamentaryConstituency,
                GeographicLevel.PlanningArea => location.PlanningArea,
                GeographicLevel.Region => location.Region,
                GeographicLevel.Sponsor => location.Sponsor,
                GeographicLevel.Ward => location.Ward,
                GeographicLevel.PoliceForceArea => location.PoliceForceArea,
                _ => null,
            };
    }
}
