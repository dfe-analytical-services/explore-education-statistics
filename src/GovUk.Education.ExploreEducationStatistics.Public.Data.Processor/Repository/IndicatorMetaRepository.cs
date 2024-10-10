using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using InterpolatedSql.Dapper;
using Microsoft.EntityFrameworkCore;

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
        var sourceDataSetVersionId = await GetSourceDataSetVersionId(dataSetVersion, cancellationToken);

        var existingMetaIdsByColumn = sourceDataSetVersionId is not null
            ? await publicDataDbContext.IndicatorMetas
                .Where(meta => meta.DataSetVersionId == sourceDataSetVersionId)
                .ToDictionaryAsync(
                    meta => meta.Column,
                    meta => meta.PublicId,
                    cancellationToken
                )
            : [];

        var currentId = await publicDataDbContext.NextSequenceValue(
            PublicDataDbContext.IndicatorMetasIdSequence,
            cancellationToken);

        var metaRows = await duckDbConnection.SqlBuilder(
                $"""
                 SELECT *
                 FROM '{dataSetVersionPathResolver.CsvMetadataPath(dataSetVersion):raw}'
                 WHERE "col_type" = {MetaFileRow.ColumnType.Indicator.ToString()}
                 AND "col_name" IN ({allowedColumns})
                 """)
            .QueryAsync<MetaFileRow>(cancellationToken: cancellationToken);

        var metas = metaRows
            .OrderBy(row => row.Label)
            .Select(row =>
            {
                var id = currentId++;

                return new IndicatorMeta
                {
                    Id = id,
                    DataSetVersionId = dataSetVersion.Id,
                    PublicId = existingMetaIdsByColumn.GetValueOrDefault(row.ColName, SqidEncoder.Encode(id)),
                    Column = row.ColName,
                    Label = row.Label,
                    Unit = row.ParsedIndicatorUnit,
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

    private async Task<Guid?> GetSourceDataSetVersionId(
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken)
    {
        return await publicDataDbContext.DataSetVersionMappings
            .Where(mapping => mapping.TargetDataSetVersionId == dataSetVersion.Id)
            .Select(mapping => mapping.SourceDataSetVersionId)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
