using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions
{
    public static class DictionaryExtensions
    {
        public static Dictionary<TKey, TValue> Filter<TKey, TValue>(this Dictionary<TKey, TValue> dictionary,
            Predicate<KeyValuePair<TKey, TValue>> predicate)
        {
            return dictionary
                .Where(pair => predicate(pair))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}