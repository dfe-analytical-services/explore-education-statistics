CREATE OR ALTER PROCEDURE InsertObservations @Observations dbo.ObservationType READONLY
AS
BEGIN
INSERT INTO Observation (Id, SubjectId, LocationId, Year, TimeIdentifier, Measures, CsvRow)
SELECT Id,
       SubjectId,
       LocationId,
       Year,
       TimeIdentifier,
       Measures,
       CsvRow
FROM @Observations
END
