using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using InterpolatedSql.Dapper;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Repository;

public class ParquetIndicatorRepository(
    IDuckDbConnection duckDbConnection,
    IDataSetVersionPathResolver dataSetVersionPathResolver)
    : IParquetIndicatorRepository
{
    public async Task<ISet<string>> ListIds(
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default)
    {
        var command = duckDbConnection.SqlBuilder(
            $"""
             SELECT DISTINCT {IndicatorsTable.Cols.Id:raw}
             FROM '{dataSetVersionPathResolver.IndicatorsPath(dataSetVersion):raw}'
             """);

        var indicators = await command.QueryAsync<string>(cancellationToken: cancellationToken);

        return indicators.ToHashSet();
    }
}
