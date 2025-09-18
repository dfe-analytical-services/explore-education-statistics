DECLARE @DeletedUserId UNIQUEIDENTIFIER = NEWID();
DECLARE @AnalystRoleId NVARCHAR(450);

-- Store the ID of the 'Analyst' role for later use
SELECT @AnalystRoleId = Id
FROM AspNetRoles AS r
WHERE r.Name = 'Analyst';

-- Insert a placeholder 'Deleted User' record. 
-- This will be used as a placeholder for any existing records where we don't know who created/modified them.
INSERT INTO Users (
    Id,
    FirstName,
    LastName,
    Email,
    SoftDeleted,
    Active,
    RoleId,
    Created,
    CreatedById
)
VALUES (
    @DeletedUserId,
    'Deleted',
    'User',
    'deleted.user@doesnotexist.com',
    SYSUTCDATETIME(),
    0,
    @AnalystRoleId,
    SYSUTCDATETIME(),
    @DeletedUserId
);

-- Update existing Users records to have Active = 1 (true) if they are not soft deleted.
UPDATE [Users]
SET Active = 1
WHERE SoftDeleted IS NULL;

-- Update existing Users records to have the same RoleId associated with their linked AspNetUser record (if it exists).
UPDATE [Users]
SET RoleId = ur.RoleId
FROM [Users]
INNER JOIN [AspNetUsers] AS au
    ON au.Email = [Users].Email
INNER JOIN [AspNetUserRoles] AS ur
    ON ur.UserId = au.Id
WHERE [Users].RoleId IS NULL;

-- For any remaining Users records that don't have a linked AspNetUser record, set their RoleId to the 'Analyst' role ID.
UPDATE [Users]
SET RoleId = @AnalystRoleId
WHERE [Users].RoleId IS NULL;

-- Update existing Users records to have the same Created date associated with their linked UserInvite record (if it exists).
UPDATE [Users]
SET Created = ui.Created
FROM [Users]
INNER JOIN [UserInvites] AS ui
    ON ui.Email = [Users].Email;

-- Update existing Users records so that CreatedById points to the correct Users.Id based on the CreatedById from their linked UserInvite record (if one exists).
UPDATE [Users]
SET CreatedById = TargetUser.Id
FROM [Users]
INNER JOIN [UserInvites] AS UI
    ON UI.Email = [Users].Email
INNER JOIN [AspNetUsers] AS AU
    ON AU.Id = UI.CreatedById
INNER JOIN [Users] AS TargetUser
    ON TargetUser.Email = AU.Email
WHERE [Users].CreatedById IS NULL;

-- For any remaining Users records where we couldn't resolve a matching CreatedById, assign the placeholder 'Deleted' user's Id.
UPDATE [Users]
SET CreatedById = @DeletedUserId
WHERE [Users].CreatedById IS NULL;
