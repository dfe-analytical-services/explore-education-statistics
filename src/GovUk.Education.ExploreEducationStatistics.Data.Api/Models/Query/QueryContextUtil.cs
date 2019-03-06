using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public static class QueryContextUtil
    {
        public static Expression<Func<T, bool>> PublicationIdExpression<T>(Guid publicationId)
            where T : ITidyData
        {
            return x => x.PublicationId == publicationId;
        }
        
        public static Expression<Func<T, bool>> LevelExpression<T>(Level level)
            where T : ITidyData
        {
            return x => x.Level == level;
        }

        public static Expression<Func<T, bool>> SchoolTypeExpression<T>(IEnumerable<SchoolType> schoolTypes)
            where T : ITidyData
        {
            return x => schoolTypes == null || !schoolTypes.Any() || schoolTypes.Contains(x.SchoolType);
        }

        public static Expression<Func<T, bool>> YearExpression<T>(IEnumerable<int> yearsQuery)
            where T : ITidyData
        {
            return x => !yearsQuery.Any() || yearsQuery.Contains(x.Year);
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