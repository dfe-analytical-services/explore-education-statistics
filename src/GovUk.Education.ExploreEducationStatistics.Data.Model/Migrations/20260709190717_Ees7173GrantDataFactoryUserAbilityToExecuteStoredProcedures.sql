DECLARE @SQL NVARCHAR(MAX);

-- If executing this migration in Azure SQL, grant the "datafactory" user
-- permissions to read db index information and to kill reorganizations
-- started by other sessions.
--
-- This is the equivalent of non-Azure SQL "master" database commands:
--
-- GRANT VIEW SERVER STATE TO [datafactory];
-- GRANT ALTER ANY CONNECTION TO [datafactory];
IF CAST(SERVERPROPERTY('EngineEdition') AS INT) = 5
BEGIN
    SET @SQL = N'
    GRANT KILL DATABASE CONNECTION TO [datafactory];
    ';
EXEC sp_executesql @SQL;
END