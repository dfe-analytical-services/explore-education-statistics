/*
  Deprecated

  Stored procedure for querying Observations. Includes arguments for Location attribute codes.
  This is maintained to serve old Data Blocks that have Location codes rather than id's in their query.

  A newer version of this stored procedure exists for querying by Location id's.
  See SelectObservations.
*/
CREATE OR
ALTER PROCEDURE SelectObservationsByLocationCodes @subjectId uniqueidentifier,
                                                  @filterItemIds IdListGuidType READONLY,
                                                  @timePeriods TimePeriodListType READONLY,
                                                  @geographicLevel nvarchar(6) = NULL,
                                                  @countries IdListVarcharType READONLY,
                                                  @englishDevolvedAreas IdListVarcharType READONLY,
                                                  @institutions IdListVarcharType READONLY,
                                                  @localAuthorities IdListVarcharType READONLY,
                                                  @localAuthorityOldCodes IdListVarcharType READONLY,
                                                  @localAuthorityDistricts IdListVarcharType READONLY,
                                                  @localEnterprisePartnerships IdListVarcharType READONLY,
                                                  @mayoralCombinedAuthorities IdListVarcharType READONLY,
                                                  @multiAcademyTrusts IdListVarcharType READONLY,
                                                  @opportunityAreas IdListVarcharType READONLY,
                                                  @parliamentaryConstituencies IdListVarcharType READONLY,
                                                  @planningAreas IdListVarcharType READONLY,
                                                  @providers IdListVarcharType READONLY,
                                                  @regions IdListVarcharType READONLY,
                                                  @rscRegions IdListVarcharType READONLY,
                                                  @schools IdListVarcharType READONLY,
                                                  @sponsors IdListVarcharType READONLY,
                                                  @wards IdListVarcharType READONLY
AS
DECLARE
    @timePeriodCount INT = (SELECT COUNT(year) FROM @timePeriods),
    @observationalUnitExists BIT = CAST(IIF(
                EXISTS(SELECT TOP 1 1 FROM @countries)
                OR EXISTS(SELECT TOP 1 1 FROM @englishDevolvedAreas)
                OR EXISTS(SELECT TOP 1 1 FROM @institutions)
                OR EXISTS(SELECT TOP 1 1 FROM @localAuthorities)
                OR EXISTS(SELECT TOP 1 1 FROM @localAuthorityOldCodes)
                OR EXISTS(SELECT TOP 1 1 FROM @localAuthorityDistricts)
                OR EXISTS(SELECT TOP 1 1 FROM @localEnterprisePartnerships)
                OR EXISTS(SELECT TOP 1 1 FROM @mayoralCombinedAuthorities)
                OR EXISTS(SELECT TOP 1 1 FROM @multiAcademyTrusts)
                OR EXISTS(SELECT TOP 1 1 FROM @opportunityAreas)
                OR EXISTS(SELECT TOP 1 1 FROM @parliamentaryConstituencies)
                OR EXISTS(SELECT TOP 1 1 FROM @planningAreas)
                OR EXISTS(SELECT TOP 1 1 FROM @providers)
                OR EXISTS(SELECT TOP 1 1 FROM @regions)
                OR EXISTS(SELECT TOP 1 1 FROM @rscRegions)
                OR EXISTS(SELECT TOP 1 1 FROM @schools)
                OR EXISTS(SELECT TOP 1 1 FROM @sponsors)
                OR EXISTS(SELECT TOP 1 1 FROM @wards), 1, 0) AS BIT),
    @filterItemsExist BIT = CAST(IIF(EXISTS(SELECT TOP 1 1 FROM @filterItemIds), 1, 0) AS BIT),
    @uniqueFiltersCount INT = 0,
    @paramDefinition NVARCHAR(2000),
    @ids NVARCHAR(MAX),
    @sql NVARCHAR(MAX)

    IF (@filterItemsExist = 1)
        BEGIN
            CREATE TABLE #FilterItemId(Id uniqueidentifier PRIMARY KEY NOT NULL)
            INSERT INTO #FilterItemId SELECT * FROM @filterItemIds ORDER BY id

            SET @uniqueFiltersCount = (
                SELECT COUNT(DISTINCT filterGroup.FilterId) 
                FROM #FilterItemId filterItemId
                JOIN FilterItem filterItem ON filterItem.Id = filterItemId.Id 
                JOIN FilterGroup filterGroup ON filterGroup.Id = filterItem.FilterGroupId
            )                   
        END

    SET @sql = N'SELECT o.id ' +
                'FROM Observation o ' +
                'JOIN Location l ON o.LocationId = l.Id '
    
    IF (@filterItemsExist = 1)
        SET @sql += N'JOIN ObservationFilterItem ofi ON o.Id = ofi.ObservationId ' +
                     'AND ofi.FilterItemId IN (SELECT id FROM #FilterItemId) '
    SET @sql += N'WHERE o.SubjectId = @subjectId '
    
    IF (@geographicLevel IS NOT NULL)
        SET @sql += N'AND o.GeographicLevel = @geographicLevel '
    IF (@timePeriodCount > 0)
        SET @sql += N'AND EXISTS(SELECT 1 from @timePeriods t WHERE t.year = o.Year AND t.timeIdentifier = o.TimeIdentifier) '
    IF (@observationalUnitExists = 1)
        BEGIN
            SET @sql += N'AND ('
            IF (EXISTS(SELECT TOP 1 1 FROM @countries))
                BEGIN
                    SET @ids = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @countries)), ''''))
                    SET @sql += N'(l.Country_Code IN (' + @ids +
                                ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''NAT'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @englishDevolvedAreas))
                BEGIN
                    SET @ids = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @englishDevolvedAreas)), ''''))
                    SET @sql += N'(l.EnglishDevolvedArea_Code IN (' + @ids +
                                ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''EDA'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @institutions))
                BEGIN
                    SET @ids = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @institutions)), ''''))
                    SET @sql += N'(l.Institution_Code IN (' + @ids +
                                ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''INS'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @localAuthorities))
                BEGIN
                    SET @ids = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @localAuthorities)), ''''))
                    SET @sql += N'(l.LocalAuthority_Code IN (' + @ids +
                                ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''LA'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @localAuthorityOldCodes))
                BEGIN
                    SET @ids = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @localAuthorityOldCodes)), ''''))
                    SET @sql += N'(l.LocalAuthority_OldCode IN (' + @ids +
                                ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''LA'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @localAuthorityDistricts))
                BEGIN
                    SET @ids = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @localAuthorityDistricts)), ''''))
                    SET @sql += N'(l.LocalAuthorityDistrict_Code IN (' + @ids +
                                ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''LAD'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @localEnterprisePartnerships))
                BEGIN
                    SET @ids = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @localEnterprisePartnerships)), ''''))
                    SET @sql += N'(l.LocalEnterprisePartnership_Code IN (' + @ids +
                                ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''LEP'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @mayoralCombinedAuthorities))
                BEGIN
                    SET @ids = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @mayoralCombinedAuthorities)), ''''))
                    SET @sql += N'(l.MayoralCombinedAuthority_Code IN (' + @ids +
                                ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''MCA'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @multiAcademyTrusts))
                BEGIN
                    SET @ids = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @multiAcademyTrusts)), ''''))
                    SET @sql += N'(l.MultiAcademyTrust_Code IN (' + @ids +
                                ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''MAT'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @opportunityAreas))
                BEGIN
                    SET @ids = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @opportunityAreas)), ''''))
                    SET @sql += N'(l.OpportunityArea_Code IN (' + @ids +
                                ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''OA'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @parliamentaryConstituencies))
                BEGIN
                    SET @ids = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @parliamentaryConstituencies)), ''''))
                    SET @sql += N'(l.ParliamentaryConstituency_Code IN (' + @ids +
                                ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''PC'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @planningAreas))
                BEGIN
                    SET @ids = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @planningAreas)), ''''))
                    SET @sql += N'(l.PlanningArea_Code IN (' + @ids +
                                ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''PA'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @providers))
                BEGIN
                    SET @ids = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @providers)), ''''))
                    SET @sql += N'(l.Provider_Code IN (' + @ids +
                                ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''PRO'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @regions))
                BEGIN
                    SET @ids = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @regions)), ''''))
                    SET @sql += N'(l.Region_Code IN (' + @ids +
                                ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''REG'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @rscRegions))
                BEGIN
                    SET @ids = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @rscRegions)), ''''))
                    SET @sql += N'(l.RscRegion_Code IN (' + @ids +
                                ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''RSCR'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @schools))
                BEGIN
                    SET @ids = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @schools)), ''''))
                    SET @sql += N'(l.School_Code IN (' + @ids +
                                ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''SCH'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @sponsors))
                BEGIN
                    SET @ids = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @sponsors)), ''''))
                    SET @sql += N'(l.Sponsor_Code IN (' + @ids +
                                ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''SPO'')) OR '
                END
            IF (EXISTS(SELECT TOP 1 1 FROM @wards))
                BEGIN
                    SET @ids = (SELECT CONCAT(CONCAT('''', (SELECT STRING_AGG(Id, ''',''') FROM @wards)), ''''))
                    SET @sql += N'(l.Ward_Code IN (' + @ids +
                                ') AND (@geographicLevel IS NOT NULL OR o.GeographicLevel = ''WAR'')) OR '
                END
            SET @sql = left(@sql, len(@sql) - 3) + N') '
        END

    IF (@filterItemsExist = 1)
        SET @sql += N'GROUP BY o.Id ' +
                     'HAVING COUNT(DISTINCT ofi.FilterId) = @uniqueFiltersCount '

    SET @sql += N'ORDER BY o.Id;'

    SET @paramDefinition = N'@subjectId uniqueidentifier,
                             @timePeriods TimePeriodListType READONLY,
                             @geographicLevel nvarchar(6) = NULL,
                             @uniqueFiltersCount INT = NULL'
    EXEC sp_executesql
         @sql,
         @paramDefinition,
         @subjectId = @subjectId,
         @timePeriods = @timePeriods,
         @geographicLevel = @geographicLevel,
         @uniqueFiltersCount = @uniqueFiltersCount;