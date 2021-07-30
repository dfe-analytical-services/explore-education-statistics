using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public static class CollectionUtils
    {
        public static List<T> AsList<T>(params T[] objects)
        {
            return new List<T>(objects);
        }
        
        public static T[] AsArray<T>(params T[] objects)
        {
            return objects;
        }
    }
}