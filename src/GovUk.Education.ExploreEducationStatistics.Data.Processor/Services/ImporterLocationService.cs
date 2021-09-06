using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class ImporterLocationService : BaseImporterService
    {
        private readonly IGuidGenerator _guidGenerator;

        public ImporterLocationService(
            IGuidGenerator guidGenerator,
            ImporterMemoryCache cache) : base(cache)
        {
            _guidGenerator = guidGenerator;
        }

        public Location FindOrCreate(
            StatisticsDbContext context,
            Country country,
            EnglishDevolvedArea englishDevolvedArea = null,
            Institution institution = null,
            LocalAuthority localAuthority = null,
            LocalAuthorityDistrict localAuthorityDistrict = null,
            LocalEnterprisePartnership localEnterprisePartnership = null,
            MayoralCombinedAuthority mayoralCombinedAuthority = null,
            Mat multiAcademyTrust = null,
            OpportunityArea opportunityArea = null,
            ParliamentaryConstituency parliamentaryConstituency = null,
            Provider provider = null,
            Region region = null,
            RscRegion rscRegion = null,
            School school = null,
            Sponsor sponsor = null,
            Ward ward = null,
            PlanningArea planningArea = null)
        {
            var cacheKey = GetCacheKey(
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
                provider,
                region,
                rscRegion,
                school,
                sponsor,
                ward,
                planningArea);

            if (GetCache().TryGetValue(cacheKey, out Location location))
            {
                return location;
            }

            location = LookupOrCreate(
                context,
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
                provider,
                region,
                rscRegion,
                school,
                sponsor,
                ward,
                planningArea);
            GetCache().Set(cacheKey, location);

            return location;
        }

        private static string GetCacheKey(
            Country country,
            EnglishDevolvedArea englishDevolvedArea,
            Institution institution,
            LocalAuthority localAuthority,
            LocalAuthorityDistrict localAuthorityDistrict,
            LocalEnterprisePartnership localEnterprisePartnership,
            MayoralCombinedAuthority mayoralCombinedAuthority,
            Mat multiAcademyTrust,
            OpportunityArea opportunityArea,
            ParliamentaryConstituency parliamentaryConstituency,
            Provider provider,
            Region region,
            RscRegion rscRegion,
            School school,
            Sponsor sponsor,
            Ward ward,
            PlanningArea planningArea)
        {
            var observationalUnits = new ObservationalUnit[]
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
                provider,
                opportunityArea,
                region,
                rscRegion,
                school,
                sponsor,
                ward,
                planningArea
            };

            const string separator = "_";

            return string.Join(separator, observationalUnits
                .Where(unit => unit != null)
                .Select(unit =>
                    $"{unit.GetType()}:{(unit is LocalAuthority la ? la.GetCodeOrOldCodeIfEmpty() : unit.Code)}:{unit.Name}"));
        }

        private Location LookupOrCreate(
            StatisticsDbContext context,
            Country country,
            EnglishDevolvedArea englishDevolvedArea = null,
            Institution institution = null,
            LocalAuthority localAuthority = null,
            LocalAuthorityDistrict localAuthorityDistrict = null,
            LocalEnterprisePartnership localEnterprisePartnership = null,
            MayoralCombinedAuthority mayoralCombinedAuthority = null,
            Mat multiAcademyTrust = null,
            OpportunityArea opportunityArea = null,
            ParliamentaryConstituency parliamentaryConstituency = null,
            Provider provider = null,
            Region region = null,
            RscRegion rscRegion = null,
            School school = null,
            Sponsor sponsor = null,
            Ward ward = null,
            PlanningArea planningArea = null)
        {
            var location = Lookup(
                context,
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
                provider,
                region,
                rscRegion,
                school,
                sponsor,
                ward,
                planningArea);

            if (location == null)
            {
                var entityEntry = context.Location.Add(new Location
                {
                    Id = _guidGenerator.NewGuid(),
                    Country = country ?? Country.Empty(),
                    EnglishDevolvedArea = englishDevolvedArea ?? EnglishDevolvedArea.Empty(),
                    Institution = institution ?? Institution.Empty(),
                    LocalAuthority = localAuthority ?? LocalAuthority.Empty(),
                    LocalAuthorityDistrict = localAuthorityDistrict ?? LocalAuthorityDistrict.Empty(),
                    LocalEnterprisePartnership = localEnterprisePartnership ?? LocalEnterprisePartnership.Empty(),
                    MayoralCombinedAuthority = mayoralCombinedAuthority ?? MayoralCombinedAuthority.Empty(),
                    MultiAcademyTrust = multiAcademyTrust ?? Mat.Empty(),
                    OpportunityArea = opportunityArea ?? OpportunityArea.Empty(),
                    ParliamentaryConstituency = parliamentaryConstituency ?? ParliamentaryConstituency.Empty(),
                    Provider = provider ?? Provider.Empty(),
                    Region = region ?? Region.Empty(),
                    RscRegion = rscRegion ?? RscRegion.Empty(),
                    School = school ?? School.Empty(),
                    Sponsor = sponsor ?? Sponsor.Empty(),
                    Ward = ward ?? Ward.Empty(),
                    PlanningArea = planningArea ?? PlanningArea.Empty()
                });

                return entityEntry.Entity;
            }

            return location;
        }

        private Location Lookup(
            StatisticsDbContext context,
            Country country,
            EnglishDevolvedArea englishDevolvedArea = null,
            Institution institution = null,
            LocalAuthority localAuthority = null,
            LocalAuthorityDistrict localAuthorityDistrict = null,
            LocalEnterprisePartnership localEnterprisePartnership = null,
            MayoralCombinedAuthority mayoralCombinedAuthority = null,
            Mat multiAcademyTrust = null,
            OpportunityArea opportunityArea = null,
            ParliamentaryConstituency parliamentaryConstituency = null,
            Provider provider = null,
            Region region = null,
            RscRegion rscRegion = null,
            School school = null,
            Sponsor sponsor = null,
            Ward ward = null,
            PlanningArea planningArea = null)
        {
            var predicateBuilder = PredicateBuilder.True<Location>()
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
            
            predicateBuilder = predicateBuilder
                .And(location => location.PlanningArea_Code == (planningArea != null ? planningArea.Code : null)
                                 && location.PlanningArea_Name == (planningArea != null ? planningArea.Name : null));

            // Can return multiple locations because SQL comparison is case insensitive
            var locations = context.Location.AsNoTracking()
                .Where(predicateBuilder)
                .ToList();

            return locations.FirstOrDefault(location =>
                location.Country_Name == country?.Name
                && location.EnglishDevolvedArea_Name == englishDevolvedArea?.Name
                && location.Institution_Name == institution?.Name
                && location.LocalAuthority_Name == localAuthority?.Name
                && location.LocalAuthorityDistrict_Name == localAuthorityDistrict?.Name
                && location.LocalEnterprisePartnership_Name == localEnterprisePartnership?.Name
                && location.MayoralCombinedAuthority_Name == mayoralCombinedAuthority?.Name
                && location.MultiAcademyTrust_Name == multiAcademyTrust?.Name
                && location.OpportunityArea_Name == opportunityArea?.Name
                && location.ParliamentaryConstituency_Name == parliamentaryConstituency?.Name
                && location.Provider_Name == provider?.Name
                && location.Region_Name == region?.Name
                && location.RscRegion_Code == rscRegion?.Code // RscRegion codes function as the name
                && location.School_Name == school?.Name
                && location.Sponsor_Name == sponsor?.Name
                && location.Ward_Name == ward?.Name
                && location.PlanningArea_Name == planningArea?.Name
            );
        }
    }
}
