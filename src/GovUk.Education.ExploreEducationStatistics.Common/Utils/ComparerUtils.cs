using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils
{
    public static class ComparerUtils
    {
        public static NullSafePropertyComparer<T> CreateComparerByProperty<T>(Func<T, object> propertyGetter)
        {
            return new NullSafePropertyComparer<T>(propertyGetter);    
        }
        
        public class NullSafePropertyComparer<T> : IEqualityComparer<T> 
        {
            private readonly Func<T, object> _propertyGetter;

            public NullSafePropertyComparer(Func<T, object> propertyGetter) 
            {
                _propertyGetter = propertyGetter;
            }

            public bool Equals(T x, T y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x == null || y == null)
                {
                    return false;
                }
                
                return Equals(_propertyGetter.Invoke(x), _propertyGetter.Invoke(y));
            }

            public int GetHashCode(T obj)
            {
                return _propertyGetter.Invoke(obj).GetHashCode();
            }
        }
    }
}