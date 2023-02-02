-- Migrate NotifySubscribers and NotifiedOn from the latest 'Approved' release status of the release if one exists

UPDATE Releases
SET Releases.NotifySubscribers = LatestReleaseStatus.NotifySubscribers,
    Releases.NotifiedOn        = LatestReleaseStatus.NotifiedOn
    FROM Releases
         CROSS APPLY
     (SELECT TOP (1) NotifySubscribers,
                     NotifiedOn
      FROM ReleaseStatus
      WHERE ReleaseStatus.ApprovalStatus = 'Approved'
        AND ReleaseStatus.ReleaseId = Releases.Id
      ORDER BY ReleaseStatus.Created DESC) LatestReleaseStatus

-- ReleaseStatus.NotifySubscribers and ReleaseStatus.NotifiedOn will be dropped by a further migration
-- once this has been successfully run in all environments. See EES-4058.
