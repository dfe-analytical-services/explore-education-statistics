-- Migrate 'EmailSent' values for existing UserPublicationRoles and UserReleaseRoles to be `Null` or `DateTimeOffset.MinValue`.
UPDATE UserPublicationRoles 
SET EmailSent = '0001-01-01T00:00:00+00:00';

UPDATE urr
SET urr.EmailSent = IIF(
    urr.Role = 'PrereleaseViewer',
    IIF(rv.ApprovalStatus = 'Approved', '0001-01-01T00:00:00+00:00', NULL),
    '0001-01-01T00:00:00+00:00'
)
FROM UserReleaseRoles urr
JOIN ReleaseVersions rv ON rv.Id = urr.ReleaseVersionId;