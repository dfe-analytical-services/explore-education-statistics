using GovUk.Education.ExploreEducationStatistics.Common.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;
using InterpolatedSql.Dapper;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository;

public class IndicatorsDuckDbRepository(PublicDataDbContext publicDataDbContext) : IIndicatorsDuckDbRepository
{
    public async Task CreateIndicatorsTable(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default
    )
    {
        await publicDataDbContext
            .Entry(dataSetVersion)
            .Collection(dsv => dsv.IndicatorMetas)
            .LoadAsync(cancellationToken);

        await duckDbConnection
            .SqlBuilder(
                $"""
                CREATE TABLE {IndicatorsTable.TableName:raw}(
                    {IndicatorsTable.Cols.Id:raw} VARCHAR PRIMARY KEY,
                    {IndicatorsTable.Cols.Column:raw} VARCHAR,
                    {IndicatorsTable.Cols.Label:raw} VARCHAR,
                    {IndicatorsTable.Cols.Unit:raw} VARCHAR,
                    {IndicatorsTable.Cols.DecimalPlaces:raw} TINYINT,
                )
                """
            )
            .ExecuteAsync(cancellationToken: cancellationToken);

        using var appender = duckDbConnection.CreateAppender(table: IndicatorsTable.TableName);

        foreach (var meta in dataSetVersion.IndicatorMetas)
        {
            var insertRow = appender.CreateRow();

            insertRow.AppendValue(meta.PublicId);
            insertRow.AppendValue(meta.Column);
            insertRow.AppendValue(meta.Label);
            insertRow.AppendValue(meta.Unit?.GetEnumLabel() ?? string.Empty);
            insertRow.AppendValue(meta.DecimalPlaces);
            insertRow.EndRow();
        }
    }
}
