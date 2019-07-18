using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions
{
    public static class TimeIdentifierCategoryExtensions
    {
        public static TimeIdentifier[] GetTimeIdentifiers(this TimeIdentifierCategory category)
        {
            var all = (TimeIdentifier[]) Enum.GetValues(typeof(TimeIdentifier));
            return all.Where(i => i.GetEnumAttribute<TimeIdentifierMetaAttribute>().Category == category).ToArray();
        }
    }
}