CREATE OR ALTER PROCEDURE EnableObservationConstraints
AS
BEGIN
    -- Enable constraints
    ALTER TABLE dbo.Observation WITH CHECK CHECK CONSTRAINT ALL;
    ALTER TABLE dbo.ObservationFilterItem WITH CHECK CHECK CONSTRAINT ALL;
END
GO