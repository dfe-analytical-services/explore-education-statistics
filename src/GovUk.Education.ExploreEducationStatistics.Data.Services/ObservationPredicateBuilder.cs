#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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

            if (query.LocationIds != null)
            {
                predicate = predicate.AndAlso(observation =>
                    query.LocationIds.Contains(observation.LocationId));
            }

            // TODO EES-3068 Migrate Location codes to ids in old Datablocks to remove this support for Location codes
            if (query.Locations != null)
            {
                if (query.Locations.GeographicLevel != null)
                {
                    predicate = predicate.AndAlso(observation =>
                        observation.GeographicLevel == query.Locations.GeographicLevel);
                }

                if (LocationAttributesExist(query.Locations))
                {
                    predicate = predicate.AndAlso(LocationAttributesPredicate(query.Locations));
                }
            }

            return predicate;
        }

        private static Expression<Func<Observation, bool>> LocationAttributesPredicate(LocationQuery query)
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

            if (query.Provider != null)
            {
                predicate = predicate.Or(ProviderPredicate(query));
            }

            if (query.Region != null)
            {
                predicate = predicate.Or(RegionPredicate(query));
            }

            if (query.RscRegion != null)
            {
                predicate = predicate.Or(RscRegionPredicate(query));
            }

            if (query.School != null)
            {
                predicate = predicate.Or(SchoolPredicate(query));
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

        private static bool LocationAttributesExist(LocationQuery query)
        {
            return !(query == null ||
                     query.Country.IsNullOrEmpty() &&
                     query.EnglishDevolvedArea.IsNullOrEmpty() &&
                     query.Institution.IsNullOrEmpty() &&
                     query.LocalAuthority.IsNullOrEmpty() &&
                     query.LocalAuthorityDistrict.IsNullOrEmpty() &&
                     query.LocalEnterprisePartnership.IsNullOrEmpty() &&
                     query.MultiAcademyTrust.IsNullOrEmpty() &&
                     query.MayoralCombinedAuthority.IsNullOrEmpty() &&
                     query.OpportunityArea.IsNullOrEmpty() &&
                     query.ParliamentaryConstituency.IsNullOrEmpty() &&
                     query.Region.IsNullOrEmpty() &&
                     query.RscRegion.IsNullOrEmpty() &&
                     query.Sponsor.IsNullOrEmpty() &&
                     query.Ward.IsNullOrEmpty() &&
                     query.PlanningArea.IsNullOrEmpty());
        }

        private static Expression<Func<Observation, bool>> CountryPredicate(LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.Country,
                observation => query.Country != null &&
                               query.Country.Contains(observation.Location.Country_Code));
        }

        private static Expression<Func<Observation, bool>> EnglishDevolvedAreaPredicate(LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.EnglishDevolvedArea,
                observation => query.EnglishDevolvedArea != null &&
                               query.EnglishDevolvedArea.Contains(observation.Location.EnglishDevolvedArea_Code));
        }

        private static Expression<Func<Observation, bool>> InstitutionPredicate(LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.Institution,
                observation => query.Institution != null &&
                               query.Institution.Contains(observation.Location.Institution_Code));
        }

        private static Expression<Func<Observation, bool>> LocalAuthorityPredicate(LocationQuery query)
        {
            var allLocalAuthorityCodes = query.LocalAuthority ?? new List<string>();
            var localAuthorityOldCodes = allLocalAuthorityCodes.Where(s => s.Length == 3).ToList();
            var localAuthorityNewCodes = allLocalAuthorityCodes.Except(localAuthorityOldCodes).ToList();

            return LocationAttributePredicate(query, GeographicLevel.LocalAuthority,
                observation => localAuthorityNewCodes.Contains(observation.Location.LocalAuthority_Code) ||
                               localAuthorityOldCodes.Contains(observation.Location.LocalAuthority_OldCode));
        }

        private static Expression<Func<Observation, bool>> LocalAuthorityDistrictPredicate(
            LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.LocalAuthorityDistrict,
                observation => query.LocalAuthorityDistrict != null &&
                               query.LocalAuthorityDistrict.Contains(observation.Location.LocalAuthorityDistrict_Code));
        }

        private static Expression<Func<Observation, bool>> LocalEnterprisePartnershipPredicate(
            LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.LocalEnterprisePartnership,
                observation => query.LocalEnterprisePartnership != null &&
                    query.LocalEnterprisePartnership.Contains(observation.Location.LocalEnterprisePartnership_Code));
        }

        private static Expression<Func<Observation, bool>> MayoralCombinedAuthorityPredicate(
            LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.MayoralCombinedAuthority,
                observation => query.MayoralCombinedAuthority != null &&
                    query.MayoralCombinedAuthority.Contains(observation.Location.MayoralCombinedAuthority_Code));
        }

        private static Expression<Func<Observation, bool>> MultiAcademyTrustPredicate(LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.MultiAcademyTrust,
                observation => query.MultiAcademyTrust != null &&
                    query.MultiAcademyTrust.Contains(observation.Location.MultiAcademyTrust_Code));
        }

        private static Expression<Func<Observation, bool>> OpportunityAreaPredicate(LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.OpportunityArea,
                observation => query.OpportunityArea != null &&
                    query.OpportunityArea.Contains(observation.Location.OpportunityArea_Code));
        }

        private static Expression<Func<Observation, bool>> ParliamentaryConstituencyPredicate(
            LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.ParliamentaryConstituency,
                observation => query.ParliamentaryConstituency != null &&
                    query.ParliamentaryConstituency.Contains(observation.Location.ParliamentaryConstituency_Code));
        }

        private static Expression<Func<Observation, bool>> ProviderPredicate(
            LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.Provider,
                observation => query.Provider != null &&
                    query.Provider.Contains(observation.Location.Provider_Code));
        }

        private static Expression<Func<Observation, bool>> RegionPredicate(LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.Region,
                observation => query.Region != null &&
                    query.Region.Contains(observation.Location.Region_Code));
        }

        private static Expression<Func<Observation, bool>> RscRegionPredicate(LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.RscRegion,
                observation => query.RscRegion != null &&
                               query.RscRegion.Contains(observation.Location.RscRegion_Code));
        }

        private static Expression<Func<Observation, bool>> SchoolPredicate(LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.School,
                observation => query.School != null &&
                    query.School.Contains(observation.Location.School_Code));
        }

        private static Expression<Func<Observation, bool>> SponsorPredicate(LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.Sponsor,
                observation => query.Sponsor != null &&
                               query.Sponsor.Contains(observation.Location.Sponsor_Code));
        }

        private static Expression<Func<Observation, bool>> WardPredicate(LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.Ward,
                observation => query.Ward != null &&
                    query.Ward.Contains(observation.Location.Ward_Code));
        }

        private static Expression<Func<Observation, bool>> PlanningAreaPredicate(LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.PlanningArea,
                observation => query.PlanningArea != null &&
                    query.PlanningArea.Contains(observation.Location.PlanningArea_Code));
        }

        private static Expression<Func<Observation, bool>> LocationAttributePredicate(LocationQuery query,
            GeographicLevel geographicLevel, Expression<Func<Observation, bool>> expression)
        {
            return query.GeographicLevel == null
                ? expression.AndAlso(observation => observation.GeographicLevel == geographicLevel)
                : expression;
        }
    }
}
