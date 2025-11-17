CREATE OR ALTER PROCEDURE dbo.PauseResumableIndexRebuilds
    @Tables nvarchar(MAX) -- This is a comma-delimited list of table names e.g. "Observation, ObservationFilterItem"
AS
BEGIN
    SET NOCOUNT ON;

    IF LEN(ISNULL(@Tables, '')) = 0
    BEGIN
        RAISERROR ('Empty @Tables argument provided', 0, 1, NULL) WITH NOWAIT;
        RETURN;
    END

    -- Put the individual table names along with their schema names into a temp table.
    SELECT obj.name as TableName,
           OBJECT_SCHEMA_NAME(obj.object_id) as SchemaName
    INTO #TableList
    FROM STRING_SPLIT(@Tables, ',')
    JOIN sys.objects obj ON obj.name = TRIM(value);

    -- Find any currently-running resumable index rebuilds.
    SELECT IDENTITY(INT, 1, 1) AS Id,
           tbl.TableName,
           tbl.SchemaName,
           r.name AS IndexName
    INTO #RunningResumables
    FROM sys.index_resumable_operations AS r
    JOIN sys.objects obj ON obj.object_id = r.object_id
    JOIN #TableList tbl ON obj.name = tbl.TableName
    WHERE r.state_desc = N'RUNNING';
    
    DECLARE 
        @ResumablesCount INT,
        @ResumablesIndex INT = 1,
        @Command NVARCHAR(MAX);

    SELECT @ResumablesCount = COUNT(*) FROM #RunningResumables;

    -- For each resumable index rebuild, issue a PAUSE command.
    WHILE @ResumablesIndex <= @ResumablesCount
    BEGIN
        BEGIN TRY
        
            SELECT @Command = 'ALTER INDEX ' + IndexName + ' ON ' + SchemaName + '.' + TableName + ' PAUSE'
            FROM #RunningResumables
            WHERE Id = @ResumablesIndex;
            
            RAISERROR ('Executing command ''%s''.', 0, 1, @Command) WITH NOWAIT;
            EXEC (@Command);
        
        END TRY
        BEGIN CATCH
                    
            DECLARE @errorMessage NVARCHAR(max) = ERROR_MESSAGE();
            DECLARE @errorSeverity INT = ERROR_SEVERITY();
            DECLARE @errorState INT = ERROR_STATE();
            DECLARE @errorProcedure NVARCHAR(max) = ERROR_PROCEDURE();
            RAISERROR (N'Error executing %s MESSAGE: %s', @errorSeverity, @errorState, @errorProcedure, @errorMessage);
        
        END CATCH

        SET @ResumablesIndex += 1;
    END
END