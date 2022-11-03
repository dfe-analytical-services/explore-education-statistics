#nullable enable
using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Microsoft.Extensions.Caching.Memory;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;

public class ImporterMemoryCache
{
    private MemoryCache Cache { get; set; } = new(new MemoryCacheOptions
    {
        SizeLimit = 50000,
        ExpirationScanFrequency = TimeSpan.FromMinutes(5)
    });
    
    private readonly MemoryCacheEntryOptions _cacheEntryOptions = new()
    {
        Size = 1,
        SlidingExpiration = TimeSpan.FromMinutes(1)
    };

    public TItem Set<TItem>(object cacheKey, TItem cacheItem)
    {
        return Cache.Set(cacheKey, cacheItem, _cacheEntryOptions);
    }
    
    public TItem GetOrCreate<TItem>(object cacheKey, Func<TItem> defaultItemProvider)
    {
        return Cache.GetOrCreate(cacheKey, entry =>
        {
            var defaultItem = defaultItemProvider.Invoke();
            entry.SetOptions(_cacheEntryOptions);
            entry.Value = defaultItem;
            return defaultItem;
        });
    }
    
    public TItem Get<TItem>(object cacheKey)
    {
        return Cache.Get<TItem>(cacheKey);
    }

    public static string GetLocationCacheKey(
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

    public static string GetLocationCacheKey(Location location)
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