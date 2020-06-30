CREATE OR ALTER PROCEDURE DisableObservationConstraints
AS
BEGIN
    -- Disable constraints
    ALTER TABLE dbo.Observation NOCHECK CONSTRAINT ALL;
    ALTER TABLE dbo.ObservationFilterItem NOCHECK CONSTRAINT ALL;
END
GO