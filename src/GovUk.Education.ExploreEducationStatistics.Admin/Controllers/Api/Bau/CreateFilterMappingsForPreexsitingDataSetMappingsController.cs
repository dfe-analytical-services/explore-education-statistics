#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using LinqToDB.Internal.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

[Route("api/bau")]
[ApiController]
[Authorize(Roles = RoleNames.BauUser)]
public class CreateFilterMappingsForPreexistingDataSetMappingsController(
    ContentDbContext contentDbContext,
    StatisticsDbContext statisticsDbContext
) : ControllerBase
{
    [HttpPut("create-filter-mappings")]
    public async Task<ActionResult> CreateFilterMappings(CancellationToken cancellationToken = default)
    {
        var dataSetMappings = contentDbContext.DataSetMappings.ToList();

        foreach (var mapping in dataSetMappings)
        {
            var originalSubjectId = await contentDbContext
                .Files.Where(f =>
                    f.Id == mapping.OriginalDataFileId
                    && f.Type == FileType.Data
                    && mapping.FilterMappings.IsNullOrEmpty()
                )
                .Select(f => f.SubjectId!.Value)
                .SingleAsync(cancellationToken);

            var replacementSubjectId = await contentDbContext
                .Files.Where(f =>
                    f.Id == mapping.ReplacementDataFileId
                    && f.Type == FileType.Data
                    && mapping.FilterMappings.IsNullOrEmpty()
                )
                .Select(f => f.SubjectId!.Value)
                .SingleAsync(cancellationToken);

            // NOTE: Basically copied from DataSetMappingService.GenerateInitialFilterMapping, although we save
            // filterMappings/replacement candidates rather than return them
            var originalFilters = await statisticsDbContext
                .Filter.AsNoTracking()
                .Include(f => f.FilterGroups)
                    .ThenInclude(fg => fg.FilterItems)
                .Where(f => f.SubjectId == originalSubjectId)
                .ToListAsync(cancellationToken);

            var replacementFilters = await statisticsDbContext
                .Filter.AsNoTracking()
                .Include(f => f.FilterGroups)
                    .ThenInclude(fg => fg.FilterItems)
                .Where(f => f.SubjectId == replacementSubjectId)
                .ToListAsync(cancellationToken);

            // The complete replacement-side catalogue, captured once here and never mutated afterwards.
            var replacementFilterCandidates = replacementFilters
                .Select(filter => new ReplacementFilter
                {
                    Id = filter.Id,
                    Label = filter.Label,
                    ColumnName = filter.Name,
                })
                .ToList();

            var replacementGroupCandidates = replacementFilters
                .SelectMany(filter =>
                    filter.FilterGroups.Select(group => new ReplacementFilterGroup
                    {
                        Id = group.Id,
                        FilterId = filter.Id,
                        Label = group.Label,
                    })
                )
                .ToList();

            var replacementItemCandidates = replacementFilters
                .SelectMany(filter => filter.FilterGroups)
                .SelectMany(group =>
                    group.FilterItems.Select(item => new ReplacementFilterItem
                    {
                        Id = item.Id,
                        FilterGroupId = group.Id,
                        Label = item.Label,
                    })
                )
                .ToList();

            // Create dictionaries to speed up performance when matching originals to replacements
            var replacementFiltersMap = replacementFilters.ToDictionary(f => f.Name, f => f); // automap filters by column name

            var replacementFilterIdToGroupLabelToGroupMap = replacementFilters
                .SelectMany(f => f.FilterGroups.Select(g => new { FilterId = f.Id, FilterGroup = g }))
                .GroupBy(x => x.FilterId)
                .ToDictionary(x => x.Key, x => x.ToDictionary(g => g.FilterGroup.Label, g => g.FilterGroup)); // automap groups by label

            var replacementGroupIdToItemLabelToItemMap = replacementFilters
                .SelectMany(f => f.FilterGroups)
                .SelectMany(g => g.FilterItems.Select(i => new { FilterGroupId = g.Id, FilterItem = i }))
                .GroupBy(x => x.FilterGroupId)
                .ToDictionary(x => x.Key, x => x.ToDictionary(g => g.FilterItem.Label, g => g.FilterItem)); // automap items by label

            // Now we create FilterMappings
            var filterMappings = new Dictionary<Guid, FilterMapping>();
            foreach (var originalFilter in originalFilters)
            {
                replacementFiltersMap.TryGetValue(originalFilter.Name, out var replacementFilter);

                var replacementGroupLabelToGroupMap =
                    replacementFilter != null
                        ? replacementFilterIdToGroupLabelToGroupMap.GetValueOrDefault(replacementFilter.Id)
                        : null;

                var filterGroupMappings = GenerateInitialFilterGroupMapping(
                    originalFilter.FilterGroups,
                    replacementGroupLabelToGroupMap,
                    replacementGroupIdToItemLabelToItemMap
                );

                var filterMapping = new FilterMapping
                {
                    OriginalId = originalFilter.Id,
                    OriginalColumnName = originalFilter.Name,
                    OriginalLabel = originalFilter.Label,

                    ReplacementId = replacementFilter?.Id,
                    ReplacementColumnName = replacementFilter?.Name,
                    ReplacementLabel = replacementFilter?.Label,

                    FilterGroupMappings = filterGroupMappings,

                    Status = replacementFilter == null ? MapStatus.Unset : MapStatus.AutoSet,
                };

                filterMappings.Add(filterMapping.OriginalId, filterMapping);
            }

            mapping.FilterMappings = filterMappings;
            mapping.ReplacementFilters = replacementFilterCandidates;
            mapping.ReplacementFilterGroups = replacementGroupCandidates;
            mapping.ReplacementFilterItems = replacementItemCandidates;
        }

        await contentDbContext.SaveChangesAsync(cancellationToken);
        return Ok("All DataSetMapping.FilterMappings created");
    }

    private static Dictionary<Guid, FilterGroupMapping> GenerateInitialFilterGroupMapping(
        List<FilterGroup> originalFilterGroups,
        Dictionary<string, FilterGroup>? replacementGroupLabelToGroupMap,
        Dictionary<Guid, Dictionary<string, FilterItem>> replacementGroupIdToItemLabelToItemMap
    )
    {
        var filterGroupMappings = new Dictionary<Guid, FilterGroupMapping>();

        foreach (var originalFilterGroup in originalFilterGroups)
        {
            FilterGroup? replacementFilterGroup = null;
            replacementGroupLabelToGroupMap?.TryGetValue(originalFilterGroup.Label, out replacementFilterGroup);

            var replacementItemLabelToItemMap =
                replacementFilterGroup != null
                    ? replacementGroupIdToItemLabelToItemMap.GetValueOrDefault(replacementFilterGroup.Id)
                    : null;
            var filterItemMappings = GenerateInitialFilterItemMapping(
                originalFilterGroup.FilterItems,
                replacementItemLabelToItemMap
            );

            var filterGroupMapping = new FilterGroupMapping
            {
                OriginalId = originalFilterGroup.Id,
                OriginalLabel = originalFilterGroup.Label,

                ReplacementId = replacementFilterGroup?.Id,
                ReplacementLabel = replacementFilterGroup?.Label,

                FilterItemMappings = filterItemMappings,

                Status =
                    replacementGroupLabelToGroupMap == null
                        ? MapStatus.ParentNotMapped
                        : (replacementFilterGroup == null ? MapStatus.Unset : MapStatus.AutoSet),
            };

            filterGroupMappings.Add(filterGroupMapping.OriginalId, filterGroupMapping);
        }

        return filterGroupMappings;
    }

    private static Dictionary<Guid, FilterItemMapping> GenerateInitialFilterItemMapping(
        List<FilterItem> originalFilterItems,
        Dictionary<string, FilterItem>? replacementItemLabelToItemMap
    )
    {
        var filterItemMappings = new Dictionary<Guid, FilterItemMapping>();

        foreach (var originalFilterItem in originalFilterItems)
        {
            FilterItem? replacementFilterItem = null;
            replacementItemLabelToItemMap?.TryGetValue(originalFilterItem.Label, out replacementFilterItem);

            var filterItemMapping = new FilterItemMapping
            {
                OriginalId = originalFilterItem.Id,
                OriginalLabel = originalFilterItem.Label,

                ReplacementId = replacementFilterItem?.Id,
                ReplacementLabel = replacementFilterItem?.Label,

                Status =
                    replacementItemLabelToItemMap == null
                        ? MapStatus.ParentNotMapped
                        : (replacementFilterItem == null ? MapStatus.Unset : MapStatus.AutoSet),
            };

            filterItemMappings.Add(filterItemMapping.OriginalId, filterItemMapping);
        }

        return filterItemMappings;
    }
}
