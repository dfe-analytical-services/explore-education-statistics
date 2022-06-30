--
-- User permissions for the CONTENT database
--
CREATE USER [adminapp] FROM LOGIN [adminapp];
ALTER ROLE [db_ddladmin] ADD MEMBER [adminapp];
ALTER ROLE [db_datareader] ADD MEMBER [adminapp];
ALTER ROLE [db_datawriter] ADD MEMBER [adminapp];