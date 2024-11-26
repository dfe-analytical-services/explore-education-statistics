UPDATE Releases
SET Releases.PublicationId      = ReleaseVersions.PublicationId,
    Releases.Slug               = ReleaseVersions.Slug,
    Releases.Year               = TRY_CAST(ReleaseVersions.ReleaseName AS INT),
    Releases.TimePeriodCoverage = ReleaseVersions.TimePeriodCoverage,
    Releases.Updated            = SYSUTCDATETIME()
FROM Releases
         INNER JOIN
     ReleaseVersions
     ON Releases.Id = ReleaseVersions.ReleaseId
         INNER JOIN
     (SELECT RV2.ReleaseId, MAX(RV2.Version) AS Version
      FROM ReleaseVersions RV2
      WHERE RV2.SoftDeleted = 0
         AND (RV2.Published IS NOT NULL OR (RV2.Published IS NULL AND RV2.Version = 0))
      GROUP BY ReleaseId) LatestVersion
     ON ReleaseVersions.ReleaseId = LatestVersion.ReleaseId AND ReleaseVersions.Version = LatestVersion.Version
WHERE ReleaseVersions.SoftDeleted = 0
