-- Migrate existing UNACCEPTED UserInvites to Users

-- Get the ID of the placeholder deleted user
DECLARE @DeletedUserId UNIQUEIDENTIFIER;

SELECT @DeletedUserId = Id
FROM Users
WHERE Email = 'deleted.user@doesnotexist.com';

INSERT INTO Users (
    Id,
    FirstName,
    LastName,
    Email,
    DeletedById,
    SoftDeleted,
    Active,
    RoleId,
    Created,
    CreatedById
)
SELECT
    NEWID() AS Id,      
    NULL AS FirstName,    
    NULL AS LastName,       
    ui.Email,               
    NULL AS DeletedById,   
    NULL AS SoftDeleted,   
    0 AS Active,           
    ui.RoleId,             
    ui.Created,          
    COALESCE(ui.CreatedById, @DeletedUserId) AS CreatedById   -- use placeholder if NULL   
FROM UserInvites ui
WHERE ui.Accepted = 0;