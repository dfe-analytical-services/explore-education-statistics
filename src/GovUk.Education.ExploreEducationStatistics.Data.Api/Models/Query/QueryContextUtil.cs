using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public static class QueryContextUtil
    {
        public static Expression<Func<T, bool>> ReleaseExpression<T>(long releaseId)
            where T : ITidyData
        {
            return x => x.ReleaseId == releaseId;
        }

        public static Expression<Func<T, bool>> LevelExpression<T>(Level level)
            where T : ITidyData
        {
            return x => x.Level == level;
        }

        public static Expression<Func<T, bool>> RegionsExpression<T>(IEnumerable<string> regions)
            where T : IGeographicData
        {
            return x => regions == null || !regions.Any() || regions.Contains(x.RegionCode);
        }

        public static Expression<Func<T, bool>> LocalAuthoritiesExpression<T>(IEnumerable<string> localAuthorities)
            where T : IGeographicData
        {
            return x => localAuthorities == null || !localAuthorities.Any() ||
                        localAuthorities.Contains(x.LocalAuthorityCode);
        }

        public static Expression<Func<T, bool>> SchoolTypeExpression<T>(IEnumerable<SchoolType> schoolTypes)
            where T : ITidyData
        {
            return x => schoolTypes == null || !schoolTypes.Any() || schoolTypes.Contains(x.SchoolType);
        }

        public static Expression<Func<T, bool>> TimePeriodExpression<T>(IEnumerable<int> timePeriods)
            where T : ITidyData
        {
            return x => !timePeriods.Any() || timePeriods.Contains(x.TimePeriod);
        }

        public static Expression<Func<T, bool>> CharacteristicsQuery<T>(IEnumerable<string> characteristics)
            where T : ICharacteristicData
        {
            return x => characteristics == null ||
                        !characteristics.Any() ||
                        characteristics.Contains(x.CharacteristicName);
        }
    }
}