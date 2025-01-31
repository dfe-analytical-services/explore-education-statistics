CREATE OR ALTER PROCEDURE FilteredObservations
    @subjectId uniqueidentifier,
    @geographicLevel nvarchar(6) = NULL,
    @timePeriodList TimePeriodListType READONLY,
    @countriesList IdListVarcharType READONLY,
    @englishDevolvedAreasList IdListVarcharType READONLY,
    @institutionsList IdListVarcharType READONLY,
    @localAuthorityList IdListVarcharType READONLY,
    @localAuthorityOldCodeList IdListVarcharType READONLY,
    @localAuthorityDistrictList IdListVarcharType READONLY,
    @localEnterprisePartnershipsList IdListVarcharType READONLY,
    @mayoralCombinedAuthoritiesList IdListVarcharType READONLY,
    @multiAcademyTrustList IdListVarcharType READONLY,
    @opportunityAreasList IdListVarcharType READONLY,
    @parliamentaryConstituenciesList IdListVarcharType READONLY,
    @providersList IdListVarcharType READONLY,
    @regionsList IdListVarcharType READONLY,
    @rscRegionsList IdListVarcharType READONLY,
    @schoolsList IdListVarcharType READONLY,
    @sponsorList IdListVarcharType READONLY,
    @wardsList IdListVarcharType READONLY,
    @planningAreasList IdListVarcharType READONLY,
    @filterItemList IdListGuidType READONLY
AS
DECLARE
    @timePeriodCount INT = (SELECT count(year) FROM @timePeriodList),
    @observationalUnitExists BIT = CAST(IIF(
                EXISTS(SELECT TOP 1 1 FROM @countriesList)
                OR EXISTS(SELECT TOP 1 1 FROM @englishDevolvedAreasList)
                OR EXISTS(SELECT TOP 1 1 FROM @institutionsList)
                OR EXISTS(SELECT TOP 1 1 FROM @localAuthorityList)
                OR EXISTS(SELECT TOP 1 1 FROM @localAuthorityOldCodeList)
                OR EXISTS(SELECT TOP 1 1 FROM @localAuthorityDistrictList)
                OR EXISTS(SELECT TOP 1 1 FROM @localEnterprisePartnershipsList)
                OR EXISTS(SELECT TOP 1 1 FROM @mayoralCombinedAuthoritiesList)
                OR EXISTS(SELECT TOP 1 1 FROM @multiAcademyTrustList)
                OR EXISTS(SELECT TOP 1 1 FROM @opportunityAreasList)
                OR EXISTS(SELECT TOP 1 1 FROM @parliamentaryConstituenciesList)
                OR EXISTS(SELECT TOP 1 1 FROM @providersList)
                OR EXISTS(SELECT TOP 1 1 FROM @regionsList)
                OR EXISTS(SELECT TOP 1 1 FROM @rscRegionsList)
                OR EXISTS(SELECT TOP 1 1 FROM @schoolsList)
                OR EXISTS(SELECT TOP 1 1 FROM @sponsorList)
                OR EXISTS(SELECT TOP 1 1 FROM @wardsList)
                OR EXISTS(SELECT TOP 1 1 FROM @planningAreasList), 1, 0) AS BIT),
    @paramDefinition NVARCHAR(2000),
    @idsList NVARCHAR(MAX),
    @sqlString NVARCHAR(MAX) = N'SELECT o.id ' +
                                'FROM Observation o ' +
                                'JOIN Location l ON o.LocationId = l.Id ' +
                                'LEFT JOIN ObservationFilterItem ofi ON o.Id = ofi.ObservationId ' +
                                'AND ofi.FilterItemId IN (SELECT id FROM @filterItemList) ' +
                                'WHERE o.SubjectId = @subjectId '
    IF (@geographicLevel IS NOT NULL)
        SET @sqlString = @sqlString + N'AND o.GeographicLevel = @geographicLevel '
    IF (@timePeriodCount > 0)
        SET @sqlString = @sqlString + N'AND EXISTS(SELECT 1 from @timePeriodList t WHERE t.year = o.Year AND t.timeIdentifier = o.TimeIdentifier) '
    IF (@observationalUnitExists = 1)
        BEGIN
            SET @sqlString = @sqlString + N'AND ('
            IF (EXISTS(SELECT TOP 1 1 FROM @countriesList))
                BEGIN
                    SET @idsList = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @countriesList)), ''''))
                    SET @sqlString = @sqlString + N'(l.Country_Code IN (' + @idsList + ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''NAT'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @englishDevolvedAreasList))
                BEGIN
                    SET @idsList = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @englishDevolvedAreasList)), ''''))
                    SET @sqlString = @sqlString + N'(l.EnglishDevolvedArea_Code IN (' + @idsList + ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''EDA'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @institutionsList))
                BEGIN
                    SET @idsList = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @institutionsList)), ''''))
                    SET @sqlString = @sqlString + N'(l.Institution_Code IN (' + @idsList + ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''INS'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @localAuthorityList))
                BEGIN
                    SET @idsList = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @localAuthorityList)), ''''))
                    SET @sqlString = @sqlString + N'(l.LocalAuthority_Code IN (' + @idsList + ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''LA'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @localAuthorityOldCodeList))
                BEGIN
                    SET @idsList = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @localAuthorityOldCodeList)), ''''))
                    SET @sqlString = @sqlString + N'(l.LocalAuthority_OldCode IN (' + @idsList + ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''LA'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @localAuthorityDistrictList))
                BEGIN
                    SET @idsList = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @localAuthorityDistrictList)), ''''))
                    SET @sqlString = @sqlString + N'(l.LocalAuthorityDistrict_Code IN (' + @idsList + ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''LAD'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @localEnterprisePartnershipsList))
                BEGIN
                    SET @idsList = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @localEnterprisePartnershipsList)), ''''))
                    SET @sqlString = @sqlString + N'(l.LocalEnterprisePartnership_Code IN (' + @idsList + ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''LEP'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @mayoralCombinedAuthoritiesList))
                BEGIN
                    SET @idsList = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @mayoralCombinedAuthoritiesList)), ''''))
                    SET @sqlString = @sqlString + N'(l.MayoralCombinedAuthority_Code IN (' + @idsList + ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''MCA'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @multiAcademyTrustList))
                BEGIN
                    SET @idsList = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @multiAcademyTrustList)), ''''))
                    SET @sqlString = @sqlString + N'(l.MultiAcademyTrust_Code IN (' + @idsList + ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''MAT'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @opportunityAreasList))
                BEGIN
                    SET @idsList = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @opportunityAreasList)), ''''))
                    SET @sqlString = @sqlString + N'(l.OpportunityArea_Code IN (' + @idsList + ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''OA'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @parliamentaryConstituenciesList))
                BEGIN
                    SET @idsList = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @parliamentaryConstituenciesList)), ''''))
                    SET @sqlString = @sqlString + N'(l.ParliamentaryConstituency_Code IN (' + @idsList + ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''PC'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @providersList))
                BEGIN
                    SET @idsList = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @providersList)), ''''))
                    SET @sqlString = @sqlString + N'(l.Provider_Code IN (' + @idsList + ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''PRO'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @regionsList))
                BEGIN
                    SET @idsList = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @regionsList)), ''''))
                    SET @sqlString = @sqlString + N'(l.Region_Code IN (' + @idsList + ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''REG'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @rscRegionsList))
                BEGIN
                    SET @idsList = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @rscRegionsList)), ''''))
                    SET @sqlString = @sqlString + N'(l.RscRegion_Code IN (' + @idsList + ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''RSCR'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @schoolsList))
                BEGIN
                    SET @idsList = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @schoolsList)), ''''))
                    SET @sqlString = @sqlString + N'(l.School_Code IN (' + @idsList + ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''SCH'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @sponsorList))
                BEGIN
                    SET @idsList = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @sponsorList)), ''''))
                    SET @sqlString = @sqlString + N'(l.Sponsor_Code IN (' + @idsList + ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''SPO'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @wardsList))
                BEGIN
                    SET @idsList = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @wardsList)), ''''))
                    SET @sqlString = @sqlString + N'(l.Ward_Code IN (' + @idsList + ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''WAR'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @planningAreasList))
                BEGIN
                    SET @idsList = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @planningAreasList)), ''''))
                    SET @sqlString = @sqlString + N'(l.PlanningArea_Code IN (' + @idsList + ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''PA'')) OR '
                END
            SET @sqlString = left(@sqlString, len(@sqlString) - 3) + N') '
        END
    SET @sqlString = @sqlString + N'GROUP BY o.Id ' +
                     'HAVING COUNT(DISTINCT ofi.FilterItemId) = (' +
                     '    SELECT COUNT(DISTINCT f.id) AS filterCount' +
                     '    FROM' +
                     '    FilterItem fi' +
                     '    JOIN FilterGroup fg ON fi.FilterGroupId = fg.Id' +
                     '    JOIN Filter f ON fg.FilterId = f.Id' +
                     '    WHERE fi.Id IN (SELECT id FROM @filterItemList)' +
                     ') ' +
                     'ORDER BY o.Id;'
    SET @paramDefinition = N'@subjectId uniqueidentifier,
                           @geographicLevel nvarchar(6) = NULL,
                           @timePeriodList TimePeriodListType READONLY,
                           @countriesList IdListVarcharType READONLY,
                           @englishDevolvedAreasList IdListVarcharType READONLY,
                           @institutionsList IdListVarcharType READONLY,
                           @localAuthorityList IdListVarcharType READONLY,
                           @localAuthorityOldCodeList IdListVarcharType READONLY,
                           @localAuthorityDistrictList IdListVarcharType READONLY,
                           @localEnterprisePartnershipsList IdListVarcharType READONLY,
                           @mayoralCombinedAuthoritiesList IdListVarcharType READONLY,
                           @multiAcademyTrustList IdListVarcharType READONLY,
                           @opportunityAreasList IdListVarcharType READONLY,
                           @parliamentaryConstituenciesList IdListVarcharType READONLY,
                           @providersList IdListVarcharType READONLY,
                           @regionsList IdListVarcharType READONLY,
                           @rscRegionsList IdListVarcharType READONLY,
                           @schoolsList IdListVarcharType READONLY,
                           @sponsorList IdListVarcharType READONLY,
                           @wardsList IdListVarcharType READONLY,
                           @planningAreasList IdListVarcharType READONLY,
                           @filterItemList IdListGuidType READONLY'
    EXEC sp_executesql @sqlString, @paramDefinition,
         @subjectId = @subjectId,
         @geographicLevel = @geographicLevel,
         @timePeriodList = @timePeriodList,
         @countriesList = @countriesList,
         @englishDevolvedAreasList = @englishDevolvedAreasList,
         @institutionsList = @institutionsList,
         @localAuthorityList = @localAuthorityList,
         @localAuthorityOldCodeList = @localAuthorityOldCodeList,
         @localAuthorityDistrictList = @localAuthorityDistrictList,
         @localEnterprisePartnershipsList = @localEnterprisePartnershipsList,
         @mayoralCombinedAuthoritiesList = @mayoralCombinedAuthoritiesList,
         @multiAcademyTrustList = @multiAcademyTrustList,
         @opportunityAreasList = @opportunityAreasList,
         @parliamentaryConstituenciesList = @parliamentaryConstituenciesList,
         @providersList = @providersList,
         @regionsList = @regionsList,
         @rscRegionsList = @rscRegionsList,
         @schoolsList = @schoolsList,
         @sponsorList = @sponsorList,
         @wardsList = @wardsList,
         @planningAreasList = @planningAreasList,
         @filterItemList = @filterItemList;
