-- Migrate existing UNACCEPTED UserInvites to Users
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
    ui.CreatedById    
FROM UserInvites ui
WHERE ui.Accepted = 0;