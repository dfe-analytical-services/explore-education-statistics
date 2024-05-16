using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using InterpolatedSql.Dapper;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.GeographicLevelUtils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository;

public class LocationRepository(
    PublicDataDbContext publicDataDbContext,
    IDataSetVersionPathResolver dataSetVersionPathResolver) : ILocationRepository
{
    public async Task<GeographicLevelMeta> CreateGeographicLevelMeta(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default)
    {
        var geographicLevels =
            (await duckDbConnection.SqlBuilder(
                $"""
                 SELECT DISTINCT geographic_level
                 FROM read_csv('{dataSetVersionPathResolver.CsvDataPath(dataSetVersion):raw}', ALL_VARCHAR = true)
                 """
            ).QueryAsync<string>(cancellationToken: cancellationToken))
            .Select(EnumToEnumLabelConverter<GeographicLevel>.FromProvider)
            .ToList();

        var geographicLevelMeta = new GeographicLevelMeta
        {
            DataSetVersionId = dataSetVersion.Id,
            Levels = geographicLevels
        };

        publicDataDbContext.GeographicLevelMetas.Add(geographicLevelMeta);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);

        return geographicLevelMeta;
    }

    public async Task CreateLocationMetas(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        IReadOnlySet<string> allowedColumns,
        CancellationToken cancellationToken = default)
    {
        var levels = ListLocationLevels(allowedColumns);

        var metas = levels
            .Select(level => new LocationMeta
            {
                DataSetVersionId = dataSetVersion.Id,
                Level = level,
            })
            .ToList();

        publicDataDbContext.LocationMetas.AddRange(metas);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);

        foreach (var meta in metas)
        {
            var nameCol = meta.Level.CsvNameColumn();
            var codeCols = meta.Level.CsvCodeColumns();
            string[] cols = [..codeCols, nameCol];

            var options = (await duckDbConnection.SqlBuilder(
                        $"""
                         SELECT {cols.JoinToString(", "):raw}
                         FROM read_csv('{dataSetVersionPathResolver.CsvDataPath(dataSetVersion):raw}', ALL_VARCHAR = true)
                         WHERE {cols.Select(col => $"{col} != ''").JoinToString(" AND "):raw}
                         GROUP BY {cols.JoinToString(", "):raw}
                         ORDER BY {cols.JoinToString(", "):raw}
                         """
                    ).QueryAsync(cancellationToken: cancellationToken)
                )
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

    public async Task CreateLocationMetaTable(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default)
    {
        await publicDataDbContext
            .Entry(dataSetVersion)
            .Collection(dsv => dsv.LocationMetas)
            .Query()
            .Include(m => m.Options)
            .LoadAsync(cancellationToken);

        await duckDbConnection.SqlBuilder(
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
        ).ExecuteAsync(cancellationToken: cancellationToken);

        var id = 1;

        foreach (var location in dataSetVersion.LocationMetas)
        {
            using var appender = duckDbConnection.CreateAppender(table: LocationOptionsTable.TableName);

            var insertRow = appender.CreateRow();

            foreach (var link in location.OptionLinks.OrderBy(l => l.Option.Label))
            {
                var option = link.Option;

                insertRow.AppendValue(id++);
                insertRow.AppendValue(option.Label);
                insertRow.AppendValue(location.Level.GetEnumValue());
                insertRow.AppendValue(option.PublicId);

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

    private static List<GeographicLevel> ListLocationLevels(IReadOnlySet<string> allowedColumns)
    {
        return
        [
            .. allowedColumns
                .Where(CsvColumnsToGeographicLevel.ContainsKey)
                .Select(col => CsvColumnsToGeographicLevel[col])
                .Distinct()
                .OrderBy(EnumToEnumLabelConverter<GeographicLevel>.ToProvider)
        ];
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
}
