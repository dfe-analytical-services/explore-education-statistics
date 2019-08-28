using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public static class ObservationPredicateBuilder
    {
        public static Expression<Func<Observation, bool>> Build(SubjectMetaQueryContext query)
        {
            var predicate = PredicateBuilder.True<Observation>()
                .And(observation => observation.SubjectId == query.SubjectId);

            if (query.TimePeriod != null)
            {
                // Don't use the observation.GetTimePeriod() extension in the expression here
                // or string interpolation as it can't be translated
                predicate = predicate.And(observation => GetTimePeriodRange(query.TimePeriod)
                    .Contains(observation.Year + "_" + observation.TimeIdentifier));
            }

            if (query.GeographicLevel != null)
            {
                predicate = predicate.And(observation => observation.GeographicLevel == query.GeographicLevel);
            }

            if (ObservationalUnitExists(query))
            {
                predicate = predicate.And(ObservationalUnitsPredicate(query));
            }

            return predicate;
        }

        private static Expression<Func<Observation, bool>> ObservationalUnitsPredicate(SubjectMetaQueryContext query)
        {
            var predicate = PredicateBuilder.False<Observation>();

            if (query.Country != null)
            {
                predicate = predicate.Or(CountryPredicate(query));
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

            return predicate;
        }

        private static bool ObservationalUnitExists(SubjectMetaQueryContext query)
        {
            return !(query.Country == null &&
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
                     query.Ward == null);
        }

        private static Expression<Func<Observation, bool>> CountryPredicate(SubjectMetaQueryContext query)
        {
            return ObservationalUnitPredicate(query, GeographicLevel.Country,
                observation => query.Country.Contains(observation.Location.Country.Code));
        }

        private static Expression<Func<Observation, bool>> InstitutionPredicate(SubjectMetaQueryContext query)
        {
            return ObservationalUnitPredicate(query, GeographicLevel.Institution,
                observation => query.Institution.Contains(observation.Location.Institution.Code));
        }

        private static Expression<Func<Observation, bool>> LocalAuthorityPredicate(SubjectMetaQueryContext query)
        {
            return ObservationalUnitPredicate(query, GeographicLevel.Local_Authority,
                observation => query.LocalAuthority.Contains(observation.Location.LocalAuthority.Code));
        }

        private static Expression<Func<Observation, bool>> LocalAuthorityDistrictPredicate(
            SubjectMetaQueryContext query)
        {
            return ObservationalUnitPredicate(query, GeographicLevel.Local_Authority_District,
                observation => query.LocalAuthorityDistrict.Contains(observation.Location.LocalAuthorityDistrict.Code));
        }

        private static Expression<Func<Observation, bool>> LocalEnterprisePartnershipPredicate(
            SubjectMetaQueryContext query)
        {
            return ObservationalUnitPredicate(query, GeographicLevel.Local_Enterprise_Partnership,
                observation =>
                    query.LocalEnterprisePartnership.Contains(observation.Location.LocalEnterprisePartnership.Code));
        }

        private static Expression<Func<Observation, bool>> MayoralCombinedAuthorityPredicate(
            SubjectMetaQueryContext query)
        {
            return ObservationalUnitPredicate(query, GeographicLevel.Mayoral_Combined_Authority,
                observation =>
                    query.MayoralCombinedAuthority.Contains(observation.Location.MayoralCombinedAuthority.Code));
        }

        private static Expression<Func<Observation, bool>> MultiAcademyTrustPredicate(SubjectMetaQueryContext query)
        {
            return ObservationalUnitPredicate(query, GeographicLevel.Multi_Academy_Trust,
                observation => query.MultiAcademyTrust.Contains(observation.Location.MultiAcademyTrust.Code));
        }

        private static Expression<Func<Observation, bool>> OpportunityAreaPredicate(SubjectMetaQueryContext query)
        {
            return ObservationalUnitPredicate(query, GeographicLevel.Opportunity_Area,
                observation => query.OpportunityArea.Contains(observation.Location.OpportunityArea.Code));
        }

        private static Expression<Func<Observation, bool>> ParliamentaryConstituencyPredicate(
            SubjectMetaQueryContext query)
        {
            return ObservationalUnitPredicate(query, GeographicLevel.Parliamentary_Constituency,
                observation =>
                    query.ParliamentaryConstituency.Contains(observation.Location.ParliamentaryConstituency.Code));
        }

        private static Expression<Func<Observation, bool>> RegionPredicate(SubjectMetaQueryContext query)
        {
            return ObservationalUnitPredicate(query, GeographicLevel.Region,
                observation => query.Region.Contains(observation.Location.Region.Code));
        }

        private static Expression<Func<Observation, bool>> RscRegionPredicate(SubjectMetaQueryContext query)
        {
            return ObservationalUnitPredicate(query, GeographicLevel.RSC_Region,
                observation => query.RscRegion.Contains(observation.Location.RscRegion.Code));
        }

        private static Expression<Func<Observation, bool>> SponsorPredicate(SubjectMetaQueryContext query)
        {
            return ObservationalUnitPredicate(query, GeographicLevel.Sponsor,
                observation => query.Sponsor.Contains(observation.Location.Sponsor.Code));
        }

        private static Expression<Func<Observation, bool>> WardPredicate(SubjectMetaQueryContext query)
        {
            return ObservationalUnitPredicate(query, GeographicLevel.Ward,
                observation => query.Ward.Contains(observation.Location.Ward.Code));
        }

        private static Expression<Func<Observation, bool>> ObservationalUnitPredicate(SubjectMetaQueryContext query,
            GeographicLevel geographicLevel, Expression<Func<Observation, bool>> expression)
        {
            var predicate = PredicateBuilder.True<Observation>()
                .And(expression);

            if (query.GeographicLevel == null)
            {
                predicate = predicate.And(observation => observation.GeographicLevel.Equals(geographicLevel));
            }

            return predicate;
        }

        private static IEnumerable<string> GetTimePeriodRange(TimePeriodQuery timePeriod)
        {
            if (timePeriod.StartCode.IsNumberOfTerms() || timePeriod.EndCode.IsNumberOfTerms())
            {
                return TimePeriodUtil.RangeForNumberOfTerms(timePeriod.StartYear, timePeriod.EndYear)
                    .Select(tuple => tuple.GetTimePeriod());
            }

            return TimePeriodUtil.Range(timePeriod).Select(tuple => tuple.GetTimePeriod());
        }
    }
}