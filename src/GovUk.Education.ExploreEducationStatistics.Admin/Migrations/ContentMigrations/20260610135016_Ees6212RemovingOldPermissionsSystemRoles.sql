-- Remove all OLD permissions system roles:
-- - Publication `Allower`
-- - Publication `Owner`
-- - Release `Approver`
-- - Release `Contributor`
DELETE FROM UserPublicationRoles
WHERE Role IN ('Allower', 'Owner');

DELETE FROM UserReleaseRoles
WHERE Role IN ('Approver', 'Contributor');