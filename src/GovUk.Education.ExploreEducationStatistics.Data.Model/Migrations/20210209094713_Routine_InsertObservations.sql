CREATE PROCEDURE InsertObservations
    @Observations dbo.ObservationType READONLY,
    @ObservationFilterItems dbo.ObservationFilterItemType READONLY
AS
BEGIN TRANSACTION 

    CREATE TABLE #InsertedObservations(
      Id BIGINT NOT NULL,
      CsvRow BIGINT NOT NULL
    );
    
    CREATE INDEX IX_InsertedObservations_CsvRow ON #InsertedObservations (CsvRow);

    INSERT INTO ObservationRow
    (ObservationId, SubjectId, GeographicLevel, LocationId, Year, TimeIdentifier, Measures, CsvRow)
    OUTPUT INSERTED.Id, INSERTED.CsvRow INTO #InsertedObservations
    SELECT * FROM @Observations;

    INSERT INTO ObservationRowFilterItem
    (observation.ObservationId, ofi.OldObservationId, ofi.FilterItemId, ofi.FilterId)
    SELECT observation.Id, ofi.OldObservationId, ofi.FilterItemId, ofi.FilterId
    FROM @ObservationFilterItems ofi
    JOIN #InsertedObservations observation ON ofi.CsvRow = observation.CsvRow;

    DROP TABLE #InsertedObservations;
    
COMMIT
GO