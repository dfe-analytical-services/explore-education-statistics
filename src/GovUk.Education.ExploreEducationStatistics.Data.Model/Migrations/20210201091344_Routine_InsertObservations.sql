CREATE PROCEDURE InsertObservations
    @Observations dbo.ObservationType READONLY,
    @ObservationFilterItems dbo.ObservationFilterItemType READONLY
AS
BEGIN

    CREATE TABLE #InsertedObservations(
      Id BIGINT NOT NULL,
      CsvRow BIGINT NOT NULL
    );

    INSERT INTO ObservationRow
    (ObservationId, SubjectId, GeographicLevel, LocationId, Year, TimeIdentifier, Measures, CsvRow)
    OUTPUT INSERTED.Id, INSERTED.CsvRow INTO #InsertedObservations
    SELECT * FROM @Observations;

    INSERT INTO ObservationRowFilterItem
    (t.ObservationId, f.OldObservationId, f.FilterItemId, f.FilterId)
    SELECT t.Id, f.OldObservationId, f.FilterItemId, f.FilterId
    FROM @ObservationFilterItems f
    JOIN #InsertedObservations t ON f.CsvRow = t.CsvRow;

    DROP TABLE #InsertedObservations;
END
GO