-- Create a view using the WITH SCHEMABINDING option
CREATE VIEW dbo.vObservationSubjectIdYearTimeIdentifier
    WITH SCHEMABINDING
    AS
    SELECT
        SubjectId,
        Year,
        TimeIdentifier,
        COUNT_BIG(*) AS Count
    FROM dbo.Observation
    GROUP BY SubjectId, Year, TimeIdentifier
GO

-- Create an index on the view
CREATE UNIQUE CLUSTERED INDEX IX_vObservationSubjectIdYearTimeIdentifier_SubjectId_Year_TimeIdentifier
    ON dbo.vObservationSubjectIdYearTimeIdentifier(SubjectId,Year,TimeIdentifier)
GO
