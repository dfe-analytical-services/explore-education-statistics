CREATE OR ALTER PROCEDURE dbo.KillIndexReorganizations
    @Tables nvarchar(MAX) -- This is a comma-delimited list of table names e.g. "Observation, ObservationFilterItem".
AS
BEGIN
    SET NOCOUNT ON;

    IF LEN(ISNULL(@Tables, '')) = 0
    BEGIN
        RAISERROR ('Empty @Tables argument provided', 0, 1, NULL) WITH NOWAIT;
        RETURN;
    END

    -- Put the individual table names along with their schema names into a temp table.
    SELECT obj.name as TableName, OBJECT_SCHEMA_NAME(obj.object_id) as SchemaName
    INTO #TableList
    FROM STRING_SPLIT(@Tables, ',')
    JOIN sys.objects obj ON obj.name = TRIM(value);

    -- Find any ALTER INDEX...REORGANIZE commands that reference any of the table names
    -- with or without their schema prefixed.
    SELECT 
        IDENTITY (INT, 1, 1) AS Id,
        r.session_id AS SessionId,
        txt.text AS SqlText,
        r.status AS Status
    INTO #RunningReorganizations
    FROM sys.dm_exec_requests r
    CROSS APPLY sys.dm_exec_sql_text(r.sql_handle) txt
    OUTER APPLY sys.dm_exec_plan_attributes(r.plan_handle) AS p
    WHERE r.database_id = DB_ID()
    AND EXISTS (
        SELECT 1 FROM #TableList tbl
        WHERE txt.text LIKE 'ALTER INDEX%ON ' + tbl.TableName + ' ' + '%REORGANIZE%'
        OR txt.text LIKE 'ALTER INDEX%ON ' + tbl.SchemaName + '.' + tbl.TableName + ' %REORGANIZE%'
    );
    
    DECLARE 
        @ReorganizationsCount INT,
        @ReorganizationsIndex INT = 1,
        @Command NVARCHAR(MAX),
        @SessionId INT,
        @SqlText NVARCHAR(MAX);
    
    SELECT @ReorganizationsCount = COUNT(*) FROM #RunningReorganizations;
    
    -- For each REORGANIZE to kill, issue a kill command.
    WHILE @ReorganizationsIndex <= @ReorganizationsCount
    BEGIN
        SELECT @Command = 'KILL ' + CAST(SessionId AS NVARCHAR),
               @SessionId = SessionId,
               @SqlText = SqlText
        FROM #RunningReorganizations
        WHERE Id = @ReorganizationsIndex;
        
        RAISERROR ('Executing command ''%s'' to kill session %d with sql ''%s''.', 0, 1, @Command, @SessionId, @SqlText) WITH NOWAIT;
        EXEC (@Command);
        
        SET @ReorganizationsIndex += 1;
    END
END