using Dapper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository;

public class IndicatorRepository(
    PublicDataDbContext publicDataDbContext,
    IDataSetVersionPathResolver dataSetVersionPathResolver) : IIndicatorRepository
{
    public async Task CreateIndicatorMetas(
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
            .Where(row => row.ColType == MetaFileRow.ColumnType.Indicator
                          && allowedColumns.Contains(row.ColName))
            .OrderBy(row => row.Label)
            .ToList()
            .Select(
                row => new IndicatorMeta
                {
                    DataSetVersionId = dataSetVersion.Id,
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

    public async Task CreateIndicatorMetaTable(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default)
    {
        await publicDataDbContext
            .Entry(dataSetVersion)
            .Collection(dsv => dsv.IndicatorMetas)
            .LoadAsync(cancellationToken);

        await duckDbConnection.ExecuteAsync(
            $"""
             CREATE TABLE {IndicatorsTable.TableName}(
                 {IndicatorsTable.Cols.Id} VARCHAR PRIMARY KEY,
                 {IndicatorsTable.Cols.Label} VARCHAR,
                 {IndicatorsTable.Cols.Unit} VARCHAR,
                 {IndicatorsTable.Cols.DecimalPlaces} TINYINT,
             )
             """
        );

        using var appender = duckDbConnection.CreateAppender(table: IndicatorsTable.TableName);

        foreach (var meta in dataSetVersion.IndicatorMetas)
        {
            var insertRow = appender.CreateRow();

            insertRow.AppendValue(meta.PublicId);
            insertRow.AppendValue(meta.Label);
            insertRow.AppendValue(meta.Unit?.GetEnumLabel() ?? string.Empty);
            insertRow.AppendValue(meta.DecimalPlaces);
            insertRow.EndRow();
        }
    }
}
