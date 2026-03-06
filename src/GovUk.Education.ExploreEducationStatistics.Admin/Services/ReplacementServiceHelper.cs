#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services;
using LinqToDB;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public abstract class ReplacementServiceHelper
{
    private static IComparer<string> LabelComparer { get; } = new LabelRelationalComparer();

    public static List<FilterSequenceEntry>? ReplaceFilterSequence(
        List<Filter> originalFilters,
        List<Filter> replacementFilters,
        ReleaseFile originalReleaseFile
    )
    {
        // If the sequence is undefined then leave it so we continue to fallback to ordering by label alphabetically
        if (originalReleaseFile.FilterSequence == null)
        {
            return null;
        }

        var originalFiltersLabelMap = originalFilters.ToDictionary(filter => filter.Name, filter => filter);

        // Step 1: Create id to replacement-id maps and work out newly added filters, filter groups and filter items

        var filtersMap = new Dictionary<Guid, Guid>();
        var filterGroupsMap = new Dictionary<Guid, Guid>();
        var filterItemsMap = new Dictionary<Guid, Guid>();
        var newlyAddedFilters = new List<Filter>();
        var newlyAddedFilterGroups = new Dictionary<Guid, List<FilterGroup>>();
        var newlyAddedFilterItems = new Dictionary<Guid, List<FilterItem>>();

        replacementFilters.ForEach(replacementFilter =>
        {
            if (originalFiltersLabelMap.TryGetValue(replacementFilter.Name, out var originalFilter))
            {
                filtersMap.Add(originalFilter.Id, replacementFilter.Id);
                var originalFilterGroupsLabelMap = originalFilter.FilterGroups.ToDictionary(fg => fg.Label);
                replacementFilter.FilterGroups.ForEach(replacementFilterGroup =>
                {
                    if (
                        originalFilterGroupsLabelMap.TryGetValue(
                            replacementFilterGroup.Label,
                            out var originalFilterGroup
                        )
                    )
                    {
                        filterGroupsMap.Add(originalFilterGroup.Id, replacementFilterGroup.Id);
                        var originalFilterItemsLabelMap = originalFilterGroup.FilterItems.ToDictionary(fi => fi.Label);
                        replacementFilterGroup.FilterItems.ForEach(replacementFilterItem =>
                        {
                            if (
                                originalFilterItemsLabelMap.TryGetValue(
                                    replacementFilterItem.Label,
                                    out var originalFilterItem
                                )
                            )
                            {
                                filterItemsMap.Add(originalFilterItem.Id, replacementFilterItem.Id);
                            }
                            else
                            {
                                if (
                                    newlyAddedFilterItems.TryGetValue(
                                        originalFilterGroup.Id,
                                        out var newlyAddedFilterItemList
                                    )
                                )
                                {
                                    newlyAddedFilterItemList.Add(replacementFilterItem);
                                }
                                else
                                {
                                    newlyAddedFilterItems.Add(originalFilterGroup.Id, ListOf(replacementFilterItem));
                                }
                            }
                        });
                    }
                    else
                    {
                        if (newlyAddedFilterGroups.TryGetValue(originalFilter.Id, out var newlyAddedFilterGroupList))
                        {
                            newlyAddedFilterGroupList.Add(replacementFilterGroup);
                        }
                        else
                        {
                            newlyAddedFilterGroups.Add(originalFilter.Id, ListOf(replacementFilterGroup));
                        }
                    }
                });
            }
            else
            {
                newlyAddedFilters.Add(replacementFilter);
            }
        });

        // Step 2: Create a new sequence based on the original:
        // - Remove any entries that don't exist in the replacement
        // - Swap the remaining id's with their replacements
        // - Append new entries that were added in the replacement

        var filterSequence = originalReleaseFile
            .FilterSequence.Where(filter => filtersMap.ContainsKey(filter.Id))
            .Select(filter =>
            {
                var newFilterSequenceEntry = new FilterSequenceEntry(
                    filtersMap[filter.Id],
                    filter
                        .ChildSequence.Where(filterGroup => filterGroupsMap.ContainsKey(filterGroup.Id))
                        .Select(filterGroup =>
                        {
                            var newFilterGroupSequenceEntry = new FilterGroupSequenceEntry(
                                filterGroupsMap[filterGroup.Id],
                                filterGroup
                                    .ChildSequence.Where(filterItem => filterItemsMap.ContainsKey(filterItem))
                                    .Select(filterItem => filterItemsMap[filterItem])
                                    .ToList()
                            );

                            if (newlyAddedFilterItems.TryGetValue(filterGroup.Id, out var newlyAddedFilterItemList))
                            {
                                newFilterGroupSequenceEntry.ChildSequence.AddRange(
                                    newlyAddedFilterItemList
                                        .OrderBy(fi => !IsTotal(fi.Label))
                                        .ThenBy(fi => fi.Label, LabelComparer)
                                        .Select(fi => fi.Id)
                                );
                            }

                            return newFilterGroupSequenceEntry;
                        })
                        .ToList()
                );

                if (newlyAddedFilterGroups.TryGetValue(filter.Id, out var newlyAddedFilterGroupList))
                {
                    newFilterSequenceEntry.ChildSequence.AddRange(
                        newlyAddedFilterGroupList
                            .OrderBy(fg => !IsTotal(fg.Label))
                            .ThenBy(fg => fg.Label, LabelComparer)
                            .Select(fg => new FilterGroupSequenceEntry(
                                fg.Id,
                                fg.FilterItems.OrderBy(fi => !IsTotal(fi.Label))
                                    .ThenBy(fi => fi.Label, LabelComparer)
                                    .Select(fi => fi.Id)
                                    .ToList()
                            ))
                    );
                }

                return newFilterSequenceEntry;
            })
            .ToList();

        filterSequence.AddRange(
            newlyAddedFilters
                .OrderBy(f => f.Label, LabelComparer)
                .Select(f => new FilterSequenceEntry(
                    f.Id,
                    f.FilterGroups.OrderBy(fg => !IsTotal(fg.Label))
                        .ThenBy(fg => fg.Label, LabelComparer)
                        .Select(fg => new FilterGroupSequenceEntry(
                            fg.Id,
                            fg.FilterItems.OrderBy(fi => !IsTotal(fi.Label))
                                .ThenBy(fi => fi.Label, LabelComparer)
                                .Select(fi => fi.Id)
                                .ToList()
                        ))
                        .ToList()
                ))
        );

        return filterSequence;
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
        // - Add replacement groups that can be mapped from originalSequence (by group label)
        // - Then add new replacement groups from mapping.IndicatorMappings that haven't been mapped from
        //   originalSequence
        // - Finally, add any new replacement groups from mapping.UnsetReplacementIndicators that haven't yet been
        //   added to replacementSequence.
        //
        // As we add replacement groups to replacementSequence, we must ensure that all replacement indicators belonging
        // to each group are added. This is because it's possible that are new indicators in the replacement and/or an
        // indicator was moved to a different group in the replacement. For example, after creating a group mapped from
        // originalSequence, we add replacement indicators that can be mapped from entries in that originalSequenceGroup
        // first (preserving the ordering as possible) but then also need to add other indicators belonging to that
        // replacement group from IndicationMappings (i.e. those that have moved group) and UnsetReplacementIndicators.
        //
        // Follow comments 1-6 in the code below to track the creation of all replacement sequence groups and indicators:
        // - Create all replacement groups with a label matching an originalSequence group (1,2,3)
        // - Create new groups for mapped indicators that previously belonged to an original group, but moved to a new
        //   group (4,5)
        // - Create new groups for UnsetReplacementIndicators (6)

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

                var unsetIndicatorsForGroup = mapping
                    .UnsetReplacementIndicators.Where(unsetIndicator => unsetIndicator.GroupId == replacementGroupId)
                    .ToList();

                if (mappingsForGroupWithReplacementSet.Count + unsetIndicatorsForGroup.Count == 0)
                {
                    // There should never be an IndicatorGroup with no Indicators, but if there was,
                    // this prevents an entry for that group being created in IndicatorSequence.
                    return null;
                }

                // 1. Create indicators for replacement group that can be mapped from originalSequence
                var replacementChildSequence = originalGroupSequence
                    .ChildSequence.Select(originalIndicatorId =>
                        mappingsForGroupWithReplacementSet.SingleOrDefault(map => map.OriginalId == originalIndicatorId)
                    )
                    .WhereNotNull()
                    .Select(map => map.ReplacementId!.Value)
                    .ToList();

                // 2. Other mapped indicators for this group which weren't mapped from originalGroupSequence. These
                // indicators originally belonged to a different group but moved into this group in the replacement
                var newChildren = mappingsForGroupWithReplacementSet
                    .Where(map => !replacementChildSequence.Contains(map.ReplacementId!.Value))
                    .Select(map => new
                    {
                        ReplacementId = map.ReplacementId!.Value,
                        ReplacementLabel = map.ReplacementLabel!,
                    })
                    .ToList();

                // 3. Unmapped unset replacement indicators that belong to this group
                newChildren.AddRange(
                    unsetIndicatorsForGroup.Select(unsetIndicator => new
                    {
                        ReplacementId = unsetIndicator.Id,
                        ReplacementLabel = unsetIndicator.Label,
                    })
                );

                replacementChildSequence.AddRange(
                    newChildren.OrderBy(child => child.ReplacementLabel).Select(child => child.ReplacementId)
                );

                return new IndicatorGroupSequenceEntry(replacementGroupId, replacementChildSequence);
            })
            .WhereNotNull()
            .ToList();

        var mappingsInNewGroups = mapping
            .IndicatorMappings.Values.Where(map =>
                map.ReplacementGroupId is not null
                && !replacementSequence.Select(groupSeq => groupSeq.Id).ToList().Contains(map.ReplacementGroupId.Value)
            )
            .GroupBy(map => new { GroupId = map.OriginalGroupId, GroupLabel = map.OriginalGroupLabel })
            .OrderBy(group => group.Key.GroupLabel)
            .ToList();

        foreach (var group in mappingsInNewGroups)
        {
            // 4. Mapped replacement indicator that moved from an original preexisting group to a new group in the replacement
            var childSequence = group
                .Select(map => new
                {
                    ReplacementId = map.ReplacementId!.Value,
                    ReplacementLabel = map.ReplacementLabel!,
                })
                .ToList();

            // 5. Unmapped unset replacement indicators that belong to this group
            childSequence.AddRange(
                mapping
                    .UnsetReplacementIndicators.Where(unsetIndicator => unsetIndicator.GroupId == group.Key.GroupId)
                    .Select(unsetIndicator => new
                    {
                        ReplacementId = unsetIndicator.Id,
                        ReplacementLabel = unsetIndicator.Label,
                    })
            );

            replacementSequence.Add(
                new IndicatorGroupSequenceEntry(
                    group.Key.GroupId,
                    childSequence.OrderBy(child => child.ReplacementLabel).Select(child => child.ReplacementId).ToList()
                )
            );
        }

        // 6. Finally, add any unset replacement indicators that belong to a new group
        var unsetIndicatorsInNewGroups = mapping
            .UnsetReplacementIndicators.Where(unsetIndicator =>
                !replacementSequence.Select(groupSeq => groupSeq.Id).ToList().Contains(unsetIndicator.GroupId)
            )
            .GroupBy(unsetIndicator => new { unsetIndicator.GroupId, unsetIndicator.GroupLabel })
            .OrderBy(grouping => grouping.Key.GroupLabel)
            .Select(grouping => new IndicatorGroupSequenceEntry(
                Id: grouping.Key.GroupId,
                ChildSequence: grouping
                    .OrderBy(unsetIndicator => unsetIndicator.Label)
                    .Select(unsetIndicator => unsetIndicator.Id)
                    .ToList()
            ));
        replacementSequence.AddRange(unsetIndicatorsInNewGroups);

        return replacementSequence;
    }

    private static bool IsTotal(string input)
    {
        return input.Equals("Total", StringComparison.OrdinalIgnoreCase);
    }
}
