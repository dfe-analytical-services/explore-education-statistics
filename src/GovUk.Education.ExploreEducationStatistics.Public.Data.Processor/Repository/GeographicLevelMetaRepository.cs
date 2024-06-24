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
    IDataSetVersionPathResolver dataSetVersionPathResolver) : IGeographicLevelMetaRepository
{
    public async Task<GeographicLevelMeta> CreateGeographicLevelMeta(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default)
    {
        var geographicLevels =
            (await duckDbConnection.SqlBuilder(
                $"""
                 SELECT DISTINCT geographic_level
                 FROM read_csv('{dataSetVersionPathResolver.CsvDataPath(dataSetVersion):raw}', ALL_VARCHAR = true)
                 """
            ).QueryAsync<string>(cancellationToken: cancellationToken))
            .Select(EnumToEnumLabelConverter<GeographicLevel>.FromProvider)
            .OrderBy(EnumToEnumLabelConverter<GeographicLevel>.ToProvider)
            .ToList();

        var meta = new GeographicLevelMeta
        {
            DataSetVersionId = dataSetVersion.Id,
            Levels = geographicLevels
        };

        publicDataDbContext.GeographicLevelMetas.Add(meta);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);

        return meta;
    }
}
