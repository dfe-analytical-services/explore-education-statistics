using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
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
        var currentId = await publicDataDbContext.NextSequenceValue(
            PublicDataDbContext.IndicatorMetasIdSequence,
            cancellationToken);

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
            .Select(row =>
            {
                var id = currentId++;

                return new IndicatorMeta
                {
                    Id = id,
                    DataSetVersionId = dataSetVersion.Id,
                    PublicId = SqidEncoder.Encode(id),
                    Column = row.ColName,
                    Label = row.Label,
                    Unit = row.IndicatorUnit,
                    DecimalPlaces = row.IndicatorDp
                };
            })
            .ToList();

        publicDataDbContext.IndicatorMetas.AddRange(metas);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);

        // Increase the sequence only by the amount that we used to generate new PublicIds.
        await publicDataDbContext.SetSequenceValue(
            PublicDataDbContext.IndicatorMetasIdSequence,
            currentId - 1,
            cancellationToken);
    }
}
