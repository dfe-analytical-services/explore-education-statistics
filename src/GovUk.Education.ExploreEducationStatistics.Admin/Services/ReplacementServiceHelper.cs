#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public abstract class ReplacementServiceHelper
{
    private static IComparer<string> LabelComparer { get; } = new LabelRelationalComparer();

    public static List<FilterSequenceEntry> ReplaceFilterSequence(
        List<FilterSequenceEntry> originalSequence,
        DataSetMapping mapping,
        List<Filter> replacementFilters
    )
    {
        // Add mapped replacement filters
        var replacementSequence = new List<FilterSequenceEntry>();
        var filterMappingsWithReplacement = mapping
            .FilterMappings.Values.Where(filterMap => filterMap.ReplacementId != null)
            .Select(filterMap => new { filterMap.OriginalId, ReplacementId = filterMap.ReplacementId!.Value })
            .ToList();

        var filterIdsWithMapping = filterMappingsWithReplacement.Select(filterMap => filterMap.OriginalId).ToHashSet();
        foreach (var filterSequenceEntry in originalSequence)
        {
            if (filterIdsWithMapping.Contains(filterSequenceEntry.Id))
            {
                var filterMapping = mapping.FilterMappings[filterSequenceEntry.Id];
                var replacementFilterGroups = replacementFilters
                    .Where(f => f.Id == filterMapping.ReplacementId)
                    .Select(f => f.FilterGroups)
                    .Single();

                var replacementGroupSequence = GenerateReplacementGroupSequence(
                    filterMapping,
                    replacementFilterGroups,
                    filterSequenceEntry
                );

                replacementSequence.Add(
                    new FilterSequenceEntry(
                        Id: mapping.FilterMappings[filterSequenceEntry.Id].ReplacementId!.Value,
                        FilterGroupSequence: replacementGroupSequence
                    )
                );
            }
        }

        // Add remaining replacement filters - those that are unmapped
        var claimedFilterIds = filterMappingsWithReplacement.Select(filterMap => filterMap.ReplacementId).ToHashSet();
        replacementSequence.AddRange(
            replacementFilters
                .Where(replacementFilter => !claimedFilterIds.Contains(replacementFilter.Id))
                .OrderBy(replacementFilter => replacementFilter.Label, LabelComparer)
                .Select(replacementFilter => new FilterSequenceEntry(
                    Id: replacementFilter.Id,
                    FilterGroupSequence: replacementFilter
                        .FilterGroups.OrderBy(replacementGroup => replacementGroup.Label, LabelComparer)
                        .Select(replacementGroup => new FilterGroupSequenceEntry(
                            Id: replacementGroup.Id,
                            FilterItemSequence: replacementGroup
                                .FilterItems.OrderBy(replacementItem => replacementItem.Label, LabelComparer)
                                .Select(replacementItem => replacementItem.Id)
                                .ToList()
                        ))
                        .ToList()
                ))
        );

        return replacementSequence;
    }

    private static List<FilterGroupSequenceEntry> GenerateReplacementGroupSequence(
        FilterMapping filterMapping,
        List<FilterGroup> replacementFilterGroups,
        FilterSequenceEntry originalFilterSequenceEntry
    )
    {
        // Add mapped replacement groups
        var replacementGroupSequence = new List<FilterGroupSequenceEntry>();
        var groupMappingsWithReplacement = filterMapping
            .FilterGroupMappings.Values.Where(groupMap => groupMap.ReplacementId != null)
            .Select(groupMap => new { groupMap.OriginalId, ReplacementId = groupMap.ReplacementId!.Value })
            .ToList();

        var groupIdsWithMapping = groupMappingsWithReplacement.Select(groupMap => groupMap.OriginalId).ToHashSet();
        foreach (var groupSequenceEntry in originalFilterSequenceEntry.FilterGroupSequence)
        {
            if (groupIdsWithMapping.Contains(groupSequenceEntry.Id))
            {
                var filterGroupMapping = filterMapping.FilterGroupMappings[groupSequenceEntry.Id];
                var replacementItems = replacementFilterGroups
                    .Where(g => g.Id == filterGroupMapping.ReplacementId)
                    .Select(g => g.FilterItems)
                    .Single();

                var replacementItemSequence = GenerateReplacementItemSequence(
                    filterGroupMapping,
                    replacementItems,
                    groupSequenceEntry
                );

                replacementGroupSequence.Add(
                    new FilterGroupSequenceEntry(
                        Id: filterMapping.FilterGroupMappings[groupSequenceEntry.Id].ReplacementId!.Value,
                        FilterItemSequence: replacementItemSequence
                    )
                );
            }
        }

        // Add remaining replacement groups - those that are unmapped
        var claimedGroupIds = groupMappingsWithReplacement.Select(groupMap => groupMap.ReplacementId).ToHashSet();
        replacementGroupSequence.AddRange(
            replacementFilterGroups
                .Where(replacementGroup => !claimedGroupIds.Contains(replacementGroup.Id))
                .OrderBy(replacementGroup => replacementGroup.Label)
                .Select(replacementGroup => new FilterGroupSequenceEntry(
                    Id: replacementGroup.Id,
                    FilterItemSequence: replacementGroup
                        .FilterItems.OrderBy(replacementItem => replacementItem.Label)
                        .Select(replacementItem => replacementItem.Id)
                        .ToList()
                ))
        );

        return replacementGroupSequence;
    }

    private static List<Guid> GenerateReplacementItemSequence(
        FilterGroupMapping groupMapping,
        List<FilterItem> replacementFilterItems,
        FilterGroupSequenceEntry originalGroupSequenceEntry
    )
    {
        // Add mapped replacement items
        var replacementItemSequence = new List<Guid>();
        var itemMappingsWithReplacement = groupMapping
            .FilterItemMappings.Values.Where(itemMap => itemMap.ReplacementId != null)
            .Select(itemMap => new { itemMap.OriginalId, ReplacementId = itemMap.ReplacementId!.Value })
            .ToList();

        var itemIdsWithMapping = itemMappingsWithReplacement.Select(itemMap => itemMap.OriginalId).ToHashSet();

        foreach (var sequenceItemId in originalGroupSequenceEntry.FilterItemSequence)
        {
            if (itemIdsWithMapping.Contains(sequenceItemId))
            {
                replacementItemSequence.Add(groupMapping.FilterItemMappings[sequenceItemId].ReplacementId!.Value);
            }
        }

        // Add remaining replacement items - those that are currently unmapped
        var claimedItemIds = itemMappingsWithReplacement.Select(itemMap => itemMap.ReplacementId).ToHashSet();
        replacementItemSequence.AddRange(
            replacementFilterItems
                .Where(replacementItem => !claimedItemIds.Contains(replacementItem.Id))
                .Select(replacementItem => replacementItem.Id)
        );

        return replacementItemSequence;
    }

    public static List<IndicatorGroupSequenceEntry> ReplaceIndicatorSequence(
        DataSetMapping mapping,
        Dictionary<Guid, string> originalGroupIdToLabelMap,
        Dictionary<string, Guid> replacementGroupLabelToIdMap,
        List<IndicatorGroupSequenceEntry> originalSequence
    )
    {
        // The below code to create replacementSequence can be summarised as "Create all replacement groups, and as
        // we create each group, ensure it contains all replacement indicators belonging to that group, preserving
        // as much of the original ordering as we can."
        //
        // We create all replacementSequence groups in three broad steps:
        // - STEP 1: Add replacement groups that can be mapped from originalSequence (by group label)
        // - STEP 2: Then add new replacement groups from mapping.IndicatorMappings that haven't been mapped from
        //   originalSequence
        // - STEP 3: Finally, add any new replacement groups from mapping.UnmappedReplacementIndicators that haven't yet been
        //   added to replacementSequence.
        //
        // As we add replacement groups to replacementSequence, we must ensure that all replacement indicators belonging
        // to each group are added. This is because it's possible that are new indicators in the replacement and/or an
        // indicator was moved to a different group in the replacement. For example, after creating a group mapped from
        // originalSequence, we add replacement indicators that can be mapped from entries in that originalSequenceGroup
        // first (preserving the ordering as possible) but then also need to add other indicators belonging to that
        // replacement group from IndicationMappings (i.e. those that have moved group) and UnmappedReplacementIndicators.
        //
        // Follow comments PART 1-6 in the code below to track the creation of all replacement sequence groups and indicators:
        // - Create all replacement groups with a label matching an originalSequence group (1,2,3)
        // - Create new groups for mapped indicators that previously belonged to an original group, but moved to a new
        //   group (4,5)
        // - Create new groups for UnmappedReplacementIndicators (6)

        var replacementSequence = originalSequence
            .Select(originalGroupSequence =>
            {
                var originalGroupLabel = originalGroupIdToLabelMap[originalGroupSequence.Id];
                if (!replacementGroupLabelToIdMap.TryGetValue(originalGroupLabel, out var replacementGroupId))
                {
                    // No replacement group matching the label of the original group, so skip this originalGroupSequence
                    return null;
                }

                var mappingsForGroupWithReplacementSet = mapping
                    .IndicatorMappings.Values.Where(map => map.ReplacementGroupId == replacementGroupId)
                    .ToList();

                var unmappedIndicatorsForGroup = mapping
                    .UnmappedReplacementIndicators.Where(unmappedIndicator =>
                        unmappedIndicator.GroupId == replacementGroupId
                    )
                    .ToList();

                if (!mappingsForGroupWithReplacementSet.Any() && !unmappedIndicatorsForGroup.Any())
                {
                    // There should never be an IndicatorGroup with no Indicators, but if there was,
                    // this prevents an entry for that group being created in IndicatorSequence.
                    return null;
                }

                // STEP 1
                // PART 1. Create indicators for replacement group that can be mapped from originalSequence
                var replacementChildSequence = originalGroupSequence
                    .ChildSequence.Select(originalIndicatorId =>
                        mappingsForGroupWithReplacementSet.SingleOrDefault(map => map.OriginalId == originalIndicatorId)
                    )
                    .WhereNotNull()
                    .Select(map => map.ReplacementId!.Value)
                    .ToList();

                // PART 2. Other mapped indicators for this group which weren't mapped from originalGroupSequence. These
                // indicators originally belonged to a different group but moved into this group in the replacement
                var newChildren = mappingsForGroupWithReplacementSet
                    .Where(map => !replacementChildSequence.Contains(map.ReplacementId!.Value))
                    .Select(map => new
                    {
                        ReplacementId = map.ReplacementId!.Value,
                        ReplacementLabel = map.ReplacementLabel!,
                    })
                    .ToList();

                // PART 3. Unmapped replacement indicators that belong to this group
                newChildren.AddRange(
                    unmappedIndicatorsForGroup.Select(unmappedIndicator => new
                    {
                        ReplacementId = unmappedIndicator.Id,
                        ReplacementLabel = unmappedIndicator.Label,
                    })
                );

                replacementChildSequence.AddRange(
                    newChildren.OrderBy(child => child.ReplacementLabel).Select(child => child.ReplacementId)
                );

                return new IndicatorGroupSequenceEntry(replacementGroupId, replacementChildSequence);
            })
            .WhereNotNull()
            .ToList();

        // STEP 2
        var groupIdsInReplacementSequence = replacementSequence.Select(groupSeq => groupSeq.Id).ToList();
        var mappingsWithNewReplacementGroup = mapping
            .IndicatorMappings.Values.Where(map =>
                map.ReplacementGroupId is not null
                && !groupIdsInReplacementSequence.Contains(map.ReplacementGroupId.Value)
            )
            .GroupBy(map => new { GroupId = map.ReplacementGroupId!.Value, GroupLabel = map.ReplacementGroupLabel! })
            .OrderBy(group => group.Key.GroupLabel)
            .ToList();

        foreach (var group in mappingsWithNewReplacementGroup)
        {
            // PART 4. Mapped replacement indicator that moved from an original preexisting group to a new group in the replacement
            var childSequence = group
                .Select(map => new
                {
                    ReplacementId = map.ReplacementId!.Value,
                    ReplacementLabel = map.ReplacementLabel!,
                })
                .ToList();

            // PART 5. Unmapped replacement indicators that belong to this group
            childSequence.AddRange(
                mapping
                    .UnmappedReplacementIndicators.Where(unmappedIndicator =>
                        unmappedIndicator.GroupId == group.Key.GroupId
                    )
                    .Select(unmappedIndicator => new
                    {
                        ReplacementId = unmappedIndicator.Id,
                        ReplacementLabel = unmappedIndicator.Label,
                    })
            );

            replacementSequence.Add(
                new IndicatorGroupSequenceEntry(
                    group.Key.GroupId,
                    childSequence.OrderBy(child => child.ReplacementLabel).Select(child => child.ReplacementId).ToList()
                )
            );
        }

        // STEP 3
        // PART 6. Finally, add any unmapped replacement indicators that belong to a new group
        var unmappedReplacementIndicatorsWithNewGroups = mapping
            .UnmappedReplacementIndicators.Where(unmappedIndicator =>
                !replacementSequence.Select(groupSeq => groupSeq.Id).ToList().Contains(unmappedIndicator.GroupId)
            )
            .GroupBy(unmappedIndicator => new { unmappedIndicator.GroupId, unmappedIndicator.GroupLabel })
            .OrderBy(grouping => grouping.Key.GroupLabel)
            .Select(grouping => new IndicatorGroupSequenceEntry(
                Id: grouping.Key.GroupId,
                ChildSequence: grouping
                    .OrderBy(unmappedIndicator => unmappedIndicator.Label)
                    .Select(unmappedIndicator => unmappedIndicator.Id)
                    .ToList()
            ));
        replacementSequence.AddRange(unmappedReplacementIndicatorsWithNewGroups);

        return replacementSequence;
    }
}
