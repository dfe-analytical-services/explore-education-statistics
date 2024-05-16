using Dapper;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository;

public class FilterRepository(
    PublicDataDbContext publicDataDbContext,
    IDataSetVersionPathResolver dataSetVersionPathResolver) : IFilterRepository
{
    public async Task CreateFilterMetas(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        IReadOnlySet<string> allowedColumns,
        CancellationToken cancellationToken = default)
    {
        // TODO EES-5097 Limit this to only select rows that are needed
        var metaFileRows = (await duckDbConnection.QueryAsync<MetaFileRow>(
                new CommandDefinition(
                    $"SELECT * FROM '{dataSetVersionPathResolver.CsvMetadataPath(dataSetVersion)}'",
                    cancellationToken: cancellationToken
                )
            ))
            .ToList();

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
                    DataSetVersionId = dataSetVersion.Id,
                    Label = row.Label,
                    Hint = row.FilterHint ?? string.Empty,
                }
            )
            .ToList();

        publicDataDbContext.FilterMetas.AddRange(metas);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);

        foreach (var meta in metas)
        {
            var options = (await duckDbConnection.QueryAsync<string>(
                    new CommandDefinition(
                        $"""
                         SELECT DISTINCT "{meta.PublicId}"
                         FROM read_csv_auto('{dataSetVersionPathResolver.CsvDataPath(dataSetVersion)}', ALL_VARCHAR = true) AS data
                         WHERE "{meta.PublicId}" != ''
                         ORDER BY "{meta.PublicId}"
                         """,
                        cancellationToken
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

            var optionTable = publicDataDbContext.GetTable<FilterOptionMeta>();

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
                        PublicId = SqidEncoder.Encode(batchStartIndex + index),
                        MetaId = meta.Id,
                        OptionId = option.Id
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

    public async Task CreateFilterMetaTable(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default)
    {
        await publicDataDbContext
            .Entry(dataSetVersion)
            .Collection(dsv => dsv.FilterMetas)
            .Query()
            .Include(m => m.Options)
            .LoadAsync(cancellationToken);

        await duckDbConnection.ExecuteAsync(
            $"""
             CREATE TABLE {FilterOptionsTable.TableName}(
                 {FilterOptionsTable.Cols.Id} INTEGER PRIMARY KEY,
                 {FilterOptionsTable.Cols.Label} VARCHAR,
                 {FilterOptionsTable.Cols.PublicId} VARCHAR,
                 {FilterOptionsTable.Cols.FilterId} VARCHAR
             )
             """
        );

        var id = 1;

        foreach (var filter in dataSetVersion.FilterMetas)
        {
            using var appender = duckDbConnection.CreateAppender(table: FilterOptionsTable.TableName);

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
}
