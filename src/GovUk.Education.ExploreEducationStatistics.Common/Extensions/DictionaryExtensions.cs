using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions
{
    public static class DictionaryExtensions
    {
        public static Dictionary<TKey, TValue> Filter<TKey, TValue>(
            this Dictionary<TKey, TValue> dictionary,
            Predicate<KeyValuePair<TKey, TValue>> predicate)
        {
            return dictionary
                .Where(pair => predicate(pair))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public static TValue GetOrSet<TKey, TValue>(
            this Dictionary<TKey, TValue> dictionary,
            TKey key,
            TValue value)
        {
            return dictionary.GetOrSet(key, () => value);
        }

        public static TValue GetOrSet<TKey, TValue>(
            this Dictionary<TKey, TValue> dictionary,
            TKey key,
            Func<TValue> supplier)
        {
            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }

            dictionary[key] = supplier();
            return dictionary[key];
        }
    }
}