CREATE PROCEDURE FilteredObservations
    @subjectId int,
    @geographicLevel varchar(max),
    @yearList IdListIntegerType READONLY,
    @countriesList IdListVarcharType READONLY,
    @regionsList IdListVarcharType READONLY,
    @localAuthorityList IdListVarcharType READONLY,
    @localAuthorityDistrictList IdListVarcharType READONLY,
    @filterList IdListIntegerType READONLY
AS
DECLARE
    @yearsCount int = (SELECT count(id) FROM @yearList),
    @countriesCount int = (SELECT count(id) FROM @countriesList),
    @regionsCount int = (SELECT count(id) FROM @regionsList),
    @localAuthoritiesCount int = (SELECT count(id) FROM @localAuthorityList),
    @localAuthorityDistrictCount int = (SELECT count(id) FROM @localAuthorityDistrictList)
SELECT o.id
FROM Observation o
JOIN ObservationFilterItem ofi ON o.Id = ofi.ObservationId
JOIN Location l on o.LocationId = l.Id
WHERE o.SubjectId = @subjectId
AND o.GeographicLevel = @geographicLevel
AND ofi.FilterItemId IN (SELECT id FROM @filterList)
AND (@yearsCount = 0 OR o.Year IN (SELECT id from @yearList))
AND (@countriesCount = 0 OR l.Country_Code IN (SELECT id from @countriesList))
AND (@regionsCount = 0 OR l.Region_Code IN (SELECT id from @regionsList))
AND (@localAuthoritiesCount = 0 OR l.LocalAuthority_Code IN (SELECT id from @localAuthorityList))
AND (@localAuthorityDistrictCount = 0 OR l.LocalAuthorityDistrict_Code IN (SELECT id from @localAuthorityDistrictList))
GROUP BY o.Id
HAVING COUNT(DISTINCT ofi.FilterItemId) = (SELECT COUNT(DISTINCT f.id) AS filterCount
FROM
     FilterItem fi
    JOIN FilterGroup fg ON fi.FilterGroupId = fg.Id
    JOIN Filter f ON fg.FilterId = f.Id
WHERE fi.Id IN (SELECT id FROM @filterList))
ORDER BY o.Id;