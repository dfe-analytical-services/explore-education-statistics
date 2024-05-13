using System.Linq.Expressions;
using Dapper;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.GeographicLevelUtils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services;

public class DataSetMetaService(
    IDataSetVersionPathResolver dataSetVersionPathResolver,
    IDuckDbConnection duckDb,
    PublicDataDbContext publicDataDbContext) : IDataSetMetaService
{
    public async Task CreateDataSetVersionMeta(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default)
    {
        var dataSetVersion = await publicDataDbContext.DataSetVersions
            .SingleAsync(dsv => dsv.Id == dataSetVersionId, cancellationToken: cancellationToken);

        var csvDataFilePath = dataSetVersionPathResolver.CsvDataPath(dataSetVersion);
        var csvMetadataFilePath = dataSetVersionPathResolver.CsvMetadataPath(dataSetVersion);

        var columns = duckDb.Query<(string ColumnName, string ColumnType)>(
                $"DESCRIBE SELECT * FROM '{csvDataFilePath}'"
            )
            .Select(row => row.ColumnName)
            .ToList();

        var allowedColumns = columns.ToHashSet();

        var metaFileRows = (await duckDb.QueryAsync<MetaFileRow>(
                new CommandDefinition(
                    $"SELECT * FROM '{csvMetadataFilePath}'",
                    cancellationToken: cancellationToken
                )
            ))
            .ToList();

        var geographicLevelMeta = await CreateGeographicLevelMeta(dataSetVersionId, csvDataFilePath, cancellationToken);
        await CreateFilterMetas(dataSetVersionId, csvDataFilePath, metaFileRows, allowedColumns, cancellationToken);
        await CreateLocationMetas(dataSetVersionId, csvDataFilePath, allowedColumns, cancellationToken);
        await CreateIndicatorMeta(dataSetVersionId, metaFileRows, allowedColumns, cancellationToken);
        await CreateTimePeriodMeta(dataSetVersionId, csvDataFilePath, cancellationToken);

        await SetMetaSummary(dataSetVersion,
            csvDataFilePath,
            geographicLevelMeta,
            metaFileRows,
            allowedColumns,
            cancellationToken);
        await SetTotalResults(dataSetVersion, csvDataFilePath, cancellationToken);
    }

    private async Task<GeographicLevelMeta> CreateGeographicLevelMeta(
        Guid dataSetVersionId,
        string csvDataFilePath,
        CancellationToken cancellationToken)
    {
        var geographicLevels = (await duckDb.QueryAsync<string>(
                new CommandDefinition(
                    $"""
                     SELECT DISTINCT geographic_level
                     FROM read_csv_auto('{csvDataFilePath}', ALL_VARCHAR = true)
                     """,
                    cancellationToken
                )
            ))
            .Select(EnumToEnumLabelConverter<GeographicLevel>.FromProvider)
            .ToList();

        var geographicLevelMeta = new GeographicLevelMeta
        {
            DataSetVersionId = dataSetVersionId, Levels = geographicLevels
        };

        publicDataDbContext.GeographicLevelMetas.Add(geographicLevelMeta);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);

        return geographicLevelMeta;
    }

    private async Task CreateFilterMetas(
        Guid dataSetVersionId,
        string dataFilePath,
        IEnumerable<MetaFileRow> metaFileRows,
        IReadOnlySet<string> allowedColumns,
        CancellationToken cancellationToken)
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
                    DataSetVersionId = dataSetVersionId,
                    Label = row.Label,
                    Hint = row.FilterHint ?? string.Empty,
                }
            )
            .ToList();

        publicDataDbContext.FilterMetas.AddRange(metas);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);

        foreach (var meta in metas)
        {
            var options = (await duckDb.QueryAsync<string>(
                    new CommandDefinition(
                        $"""
                         SELECT DISTINCT "{meta.PublicId}"
                         FROM read_csv_auto('{dataFilePath}', ALL_VARCHAR = true) AS data
                         WHERE "{meta.PublicId}" != ''
                         ORDER BY "{meta.PublicId}"
                         """,
                        cancellationToken
                    )
                ))
                .Select(
                    label => new FilterOptionMeta
                    {
                        Label = label, IsAggregate = label == "Total" ? true : null
                    }
                )
                .ToList();

            var optionTable = publicDataDbContext.GetTable<FilterOptionMeta>();

            // Merge to only inserting new filter options
            // that don't already exist in the table.
            await optionTable
                .Merge()
                .Using(options)
                .On(
                    o => new
                    {
                        o.Label, o.IsAggregate
                    },
                    o => new
                    {
                        o.Label, o.IsAggregate
                    }
                )
                .InsertWhenNotMatched()
                .MergeAsync(cancellationToken);

            var startIndex = await publicDataDbContext.FilterOptionMetaLinks.CountAsync(token: cancellationToken);

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
                    .Select(o => o.Label + ',' + (o.IsAggregate == true ? "True" : ""))
                    .ToHashSet();

                var links = await optionTable
                    .Where(o =>
                        batchRowKeys.Contains(o.Label + ',' + (o.IsAggregate == true ? "True" : "")))
                    .Select((option, index) => new FilterOptionMetaLink
                    {
                        PublicId = SqidEncoder.Encode(batchStartIndex + index), MetaId = meta.Id, OptionId = option.Id
                    })
                    .ToListAsync(token: cancellationToken);

                publicDataDbContext.FilterOptionMetaLinks.AddRange(links);
                await publicDataDbContext.SaveChangesAsync(cancellationToken);

                current += batchSize;
            }

            var insertedLinks = await publicDataDbContext.FilterOptionMetaLinks
                .CountAsync(l => l.MetaId == meta.Id,
                    cancellationToken: cancellationToken);

            if (insertedLinks != options.Count)
            {
                throw new InvalidOperationException(
                    $"Inserted incorrect number of filter option meta links for {meta.PublicId}. " +
                    $"Inserted: {insertedLinks}, expected: {options.Count}");
            }
        }
    }

    private async Task CreateIndicatorMeta(
        Guid dataSetVersionId,
        IEnumerable<MetaFileRow> metaFileRows,
        IReadOnlySet<string> allowedColumns,
        CancellationToken cancellationToken)
    {
        var metas = metaFileRows
            .Where(row => row.ColType == MetaFileRow.ColumnType.Indicator
                          && allowedColumns.Contains(row.ColName))
            .OrderBy(row => row.Label)
            .ToList()
            .Select(
                row => new IndicatorMeta
                {
                    DataSetVersionId = dataSetVersionId,
                    PublicId = row.ColName,
                    Label = row.Label,
                    Unit = row.IndicatorUnit,
                    DecimalPlaces = row.IndicatorDp
                }
            )
            .ToList();

        publicDataDbContext.IndicatorMetas.AddRange(metas);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task CreateLocationMetas(
        Guid dataSetVersionId,
        string dataFilePath,
        IReadOnlySet<string> allowedColumns,
        CancellationToken cancellationToken)
    {
        var levels = ListLocationLevels(allowedColumns);

        var metas = levels
            .Select(level => new LocationMeta
            {
                DataSetVersionId = dataSetVersionId, Level = level,
            })
            .ToList();

        publicDataDbContext.LocationMetas.AddRange(metas);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);

        foreach (var meta in metas)
        {
            var nameCol = meta.Level.CsvNameColumn();
            var codeCols = meta.Level.CsvCodeColumns();
            string[] cols = [..codeCols, nameCol];

            var options = (await duckDb.QueryAsync(
                    new CommandDefinition(
                        $"""
                         SELECT {cols.JoinToString(", ")}
                         FROM read_csv_auto('{dataFilePath}', ALL_VARCHAR = true)
                         WHERE {cols.Select(col => $"{col} != ''").JoinToString(" AND ")}
                         GROUP BY {cols.JoinToString(", ")}
                         ORDER BY {cols.JoinToString(", ")}
                         """,
                        cancellationToken
                    )
                ))
                .Cast<IDictionary<string, object?>>()
                .Select(row => row.ToDictionary(
                    kv => kv.Key,
                    kv => (string)kv.Value!
                ))
                .Select(row => MapLocationOptionMeta(row, meta.Level).ToRow())
                .ToList();

            var optionTable = publicDataDbContext
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
                        .ToListAsync(token: cancellationToken))
                    .Select(o => o.GetRowKey())
                    .ToHashSet();

                if (existingRowKeys.Count != batch.Count)
                {
                    var newOptions = batch
                        .Where(o => !existingRowKeys.Contains(o.GetRowKey()))
                        .ToList();

                    var startIndex = await publicDataDbContext.LocationOptionMetas.CountAsync(token: cancellationToken);

                    foreach (var option in newOptions)
                    {
                        option.Id = startIndex++;
                        option.PublicId = SqidEncoder.Encode(option.Id);
                    }

                    await optionTable.BulkCopyAsync(newOptions, cancellationToken);
                }

                var links = await optionTable
                    .Where(hasBatchRowKey)
                    .Select((option, index) => new LocationOptionMetaLink
                    {
                        MetaId = meta.Id,
                        OptionId = option.Id
                    })
                    .ToListAsync(token: cancellationToken);

                publicDataDbContext.LocationOptionMetaLinks.AddRange(links);
                await publicDataDbContext.SaveChangesAsync(cancellationToken);

                current += batchSize;
            }

            var insertedLinks = await publicDataDbContext.LocationOptionMetaLinks
                .CountAsync(
                    l => l.MetaId == meta.Id,
                    cancellationToken: cancellationToken);

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
                PublicId = string.Empty,
                Label = label,
                Code = row[LocalAuthorityCsvColumns.NewCode],
                OldCode = row[LocalAuthorityCsvColumns.OldCode]
            },
            GeographicLevel.School => new LocationSchoolOptionMeta
            {
                PublicId = string.Empty,
                Label = label,
                Urn = row[SchoolCsvColumns.Urn],
                LaEstab = row[SchoolCsvColumns.LaEstab]
            },
            GeographicLevel.Provider => new LocationProviderOptionMeta
            {
                PublicId = string.Empty,
                Label = label,
                Ukprn = row[ProviderCsvColumns.Ukprn]
            },
            GeographicLevel.RscRegion => new LocationRscRegionOptionMeta
            {
                PublicId = string.Empty,
                Label = label
            },
            _ => new LocationCodedOptionMeta
            {
                PublicId = string.Empty,
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

    private async Task CreateTimePeriodMeta(
        Guid dataSetVersionId,
        string dataFilePath,
        CancellationToken cancellationToken)
    {
        var metas = (await duckDb.QueryAsync<(string TimePeriod, string TimeIdentifier)>(
                new CommandDefinition(
                    $"""
                     SELECT DISTINCT time_period, time_identifier
                     FROM read_csv_auto('{dataFilePath}', ALL_VARCHAR = true)
                     ORDER BY time_period
                     """,
                    cancellationToken
                )
            ))
            .Select(
                tuple => new TimePeriodMeta
                {
                    DataSetVersionId = dataSetVersionId,
                    Period = TimePeriodFormatter.FormatFromCsv(tuple.TimePeriod),
                    Code = EnumToEnumLabelConverter<TimeIdentifier>.FromProvider(tuple.TimeIdentifier)
                }
            )
            .OrderBy(meta => meta.Period)
            .ThenBy(meta => meta.Code)
            .ToList();

        publicDataDbContext.TimePeriodMetas.AddRange(metas);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SetMetaSummary(
        DataSetVersion dataSetVersion,
        string csvDataFilePath,
        GeographicLevelMeta geographicLevelMeta,
        IList<MetaFileRow> metaFileRows,
        HashSet<string> allowedColumns,
        CancellationToken cancellationToken)
    {
        var timePeriods = (await duckDb.QueryAsync<(string TimePeriod, string TimeIdentifier)>(
                new CommandDefinition(
                    $"""
                     SELECT DISTINCT time_period, time_identifier
                     FROM read_csv_auto('{csvDataFilePath}', ALL_VARCHAR = true)
                     ORDER BY time_period
                     """,
                    cancellationToken
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

        dataSetVersion.MetaSummary = new DataSetVersionMetaSummary
        {
            TimePeriodRange = new TimePeriodRange
            {
                Start = new TimePeriodRangeBound
                {
                    Period = timePeriods[0].Period, Code = timePeriods[0].Identifier
                },
                End = new TimePeriodRangeBound
                {
                    Period = timePeriods[^1].Period, Code = timePeriods[^1].Identifier
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
            GeographicLevels = geographicLevelMeta.Levels
        };

        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SetTotalResults(
        DataSetVersion dataSetVersion,
        string csvDataFilePath,
        CancellationToken cancellationToken)
    {
        dataSetVersion.TotalResults = await duckDb.QuerySingleAsync<int>($"SELECT COUNT(*) FROM '{csvDataFilePath}'");
        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }
}
