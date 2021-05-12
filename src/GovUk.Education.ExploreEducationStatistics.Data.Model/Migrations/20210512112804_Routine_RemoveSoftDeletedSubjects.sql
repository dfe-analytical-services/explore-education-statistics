CREATE OR
ALTER PROCEDURE RemoveSoftDeletedSubjects @TotalObservationLimit INT,
                                          @ObservationCommitBatchSize INT,
                                          @ObservationFilterItemCommitBatchSize INT
AS
BEGIN
    -- Create a temporary table of soft-deleted Subjects
    DROP TABLE IF EXISTS #SoftDeletedSubjects;
    CREATE TABLE #SoftDeletedSubjects
    (
        Id               UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ObservationCount INT              NOT NULL,
        RunningTotal     INT              NOT NULL
    );

    -- Define a temporary result set of all soft-deleted Subjects
    -- For each Subject ensure no ReleaseSubject link (should never happen)
    -- For each Subject include the Observation count
    -- Order the Subjects in ascending order by Observation count
    -- Calculate a running total of the Observation count
    -- Insert from this result set into the table of soft-deleted Subjects where the running total is less than the desired Observation limit
    WITH ObservationCounts(Id, ObservationCount, RunningTotal) AS
             (
                 SELECT Subject.Id,
                        COUNT(Observation.Id) AS ObservationCount,
                        SUM(COUNT(Observation.Id))
                            OVER (ORDER BY COUNT(Observation.Id) ROWS UNBOUNDED PRECEDING) AS RunningTotal
                 FROM Subject
                          LEFT JOIN Observation ON Subject.Id = Observation.SubjectId
                 WHERE Subject.SoftDeleted = 1
                   AND NOT EXISTS(SELECT *
                                  FROM ReleaseSubject
                                  WHERE ReleaseSubject.SubjectId = Subject.Id)
                 GROUP BY Subject.Id
             )
    INSERT
    INTO #SoftDeletedSubjects
    SELECT *
    FROM ObservationCounts
    WHERE RunningTotal <= @TotalObservationLimit
    ORDER BY ObservationCount;

    -- If any soft-deleted Subjects are found
    IF EXISTS(SELECT 1 from #SoftDeletedSubjects)
        BEGIN
            -- Log the table of soft-deleted Subjects immediately as JSON for information
            DECLARE @SubjectListAsJson VARCHAR(MAX) = (SELECT *
                                                       FROM #SoftDeletedSubjects
                                                       ORDER BY ObservationCount
                                                       FOR JSON AUTO);
            RAISERROR (N'Top Soft-deleted Subjects (limited to %d Observations): %s', 0, 1, @TotalObservationLimit, @SubjectListAsJson) WITH NOWAIT;

            -- Delete the ObservationFilterItem link rows associated with the Subjects
            -- Delete in batches to prevent the transaction log file from exploding in size
            RAISERROR (N'Deleting from ObservationFilterItem', 0, 1) WITH NOWAIT;
            DECLARE @observationFilterItemRowsAffected INT = 1
            WHILE @observationFilterItemRowsAffected <> 0
                BEGIN
                    BEGIN TRANSACTION
                        DELETE TOP (@ObservationFilterItemCommitBatchSize) OFI
                        FROM ObservationFilterItem OFI
                        WHERE EXISTS(SELECT 1
                                     FROM Filter
                                     WHERE Filter.Id = OFI.FilterId
                                       AND Filter.SubjectId IN (SELECT Id FROM #SoftDeletedSubjects));

                        SET @observationFilterItemRowsAffected = @@ROWCOUNT
                    COMMIT;
                END;

            -- Delete the Observation rows associated with the Subjects
            -- Delete in batches to prevent the transaction log file from exploding in size
            RAISERROR (N'Deleting from Observation', 0, 1) WITH NOWAIT;
            DECLARE @observationRowsAffected INT = 1
            WHILE @observationRowsAffected <> 0
                BEGIN
                    BEGIN TRANSACTION
                        DELETE TOP (@ObservationCommitBatchSize) O
                        FROM Observation O
                                 JOIN #SoftDeletedSubjects ON O.SubjectId = #SoftDeletedSubjects.Id;

                        SET @observationRowsAffected = @@ROWCOUNT
                    COMMIT;
                END;

            -- Delete the Subjects
            -- This includes a cascade delete of all records in child tables referencing the Subject)
            RAISERROR (N'Deleting from Subject', 0, 1) WITH NOWAIT;
            DELETE S
            FROM Subject S
                     JOIN #SoftDeletedSubjects ON S.Id = #SoftDeletedSubjects.Id;
        END;
    ELSE
        RAISERROR (N'No soft deleted Subjects found to delete', 0, 1) WITH NOWAIT;
    -- Drop the temporary table of soft-deleted Subjects
    DROP TABLE #SoftDeletedSubjects;
END
