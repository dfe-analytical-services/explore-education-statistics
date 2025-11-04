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
        @ModifiedTables AS ModifiedTablesType;

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

    -- Create result tables if necessary
    IF OBJECT_ID(N'__Log_RebuildIndexes', N'U') IS NULL
    CREATE TABLE __Log_RebuildIndexes
    (
        Id         INT IDENTITY (1,1),
        StartTime  DATETIME2 NOT NULL,
        EndTime    DATETIME2,
        HitTimeout BIT,
        PRIMARY KEY (Id),
    );

    IF OBJECT_ID(N'__Log_RebuildIndexesAlterIndexes', N'U') IS NULL
    CREATE TABLE __Log_RebuildIndexesAlterIndexes
    (
        Id               INT IDENTITY (1,1),
        RunId            INT           NOT NULL,
        IndexName        NVARCHAR(MAX) NOT NULL,
        SchemaName       NVARCHAR(MAX) NOT NULL,
        ObjectName       NVARCHAR(MAX) NOT NULL,
        StartTime        DATETIME2,
        EndTime          DATETIME2,
        StartFragPercent FLOAT,
        ActionRequired   NVARCHAR(MAX),
        HitTimeout       BIT DEFAULT 0,
        PRIMARY KEY (Id),
        FOREIGN KEY (RunId) REFERENCES __Log_RebuildIndexes (Id),
    );

    -- Create new row for this stored procedure run
    INSERT INTO __Log_RebuildIndexes (StartTime)
    VALUES (GETUTCDATE())
    -- The Id is generated, but we want it, so set @RunId equal to it here
    SET @RunId = SCOPE_IDENTITY();

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
                    END AS ActionRequired
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

    -- Create rows for indexes
    SELECT @StatsCount = COUNT(Id) FROM #Stats;

    SET @StatsRowIndex = 1;

    INSERT INTO __Log_RebuildIndexesAlterIndexes (RunId, IndexName, SchemaName,
                                                  ObjectName, StartFragPercent, ActionRequired)
    SELECT @RunId,
           IndexName,
           SchemaName,
           ObjectName,
           FragPercent,
           ActionRequired
    FROM #Stats;

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
            -- to skip if we exceed @StopTime
            IF @ActionRequired = 'None'
                BEGIN
                    SET @StatsRowIndex += 1;
                    CONTINUE;
                END

            -- Reorganise or rebuild
            RAISERROR ('Executing command ''%s''.', 0, 1, @Command) WITH NOWAIT;
            EXEC (@Command);

            -- Add table to the list of modified tables to ensure statistics get updated at the end
            INSERT INTO @ModifiedTables (TableName)
            SELECT @ObjectName
            WHERE NOT EXISTS (SELECT * FROM @ModifiedTables WHERE TableName = @ObjectName);

            -- Save end info for reporting
            UPDATE __Log_RebuildIndexesAlterIndexes
            SET __Log_RebuildIndexesAlterIndexes.EndTime = GETUTCDATE()
            WHERE __Log_RebuildIndexesAlterIndexes.RunId = @RunId
              AND __Log_RebuildIndexesAlterIndexes.ObjectName = @ObjectName
              AND __Log_RebuildIndexesAlterIndexes.SchemaName = @SchemaName
              AND __Log_RebuildIndexesAlterIndexes.IndexName = @IndexName;

            IF @StopTime < GETUTCDATE()
                BEGIN
                    RAISERROR ('Hit timeout while processing indexes. Skipping remaining tasks', 0, 1, NULL) WITH NOWAIT;
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
    EXEC UpdateStatistics @ModifiedTables;

    UPDATE __Log_RebuildIndexes
    SET EndTime    = GETUTCDATE(),
        HitTimeout = @SkipRemainingTasks
    WHERE Id = @RunId;

    IF @SkipRemainingTasks = 1
        RAISERROR ('Reindexing did not complete in %d minutes. Remaining indexes skipped.', 16, 1, @StopInMinutes)
            WITH NOWAIT;
END
