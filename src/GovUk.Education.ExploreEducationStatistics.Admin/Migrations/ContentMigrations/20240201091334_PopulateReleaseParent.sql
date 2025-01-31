-- Populate a temp table
-- Use a CTE query which assigns a new release parent id for every release version 0
-- Recurse through all other versions to identify the release parent id from the previous version via PreviousVersionId
WITH Release_Recursive AS (
    SELECT
        Id
         ,PreviousVersionId
         ,Version
         ,NEWID() AS ReleaseParentId
    FROM Releases rel
    WHERE Version = 0
    UNION ALL
    SELECT
        Releases.Id
         ,Release_Recursive.Id
         ,Releases.Version
         ,Release_Recursive.ReleaseParentId
    FROM Releases
        INNER JOIN Release_Recursive ON Releases.PreviousVersionId = Release_Recursive.Id
)
SELECT *
INTO #ReleasesWithReleaseParentIds
FROM Release_Recursive
ORDER BY ReleaseParentId, Version;

-- Insert new values into the ReleaseParents table for all the distinct ReleaseParentId's found in the temp table
INSERT INTO ReleaseParents (
    Id,
    Created)
SELECT
    DISTINCT ReleaseParentId,
    GETUTCDATE()
FROM #ReleasesWithReleaseParentIds;

-- Update the Releases table with the ReleaseParentId's found in the temp table for all releases
UPDATE Releases
SET ReleaseParentId = Temp.ReleaseParentId
FROM Releases R
    INNER JOIN #ReleasesWithReleaseParentIds Temp ON R.Id = Temp.Id;
