using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using CliWrap;
using Dapper;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Scripts.Models;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Scripts.Seeds;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using InterpolatedSql.Dapper;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.GeographicLevelUtils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Scripts.Commands;

[Command("seed:data", Description = "Generate seed data for the public API")]
public class SeedDataCommand : ICommand
{
    private const string DbConnectionString = "Host=db;Username=app_public_data_api;Password=password;Database=public_data";

    [CommandOption("dump-sql", Description = "Dump seed data SQL to the `data/public-api-db` directory")]
    public bool DumpSql { get; init; }

    public async ValueTask ExecuteAsync(IConsole console)
    {
        var cancellationToken = console.RegisterCancellationHandler();

        await using var dbContext = await SetUpPublicDataDbContext(console, cancellationToken);

        // Set current directory to the assembly's directory to simplify pathing
        Directory.SetCurrentDirectory(Assembly.GetExecutingAssembly().GetDirectoryPath());

        // Match CSV columns with underscores
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        var dataSetSeeds = new List<DataSetSeed>
        {
            DataSetSeed.AbsenceByCharacteristic2016,
            DataSetSeed.AbsenceRatesCharacteristic,
            DataSetSeed.AbsenceRatesGeographicLevel,
            DataSetSeed.AbsenceRatesGeographicLevelSchool,
            DataSetSeed.ExclusionsByGeographicLevel,
            DataSetSeed.SpcEthnicityLanguage,
            DataSetSeed.SpcYearGroupGender,
            DataSetSeed.Nat01,
        };

        var stopwatch = Stopwatch.StartNew();

        await console.Output.WriteLineAsync($"Started seeding data for {dataSetSeeds.Count} data sets");

        foreach (var seed in dataSetSeeds)
        {
            await using var duckDb = new DuckDbConnection();
            await duckDb.OpenAsync(cancellationToken);

            var seeder = new Seeder(
                seed: seed,
                dbContext: dbContext,
                duckDbConnection: duckDb,
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

    private static async Task<PublicDataDbContext> SetUpPublicDataDbContext(IConsole console,
        CancellationToken cancellationToken)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(DbConnectionString);

        var options = new DbContextOptionsBuilder<PublicDataDbContext>()
            .UseNpgsql(dataSourceBuilder.Build())
            .Options;

        var dbContext = new PublicDataDbContext(options);

        dbContext.Database.SetCommandTimeout(300);
        await console.Output.WriteLineAsync("Migrating database");
        await dbContext.Database.MigrateAsync(cancellationToken: cancellationToken);

        LinqToDBForEFTools.Initialize();

        // Clear any tables and restart any sequences in case we're re-running the command
        var tables = dbContext.Model.GetEntityTypes()
            .Select(type => type.GetTableName())
            .Distinct()
            .OfType<string>()
            .ToList();

        foreach (var table in tables)
        {
#pragma warning disable EF1002
            await dbContext.Database.ExecuteSqlRawAsync(
#pragma warning restore EF1002
                $"""TRUNCATE TABLE "{table}" RESTART IDENTITY CASCADE;""",
                cancellationToken: cancellationToken
            );
        }

        var sequences = dbContext.Model.GetSequences();

        foreach (var sequence in sequences)
        {
#pragma warning disable EF1002
            await dbContext.Database.ExecuteSqlRawAsync(
#pragma warning restore EF1002
                $"""ALTER SEQUENCE "{sequence.Name}" RESTART WITH 1;""",
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

        // The docker entrypoint executes initialisation scripts in name order.
        // This file is prefixed '01-' to be run in sequence after '00-init.sh'
        var sqlFilePath = Path.Combine(outputDir, "01-public_data.sql");

        await Cli.Wrap("pg_dump")
            .WithArguments([
                "--dbname", "public_data",
                "--file", sqlFilePath,
                "--schema", "public",
                "--username", "postgres",
                "--host", "db",
                "--port", "5432",
                "--clean",
                "--if-exists",
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
        private readonly DataSetSeed _seed;
        private readonly PublicDataDbContext _dbContext;
        private readonly IDuckDbConnection _duckDbConnection;
        private readonly IConsole _console;
        private readonly CancellationToken _cancellationToken;

        private readonly string _dataFilePath;
        private readonly string _metaFilePath;

        public Seeder(
            DataSetSeed seed,
            PublicDataDbContext dbContext,
            IDuckDbConnection duckDbConnection,
            IConsole console,
            CancellationToken cancellationToken)
        {
            _seed = seed;
            _dbContext = dbContext;
            _duckDbConnection = duckDbConnection;
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

            var columns = _duckDbConnection.Query<(string ColumnName, string ColumnType)>(
                    $"DESCRIBE SELECT * FROM '{_dataFilePath}'"
                )
                .Select(row => row.ColumnName)
                .ToList();

            var allowedColumns = columns.ToHashSet();

            var metaFileRows = (await _duckDbConnection.SqlBuilder(
                    $"SELECT * FROM '{_metaFilePath:raw}'")
                .QueryAsync<MetaFileRow>(cancellationToken: _cancellationToken)).AsList();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(_cancellationToken);

            await _dbContext.DataSets.AddAsync(_seed.DataSet, _cancellationToken);

            var dataSetVersion = await CreateDataSetVersion(metaFileRows, allowedColumns);
            await CreateDataSetMeta(metaFileRows, allowedColumns);

            _seed.DataSet.LatestLiveVersion = dataSetVersion;

            await _dbContext.SaveChangesAsync(_cancellationToken);

            await transaction.CommitAsync(_cancellationToken);

            stopwatch.Stop();

            await _console.Output.WriteLineAsync(
                $"=> Finished seeding meta in {stopwatch.Elapsed.TotalSeconds} seconds"
            );

            stopwatch.Restart();

            await _console.Output.WriteLineAsync("=> Started seeding Parquet data");

            await SeedParquetData();
            await OutputFiles();

            stopwatch.Stop();

            await _console.Output.WriteLineAsync(
                $"=> Finished seeding Parquet data in {stopwatch.Elapsed.TotalSeconds} seconds"
            );
        }

        private async Task<DataSetVersion> CreateDataSetVersion(
            IList<MetaFileRow> metaFileRows,
            HashSet<string> allowedColumns)
        {
            var totalResults = await _duckDbConnection.SqlBuilder(
                $"""
                 SELECT COUNT(*)
                 FROM '{_dataFilePath:raw}'
                 """
            ).QuerySingleAsync<int>(cancellationToken: _cancellationToken);

            var geographicLevels =
                (await _duckDbConnection.SqlBuilder(
                    $"""
                     SELECT DISTINCT geographic_level
                     FROM read_csv('{_dataFilePath:raw}', ALL_VARCHAR = true)
                     """
                ).QueryAsync<string>(cancellationToken: _cancellationToken))
                .Select(EnumToEnumLabelConverter<GeographicLevel>.FromProvider)
                .OrderBy(EnumToEnumLabelConverter<GeographicLevel>.ToProvider)
                .ToList();

            var timePeriods = (await _duckDbConnection.SqlBuilder(
                        $"""
                         SELECT DISTINCT time_period, time_identifier
                         FROM read_csv('{_dataFilePath:raw}', ALL_VARCHAR = true)
                         ORDER BY time_period
                         """
                    ).QueryAsync<(string TimePeriod, string TimeIdentifier)>(cancellationToken: _cancellationToken))
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
                Release = _seed.Release,
                Notes = string.Empty,
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
            var currentId = await _dbContext.NextSequenceValue(
                PublicDataDbContext.IndicatorMetasIdSequence,
                _cancellationToken);

            var metas = metaFileRows
                .Where(row => row.ColType == MetaFileRow.ColumnType.Indicator
                              && allowedColumns.Contains(row.ColName))
                .OrderBy(row => row.Label)
                .ToList()
                .Select(row =>
                {
                    var id = currentId++;

                    return new IndicatorMeta
                    {
                        Id = id,
                        DataSetVersionId = _seed.DataSetVersionId,
                        PublicId = SqidEncoder.Encode(id),
                        Column = row.ColName,
                        Label = row.Label,
                        Unit = row.ParsedIndicatorUnit,
                        DecimalPlaces = row.IndicatorDp
                    };
                })
                .ToList();

            _dbContext.IndicatorMetas.AddRange(metas);
            await _dbContext.SaveChangesAsync(_cancellationToken);

            await _dbContext.SetSequenceValue(
                PublicDataDbContext.IndicatorMetasIdSequence,
                currentId - 1,
                _cancellationToken
            );
        }

        private async Task CreateFilterMetas(
            IEnumerable<MetaFileRow> metaFileRows,
            IReadOnlySet<string> allowedColumns)
        {
            var filterMetaRows = metaFileRows
                .Where(
                    row => row.ColType == MetaFileRow.ColumnType.Filter
                           && allowedColumns.Contains(row.ColName)
                )
                .OrderBy(row => row.Label);

            foreach (var metaRow in filterMetaRows)
            {
                var metaId = await _dbContext.NextSequenceValue(
                    PublicDataDbContext.FilterMetasIdSequence,
                    _cancellationToken);

                var meta = new FilterMeta
                {
                    Id = metaId,
                    PublicId = SqidEncoder.Encode(metaId),
                    Column = metaRow.ColName,
                    DataSetVersionId = _seed.DataSetVersionId,
                    Label = metaRow.Label,
                    Hint = metaRow.FilterHint ?? string.Empty,
                };

                _dbContext.FilterMetas.Add(meta);
                await _dbContext.SaveChangesAsync(_cancellationToken);

                var options = (await _duckDbConnection.SqlBuilder(
                            $"""
                             SELECT DISTINCT "{meta.Column:raw}"
                             FROM read_csv('{_dataFilePath:raw}', ALL_VARCHAR = true) AS data
                             WHERE "{meta.Column:raw}" != ''
                             ORDER BY "{meta.Column:raw}"
                             """
                        ).QueryAsync<string>(cancellationToken: _cancellationToken)
                    )
                    .Select(
                        label => new FilterOptionMeta
                        {
                            Label = label,
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
                        o =>  o.Label,
                        o =>  o.Label
                    )
                    .InsertWhenNotMatched()
                    .MergeAsync(_cancellationToken);

                var startIndex = await _dbContext.NextSequenceValue(
                    PublicDataDbContext.FilterOptionMetaLinkSequence,
                    _cancellationToken
                );

                var current = 0;
                const int batchSize = 1000;

                while (current < options.Count)
                {
                    var batchStartIndex = startIndex + current;
                    var batch = options
                        .Skip(current)
                        .Take(batchSize)
                        .ToList();

                    // Although not necessary for filter options, we've adopted the 'row key'
                    // technique that was used for the location meta. This is more for
                    // future-proofing if we ever add more columns to the filter options table.
                    var batchRowKeys = batch
                        .Select(o => o.Label)
                        .ToHashSet();

                    var links = await optionTable
                        .Where(o => batchRowKeys.Contains(o.Label))
                        .OrderBy(o => o.Label)
                        .Select((option, index) => new FilterOptionMetaLink
                        {
                            PublicId = SqidEncoder.Encode(batchStartIndex + index),
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
                        $"Inserted incorrect number of filter option meta links for filter (id = {meta.Id}, column = {meta.Column}). " +
                        $"Inserted: {insertedLinks}, expected: {options.Count}");
                }

                await _dbContext.SetSequenceValue(
                    PublicDataDbContext.FilterOptionMetaLinkSequence,
                    startIndex + insertedLinks - 1,
                    _cancellationToken
                );

                var defaultOption = _dbContext.FilterOptionMetaLinks
                    .AsNoTracking()
                    .Include(l => l.Option)
                    .Where(l => l.MetaId == meta.Id)
                    .Select(l => l.Option)
                    .FirstOrDefault(option => metaRow.FilterDefault != null
                        ? option.Label == metaRow.FilterDefault
                        : option.Label == "Total");

                if (defaultOption is not null)
                {
                    await _dbContext.FilterMetas
                        .AsNoTracking()
                        .Where(fm => fm.Id == meta.Id)
                        .ExecuteUpdateAsync(setters => setters
                                .SetProperty(fm => fm.DefaultOptionId, defaultOption.Id),
                            cancellationToken: _cancellationToken);
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
                string[] cols = [.. codeCols, nameCol];

                var options = (await _duckDbConnection.SqlBuilder(
                            $"""
                             SELECT {cols.JoinToString(", "):raw}
                             FROM read_csv('{_dataFilePath:raw}', ALL_VARCHAR = true)
                             WHERE {cols.Select(col => $"{col} != ''").JoinToString(" AND "):raw}
                             GROUP BY {cols.JoinToString(", "):raw}
                             ORDER BY {cols.JoinToString(", "):raw}
                             """
                        ).QueryAsync(cancellationToken: _cancellationToken))
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
                        .Select(o => o.GetRowKey())
                        .ToHashSet();

                    Expression<Func<LocationOptionMetaRow, bool>> hasBatchRowKey =
                        o => o.Type == batch[0].Type &&
                             batchRowKeys.Contains(
                                 o.Type + ',' +
                                 o.Label + ',' +
                                 (o.Code ?? "null") + ',' +
                                 (o.OldCode ?? "null") + ',' +
                                 (o.Urn ?? "null") + ',' +
                                 (o.LaEstab ?? "null") + ',' +
                                 (o.Ukprn ?? "null")
                             );

                    var existingRowKeys = (await optionTable
                            .Where(hasBatchRowKey)
                            .ToListAsync(token: _cancellationToken))
                        .Select(o => o.GetRowKey())
                        .ToHashSet();

                    if (existingRowKeys.Count != batch.Count)
                    {
                        var newOptions = batch
                            .Where(o => !existingRowKeys.Contains(o.GetRowKey()))
                            .ToList();

                        var startIndex = await _dbContext.NextSequenceValue(
                            PublicDataDbContext.LocationOptionMetasIdSequence,
                            _cancellationToken
                        );

                        foreach (var option in newOptions)
                        {
                            option.Id = startIndex++;
                        }

                        await optionTable.BulkCopyAsync(
                            new BulkCopyOptions { KeepIdentity = true },
                            newOptions,
                            cancellationToken: _cancellationToken);

                        await _dbContext.SetSequenceValue(
                            PublicDataDbContext.LocationOptionMetasIdSequence,
                            startIndex - 1,
                            _cancellationToken
                        );
                    }

                    var links = await optionTable
                        .Where(hasBatchRowKey)
                        .Select((option, index) => new LocationOptionMetaLink
                        {
                            PublicId = SqidEncoder.Encode(option.Id),
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

        private static LocationOptionMeta MapLocationOptionMeta(
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
                    Label = label,
                    Urn = row[SchoolCsvColumns.Urn],
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

        private static List<GeographicLevel> ListLocationLevels(IReadOnlySet<string> allowedColumns)
        {
            return allowedColumns
                .Where(CsvColumnsToGeographicLevel.ContainsKey)
                .Select(col => CsvColumnsToGeographicLevel[col])
                .Distinct()
                .OrderBy(EnumToEnumLabelConverter<GeographicLevel>.ToProvider)
                .ToList();
        }

        private async Task CreateTimePeriodMeta()
        {
            var metas = (await _duckDbConnection.SqlBuilder(
                    $"""
                     SELECT DISTINCT time_period, time_identifier
                     FROM read_csv('{_dataFilePath:raw}', ALL_VARCHAR = true)
                     ORDER BY time_period
                     """
                ).QueryAsync<(string TimePeriod, string TimeIdentifier)>(cancellationToken: _cancellationToken))
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
            await CreateParquetMetaTables(version);

            await _duckDbConnection.SqlBuilder("CREATE SEQUENCE data_seq START 1")
                .ExecuteAsync(cancellationToken: _cancellationToken);

            string[] columns =
            [
                $"{DataTable.Cols.Id} UINTEGER NOT NULL PRIMARY KEY",
                $"{DataTable.Cols.TimePeriodId} INTEGER NOT NULL",
                $"{DataTable.Cols.GeographicLevel} VARCHAR NOT NULL",
                ..version.LocationMetas.Select(location => $"{DataTable.Cols.LocationId(location)} INTEGER NOT NULL"),
                ..version.FilterMetas.Select(filter => $"{DataTable.Cols.Filter(filter)} INTEGER NOT NULL"),
                ..version.IndicatorMetas.Select(indicator => $"{DataTable.Cols.Indicator(indicator)} VARCHAR NOT NULL"),
            ];

            await _duckDbConnection.SqlBuilder(
                    $"""
                     CREATE TABLE {DataTable.TableName:raw}
                     ({columns.JoinToString(",\n"):raw})
                     """)
                .ExecuteAsync(cancellationToken: _cancellationToken);

            string[] insertColumns =
            [
                "nextval('data_seq') AS id",
                $"{TimePeriodsTable.Ref().Id} AS {DataTable.Cols.TimePeriodId}",
                DataSourceTable.Ref.GeographicLevel,
                ..version.LocationMetas.Select(location =>
                    $"COALESCE({LocationOptionsTable.Ref(location).Id}, 0) AS {DataTable.Cols.LocationId(location)}"),
                ..version.FilterMetas.Select(filter =>
                    $"COALESCE({FilterOptionsTable.Ref(filter).Id}, 0) AS {DataTable.Cols.Filter(filter)}"),
                ..version.IndicatorMetas.Select(DataTable.Cols.Indicator),
            ];

            string[] insertJoins =
            [
                ..version.LocationMetas.Select(
                    location =>
                    {
                        var codeColumns = GetParquetLocationCodeColumns(location.Level);

                        string[] conditions =
                        [
                            ..codeColumns.Select(col =>
                                $"{LocationOptionsTable.Ref(location).Col(col.Name)} = {DataSourceTable.Ref.Col(col.CsvName)}"),
                            $"{LocationOptionsTable.Ref(location).Label} = {DataSourceTable.Ref.Col(location.Level.CsvNameColumn())}"
                        ];

                        return $"""
                                LEFT JOIN {LocationOptionsTable.TableName} AS {LocationOptionsTable.Alias(location)}
                                ON {conditions.JoinToString(" AND ")}
                                """;
                    }
                ),
                ..version.FilterMetas.Select(
                    filter => $"""
                               LEFT JOIN {FilterOptionsTable.TableName} AS {FilterOptionsTable.Alias(filter)}
                               ON {FilterOptionsTable.Ref(filter).FilterColumn} = '{filter.Column}'
                               AND {FilterOptionsTable.Ref(filter).Label} = {DataSourceTable.Ref.Col(filter.Column)}
                               """
                ),
                $"""
                 JOIN {TimePeriodsTable.TableName}
                 ON {TimePeriodsTable.Ref().Period} = {DataSourceTable.Ref.TimePeriod}
                 AND {TimePeriodsTable.Ref().Identifier} = {DataSourceTable.Ref.TimeIdentifier}
                 """
            ];

            await _duckDbConnection.SqlBuilder(
                $"""
                 INSERT INTO {DataTable.TableName:raw}
                 SELECT
                 {insertColumns.JoinToString(",\n"):raw}
                 FROM read_csv('{_dataFilePath:raw}', ALL_VARCHAR = true) AS {DataSourceTable.TableName:raw}
                 {insertJoins.JoinToString('\n'):raw}
                 ORDER BY
                 {DataSourceTable.Ref.GeographicLevel:raw} ASC,
                 {DataSourceTable.Ref.TimePeriod:raw} DESC
                 """
            ).ExecuteAsync(cancellationToken: _cancellationToken);
        }

        private async Task CreateParquetMetaTables(DataSetVersion version)
        {
            await CreateParquetIndicatorsTable(version);
            await CreateParquetLocationOptionsTable(version);
            await CreateParquetFilterOptionsTable(version);
            await CreateParquetTimePeriodsTable(version);
        }

        private async Task CreateParquetIndicatorsTable(DataSetVersion version)
        {
            await _duckDbConnection.SqlBuilder(
                $"""
                 CREATE TABLE {IndicatorsTable.TableName:raw}(
                     {IndicatorsTable.Cols.Id:raw} VARCHAR PRIMARY KEY,
                     {IndicatorsTable.Cols.Column:raw} VARCHAR,
                     {IndicatorsTable.Cols.Label:raw} VARCHAR,
                     {IndicatorsTable.Cols.Unit:raw} VARCHAR,
                     {IndicatorsTable.Cols.DecimalPlaces:raw} TINYINT,
                 )
                 """
            ).ExecuteAsync(cancellationToken: _cancellationToken);

            using var appender = _duckDbConnection.CreateAppender(table: IndicatorsTable.TableName);

            foreach (var meta in version.IndicatorMetas)
            {
                var insertRow = appender.CreateRow();

                insertRow.AppendValue(meta.PublicId);
                insertRow.AppendValue(meta.Column);
                insertRow.AppendValue(meta.Label);
                insertRow.AppendValue(meta.Unit?.GetEnumLabel() ?? string.Empty);
                insertRow.AppendValue(meta.DecimalPlaces);
                insertRow.EndRow();
            }
        }

        private async Task CreateParquetLocationOptionsTable(DataSetVersion version)
        {
            await _duckDbConnection.SqlBuilder(
                $"""
                 CREATE TABLE {LocationOptionsTable.TableName:raw}(
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
            ).ExecuteAsync(cancellationToken: _cancellationToken);

            var id = 1;

            foreach (var location in version.LocationMetas)
            {
                using var appender = _duckDbConnection.CreateAppender(table: LocationOptionsTable.TableName);

                foreach (var link in location.OptionLinks.OrderBy(l => l.Option.Label))
                {
                    var option = link.Option;

                    var insertRow = appender.CreateRow();

                    insertRow.AppendValue(id++);
                    insertRow.AppendValue(option.Label);
                    insertRow.AppendValue(location.Level.GetEnumValue());
                    insertRow.AppendValue(link.PublicId);

                    switch (option)
                    {
                        case LocationLocalAuthorityOptionMeta laOption:
                            insertRow.AppendValue(laOption.Code);
                            insertRow.AppendValue(laOption.OldCode);
                            insertRow.AppendNullValue();
                            insertRow.AppendNullValue();
                            insertRow.AppendNullValue();
                            break;
                        case LocationCodedOptionMeta codedOption:
                            insertRow.AppendValue(codedOption.Code);
                            insertRow.AppendNullValue();
                            insertRow.AppendNullValue();
                            insertRow.AppendNullValue();
                            insertRow.AppendNullValue();
                            break;
                        case LocationProviderOptionMeta providerOption:
                            insertRow.AppendNullValue();
                            insertRow.AppendNullValue();
                            insertRow.AppendNullValue();
                            insertRow.AppendNullValue();
                            insertRow.AppendValue(providerOption.Ukprn);
                            break;
                        case LocationSchoolOptionMeta schoolOption:
                            insertRow.AppendNullValue();
                            insertRow.AppendNullValue();
                            insertRow.AppendValue(schoolOption.Urn);
                            insertRow.AppendValue(schoolOption.LaEstab);
                            insertRow.AppendNullValue();
                            break;
                    }

                    insertRow.EndRow();
                }
            }
        }

        private async Task CreateParquetFilterOptionsTable(DataSetVersion version)
        {
            await _duckDbConnection.SqlBuilder(
                $"""
                 CREATE TABLE {FilterOptionsTable.TableName:raw}(
                     {FilterOptionsTable.Cols.Id:raw} INTEGER PRIMARY KEY,
                     {FilterOptionsTable.Cols.Label:raw} VARCHAR,
                     {FilterOptionsTable.Cols.PublicId:raw} VARCHAR,
                     {FilterOptionsTable.Cols.FilterId:raw} VARCHAR,
                     {FilterOptionsTable.Cols.FilterColumn:raw} VARCHAR
                 )
                 """
            ).ExecuteAsync(cancellationToken: _cancellationToken);

            var id = 1;

            foreach (var filter in version.FilterMetas)
            {
                using var appender = _duckDbConnection.CreateAppender(table: FilterOptionsTable.TableName);

                foreach (var link in filter.OptionLinks.OrderBy(l => l.Option.Label))
                {
                    var insertRow = appender.CreateRow();

                    insertRow.AppendValue(id++);
                    insertRow.AppendValue(link.Option.Label);
                    insertRow.AppendValue(link.PublicId);
                    insertRow.AppendValue(filter.PublicId);
                    insertRow.AppendValue(filter.Column);

                    insertRow.EndRow();
                }
            }
        }

        private async Task CreateParquetTimePeriodsTable(DataSetVersion version)
        {
            await _duckDbConnection.SqlBuilder(
                $"""
                 CREATE TABLE {TimePeriodsTable.TableName:raw}(
                     {TimePeriodsTable.Cols.Id:raw} INTEGER PRIMARY KEY,
                     {TimePeriodsTable.Cols.Period:raw} VARCHAR,
                     {TimePeriodsTable.Cols.Identifier:raw} VARCHAR
                 )
                 """
            ).ExecuteAsync(cancellationToken: _cancellationToken);

            using var appender = _duckDbConnection.CreateAppender(table: TimePeriodsTable.TableName);

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

        private LocationColumn[] GetParquetLocationCodeColumns(GeographicLevel geographicLevel)
        {
            return geographicLevel switch
            {
                GeographicLevel.LocalAuthority =>
                [
                    new LocationColumn(Name: LocationOptionsTable.Cols.Code, CsvName: LocalAuthorityCsvColumns.NewCode),
                    new LocationColumn(Name: LocationOptionsTable.Cols.OldCode,
                        CsvName: LocalAuthorityCsvColumns.OldCode)
                ],
                GeographicLevel.Provider =>
                [
                    new LocationColumn(Name: LocationOptionsTable.Cols.Ukprn, CsvName: ProviderCsvColumns.Ukprn)
                ],
                GeographicLevel.RscRegion => [],
                GeographicLevel.School =>
                [
                    new LocationColumn(Name: LocationOptionsTable.Cols.Urn, CsvName: SchoolCsvColumns.Urn),
                    new LocationColumn(Name: LocationOptionsTable.Cols.LaEstab, CsvName: SchoolCsvColumns.LaEstab)
                ],
                _ =>
                [
                    new LocationColumn(Name: LocationOptionsTable.Cols.Code,
                        CsvName: geographicLevel.CsvCodeColumns().First())
                ],
            };
        }

        private async Task OutputFiles()
        {
            var versionDir = CreateOutputDirectory();

            await OutputParquetFiles(versionDir);

            await OutputCompressedDataFile(versionDir);
        }

        private string CreateOutputDirectory()
        {
            var projectRootPath = PathUtils.ProjectRootPath;
            var dataDir = Path.Combine(projectRootPath, "data", "public-api-data", DataSetFilenames.DataSetsDirectory);

            if (!Path.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }

            var dataSetDir = Path.Combine(dataDir, _seed.DataSet.Id.ToString());

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

            var versionDir = Path.Combine(dataSetDir, "v1.0.0");

            Directory.CreateDirectory(versionDir);

            return versionDir;
        }

        private async Task OutputParquetFiles(string dataSetVersionDirectory)
        {
            await _duckDbConnection.SqlBuilder(
                    $"EXPORT DATABASE '{dataSetVersionDirectory:raw}' (FORMAT PARQUET, CODEC ZSTD)")
                .ExecuteAsync(cancellationToken: _cancellationToken);

            // Convert absolute paths in load.sql to relative paths otherwise
            // these refer to the machine that the script was run on.

            var loadSqlFilePath = Path.Combine(dataSetVersionDirectory, DataSetFilenames.DuckDbLoadSqlFile);

            var absolutePathToReplace = $"{dataSetVersionDirectory.Replace('\\', '/')}/";

            var newLines = (await File.ReadAllLinesAsync(loadSqlFilePath, _cancellationToken))
                .Select(line => line.Replace(absolutePathToReplace, ""));

            await File.WriteAllLinesAsync(loadSqlFilePath, newLines, _cancellationToken);
        }

        private async Task OutputCompressedDataFile(string versionDir)
        {
            var sourceStream = new FileStream(_dataFilePath, FileMode.Open);

            var destinationPath = Path.Combine(versionDir, DataSetFilenames.CsvDataFile);
            var targetStream = new FileStream(destinationPath, FileMode.Create);

            await CompressionUtils.CompressToStream(
                sourceStream,
                targetStream,
                ContentEncodings.Gzip,
                _cancellationToken);
        }

        private record LocationColumn(string Name, string CsvName);
    }

    private static class DataSourceTable
    {
        public const string TableName = "data_source";

        public static readonly TableRef Ref = new(TableName);

        public class TableRef(string table)
        {
            public readonly string TimePeriod = $"{table}.time_period";
            public readonly string TimeIdentifier = $"{table}.time_identifier";
            public readonly string GeographicLevel = $"{table}.geographic_level";

            public string Col(string column) => $"{table}.\"{column}\"";
        }
    }
}
