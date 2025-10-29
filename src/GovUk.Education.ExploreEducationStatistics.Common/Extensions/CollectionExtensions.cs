namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class CollectionExtensions
{
    public static void AddRange<T>(this ICollection<T> source, params T[] items) => source.AddRange(items.ToList());

    public static void AddRange<T>(this ICollection<T> source, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            source.Add(item);
        }
    }

    public static int LastIndex<T>(this ICollection<T> source) => source.Count - 1;

    public static bool IsLastIndex<T>(this ICollection<T> source, int index) => index == source.LastIndex();
}
