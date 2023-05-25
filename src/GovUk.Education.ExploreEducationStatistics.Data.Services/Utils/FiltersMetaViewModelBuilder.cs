#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.Utils.MetaViewModelBuilderUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;

public static class FiltersMetaViewModelBuilder
{
    public static Dictionary<string, FilterMetaViewModel> BuildFiltersFromFilterItems(
        IEnumerable<FilterItem> values,
        IEnumerable<FilterSequenceEntry>? sequence = null)
    {
        var filters = GroupFilterItemsByFilter(values);
        return BuildFilters(filters, sequence);
    }

    public static Dictionary<string, FilterMetaViewModel> BuildFilters(IEnumerable<Filter> values,
        IEnumerable<FilterSequenceEntry>? sequence = null)
    {
        return OrderAsDictionary(values,
            idSelector: value => value.Id,
            labelSelector: value => value.Label,
            sequenceIdSelector: sequenceEntry => sequenceEntry.Id,
            resultSelector: (input, index) =>
            {
                var totalFilterItemId = GetTotal(input)?.Id;
                return new FilterMetaViewModel(input, totalFilterItemId, index)
                {
                    Options = BuildFilterGroups(input.Value.FilterGroups, input.Sequence?.ChildSequence)
                };
            },
            sequence: sequence
        );
    }

    public static Dictionary<string, FilterCsvMetaViewModel> BuildCsvFiltersFromFilterItems(
        IEnumerable<FilterItem> values)
    {
        var filters = GroupFilterItemsByFilter(values);

        return filters.ToDictionary(
            filter => filter.Name,
            filter => new FilterCsvMetaViewModel(filter)
        );
    }

    private static Dictionary<string, FilterGroupMetaViewModel> BuildFilterGroups(
        IEnumerable<FilterGroup> values,
        IEnumerable<FilterGroupSequenceEntry>? sequence = null)
    {
        return OrderAsDictionaryWithTotalFirst(values,
            idSelector: value => value.Id,
            labelSelector: value => value.Label,
            sequenceIdSelector: sequenceEntry => sequenceEntry.Id,
            resultSelector: (input, index) => new FilterGroupMetaViewModel(input, index)
            {
                Options = BuildFilterItems(input.Value.FilterItems, input.Sequence?.ChildSequence)
            },
            sequence: sequence
        );
    }

    private static List<FilterItemMetaViewModel> BuildFilterItems(IEnumerable<FilterItem> values,
        IEnumerable<Guid>? sequence = null)
    {
        return OrderAsListTotalFirst(values,
            idSelector: value => value.Id,
            labelSelector: value => value.Label,
            sequenceIdSelector: sequenceEntry => sequenceEntry,
            resultSelector: value => new FilterItemMetaViewModel(value),
            sequence
        );
    }

    private static FilterItem? GetTotal(Filter filter)
    {
        return GetTotalGroup(filter)?.FilterItems.FirstOrDefault(filterItem =>
            IsLabelTotal(filterItem, item => item.Label));
    }

    private static FilterGroup? GetTotalGroup(Filter filter)
    {
        var filterGroups = filter.FilterGroups;

        // Return the group if there is only one, otherwise the 'Total' group if it exists
        return filterGroups.Count == 1
            ? filterGroups.First()
            : filterGroups.FirstOrDefault(filterGroup => IsLabelTotal(filterGroup, group => group.Label));
    }

    private static List<Filter> GroupFilterItemsByFilter(IEnumerable<FilterItem> filterItems)
    {
        return filterItems
            .GroupBy(filterItem => filterItem.FilterGroup, filterItem => filterItem, FilterGroup.IdComparer)
            .Select(grouping =>
            {
                var filterGroup = grouping.Key;
                // Ensure the filter group only has filter items belonging to the input list
                var copy = filterGroup.Clone();
                copy.FilterItems = grouping.ToList();
                return copy;
            })
            .GroupBy(filterGroup => filterGroup.Filter, filterGroup => filterGroup, Filter.IdComparer)
            .Select(grouping =>
            {
                var filter = grouping.Key;
                // Ensure the filter only has filter groups belonging to the input list
                var copy = filter.Clone();
                copy.FilterGroups = grouping.ToList();
                return copy;
            })
            .ToList();
    }
}
