CREATE OR ALTER PROCEDURE RebuildIndexes @Tables NVARCHAR(MAX), -- comma separated list of user-defined table names
                                         @FragmentationThresholdReorganize FLOAT = 5,
                                         @FragmentationThresholdRebuild FLOAT = 30,
                                         @StopInMinutes INT = 600
AS
BEGIN
    IF LEN(ISNULL(@Tables, '')) = 0
        BEGIN
            RAISERROR ('Empty @Tables argument provided', 0, 1, NULL) WITH NOWAIT;
            RETURN;
        END

    DECLARE @StatsCount INT,
        @StatsRowIndex INT = 1,
        @TablesCount INT,
        @TablesRowIndex INT = 1,
        @RunId INT,
        @StopTime DATETIME = DATEADD(MINUTE, @StopInMinutes, GETUTCDATE()),
        @SkipRemainingTasks BIT = 0,

        -- Used when iterating over indexes
        @Command NVARCHAR(MAX),
        @FragPercentBefore FLOAT,
        @IndexName NVARCHAR(MAX),
        @SchemaName NVARCHAR(MAX),
        @ObjectName NVARCHAR(MAX),
        @ActionRequired NVARCHAR(MAX),

        -- Used when iterating over tables
        @CurrentTable NVARCHAR(MAX) = '',
        @CurrentTableIndexesProcessed INT;

    DROP TABLE IF EXISTS #TableList;

    CREATE TABLE #TableList
    (
        Id         INT IDENTITY (1, 1),
        ObjectName NVARCHAR(128),
        PRIMARY KEY (Id)
    );

    CREATE NONCLUSTERED INDEX ix_temp_idx ON #TableList (ObjectName);

    INSERT INTO #TableList (ObjectName)
    SELECT TRIM(value)
    FROM STRING_SPLIT(@Tables, ',');

    /*-----------------------------------------------------------------------*/

    DROP TABLE IF EXISTS #Stats;

    -- Get a list of all indexes and their fragmentation percentage
    SELECT Id = IDENTITY(INT, 1, 1),
                Fragmented.IndexName,
                Fragmented.SchemaName,
                Fragmented.ObjectName,
                Fragmented.FragPercent,
                CASE
                    WHEN Fragmented.FragPercent >= @FragmentationThresholdRebuild THEN 'Rebuild'
                    WHEN Fragmented.FragPercent >= @FragmentationThresholdReorganize THEN 'Reorganize'
                    ELSE 'None'
                    END As ActionRequired
    INTO #Stats
    FROM (SELECT idx.name                         AS IndexName,
                 sch.name                         AS SchemaName,
                 tl.ObjectName,
                 ips.avg_fragmentation_in_percent AS FragPercent
          FROM #TableList AS tl
                   JOIN sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ips
                        ON ips.object_id = OBJECT_ID(tl.ObjectName, 'U')
                   JOIN sys.indexes idx ON idx.object_id = ips.object_id AND idx.index_id = ips.index_id
                   JOIN sys.objects obj ON obj.object_id = ips.object_id
                   JOIN sys.schemas sch ON sch.schema_id = obj.schema_id
          WHERE ips.alloc_unit_type_desc = 'IN_ROW_DATA') AS Fragmented;

    ALTER TABLE #Stats
        ADD PRIMARY KEY (Id);

    /*-----------------------------------------------------------------------*/

    -- Create result tables if necessary
    IF OBJECT_ID(N'__Log_RebuildIndexes', N'U') IS NULL
    CREATE TABLE __Log_RebuildIndexes
    (
        Id         INT IDENTITY (1,1),
        StartTime  DATETIME2 NOT NULL,
        EndTime    DATETIME2,
        HitTimeout BIT,
        ReportSent DATETIME2,
        PRIMARY KEY (Id),
    );

    IF OBJECT_ID(N'__Log_RebuildIndexesAlterIndexes', N'U') IS NULL
    CREATE TABLE __Log_RebuildIndexesAlterIndexes
    (
        Id               INT IDENTITY (1,1),
        StatsRowIndex    INT           NOT NULL,
        RunId            INT           NOT NULL,
        IndexName        NVARCHAR(MAX) NOT NULL,
        SchemaName       NVARCHAR(MAX) NOT NULL,
        ObjectName       NVARCHAR(MAX) NOT NULL,
        StartTime        DATETIME2,
        EndTime          DATETIME2,
        StartFragPercent FLOAT,
        EndFragPercent   FLOAT,
        ActionRequired   NVARCHAR(MAX),
        HitTimeout       BIT DEFAULT 0,
        PRIMARY KEY (Id),
        FOREIGN KEY (RunId) REFERENCES __Log_RebuildIndexes (Id),
    );

    IF OBJECT_ID(N'__Log_RebuildIndexesUpdateStatistics', N'U') IS NULL
    CREATE TABLE __Log_RebuildIndexesUpdateStatistics
    (
        Id        INT IDENTITY (1,1),
        RunId     INT           NOT NULL,
        TableName NVARCHAR(MAX) NOT NULL,
        StartTime DATETIME2,
        EndTime   DATETIME2,
        PRIMARY KEY (Id),
        FOREIGN KEY (RunId) REFERENCES __Log_RebuildIndexes (Id),
    );

    -- Create new row for this stored procedure run
    INSERT INTO __Log_RebuildIndexes (StartTime)
    VALUES (GETUTCDATE())
    SET @RunId = SCOPE_IDENTITY();
    -- The Id is generated, but we want it, so set @RunId equal to it here

    -- Create rows for indexes
    SELECT @StatsCount = COUNT(Id) FROM #Stats;

    SET @StatsRowIndex = 1;

    INSERT INTO __Log_RebuildIndexesAlterIndexes (StatsRowIndex, RunId, IndexName, SchemaName,
                                                  ObjectName, StartFragPercent, ActionRequired)
    SELECT Id,
           @RunId,
           IndexName,
           SchemaName,
           ObjectName,
           FragPercent,
           ActionRequired
    FROM #Stats;

    -- Create rows for tables
    INSERT INTO __Log_RebuildIndexesUpdateStatistics (RunId, TableName)
    SELECT @RunId,
           TL.ObjectName
    FROM #TableList TL;

    /*-----------------------------------------------------------------------*/

    -- Rebuild or reorganize each index depending on the fragmentation percentage
    SET @StatsRowIndex = 1;

    WHILE @StatsRowIndex <= @StatsCount AND @SkipRemainingTasks = 0
        BEGIN
            SELECT @Command =
                   'ALTER INDEX ' + IndexName + ' ON ' + SchemaName + '.' + ObjectName +
                   IIF(ActionRequired = 'Rebuild',
                       ' REBUILD WITH (ONLINE = ON)',
                       ' REORGANIZE'),
                   @FragPercentBefore = FragPercent,
                   @IndexName = IndexName,
                   @SchemaName = SchemaName,
                   @ObjectName = ObjectName,
                   @ActionRequired = ActionRequired
            FROM #Stats
            WHERE Id = @StatsRowIndex;

            -- Save start time for reporting
            UPDATE __Log_RebuildIndexesAlterIndexes
            SET StartTime = GETUTCDATE()
            WHERE RunId = @RunId
              AND ObjectName = @ObjectName
              AND SchemaName = @SchemaName
              AND IndexName = @IndexName;

            -- Do this check after setting StartTime, as we rely on StartTime to determine which indexes we're going
            -- to skip if we pass @StopTime
            IF @ActionRequired = 'None'
                BEGIN
                    RAISERROR ('Skipping command - fragmentation percent did not hit reorganisation threshold ''%s''.', 0, 1,
                        @Command) WITH NOWAIT;
                    SET @StatsRowIndex += 1;
                    CONTINUE;
                END

            -- Reorganise or rebuild
            RAISERROR ('Executing command ''%s''.', 0, 1, @Command) WITH NOWAIT;
            EXEC (@Command);

            -- Save end info for reporting
            UPDATE __Log_RebuildIndexesAlterIndexes
            SET __Log_RebuildIndexesAlterIndexes.EndTime        = GETUTCDATE(),
                __Log_RebuildIndexesAlterIndexes.EndFragPercent = Fragmented.FragPercent
            FROM (SELECT ips.avg_fragmentation_in_percent AS FragPercent
                  FROM #TableList AS tl
                           JOIN sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ips
                                ON ips.object_id = OBJECT_ID(tl.ObjectName, 'U')
                           JOIN sys.indexes idx ON idx.object_id = ips.object_id AND idx.index_id = ips.index_id
                           JOIN sys.objects obj ON obj.object_id = ips.object_id
                           JOIN sys.schemas sch ON sch.schema_id = obj.schema_id
                  WHERE ips.alloc_unit_type_desc = 'IN_ROW_DATA'
                    AND tl.ObjectName = @ObjectName
                    AND sch.Name = @SchemaName
                    AND idx.name = @IndexName) AS Fragmented
            WHERE __Log_RebuildIndexesAlterIndexes.RunId = @RunId
              AND __Log_RebuildIndexesAlterIndexes.ObjectName = @ObjectName
              AND __Log_RebuildIndexesAlterIndexes.SchemaName = @SchemaName
              AND __Log_RebuildIndexesAlterIndexes.IndexName = @IndexName;

            IF @StopTime < GETUTCDATE()
                BEGIN
                    RAISERROR ('Hit timeout while processing indexes. Skipping remaining tasks', 0, 1, NULL)
                        WITH NOWAIT;

                    -- We set the StartTime even when ActionRequired = 'None'. This allows us to update rows that
                    -- won't be processed if we've exceeded @StopTime
                    UPDATE __Log_RebuildIndexesAlterIndexes
                    SET HitTimeout = 1
                    WHERE StartTime IS NULL;

                    SET @SkipRemainingTasks = 1;
                END

            SET @StatsRowIndex += 1;
        END

    RAISERROR ('Completed optimising indexes for tables.', 0, 1, NULL) WITH NOWAIT;

    /*-----------------------------------------------------------------------*/

    -- Call UPDATE STATISTICS on any tables that have had their indexes rebuilt/reorganized
    SET @TablesRowIndex = 1;
    SELECT @TablesCount = COUNT(Id) FROM #TableList;
    WHILE @TablesRowIndex <= @TablesCount AND @SkipRemainingTasks = 0
        BEGIN
            SELECT @CurrentTable = ObjectName
            FROM #TableList
            WHERE Id = @TablesRowIndex;

            SET @Command = 'UPDATE STATISTICS ' + @CurrentTable;

            -- Save start time
            UPDATE __Log_RebuildIndexesUpdateStatistics
            SET StartTime = GETUTCDATE()
            WHERE RunId = @RunId
              AND TableName = @CurrentTable;

            -- Skip if no indexes related to table were rebuilt/reorganized
            SELECT @CurrentTableIndexesProcessed = COUNT(*)
            FROM __Log_RebuildIndexesAlterIndexes
            WHERE RunId = @RunId
              AND ObjectName = @CurrentTable
              AND EndTime IS NOT NULL

            IF @CurrentTableIndexesProcessed = 0
                BEGIN
                    RAISERROR ('No indexes related to table ''%s'' updated, so skipping update statistics.',
                        0, 1, @CurrentTable) WITH NOWAIT;
                    SET @TablesRowIndex += 1;
                    CONTINUE;
                END

            -- Run
            RAISERROR ('Updating statistics on table ''%s''.', 0, 1, @CurrentTable) WITH NOWAIT;
            EXEC (@Command);

            -- Save end time
            UPDATE __Log_RebuildIndexesUpdateStatistics
            SET EndTime = GETUTCDATE()
            WHERE RunId = @RunId
              AND TableName = @CurrentTable;

            -- If we completed the last update statistics query, it doesn't matter if we've exceeded @StopTime: we've
            -- not skipped anything
            IF @TablesRowIndex + 1 <= @TablesCount AND @StopTime < GETUTCDATE()
                BEGIN
                    RAISERROR ('Hit timeout while processing tables. Skipping remaining tasks', 0, 1, NULL)
                        WITH NOWAIT;

                    SET @SkipRemainingTasks = 1;
                END

            SET @TablesRowIndex += 1;
        END

    RAISERROR ('Completed updating statistics on tables.', 0, 1, NULL) WITH NOWAIT;

    UPDATE __Log_RebuildIndexes
    SET EndTime    = GETUTCDATE(),
        HitTimeout = @SkipRemainingTasks
    WHERE Id = @RunId;

    IF @SkipRemainingTasks = 1
        BEGIN
            RAISERROR ('Reindexing did not complete in %d minutes. Remaining indexes skipped.', 16, 1, @StopInMinutes)
                WITH NOWAIT;
        END
END
