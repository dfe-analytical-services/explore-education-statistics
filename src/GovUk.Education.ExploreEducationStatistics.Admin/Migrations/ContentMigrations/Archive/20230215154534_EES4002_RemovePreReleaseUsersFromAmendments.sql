-- Delete any Pre-release users from any existing Release amendments, as we require them to initially have blank 
-- pre-release user lists when an amendment is created.
DELETE FROM UserReleaseRoles
WHERE Id IN (
    SELECT urr.Id
    FROM UserReleaseRoles urr
    JOIN Releases ON Releases.Id = urr.ReleaseId
    WHERE urr.Role = 'PrereleaseViewer'
    AND Releases.PreviousVersionId IS NOT NULL
);