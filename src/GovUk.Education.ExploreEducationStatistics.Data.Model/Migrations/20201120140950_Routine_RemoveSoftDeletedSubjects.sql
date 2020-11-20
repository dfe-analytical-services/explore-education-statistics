CREATE OR
ALTER PROCEDURE RemoveSoftDeletedSubjects @TotalObservationLimit INT,
                                          @ObservationCommitBatchSize INT,
                                          @ObservationFilterItemCommitBatchSize INT
AS
BEGIN
    DROP TABLE IF EXISTS #SoftDeletedSubjects;
    CREATE TABLE #SoftDeletedSubjects
    (
        Id               UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Filename         NVARCHAR(MAX),
        Name             NVARCHAR(MAX)    NOT NULL,
        ObservationCount INT              NOT NULL,
        RunningTotal     INT              NOT NULL
    );
    WITH ObservationCounts(Id, Filename, Name, ObservationCount, RunningTotal) AS
             (
                 SELECT Subject.Id,
                        Subject.Filename,
                        Subject.Name,
                        COUNT(Observation.Id)                                            AS ObservationCount,
                        SUM(COUNT(Observation.Id)) OVER (ORDER BY COUNT(Observation.Id)) AS RunningTotal
                 FROM Subject
                          LEFT JOIN Observation ON Subject.Id = Observation.SubjectId
                 WHERE Subject.SoftDeleted = 1
                   AND NOT EXISTS(SELECT *
                                  FROM ReleaseSubject
                                  WHERE ReleaseSubject.SubjectId = Subject.Id)
                 GROUP BY Subject.Id, Subject.Filename, Subject.Name
             )
    INSERT
    INTO #SoftDeletedSubjects
    SELECT *
    FROM ObservationCounts
    WHERE RunningTotal <= @TotalObservationLimit
    ORDER BY ObservationCount;

    IF EXISTS(SELECT 1 from #SoftDeletedSubjects)
        BEGIN
            DECLARE @SubjectListAsJson VARCHAR(MAX) = (SELECT *
                                                       FROM #SoftDeletedSubjects
                                                       ORDER BY ObservationCount
                                                       FOR JSON AUTO);
            RAISERROR (N'Top Soft-deleted Subjects (limited to %d Observations): %s', 0, 1, @TotalObservationLimit, @SubjectListAsJson) WITH NOWAIT;

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

            RAISERROR (N'Deleting from Subject', 0, 1) WITH NOWAIT;

            DELETE S
            FROM Subject S
                     JOIN #SoftDeletedSubjects ON S.Id = #SoftDeletedSubjects.Id;
        END;
    ELSE
        RAISERROR (N'No soft deleted Subjects found to delete', 0, 1) WITH NOWAIT;
    DROP TABLE #SoftDeletedSubjects;
END;
