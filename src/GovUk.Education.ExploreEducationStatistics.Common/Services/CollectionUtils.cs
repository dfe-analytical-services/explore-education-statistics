#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public static class CollectionUtils
    {
        public static List<T> AsList<T>(params T[] objects)
        {
            return ListOf(objects);
        }

        public static List<T> ListOf<T>(params T[] objects)
        {
            return new List<T>(objects);
        }

        public static HashSet<T> SetOf<T>(params T[] objects)
        {
            return new HashSet<T>(objects);
        }

        public static Tuple<T1, T2> TupleOf<T1, T2>(T1 obj1, T2? obj2)
        {
            return new Tuple<T1, T2>(obj1, obj2!);
        }
    }
}
