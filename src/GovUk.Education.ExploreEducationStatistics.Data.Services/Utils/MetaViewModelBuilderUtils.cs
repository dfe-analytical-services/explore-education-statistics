#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;

public static class MetaViewModelBuilderUtils
{
    private static IComparer<string> LabelComparer { get; } = new LabelRelationalComparer();

    private static bool IsLabelTotal<T>(T input, Func<T, string> labelSelector)
    {
        return labelSelector(input).Equals("Total", StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<Ordered<TValue, TSequence>> OrderByLabel<TValue, TSequence>(
        IEnumerable<TValue> values,
        Func<TValue, string> labelSelector
    )
    {
        return values.OrderBy(labelSelector, LabelComparer).Select(source => new Ordered<TValue, TSequence>(source));
    }

    private static IEnumerable<Ordered<TValue, TSequence>> OrderByLabelWithTotalFirst<TValue, TSequence>(
        IEnumerable<TValue> values,
        Func<TValue, string> labelSelector
    )
    {
        return values
            .OrderBy(value => !IsLabelTotal(value, labelSelector))
            .ThenBy(labelSelector, LabelComparer)
            .Select(source => new Ordered<TValue, TSequence>(source));
    }

    private static IEnumerable<Ordered<TValue, TSequence>> OrderBySequence<TValue, TSequence, TId>(
        IEnumerable<TValue> values,
        Func<TValue, TId> idSelector,
        Func<TSequence, TId> sequenceIdSelector,
        IEnumerable<TSequence> sequences
    )
        where TId : notnull
    {
        // TODO EES-1238 improve this to use the source list as the iteration base and throw exception if no ordering?
        var valueMap = values.ToDictionary(idSelector);
        foreach (var sequence in sequences)
        {
            var id = sequenceIdSelector(sequence);
            if (valueMap.TryGetValue(id, out var value))
            {
                yield return new Ordered<TValue, TSequence>(value, sequence);
            }
        }
    }

    public static Dictionary<string, TResult> OrderAsDictionary<TValue, TId, TSequence, TResult>(
        IEnumerable<TValue> values,
        Func<TValue, TId> idSelector,
        Func<TValue, string> labelSelector,
        Func<TSequence, TId> sequenceIdSelector,
        Func<Ordered<TValue, TSequence>, int, TResult> resultSelector,
        IEnumerable<TSequence>? sequence
    )
        where TId : notnull
    {
        return (
            sequence == null
                ? OrderByLabel<TValue, TSequence>(values, labelSelector)
                : OrderBySequence(values, idSelector, sequenceIdSelector, sequence)
        ).ToDictionaryIndexed(value => labelSelector(value).PascalCase(), resultSelector);
    }

    public static Dictionary<string, TResult> OrderAsDictionaryWithTotalFirst<TValue, TId, TSequence, TResult>(
        IEnumerable<TValue> values,
        Func<TValue, TId> idSelector,
        Func<TValue, string> labelSelector,
        Func<TSequence, TId> sequenceIdSelector,
        Func<Ordered<TValue, TSequence>, int, TResult> resultSelector,
        IEnumerable<TSequence>? sequence
    )
        where TId : notnull
    {
        return (
            sequence == null
                ? OrderByLabelWithTotalFirst<TValue, TSequence>(values, labelSelector)
                : OrderBySequence(values, idSelector, sequenceIdSelector, sequence)
        ).ToDictionaryIndexed(value => labelSelector(value).PascalCase(), resultSelector);
    }

    public static IEnumerable<TResult> OrderBySequenceOrLabel<TValue, TId, TSequence, TResult>(
        IEnumerable<TValue> values,
        Func<TValue, TId> idSelector,
        Func<TValue, string> labelSelector,
        Func<TSequence, TId> sequenceIdSelector,
        Func<Ordered<TValue, TSequence>, TResult> resultSelector,
        IEnumerable<TSequence>? sequence
    )
        where TId : notnull =>
        (
            sequence == null
                ? OrderByLabel<TValue, TSequence>(values, labelSelector)
                : OrderBySequence(values, idSelector, sequenceIdSelector, sequence)
        ).Select(resultSelector);

    public static IEnumerable<TResult> OrderBySequenceOrLabelTotalFirst<TValue, TId, TSequence, TResult>(
        IEnumerable<TValue> values,
        Func<TValue, TId> idSelector,
        Func<TValue, string> labelSelector,
        Func<TSequence, TId> sequenceIdSelector,
        Func<Ordered<TValue, TSequence>, TResult> resultSelector,
        IEnumerable<TSequence>? sequence
    )
        where TId : notnull =>
        (
            sequence == null
                ? OrderByLabelWithTotalFirst<TValue, TSequence>(values, labelSelector)
                : OrderBySequence(values, idSelector, sequenceIdSelector, sequence)
        ).Select(resultSelector);

    public record Ordered<TValue, TSequence>(TValue Value, TSequence? Sequence = default)
    {
        public static implicit operator TValue(Ordered<TValue, TSequence> o) => o.Value;
    }
}
