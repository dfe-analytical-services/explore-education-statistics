using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
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
    IOptions<AppSettingsOptions> appSettingsOptions,
    IDataSetVersionPathResolver dataSetVersionPathResolver) : IFilterMetaRepository
{
    private readonly AppSettingsOptions _appSettingsOptions = appSettingsOptions.Value;
    
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
        var metas = await GetFilterMetas(
            duckDbConnection,
            dataSetVersion,
            allowedColumns,
            cancellationToken);

        publicDataDbContext.FilterMetas.AddRange(metas);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);

        foreach (var meta in metas)
        {
            var options = await GetFilterOptionMeta(
                duckDbConnection,
                dataSetVersion,
                meta,
                cancellationToken);

            var optionTable = publicDataDbContext.GetTable<FilterOptionMeta>();

            // Merge to only inserting new filter options
            // that don't already exist in the table.
            await optionTable
                .Merge()
                .Using(options)
                .On(
                    o => new
                    {
                        o.Label,
                        o.IsAggregate
                    },
                    o => new
                    {
                        o.Label,
                        o.IsAggregate
                    }
                )
                .InsertWhenNotMatched()
                .MergeAsync(cancellationToken);

            var startIndex = await publicDataDbContext.NextSequenceValue(
                PublicDataDbContext.FilterOptionMetaLinkSequence,
                cancellationToken);

            var current = 0;

            while (current < options.Count)
            {
                var batchStartIndex = startIndex + current;
                var batch = options
                    .Skip(current)
                    .Take(_appSettingsOptions.MetaInsertBatchSize)
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
                    .OrderBy(o => o.Label)
                    .Select((option, index) => new FilterOptionMetaLink
                    {
                        PublicId = SqidEncoder.Encode(batchStartIndex + index),
                        MetaId = meta.Id,
                        OptionId = option.Id
                    })
                    .ToListAsync(token: cancellationToken);

                publicDataDbContext.FilterOptionMetaLinks.AddRange(links);
                await publicDataDbContext.SaveChangesAsync(cancellationToken);

                current += _appSettingsOptions.MetaInsertBatchSize;
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

            await publicDataDbContext.SetSequenceValue(
                PublicDataDbContext.FilterOptionMetaLinkSequence,
                startIndex + insertedLinks - 1,
                cancellationToken);
        }
    }

    private async Task<List<FilterMeta>> GetFilterMetas(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        IReadOnlySet<string> allowedColumns,
        CancellationToken cancellationToken)
    {
        return (await duckDbConnection.SqlBuilder(
                    $"""
                     SELECT *
                     FROM '{dataSetVersionPathResolver.CsvMetadataPath(dataSetVersion):raw}'
                     WHERE "col_type" = {MetaFileRow.ColumnType.Filter.ToString()}
                     AND "col_name" IN ({allowedColumns})
                     """)
                .QueryAsync<MetaFileRow>(cancellationToken: cancellationToken)
            )
            .OrderBy(row => row.Label)
            .Select(
                row => new FilterMeta
                {
                    PublicId = row.ColName,
                    DataSetVersionId = dataSetVersion.Id,
                    Label = row.Label,
                    Hint = row.FilterHint ?? string.Empty
                }
            )
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
                 SELECT DISTINCT "{meta.PublicId:raw}"
                 FROM read_csv('{dataSetVersionPathResolver.CsvDataPath(dataSetVersion):raw}', ALL_VARCHAR = true) AS data
                 WHERE "{meta.PublicId:raw}" != ''
                 ORDER BY "{meta.PublicId:raw}"
                 """
            ).QueryAsync<string>(cancellationToken: cancellationToken))
            .Select(
                label => new FilterOptionMeta
                {
                    Label = label,
                    IsAggregate = label == "Total" ? true : null
                }
            )
            .ToList();
    }
}
