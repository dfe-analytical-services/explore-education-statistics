using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using InterpolatedSql.Dapper;

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
        var metas = (await duckDbConnection.SqlBuilder(
                    $"""
                     SELECT *
                     FROM '{dataSetVersionPathResolver.CsvMetadataPath(dataSetVersion):raw}'
                     WHERE "col_type" = {MetaFileRow.ColumnType.Indicator.ToString()}
                     AND "col_name" IN ({allowedColumns})
                     """)
                .QueryAsync<MetaFileRow>(cancellationToken: cancellationToken)
            )
            .OrderBy(row => row.Label)
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

        await duckDbConnection.SqlBuilder(
            $"""
             CREATE TABLE {IndicatorsTable.TableName:raw}(
                 {IndicatorsTable.Cols.Id:raw} VARCHAR PRIMARY KEY,
                 {IndicatorsTable.Cols.Label:raw} VARCHAR,
                 {IndicatorsTable.Cols.Unit:raw} VARCHAR,
                 {IndicatorsTable.Cols.DecimalPlaces:raw} TINYINT,
             )
             """
        ).ExecuteAsync(cancellationToken: cancellationToken);

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
