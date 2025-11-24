using Dapper;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using InterpolatedSql.Dapper;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Repository;

public class ParquetTimePeriodRepository(
    IDuckDbConnection duckDbConnection,
    IDataSetVersionPathResolver dataSetVersionPathResolver
) : IParquetTimePeriodRepository
{
    public async Task<IList<ParquetTimePeriod>> List(
        DataSetVersion dataSetVersion,
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default
    )
    {
        var idsList = ids.ToList();

        if (idsList.Count == 0)
        {
            return new List<ParquetTimePeriod>();
        }

        var command = duckDbConnection.SqlBuilder(
            $"""
            SELECT *
            FROM '{dataSetVersionPathResolver.TimePeriodsPath(dataSetVersion):raw}'
            WHERE {TimePeriodsTable.Cols.Id:raw} IN ({idsList})
            """
        );

        return (await command.QueryAsync<ParquetTimePeriod>(cancellationToken: cancellationToken)).AsList();
    }

    public async Task<IList<ParquetTimePeriod>> List(
        DataSetVersion dataSetVersion,
        IEnumerable<DataSetQueryTimePeriod> timePeriods,
        CancellationToken cancellationToken = default
    )
    {
        var timePeriodsList = timePeriods.ToList();

        if (timePeriodsList.Count == 0)
        {
            return new List<ParquetTimePeriod>();
        }

        var inFragment = new DuckDbSqlBuilder().AppendRange(
            timePeriodsList.Select(tp =>
                (FormattableString)
                    $"({TimePeriodFormatter.FormatToCsv(tp.ParsedPeriod())}, {tp.ParsedCode().GetEnumLabel()})"
            ),
            joinString: ", "
        );

        var command = duckDbConnection.SqlBuilder(
            $"""
            SELECT *
            FROM '{dataSetVersionPathResolver.TimePeriodsPath(dataSetVersion):raw}'
            WHERE ({TimePeriodsTable.Cols.Period:raw}, {TimePeriodsTable.Cols.Identifier:raw})
               IN ({inFragment})
            """
        );

        return (await command.QueryAsync<ParquetTimePeriod>(cancellationToken: cancellationToken)).AsList();
    }
}
