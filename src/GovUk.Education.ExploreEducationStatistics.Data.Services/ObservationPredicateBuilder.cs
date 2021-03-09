using System;
using System.Linq;
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public static class ObservationPredicateBuilder
    {
        public static Expression<Func<Observation, bool>> Build(SubjectMetaQueryContext query)
        {
            var predicate = PredicateBuilder.True<Observation>()
                .AndAlso(observation => observation.SubjectId == query.SubjectId);

            if (query.TimePeriod != null)
            {
                var timePeriodRange = TimePeriodUtil.Range(query.TimePeriod);

                var subPredicate = PredicateBuilder.False<Observation>();

                foreach (var tuple in timePeriodRange)
                {
                    var year = tuple.Year;
                    var timeIdentifier = tuple.TimeIdentifier;

                    subPredicate = subPredicate.Or(observation =>
                        observation.Year == year && observation.TimeIdentifier == timeIdentifier);
                }

                predicate = predicate.AndAlso(subPredicate);
            }

            if (query.Locations?.GeographicLevel != null)
            {
                predicate = predicate.AndAlso(observation =>
                    observation.GeographicLevel == query.Locations.GeographicLevel);
            }

            if (ObservationalUnitExists(query.Locations))
            {
                predicate = predicate.AndAlso(ObservationalUnitsPredicate(query.Locations));
            }

            return predicate;
        }

        private static Expression<Func<Observation, bool>> ObservationalUnitsPredicate(LocationQuery query)
        {
            var predicate = PredicateBuilder.False<Observation>();

            if (query.Country != null)
            {
                predicate = predicate.Or(CountryPredicate(query));
            }

            if (query.EnglishDevolvedArea != null)
            {
                predicate = predicate.Or(EnglishDevolvedAreaPredicate(query));
            }

            if (query.Institution != null)
            {
                predicate = predicate.Or(InstitutionPredicate(query));
            }

            if (query.LocalAuthority != null)
            {
                predicate = predicate.Or(LocalAuthorityPredicate(query));
            }

            if (query.LocalAuthorityDistrict != null)
            {
                predicate = predicate.Or(LocalAuthorityDistrictPredicate(query));
            }

            if (query.LocalEnterprisePartnership != null)
            {
                predicate = predicate.Or(LocalEnterprisePartnershipPredicate(query));
            }

            if (query.MayoralCombinedAuthority != null)
            {
                predicate = predicate.Or(MayoralCombinedAuthorityPredicate(query));
            }

            if (query.MultiAcademyTrust != null)
            {
                predicate = predicate.Or(MultiAcademyTrustPredicate(query));
            }

            if (query.OpportunityArea != null)
            {
                predicate = predicate.Or(OpportunityAreaPredicate(query));
            }

            if (query.ParliamentaryConstituency != null)
            {
                predicate = predicate.Or(ParliamentaryConstituencyPredicate(query));
            }

            if (query.Region != null)
            {
                predicate = predicate.Or(RegionPredicate(query));
            }

            if (query.RscRegion != null)
            {
                predicate = predicate.Or(RscRegionPredicate(query));
            }

            if (query.Sponsor != null)
            {
                predicate = predicate.Or(SponsorPredicate(query));
            }

            if (query.Ward != null)
            {
                predicate = predicate.Or(WardPredicate(query));
            }

            if (query.PlanningArea != null)
            {
                predicate = predicate.Or(PlanningAreaPredicate(query));
            }

            return predicate;
        }

        private static bool ObservationalUnitExists(LocationQuery query)
        {
            return !(query == null ||
                     query.Country == null &&
                     query.EnglishDevolvedArea == null &&
                     query.Institution == null &&
                     query.LocalAuthority == null &&
                     query.LocalAuthorityDistrict == null &&
                     query.LocalEnterprisePartnership == null &&
                     query.MultiAcademyTrust == null &&
                     query.MayoralCombinedAuthority == null &&
                     query.OpportunityArea == null &&
                     query.ParliamentaryConstituency == null &&
                     query.Region == null &&
                     query.RscRegion == null &&
                     query.Sponsor == null &&
                     query.Ward == null &&
                     query.PlanningArea == null);
        }

        private static Expression<Func<Observation, bool>> CountryPredicate(LocationQuery query)
        {
            return ObservationalUnitPredicate(query, GeographicLevel.Country,
                observation => query.Country.Contains(observation.Location.Country_Code));
        }

        private static Expression<Func<Observation, bool>> EnglishDevolvedAreaPredicate(LocationQuery query)
        {
            return ObservationalUnitPredicate(query, GeographicLevel.EnglishDevolvedArea,
                observation => query.EnglishDevolvedArea.Contains(observation.Location.EnglishDevolvedArea_Code));
        }

        private static Expression<Func<Observation, bool>> InstitutionPredicate(LocationQuery query)
        {
            return ObservationalUnitPredicate(query, GeographicLevel.Institution,
                observation => query.Institution.Contains(observation.Location.Institution_Code));
        }

        private static Expression<Func<Observation, bool>> LocalAuthorityPredicate(LocationQuery query)
        {
            var localAuthorityOldCodes = query.LocalAuthority.Where(s => s.Length == 3).ToList();
            var localAuthorityCodes = query.LocalAuthority.Except(localAuthorityOldCodes).ToList();

            return ObservationalUnitPredicate(query, GeographicLevel.LocalAuthority,
                observation => localAuthorityCodes.Contains(observation.Location.LocalAuthority_Code) ||
                               localAuthorityOldCodes.Contains(observation.Location.LocalAuthority_OldCode));
        }

        private static Expression<Func<Observation, bool>> LocalAuthorityDistrictPredicate(
            LocationQuery query)
        {
            return ObservationalUnitPredicate(query, GeographicLevel.LocalAuthorityDistrict,
                observation => query.LocalAuthorityDistrict.Contains(observation.Location.LocalAuthorityDistrict_Code));
        }

        private static Expression<Func<Observation, bool>> LocalEnterprisePartnershipPredicate(
            LocationQuery query)
        {
            return ObservationalUnitPredicate(query, GeographicLevel.LocalEnterprisePartnership,
                observation =>
                    query.LocalEnterprisePartnership.Contains(observation.Location.LocalEnterprisePartnership_Code));
        }

        private static Expression<Func<Observation, bool>> MayoralCombinedAuthorityPredicate(
            LocationQuery query)
        {
            return ObservationalUnitPredicate(query, GeographicLevel.MayoralCombinedAuthority,
                observation =>
                    query.MayoralCombinedAuthority.Contains(observation.Location.MayoralCombinedAuthority_Code));
        }

        private static Expression<Func<Observation, bool>> MultiAcademyTrustPredicate(LocationQuery query)
        {
            return ObservationalUnitPredicate(query, GeographicLevel.MultiAcademyTrust,
                observation => query.MultiAcademyTrust.Contains(observation.Location.MultiAcademyTrust_Code));
        }

        private static Expression<Func<Observation, bool>> OpportunityAreaPredicate(LocationQuery query)
        {
            return ObservationalUnitPredicate(query, GeographicLevel.OpportunityArea,
                observation => query.OpportunityArea.Contains(observation.Location.OpportunityArea_Code));
        }

        private static Expression<Func<Observation, bool>> ParliamentaryConstituencyPredicate(
            LocationQuery query)
        {
            return ObservationalUnitPredicate(query, GeographicLevel.ParliamentaryConstituency,
                observation =>
                    query.ParliamentaryConstituency.Contains(observation.Location.ParliamentaryConstituency_Code));
        }

        private static Expression<Func<Observation, bool>> RegionPredicate(LocationQuery query)
        {
            return ObservationalUnitPredicate(query, GeographicLevel.Region,
                observation => query.Region.Contains(observation.Location.Region_Code));
        }

        private static Expression<Func<Observation, bool>> RscRegionPredicate(LocationQuery query)
        {
            return ObservationalUnitPredicate(query, GeographicLevel.RscRegion,
                observation => query.RscRegion.Contains(observation.Location.RscRegion_Code));
        }

        private static Expression<Func<Observation, bool>> SponsorPredicate(LocationQuery query)
        {
            return ObservationalUnitPredicate(query, GeographicLevel.Sponsor,
                observation => query.Sponsor.Contains(observation.Location.Sponsor_Code));
        }

        private static Expression<Func<Observation, bool>> WardPredicate(LocationQuery query)
        {
            return ObservationalUnitPredicate(query, GeographicLevel.Ward,
                observation => query.Ward.Contains(observation.Location.Ward_Code));
        }

        private static Expression<Func<Observation, bool>> PlanningAreaPredicate(LocationQuery query)
        {
            return ObservationalUnitPredicate(query, GeographicLevel.PlanningArea,
                observation => query.PlanningArea.Contains(observation.Location.PlanningArea_Code));
        }

        private static Expression<Func<Observation, bool>> ObservationalUnitPredicate(LocationQuery query,
            GeographicLevel geographicLevel, Expression<Func<Observation, bool>> expression)
        {
            return query.GeographicLevel == null
                ? expression.AndAlso(observation => observation.GeographicLevel == geographicLevel)
                : expression;
        }
    }
}
