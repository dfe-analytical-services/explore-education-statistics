#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;

public class DataSetMappingService(IDbContextSupplier dbContextSupplier) : IDataSetMappingService
{
    public async Task CreateInitialDataSetMappingIfReplacement(Guid replacementFileId)
    {
        await using var contentDbContext = dbContextSupplier.CreateDbContext<ContentDbContext>();
        await using var statisticsDbContext = dbContextSupplier.CreateDbContext<StatisticsDbContext>();

        var replacementFile = await contentDbContext
            .Files.Include(f => f.Replacing)
            .SingleOrDefaultAsync(f => f.Id == replacementFileId && f.Type == FileType.Data);

        if (replacementFile?.Replacing == null)
        {
            return; // it's not an ongoing replacement so we don't need to generate a DataSetMappings entry
        }

        var originalFile = replacementFile.Replacing!;

        var indicatorMappings = await GenerateInitialIndicatorMapping(
            statisticsDbContext,
            originalFile.SubjectId!.Value,
            replacementFile.SubjectId!.Value
        );

        var locationMappings = await GenerateInitialLocationMapping(
            statisticsDbContext,
            originalFile.SubjectId!.Value,
            replacementFile.SubjectId!.Value
        );

        var filterMappings = await GenerateInitialFilterMapping(
            statisticsDbContext,
            originalFile.SubjectId!.Value,
            replacementFile.SubjectId!.Value
        );

        var newMapping = new DataSetMapping
        {
            OriginalDataFileId = originalFile.Id,
            ReplacementDataFileId = replacementFile.Id,
            IndicatorMappings = indicatorMappings,
            LocationMappings = locationMappings,
            FilterMappings = filterMappings,
        };

        contentDbContext.DataSetMappings.Add(newMapping);
        await contentDbContext.SaveChangesAsync();
    }

    private async Task<Dictionary<Guid, IndicatorMapping>> GenerateInitialIndicatorMapping(
        StatisticsDbContext statisticsDbContext,
        Guid originalSubjectId,
        Guid replacementSubjectId
    )
    {
        var originalIndicators = await statisticsDbContext
            .Indicator.Include(i => i.IndicatorGroup)
            .Where(i => i.IndicatorGroup.SubjectId == originalSubjectId)
            .ToListAsync();

        var replacementIndicatorNameToIndicatorMap = await statisticsDbContext
            .Indicator.Include(i => i.IndicatorGroup)
            .Where(i => i.IndicatorGroup.SubjectId == replacementSubjectId)
            .ToDictionaryAsync(i => i.Name, i => i);

        var indicatorMappings = originalIndicators.ToDictionary(
            originalIndicator => originalIndicator.Id,
            originalIndicator =>
            {
                // Only if a replacement indicator has the same column name as an original indicator AND the same group
                // label, we auto map it.
                if (
                    replacementIndicatorNameToIndicatorMap.TryGetValue(
                        originalIndicator.Name,
                        out var replacementIndicator
                    )
                )
                {
                    if (replacementIndicator.IndicatorGroup.Label != originalIndicator.IndicatorGroup.Label)
                    {
                        replacementIndicator = null;
                    }
                }

                return new IndicatorMapping
                {
                    OriginalId = originalIndicator.Id,
                    OriginalLabel = originalIndicator.Label,
                    OriginalColumnName = originalIndicator.Name,
                    OriginalGroupId = originalIndicator.IndicatorGroupId,
                    OriginalGroupLabel = originalIndicator.IndicatorGroup.Label,
                    ReplacementId = replacementIndicator?.Id,
                    ReplacementLabel = replacementIndicator?.Label,
                    ReplacementColumnName = replacementIndicator?.Name,
                    ReplacementGroupId = replacementIndicator?.IndicatorGroupId,
                    ReplacementGroupLabel = replacementIndicator?.IndicatorGroup.Label,
                    Status = replacementIndicator == null ? MapStatus.Unset : MapStatus.AutoSet,
                };
            }
        );

        return indicatorMappings;
    }

    private async Task<Dictionary<Guid, LocationMapping>> GenerateInitialLocationMapping(
        StatisticsDbContext statisticsDbContext,
        Guid originalSubjectId,
        Guid replacementSubjectId
    )
    {
        var originalLocations = await statisticsDbContext
            .Observation.AsNoTracking()
            .Where(o => o.SubjectId == originalSubjectId)
            .Select(observation => observation.Location)
            .Distinct()
            .ToListAsync();

        var replacementIdToLocationMap = await statisticsDbContext
            .Observation.AsNoTracking()
            .Where(o => o.SubjectId == replacementSubjectId)
            .Select(observation => observation.Location)
            .Distinct()
            .ToDictionaryAsync(location => location.Id, location => location);

        var locationMappings = originalLocations.ToDictionary(
            originalLocation => originalLocation.Id,
            originalLocation =>
            {
                replacementIdToLocationMap.TryGetValue(originalLocation.Id, out var replacementLocation);
                if (replacementLocation == null)
                {
                    // If none matching by Id, check if any matching by GeogLvl + Code. We don't check by Name to
                    // preserve behaviour from before location mapping was introduced (which allowed analysts to
                    // change/fix location names with replacements).
                    var matchingReplacements = replacementIdToLocationMap
                        .Values.Where(location =>
                            location.GeographicLevel == originalLocation.GeographicLevel
                            && location.ToLocationAttribute().GetCodeOrFallback()
                                == originalLocation.ToLocationAttribute().GetCodeOrFallback()
                        )
                        .ToList();
                    replacementLocation = matchingReplacements.Count == 1 ? matchingReplacements[0] : null;
                }

                return new LocationMapping
                {
                    OriginalId = originalLocation.Id,
                    OriginalCode = originalLocation.ToLocationAttribute().GetCodeOrFallback(),
                    OriginalName = originalLocation.ToLocationAttribute().Name ?? "",
                    OriginalGeographicLevel = originalLocation.GeographicLevel,
                    ReplacementId = replacementLocation?.Id,
                    ReplacementCode = replacementLocation?.ToLocationAttribute().GetCodeOrFallback(),
                    ReplacementName = replacementLocation?.ToLocationAttribute().Name ?? "",
                    ReplacementGeographicLevel = replacementLocation?.GeographicLevel,
                    Status = replacementLocation == null ? MapStatus.Unset : MapStatus.AutoSet,
                };
            }
        );

        return locationMappings;
    }

    private static async Task<Dictionary<Guid, FilterMapping>> GenerateInitialFilterMapping(
        StatisticsDbContext statisticsDbContext,
        Guid originalSubjectId,
        Guid replacementSubjectId
    )
    {
        var filters = await statisticsDbContext
            .Filter.AsNoTracking()
            .Include(f => f.FilterGroups)
                .ThenInclude(fg => fg.FilterItems)
            .Where(f => f.SubjectId == originalSubjectId || f.SubjectId == replacementSubjectId)
            .ToListAsync();

        var originalFilters = filters.Where(f => f.SubjectId == originalSubjectId).ToList();

        var replacementFilters = filters.Where(f => f.SubjectId == replacementSubjectId).ToList();

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

        return filterMappings;
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
