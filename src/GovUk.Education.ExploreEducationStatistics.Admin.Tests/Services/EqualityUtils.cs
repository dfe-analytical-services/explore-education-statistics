using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class EqualityUtils
    {
        public static IEqualityComparer<PartialDate> PartialDateEqualityComparer()
        {
            return EqualityComparer<PartialDate> ((x, y) => x?.Day == y?.Day && x?.Month == y?.Month && x?.Year == y?.Year, 
                x => new int?[]{x?.Day?.GetHashCode(), x?.Year?.GetHashCode(), x?.Year?.GetHashCode()}.Sum() ?? 0);
        }

        private static IEqualityComparer<T> EqualityComparer<T>(Func<T, T, bool> eq, Func<T, int> hash)
        {
            return new AnonEqualityComparer<T>(eq, hash);
        }

        private class AnonEqualityComparer<T> : IEqualityComparer<T>
        {
            private readonly Func<T, T, bool> _eq;
            private readonly Func<T, int> _hash;
            
            public AnonEqualityComparer(Func<T, T, bool> eq, Func<T, int> hash)
            {
                _eq = eq;
                _hash = hash;
            }

            public bool Equals(T x, T y)
            {
                return _eq(x, y);
            }

            public int GetHashCode(T obj)
            {
                return _hash(obj);
            }
        }
    }
}
