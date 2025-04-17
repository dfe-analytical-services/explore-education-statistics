using GovUk.Education.ExploreEducationStatistics.Common.DuckDb.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using InterpolatedSql.Dapper;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Repository;

public class ParquetIndicatorRepository(
    IDuckDbConnection duckDbConnection,
    IDataSetVersionPathResolver dataSetVersionPathResolver)
    : IParquetIndicatorRepository
{
    public async Task<Dictionary<string, string>> GetColumnsById(
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default)
    {
        var command = duckDbConnection.SqlBuilder(
            $"""
             SELECT DISTINCT 
                {IndicatorsTable.Cols.Id:raw}, 
                {IndicatorsTable.Cols.Column:raw}
             FROM '{dataSetVersionPathResolver.IndicatorsPath(dataSetVersion):raw}'
             """);

        var indicators = await command
            .QueryAsync<(string Id, string Column)>(cancellationToken: cancellationToken);

        return indicators.ToDictionary(
            tuple => tuple.Id,
            tuple => tuple.Column
        );
    }
}
