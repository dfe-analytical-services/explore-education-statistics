#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services;

public class FilterAndIndicatorViewModelBuilders
{
    public static class FiltersViewModelBuilder
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
            FilterMetaViewModel BuildViewModel(Ordered<Filter, FilterSequenceEntry> input, int index)
            {
                var totalFilterItemId = GetTotal(input)?.Id;
                return new FilterMetaViewModel(input, totalFilterItemId, index)
                {
                    Options = BuildFilterGroups(input.Value.FilterGroups, input.Sequence?.ChildSequence)
                };
            }

            return OrderAsDictionary(values,
                idSelector: value => value.Id,
                labelSelector: value => value.Label,
                sequenceIdSelector: s => s.Id,
                resultSelector: BuildViewModel,
                sequence
            );
        }

        private static Dictionary<string, FilterGroupMetaViewModel> BuildFilterGroups(
            IEnumerable<FilterGroup> values,
            IEnumerable<FilterGroupSequenceEntry>? sequence = null)
        {
            FilterGroupMetaViewModel BuildViewModel(Ordered<FilterGroup, FilterGroupSequenceEntry> input, int index)
            {
                return new FilterGroupMetaViewModel(input, index)
                {
                    Options = BuildFilterItems(input.Value.FilterItems, input.Sequence?.ChildSequence)
                };
            }

            return OrderAsDictionaryWithTotalFirst(values,
                idSelector: value => value.Id,
                labelSelector: value => value.Label,
                sequenceIdSelector: sequenceEntry => sequenceEntry.Id,
                resultSelector: BuildViewModel,
                sequence
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
            return GetTotalGroup(filter)?.FilterItems.FirstOrDefault(IsFilterItemTotal);
        }

        private static FilterGroup? GetTotalGroup(Filter filter)
        {
            var filterGroups = filter.FilterGroups;

            // Return the group if there is only one, otherwise the 'Total' group if it exists
            return filterGroups.Count == 1
                ? filterGroups.First()
                : filterGroups.FirstOrDefault(IsFilterGroupTotal);
        }

        private static bool IsFilterItemTotal(FilterItem filterItem)
        {
            return IsLabelTotal(filterItem, item => item.Label);
        }

        private static bool IsFilterGroupTotal(FilterGroup filterGroup)
        {
            return IsLabelTotal(filterGroup, group => group.Label);
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

    public static class IndicatorsViewModelBuilder
    {
        public static Dictionary<string, IndicatorGroupMetaViewModel> BuildIndicatorGroups(
            IEnumerable<IndicatorGroup> values,
            IEnumerable<IndicatorGroupSequenceEntry>? sequence = null)
        {
            IndicatorGroupMetaViewModel BuildViewModel(Ordered<IndicatorGroup, IndicatorGroupSequenceEntry> input, int index)
            {
                return new IndicatorGroupMetaViewModel(input, index)
                {
                    Options = BuildIndicators(input.Value.Indicators, input.Sequence?.ChildSequence)
                };
            }

            return OrderAsDictionary(values,
                idSelector: filter => filter.Id,
                labelSelector: filter => filter.Label,
                sequenceIdSelector: filterOrdering => filterOrdering.Id,
                resultSelector: BuildViewModel,
                sequence
            );
        }

        public static List<IndicatorMetaViewModel> BuildIndicators(IEnumerable<Indicator> values,
            IEnumerable<Guid>? sequence = null)
        {
            return OrderAsList(values,
                idSelector: value => value.Id,
                labelSelector: value => value.Label,
                sequenceIdSelector: sequenceEntry => sequenceEntry,
                resultSelector: value => new IndicatorMetaViewModel(value),
                sequence
            );
        }
    }

    private static IComparer<string> LabelComparer { get; } = new LabelRelationalComparer();

    private static bool IsLabelTotal<T>(T input, Func<T, string> labelSelector)
    {
        return labelSelector(input).Equals("Total", StringComparison.InvariantCultureIgnoreCase);
    }

    private static IEnumerable<Ordered<TValue, TSequence>> OrderByLabel<TValue, TSequence>(
        IEnumerable<TValue> values,
        Func<TValue, string> labelSelector)
    {
        return values.OrderBy(labelSelector, LabelComparer)
            .Select(source => new Ordered<TValue, TSequence>(source));
    }

    private static List<Ordered<TValue, TSequence>> OrderByLabelWithTotalFirst<TValue, TSequence>(
        IEnumerable<TValue> values,
        Func<TValue, string> labelSelector)
    {
        return values.OrderBy(value => !IsLabelTotal(value, labelSelector))
            .ThenBy(labelSelector, LabelComparer)
            .Select(source => new Ordered<TValue, TSequence>(source))
            .ToList();
    }

    private static IEnumerable<Ordered<TValue, TSequence>> OrderBySequence<TValue, TSequence, TId>(
        IEnumerable<TValue> values,
        Func<TValue, TId> idSelector,
        Func<TSequence, TId> sequenceIdSelector,
        IEnumerable<TSequence> sequences) where TId : notnull
    {
        // TODO EES-1238 improve this to use the source list as the iteration base and throw exception if no ordering?
        var valueMap = values.ToDictionary(idSelector);
        foreach (var sequence in sequences)
        {
            var id = sequenceIdSelector(sequence);
            if (valueMap.ContainsKey(id))
            {
                yield return new Ordered<TValue, TSequence>(valueMap[id], sequence);
            }
        }
    }

    private static Dictionary<string, TResult> OrderAsDictionary<TValue, TId, TSequence, TResult>(
        IEnumerable<TValue> values,
        Func<TValue, TId> idSelector,
        Func<TValue, string> labelSelector,
        Func<TSequence, TId> sequenceIdSelector,
        Func<Ordered<TValue, TSequence>, int, TResult> resultSelector,
        IEnumerable<TSequence>? sequence) where TId : notnull
    {
        var ordered = (sequence == null
            ? OrderByLabel<TValue, TSequence>(values, labelSelector)
            : OrderBySequence(values, idSelector, sequenceIdSelector, sequence)).ToList();

        var result = new Dictionary<string, TResult>(ordered.Count);
        ordered.ForEach((ordered1, index) =>
        {
            var key = labelSelector(ordered1).PascalCase();
            var value = resultSelector(ordered1, index);
            result.Add(key, value);
        });
        return result;
    }

    private static Dictionary<string, TResult> OrderAsDictionaryWithTotalFirst<TValue, TId, TSequence, TResult>(
        IEnumerable<TValue> values,
        Func<TValue, TId> idSelector,
        Func<TValue, string> labelSelector,
        Func<TSequence, TId> sequenceIdSelector,
        Func<Ordered<TValue, TSequence>, int, TResult> resultSelector,
        IEnumerable<TSequence>? sequence) where TId : notnull
    {
        var ordered = (sequence == null
            ? OrderByLabelWithTotalFirst<TValue, TSequence>(values, labelSelector)
            : OrderBySequence(values, idSelector, sequenceIdSelector, sequence)).ToList();

        var result = new Dictionary<string, TResult>(ordered.Count);
        ordered.ForEach((value, index) =>
        {
            result.Add(labelSelector(value).PascalCase(), resultSelector(value, index));
        });
        return result;
    }

    private static List<TResult> OrderAsList<TValue, TId, TSequence, TResult>(
        IEnumerable<TValue> values,
        Func<TValue, TId> idSelector,
        Func<TValue, string> labelSelector,
        Func<TSequence, TId> sequenceIdSelector,
        Func<Ordered<TValue, TSequence>, TResult> resultSelector,
        IEnumerable<TSequence>? sequence) where TId : notnull
    {
        return (sequence == null
                ? OrderByLabel<TValue, TSequence>(values, labelSelector)
                : OrderBySequence(values, idSelector, sequenceIdSelector, sequence))
            .Select(resultSelector)
            .ToList();
    }

    private static List<TResult> OrderAsListTotalFirst<TValue, TId, TSequence, TResult>(
        IEnumerable<TValue> values,
        Func<TValue, TId> idSelector,
        Func<TValue, string> labelSelector,
        Func<TSequence, TId> sequenceIdSelector,
        Func<Ordered<TValue, TSequence>, TResult> resultSelector,
        IEnumerable<TSequence>? sequence) where TId : notnull
    {
        return (sequence == null
                ? OrderByLabelWithTotalFirst<TValue, TSequence>(values, labelSelector)
                : OrderBySequence(values, idSelector, sequenceIdSelector, sequence))
            .Select(resultSelector)
            .ToList();
    }

    private class Ordered<TValue, TSequence>
    {
        public TValue Value { get; }
        public TSequence? Sequence { get; }

        public Ordered(TValue value)
        {
            Value = value;
        }

        public Ordered(TValue value, TSequence? sequence)
        {
            Value = value;
            Sequence = sequence;
        }

        public static implicit operator TValue(Ordered<TValue, TSequence> o) => o.Value;
    }
}
