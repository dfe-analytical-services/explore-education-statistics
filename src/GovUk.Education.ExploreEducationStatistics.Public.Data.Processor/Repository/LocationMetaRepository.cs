using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Options;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using InterpolatedSql.Dapper;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.GeographicLevelUtils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository;

public class LocationMetaRepository(
    PublicDataDbContext publicDataDbContext,
    IOptions<AppOptions> appOptions,
    IDataSetVersionPathResolver dataSetVersionPathResolver
) : ILocationMetaRepository
{
    private readonly AppOptions _appOptions = appOptions.Value;

    public async Task<IDictionary<LocationMeta, List<LocationOptionMetaRow>>> ReadLocationMetas(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        IReadOnlySet<string> allowedColumns,
        CancellationToken cancellationToken
    )
    {
        var metas = GetLocationMetas(dataSetVersion, allowedColumns);

        return await metas
            .ToAsyncEnumerable()
            .ToDictionaryAwaitAsync(
                keySelector: ValueTask.FromResult,
                elementSelector: async meta =>
                    await GetLocationOptionMetas(duckDbConnection, dataSetVersion, meta, cancellationToken),
                cancellationToken
            );
    }

    public async Task CreateLocationMetas(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        IReadOnlySet<string> allowedColumns,
        CancellationToken cancellationToken = default
    )
    {
        var metas = GetLocationMetas(dataSetVersion, allowedColumns);
        publicDataDbContext.LocationMetas.AddRange(metas);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);

        var publicIdMappings = await CreatePublicIdMappings(dataSetVersion, cancellationToken);

        foreach (var meta in metas)
        {
            var options = await GetLocationOptionMetas(duckDbConnection, dataSetVersion, meta, cancellationToken);

            var optionTable = publicDataDbContext
                .GetTable<LocationOptionMetaRow>()
                .TableName(nameof(PublicDataDbContext.LocationOptionMetas));

            var current = 0;

            while (current < options.Count)
            {
                var batch = options.Skip(current).Take(_appOptions.MetaInsertBatchSize).ToList();

                // We create a 'row key' for each option that allows us to quickly
                // find the option rows that exist in the database. It's typically
                // much slower to have multiple WHERE clauses for each row that check
                // against every other row. Out of several such attempts, the 'row key'
                // technique was the fastest and simplest way to create the links.
                var batchRowKeys = batch.Select(o => o.GetRowKey()).ToHashSet();

                Expression<Func<LocationOptionMetaRow, bool>> hasBatchRowKey = o =>
                    o.Type == batch[0].Type
                    && batchRowKeys.Contains(
                        o.Type
                            + ','
                            + o.Label
                            + ','
                            + (o.Code ?? "null")
                            + ','
                            + (o.OldCode ?? "null")
                            + ','
                            + (o.Urn ?? "null")
                            + ','
                            + (o.LaEstab ?? "null")
                            + ','
                            + (o.Ukprn ?? "null")
                    );

                var existingRowKeys = (await optionTable.Where(hasBatchRowKey).ToListAsync(token: cancellationToken))
                    .Select(o => o.GetRowKey())
                    .ToHashSet();

                if (existingRowKeys.Count != batch.Count)
                {
                    var newOptions = batch.Where(o => !existingRowKeys.Contains(o.GetRowKey())).ToList();

                    var startIndex = await publicDataDbContext.NextSequenceValue(
                        PublicDataDbContext.LocationOptionMetasIdSequence,
                        cancellationToken
                    );

                    foreach (var option in newOptions)
                    {
                        option.Id = startIndex++;
                    }

                    await optionTable.BulkCopyAsync(
                        new BulkCopyOptions { KeepIdentity = true },
                        newOptions,
                        cancellationToken
                    );

                    await publicDataDbContext.SetSequenceValue(
                        PublicDataDbContext.LocationOptionMetasIdSequence,
                        startIndex - 1,
                        cancellationToken
                    );
                }

                var links = await optionTable
                    .Where(hasBatchRowKey)
                    .Select(
                        (option, index) =>
                            new LocationOptionMetaLink
                            {
                                PublicId = CreatePublicIdForLocationOptionMetaLink(publicIdMappings, meta, option),
                                MetaId = meta.Id,
                                OptionId = option.Id,
                            }
                    )
                    .ToListAsync(token: cancellationToken);

                publicDataDbContext.LocationOptionMetaLinks.AddRange(links);
                await publicDataDbContext.SaveChangesAsync(cancellationToken);

                current += _appOptions.MetaInsertBatchSize;
            }

            var insertedLinks = await publicDataDbContext.LocationOptionMetaLinks.CountAsync(
                l => l.MetaId == meta.Id,
                cancellationToken: cancellationToken
            );

            if (insertedLinks != options.Count)
            {
                throw new InvalidOperationException(
                    $"Inserted incorrect number of location option meta links for {meta.Level}. "
                        + $"Inserted: {insertedLinks}, expected: {options.Count}"
                );
            }
        }
    }

    private static string CreatePublicIdForLocationOptionMetaLink(
        PublicIdMappings publicIdMappings,
        GeographicLevel level,
        LocationOptionMetaRow option
    )
    {
        return publicIdMappings.GetPublicIdForCandidate(
                level: level,
                candidateKey: MappingKeyGenerators.LocationOptionMetaRow(option)
            ) ?? SqidEncoder.Encode(option.Id);
    }

    private async Task<List<LocationOptionMetaRow>> GetLocationOptionMetas(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        LocationMeta meta,
        CancellationToken cancellationToken
    )
    {
        var nameCol = meta.Level.CsvNameColumn();
        var codeCols = meta.Level.CsvCodeColumns();
        string[] cols = [.. codeCols, nameCol];

        return (
            await duckDbConnection
                .SqlBuilder(
                    $"""
                    SELECT {cols.JoinToString(", "):raw}
                    FROM read_csv(
                        '{dataSetVersionPathResolver.CsvDataPath(dataSetVersion):raw}',
                        {DuckDbConstants.ReadCsvOptions:raw}
                    )
                    WHERE {cols.Select(col => $"{col} != ''").JoinToString(" AND "):raw}
                    GROUP BY {cols.JoinToString(", "):raw}
                    ORDER BY {cols.JoinToString(", "):raw}
                    """
                )
                .QueryAsync(cancellationToken: cancellationToken)
        ).Cast<IDictionary<string, object?>>().Select(row => row.ToDictionary(kv => kv.Key, kv => (string)kv.Value!)).Select(row => MapLocationOptionMeta(row, meta.Level).ToRow()).ToList();
    }

    private static List<LocationMeta> GetLocationMetas(
        DataSetVersion dataSetVersion,
        IReadOnlySet<string> allowedColumns
    )
    {
        var levels = ListLocationLevels(allowedColumns);

        return levels
            .Select(level => new LocationMeta { DataSetVersionId = dataSetVersion.Id, Level = level })
            .ToList();
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

    private static LocationOptionMeta MapLocationOptionMeta(IDictionary<string, string> row, GeographicLevel level)
    {
        var cols = level.CsvColumns();
        var label = row[level.CsvNameColumn()];

        return level switch
        {
            GeographicLevel.LocalAuthority => new LocationLocalAuthorityOptionMeta
            {
                Label = label,
                Code = row[LocalAuthorityCsvColumns.NewCode],
                OldCode = row[LocalAuthorityCsvColumns.OldCode],
            },
            GeographicLevel.School => new LocationSchoolOptionMeta
            {
                Label = label,
                Urn = row[SchoolCsvColumns.Urn],
                LaEstab = row[SchoolCsvColumns.LaEstab],
            },
            GeographicLevel.Provider => new LocationProviderOptionMeta
            {
                Label = label,
                Ukprn = row[ProviderCsvColumns.Ukprn],
            },
            GeographicLevel.RscRegion => new LocationRscRegionOptionMeta { Label = label },
            _ => new LocationCodedOptionMeta { Label = label, Code = row[cols.Codes.First()] },
        };
    }

    private async Task<PublicIdMappings> CreatePublicIdMappings(
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken
    )
    {
        var mappings = await EntityFrameworkQueryableExtensions.SingleOrDefaultAsync(
            publicDataDbContext.DataSetVersionMappings,
            mapping => mapping.TargetDataSetVersionId == dataSetVersion.Id,
            cancellationToken
        );

        if (mappings is null)
        {
            return new PublicIdMappings();
        }

        var mappingsByLevel = mappings.LocationMappingPlan.Levels.ToDictionary(
            keySelector: level => level.Key,
            elementSelector: level =>
                level
                    .Value.Mappings.Values.Where(mapping =>
                        mapping.Type is MappingType.AutoMapped or MappingType.ManualMapped
                    )
                    .ToDictionary(
                        keySelector: mapping => mapping.CandidateKey!,
                        elementSelector: mapping => mapping.PublicId
                    )
        );

        return new PublicIdMappings { Levels = mappingsByLevel };
    }

    private record PublicIdMappings
    {
        public Dictionary<GeographicLevel, Dictionary<string, string>> Levels { get; init; } = [];

        public string? GetPublicIdForCandidate(GeographicLevel level, string candidateKey)
        {
            if (!Levels.TryGetValue(level, out var mappingLevel))
            {
                return null;
            }

            return mappingLevel.GetValueOrDefault(candidateKey);
        }
    }
}
