/*
  Stored procedure for querying Observations.

  A deprecated version of this stored procedure exists for querying by Location codes rather than id's.
  See SelectObservationsByLocationCodes.
*/
CREATE OR
ALTER PROCEDURE SelectObservations @subjectId uniqueidentifier,
                                   @filterItemIds IdListGuidType READONLY,
                                   @locationIds IdListGuidType READONLY,
                                   @timePeriods TimePeriodListType READONLY
AS
DECLARE
    @timePeriodCount INT = (SELECT COUNT(year) FROM @timePeriods),
    @paramDefinition NVARCHAR(2000),
    @locationIdsList NVARCHAR(MAX) = (SELECT CONCAT(CONCAT('''', CAST((SELECT STRING_AGG(CAST(Id AS NVARCHAR(MAX)), ''',''') FROM @locationIds) AS NVARCHAR(MAX))), '''')),
    @filterItemsExist BIT = CAST(IIF(EXISTS(SELECT TOP 1 1 FROM @filterItemIds), 1, 0) AS BIT),
    @uniqueFiltersCount INT = 0,
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
                'FROM Observation o '
                
    IF (@filterItemsExist = 1)
        SET @sql += N'JOIN ObservationFilterItem ofi ON o.Id = ofi.ObservationId ' +
                     'AND ofi.FilterItemId IN (SELECT id FROM #FilterItemId) '

    SET @sql += N'WHERE o.SubjectId = @subjectId ' +
                 'AND o.LocationId IN (' + @locationIdsList + ') '
                
    IF (@timePeriodCount > 0)
        SET @sql += N'AND EXISTS(SELECT 1 from @timePeriods t WHERE t.year = o.Year AND t.timeIdentifier = o.TimeIdentifier) '

    IF (@filterItemsExist = 1)
        SET @sql += N'GROUP BY o.Id ' +
                     'HAVING COUNT(DISTINCT ofi.FilterId) = @uniqueFiltersCount '

    SET @sql += N'ORDER BY o.Id;'

    SET @paramDefinition = N'@subjectId uniqueidentifier,
                             @timePeriods TimePeriodListType READONLY,
                             @uniqueFiltersCount INT = NULL'
    EXEC sp_executesql
         @sql,
         @paramDefinition,
         @subjectId = @subjectId,
         @timePeriods = @timePeriods,
         @uniqueFiltersCount = @uniqueFiltersCount;