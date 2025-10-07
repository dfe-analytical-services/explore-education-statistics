namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class DictionaryExtensions
{
    public static Dictionary<TKey, TValue> Filter<TKey, TValue>(
        this Dictionary<TKey, TValue> dictionary,
        Predicate<KeyValuePair<TKey, TValue>> predicate
    )
        where TKey : notnull
    {
        return dictionary.Where(pair => predicate(pair)).ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    public static TValue GetOrSet<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        where TKey : notnull
    {
        return dictionary.GetOrSet(key, () => value);
    }

    public static TValue GetOrSet<TKey, TValue>(
        this Dictionary<TKey, TValue> dictionary,
        TKey key,
        Func<TValue> supplier
    )
        where TKey : notnull
    {
        if (dictionary.ContainsKey(key))
        {
            return dictionary[key];
        }

        dictionary[key] = supplier();
        return dictionary[key];
    }
}
