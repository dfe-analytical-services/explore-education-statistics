#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using NaturalSort.Extension;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class EnumerableExtensions
{
    /// <summary>
    /// Batches the source sequence into sized buckets.
    /// </summary>
    /// <typeparam name="TSource">Type of elements in <paramref name="source"/> sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="size">Size of buckets.</param>
    /// <returns>A sequence of equally sized buckets containing elements of the source collection.</returns>
    /// <remarks>
    /// This operator uses deferred execution and streams its results (buckets and bucket content).
    /// </remarks>
    public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(
        this IEnumerable<TSource> source,
        int size
    )
    {
        return Batch(source, size, x => x);
    }

    /// <summary>
    /// Batches the source sequence into sized buckets and applies a projection to each bucket.
    /// </summary>
    /// <typeparam name="TSource">Type of elements in <paramref name="source"/> sequence.</typeparam>
    /// <typeparam name="TResult">Type of result returned by <paramref name="resultSelector"/>.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="size">Size of buckets.</param>
    /// <param name="resultSelector">The projection to apply to each bucket.</param>
    /// <returns>A sequence of projections on equally sized buckets containing elements of the source collection.</returns>
    /// <remarks>
    /// This operator uses deferred execution and streams its results (buckets and bucket content).
    /// </remarks>
    public static IEnumerable<TResult> Batch<TSource, TResult>(
        this IEnumerable<TSource> source,
        int size,
        Func<IEnumerable<TSource>, TResult> resultSelector
    )
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size));
        if (resultSelector == null)
            throw new ArgumentNullException(nameof(resultSelector));

        return _();

        IEnumerable<TResult> _()
        {
            TSource[]? bucket = null;
            var count = 0;

            foreach (var item in source)
            {
                if (bucket == null)
                {
                    bucket = new TSource[size];
                }

                bucket[count++] = item;

                // The bucket is fully buffered before it's yielded
                if (count != size)
                {
                    continue;
                }

                yield return resultSelector(bucket);

                bucket = null;
                count = 0;
            }

            // Return the last bucket with all remaining elements
            if (bucket != null && count > 0)
            {
                Array.Resize(ref bucket, count);
                yield return resultSelector(bucket);
            }
        }
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> func)
    {
        foreach (var item in source)
        {
            func(item);
        }
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> func)
    {
        var index = 0;

        foreach (var item in source)
        {
            func(item, index);
            index += 1;
        }
    }

    public static int IndexOfFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        var index = 0;

        foreach (var item in source)
        {
            if (predicate(item))
            {
                return index;
            }

            index += 1;
        }

        return -1;
    }

    public static async Task<Either<TLeft, List<TRight>>> ForEachAsync<T, TLeft, TRight>(
        this IEnumerable<T> source,
        Func<T, Task<Either<TLeft, TRight>>> func
    )
    {
        var rightResults = new List<TRight>();

        foreach (var item in source)
        {
            var result = await func(item);

            if (result.IsLeft)
            {
                return new Either<TLeft, List<TRight>>(result.Left);
            }

            rightResults.Add(result.Right);
        }

        return rightResults;
    }

    public static Dictionary<TKey, TElement> ToDictionaryIndexed<TSource, TKey, TElement>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TSource, int, TElement> elementSelector
    )
        where TKey : notnull
    {
        var sourceList = source.ToList();

        var result = new Dictionary<TKey, TElement>(sourceList.Count);
        sourceList.ForEach(
            (value, index) =>
            {
                result.Add(keySelector(value), elementSelector(value, index));
            }
        );
        return result;
    }

    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? list)
    {
        if (list == null)
        {
            return true;
        }

        return !list.Any();
    }

    public static string JoinToString<T>(this IEnumerable<T> source)
    {
        return string.Join(string.Empty, source);
    }

    public static string JoinToString<T>(this IEnumerable<T> source, char delimiter)
    {
        return string.Join(delimiter, source);
    }

    public static string JoinToString<T>(this IEnumerable<T> source, string delimiter)
    {
        return string.Join(delimiter, source);
    }

    /// <summary>
    /// Apply offset pagination to <see cref="IEnumerable{T}"/> collections in-memory.
    /// </summary>
    /// <remarks>
    /// This method uses deferred execution and does not enumerate the source sequence
    /// until the returned sequence is enumerated.
    /// </remarks>
    /// <typeparam name="T">The type of elements in the source sequence</typeparam>
    /// <param name="source">The source sequence to paginate</param>
    /// <param name="page">The page number to return</param>
    /// <param name="pageSize">The number of elements that should be returned</param>
    /// <returns>
    /// A sequence containing the elements of the specified page.
    /// </returns>
    public static IEnumerable<T> Paginate<T>(this IEnumerable<T> source, int page, int pageSize)
    {
        return source.Skip((page - 1) * pageSize).Take(pageSize);
    }

    public static IEnumerable<TResult> SelectNullSafe<TSource, TResult>(
        this IEnumerable<TSource>? source,
        Func<TSource, TResult> selector
    )
    {
        if (source == null)
        {
            return new List<TResult>();
        }

        return source.Select(selector);
    }

    public static async Task<IEnumerable<TResult>> SelectAsync<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, Task<TResult>> asyncSelector
    )
    {
        var result = new List<TResult>();

        foreach (var item in source)
        {
            result.Add(await asyncSelector(item));
        }

        return result;
    }

    public static async Task<IEnumerable<TResult>> SelectAsyncWithIndex<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, int, Task<TResult>> asyncSelector
    )
    {
        var result = new List<TResult>();

        var index = 0;

        foreach (var item in source)
        {
            result.Add(await asyncSelector(item, index++));
        }

        return result;
    }

    public static async Task<TSource?> FirstOrDefaultAsync<TSource>(
        this IEnumerable<TSource> source,
        Func<TSource, Task<bool>> predicate
    )
        where TSource : class
    {
        foreach (var item in source)
        {
            if (await predicate(item))
            {
                return item;
            }
        }

        return default;
    }

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
        where T : class
    {
        return source.Where(item => item is not null)!;
    }

    public static IAsyncEnumerable<T> WhereNotNull<T>(this IAsyncEnumerable<T?> source)
        where T : class
    {
        return source.Where(item => item is not null)!;
    }

    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self) =>
        self.Select((item, index) => (item, index));

    public static bool IsSameAsIgnoringOrder<T>(this IEnumerable<T> first, IEnumerable<T> second)
    {
        var firstList = first.ToList();
        var secondList = second.ToList();

        var firstNotInSecond = firstList.Except(secondList);
        var secondNotInFirst = secondList.Except(firstList);

        return !(firstNotInSecond.Any() || secondNotInFirst.Any());
    }

    public static Tuple<T, T> ToTuple2<T>(this IEnumerable<T> collection)
        where T : class
    {
        var list = collection.ToList();

        if (list.Count != 2)
        {
            throw new ArgumentException(
                $"Expected 2 list items when constructing a 2-tuple, but found {list.Count}"
            );
        }

        return new Tuple<T, T>(list[0], list[1]);
    }

    public static Tuple<T, T, T> ToTuple3<T>(this IEnumerable<T> collection)
        where T : class
    {
        var list = collection.ToList();

        if (list.Count != 3)
        {
            throw new ArgumentException(
                $"Expected 3 list items when constructing a 3-tuple, but found {list.Count}"
            );
        }

        return new Tuple<T, T, T>(list[0], list[1], list[2]);
    }

    public static bool ContainsAll<T>(this IEnumerable<T> source, IEnumerable<T> values)
    {
        return values.All(id => source.Contains(id));
    }

    /// <summary>
    /// Order some objects, according to a string key, in natural order for humans to read.
    /// </summary>
    public static IOrderedEnumerable<T> NaturalOrderBy<T>(
        this IEnumerable<T> source,
        Func<T, string> keySelector,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase
    )
    {
        return source.OrderBy(keySelector, comparison.WithNaturalSort());
    }

    /// <summary>
    /// Subsequently order some objects, according to a string key, in natural order for humans to read.
    /// </summary>
    public static IOrderedEnumerable<T> NaturalThenBy<T>(
        this IOrderedEnumerable<T> source,
        Func<T, string> keySelector,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase
    )
    {
        return source.ThenBy(keySelector, comparison.WithNaturalSort());
    }

    public static List<(T1, T2)> Cartesian<T1, T2>(
        this IEnumerable<T1> list1,
        IEnumerable<T2>? list2
    )
    {
        return list2 == null
            ? []
            : list1.Join(list2, _ => true, _ => true, (t1, t2) => (t1, t2)).ToList();
    }

    public static List<(T1, T2, T3)> Cartesian<T1, T2, T3>(
        this IEnumerable<T1> list1,
        IEnumerable<T2>? list2,
        IEnumerable<T3>? list3
    )
    {
        return list2 == null || list3 == null
            ? []
            : list1
                .Join(list2, _ => true, _ => true, (t1, t2) => (t1, t2))
                .Join(list3, _ => true, _ => true, (tuple, t3) => (tuple.t1, tuple.t2, t3))
                .ToList();
    }
}
