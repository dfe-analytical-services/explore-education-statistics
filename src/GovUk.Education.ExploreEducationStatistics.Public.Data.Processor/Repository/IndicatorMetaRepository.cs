using GovUk.Education.ExploreEducationStatistics.Common.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using InterpolatedSql.Dapper;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository;

public class IndicatorMetaRepository(
    PublicDataDbContext publicDataDbContext,
    IDataSetVersionPathResolver dataSetVersionPathResolver
) : IIndicatorMetaRepository
{
    public async Task<IList<IndicatorMeta>> ReadIndicatorMetas(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        IReadOnlySet<string> allowedColumns,
        CancellationToken cancellationToken = default
    )
    {
        return await GetIndicatorMetas(duckDbConnection, dataSetVersion, allowedColumns, cancellationToken);
    }

    public async Task CreateIndicatorMetas(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        IReadOnlySet<string> allowedColumns,
        CancellationToken cancellationToken = default
    )
    {
        var publicIdMappings = await GetPublicIdMappings(dataSetVersion, cancellationToken);

        var metas = await GetIndicatorMetas(duckDbConnection, dataSetVersion, allowedColumns, cancellationToken);

        var currentId = await publicDataDbContext.NextSequenceValue(
            PublicDataDbContext.IndicatorMetasIdSequence,
            cancellationToken
        );

        foreach (var meta in metas)
        {
            meta.Id = currentId++;
            meta.PublicId = GetOrCreatePublicIdForIndicatorMeta(publicIdMappings: publicIdMappings, indicator: meta);
        }

        publicDataDbContext.IndicatorMetas.AddRange(metas);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);

        // Increase the sequence only by the amount that we used to generate new PublicIds.
        await publicDataDbContext.SetSequenceValue(
            PublicDataDbContext.IndicatorMetasIdSequence,
            currentId - 1,
            cancellationToken
        );
    }

    private static string GetOrCreatePublicIdForIndicatorMeta(
        PublicIdMappings publicIdMappings,
        IndicatorMeta indicator
    )
    {
        return publicIdMappings.GetPublicIdForIndicatorCandidate(
                indicatorCandidateKey: MappingKeyGenerators.IndicatorMeta(indicator)
            ) ?? SqidEncoder.Encode(indicator.Id);
    }

    private async Task<PublicIdMappings> GetPublicIdMappings(
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken
    )
    {
        var mappings = await publicDataDbContext
            .DataSetVersionMappings.Where(mapping => mapping.TargetDataSetVersionId == dataSetVersion.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (mappings is null)
        {
            return new PublicIdMappings();
        }

        var indicatorMappings = mappings
            .IndicatorMappingPlan.Mappings.Values.Where(mapping =>
                mapping.Type is MappingType.AutoMapped or MappingType.ManualMapped
            )
            .ToDictionary(mapping => mapping.CandidateKey!, mapping => mapping.PublicId);

        return new PublicIdMappings { Indicators = indicatorMappings };
    }

    private async Task<List<IndicatorMeta>> GetIndicatorMetas(
        IDuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        IReadOnlySet<string> allowedColumns,
        CancellationToken cancellationToken
    )
    {
        var metaRows = await duckDbConnection
            .SqlBuilder(
                $"""
                SELECT *
                FROM '{dataSetVersionPathResolver.CsvMetadataPath(dataSetVersion):raw}'
                WHERE "col_type" = {MetaFileRow.ColumnType.Indicator.ToString()}
                AND "col_name" IN ({allowedColumns})
                """
            )
            .QueryAsync<MetaFileRow>(cancellationToken: cancellationToken);

        return metaRows
            .OrderBy(row => row.Label)
            .Select(row => new IndicatorMeta
            {
                DataSetVersionId = dataSetVersion.Id,
                PublicId = string.Empty,
                Column = row.ColName,
                Label = row.Label,
                Unit = row.ParsedIndicatorUnit,
                DecimalPlaces = row.IndicatorDp,
            })
            .ToList();
    }

    private record PublicIdMappings
    {
        /// <summary>
        /// Indicator public IDs mappings by column.
        /// </summary>
        public Dictionary<string, string> Indicators { get; init; } = [];

        public string? GetPublicIdForIndicatorCandidate(string indicatorCandidateKey)
        {
            return Indicators.GetValueOrDefault(indicatorCandidateKey);
        }
    }
}
