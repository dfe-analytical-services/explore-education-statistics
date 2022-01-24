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

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public static class LocationPredicateBuilder
    {
        public static Expression<Func<Location, bool>> Build(
            IEnumerable<Guid>? locationIds,
            LocationQuery? locationCodes)
        {
            var predicate = PredicateBuilder.True<Location>();

            if (locationIds != null)
            {
                predicate = predicate.AndAlso(location =>
                    locationIds.Contains(location.Id));
            }

            // TODO EES-3068 Migrate Location codes to ids in old Datablocks to remove this support for Location codes
            if (locationCodes != null)
            {
                if (locationCodes.GeographicLevel != null)
                {
                    predicate = predicate.AndAlso(location => location.GeographicLevel == locationCodes.GeographicLevel);
                }

                if (LocationAttributesExist(locationCodes))
                {
                    predicate = predicate.AndAlso(LocationAttributesPredicate(locationCodes));
                }
            }

            return predicate;
        }

        private static Expression<Func<Location, bool>> LocationAttributesPredicate(LocationQuery query)
        {
            var predicate = PredicateBuilder.False<Location>();

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
            return !(query.Country.IsNullOrEmpty() &&
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

        private static Expression<Func<Location, bool>> CountryPredicate(LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.Country,
                location => query.Country != null &&
                               query.Country.Contains(location.Country_Code));
        }

        private static Expression<Func<Location, bool>> EnglishDevolvedAreaPredicate(LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.EnglishDevolvedArea,
                location => query.EnglishDevolvedArea != null &&
                               query.EnglishDevolvedArea.Contains(location.EnglishDevolvedArea_Code));
        }

        private static Expression<Func<Location, bool>> InstitutionPredicate(LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.Institution,
                location => query.Institution != null &&
                               query.Institution.Contains(location.Institution_Code));
        }

        private static Expression<Func<Location, bool>> LocalAuthorityPredicate(LocationQuery query)
        {
            var allLocalAuthorityCodes = query.LocalAuthority ?? new List<string>();
            var localAuthorityOldCodes = allLocalAuthorityCodes.Where(s => s.Length == 3).ToList();
            var localAuthorityNewCodes = allLocalAuthorityCodes.Except(localAuthorityOldCodes).ToList();

            return LocationAttributePredicate(query, GeographicLevel.LocalAuthority,
                location => localAuthorityNewCodes.Contains(location.LocalAuthority_Code) ||
                               localAuthorityOldCodes.Contains(location.LocalAuthority_OldCode));
        }

        private static Expression<Func<Location, bool>> LocalAuthorityDistrictPredicate(
            LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.LocalAuthorityDistrict,
                location => query.LocalAuthorityDistrict != null &&
                               query.LocalAuthorityDistrict.Contains(location.LocalAuthorityDistrict_Code));
        }

        private static Expression<Func<Location, bool>> LocalEnterprisePartnershipPredicate(
            LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.LocalEnterprisePartnership,
                location => query.LocalEnterprisePartnership != null &&
                    query.LocalEnterprisePartnership.Contains(location.LocalEnterprisePartnership_Code));
        }

        private static Expression<Func<Location, bool>> MayoralCombinedAuthorityPredicate(
            LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.MayoralCombinedAuthority,
                location => query.MayoralCombinedAuthority != null &&
                    query.MayoralCombinedAuthority.Contains(location.MayoralCombinedAuthority_Code));
        }

        private static Expression<Func<Location, bool>> MultiAcademyTrustPredicate(LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.MultiAcademyTrust,
                location => query.MultiAcademyTrust != null &&
                    query.MultiAcademyTrust.Contains(location.MultiAcademyTrust_Code));
        }

        private static Expression<Func<Location, bool>> OpportunityAreaPredicate(LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.OpportunityArea,
                location => query.OpportunityArea != null &&
                    query.OpportunityArea.Contains(location.OpportunityArea_Code));
        }

        private static Expression<Func<Location, bool>> ParliamentaryConstituencyPredicate(
            LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.ParliamentaryConstituency,
                location => query.ParliamentaryConstituency != null &&
                    query.ParliamentaryConstituency.Contains(location.ParliamentaryConstituency_Code));
        }

        private static Expression<Func<Location, bool>> ProviderPredicate(
            LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.Provider,
                location => query.Provider != null &&
                    query.Provider.Contains(location.Provider_Code));
        }

        private static Expression<Func<Location, bool>> RegionPredicate(LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.Region,
                location => query.Region != null &&
                    query.Region.Contains(location.Region_Code));
        }

        private static Expression<Func<Location, bool>> RscRegionPredicate(LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.RscRegion,
                location => query.RscRegion != null &&
                               query.RscRegion.Contains(location.RscRegion_Code));
        }

        private static Expression<Func<Location, bool>> SchoolPredicate(LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.School,
                location => query.School != null &&
                    query.School.Contains(location.School_Code));
        }

        private static Expression<Func<Location, bool>> SponsorPredicate(LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.Sponsor,
                location => query.Sponsor != null &&
                               query.Sponsor.Contains(location.Sponsor_Code));
        }

        private static Expression<Func<Location, bool>> WardPredicate(LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.Ward,
                location => query.Ward != null &&
                    query.Ward.Contains(location.Ward_Code));
        }

        private static Expression<Func<Location, bool>> PlanningAreaPredicate(LocationQuery query)
        {
            return LocationAttributePredicate(query, GeographicLevel.PlanningArea,
                location => query.PlanningArea != null &&
                    query.PlanningArea.Contains(location.PlanningArea_Code));
        }

        private static Expression<Func<Location, bool>> LocationAttributePredicate(LocationQuery query,
            GeographicLevel geographicLevel, Expression<Func<Location, bool>> expression)
        {
            return query.GeographicLevel == null
                ? expression.AndAlso(location => location.GeographicLevel == geographicLevel)
                : expression;
        }
    }
}
