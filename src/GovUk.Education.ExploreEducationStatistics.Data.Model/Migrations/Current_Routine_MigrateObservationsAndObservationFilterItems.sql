CREATE OR
ALTER PROCEDURE MigrateObservationsAndObservationFilterItems 
    @SubjectsBatchSize INT
AS
BEGIN
    -- Create a temporary table of Subjects to migrate
    DROP TABLE IF EXISTS #SubjectsToMigrate;
    CREATE TABLE #SubjectsToMigrate
    (
        Id               UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Filename         NVARCHAR(MAX),
        Name             NVARCHAR(MAX)    NOT NULL,
        ObservationCount INT              NOT NULL
    );

    -- Define a temporary result set of all Subjects that have not yet been migrated into the ObservationRow and 
    -- ObservationRowFilterItem tables.
    -- Order them by Observation count in descending order, so we deal with the biggest first.
    -- Filter out Soft Deleted Subjects as we don't benefit by migrating these over.
    INSERT INTO #SubjectsToMigrate
    SELECT TOP (@SubjectsBatchSize) 
           Subject.Id,
           Subject.Filename,
           Subject.Name,
           COUNT(Observation.Id) AS ObservationCount
    FROM Subject
    LEFT JOIN Observation ON Subject.Id = Observation.SubjectId
    WHERE Subject.SoftDeleted = 0 
    AND NOT EXISTS (SELECT 1 FROM ObservationRow WHERE SubjectId = Subject.Id)
    GROUP BY Subject.Id, Subject.Filename, Subject.Name
    ORDER BY 4 DESC;
    
    -- If any Subjects are found to migrate
    IF EXISTS(SELECT 1 from #SubjectsToMigrate)
        BEGIN
            -- Log the table of Subjects to migrate immediately as JSON for information
            DECLARE @SubjectListAsJson VARCHAR(MAX) = (SELECT *
                                                       FROM #SubjectsToMigrate
                                                       ORDER BY ObservationCount
                                                       FOR JSON AUTO);
            RAISERROR (N'Top Subjects to migrate (limited to %d Subjects): %s', 0, 1, @SubjectsBatchSize, @SubjectListAsJson) WITH NOWAIT;

            DECLARE @SubjectId uniqueidentifier
            DECLARE @SubjectName NVARCHAR(MAX)

            SELECT @SubjectId = MIN(Id) FROM #SubjectsToMigrate
            
            WHILE @SubjectId IS NOT NULL
            BEGIN
            
                SELECT @SubjectName = Name FROM #SubjectsToMigrate WHERE Id = @SubjectId
                
                -- Migrate the Observation and ObservationFilterItem rows associated with the Subjects.
                -- Migrate in batches to prevent the transaction log file from exploding in size
                RAISERROR (N'Migrating from Observation and ObservationFilterItem to ObservationRow and ObservationRowFilterItem', 0, 1) WITH NOWAIT;
                
                BEGIN TRANSACTION
                
                    INSERT INTO ObservationRow (ObservationId, SubjectId, GeographicLevel, LocationId, Year, TimeIdentifier, Measures, CsvRow)
                    SELECT existing.Id,
                           existing.SubjectId,
                           existing.GeographicLevel,
                           existing.LocationId,
                           existing.Year,
                           existing.TimeIdentifier,
                           existing.Measures,
                           existing.CsvRow FROM (
                                SELECT o.Id,
                                       o.SubjectId,
                                       o.GeographicLevel,
                                       o.LocationId,
                                       o.Year,
                                       o.TimeIdentifier,
                                       o.Measures,
                                       o.CsvRow
                                FROM Observation o
                                WHERE o.SubjectId = @SubjectId
                           ) AS existing
                           ORDER BY existing.CsvRow ASC;
    
                    INSERT INTO ObservationRowFilterItem (ObservationId, OldObservationId, FilterItemId, FilterId)
                    SELECT observationrow.Id, observationrow.ObservationId, observationrow.FilterItemId, observationrow.FilterId 
                    FROM (
                        SELECT row.Id, ofi.ObservationId, ofi.FilterItemId, ofi.FilterId
                        FROM ObservationFilterItem ofi
                        JOIN ObservationRow row ON row.ObservationId = ofi.ObservationId
                        WHERE row.SubjectId = @SubjectId
                    ) AS observationrow
                    ORDER BY observationrow.Id ASC;
    
                COMMIT;
                
                SELECT @SubjectId = MIN(Id) FROM #SubjectsToMigrate WHERE Id > @SubjectId
            END
            
        END;
    ELSE
    
    RAISERROR (N'No Subjects found to migrate', 0, 1) WITH NOWAIT;
    -- Drop the temporary table of Subjects to migrate
    DROP TABLE #SubjectsToMigrate;
END;
