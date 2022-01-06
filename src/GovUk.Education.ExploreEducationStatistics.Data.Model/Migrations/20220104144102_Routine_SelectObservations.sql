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
    @sql NVARCHAR(MAX)

    CREATE TABLE #FilterItemId(Id uniqueidentifier)
    INSERT INTO #FilterItemId SELECT * FROM @filterItemIds

    SET @sql = N'SELECT o.id ' +
                'FROM Observation o '
                
    IF (EXISTS(SELECT * FROM #FilterItemId))
        SET @sql += N'JOIN ObservationFilterItem ofi ON o.Id = ofi.ObservationId ' +
                     'JOIN #FilterItemId filterItemId ON ofi.FilterItemId = filterItemId.id '

    SET @sql += N'WHERE o.SubjectId = @subjectId ' +
                 'AND o.LocationId IN (' + @locationIdsList + ') '
                
    IF (@timePeriodCount > 0)
        SET @sql += N'AND EXISTS(SELECT 1 from @timePeriods t WHERE t.year = o.Year AND t.timeIdentifier = o.TimeIdentifier) '

    IF (EXISTS(SELECT * FROM #FilterItemId))
        SET @sql += N'GROUP BY o.Id ' +
                     'HAVING COUNT(DISTINCT ofi.FilterId) = ' +
                     '(SELECT COUNT(f.Id) FROM Filter f WHERE f.SubjectId = @subjectId)'

    SET @sql += N'ORDER BY o.Id;'

    SET @paramDefinition = N'@subjectId uniqueidentifier,
                           @filterItemIds IdListGuidType READONLY,
                           @locationIds IdListGuidType READONLY,
                           @timePeriods TimePeriodListType READONLY'
    EXEC sp_executesql
         @sql,
         @paramDefinition,
         @subjectId = @subjectId,
         @filterItemIds = @filterItemIds,
         @locationIds = @locationIds,
         @timePeriods = @timePeriods;