IF NOT EXISTS 
   (
     SELECT name FROM master.dbo.sysdatabases 
    WHERE name = N'content'
    )
BEGIN
    CREATE DATABASE [content]
    SELECT 'content database has been created'
END;

IF NOT EXISTS 
   (
     SELECT name FROM master.dbo.sysdatabases 
    WHERE name = N'statistics'
    )
BEGIN
    CREATE DATABASE [statistics]
    SELECT 'statistics database has been created'
END;