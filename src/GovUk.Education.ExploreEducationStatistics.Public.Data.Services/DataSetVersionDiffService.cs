using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Services;

public class DataSetVersionDiffService(PublicDataDbContext publicDataDbContext) : IDataSetVersionDiffService
{
    private static readonly MappingType[] NoMappingTypes =
    [
        MappingType.ManualNone,
        MappingType.AutoNone
    ];

    public async Task<bool> IsMajorVersionUpdate(
        Guid targetDataSetVersionId,
        CancellationToken cancellationToken)
    {
        var mappings = await publicDataDbContext
            .DataSetVersionMappings
            .Include(mapping => mapping.TargetDataSetVersion)
            .Where(mapping => mapping.TargetDataSetVersionId == targetDataSetVersionId)
            .SingleAsync(cancellationToken);

        var hasDeletedLocationLevels = await HasDeletedLocationLevels(targetDataSetVersionId, cancellationToken);

        var hasMajorLocationChange = mappings.LocationMappingPlan
            .Levels
            .Any(
                level => level.Value.Candidates.Count == 0
                         || level.Value.Mappings
                             .Any(optionMapping => NoMappingTypes.Contains(optionMapping.Value.Type))
            );

        var hasMajorFilterUpdate = mappings.FilterMappingPlan
            .Mappings
            .SelectMany(filterMapping => filterMapping.Value.OptionMappings)
            .Any(optionMapping => NoMappingTypes.Contains(optionMapping.Value.Type));

        return hasDeletedLocationLevels;
    }

    private async Task<bool> HasDeletedLocationLevels(
        Guid targetDataSetVersionId,
        CancellationToken cancellationToken)
    {
        var targetDataSetVersionIdParam = new NpgsqlParameter("targetDataSetVersionId", targetDataSetVersionId);

        var deletedLevelCount = await publicDataDbContext.Database
            .SqlQueryRaw<int>(
                $$$"""
                   SELECT DISTINCT COUNT(Level.key) "Value"
                   FROM "{{{nameof(PublicDataDbContext.DataSetVersionMappings)}}}" Mapping,
                        jsonb_each(Mapping."{{{nameof(DataSetVersionMapping.LocationMappingPlan)}}}"
                                       -> '{{{nameof(LocationMappingPlan.Levels)}}}') Level
                   WHERE "{{{nameof(DataSetVersionMapping.TargetDataSetVersionId)}}}" = @targetDataSetVersionId
                     AND Level.value -> '{{{nameof(LocationLevelMappings.Candidates)}}}' = '{{}}'::jsonb
                   """,
                parameters: [targetDataSetVersionIdParam]
            )
            .FirstAsync(cancellationToken);

        return deletedLevelCount > 0;
    }

    private async Task<List<FilterAndOptionMappingTypes>> GetFilterAndOptionMappingTypes(
        Guid targetDataSetVersionId,
        CancellationToken cancellationToken)
    {
        var targetDataSetVersionIdParam = new NpgsqlParameter("targetDataSetVersionId", targetDataSetVersionId);

        // Find all the distinct combinations of parent filters' mapping types against the distinct
        // mapping types of their children.
        return await publicDataDbContext.Database
            .SqlQueryRaw<FilterAndOptionMappingTypes>(
                $"""
                 SELECT DISTINCT 
                     FilterMappingType "{nameof(FilterAndOptionMappingTypes.FilterMappingTypeRaw)}",
                     OptionMappingType "{nameof(FilterAndOptionMappingTypes.OptionMappingTypeRaw)}" 
                 FROM (
                     SELECT FilterMappingType, 
                            OptionMappingType 
                     FROM 
                         "{nameof(PublicDataDbContext.DataSetVersionMappings)}" Mapping,
                         jsonb_each(Mapping."{nameof(DataSetVersionMapping.FilterMappingPlan)}" 
                                        -> '{nameof(FilterMappingPlan.Mappings)}') FilterMapping,
                         jsonb_each(FilterMapping.value -> '{nameof(FilterMapping.OptionMappings)}') OptionMapping,
                         jsonb_extract_path_text(FilterMapping.value, '{nameof(FilterMapping.Type)}') FilterMappingType,
                         jsonb_extract_path_text(OptionMapping.value, '{nameof(FilterOptionMapping.Type)}') OptionMappingType
                     WHERE "{nameof(DataSetVersionMapping.TargetDataSetVersionId)}" = @targetDataSetVersionId
                 )
                 """,
                parameters: [targetDataSetVersionIdParam])
            .ToListAsync(cancellationToken);
    }

    private record FilterAndOptionMappingTypes
    {
        public required string FilterMappingTypeRaw { get; init; }

        public required string OptionMappingTypeRaw { get; init; }

        public MappingType FilterMappingType => EnumUtil.GetFromEnumValue<MappingType>(FilterMappingTypeRaw);

        public MappingType OptionMappingType => EnumUtil.GetFromEnumValue<MappingType>(OptionMappingTypeRaw);
    }
}
