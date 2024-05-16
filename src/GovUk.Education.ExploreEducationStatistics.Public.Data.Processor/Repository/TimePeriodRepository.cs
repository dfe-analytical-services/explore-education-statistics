using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using InterpolatedSql.Dapper;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository;

public class TimePeriodRepository(
    PublicDataDbContext publicDataDbContext,
    IDataSetVersionPathResolver dataSetVersionPathResolver) : ITimePeriodRepository
{
    public async Task<List<TimePeriodMeta>> CreateTimePeriodMetas(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default)
    {
        var metas = (await duckDbConnection.SqlBuilder(
                $"""
                 SELECT DISTINCT time_period, time_identifier
                 FROM read_csv('{dataSetVersionPathResolver.CsvDataPath(dataSetVersion):raw}', ALL_VARCHAR = true)
                 ORDER BY time_period
                 """
            ).QueryAsync<(string TimePeriod, string TimeIdentifier)>(cancellationToken: cancellationToken))
            .Select(tuple => new TimePeriodMeta
                {
                    DataSetVersionId = dataSetVersion.Id,
                    Period = TimePeriodFormatter.FormatFromCsv(tuple.TimePeriod),
                    Code = EnumToEnumLabelConverter<TimeIdentifier>.FromProvider(tuple.TimeIdentifier)
                }
            )
            .OrderBy(meta => meta.Period)
            .ThenBy(meta => meta.Code)
            .ToList();

        publicDataDbContext.TimePeriodMetas.AddRange(metas);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);

        return metas;
    }

    public async Task CreateTimePeriodMetaTable(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default)
    {
        await publicDataDbContext
            .Entry(dataSetVersion)
            .Collection(dsv => dsv.TimePeriodMetas)
            .LoadAsync(cancellationToken);

        await duckDbConnection.SqlBuilder(
            $"""
             CREATE TABLE {TimePeriodsTable.TableName:raw}(
                 {TimePeriodsTable.Cols.Id:raw} INTEGER PRIMARY KEY,
                 {TimePeriodsTable.Cols.Period:raw} VARCHAR,
                 {TimePeriodsTable.Cols.Identifier:raw} VARCHAR
             )
             """
        ).ExecuteAsync(cancellationToken: cancellationToken);

        using var appender = duckDbConnection.CreateAppender(table: TimePeriodsTable.TableName);

        var timePeriods = dataSetVersion.TimePeriodMetas
            .OrderBy(tp => tp.Period)
            .ThenBy(tp => tp.Code);

        var id = 1;

        foreach (var timePeriod in timePeriods)
        {
            var insertRow = appender.CreateRow();

            insertRow.AppendValue(id++);
            insertRow.AppendValue(TimePeriodFormatter.FormatToCsv(timePeriod.Period));
            insertRow.AppendValue(timePeriod.Code.GetEnumLabel());
            insertRow.EndRow();
        }
    }
}
