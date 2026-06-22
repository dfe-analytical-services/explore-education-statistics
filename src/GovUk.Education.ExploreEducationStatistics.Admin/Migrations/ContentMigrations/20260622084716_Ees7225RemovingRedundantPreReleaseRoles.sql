-- Remove any redundant pre-release roles that have been created for users who already have a
-- publication-level role for the same release.
DELETE uprr
FROM UserPreReleaseRoles uprr
INNER JOIN ReleaseVersions rv
    ON rv.Id = uprr.ReleaseVersionId
INNER JOIN Releases r
    ON r.Id = rv.ReleaseId
INNER JOIN UserPublicationRoles upr
    ON upr.UserId = uprr.UserId
   AND upr.PublicationId = r.PublicationId;