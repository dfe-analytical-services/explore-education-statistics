using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Repositories.Public.Data.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Repositories.Public.Data;

public class MappingTypesRepository(PublicDataDbContext context) : IMappingTypesRepository
{
    private readonly PublicDataDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<List<LocationMappingTypes>> GetLocationOptionMappingTypes(
        Guid targetDataSetVersionId,
        CancellationToken cancellationToken = default)
    {
        var targetDataSetVersionIdParam = new NpgsqlParameter("targetDataSetVersionId", targetDataSetVersionId);

        // Find the distinct mapping types for location options across location levels
        // that still have candidates (and haven't been deleted).
        return await _context.Database
            .SqlQueryRaw<LocationMappingTypes>(
                $$$"""
                   SELECT DISTINCT
                       CASE WHEN Level.value -> '{{{nameof(LocationLevelMappings.Candidates)}}}' = '{{}}' 
                           THEN '{{{nameof(MappingType.AutoNone)}}}'
                           ELSE '{{{nameof(MappingType.AutoMapped)}}}'
                       END "{{{nameof(LocationMappingTypes.LocationLevelRaw)}}}",
                       OptionMappingType "{{{nameof(LocationMappingTypes.LocationOptionRaw)}}}"
                   FROM
                       "{{{nameof(PublicDataDbContext.DataSetVersionMappings)}}}" Mapping,
                       jsonb_each(Mapping."{{{nameof(DataSetVersionMapping.LocationMappingPlan)}}}"
                                    -> '{{{nameof(LocationMappingPlan.Levels)}}}') Level,
                       jsonb_each(Level.value -> '{{{nameof(LocationLevelMappings.Mappings)}}}') OptionMapping,
                       jsonb_extract_path_text(OptionMapping.value, '{{{nameof(LocationOptionMapping.Type)}}}') OptionMappingType
                   WHERE "{{{nameof(DataSetVersionMapping.TargetDataSetVersionId)}}}" = @targetDataSetVersionId
                   """,
                parameters: [targetDataSetVersionIdParam])
            .ToListAsync(cancellationToken);
    }

    public async Task<List<FilterMappingTypes>> GetFilterOptionMappingTypes(Guid targetDataSetVersionId, CancellationToken cancellationToken = default)
    {
        var targetDataSetVersionIdParam = new NpgsqlParameter("targetDataSetVersionId", targetDataSetVersionId);

        // Find all the distinct combinations of parent filters' mapping types against the distinct
        // mapping types of their children.
        return await _context.Database
            .SqlQueryRaw<FilterMappingTypes>(
                $"""
                 SELECT DISTINCT 
                     FilterMappingType "{nameof(FilterMappingTypes.FilterRaw)}", 
                     OptionMappingType "{nameof(FilterMappingTypes.FilterOptionRaw)}" 
                 FROM 
                     "{nameof(PublicDataDbContext.DataSetVersionMappings)}" Mapping,
                     jsonb_each(Mapping."{nameof(DataSetVersionMapping.FilterMappingPlan)}" 
                                    -> '{nameof(FilterMappingPlan.Mappings)}') FilterMapping,
                     jsonb_each(FilterMapping.value -> '{nameof(FilterMapping.OptionMappings)}') OptionMapping,
                     jsonb_extract_path_text(FilterMapping.value, '{nameof(FilterMapping.Type)}') FilterMappingType,
                     jsonb_extract_path_text(OptionMapping.value, '{nameof(FilterOptionMapping.Type)}') OptionMappingType
                 WHERE "{nameof(DataSetVersionMapping.TargetDataSetVersionId)}" = @targetDataSetVersionId
                 """,
                parameters: [targetDataSetVersionIdParam])
            .ToListAsync(cancellationToken);
    }
}
