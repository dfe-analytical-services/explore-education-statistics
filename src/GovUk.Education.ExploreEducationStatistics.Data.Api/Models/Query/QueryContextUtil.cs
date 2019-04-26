using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public static class QueryContextUtil
    {
        public static Expression<Func<T, bool>> GeographicLevelExpression<T>(GeographicLevel geographicLevel)
            where T : Observation
        {
            return x => x.GeographicLevel == geographicLevel;
        }

        public static Expression<Func<T, bool>> CountriesExpression<T>(IEnumerable<string> countries)
            where T : Observation
        {
            return x => countries == null || !countries.Any() ||
                        countries.Contains(x.Location.Country.Code);
        }
        
        public static Expression<Func<T, bool>> LocalAuthoritiesExpression<T>(IEnumerable<string> localAuthorities)
            where T : Observation
        {
            return x => localAuthorities == null || !localAuthorities.Any() ||
                        localAuthorities.Contains(x.Location.LocalAuthority.Code);
        }

        public static Expression<Func<T, bool>> LocalAuthorityDistrictsExpression<T>(
            IEnumerable<string> localAuthorityDistricts)
            where T : Observation
        {
            return x => localAuthorityDistricts == null || !localAuthorityDistricts.Any() ||
                        localAuthorityDistricts.Contains(x.Location.LocalAuthorityDistrict.Code);
        }

        public static Expression<Func<T, bool>> RegionsExpression<T>(IEnumerable<string> regions)
            where T : Observation
        {
            return x => regions == null || !regions.Any() || regions.Contains(x.Location.Region.Code);
        }

        public static Expression<Func<T, bool>> SubjectExpression<T>(long subjectId)
            where T : Observation
        {
            return x => x.SubjectId == subjectId;
        }

        public static Expression<Func<T, bool>> TimePeriodExpression<T>(IEnumerable<int> timePeriods)
            where T : Observation
        {
            return x => !timePeriods.Any() || timePeriods.Contains(x.Year);
        }
    }
}