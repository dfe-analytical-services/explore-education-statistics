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
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.GeographicLevelUtils;

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
                        Start = new TimePeriodOptionMeta
                        {
                            Year = timePeriods[0].Year,
                            Code = timePeriods[0].TimeIdentifier
                        },
                        End = new TimePeriodOptionMeta
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

        private async Task CreateDataSetMeta(
            IList<MetaFileRow> metaFileRows,
            IReadOnlySet<string> allowedColumns)
        {
            await CreateGeographicLevelMeta(allowedColumns);
            await CreateFilterMetas(metaFileRows, allowedColumns);
            await CreateLocationMetas(allowedColumns);
            await CreateIndicatorMeta(metaFileRows, allowedColumns);
            await CreateTimePeriodMeta();
        }

        private async Task CreateIndicatorMeta(
            IEnumerable<MetaFileRow> metaFileRows,
            IReadOnlySet<string> allowedColumns)
        {
            var options = metaFileRows
                .Where(row => row.ColType == MetaFileRow.ColumnType.Indicator
                              && allowedColumns.Contains(row.ColName))
                .OrderBy(row => row.Label)
                .Select(
                    row => new IndicatorOptionMeta
                    {
                        Identifier = row.ColName,
                        Label = row.Label,
                        Unit = row.IndicatorUnit,
                        DecimalPlaces = row.IndicatorDp
                    }
                )
                .ToList();

            var meta = new IndicatorMeta
            {
                DataSetVersionId = _seed.DataSetVersionId,
                Options = options
            };

            _dbContext.IndicatorMetas.AddRange(meta);
            await _dbContext.SaveChangesAsync(_cancellationToken);
        }

        private async Task CreateFilterMetas(
            IEnumerable<MetaFileRow> metaFileRows,
            IReadOnlySet<string> allowedColumns)
        {
            var metas = await metaFileRows
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
                        PublicId = row.ColName,
                        DataSetVersionId = _seed.DataSetVersionId,
                        Label = row.Label,
                        Hint = row.FilterHint ?? string.Empty,
                        Options = options
                    };
                })
                .ToListAsync(_cancellationToken);

            _dbContext.FilterMetas.AddRange(metas);
            await _dbContext.SaveChangesAsync(_cancellationToken);
        }

         private async Task CreateLocationMetas(IReadOnlySet<string> allowedColumns)
         {
             var geographicLevels = ListGeographicLevels(allowedColumns);

             var metas = await geographicLevels
                 .ToAsyncEnumerable()
                 .SelectAwait<GeographicLevel, LocationMeta>(async level =>
                 {
                     var nameCol = level.CsvNameColumn();
                     var codeCols = level.CsvCodeColumns();
                     string[] cols = [..codeCols, nameCol];

                     var rows = (await _duckDb.QueryAsync(
                             new CommandDefinition(
                                 $"""
                                  SELECT {cols.JoinToString(", ")}
                                  FROM read_csv_auto('{_dataFilePath}', ALL_VARCHAR = TRUE)
                                  WHERE {cols.Select(col => $"{col} != ''").JoinToString(" AND ")}
                                  GROUP BY {cols.JoinToString(", ")}
                                  ORDER BY {cols.JoinToString(", ")}
                                  """,
                                 cancellationToken: _cancellationToken
                             )
                         ))
                         .Cast<IDictionary<string, object?>>()
                         .Select(row =>
                             row.ToDictionary(
                                 kv => kv.Key,
                                 kv => (string)kv.Value!)
                         );

                     return level switch
                     {
                         GeographicLevel.LocalAuthority => new LocationLocalAuthorityMeta
                         {
                             DataSetVersionId = _seed.DataSetVersionId,
                             Level = level,
                             Options = rows
                                 .Select((row, index) => MapLocationOptionMeta(row, index, level))
                                 .Cast<LocationLocalAuthorityOptionMeta>()
                                 .ToList()
                         },
                         GeographicLevel.Provider => new LocationProviderMeta
                         {
                             DataSetVersionId = _seed.DataSetVersionId,
                             Level = level,
                             Options = rows
                                 .Select((row, index) => MapLocationOptionMeta(row, index, level))
                                 .Cast<LocationProviderOptionMeta>()
                                 .ToList()
                         },
                         GeographicLevel.RscRegion => new LocationRscRegionMeta
                         {
                             DataSetVersionId = _seed.DataSetVersionId,
                             Level = level,
                             Options = rows
                                 .Select((row, index) => MapLocationOptionMeta(row, index, level))
                                 .Cast<LocationRscRegionOptionMeta>()
                                 .ToList(),
                         },
                         GeographicLevel.School => new LocationSchoolMeta
                         {
                             DataSetVersionId = _seed.DataSetVersionId,
                             Level = level,
                             Options = rows
                                 .Select((row, index) => MapLocationOptionMeta(row, index, level))
                                 .Cast<LocationSchoolOptionMeta>()
                                 .ToList()
                         },
                         _ => new LocationDefaultMeta
                         {
                             DataSetVersionId = _seed.DataSetVersionId,
                             Level = level,
                             Options = rows
                                 .Select((row, index) => MapLocationOptionMeta(row, index, level))
                                 .Cast<LocationOptionMeta>()
                                 .ToList()
                         }
                     };
                 })
                 .ToListAsync(_cancellationToken);

             _dbContext.LocationMetas.AddRange(metas);
             await _dbContext.SaveChangesAsync(_cancellationToken);
         }
         

        private LocationOptionMetaBase MapLocationOptionMeta(
            IDictionary<string, string> row,
            int index,
            GeographicLevel level)
        {
            var cols = level.CsvColumns();

            var publicId = _shortId.Generate();
            var privateId = index + 1;
            var label = row[level.CsvNameColumn()];

            return level switch
            {
                GeographicLevel.LocalAuthority => new LocationLocalAuthorityOptionMeta
                {
                    PublicId = publicId,
                    PrivateId = privateId,
                    Label = label,
                    Code = row[LocalAuthorityCsvColumns.NewCode],
                    OldCode = row[LocalAuthorityCsvColumns.OldCode]
                },
                GeographicLevel.School => new LocationSchoolOptionMeta
                {
                    PublicId = publicId,
                    PrivateId = privateId,
                    Label = label,
                    Urn = row[SchoolCsvColumns.Urn],
                    LaEstab = row[SchoolCsvColumns.LaEstab]
                },
                GeographicLevel.Provider => new LocationProviderOptionMeta
                {
                    PublicId = publicId,
                    PrivateId = privateId,
                    Label = label,
                    Ukprn = row[ProviderCsvColumns.Ukprn]
                },
                GeographicLevel.RscRegion => new LocationRscRegionOptionMeta
                {
                    PublicId = publicId,
                    PrivateId = privateId,
                    Label = label
                },
                _ => new LocationOptionMeta
                {
                    PublicId = publicId,
                    PrivateId = privateId,
                    Label = label,
                    Code = row[cols.Codes.First()]
                }
            };
        }

        private List<GeographicLevel> ListGeographicLevels(IReadOnlySet<string> allowedColumns)
        {
            return allowedColumns
                .Where(col => CsvColumnsToGeographicLevel.ContainsKey(col))
                .Select(col => CsvColumnsToGeographicLevel[col])
                .Distinct()
                .OrderBy(EnumToEnumLabelConverter<GeographicLevel>.ToProvider)
                .ToList();
        }

        private async Task CreateGeographicLevelMeta(IReadOnlySet<string> allowedColumns)
        {
            var meta = new GeographicLevelMeta
            {
                DataSetVersionId = _seed.DataSetVersionId,
                Options = ListGeographicLevels(allowedColumns).ToHashSet()
            };

            _dbContext.GeographicLevelMetas.AddRange(meta);
            await _dbContext.SaveChangesAsync(_cancellationToken);
        }

        private async Task CreateTimePeriodMeta()
        {
            var options = (await _duckDb.QueryAsync<(int TimePeriod, string TimeIdentifier)>(
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
                    tuple => new TimePeriodOptionMeta
                    {
                        Year = tuple.TimePeriod,
                        Code = EnumToEnumLabelConverter<TimeIdentifier>.FromProvider(tuple.TimeIdentifier)
                    }
                )
                .OrderBy(meta => meta.Year)
                .ThenBy(meta => meta.Code)
                .ToList();

            var meta = new TimePeriodMeta
            {
                DataSetVersionId = _seed.DataSetVersionId,
                Options = options
            };

            _dbContext.TimePeriodMetas.AddRange(meta);
            await _dbContext.SaveChangesAsync(_cancellationToken);
        }

        private async Task SeedParquetData()
        {
            var version = await _dbContext.DataSetVersions
                .Include(v => v.FilterMetas)
                .Include(v => v.IndicatorMeta)
                .Include(v => v.LocationMetas)
                .Include(v => v.GeographicLevelMeta)
                .Include(v => v.TimePeriodMeta)
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
                "time_period VARCHAR",
                "time_identifier VARCHAR",
                "geographic_level VARCHAR",
                ..version.LocationMetas.Select(location => $"{location.Level.GetEnumValue().ToLower()}_id INTEGER"),
                ..version.FilterMetas.Select(filter => $"\"{filter.PublicId}\" INTEGER"),
                ..version.IndicatorMeta.Options.Select(indicator => $"\"{indicator.Identifier}\" VARCHAR"),
            ];

            await _duckDb.ExecuteAsync($"CREATE TABLE data({columns.JoinToString(",\n")})");

            string[] insertColumns =
            [
                "nextval('data_seq') AS id",
                "data_source.time_period",
                "data_source.time_identifier",
                "data_source.geographic_level",
                ..version.LocationMetas.Select(location =>
                    $"{GetDuckDbLocationTableName(location.Level)}.id AS {location.Level.GetEnumValue().ToLower()}_id"),
                ..version.FilterMetas.Select(filter => $"\"{filter.PublicId}\".id AS \"{filter.PublicId}\""),
                ..version.IndicatorMeta.Options.Select(indicator => $"\"{indicator.Identifier}\""),
            ];

            string[] insertJoins =
            [
                ..version.LocationMetas.Select(location =>
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
                }),
                ..version.FilterMetas.Select(filter => $"""
                     LEFT JOIN filters AS "{filter.PublicId}"
                     ON "{filter.PublicId}".public_id = '{filter.PublicId}'
                     AND "{filter.PublicId}".label = data_source."{filter.PublicId}"
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

        private async Task CreateDuckDbMetaTables(DataSetVersion version)
        {
            foreach (var location in version.LocationMetas)
            {
                var locationTable = GetDuckDbLocationTableName(location.Level);

                string[] locationCols = [
                    "id INTEGER",
                    "name VARCHAR",
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

                switch (location)
                {
                    case LocationDefaultMeta defaultLocationMeta:
                    {
                        AppendLocationOptionRows(appender, defaultLocationMeta.Options);
                        break;
                    }
                    case LocationLocalAuthorityMeta localAuthorityMeta:
                        AppendLocationOptionRows(appender, localAuthorityMeta.Options);
                        break;
                    case LocationProviderMeta providerMeta:
                        AppendLocationOptionRows(appender, providerMeta.Options);
                        break;
                    case LocationRscRegionMeta rscRegionMeta:
                        AppendLocationOptionRows(appender, rscRegionMeta.Options);
                        break;
                    case LocationSchoolMeta schoolMeta:
                        AppendLocationOptionRows(appender, schoolMeta.Options);
                        break;
                }
            }

            await _duckDb.ExecuteAsync(
                """
                 CREATE TABLE filters(
                     id INTEGER,
                     label VARCHAR,
                     public_id VARCHAR
                 )
                 """
            );

            foreach (var filter in version.FilterMetas)
            {
                using var appender = _duckDb.CreateAppender(table: "filters");

                foreach (var option in filter.Options)
                {
                    var insertRow = appender.CreateRow();

                    insertRow.AppendValue(option.PrivateId);
                    insertRow.AppendValue(option.Label);
                    insertRow.AppendValue(filter.PublicId);

                    insertRow.EndRow();
                }
            }
        }

        private void AppendLocationOptionRows<TOptionMeta>(DuckDBAppender appender, List<TOptionMeta> options)
            where TOptionMeta : LocationOptionMetaBase
        {
            foreach (var option in options)
            {
                var row = appender.CreateRow();

                row.AppendValue(option.PrivateId);
                row.AppendValue(option.Label);

                switch (option)
                {
                    case LocationOptionMeta codedOption:
                        row.AppendValue(codedOption.Code);
                        break;
                    case LocationLocalAuthorityOptionMeta laOption:
                        row.AppendValue(laOption.Code);
                        row.AppendValue(laOption.OldCode);
                        break;
                    case LocationProviderOptionMeta providerOption:
                        row.AppendValue(providerOption.Ukprn);
                        break;
                    case LocationSchoolOptionMeta schoolOption:
                        row.AppendValue(schoolOption.Urn);
                        row.AppendValue(schoolOption.LaEstab);
                        break;
                }

                row.EndRow();
            }
        }

        private LocationColumn[] GetDuckDbLocationCodeColumns(GeographicLevel geographicLevel)
        {
            return geographicLevel switch
            {
                GeographicLevel.LocalAuthority => [
                    new LocationColumn(Name: "code", CsvName: LocalAuthorityCsvColumns.NewCode),
                    new LocationColumn(Name: "old_code", CsvName: LocalAuthorityCsvColumns.OldCode)
                ],
                GeographicLevel.Provider => [
                    new LocationColumn(Name: "ukprn", CsvName: ProviderCsvColumns.Ukprn)
                ],
                GeographicLevel.RscRegion => [],
                GeographicLevel.School => [
                    new LocationColumn(Name: "urn", CsvName: SchoolCsvColumns.Urn),
                    new LocationColumn(Name: "laestab", CsvName: SchoolCsvColumns.LaEstab)
                ],
                _ => [
                    new LocationColumn(Name: "code", CsvName: geographicLevel.CsvCodeColumns().First())
                ],
            };
        }

        private string GetDuckDbLocationTableName(GeographicLevel geographicLevel)
        {
            return $"locations_{geographicLevel.GetEnumValue().ToLower()}";
        }

        private record LocationColumn(string Name, string CsvName);
    }
}
