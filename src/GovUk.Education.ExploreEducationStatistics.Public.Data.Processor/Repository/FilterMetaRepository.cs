using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Options;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using InterpolatedSql.Dapper;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository;

public class FilterMetaRepository(
    PublicDataDbContext publicDataDbContext,
    IOptions<AppOptions> appOptions,
    IDataSetVersionPathResolver dataSetVersionPathResolver) : IFilterMetaRepository
{
    private readonly AppOptions _appOptions = appOptions.Value;

    public async Task<IDictionary<FilterMeta, List<FilterOptionMeta>>> ReadFilterMetas(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        IReadOnlySet<string> allowedColumns,
        CancellationToken cancellationToken = default)
    {
        var metas = await GetFilterMetas(
            duckDbConnection,
            dataSetVersion,
            allowedColumns,
            cancellationToken);

        return await metas
            .ToAsyncEnumerable()
            .ToDictionaryAwaitAsync(
                keySelector: ValueTask.FromResult,
                elementSelector: async meta =>
                    await GetFilterOptionMeta(
                        duckDbConnection,
                        dataSetVersion,
                        meta,
                        cancellationToken),
                cancellationToken);
    }

    public async Task CreateFilterMetas(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        IReadOnlySet<string> allowedColumns,
        CancellationToken cancellationToken = default)
    {
        var publicIdMappings = await CreatePublicIdMappings(dataSetVersion, cancellationToken);

        var currentMetaId = await publicDataDbContext.NextSequenceValue(
            PublicDataDbContext.FilterMetasIdSequence,
            cancellationToken);

        var metas = await GetFilterMetas(
            duckDbConnection,
            dataSetVersion,
            allowedColumns,
            cancellationToken);

        foreach (var meta in metas)
        {
            meta.Id = currentMetaId++;
            meta.PublicId = publicIdMappings.Filters.GetValueOrDefault(meta.Column, SqidEncoder.Encode(meta.Id));
        }

        publicDataDbContext.FilterMetas.AddRange(metas);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);

        // Avoid trying to set to 0 (which only
        // happens synthetically during tests).
        if (currentMetaId > 1)
        {
            await publicDataDbContext.SetSequenceValue(
                PublicDataDbContext.FilterMetasIdSequence,
                currentMetaId - 1,
                cancellationToken
            );
        }

        foreach (var meta in metas)
        {
            var options = await GetFilterOptionMeta(
                duckDbConnection,
                dataSetVersion,
                meta,
                cancellationToken);

            var hasTotal = options.Any(option => option.Label == "Total");
            if (meta.AutoSelectLabel == null && hasTotal)
            {
                // If filter_default in the CSV meta file row is null, FilterMeta.AutoSelectLabel will initially be set
                // to null even if a "Total" option exists. If there is a "Total", we set FilterMeta.AutoSelectLabel
                // here.
                await publicDataDbContext.FilterMetas
                    .AsNoTracking() // to avoid IUpdatedTimestamp - we don't want to set FilterMeta.Updated as part of this UPDATE
                    .Where(fm => fm.Id == meta.Id)
                    .ExecuteUpdateAsync(setters =>
                        setters.SetProperty(fm => fm.AutoSelectLabel, "Total"),
                        cancellationToken: cancellationToken);
            }

            var optionTable = publicDataDbContext.GetTable<FilterOptionMeta>();

            // Merge to only inserting new filter options
            // that don't already exist in the table.
            await optionTable
                .Merge()
                .Using(options)
                .On(
                    o => o.Label,
                    o => o.Label
                )
                .InsertWhenNotMatched()
                .MergeAsync(cancellationToken);

            var currentLinkId = await publicDataDbContext.NextSequenceValue(
                PublicDataDbContext.FilterOptionMetaLinkSequence,
                cancellationToken);

            var current = 0;

            while (current < options.Count)
            {
                var batch = options
                    .Skip(current)
                    .Take(_appOptions.MetaInsertBatchSize)
                    .ToList();

                // Although not necessary for filter options, we've adopted the 'row key'
                // technique that was used for the location meta. This is more for
                // future-proofing if we ever add more columns to the filter options table.
                var batchRowKeys = batch
                    .Select(o => o.Label)
                    .ToHashSet();

                var filterOptionMeta = await optionTable
                    .Where(o =>
                        batchRowKeys.Contains(o.Label))
                    .OrderBy(o => o.Label)
                    .ToListAsync(token: cancellationToken);

                var links = filterOptionMeta
                    .Select(option => new FilterOptionMetaLink
                    {
                        PublicId = CreatePublicIdForFilterOptionMetaLink(
                            publicIdMappings: publicIdMappings,
                            filter: meta,
                            option: option,
                            defaultPublicIdFn: () => SqidEncoder.Encode(currentLinkId++)),
                        MetaId = meta.Id,
                        OptionId = option.Id
                    })
                    .ToList();

                publicDataDbContext.FilterOptionMetaLinks.AddRange(links);
                await publicDataDbContext.SaveChangesAsync(cancellationToken);

                current += _appOptions.MetaInsertBatchSize;
            }

            var insertedLinks = await publicDataDbContext.FilterOptionMetaLinks
                .CountAsync(l => l.MetaId == meta.Id,
                    cancellationToken: cancellationToken);

            if (insertedLinks != options.Count)
            {
                throw new InvalidOperationException(
                    $"Inserted incorrect number of filter option meta links for filter (id = {meta.Id}, column = {meta.Column}). " +
                    $"Inserted: {insertedLinks}, expected: {options.Count}");
            }

            // Avoid trying to set to 0 (which only
            // happens synthetically during tests).
            if (currentLinkId > 1)
            {
                // Increase the sequence only by the amount that we used to generate new PublicIds.
                await publicDataDbContext.SetSequenceValue(
                    PublicDataDbContext.FilterOptionMetaLinkSequence,
                    currentLinkId - 1,
                    cancellationToken
                );
            }
        }
    }

    private async Task<List<FilterMeta>> GetFilterMetas(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        IReadOnlySet<string> allowedColumns,
        CancellationToken cancellationToken)
    {
        var metaRows = await duckDbConnection.SqlBuilder(
                $"""
                 SELECT *
                 FROM '{dataSetVersionPathResolver.CsvMetadataPath(dataSetVersion):raw}'
                 WHERE "col_type" = {MetaFileRow.ColumnType.Filter.ToString()}
                 AND "col_name" IN ({allowedColumns})
                 """)
            .QueryAsync<MetaFileRow>(cancellationToken: cancellationToken);

        return metaRows
            .OrderBy(row => row.Label)
            .Select(row => new FilterMeta
            {
                PublicId = string.Empty,
                Column = row.ColName,
                DataSetVersionId = dataSetVersion.Id,
                Label = row.Label,
                Hint = row.FilterHint ?? string.Empty,
                AutoSelectLabel = row.FilterDefault,
            })
            .ToList();
    }

    private async Task<List<FilterOptionMeta>> GetFilterOptionMeta(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        FilterMeta meta,
        CancellationToken cancellationToken)
    {
        return (await duckDbConnection.SqlBuilder(
                $"""
                 SELECT DISTINCT "{meta.Column:raw}"
                 FROM read_csv('{dataSetVersionPathResolver.CsvDataPath(dataSetVersion):raw}', ALL_VARCHAR = true) AS data
                 WHERE "{meta.Column:raw}" != ''
                 ORDER BY "{meta.Column:raw}"
                 """
            ).QueryAsync<string>(cancellationToken: cancellationToken))
            .Select(
                label => new FilterOptionMeta
                {
                    Label = label,
                })
            .ToList();
    }

    private async Task<PublicIdMappings> CreatePublicIdMappings(
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken)
    {
        var mappings = await publicDataDbContext.DataSetVersionMappings
            .Where(mapping => mapping.TargetDataSetVersionId == dataSetVersion.Id)
            .SingleOrDefaultAsync(token: cancellationToken);

        if (mappings is null)
        {
            return new PublicIdMappings();
        }

        var filterMappings = mappings.FilterMappingPlan
            .Mappings
            .Where(mapping => mapping.Value.Type is MappingType.AutoMapped or MappingType.ManualMapped)
            .ToDictionary(
                filter => filter.Key,
                filter => filter.Value.PublicId);

        var filterOptionMappings = mappings.FilterMappingPlan
            .Mappings
            .ToDictionary(
                filter => filter.Key,
                filter => filter.Value
                    .OptionMappings
                    .Values
                    .Where(mapping => mapping.Type is MappingType.AutoMapped or MappingType.ManualMapped)
                    .ToDictionary(
                        mapping => mapping.CandidateKey!,
                        mapping => mapping.PublicId)
            );

        return new PublicIdMappings
        {
            Filters = filterMappings,
            FilterOptions = filterOptionMappings,
        };
    }

    private static string CreatePublicIdForFilterOptionMetaLink(
        PublicIdMappings publicIdMappings,
        FilterMeta filter,
        FilterOptionMeta option,
        Func<string> defaultPublicIdFn)
    {
        return publicIdMappings
                   .GetPublicIdForFilterOptionCandidate(
                       filterKey: MappingKeyGenerators.Filter(filter),
                       MappingKeyGenerators.FilterOptionMeta(option))
               ?? defaultPublicIdFn.Invoke();
    }

    private record PublicIdMappings
    {
        /// <summary>
        /// Filter public IDs mappings by column.
        /// </summary>
        public Dictionary<string, string> Filters { get; init; } = [];

        /// <summary>
        /// Filter option public ID mappings grouped by filter column.
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> FilterOptions { get; init; } = [];

        public string? GetPublicIdForFilterOptionCandidate(string filterKey, string filterOptionCandidateKey)
        {
            if (!FilterOptions.TryGetValue(filterKey, out var filterOptionMappings))
            {
                return null;
            }

            return filterOptionMappings.GetValueOrDefault(filterOptionCandidateKey);
        }
    }
}
