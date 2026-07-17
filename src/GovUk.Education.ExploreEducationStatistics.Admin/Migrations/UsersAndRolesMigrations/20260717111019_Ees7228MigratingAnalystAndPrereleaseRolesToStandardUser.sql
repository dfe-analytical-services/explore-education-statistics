-- Migrating Analyst and Prerelease users to the new Standard User role, and removing the Analyst and Prerelease roles from the system.
UPDATE AspNetUserRoles
SET RoleId = 'f9ddb43e-aa9e-41ed-837d-3062e130c425'
WHERE RoleId = '17e634f4-7a2b-4a23-8636-b079877b4232';

UPDATE Users
SET RoleId = 'f9ddb43e-aa9e-41ed-837d-3062e130c425'
WHERE RoleId = '17e634f4-7a2b-4a23-8636-b079877b4232';
