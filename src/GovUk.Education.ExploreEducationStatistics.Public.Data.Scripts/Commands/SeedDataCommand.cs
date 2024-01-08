using System.Diagnostics;
using System.Reflection;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
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
using GovUk.Education.ExploreEducationStatistics.Public.Data.Scripts.Utils;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Scripts.Commands;

[Command("seed:data", Description = "Generate seed data for the public API database")]
public class SeedDataCommand : ICommand
{
    private const string DbConnectionString = "Host=db;Username=postgres;Password=password;Database=public_data";

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

        await console.Output.WriteLineAsync($"Done! Finished all seeding in {stopwatch.Elapsed.TotalSeconds} seconds!");
    }

    private static async Task<PublicDataDbContext> SetUpPublicDataDbContext(CancellationToken cancellationToken)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(DbConnectionString);
        dataSourceBuilder.MapEnum<GeographicLevel>();

        var options = new DbContextOptionsBuilder<PublicDataDbContext>()
            .UseNpgsql(dataSourceBuilder.Build())
            .Options;

        var dbContext = new PublicDataDbContext(options);

        dbContext.Database.SetCommandTimeout(300);
        await dbContext.Database.MigrateAsync(cancellationToken: cancellationToken);

        var tables = dbContext.Model.GetEntityTypes()
            .Select(type => type.GetTableName())
            .Distinct()
            .Cast<string>()
            .ToList();

        // Clear any tables in case we're re-running the command
        foreach (var table in tables)
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                $"""TRUNCATE TABLE "{table}" RESTART IDENTITY CASCADE;""",
                cancellationToken: cancellationToken);
        }

        return dbContext;
    }

    private class Seeder
    {
        private static int _idSeedNumber;

        private readonly DataSetSeed _seed;
        private readonly PublicDataDbContext _dbContext;
        private readonly DuckDBConnection _duckDb;
        private readonly IConsole _console;
        private readonly CancellationToken _cancellationToken;

        private readonly ShortId _shortId;
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

            _idSeedNumber += 1;
            _shortId = new ShortId(_idSeedNumber, checkCollisions: true);

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
            var dataSetMeta = await CreateDataSetMeta(metaFileRows, allowedColumns);

            await transaction.CommitAsync(_cancellationToken);

            stopwatch.Stop();

            await _console.Output.WriteLineAsync(
                $"=> Finished seeding meta in {stopwatch.Elapsed.TotalSeconds} seconds"
            );

            stopwatch.Restart();

            await _console.Output.WriteLineAsync("=> Started seeding Parquet data");

            await SeedParquetData(dataSetMeta, dataSetVersion.TotalResults);

            stopwatch.Stop();

            await _console.Output.WriteLineAsync($"=> Finished seeding Parquet data in {stopwatch.Elapsed.TotalSeconds} seconds");
        }

        private async Task<DataSetVersion> CreateDataSetVersion(
            IList<MetaFileRow> metaFileRows,
            HashSet<string> allowedColumns)
        {
            var totalResults = await _duckDb.QuerySingleAsync<int>($"SELECT COUNT(*) FROM '{_dataFilePath}'");

            var timePeriods = (await _duckDb.QueryAsync<(int TimePeriod, string TimeIdentifier)>(
                    new CommandDefinition(
                        $"""
                         SELECT DISTINCT time_period, time_identifier
                         FROM '{_dataFilePath}'
                         ORDER BY time_period
                         """,
                        cancellationToken: _cancellationToken
                    )
                ))
                .Select(row => (
                    Year: row.TimePeriod,
                    TimeIdentifier: EnumToEnumLabelConverter<TimeIdentifier>.FromProvider(row.TimeIdentifier)
                ))
                .OrderBy(tuple => tuple.Year)
                .ThenBy(tuple => tuple.TimeIdentifier)
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
                        Start = new TimePeriodMeta
                        {
                            Year = timePeriods[0].Year,
                            Code = timePeriods[0].TimeIdentifier
                        },
                        End = new TimePeriodMeta
                        {
                            Year = timePeriods[^1].Year,
                            Code = timePeriods[^1].TimeIdentifier
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
                    GeographicLevels = ListGeographicLevels(allowedColumns)
                },
                Published = _seed.DataSet.Published,
            };

            await _dbContext.DataSetVersions.AddAsync(dataSetVersion, _cancellationToken);
            await _dbContext.SaveChangesAsync(_cancellationToken);

            return dataSetVersion;
        }

        private async Task<DataSetMeta> CreateDataSetMeta(
            IList<MetaFileRow> metaFileRows,
            IReadOnlySet<string> allowedColumns)
        {
            var geographicLevels = ListGeographicLevels(allowedColumns);

            var dataSetMeta = new DataSetMeta
            {
                Id = _seed.DataSetMetaId,
                DataSetVersionId = _seed.DataSetVersionId,
                Filters = await ListFilterMeta(metaFileRows, allowedColumns),
                Indicators = ListIndicatorMeta(metaFileRows, allowedColumns),
                TimePeriods = await ListTimePeriodMeta(),
                Locations = await ListLocationMeta(geographicLevels),
                GeographicLevels = geographicLevels
            };

            await _dbContext.DataSetMeta.AddAsync(dataSetMeta, _cancellationToken);
            await _dbContext.SaveChangesAsync(_cancellationToken);

            return dataSetMeta;
        }

        private List<IndicatorMeta> ListIndicatorMeta(
            IEnumerable<MetaFileRow> metaFileRows,
            IReadOnlySet<string> allowedColumns)
        {
            return metaFileRows
                .Where(row => row.ColType == MetaFileRow.ColumnType.Indicator
                              && allowedColumns.Contains(row.ColName))
                .OrderBy(row => row.Label)
                .Select(
                    row => new IndicatorMeta
                    {
                        Identifier = row.ColName,
                        Label = row.Label,
                        Unit = row.IndicatorUnit,
                        DecimalPlaces = row.IndicatorDp
                    }
                )
                .ToList();
        }

        private async Task<List<FilterMeta>> ListFilterMeta(
            IEnumerable<MetaFileRow> metaFileRows,
            IReadOnlySet<string> allowedColumns)
        {
            return await metaFileRows
                .Where(row => row.ColType == MetaFileRow.ColumnType.Filter
                              && allowedColumns.Contains(row.ColName))
                .OrderBy(row => row.Label)
                .ToAsyncEnumerable()
                .SelectAwait(async row =>
                {
                    var options = (await _duckDb.QueryAsync<string>(
                            new CommandDefinition(
                                $"""
                                 SELECT DISTINCT "{row.ColName}"
                                 FROM '{_dataFilePath}' AS data
                                 WHERE "{row.ColName}" != ''
                                 ORDER BY "{row.ColName}"
                                 """,
                                cancellationToken: _cancellationToken
                            )
                        ))
                        .Select(
                            (label, index) => new FilterOptionMeta
                            {
                                PublicId = _shortId.Generate(),
                                PrivateId = index + 1,
                                Label = label
                            }
                        )
                        .ToList();

                    return new FilterMeta
                    {
                        Identifier = row.ColName,
                        Label = row.Label,
                        Hint = row.FilterHint ?? string.Empty,
                        Options = options
                    };
                })
                .ToListAsync(_cancellationToken);
        }

        private async Task<List<LocationMeta>> ListLocationMeta(List<GeographicLevel> geographicLevels)
        {
            return await geographicLevels
                .ToAsyncEnumerable()
                .SelectAwait(async level =>
                {
                    var cols = level.CsvColumns();
                    var options = (await _duckDb.QueryAsync<(string Code, string Name)>(
                            new CommandDefinition(
                                $"""
                                 SELECT {cols.Code}, {cols.Name}
                                 FROM '{_dataFilePath}'
                                 WHERE {cols.Name} != ''
                                 GROUP BY {cols.Code}, {cols.Name}
                                 ORDER BY {cols.Name}, {cols.Code}
                                 """,
                                cancellationToken: _cancellationToken
                            )
                        ))
                        .Select(
                            (tuple, index) => new LocationOptionMeta
                            {
                                PublicId = _shortId.Generate(),
                                PrivateId = index + 1,
                                Code = tuple.Code,
                                Label = tuple.Name
                            }
                        )
                        .ToList();

                    return new LocationMeta
                    {
                        Level = level,
                        Options = options
                    };
                })
                .ToListAsync(_cancellationToken);
        }

        private static List<GeographicLevel> ListGeographicLevels(IReadOnlySet<string> allowedColumns)
        {
            return allowedColumns
                .Where(col => GeographicLevelUtils.CsvColumnsToGeographicLevel.ContainsKey(col))
                .Select(col => GeographicLevelUtils.CsvColumnsToGeographicLevel[col])
                .Distinct()
                .OrderBy(EnumToEnumLabelConverter<GeographicLevel>.ToProvider)
                .ToList();
        }

        private async Task<List<TimePeriodMeta>> ListTimePeriodMeta()
        {
            return (await _duckDb.QueryAsync<(int TimePeriod, string TimeIdentifier)>(
                    new CommandDefinition(
                        $"""
                         SELECT DISTINCT time_period, time_identifier
                         FROM '{_dataFilePath}'
                         ORDER BY time_period
                         """,
                        cancellationToken: _cancellationToken
                    )
                ))
                .Select(
                    tuple => new TimePeriodMeta
                    {
                        Year = tuple.TimePeriod,
                        Code = EnumToEnumLabelConverter<TimeIdentifier>.FromProvider(tuple.TimeIdentifier)
                    }
                )
                .OrderBy(meta => meta.Year)
                .ThenBy(meta => meta.Code)
                .ToList();
        }

        private async Task SeedParquetData(DataSetMeta meta, long totalRows)
        {
            await _console.Output.WriteLineAsync($"=> Processing {totalRows} rows");

            // Create temporary meta tables in DuckDB to allow us to do data transform
            // in DuckDB itself (i.e. changing all the filters and locations to normalised IDs).
            // Trying to transform the data via using Appender API in C# is slower
            // and seems to regularly cause DuckDB crashes for larger data sets.
            await CreateDuckDbMetaTables(meta);

            await _duckDb.ExecuteAsync("CREATE SEQUENCE data_seq START 1");

            string[] columns =
            [
                "id UINTEGER PRIMARY KEY",
                "time_period VARCHAR",
                "time_identifier VARCHAR",
                "geographic_level VARCHAR",
                ..meta.Locations.Select(location => $"\"{location.Level}\" INTEGER"),
                ..meta.Filters.Select(filter => $"\"{filter.Identifier}\" INTEGER"),
                ..meta.Indicators.Select(indicator => $"\"{indicator.Identifier}\" VARCHAR"),
            ];

            await _duckDb.ExecuteAsync($"CREATE TABLE data({columns.JoinToString(",\n")})");

            string[] insertColumns =
            [
                "nextval('data_seq') AS id",
                "data_source.time_period",
                "data_source.time_identifier",
                "data_source.geographic_level",
                ..meta.Locations.Select(location => $"{location.Level}.id AS {location.Level}"),
                ..meta.Filters.Select(filter => $"{filter.Identifier}.id AS \"{filter.Identifier}\""),
                ..meta.Indicators.Select(indicator => $"\"{indicator.Identifier}\""),
            ];

            string[] insertJoins =
            [
                ..meta.Locations.Select(location => $"""
                     LEFT JOIN locations AS {location.Level}
                     ON {location.Level}.level = '{location.Level}'
                     AND {location.Level}.code = data_source.{location.Level.CsvCodeColumn()}
                     AND {location.Level}.name = data_source.{location.Level.CsvNameColumn()}
                     """
                ),
                ..meta.Filters.Select(filter => $"""
                     LEFT JOIN filters AS "{filter.Identifier}"
                     ON "{filter.Identifier}".type = '{filter.Identifier}'
                     AND "{filter.Identifier}".label = data_source."{filter.Identifier}"
                     """
                )
            ];

            await _duckDb.ExecuteAsync(new CommandDefinition(
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
            ));

            // Finish up by outputting Parquet file

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

            var outputPath = Path.Combine(versionDir, "data.parquet");

            await _duckDb.ExecuteAsync(new CommandDefinition(
                $"COPY data TO '{outputPath}' (FORMAT PARQUET, COMPRESSION ZSTD)",
                cancellationToken: _cancellationToken
            ));
        }

        private async Task CreateDuckDbMetaTables(DataSetMeta meta)
        {
            await _duckDb.ExecuteAsync(
                """
                 CREATE TABLE locations(
                     id INTEGER,
                     code VARCHAR,
                     name VARCHAR,
                     level VARCHAR
                 )
                 """
            );

            foreach (var location in meta.Locations)
            {
                using var appender = _duckDb.CreateAppender(table: "locations");

                foreach (var option in location.Options)
                {
                    var insertRow = appender.CreateRow();

                    insertRow.AppendValue(option.PrivateId);
                    insertRow.AppendValue(option.Code);
                    insertRow.AppendValue(option.Label);
                    insertRow.AppendValue(location.Level.ToString());

                    insertRow.EndRow();
                }
            }

            await _duckDb.ExecuteAsync(
                """
                 CREATE TABLE filters(
                     id INTEGER,
                     label VARCHAR,
                     type VARCHAR
                 )
                 """
            );

            foreach (var filter in meta.Filters)
            {
                using var appender = _duckDb.CreateAppender(table: "filters");

                foreach (var option in filter.Options)
                {
                    var insertRow = appender.CreateRow();

                    insertRow.AppendValue(option.PrivateId);
                    insertRow.AppendValue(option.Label);
                    insertRow.AppendValue(filter.Identifier);

                    insertRow.EndRow();
                }
            }
        }
    }
}
