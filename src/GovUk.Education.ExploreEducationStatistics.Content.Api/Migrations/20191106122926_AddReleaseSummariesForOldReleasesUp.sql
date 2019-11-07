BEGIN TRANSACTION;

DECLARE @ids TABLE
             (
                 ReleaseId           uniqueidentifier,
                 NewReleaseSummaryId uniqueidentifier
             );

INSERT INTO @ids
SELECT R.Id, NEWID()
FROM Releases R
         LEFT JOIN ReleaseSummaries RS ON R.Id = RS.ReleaseId
WHERE RS.Id IS NULL;

INSERT INTO ReleaseSummaries(Id, ReleaseId)
SELECT I.NewReleaseSummaryId AS Id,
       I.ReleaseId           AS ReleaseId
FROM @ids I;

INSERT INTO ReleaseSummaryVersions
SELECT NEWID()               AS Id,
       I.NewReleaseSummaryId AS ReleaseSummaryId,
       R.ReleaseName,
       R.PublishScheduled,
       R.Slug,
       R.Summary,
       R.TypeId,
       R.TimePeriodCoverage,
       R.NextReleaseDate,
       GETDATE()
FROM @ids I
         INNER JOIN Releases R ON R.Id = I.ReleaseId;

COMMIT;
