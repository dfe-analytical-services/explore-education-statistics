#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;

public static class MetaViewModelBuilderUtils
{
    private static IComparer<string> LabelComparer { get; } = new LabelRelationalComparer();

    public static bool IsLabelTotal<T>(T input, Func<T, string> labelSelector)
    {
        return labelSelector(input).Equals("Total", StringComparison.OrdinalIgnoreCase);
    }

    public static IEnumerable<Ordered<TValue, TSequence>> OrderByLabel<TValue, TSequence>(
        IEnumerable<TValue> values,
        Func<TValue, string> labelSelector)
    {
        return values.OrderBy(labelSelector, LabelComparer)
            .Select(source => new Ordered<TValue, TSequence>(source));
    }

    public static List<Ordered<TValue, TSequence>> OrderByLabelWithTotalFirst<TValue, TSequence>(
        IEnumerable<TValue> values,
        Func<TValue, string> labelSelector)
    {
        return values.OrderBy(value => !IsLabelTotal(value, labelSelector))
            .ThenBy(labelSelector, LabelComparer)
            .Select(source => new Ordered<TValue, TSequence>(source))
            .ToList();
    }

    public static IEnumerable<Ordered<TValue, TSequence>> OrderBySequence<TValue, TSequence, TId>(
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

    public static Dictionary<string, TResult> OrderAsDictionary<TValue, TId, TSequence, TResult>(
        IEnumerable<TValue> values,
        Func<TValue, TId> idSelector,
        Func<TValue, string> labelSelector,
        Func<TSequence, TId> sequenceIdSelector,
        Func<Ordered<TValue, TSequence>, int, TResult> resultSelector,
        IEnumerable<TSequence>? sequence) where TId : notnull
    {
        return (sequence == null
                ? OrderByLabel<TValue, TSequence>(values, labelSelector)
                : OrderBySequence(values, idSelector, sequenceIdSelector, sequence))
            .ToDictionaryIndexed(value => labelSelector(value).PascalCase(),
                resultSelector);
    }

    public static Dictionary<string, TResult> OrderAsDictionaryWithTotalFirst<TValue, TId, TSequence, TResult>(
        IEnumerable<TValue> values,
        Func<TValue, TId> idSelector,
        Func<TValue, string> labelSelector,
        Func<TSequence, TId> sequenceIdSelector,
        Func<Ordered<TValue, TSequence>, int, TResult> resultSelector,
        IEnumerable<TSequence>? sequence) where TId : notnull
    {
        return (sequence == null
                ? OrderByLabelWithTotalFirst<TValue, TSequence>(values, labelSelector)
                : OrderBySequence(values, idSelector, sequenceIdSelector, sequence))
            .ToDictionaryIndexed(value => labelSelector(value).PascalCase(),
                resultSelector);
    }

    public static List<TResult> OrderAsList<TValue, TId, TSequence, TResult>(
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

    public static List<TResult> OrderAsListTotalFirst<TValue, TId, TSequence, TResult>(
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

    public class Ordered<TValue, TSequence>
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
