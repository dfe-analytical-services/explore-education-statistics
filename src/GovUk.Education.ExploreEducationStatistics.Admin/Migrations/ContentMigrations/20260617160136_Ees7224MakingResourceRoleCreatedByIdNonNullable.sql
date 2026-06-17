-- Get the ID of the placeholder deleted user
DECLARE @DeletedUserId UNIQUEIDENTIFIER;

SELECT @DeletedUserId = Id
FROM Users
WHERE Email = 'deleted.user@doesnotexist.com';

-- Migrate any cases where the CreatedById is null to point to the deleted user placeholder user
-- so that we can make the column non-nullable.
UPDATE UserPublicationRoles
SET CreatedById = @DeletedUserId
WHERE CreatedById IS NULL;

UPDATE UserPreReleaseRoles
SET CreatedById = @DeletedUserId
WHERE CreatedById IS NULL;
