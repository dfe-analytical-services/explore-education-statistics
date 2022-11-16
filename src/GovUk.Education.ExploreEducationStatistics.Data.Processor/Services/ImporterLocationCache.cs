#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;

public class ImporterLocationCache : IImporterLocationCache
{
    private readonly Dictionary<string, Location> _locations = new();

    public void LoadLocations(StatisticsDbContext context)
    {
        var existingLocations = context
            .Location
            .AsNoTracking()
            .ToList();
        
        existingLocations.ForEach(location => _locations.Add(GetLocationCacheKey(location), location));
    }

    public Location Get(Location locationFromCsv)
    {
        return _locations[GetLocationCacheKey(locationFromCsv)];
    }
    
    public async Task<Location> GetOrCreateAndCache(Location locationFromCsv, Func<Task<Location>> locationProvider)
    {
        var locationCacheKey = GetLocationCacheKey(locationFromCsv);

        if (_locations.ContainsKey(locationCacheKey))
        {
            return _locations[locationCacheKey];
        }

        var providedLocation = await locationProvider.Invoke();
        _locations.Add(locationCacheKey, providedLocation);
        return providedLocation;
    }

    private static string GetLocationCacheKey(
        GeographicLevel geographicLevel,
        Country? country,
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
        var locationAttributes = new LocationAttribute?[]
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

    private static string GetLocationCacheKey(Location location)
    {
        return GetLocationCacheKey(
            location.GeographicLevel,
            location.Country,
            location.EnglishDevolvedArea,
            location.Institution,
            location.LocalAuthority,
            location.LocalAuthorityDistrict,
            location.LocalEnterprisePartnership,
            location.MayoralCombinedAuthority,
            location.MultiAcademyTrust,
            location.OpportunityArea,
            location.ParliamentaryConstituency,
            location.PlanningArea,
            location.Provider,
            location.Region,
            location.RscRegion,
            location.School,
            location.Sponsor,
            location.Ward);
    }
}