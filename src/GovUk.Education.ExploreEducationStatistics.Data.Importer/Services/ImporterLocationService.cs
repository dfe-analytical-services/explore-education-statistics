using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services
{
    public class ImporterLocationService : BaseImporterService
    {
        public ImporterLocationService(ImporterMemoryCache cache) : base(cache)
        {
        }

        public Location Find(
            StatisticsDbContext context,
            Country country,
            Institution institution = null,
            LocalAuthority localAuthority = null,
            LocalAuthorityDistrict localAuthorityDistrict = null,
            LocalEnterprisePartnership localEnterprisePartnership = null,
            MayoralCombinedAuthority mayoralCombinedAuthority = null,
            Mat multiAcademyTrust = null,
            OpportunityArea opportunityArea = null,
            ParliamentaryConstituency parliamentaryConstituency = null,
            Region region = null,
            RscRegion rscRegion = null,
            Sponsor sponsor = null,
            Ward ward = null)
        {
            var cacheKey = GetCacheKey(country, institution, localAuthority, localAuthorityDistrict,
                localEnterprisePartnership, mayoralCombinedAuthority, multiAcademyTrust, opportunityArea,
                parliamentaryConstituency, region, rscRegion, sponsor, ward);

            if (GetCache().TryGetValue(cacheKey, out Location location))
            {
                return location;
            }

            location = LookupOrCreate(context, country, institution, localAuthority, localAuthorityDistrict,
                localEnterprisePartnership, mayoralCombinedAuthority, multiAcademyTrust, opportunityArea,
                parliamentaryConstituency, region, rscRegion, sponsor, ward);
            GetCache().Set(cacheKey, location);

            return location;
        }

        private static string GetCacheKey(Country country,
            Institution institution,
            LocalAuthority localAuthority,
            LocalAuthorityDistrict localAuthorityDistrict,
            LocalEnterprisePartnership localEnterprisePartnership,
            MayoralCombinedAuthority mayoralCombinedAuthority,
            Mat multiAcademyTrust,
            OpportunityArea opportunityArea,
            ParliamentaryConstituency parliamentaryConstituency,
            Region region,
            RscRegion rscRegion,
            Sponsor sponsor,
            Ward ward)
        {
            var observationalUnits = new IObservationalUnit[]
            {
                country, institution, localAuthority, localAuthorityDistrict, localEnterprisePartnership,
                mayoralCombinedAuthority, multiAcademyTrust, parliamentaryConstituency, opportunityArea, region,
                rscRegion, sponsor, ward
            };

            const string separator = "_";
            
            return string.Join(separator, observationalUnits
                .Where(unit => unit != null)
                .Select(unit => $"{unit.GetType()}:{(unit is LocalAuthority la ? la.GetCodeOrOldCodeIfEmpty() : unit.Code )}:{unit.Name}"));
        }

        private Location LookupOrCreate(
            StatisticsDbContext context,
            Country country,
            Institution institution = null,
            LocalAuthority localAuthority = null,
            LocalAuthorityDistrict localAuthorityDistrict = null,
            LocalEnterprisePartnership localEnterprisePartnership = null,
            MayoralCombinedAuthority mayoralCombinedAuthority = null,
            Mat multiAcademyTrust = null,
            OpportunityArea opportunityArea = null,
            ParliamentaryConstituency parliamentaryConstituency = null,
            Region region = null,
            RscRegion rscRegion = null,
            Sponsor sponsor = null,
            Ward ward = null)
        {
            var location = Lookup(
                context,
                country,
                institution,
                localAuthority,
                localAuthorityDistrict,
                localEnterprisePartnership,
                mayoralCombinedAuthority,
                multiAcademyTrust,
                opportunityArea,
                parliamentaryConstituency,
                region,
                rscRegion,
                sponsor,
                ward);

            if (location == null)
            {
                var entityEntry = context.Location.Add(new Location
                {
                    Country = country ?? Country.Empty(),
                    Institution = institution ?? Institution.Empty(),
                    LocalAuthority = localAuthority ?? LocalAuthority.Empty(),
                    LocalAuthorityDistrict = localAuthorityDistrict ?? LocalAuthorityDistrict.Empty(),
                    LocalEnterprisePartnership = localEnterprisePartnership ?? LocalEnterprisePartnership.Empty(),
                    MayoralCombinedAuthority = mayoralCombinedAuthority ?? MayoralCombinedAuthority.Empty(),
                    MultiAcademyTrust = multiAcademyTrust ?? Mat.Empty(),
                    OpportunityArea = opportunityArea ?? OpportunityArea.Empty(),
                    ParliamentaryConstituency = parliamentaryConstituency ?? ParliamentaryConstituency.Empty(),
                    Region = region ?? Region.Empty(),
                    RscRegion = rscRegion ?? RscRegion.Empty(),
                    Sponsor = sponsor ?? Sponsor.Empty(),
                    Ward = ward ?? Ward.Empty()
                });

                return entityEntry.Entity;
            }

            return location;
        }

        private Location Lookup(
            StatisticsDbContext context,
            Country country,
            Institution institution = null,
            LocalAuthority localAuthority = null,
            LocalAuthorityDistrict localAuthorityDistrict = null,
            LocalEnterprisePartnership localEnterprisePartnership = null,
            MayoralCombinedAuthority mayoralCombinedAuthority = null,
            Mat multiAcademyTrust = null,
            OpportunityArea opportunityArea = null,
            ParliamentaryConstituency parliamentaryConstituency = null,
            Region region = null,
            RscRegion rscRegion = null,
            Sponsor sponsor = null,
            Ward ward = null)
        {
            var predicateBuilder = PredicateBuilder.True<Location>()
                .And(location => location.Country.Code == country.Code && location.Country.Name == country.Name);

            predicateBuilder = predicateBuilder
                .And(location => location.Institution.Code == (institution != null ? institution.Code : null) 
                                 && location.Institution.Name == (institution != null ? institution.Name : null));

            // If no LA code then try using the old code
            if (localAuthority != null && localAuthority.Code == null)
            {
                predicateBuilder = predicateBuilder
                    .And(location => location.LocalAuthority.OldCode == (localAuthority != null && localAuthority.OldCode != null ? localAuthority.OldCode : null)
                                     && location.LocalAuthority.Name == (localAuthority != null ? localAuthority.Name : null));
            }
            else
            {
                predicateBuilder = predicateBuilder
                    .And(location => location.LocalAuthority.Code == (localAuthority != null && localAuthority.Code != null ? localAuthority.Code : null)
                                     && location.LocalAuthority.Name == (localAuthority != null ? localAuthority.Name : null)); 
            }
        
            predicateBuilder = predicateBuilder
                .And(location => location.LocalAuthorityDistrict.Code == (localAuthorityDistrict != null ? localAuthorityDistrict.Code : null)
                                 && location.LocalAuthorityDistrict.Name == (localAuthorityDistrict != null ? localAuthorityDistrict.Name : null));

            predicateBuilder = predicateBuilder
                .And(location => location.LocalEnterprisePartnership.Code == (localEnterprisePartnership != null ? localEnterprisePartnership.Code : null)
                                 && location.LocalEnterprisePartnership.Name == (localEnterprisePartnership != null ? localEnterprisePartnership.Name : null));

            predicateBuilder = predicateBuilder
                .And(location => location.MayoralCombinedAuthority.Code == (mayoralCombinedAuthority != null ? mayoralCombinedAuthority.Code : null)
                                 && location.MayoralCombinedAuthority.Name == (mayoralCombinedAuthority != null ? mayoralCombinedAuthority.Name : null));

            predicateBuilder = predicateBuilder
                .And(location => location.MultiAcademyTrust.Code == (multiAcademyTrust != null ? multiAcademyTrust.Code : null)
                                 && location.MultiAcademyTrust.Name == (multiAcademyTrust != null ? multiAcademyTrust.Name : null));

            predicateBuilder = predicateBuilder
                .And(location => location.OpportunityArea.Code == (opportunityArea != null ? opportunityArea.Code : null)
                                 && location.OpportunityArea.Name == (opportunityArea != null ? opportunityArea.Name : null));

            predicateBuilder = predicateBuilder
                .And(location => location.ParliamentaryConstituency.Code == (parliamentaryConstituency != null ? parliamentaryConstituency.Code : null)
                                 && location.ParliamentaryConstituency.Name == (parliamentaryConstituency != null ? parliamentaryConstituency.Name : null));

            predicateBuilder = predicateBuilder
                .And(location => location.Region.Code == (region != null ? region.Code : null)
                                 && location.Region.Name == (region != null ? region.Name : null));

            predicateBuilder = predicateBuilder
                .And(location => location.RscRegion.Code == (rscRegion != null ? rscRegion.Code : null)
                                 && location.RscRegion.Name == (rscRegion != null ? rscRegion.Name : null));

            predicateBuilder = predicateBuilder
                .And(location => location.Sponsor.Code == (sponsor != null ? sponsor.Code : null)
                                 && location.Sponsor.Name == (sponsor != null ? sponsor.Name : null));

            predicateBuilder = predicateBuilder
                .And(location => location.Ward.Code == (ward != null ? ward.Code : null)
                                 && location.Ward.Name == (ward != null ? ward.Name : null));

            return context.Location.AsNoTracking().FirstOrDefault(predicateBuilder);
        }
    }
}