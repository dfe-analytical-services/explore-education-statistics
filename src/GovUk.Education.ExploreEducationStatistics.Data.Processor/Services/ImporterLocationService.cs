#nullable enable
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class ImporterLocationService
    {
        private readonly IGuidGenerator _guidGenerator;
        private readonly ImporterMemoryCache _memoryCache;

        public ImporterLocationService(
            IGuidGenerator guidGenerator,
            ImporterMemoryCache memoryCache)
        {
            _guidGenerator = guidGenerator;
            _memoryCache = memoryCache;
        }

        public Location FindOrCreate(
            StatisticsDbContext context,
            GeographicLevel geographicLevel,
            Country country,
            EnglishDevolvedArea? englishDevolvedArea = null,
            Institution? institution = null,
            LocalAuthority? localAuthority = null,
            LocalAuthorityDistrict? localAuthorityDistrict = null,
            LocalEnterprisePartnership? localEnterprisePartnership = null,
            MayoralCombinedAuthority? mayoralCombinedAuthority = null,
            Mat? multiAcademyTrust = null,
            OpportunityArea? opportunityArea = null,
            ParliamentaryConstituency? parliamentaryConstituency = null,
            PlanningArea? planningArea = null,
            Provider? provider = null,
            Region? region = null,
            RscRegion? rscRegion = null,
            School? school = null,
            Sponsor? sponsor = null,
            Ward? ward = null)
        {
            var cacheKey = GetCacheKey(
                geographicLevel,
                country,
                englishDevolvedArea,
                institution,
                localAuthority,
                localAuthorityDistrict,
                localEnterprisePartnership,
                mayoralCombinedAuthority,
                multiAcademyTrust,
                opportunityArea,
                parliamentaryConstituency,
                planningArea,
                provider,
                region,
                rscRegion,
                school,
                sponsor,
                ward);

            if (_memoryCache.Cache.TryGetValue(cacheKey, out Location location))
            {
                return location;
            }

            location = LookupOrCreate(
                context,
                geographicLevel,
                country,
                englishDevolvedArea,
                institution,
                localAuthority,
                localAuthorityDistrict,
                localEnterprisePartnership,
                mayoralCombinedAuthority,
                multiAcademyTrust,
                opportunityArea,
                parliamentaryConstituency,
                planningArea,
                provider,
                region,
                rscRegion,
                school,
                sponsor,
                ward);
            _memoryCache.Cache.Set(cacheKey, location);

            return location;
        }

        private static string GetCacheKey(
            GeographicLevel geographicLevel,
            Country country,
            EnglishDevolvedArea? englishDevolvedArea,
            Institution? institution,
            LocalAuthority? localAuthority,
            LocalAuthorityDistrict? localAuthorityDistrict,
            LocalEnterprisePartnership? localEnterprisePartnership,
            MayoralCombinedAuthority? mayoralCombinedAuthority,
            Mat? multiAcademyTrust,
            OpportunityArea? opportunityArea,
            ParliamentaryConstituency? parliamentaryConstituency,
            PlanningArea? planningArea,
            Provider? provider,
            Region? region,
            RscRegion? rscRegion,
            School? school,
            Sponsor? sponsor,
            Ward? ward)
        {
            var locationAttributes = new ILocationAttribute?[]
            {
                country,
                englishDevolvedArea,
                institution,
                localAuthority,
                localAuthorityDistrict,
                localEnterprisePartnership,
                mayoralCombinedAuthority,
                multiAcademyTrust,
                parliamentaryConstituency,
                planningArea,
                provider,
                opportunityArea,
                region,
                rscRegion,
                school,
                sponsor,
                ward
            };

            var tokens = locationAttributes
                .WhereNotNull()
                .Select(attribute => attribute.GetCacheKey())
                .ToList();

            const char separator = '_';
            return $"{geographicLevel}{separator}{tokens.JoinToString(separator)}";
        }

        private Location LookupOrCreate(
            StatisticsDbContext context,
            GeographicLevel geographicLevel,
            Country country,
            EnglishDevolvedArea? englishDevolvedArea,
            Institution? institution,
            LocalAuthority? localAuthority,
            LocalAuthorityDistrict? localAuthorityDistrict,
            LocalEnterprisePartnership? localEnterprisePartnership,
            MayoralCombinedAuthority? mayoralCombinedAuthority,
            Mat? multiAcademyTrust,
            OpportunityArea? opportunityArea,
            ParliamentaryConstituency? parliamentaryConstituency,
            PlanningArea? planningArea,
            Provider? provider,
            Region? region,
            RscRegion? rscRegion,
            School? school,
            Sponsor? sponsor,
            Ward? ward)
        {
            var location = Lookup(
                context,
                geographicLevel,
                country,
                englishDevolvedArea,
                institution,
                localAuthority,
                localAuthorityDistrict,
                localEnterprisePartnership,
                mayoralCombinedAuthority,
                multiAcademyTrust,
                opportunityArea,
                parliamentaryConstituency,
                planningArea,
                provider,
                region,
                rscRegion,
                school,
                sponsor,
                ward);

            if (location == null)
            {
                var entityEntry = context.Location.Add(new Location
                {
                    Id = _guidGenerator.NewGuid(),
                    GeographicLevel = geographicLevel,
                    Country = country,
                    EnglishDevolvedArea = englishDevolvedArea,
                    Institution = institution,
                    LocalAuthority = localAuthority,
                    LocalAuthorityDistrict = localAuthorityDistrict,
                    LocalEnterprisePartnership = localEnterprisePartnership,
                    MayoralCombinedAuthority = mayoralCombinedAuthority,
                    MultiAcademyTrust = multiAcademyTrust,
                    OpportunityArea = opportunityArea,
                    ParliamentaryConstituency = parliamentaryConstituency,
                    PlanningArea = planningArea,
                    Provider = provider,
                    Region = region,
                    RscRegion = rscRegion,
                    School = school,
                    Sponsor = sponsor,
                    Ward = ward
                });

                return entityEntry.Entity;
            }

            return location;
        }

        private Location? Lookup(
            StatisticsDbContext context,
            GeographicLevel geographicLevel,
            Country country,
            EnglishDevolvedArea? englishDevolvedArea,
            Institution? institution,
            LocalAuthority? localAuthority,
            LocalAuthorityDistrict? localAuthorityDistrict,
            LocalEnterprisePartnership? localEnterprisePartnership,
            MayoralCombinedAuthority? mayoralCombinedAuthority,
            Mat? multiAcademyTrust,
            OpportunityArea? opportunityArea,
            ParliamentaryConstituency? parliamentaryConstituency,
            PlanningArea? planningArea,
            Provider? provider,
            Region? region,
            RscRegion? rscRegion,
            School? school,
            Sponsor? sponsor,
            Ward? ward)
        {
            var predicateBuilder = PredicateBuilder.True<Location>()
                .And(location => location.GeographicLevel == geographicLevel);

            predicateBuilder = predicateBuilder
                .And(location => location.Country_Code == country.Code
                                 && location.Country_Name == country.Name);

            predicateBuilder = predicateBuilder
                .And(location => location.EnglishDevolvedArea_Code == (englishDevolvedArea != null ? englishDevolvedArea.Code : null) 
                                 && location.EnglishDevolvedArea_Name == (englishDevolvedArea != null ? englishDevolvedArea.Name : null));

            predicateBuilder = predicateBuilder
                .And(location => location.Institution_Code == (institution != null ? institution.Code : null) 
                                 && location.Institution_Name == (institution != null ? institution.Name : null));

            // Also match the old LA code even if blank
            predicateBuilder = predicateBuilder
                .And(location =>
                    location.LocalAuthority_Code == (localAuthority != null && localAuthority.Code != null ? localAuthority.Code : null)
                    && location.LocalAuthority_OldCode == (localAuthority != null && localAuthority.OldCode != null ? localAuthority.OldCode : null)
                    && location.LocalAuthority_Name == (localAuthority != null ? localAuthority.Name : null));

            predicateBuilder = predicateBuilder
                .And(location => location.LocalAuthorityDistrict_Code == (localAuthorityDistrict != null ? localAuthorityDistrict.Code : null)
                                 && location.LocalAuthorityDistrict_Name == (localAuthorityDistrict != null ? localAuthorityDistrict.Name : null));

            predicateBuilder = predicateBuilder
                .And(location => location.LocalEnterprisePartnership_Code == (localEnterprisePartnership != null ? localEnterprisePartnership.Code : null)
                                 && location.LocalEnterprisePartnership_Name == (localEnterprisePartnership != null ? localEnterprisePartnership.Name : null));

            predicateBuilder = predicateBuilder
                .And(location => location.MayoralCombinedAuthority_Code == (mayoralCombinedAuthority != null ? mayoralCombinedAuthority.Code : null)
                                 && location.MayoralCombinedAuthority_Name == (mayoralCombinedAuthority != null ? mayoralCombinedAuthority.Name : null));

            predicateBuilder = predicateBuilder
                .And(location => location.MultiAcademyTrust_Code == (multiAcademyTrust != null ? multiAcademyTrust.Code : null)
                                 && location.MultiAcademyTrust_Name == (multiAcademyTrust != null ? multiAcademyTrust.Name : null));

            predicateBuilder = predicateBuilder
                .And(location => location.OpportunityArea_Code == (opportunityArea != null ? opportunityArea.Code : null)
                                 && location.OpportunityArea_Name == (opportunityArea != null ? opportunityArea.Name : null));

            predicateBuilder = predicateBuilder
                .And(location => location.ParliamentaryConstituency_Code == (parliamentaryConstituency != null ? parliamentaryConstituency.Code : null)
                                 && location.ParliamentaryConstituency_Name == (parliamentaryConstituency != null ? parliamentaryConstituency.Name : null));

            predicateBuilder = predicateBuilder
                .And(location => location.PlanningArea_Code == (planningArea != null ? planningArea.Code : null) 
                                 && location.PlanningArea_Name == (planningArea != null ? planningArea.Name : null));

            predicateBuilder = predicateBuilder
                .And(location => location.Provider_Code == (provider != null ? provider.Code : null)
                                 && location.Provider_Name == (provider != null ? provider.Name : null));

            predicateBuilder = predicateBuilder
                .And(location => location.Region_Code == (region != null ? region.Code : null)
                                 && location.Region_Name == (region != null ? region.Name : null));

            // Note that Name is not included in the predicate here as it is the same as the code
            predicateBuilder = predicateBuilder
                .And(location => location.RscRegion_Code == (rscRegion != null ? rscRegion.Code : null));

            predicateBuilder = predicateBuilder
                .And(location =>
                    location.School_Code == (school != null ? school.Code : null)
                    && location.School_Name == (school != null ? school.Name : null));

            predicateBuilder = predicateBuilder
                .And(location => location.Sponsor_Code == (sponsor != null ? sponsor.Code : null)
                                 && location.Sponsor_Name == (sponsor != null ? sponsor.Name : null));

            predicateBuilder = predicateBuilder
                .And(location => location.Ward_Code == (ward != null ? ward.Code : null)
                                 && location.Ward_Name == (ward != null ? ward.Name : null));

            // This can return multiple results because C# equality is translated directly to SQL equality
            // and our config of SqlServer is using the default case-insensitive collation
            // See https://docs.microsoft.com/en-us/ef/core/miscellaneous/collations-and-case-sensitivity
            var locations = context.Location
                .AsNoTracking()
                .Where(predicateBuilder)
                .ToList();

            // Perform case-sensitive comparison on the Name fields
            return locations.FirstOrDefault(location =>
                location.Country_Name == country.Name
                && location.EnglishDevolvedArea_Name == englishDevolvedArea?.Name
                && location.Institution_Name == institution?.Name
                && location.LocalAuthority_Name == localAuthority?.Name
                && location.LocalAuthorityDistrict_Name == localAuthorityDistrict?.Name
                && location.LocalEnterprisePartnership_Name == localEnterprisePartnership?.Name
                && location.MayoralCombinedAuthority_Name == mayoralCombinedAuthority?.Name
                && location.MultiAcademyTrust_Name == multiAcademyTrust?.Name
                && location.OpportunityArea_Name == opportunityArea?.Name
                && location.ParliamentaryConstituency_Name == parliamentaryConstituency?.Name
                && location.PlanningArea_Name == planningArea?.Name
                && location.Provider_Name == provider?.Name
                && location.Region_Name == region?.Name
                && location.RscRegion_Code == rscRegion?.Code // RscRegion codes function as the name
                && location.School_Name == school?.Name
                && location.Sponsor_Name == sponsor?.Name
                && location.Ward_Name == ward?.Name
            );
        }
    }
}
