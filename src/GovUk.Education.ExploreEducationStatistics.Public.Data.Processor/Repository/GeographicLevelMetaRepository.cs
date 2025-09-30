using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using InterpolatedSql.Dapper;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository;

public class GeographicLevelMetaRepository(
    PublicDataDbContext publicDataDbContext,
    IDataSetVersionPathResolver dataSetVersionPathResolver
) : IGeographicLevelMetaRepository
{
    public Task<GeographicLevelMeta> ReadGeographicLevelMeta(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default
    )
    {
        return GetGeographicLevelMeta(duckDbConnection, dataSetVersion, cancellationToken);
    }

    public async Task<GeographicLevelMeta> CreateGeographicLevelMeta(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default
    )
    {
        var meta = await GetGeographicLevelMeta(
            duckDbConnection,
            dataSetVersion,
            cancellationToken
        );

        publicDataDbContext.GeographicLevelMetas.Add(meta);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);

        return meta;
    }

    private async Task<GeographicLevelMeta> GetGeographicLevelMeta(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken
    )
    {
        var geographicLevels = (
            await duckDbConnection
                .SqlBuilder(
                    $"""
                    SELECT DISTINCT geographic_level
                    FROM read_csv('{dataSetVersionPathResolver.CsvDataPath(
                        dataSetVersion
                    ):raw}', ALL_VARCHAR = true)
                    """
                )
                .QueryAsync<string>(cancellationToken: cancellationToken)
        )
            .Select(EnumToEnumLabelConverter<GeographicLevel>.FromProvider)
            .OrderBy(EnumToEnumLabelConverter<GeographicLevel>.ToProvider)
            .ToList();

        return new GeographicLevelMeta
        {
            DataSetVersionId = dataSetVersion.Id,
            Levels = geographicLevels,
        };
    }
}
