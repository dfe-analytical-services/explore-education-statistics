using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class ImporterLocationService : BaseImporterService
    {
        private readonly IGuidGenerator _guidGenerator;
        private readonly ILogger<ImporterLocationService> _logger;

        public ImporterLocationService(
            IGuidGenerator guidGenerator,
            ImporterMemoryCache cache,
            ILogger<ImporterLocationService> logger) : base(cache)
        {
            _guidGenerator = guidGenerator;
            _logger = logger;
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
            Ward ward = null,
            PlanningArea planningArea = null)
        {
            return _logger.TraceTime(() =>
            {
                var cacheKey = GetCacheKey(country, institution, localAuthority, localAuthorityDistrict,
                    localEnterprisePartnership, mayoralCombinedAuthority, multiAcademyTrust, opportunityArea,
                    parliamentaryConstituency, region, rscRegion, sponsor, ward, planningArea);

                if (GetCache().TryGetValue(cacheKey, out Location location))
                {
                    return location;
                }
                
                location = LookupOrCreate(context, country, institution, localAuthority, localAuthorityDistrict,
                    localEnterprisePartnership, mayoralCombinedAuthority, multiAcademyTrust, opportunityArea,
                    parliamentaryConstituency, region, rscRegion, sponsor, ward, planningArea);
                GetCache().Set(cacheKey, location);

                return location;
                
            }, "look up a location or create a new one", true);
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
            Ward ward,
            PlanningArea planningArea)
        {
            var observationalUnits = new IObservationalUnit[]
            {
                country, institution, localAuthority, localAuthorityDistrict, localEnterprisePartnership,
                mayoralCombinedAuthority, multiAcademyTrust, parliamentaryConstituency, opportunityArea, region,
                rscRegion, sponsor, ward, planningArea
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
            Ward ward = null,
            PlanningArea planningArea = null)
        {
            var location = _logger.TraceTime(() => Lookup(
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
                ward,
                planningArea),
                "look up a possibly existing Location");

            if (location == null)
            {
                var entityEntry = _logger.TraceTime(() => context.Location.Add(new Location
                {
                    Id = _guidGenerator.NewGuid(),
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
                    Ward = ward ?? Ward.Empty(),
                    PlanningArea = planningArea ?? PlanningArea.Empty()
                }), 
                    "add a new Location to the db context");

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
            Ward ward = null,
            PlanningArea planningArea = null)
        {
            var predicateBuilder = PredicateBuilder.True<Location>()
                .And(location => location.Country_Code == country.Code 
                                 && location.Country_Name == country.Name);

            if (institution != null)
            {
                predicateBuilder = predicateBuilder
                    .And(location => location.Institution_Code == institution.Code 
                                     && location.Institution_Name == institution.Name);
            }

            if (localAuthority != null)
            {
                // Also match the old LA code even if blank
                predicateBuilder = predicateBuilder
                    .And(location => location.LocalAuthority_Code == localAuthority.Code
                        && location.LocalAuthority_OldCode == localAuthority.OldCode
                        && location.LocalAuthority_Name == localAuthority.Name);
            }

            if (localAuthorityDistrict != null)
            {
                predicateBuilder = predicateBuilder
                    .And(location =>
                        location.LocalAuthorityDistrict_Code == localAuthorityDistrict.Code
                        && location.LocalAuthorityDistrict_Name == localAuthorityDistrict.Name);
            }

            if (localEnterprisePartnership != null)
            {
                predicateBuilder = predicateBuilder
                    .And(location =>
                        location.LocalEnterprisePartnership_Code == localEnterprisePartnership.Code
                        && location.LocalEnterprisePartnership_Name == localEnterprisePartnership.Name);
            }

            if (mayoralCombinedAuthority != null)
            {
                predicateBuilder = predicateBuilder
                    .And(location => location.MayoralCombinedAuthority_Code == mayoralCombinedAuthority.Code
                                     && location.MayoralCombinedAuthority_Name == mayoralCombinedAuthority.Name);
            }

            if (multiAcademyTrust != null)
            {
                predicateBuilder = predicateBuilder
                    .And(location => location.MultiAcademyTrust_Code == multiAcademyTrust.Code
                                     && location.MultiAcademyTrust_Name == multiAcademyTrust.Name);
            }

            if (opportunityArea != null)
            {
                predicateBuilder = predicateBuilder
                    .And(location => location.OpportunityArea_Code == opportunityArea.Code
                                     && location.OpportunityArea_Name == opportunityArea.Name);
            }

            if (parliamentaryConstituency != null)
            {
                predicateBuilder = predicateBuilder
                    .And(location => location.ParliamentaryConstituency_Code == parliamentaryConstituency.Code
                                     && location.ParliamentaryConstituency_Name == parliamentaryConstituency.Name);
            }

            if (region != null)
            {
                predicateBuilder = predicateBuilder
                    .And(location => location.Region_Code == region.Code
                                     && location.Region_Name == region.Name);
            }
            
            // Note that Name is not included in the predicate here as it is the same as the code
            if (rscRegion != null)
            {
                predicateBuilder = predicateBuilder
                    .And(location => location.RscRegion_Code == rscRegion.Code);
            }

            if (sponsor != null)
            {
                predicateBuilder = predicateBuilder
                    .And(location => location.Sponsor_Code == sponsor.Code
                                     && location.Sponsor_Name == sponsor.Name);
            }

            if (ward != null)
            {
                predicateBuilder = predicateBuilder
                    .And(location => location.Ward_Code == ward.Code
                                     && location.Ward_Name == ward.Name);
            }

            if (planningArea != null)
            {
                predicateBuilder = predicateBuilder
                    .And(location => location.PlanningArea_Code == planningArea.Code
                                     && location.PlanningArea_Name == planningArea.Name);
            }
            
            return context.Location.AsNoTracking().FirstOrDefault(predicateBuilder);
        }
    }
}