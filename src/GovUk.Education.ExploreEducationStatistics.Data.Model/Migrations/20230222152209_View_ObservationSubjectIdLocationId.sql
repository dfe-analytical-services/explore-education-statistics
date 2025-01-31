-- Create a view using the WITH SCHEMABINDING option
CREATE VIEW dbo.vObservationSubjectIdLocationId
    WITH SCHEMABINDING
    AS
    SELECT
        SubjectId,
        LocationId,
        COUNT_BIG(*) AS Count
    FROM dbo.Observation
    GROUP BY SubjectId, LocationId
GO

-- Create an index on the view
CREATE UNIQUE CLUSTERED INDEX IX_vObservationSubjectIdLocationId_SubjectId_LocationId
    ON dbo.vObservationSubjectIdLocationId(SubjectId,LocationId)
GO
