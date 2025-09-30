using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using InterpolatedSql.Dapper;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository;

public class TimePeriodMetaRepository(
    PublicDataDbContext publicDataDbContext,
    IDataSetVersionPathResolver dataSetVersionPathResolver
) : ITimePeriodMetaRepository
{
    public Task<List<TimePeriodMeta>> ReadTimePeriodMetas(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default
    )
    {
        return GetTimePeriodMetas(duckDbConnection, dataSetVersion, cancellationToken);
    }

    public async Task<List<TimePeriodMeta>> CreateTimePeriodMetas(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default
    )
    {
        var metas = await GetTimePeriodMetas(duckDbConnection, dataSetVersion, cancellationToken);

        publicDataDbContext.TimePeriodMetas.AddRange(metas);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);

        return metas;
    }

    private async Task<List<TimePeriodMeta>> GetTimePeriodMetas(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken
    )
    {
        return (
            await duckDbConnection
                .SqlBuilder(
                    $"""
                    SELECT DISTINCT time_period, time_identifier
                    FROM read_csv('{dataSetVersionPathResolver.CsvDataPath(
                        dataSetVersion
                    ):raw}', ALL_VARCHAR = true)
                    ORDER BY time_period
                    """
                )
                .QueryAsync<(string TimePeriod, string TimeIdentifier)>(
                    cancellationToken: cancellationToken
                )
        )
            .Select(tuple => new TimePeriodMeta
            {
                DataSetVersionId = dataSetVersion.Id,
                Period = TimePeriodFormatter.FormatFromCsv(tuple.TimePeriod),
                Code = EnumToEnumLabelConverter<TimeIdentifier>.FromProvider(tuple.TimeIdentifier),
            })
            .OrderBy(meta => meta.Period)
            .ThenBy(meta => meta.Code)
            .ToList();
    }
}
