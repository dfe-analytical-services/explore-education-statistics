using System.Diagnostics;
using System.Reflection;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using CliWrap;
using Dapper;
using DuckDB.NET.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Scripts.Models;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Scripts.Seeds;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.GeographicLevelUtils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Scripts.Commands;

[Command("seed:data", Description = "Generate seed data for the public API")]
public class SeedDataCommand : ICommand
{
    private const string DbConnectionString = "Host=db;Username=postgres;Password=password;Database=public_data";

    [CommandOption("dump-sql", Description = "Dump seed data SQL to the `data/public-api-db` directory")]
    public bool DumpSql { get; init; }

    public async ValueTask ExecuteAsync(IConsole console)
    {
        var cancellationToken = console.RegisterCancellationHandler();

        await using var dbContext = await SetUpPublicDataDbContext(cancellationToken);

        // Set current directory to the assembly's directory to simplify pathing
        Directory.SetCurrentDirectory(Assembly.GetExecutingAssembly().GetDirectoryPath());

        // Match CSV columns with underscores
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        var dataSetSeeds = new List<DataSetSeed>
        {
            DataSetSeed.AbsenceRatesCharacteristic,
            DataSetSeed.AbsenceRatesGeographicLevel,
            DataSetSeed.AbsenceRatesGeographicLevelSchool,
            DataSetSeed.SpcEthnicityLanguage,
            DataSetSeed.SpcYearGroupGender,
            DataSetSeed.Nat01,
            DataSetSeed.Qua01,
        };

        var stopwatch = Stopwatch.StartNew();

        await console.Output.WriteLineAsync($"Started seeding data for {dataSetSeeds.Count} data sets");

        foreach (var seed in dataSetSeeds)
        {
            await using var duckDb = new DuckDBConnection("DataSource=:memory:");
            await duckDb.OpenAsync(cancellationToken);

            var seeder = new Seeder(
                seed: seed,
                dbContext: dbContext,
                duckDb: duckDb,
                console: console,
                cancellationToken: cancellationToken
            );

            await seeder.Generate();

            await duckDb.CloseAsync();
        }

        stopwatch.Stop();

        await console.Output.WriteLineAsync($"Finished all seeding in {stopwatch.Elapsed.TotalSeconds} seconds!");

        if (DumpSql)
        {
            await DumpSqlFile(console);
        }
    }

    private static async Task<PublicDataDbContext> SetUpPublicDataDbContext(CancellationToken cancellationToken)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(DbConnectionString);

        var options = new DbContextOptionsBuilder<PublicDataDbContext>()
            .UseNpgsql(dataSourceBuilder.Build())
            .Options;

        var dbContext = new PublicDataDbContext(options);

        dbContext.Database.SetCommandTimeout(300);
        await dbContext.Database.MigrateAsync(cancellationToken: cancellationToken);

        LinqToDBForEFTools.Initialize();

        var tables = dbContext.Model.GetEntityTypes()
            .Select(type => type.GetTableName())
            .Distinct()
            .Cast<string>()
            .ToList();

        // Clear any tables in case we're re-running the command
        foreach (var table in tables)
        {
#pragma warning disable EF1002
            await dbContext.Database.ExecuteSqlRawAsync(
#pragma warning restore EF1002
                $"""TRUNCATE TABLE "{table}" RESTART IDENTITY CASCADE;""",
                cancellationToken: cancellationToken
            );
        }

        return dbContext;
    }

    private async Task DumpSqlFile(IConsole console)
    {
        await console.Output.WriteLineAsync("Dumping seed data SQL...");

        var pgDumpVersion = await Cli.Wrap("pg_dump")
            .WithArguments("--version")
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync();

        if (pgDumpVersion.ExitCode != 0)
        {
            throw new NotSupportedException("Install `pg_dump` to dump seed data SQL");
        }

        var outputDir = Path.Combine(PathUtils.ProjectRootPath, "data", "public-api-db");

        if (!Path.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        var sqlFilePath = Path.Combine(outputDir, "public_data.sql");

        await Cli.Wrap("pg_dump")
            .WithArguments([
                "--dbname", "public_data",
                "--file", sqlFilePath,
                "--schema", "public",
                "--username", "postgres",
                "--host", "db",
                "--port", "5432"
            ])
            .WithEnvironmentVariables(env => env
                .Set("PGPASSWORD", "password"))
            .WithStandardOutputPipe(PipeTarget.ToStream(console.Output.BaseStream))
            .WithStandardErrorPipe(PipeTarget.ToStream(console.Error.BaseStream))
            .ExecuteAsync();

        await console.Output.WriteLineAsync($"Finished dumping seed data SQL to: {sqlFilePath}");
    }

    private class Seeder
    {
        private const string FiltersDuckDbTable = "filters";
        private const string TimePeriodsDuckDbTable = "time_periods";

        private readonly DataSetSeed _seed;
        private readonly PublicDataDbContext _dbContext;
        private readonly DuckDBConnection _duckDb;
        private readonly IConsole _console;
        private readonly CancellationToken _cancellationToken;

        private readonly string _dataFilePath;
        private readonly string _metaFilePath;

        public Seeder(
            DataSetSeed seed,
            PublicDataDbContext dbContext,
            DuckDBConnection duckDb,
            IConsole console,
            CancellationToken cancellationToken)
        {
            _seed = seed;
            _dbContext = dbContext;
            _duckDb = duckDb;
            _console = console;
            _cancellationToken = cancellationToken;

            _dataFilePath = Path.Combine("SeedFiles", $"{_seed.Filename}.csv");
            _metaFilePath = Path.Combine("SeedFiles", $"{_seed.Filename}.meta.csv");

            if (!File.Exists(_dataFilePath))
            {
                throw new FileNotFoundException($"Could not find data file: '{_dataFilePath}'");
            }

            if (!File.Exists(_metaFilePath))
            {
                throw new FileNotFoundException($"Could not find meta file: '{_metaFilePath}'");
            }
        }

        public async Task Generate()
        {
            await _console.Output.WriteLineAsync($"Seeding '{_seed.DataSet.Title}' {_seed.DataSet.Id}");

            var stopwatch = Stopwatch.StartNew();

            stopwatch.Start();

            await _console.Output.WriteLineAsync("=> Started seeding meta");

            var columns = _duckDb.Query<(string ColumnName, string ColumnType)>(
                    $"DESCRIBE SELECT * FROM '{_dataFilePath}'"
                )
                .Select(row => row.ColumnName)
                .ToList();

            var allowedColumns = columns.ToHashSet();

            var metaFileRows = (await _duckDb.QueryAsync<MetaFileRow>(
                    new CommandDefinition(
                        $"SELECT * FROM '{_metaFilePath}'",
                        cancellationToken: _cancellationToken
                    )
                ))
                .ToList();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(_cancellationToken);

            await _dbContext.DataSets.AddAsync(_seed.DataSet, _cancellationToken);

            var dataSetVersion = await CreateDataSetVersion(metaFileRows, allowedColumns);
            await CreateDataSetMeta(metaFileRows, allowedColumns);

            _seed.DataSet.LatestVersion = dataSetVersion;

            await _dbContext.SaveChangesAsync(_cancellationToken);

            await transaction.CommitAsync(_cancellationToken);

            stopwatch.Stop();

            await _console.Output.WriteLineAsync(
                $"=> Finished seeding meta in {stopwatch.Elapsed.TotalSeconds} seconds"
            );

            stopwatch.Restart();

            await _console.Output.WriteLineAsync("=> Started seeding Parquet data");

            await SeedParquetData();

            stopwatch.Stop();

            await _console.Output.WriteLineAsync(
                $"=> Finished seeding Parquet data in {stopwatch.Elapsed.TotalSeconds} seconds"
            );
        }

        private async Task<DataSetVersion> CreateDataSetVersion(
            IList<MetaFileRow> metaFileRows,
            HashSet<string> allowedColumns)
        {
            var totalResults = await _duckDb.QuerySingleAsync<int>($"SELECT COUNT(*) FROM '{_dataFilePath}'");

            var geographicLevels = (await _duckDb.QueryAsync<string>(
                    new CommandDefinition(
                        $"""
                         SELECT DISTINCT geographic_level
                         FROM read_csv_auto('{_dataFilePath}', ALL_VARCHAR = true)
                         """,
                        cancellationToken: _cancellationToken
                    )
                ))
                .Select(EnumToEnumLabelConverter<GeographicLevel>.FromProvider)
                .ToList();

            var timePeriods = (await _duckDb.QueryAsync<(string TimePeriod, string TimeIdentifier)>(
                    new CommandDefinition(
                        $"""
                         SELECT DISTINCT time_period, time_identifier
                         FROM read_csv_auto('{_dataFilePath}', ALL_VARCHAR = true)
                         ORDER BY time_period
                         """,
                        cancellationToken: _cancellationToken
                    )
                ))
                .Select(
                    row => (
                        Period: TimePeriodFormatter.FormatFromCsv(row.TimePeriod),
                        Identifier: EnumToEnumLabelConverter<TimeIdentifier>.FromProvider(row.TimeIdentifier)
                    )
                )
                .OrderBy(tuple => tuple.Period)
                .ThenBy(tuple => tuple.Identifier)
                .ToList();

            var dataSetVersion = new DataSetVersion
            {
                Id = _seed.DataSetVersionId,
                VersionMajor = 1,
                VersionMinor = 0,
                Status = DataSetVersionStatus.Published,
                Notes = string.Empty,
                ParquetFilename = string.Empty,
                CsvFileId = Guid.NewGuid(),
                DataSetId = _seed.DataSet.Id,
                TotalResults = totalResults,
                MetaSummary = new DataSetVersionMetaSummary
                {
                    TimePeriodRange = new TimePeriodRange
                    {
                        Start = new TimePeriodRangeBound
                        {
                            Period = timePeriods[0].Period,
                            Code = timePeriods[0].Identifier
                        },
                        End = new TimePeriodRangeBound
                        {
                            Period = timePeriods[^1].Period,
                            Code = timePeriods[^1].Identifier
                        },
                    },
                    Filters = metaFileRows
                        .Where(row => row.ColType == MetaFileRow.ColumnType.Filter
                                      && allowedColumns.Contains(row.ColName))
                        .OrderBy(row => row.Label)
                        .Select(row => row.Label)
                        .ToList(),
                    Indicators = metaFileRows
                        .Where(
                            row => row.ColType == MetaFileRow.ColumnType.Indicator
                                   && allowedColumns.Contains(row.ColName)
                        )
                        .OrderBy(row => row.Label)
                        .Select(row => row.Label)
                        .ToList(),
                    GeographicLevels = geographicLevels
                },
                GeographicLevelMeta = new GeographicLevelMeta
                {
                    DataSetVersionId = _seed.DataSetVersionId,
                    Levels = geographicLevels
                },
                Published = _seed.DataSet.Published,
            };

            await _dbContext.DataSetVersions.AddAsync(dataSetVersion, _cancellationToken);
            await _dbContext.SaveChangesAsync(_cancellationToken);

            return dataSetVersion;
        }

        private async Task CreateDataSetMeta(
            IList<MetaFileRow> metaFileRows,
            IReadOnlySet<string> allowedColumns)
        {
            await CreateFilterMetas(metaFileRows, allowedColumns);
            await CreateLocationMetas(allowedColumns);
            await CreateIndicatorMeta(metaFileRows, allowedColumns);
            await CreateTimePeriodMeta();
        }

        private async Task CreateIndicatorMeta(
            IEnumerable<MetaFileRow> metaFileRows,
            IReadOnlySet<string> allowedColumns)
        {
            var metas = metaFileRows
                .Where(row => row.ColType == MetaFileRow.ColumnType.Indicator
                              && allowedColumns.Contains(row.ColName))
                .OrderBy(row => row.Label)
                .ToList()
                .Select(
                    row => new IndicatorMeta
                    {
                        DataSetVersionId = _seed.DataSetVersionId,
                        PublicId = row.ColName,
                        Label = row.Label,
                        Unit = row.IndicatorUnit,
                        DecimalPlaces = row.IndicatorDp
                    }
                )
                .ToList();

            _dbContext.IndicatorMetas.AddRange(metas);
            await _dbContext.SaveChangesAsync(_cancellationToken);
        }

        private async Task CreateFilterMetas(
            IEnumerable<MetaFileRow> metaFileRows,
            IReadOnlySet<string> allowedColumns)
        {
            var metas = metaFileRows
                .Where(
                    row => row.ColType == MetaFileRow.ColumnType.Filter
                           && allowedColumns.Contains(row.ColName)
                )
                .OrderBy(row => row.Label)
                .Select(
                    row => new FilterMeta
                    {
                        PublicId = row.ColName,
                        DataSetVersionId = _seed.DataSetVersionId,
                        Label = row.Label,
                        Hint = row.FilterHint ?? string.Empty,
                    }
                )
                .ToList();

            _dbContext.FilterMetas.AddRange(metas);
            await _dbContext.SaveChangesAsync(_cancellationToken);

            foreach (var meta in metas)
            {
                var options = (await _duckDb.QueryAsync<string>(
                        new CommandDefinition(
                            $"""
                             SELECT DISTINCT "{meta.PublicId}"
                             FROM read_csv_auto('{_dataFilePath}', ALL_VARCHAR = true) AS data
                             WHERE "{meta.PublicId}" != ''
                             ORDER BY "{meta.PublicId}"
                             """,
                            cancellationToken: _cancellationToken
                        )
                    ))
                    .Select(
                        label => new FilterOptionMeta
                        {
                            Label = label,
                            IsAggregate = label == "Total" ? true : null
                        }
                    )
                    .ToList();

                var optionTable = _dbContext.GetTable<FilterOptionMeta>();

                // Merge to only inserting new filter options
                // that don't already exist in the table.
                await optionTable
                    .Merge()
                    .Using(options)
                    .On(
                        o => new { o.Label, o.IsAggregate },
                        o => new { o.Label, o.IsAggregate }
                    )
                    .InsertWhenNotMatched()
                    .MergeAsync(_cancellationToken);

                var current = 0;
                const int batchSize = 1000;

                while (current < options.Count)
                {
                    var batch = options
                        .Skip(current)
                        .Take(batchSize)
                        .ToList();

                    // Although not necessary for filter options, we've adopted the 'row key'
                    // technique that was used for the location meta. This is more for
                    // future-proofing if we ever add more columns to the filter options table.
                    var batchRowKeys = batch
                        .Select(o => o.Label + ',' + (o.IsAggregate == true ? "True" : ""))
                        .ToHashSet();

                    var links = await optionTable
                        .Where(o =>
                            batchRowKeys.Contains(o.Label + ',' + (o.IsAggregate == true ? "True" : "" )))
                        .Select(option => new FilterOptionMetaLink
                        {
                            MetaId = meta.Id,
                            OptionId = option.Id
                        })
                        .ToListAsync(token: _cancellationToken);

                    _dbContext.FilterOptionMetaLinks.AddRange(links);
                    await _dbContext.SaveChangesAsync(_cancellationToken);

                    current += batchSize;
                }

                var insertedLinks = await _dbContext.FilterOptionMetaLinks
                    .CountAsync(l => l.MetaId == meta.Id,
                        cancellationToken: _cancellationToken);

                if (insertedLinks != options.Count)
                {
                    throw new InvalidOperationException(
                        $"Inserted incorrect number of filter option meta links for {meta.PublicId}. " +
                        $"Inserted: {insertedLinks}, expected: {options.Count}");
                }
            }
        }

        private async Task CreateLocationMetas(IReadOnlySet<string> allowedColumns)
        {
            var levels = ListLocationLevels(allowedColumns);

            var metas = levels
                .Select(level => new LocationMeta
                {
                    DataSetVersionId = _seed.DataSetVersionId,
                    Level = level,
                })
                .ToList();

            _dbContext.LocationMetas.AddRange(metas);
            await _dbContext.SaveChangesAsync(_cancellationToken);

            foreach (var meta in metas)
            {
                var nameCol = meta.Level.CsvNameColumn();
                var codeCols = meta.Level.CsvCodeColumns();
                string[] cols = [..codeCols, nameCol];

                var options = (await _duckDb.QueryAsync(
                        new CommandDefinition(
                            $"""
                             SELECT {cols.JoinToString(", ")}
                             FROM read_csv_auto('{_dataFilePath}', ALL_VARCHAR = true)
                             WHERE {cols.Select(col => $"{col} != ''").JoinToString(" AND ")}
                             GROUP BY {cols.JoinToString(", ")}
                             ORDER BY {cols.JoinToString(", ")}
                             """,
                            cancellationToken: _cancellationToken
                        )
                    ))
                    .Cast<IDictionary<string, object?>>()
                    .Select(row => row.ToDictionary(
                        kv => kv.Key,
                        kv => (string)kv.Value!
                    ))
                    .Select(row => MapLocationOptionMeta(row, meta.Level).ToRow())
                    .ToList();

                var optionTable = _dbContext
                    .GetTable<LocationOptionMetaRow>()
                    .TableName(nameof(PublicDataDbContext.LocationOptionMetas));

                await optionTable
                    .Merge()
                    .Using(options)
                    .On(
                        o => new { o.Type, o.Label, o.Code, o.OldCode, o.Urn, o.LaEstab, o.Ukprn },
                        o => new { o.Type, o.Label, o.Code, o.OldCode, o.Urn, o.LaEstab, o.Ukprn }
                    )
                    .InsertWhenNotMatched()
                    .MergeAsync(_cancellationToken);

                var current = 0;

                const int batchSize = 1000;

                while (current < options.Count)
                {
                    var batch = options
                        .Skip(current)
                        .Take(batchSize)
                        .ToList();

                    // We create a 'row key' for each option that allows us to quickly
                    // find the option rows that exist in the database. It's typically
                    // much slower to have multiple WHERE clauses for each row that check
                    // against every other row. Out of several such attempts, the 'row key'
                    // technique was the fastest and simplest way to create the links.
                    var batchRowKeys = batch
                        .Select(
                            o =>
                                o.Type + ',' +
                                o.Label + ',' +
                                (o.Code ?? "null") + ',' +
                                (o.OldCode ?? "null") + ',' +
                                (o.Urn ?? "null") + ',' +
                                (o.LaEstab ?? "null") + ',' +
                                (o.Ukprn ?? "null")
                        )
                        .ToHashSet();

                    var links = await optionTable
                        .Where(
                            o => o.Type == batch[0].Type &&
                                 batchRowKeys.Contains(
                                     o.Type + ',' +
                                     o.Label + ',' +
                                     (o.Code ?? "null") + ',' +
                                     (o.OldCode ?? "null") + ',' +
                                     (o.Urn ?? "null") + ',' +
                                     (o.LaEstab ?? "null") + ',' +
                                     (o.Ukprn ?? "null")
                                )
                        )
                        .Select(option => new LocationOptionMetaLink
                        {
                            MetaId = meta.Id,
                            OptionId = option.Id
                        })
                        .ToListAsync(token: _cancellationToken);

                    _dbContext.LocationOptionMetaLinks.AddRange(links);
                    await _dbContext.SaveChangesAsync(_cancellationToken);

                    current += batchSize;
                }

                var insertedLinks = await _dbContext.LocationOptionMetaLinks
                    .CountAsync(
                    l => l.MetaId == meta.Id,
                        cancellationToken: _cancellationToken);

                if (insertedLinks != options.Count)
                {
                    throw new InvalidOperationException(
                        $"Inserted incorrect number of location option meta links for {meta.Level}. " +
                        $"Inserted: {insertedLinks}, expected: {options.Count}"
                    );
                }
            }
        }

        private LocationOptionMeta MapLocationOptionMeta(
            IDictionary<string, string> row,
            GeographicLevel level)
        {
            var cols = level.CsvColumns();
            var label = row[level.CsvNameColumn()];

            return level switch
            {
                GeographicLevel.LocalAuthority => new LocationLocalAuthorityOptionMeta
                {
                    Label = label,
                    Code = row[LocalAuthorityCsvColumns.NewCode],
                    OldCode = row[LocalAuthorityCsvColumns.OldCode]
                },
                GeographicLevel.School => new LocationSchoolOptionMeta
                {
                    Label = label, Urn = row[SchoolCsvColumns.Urn],
                    LaEstab = row[SchoolCsvColumns.LaEstab]
                },
                GeographicLevel.Provider => new LocationProviderOptionMeta
                {
                    Label = label,
                    Ukprn = row[ProviderCsvColumns.Ukprn]
                },
                GeographicLevel.RscRegion => new LocationRscRegionOptionMeta
                {
                    Label = label
                },
                _ => new LocationCodedOptionMeta
                {
                    Label = label,
                    Code = row[cols.Codes.First()]
                }
            };
        }

        private List<GeographicLevel> ListLocationLevels(IReadOnlySet<string> allowedColumns)
        {
            return allowedColumns
                .Where(col => CsvColumnsToGeographicLevel.ContainsKey(col))
                .Select(col => CsvColumnsToGeographicLevel[col])
                .Distinct()
                .OrderBy(EnumToEnumLabelConverter<GeographicLevel>.ToProvider)
                .ToList();
        }

        private async Task CreateTimePeriodMeta()
        {
            var metas = (await _duckDb.QueryAsync<(string TimePeriod, string TimeIdentifier)>(
                    new CommandDefinition(
                        $"""
                         SELECT DISTINCT time_period, time_identifier
                         FROM read_csv_auto('{_dataFilePath}', ALL_VARCHAR = true)
                         ORDER BY time_period
                         """,
                        cancellationToken: _cancellationToken
                    )
                ))
                .Select(
                    tuple => new TimePeriodMeta
                    {
                        DataSetVersionId = _seed.DataSetVersionId,
                        Period = TimePeriodFormatter.FormatFromCsv(tuple.TimePeriod),
                        Code = EnumToEnumLabelConverter<TimeIdentifier>.FromProvider(tuple.TimeIdentifier)
                    }
                )
                .OrderBy(meta => meta.Period)
                .ThenBy(meta => meta.Code)
                .ToList();

            _dbContext.TimePeriodMetas.AddRange(metas);
            await _dbContext.SaveChangesAsync(_cancellationToken);
        }

        private async Task SeedParquetData()
        {
            var version = await _dbContext.DataSetVersions
                .AsSplitQuery()
                .Include(v => v.FilterMetas)
                .ThenInclude(m => m.Options)
                .Include(v => v.IndicatorMetas)
                .Include(v => v.LocationMetas)
                .ThenInclude(m => m.Options)
                .Include(v => v.TimePeriodMetas)
                .FirstAsync(v => v.Id == _seed.DataSetVersionId, cancellationToken: _cancellationToken);

            await _console.Output.WriteLineAsync($"=> Processing {version.TotalResults} rows");

            // Create temporary meta tables in DuckDB to allow us to do data transform
            // in DuckDB itself (i.e. changing all the filters and locations to normalised IDs).
            // Trying to transform the data via using Appender API in C# is slower
            // and seems to regularly cause DuckDB crashes for larger data sets.
            await CreateDuckDbMetaTables(version);

            await _duckDb.ExecuteAsync("CREATE SEQUENCE data_seq START 1");

            string[] columns =
            [
                "id UINTEGER PRIMARY KEY",
                "time_period_id INTEGER",
                "geographic_level VARCHAR",
                ..version.LocationMetas.Select(location => $"{GetDuckDbLocationTableName(location)}_id INTEGER"),
                ..version.FilterMetas.Select(filter => $"\"{filter.PublicId}\" INTEGER"),
                ..version.IndicatorMetas.Select(indicator => $"\"{indicator.PublicId}\" VARCHAR"),
            ];

            await _duckDb.ExecuteAsync($"CREATE TABLE data({columns.JoinToString(",\n")})");

            string[] insertColumns =
            [
                "nextval('data_seq') AS id",
                "time_periods.id AS time_period_id",
                "data_source.geographic_level",
                ..version.LocationMetas.Select(
                    location =>
                        $"{GetDuckDbLocationTableName(location)}.id AS {GetDuckDbLocationTableName(location)}_id"
                ),
                ..version.FilterMetas.Select(filter => $"\"{filter.PublicId}\".id AS \"{filter.PublicId}\""),
                ..version.IndicatorMetas.Select(indicator => $"\"{indicator.PublicId}\""),
            ];

            string[] insertJoins =
            [
                ..version.LocationMetas.Select(
                    location =>
                    {
                        var locationTable = GetDuckDbLocationTableName(location.Level);
                        var codeColumns = GetDuckDbLocationCodeColumns(location.Level);

                        string[] conditions =
                        [
                            ..codeColumns.Select(col => $"{locationTable}.{col.Name} = data_source.{col.CsvName}"),
                            $"{locationTable}.name = data_source.{location.Level.CsvNameColumn()}"
                        ];

                        return $"""
                                LEFT JOIN {locationTable}
                                ON {conditions.JoinToString(" AND ")}
                                """;
                    }
                ),
                ..version.FilterMetas.Select(
                    filter => $"""
                               LEFT JOIN {FiltersDuckDbTable} AS "{filter.PublicId}"
                               ON "{filter.PublicId}".column_name = '{filter.PublicId}'
                               AND "{filter.PublicId}".label = data_source."{filter.PublicId}"
                               """
                ),
                """
                JOIN time_periods ON time_periods.period = data_source.time_period 
                AND time_periods.identifier = data_source.time_identifier
                """
            ];

            await _duckDb.ExecuteAsync(
                new CommandDefinition(
                    $"""
                     INSERT INTO data
                     SELECT
                        {insertColumns.JoinToString(",\n")}
                     FROM read_csv_auto('{_dataFilePath}', ALL_VARCHAR = true) AS data_source
                     {insertJoins.JoinToString('\n')}
                     ORDER BY
                         data_source.geographic_level ASC,
                         data_source.time_period DESC
                     """,
                    cancellationToken: _cancellationToken
                )
            );

            await OutputParquetFiles();
        }

        private async Task OutputParquetFiles()
        {
            var projectRootPath = PathUtils.ProjectRootPath;
            var parquetDir = Path.Combine(projectRootPath, "data", "public-api-parquet");

            if (!Path.Exists(parquetDir))
            {
                Directory.CreateDirectory(parquetDir);
            }

            var dataSetDir = Path.Combine(parquetDir, _seed.DataSet.Id.ToString());

            if (Path.Exists(dataSetDir))
            {
                // Make sure data set directory is empty before exporting
                Directory.Delete(dataSetDir, recursive: true);

                // Deletion can be asynchronous (at OS level), meaning
                // we have to do this to make sure the directory has
                // actually been removed before proceeding...
                while (Directory.Exists(dataSetDir))
                {
                    Thread.Sleep(100);
                }
            }

            Directory.CreateDirectory(dataSetDir);

            var versionDir = Path.Combine(dataSetDir, "v1.0");

            Directory.CreateDirectory(versionDir);

            await _duckDb.ExecuteAsync(
                new CommandDefinition(
                    $"EXPORT DATABASE '{versionDir}' (FORMAT PARQUET, CODEC ZSTD)",
                    cancellationToken: _cancellationToken
                )
            );

            // Convert absolute paths in load.sql to relative paths otherwise
            // these refer to the machine that the script was ran on.

            var loadSqlFilePath = Path.Combine(versionDir, "load.sql");

            var newLines = (await File.ReadAllLinesAsync(loadSqlFilePath, _cancellationToken))
                .Select(line => line.Replace($"{versionDir}{Path.DirectorySeparatorChar}", ""));

            await File.WriteAllLinesAsync(loadSqlFilePath, newLines, _cancellationToken);
        }

        private async Task CreateDuckDbMetaTables(DataSetVersion version)
        {
            await CreateDuckDbLocationMetaTables(version);
            await CreateDuckDbFilterMetaTable(version);
            await CreateDuckDbTimePeriodMetaTable(version);
        }

        private async Task CreateDuckDbLocationMetaTables(DataSetVersion version)
        {
            foreach (var location in version.LocationMetas)
            {
                var locationTable = GetDuckDbLocationTableName(location.Level);

                string[] locationCols =
                [
                    "id INTEGER PRIMARY KEY",
                    "name VARCHAR",
                    "public_id INTEGER",
                    ..GetDuckDbLocationCodeColumns(location.Level).Select(col => $"{col.Name} VARCHAR")
                ];

                await _duckDb.ExecuteAsync(
                    $"""
                     CREATE TABLE {locationTable}(
                        {locationCols.JoinToString(",\n")}
                     )
                     """
                );

                using var appender = _duckDb.CreateAppender(table: locationTable);

                var insertRow = appender.CreateRow();
                var id = 1;

                foreach (var link in location.OptionLinks.OrderBy(l => l.Option.Label))
                {
                    var option = link.Option;

                    insertRow.AppendValue(id++);
                    insertRow.AppendValue(option.Label);
                    insertRow.AppendValue(link.PublicId);

                    switch (option)
                    {
                        case LocationLocalAuthorityOptionMeta laOption:
                            insertRow.AppendValue(laOption.Code);
                            insertRow.AppendValue(laOption.OldCode);
                            break;
                        case LocationCodedOptionMeta codedOption:
                            insertRow.AppendValue(codedOption.Code);
                            break;
                        case LocationProviderOptionMeta providerOption:
                            insertRow.AppendValue(providerOption.Ukprn);
                            break;
                        case LocationSchoolOptionMeta schoolOption:
                            insertRow.AppendValue(schoolOption.Urn);
                            insertRow.AppendValue(schoolOption.LaEstab);
                            break;
                    }

                    insertRow.EndRow();
                }
            }
        }

        private async Task CreateDuckDbFilterMetaTable(DataSetVersion version)
        {
            await _duckDb.ExecuteAsync(
                $"""
                 CREATE TABLE {FiltersDuckDbTable}(
                     id INTEGER PRIMARY KEY,
                     label VARCHAR,
                     public_id INTEGER,
                     column_name VARCHAR
                 )
                 """
            );

            var id = 1;

            foreach (var filter in version.FilterMetas)
            {
                using var appender = _duckDb.CreateAppender(table: FiltersDuckDbTable);

                foreach (var link in filter.OptionLinks.OrderBy(l => l.Option.Label))
                {
                    var insertRow = appender.CreateRow();

                    insertRow.AppendValue(id++);
                    insertRow.AppendValue(link.Option.Label);
                    insertRow.AppendValue(link.PublicId);
                    insertRow.AppendValue(filter.PublicId);

                    insertRow.EndRow();
                }
            }
        }

        private async Task CreateDuckDbTimePeriodMetaTable(DataSetVersion version)
        {
            await _duckDb.ExecuteAsync(
                $"""
                 CREATE TABLE {TimePeriodsDuckDbTable}(
                     id INTEGER PRIMARY KEY,
                     period VARCHAR,
                     identifier VARCHAR
                 )
                 """
            );

            using var appender = _duckDb.CreateAppender(table: TimePeriodsDuckDbTable);

            var timePeriods = version.TimePeriodMetas
                .OrderBy(tp => tp.Period)
                .ThenBy(tp => tp.Code);

            var id = 1;

            foreach (var timePeriod in timePeriods)
            {
                var insertRow = appender.CreateRow();

                insertRow.AppendValue(id++);
                insertRow.AppendValue(TimePeriodFormatter.FormatToCsv(timePeriod.Period));
                insertRow.AppendValue(timePeriod.Code.GetEnumLabel());
                insertRow.EndRow();
            }
        }

        private LocationColumn[] GetDuckDbLocationCodeColumns(GeographicLevel geographicLevel)
        {
            return geographicLevel switch
            {
                GeographicLevel.LocalAuthority =>
                [
                    new LocationColumn(Name: "code", CsvName: LocalAuthorityCsvColumns.NewCode),
                    new LocationColumn(Name: "old_code", CsvName: LocalAuthorityCsvColumns.OldCode)
                ],
                GeographicLevel.Provider =>
                [
                    new LocationColumn(Name: "ukprn", CsvName: ProviderCsvColumns.Ukprn)
                ],
                GeographicLevel.RscRegion => [],
                GeographicLevel.School =>
                [
                    new LocationColumn(Name: "urn", CsvName: SchoolCsvColumns.Urn),
                    new LocationColumn(Name: "laestab", CsvName: SchoolCsvColumns.LaEstab)
                ],
                _ =>
                [
                    new LocationColumn(Name: "code", CsvName: geographicLevel.CsvCodeColumns().First())
                ],
            };
        }

        private string GetDuckDbLocationTableName(LocationMeta locationMeta)
            => GetDuckDbLocationTableName(locationMeta.Level);

        private string GetDuckDbLocationTableName(GeographicLevel geographicLevel)
            => $"locations_{geographicLevel.GetEnumValue().ToLower()}";

        private record LocationColumn(string Name, string CsvName);
    }
}
