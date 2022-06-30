--
-- User permissions for the STATISTICS database
--
CREATE USER [adminapp] FROM LOGIN [adminapp];
ALTER ROLE [db_ddladmin] ADD MEMBER [adminapp];
ALTER ROLE [db_datareader] ADD MEMBER [adminapp];
ALTER ROLE [db_datawriter] ADD MEMBER [adminapp];
GRANT EXECUTE ON TYPE::IdListGuidType TO [adminapp];
GRANT EXECUTE ON OBJECT::FilteredFootnotes TO [adminapp];