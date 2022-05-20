#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ReplacementServiceHelper
{
    private static IComparer<string> LabelComparer { get; } = new LabelRelationalComparer();

    public static List<FilterSequenceEntry>? ReplaceFilterSequence(
        List<Filter> originalFilters,
        List<Filter> replacementFilters,
        ReleaseSubject originalReleaseSubject)
    {
        // If the sequence is undefined then leave it so we continue to fallback to ordering by label alphabetically
        if (originalReleaseSubject.FilterSequence == null)
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
                    if (originalFilterGroupsLabelMap.TryGetValue(replacementFilterGroup.Label,
                            out var originalFilterGroup))
                    {
                        filterGroupsMap.Add(originalFilterGroup.Id, replacementFilterGroup.Id);
                        var originalFilterItemsLabelMap = originalFilterGroup.FilterItems.ToDictionary(fi => fi.Label);
                        replacementFilterGroup.FilterItems.ForEach(replacementFilterItem =>
                        {
                            if (originalFilterItemsLabelMap.TryGetValue(replacementFilterItem.Label,
                                    out var originalFilterItem))
                            {
                                filterItemsMap.Add(originalFilterItem.Id, replacementFilterItem.Id);
                            }
                            else
                            {
                                if (newlyAddedFilterItems.TryGetValue(originalFilterGroup.Id,
                                        out var newlyAddedFilterItemList))
                                {
                                    newlyAddedFilterItemList.Add(replacementFilterItem);
                                }
                                else
                                {
                                    newlyAddedFilterItems.Add(originalFilterGroup.Id,
                                        ListOf(replacementFilterItem));
                                }
                            }
                        });
                    }
                    else
                    {
                        if (newlyAddedFilterGroups.TryGetValue(originalFilter.Id,
                                out var newlyAddedFilterGroupList))
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

        var filterSequence = originalReleaseSubject.FilterSequence
            .Where(filter => filtersMap.ContainsKey(filter.Id))
            .Select(filter =>
            {
                var newFilterSequenceEntry = new FilterSequenceEntry(
                    filtersMap[filter.Id],
                    filter.ChildSequence
                        .Where(filterGroup => filterGroupsMap.ContainsKey(filterGroup.Id))
                        .Select(filterGroup =>
                        {
                            var newFilterGroupSequenceEntry = new FilterGroupSequenceEntry(
                                filterGroupsMap[filterGroup.Id],
                                filterGroup.ChildSequence
                                    .Where(filterItem => filterItemsMap.ContainsKey(filterItem))
                                    .Select(filterItem => filterItemsMap[filterItem])
                                    .ToList());

                            if (newlyAddedFilterItems.TryGetValue(filterGroup.Id, out var newlyAddedFilterItemList))
                            {
                                newFilterGroupSequenceEntry.ChildSequence.AddRange(
                                    newlyAddedFilterItemList
                                        .OrderBy(fi => !IsTotal(fi.Label))
                                        .ThenBy(fi => fi.Label, LabelComparer)
                                        .Select(fi => fi.Id));
                            }

                            return newFilterGroupSequenceEntry;
                        }).ToList());

                if (newlyAddedFilterGroups.TryGetValue(filter.Id, out var newlyAddedFilterGroupList))
                {
                    newFilterSequenceEntry.ChildSequence.AddRange(newlyAddedFilterGroupList
                        .OrderBy(fg => !IsTotal(fg.Label))
                        .ThenBy(fg => fg.Label, LabelComparer)
                        .Select(fg => new FilterGroupSequenceEntry(fg.Id,
                            fg.FilterItems
                                .OrderBy(fi => !IsTotal(fi.Label))
                                .ThenBy(fi => fi.Label, LabelComparer)
                                .Select(fi => fi.Id).ToList()))
                    );
                }

                return newFilterSequenceEntry;
            }).ToList();

        filterSequence.AddRange(newlyAddedFilters
            .OrderBy(f => f.Label, LabelComparer)
            .Select(f => new FilterSequenceEntry(f.Id,
                f.FilterGroups
                    .OrderBy(fg => !IsTotal(fg.Label))
                    .ThenBy(fg => fg.Label, LabelComparer)
                    .Select(fg =>
                        new FilterGroupSequenceEntry(fg.Id, fg.FilterItems
                            .OrderBy(fi => !IsTotal(fi.Label))
                            .ThenBy(fi => fi.Label, LabelComparer)
                            .Select(fi => fi.Id).ToList())).ToList())));

        return filterSequence;
    }

    public static List<IndicatorGroupSequenceEntry>? ReplaceIndicatorSequence(
        List<IndicatorGroup> originalIndicatorGroups,
        List<IndicatorGroup> replacementIndicatorGroups,
        ReleaseSubject originalReleaseSubject)
    {
        // If the sequence is undefined then leave it so we continue to fallback to ordering by label alphabetically
        if (originalReleaseSubject.IndicatorSequence == null)
        {
            return null;
        }

        var originalIndicatorGroupsLabelMap = originalIndicatorGroups
            .ToDictionary(indicatorGroup => indicatorGroup.Label, indicatorGroup => indicatorGroup);

        // Step 1: Create id to replacement-id maps and work out newly added indicator groups and indicators

        var indicatorGroupsMap = new Dictionary<Guid, Guid>();
        var indicatorsMap = new Dictionary<Guid, Guid>();
        var newlyAddedIndicatorGroups = new List<IndicatorGroup>();
        var newlyAddedIndicators = new Dictionary<Guid, List<Indicator>>();

        replacementIndicatorGroups.ForEach(replacementIndicatorGroup =>
        {
            if (originalIndicatorGroupsLabelMap.TryGetValue(replacementIndicatorGroup.Label,
                    out var originalIndicatorGroup))
            {
                indicatorGroupsMap.Add(originalIndicatorGroup.Id, replacementIndicatorGroup.Id);
                var originalIndicatorsNameMap = originalIndicatorGroup.Indicators.ToDictionary(i => i.Name);
                replacementIndicatorGroup.Indicators.ForEach(replacementIndicator =>
                {
                    if (originalIndicatorsNameMap.TryGetValue(replacementIndicator.Name, out var originalIndicator))
                    {
                        indicatorsMap.Add(originalIndicator.Id, replacementIndicator.Id);
                    }
                    else
                    {
                        if (newlyAddedIndicators.TryGetValue(originalIndicatorGroup.Id,
                                out var newlyAddedIndicatorList))
                        {
                            newlyAddedIndicatorList.Add(replacementIndicator);
                        }
                        else
                        {
                            newlyAddedIndicators.Add(originalIndicatorGroup.Id, ListOf(replacementIndicator));
                        }
                    }
                });
            }
            else
            {
                newlyAddedIndicatorGroups.Add(replacementIndicatorGroup);
            }
        });

        // Step 2: Create a new sequence based on the original:
        // - Remove any entries that don't exist in the replacement
        // - Swap the remaining id's with their replacements
        // - Append new entries that were added in the replacement

        var indicatorSequence = originalReleaseSubject.IndicatorSequence
            .Where(indicatorGroup => indicatorGroupsMap.ContainsKey(indicatorGroup.Id))
            .Select(indicatorGroup =>
            {
                var newIndicatorGroupSequenceEntry = new IndicatorGroupSequenceEntry(
                    indicatorGroupsMap[indicatorGroup.Id],
                    indicatorGroup.ChildSequence
                        .Where(indicator => indicatorsMap.ContainsKey(indicator))
                        .Select(indicator => indicatorsMap[indicator])
                        .ToList());

                if (newlyAddedIndicators.TryGetValue(indicatorGroup.Id, out var newlyAddedIndicatorList))
                {
                    newIndicatorGroupSequenceEntry.ChildSequence.AddRange(
                        newlyAddedIndicatorList
                            .OrderBy(i => i.Label, LabelComparer)
                            .Select(i => i.Id));
                }

                return newIndicatorGroupSequenceEntry;
            }).ToList();

        indicatorSequence.AddRange(newlyAddedIndicatorGroups
            .OrderBy(ig => ig.Label, LabelComparer)
            .Select(ig => new IndicatorGroupSequenceEntry(ig.Id,
                ig.Indicators
                    .OrderBy(i => i.Label, LabelComparer)
                    .Select(i => i.Id).ToList())));

        return indicatorSequence;
    }

    private static bool IsTotal(string input)
    {
        return input.Equals("Total", StringComparison.OrdinalIgnoreCase);
    }
}
