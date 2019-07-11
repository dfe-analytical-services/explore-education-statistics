ALTER PROCEDURE FilteredObservations
    @subjectId int,
    @geographicLevel nvarchar(6) = NULL,
    @yearList IdListIntegerType READONLY,
    @countriesList IdListVarcharType READONLY,
    @institutionsList IdListVarcharType READONLY,
    @localAuthorityList IdListVarcharType READONLY,
    @localAuthorityDistrictList IdListVarcharType READONLY,
    @localEnterprisePartnershipsList IdListVarcharType READONLY,
    @mayoralCombinedAuthoritiesList IdListVarcharType READONLY,
    @multiAcademyTrustList IdListVarcharType READONLY,
    @opportunityAreasList IdListVarcharType READONLY,
    @parliamentaryConstituenciesList IdListVarcharType READONLY,
    @regionsList IdListVarcharType READONLY,
    @rscRegionsList IdListVarcharType READONLY,
    @sponsorList IdListVarcharType READONLY,
    @wardsList IdListVarcharType READONLY,    
    @filterList IdListIntegerType READONLY
AS
DECLARE
    @yearsCount int = (SELECT count(id) FROM @yearList),
    @countriesCount int = (SELECT count(id) FROM @countriesList),
    @institutionCount int = (SELECT count(id) FROM @institutionsList),
    @localAuthoritiesCount int = (SELECT count(id) FROM @localAuthorityList),
    @localAuthorityDistrictCount int = (SELECT count(id) FROM @localAuthorityDistrictList),
    @localEnterprisePartnershipCount int = (SELECT count(id) FROM @localEnterprisePartnershipsList),
    @mayoralCombinedAuthorityCount int = (SELECT count(id) FROM @mayoralCombinedAuthoritiesList),
    @multiAcademyTrustCount int = (SELECT count(id) FROM @multiAcademyTrustList),
    @opportunityAreaCount int = (SELECT count(id) FROM @opportunityAreasList),
    @parliamentaryConstituencyCount int = (SELECT count(id) FROM @parliamentaryConstituenciesList),
    @regionsCount int = (SELECT count(id) FROM @regionsList),
    @rscRegionsCount int = (SELECT count(id) FROM @rscRegionsList),
    @sponsorCount int = (SELECT count(id) FROM @sponsorList),
    @wardCount int = (SELECT count(id) FROM @wardsList)
SELECT o.id
FROM Observation o
JOIN ObservationFilterItem ofi ON o.Id = ofi.ObservationId
JOIN Location l on o.LocationId = l.Id
WHERE o.SubjectId = @subjectId
AND o.GeographicLevel = ISNULL(@geographicLevel, o.GeographicLevel)
AND ofi.FilterItemId IN (SELECT id FROM @filterList)
AND (@yearsCount = 0 OR o.Year IN (SELECT id from @yearList))
AND (@countriesCount = 0 OR l.Country_Code IN (SELECT id from @countriesList))
AND (@institutionCount = 0 OR l.Institution_Code IN (SELECT id from @institutionsList))
AND (@localAuthoritiesCount = 0 OR l.LocalAuthority_Code IN (SELECT id from @localAuthorityList))
AND (@localAuthorityDistrictCount = 0 OR l.LocalAuthorityDistrict_Code IN (SELECT id from @localAuthorityDistrictList))
AND (@localEnterprisePartnershipCount = 0 OR l.LocalEnterprisePartnership_Code IN (SELECT id from @localEnterprisePartnershipsList))
AND (@mayoralCombinedAuthorityCount = 0 OR l.MayoralCombinedAuthority_Code IN (SELECT id from @mayoralCombinedAuthoritiesList))
AND (@multiAcademyTrustCount = 0 OR l.MultiAcademyTrust_Code IN (SELECT id from @multiAcademyTrustList))
AND (@opportunityAreaCount = 0 OR l.OpportunityArea_Code IN (SELECT id from @opportunityAreasList))
AND (@parliamentaryConstituencyCount = 0 OR l.ParliamentaryConstituency_Code IN (SELECT id from @parliamentaryConstituenciesList))
AND (@regionsCount = 0 OR l.Region_Code IN (SELECT id from @regionsList))
AND (@rscRegionsCount = 0 OR l.RscRegion_Code IN (SELECT id from @rscRegionsList))
AND (@sponsorCount = 0 OR l.Sponsor_Code IN (SELECT id from @sponsorList))
AND (@wardCount = 0 OR l.Ward_Code IN (SELECT id from @wardsList))
GROUP BY o.Id
HAVING COUNT(DISTINCT ofi.FilterItemId) = (SELECT COUNT(DISTINCT f.id) AS filterCount
FROM
     FilterItem fi
    JOIN FilterGroup fg ON fi.FilterGroupId = fg.Id
    JOIN Filter f ON fg.FilterId = f.Id
WHERE fi.Id IN (SELECT id FROM @filterList))
ORDER BY o.Id;