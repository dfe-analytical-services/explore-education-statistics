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
    @sql NVARCHAR(MAX) = N'SELECT o.id ' +
                         'FROM Observation o ' +
                         'LEFT JOIN ObservationFilterItem ofi ON o.Id = ofi.ObservationId ' +
                         'AND ofi.FilterItemId IN (SELECT id FROM @filterItemIds) ' +
                         'WHERE o.SubjectId = @subjectId ' +
                         'AND o.LocationId IN (SELECT id FROM @locationIds) '
    IF (@timePeriodCount > 0)
        SET @sql += N'AND EXISTS(SELECT 1 from @timePeriods t WHERE t.year = o.Year AND t.timeIdentifier = o.TimeIdentifier) '
    SET @sql += N'GROUP BY o.Id ' +
                'HAVING COUNT(DISTINCT ofi.FilterItemId) = (' +
                '    SELECT COUNT(DISTINCT f.id) AS filterCount' +
                '    FROM' +
                '    FilterItem fi' +
                '    JOIN FilterGroup fg ON fi.FilterGroupId = fg.Id' +
                '    JOIN Filter f ON fg.FilterId = f.Id' +
                '    WHERE fi.Id IN (SELECT id FROM @filterItemIds)' +
                ') ' +
                'ORDER BY o.Id;'

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
