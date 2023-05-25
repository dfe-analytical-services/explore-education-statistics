#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class CollectionExtensions
{
    public static void AddRange<T>(this ICollection<T> source, params T[] items)
        => source.AddRange(items.ToList());

    public static void AddRange<T>(this ICollection<T> source, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            source.Add(item);
        }
    }
}
