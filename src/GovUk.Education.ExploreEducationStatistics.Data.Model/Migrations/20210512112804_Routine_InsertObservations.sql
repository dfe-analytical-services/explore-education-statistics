CREATE PROCEDURE InsertObservations @Observations dbo.ObservationType READONLY
AS
BEGIN
    INSERT INTO Observation SELECT * FROM @Observations
END
