CREATE OR ALTER PROCEDURE GetFilteredObservations
    @geographicLevel nvarchar(6) = NULL,
    @timePeriodList TimePeriodListType READONLY,
    @countriesList IdListVarcharType READONLY,
    @institutionsList IdListVarcharType READONLY,
    @localAuthorityList IdListVarcharType READONLY,
    @localAuthorityOldCodeList IdListVarcharType READONLY,
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
    @planningAreasList IdListVarcharType READONLY,
    @filterItemList IdListGuidType READONLY
AS
DECLARE @filterListTemp AS TABLE (RowID INT NOT NULL PRIMARY KEY IDENTITY(1,1), FilterId uniqueidentifier);

INSERT INTO @filterListTemp
SELECT DISTINCT F.Id
FROM Filter F
         JOIN FilterGroup FG ON F.Id = FG.FilterId
         JOIN FilterItem FI ON FG.Id = FI.FilterGroupId
WHERE FI.Id IN (SELECT ID FROM @filterItemList);

DECLARE @numFilters int = @@ROWCOUNT,
@filterList FilterTableType,
@COUNT int = 1;

INSERT INTO @filterList SELECT * FROM @filterListTemp;

DECLARE @sqlString NVARCHAR(4000) = N'WITH ReducedRows AS (SELECT a.ObservationId, a.FilterItemId FROM ' +
                                    N'(SELECT ObservationId, FilterItemId FROM dbo.ObservationFilterItem a WHERE a.FilterId = ' +
                                    N'(SELECT FilterId FROM @filterList WHERE RowID = 1) AND a.FilterItemId IN ' +
                                    N'(SELECT id from @filterItemList) ';

    -- Use Row Reduction
    WHILE (@COUNT < @numFilters)
        BEGIN
            SET @COUNT = @COUNT + 1;
            SET @sqlString = @sqlString +
                             N' AND EXISTS (SELECT 1 FROM dbo.ObservationFilterItem b ' +
                             N'WHERE a.ObservationId = b.ObservationId AND b.FilterItemId IN' +
                             N' (select id from @filterItemList) AND b.FilterId = (SELECT FilterId FROM @filterList WHERE RowID = ' +
                             CAST(@COUNT AS VARCHAR) +
                             N'))';
        END

    SET @sqlString = @sqlString +
                     N') a JOIN dbo.ObservationFilterItem b ON a.ObservationId = b.ObservationId AND a.FilterItemId = b.FilterItemId) '

DECLARE
    @timePeriodCount INT = (SELECT count(year) FROM @timePeriodList),
    @observationalUnitExists BIT = CAST(IIF(
                EXISTS(SELECT TOP 1 1 FROM @countriesList)
                OR EXISTS(SELECT TOP 1 1 FROM @institutionsList)
                OR EXISTS(SELECT TOP 1 1 FROM @localAuthorityList)
                OR EXISTS(SELECT TOP 1 1 FROM @localAuthorityOldCodeList)
                OR EXISTS(SELECT TOP 1 1 FROM @localAuthorityDistrictList)
                OR EXISTS(SELECT TOP 1 1 FROM @localEnterprisePartnershipsList)
                OR EXISTS(SELECT TOP 1 1 FROM @mayoralCombinedAuthoritiesList)
                OR EXISTS(SELECT TOP 1 1 FROM @multiAcademyTrustList)
                OR EXISTS(SELECT TOP 1 1 FROM @opportunityAreasList)
                OR EXISTS(SELECT TOP 1 1 FROM @parliamentaryConstituenciesList)
                OR EXISTS(SELECT TOP 1 1 FROM @regionsList)
                OR EXISTS(SELECT TOP 1 1 FROM @rscRegionsList)
                OR EXISTS(SELECT TOP 1 1 FROM @sponsorList)
                OR EXISTS(SELECT TOP 1 1 FROM @wardsList)
                OR EXISTS(SELECT TOP 1 1 FROM @planningAreasList), 1, 0) AS BIT),
    @paramDefinition NVARCHAR(4000);

    SET @sqlString = @sqlString + N'SELECT DISTINCT o.Id from Observation o ' +
                     N'JOIN Location l ON o.LocationId = l.Id ' +
                     N'WHERE EXISTS (SELECT 1 FROM ReducedRows WHERE ObservationId = o.Id) '

    IF (@geographicLevel IS NOT NULL)
        SET @sqlString = @sqlString + N'AND o.GeographicLevel = @geographicLevel '
    IF (@timePeriodCount > 0)
        SET @sqlString = @sqlString + N'AND EXISTS(SELECT 1 from @timePeriodList t WHERE t.year = o.Year AND t.timeIdentifier = o.TimeIdentifier) '
    IF (@observationalUnitExists = 1)
        BEGIN
            SET @sqlString = @sqlString + N'AND ('
            IF (EXISTS(SELECT TOP 1 1 FROM @countriesList))
                SET @sqlString = @sqlString + N'(l.Country_Code IN (SELECT id from @countriesList) AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''NAT'')) OR '
            IF (EXISTS(SELECT TOP 1 1 FROM @institutionsList))
                SET @sqlString = @sqlString + N'(l.Institution_Code IN (SELECT id from @institutionsList) AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''INS'')) OR '
            IF (EXISTS(SELECT TOP 1 1 FROM @localAuthorityList))
                SET @sqlString = @sqlString + N'(l.LocalAuthority_Code IN (SELECT id from @localAuthorityList) AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''LA'')) OR '
            IF (EXISTS(SELECT TOP 1 1 FROM @localAuthorityOldCodeList))
                SET @sqlString = @sqlString + N'(l.LocalAuthority_OldCode IN (SELECT id from @localAuthorityOldCodeList) AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''LA'')) OR '
            IF (EXISTS(SELECT TOP 1 1 FROM @localAuthorityDistrictList))
                SET @sqlString = @sqlString + N'(l.LocalAuthorityDistrict_Code IN (SELECT id from @localAuthorityDistrictList) AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''LAD'')) OR '
            IF (EXISTS(SELECT TOP 1 1 FROM @localEnterprisePartnershipsList))
                SET @sqlString = @sqlString + N'(l.LocalEnterprisePartnership_Code IN (SELECT id from @localEnterprisePartnershipsList) AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''LEP'')) OR '
            IF (EXISTS(SELECT TOP 1 1 FROM @mayoralCombinedAuthoritiesList))
                SET @sqlString = @sqlString + N'(l.MayoralCombinedAuthority_Code IN (SELECT id from @mayoralCombinedAuthoritiesList) AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''MCA'')) OR '
            IF (EXISTS(SELECT TOP 1 1 FROM @multiAcademyTrustList))
                SET @sqlString = @sqlString + N'(l.MultiAcademyTrust_Code IN (SELECT id from @multiAcademyTrustList) AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''MAT'')) OR '
            IF (EXISTS(SELECT TOP 1 1 FROM @opportunityAreasList))
                SET @sqlString = @sqlString + N'(l.OpportunityArea_Code IN (SELECT id from @opportunityAreasList) AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''OA'')) OR '
            IF (EXISTS(SELECT TOP 1 1 FROM @parliamentaryConstituenciesList))
                SET @sqlString = @sqlString + N'(l.ParliamentaryConstituency_Code IN (SELECT id from @parliamentaryConstituenciesList) AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''PC'')) OR '
            IF (EXISTS(SELECT TOP 1 1 FROM @regionsList))
                SET @sqlString = @sqlString + N'(l.Region_Code IN (SELECT id from @regionsList) AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''REG'')) OR '
            IF (EXISTS(SELECT TOP 1 1 FROM @rscRegionsList))
                SET @sqlString = @sqlString + N'(l.RscRegion_Code IN (SELECT id from @rscRegionsList) AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''RSCR'')) OR '
            IF (EXISTS(SELECT TOP 1 1 FROM @sponsorList))
                SET @sqlString = @sqlString + N'(l.Sponsor_Code IN (SELECT id from @sponsorList) AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''SPO'')) OR '
            IF (EXISTS(SELECT TOP 1 1 FROM @wardsList))
                SET @sqlString = @sqlString + N'(l.Ward_Code IN (SELECT id from @wardsList) AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''WAR'')) OR '
            IF (EXISTS(SELECT TOP 1 1 FROM @planningAreasList))
                SET @sqlString = @sqlString + N'(l.PlanningArea_Code IN (SELECT id from @planningAreasList) AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''PA'')) OR '
            SET @sqlString = left(@sqlString, len(@sqlString) - 3) + N') '
        END

    SET @paramDefinition = N'@geographicLevel nvarchar(6) = NULL,
                       @timePeriodList TimePeriodListType READONLY,
                       @countriesList IdListVarcharType READONLY,
                       @institutionsList IdListVarcharType READONLY,
                       @localAuthorityList IdListVarcharType READONLY,
                       @localAuthorityOldCodeList IdListVarcharType READONLY,
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
                       @planningAreasList IdListVarcharType READONLY,
                       @filterItemList IdListGuidType READONLY,
                       @filterList FilterTableType READONLY'

    EXEC sp_executesql @sqlString, @paramDefinition,
         @geographicLevel = @geographicLevel,
         @timePeriodList = @timePeriodList,
         @countriesList = @countriesList,
         @institutionsList = @institutionsList,
         @localAuthorityList = @localAuthorityList,
         @localAuthorityOldCodeList = @localAuthorityOldCodeList,
         @localAuthorityDistrictList = @localAuthorityDistrictList,
         @localEnterprisePartnershipsList = @localEnterprisePartnershipsList,
         @mayoralCombinedAuthoritiesList = @mayoralCombinedAuthoritiesList,
         @multiAcademyTrustList = @multiAcademyTrustList,
         @opportunityAreasList = @opportunityAreasList,
         @parliamentaryConstituenciesList = @parliamentaryConstituenciesList,
         @regionsList = @regionsList,
         @rscRegionsList = @rscRegionsList,
         @sponsorList = @sponsorList,
         @wardsList = @wardsList,
         @planningAreasList = @planningAreasList,
         @filterItemList = @filterItemList,
         @filterList = @filterList;
