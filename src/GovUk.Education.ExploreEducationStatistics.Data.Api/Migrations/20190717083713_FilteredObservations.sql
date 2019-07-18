ALTER PROCEDURE FilteredObservations
    @subjectId INT,
    @geographicLevel nvarchar(6) = NULL,
    @timePeriodList TimePeriodListType READONLY,
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
    @timePeriodCount INT = (SELECT count(year) FROM @timePeriodList),
    @observationalUnitExists BIT = CAST(IIF(
        EXISTS(SELECT TOP 1 1 FROM @countriesList)
        OR EXISTS(SELECT TOP 1 1 FROM @institutionsList)
        OR EXISTS(SELECT TOP 1 1 FROM @localAuthorityList)
        OR EXISTS(SELECT TOP 1 1 FROM @localAuthorityDistrictList)
        OR EXISTS(SELECT TOP 1 1 FROM @localEnterprisePartnershipsList)
        OR EXISTS(SELECT TOP 1 1 FROM @mayoralCombinedAuthoritiesList)
        OR EXISTS(SELECT TOP 1 1 FROM @multiAcademyTrustList)
        OR EXISTS(SELECT TOP 1 1 FROM @opportunityAreasList)
        OR EXISTS(SELECT TOP 1 1 FROM @parliamentaryConstituenciesList)
        OR EXISTS(SELECT TOP 1 1 FROM @regionsList)
        OR EXISTS(SELECT TOP 1 1 FROM @rscRegionsList)
        OR EXISTS(SELECT TOP 1 1 FROM @sponsorList)
        OR EXISTS(SELECT TOP 1 1 FROM @wardsList), 1, 0) AS BIT) 
SELECT o.id
FROM Observation o
JOIN ObservationFilterItem ofi ON o.Id = ofi.ObservationId
JOIN Location l ON o.LocationId = l.Id
WHERE o.SubjectId = @subjectId
AND o.GeographicLevel = ISNULL(@geographicLevel, o.GeographicLevel)
AND ofi.FilterItemId IN (SELECT id FROM @filterList)
AND (@timePeriodCount = 0 OR
     EXISTS(SELECT 1 from @timePeriodList t WHERE t.year = o.Year AND t.timeIdentifier = o.TimeIdentifier))
AND (
    @observationalUnitExists = 0
    OR (l.Country_Code IN (SELECT id from @countriesList) AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = 'NAT'))
    OR (l.Institution_Code IN (SELECT id from @institutionsList) AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = 'INS'))
    OR (l.LocalAuthority_Code IN (SELECT id from @localAuthorityList) AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = 'LA'))
    OR (l.LocalAuthorityDistrict_Code IN (SELECT id from @localAuthorityDistrictList) AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = 'LAD'))
    OR (l.LocalEnterprisePartnership_Code IN (SELECT id from @localEnterprisePartnershipsList) AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = 'LEP'))
    OR (l.MayoralCombinedAuthority_Code IN (SELECT id from @mayoralCombinedAuthoritiesList) AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = 'MCA'))
    OR (l.MultiAcademyTrust_Code IN (SELECT id from @multiAcademyTrustList) AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = 'MAT'))
    OR (l.OpportunityArea_Code IN (SELECT id from @opportunityAreasList) AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = 'OA'))
    OR (l.ParliamentaryConstituency_Code IN (SELECT id from @parliamentaryConstituenciesList) AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = 'PC'))
    OR (l.Region_Code IN (SELECT id from @regionsList) AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = 'REG'))
    OR (l.RscRegion_Code IN (SELECT id from @rscRegionsList) AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = 'RSCR'))
    OR (l.Sponsor_Code IN (SELECT id from @sponsorList) AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = 'SPO'))
    OR (l.Ward_Code IN (SELECT id from @wardsList) AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = 'WAR'))
)
GROUP BY o.Id
HAVING COUNT(DISTINCT ofi.FilterItemId) = (SELECT COUNT(DISTINCT f.id) AS filterCount
FROM
     FilterItem fi
    JOIN FilterGroup fg ON fi.FilterGroupId = fg.Id
    JOIN Filter f ON fg.FilterId = f.Id
WHERE fi.Id IN (SELECT id FROM @filterList))
ORDER BY o.Id;