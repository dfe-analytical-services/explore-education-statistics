using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using InterpolatedSql.Dapper;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository;

public class IndicatorMetaRepository(
    PublicDataDbContext publicDataDbContext,
    IDataSetVersionPathResolver dataSetVersionPathResolver) : IIndicatorMetaRepository
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
}
