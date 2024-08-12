CREATE OR ALTER PROCEDURE RebuildIndexes @Tables NVARCHAR(MAX), -- comma separated list of user-defined table names
                                         @FragmentationThresholdReorganize INT = 5,
                                         @FragmentationThresholdRebuild INT = 30,
                                         @StopInMinutes INT = 600
AS
BEGIN
    IF LEN(ISNULL(@Tables, '')) = 0
        BEGIN
            RAISERROR ('Empty @Tables argument provided', 0, 1, NULL) WITH NOWAIT;
            RETURN
        END

    DECLARE @StatsCount INT,
        @StatsRowIndex INT = 1,
        @TablesCount INT,
        @TablesRowIndex INT = 1,
        @RunId UNIQUEIDENTIFIER = NEWID(),
        @Command NVARCHAR(MAX),
        @StopTime DATETIME = DATEADD(MINUTE, @StopInMinutes, GETUTCDATE()),
        @SkipRemainingTasks BIT = 0;

    DROP TABLE IF EXISTS #TableList;

    CREATE TABLE #TableList
    (
        Id         INT IDENTITY (1, 1) PRIMARY KEY,
        ObjectName NVARCHAR(128)
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
           Fragmented.FragPercent
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
        ADD PRIMARY KEY (Id)

    /*-----------------------------------------------------------------------*/

    -- Create result tables if necessary
    IF OBJECT_ID(N'__RebuildIndexes', N'U') IS NULL
    CREATE TABLE __RebuildIndexes
    (
        [Id]         UNIQUEIDENTIFIER NOT NULL,
        [StartTime]  DATETIME2        NOT NULL,
        [EndTime]    DATETIME2,
        [HitTimeout] BIT,
        [ReportSent] DATETIME2,
    );

    IF OBJECT_ID(N'__RebuildIndexesAlterIndexes', N'U') IS NULL
    CREATE TABLE __RebuildIndexesAlterIndexes
    (
        [Id]                 INT              NOT NULL,
        [RunId]              UNIQUEIDENTIFIER NOT NULL,
        [IndexName]          NVARCHAR(MAX)    NOT NULL,
        [SchemaName]         NVARCHAR(MAX)    NOT NULL,
        [ObjectName]         NVARCHAR(MAX)    NOT NULL,
        [StartTime]          DATETIME2,
        [EndTime]            DATETIME2,
        [InitialFragPercent] INT,
        [StartFragPercent]   INT,
        [EndFragPercent]     INT,
        [Info]               NVARCHAR(MAX),
    );

    IF OBJECT_ID(N'__RebuildIndexesUpdateStatistics', N'U') IS NULL
    CREATE TABLE __RebuildIndexesUpdateStatistics
    (
        [RunId]     UNIQUEIDENTIFIER NOT NULL,
        [TableName] NVARCHAR(MAX)    NOT NULL,
        [StartTime] DATETIME2,
        [EndTime]   DATETIME2,
    );

    -- Create new row for this stored procedure run
    INSERT INTO __RebuildIndexes (Id, StartTime, EndTime, HitTimeout, ReportSent)
    VALUES (@RunId, GETUTCDATE(), NULL, NULL, NULL);

    -- Create rows for indexes
    SELECT @StatsCount = COUNT(Id) FROM #Stats;

    SET @StatsRowIndex = 1;

    WHILE @StatsRowIndex <= @StatsCount
        BEGIN
            INSERT INTO __RebuildIndexesAlterIndexes
            SELECT @StatsRowIndex,
                   @RunId,
                   IndexName,
                   SchemaName,
                   ObjectName,
                   NULL,
                   NULL,
                   FragPercent,
                   NULL,
                   NULL,
                   NULL
            FROM #Stats
            WHERE Id = @StatsRowIndex;

            SET @StatsRowIndex += 1;
        END

    -- Create rows for tables
    INSERT INTO __RebuildIndexesUpdateStatistics
    SELECT @RunId,
           TL.ObjectName,
           NULL,
           NULL
    FROM #TableList TL;

    /*-----------------------------------------------------------------------*/

    -- Rebuild or reorganize each index depending on the fragmentation percentage
    SET @StatsRowIndex = 1;

    WHILE @StatsRowIndex <= @StatsCount AND @SkipRemainingTasks = 0
        BEGIN
            DECLARE @FragPercentBefore INT;
            SELECT @Command =
                   'ALTER INDEX ' + IndexName + ' ON ' + SchemaName + '.' + ObjectName +
                   IIF(FragPercent >= @FragmentationThresholdRebuild,
                       ' REBUILD WITH (ONLINE = ON)',
                       ' REORGANIZE'),
                   @FragPercentBefore = FragPercent
            FROM #Stats
            WHERE Id = @StatsRowIndex;

            IF @FragPercentBefore < @FragmentationThresholdReorganize
                BEGIN
                    RAISERROR ('Skipping command - fragmentation percent did not hit reorganisation threshold ''%s''.', 0, 1, @Command) WITH NOWAIT;

                    UPDATE __RebuildIndexesAlterIndexes
                    SET Info = 'Skipped: fragmentation percentage did not hit reorganisation threshold'
                    WHERE RunId = @RunId
                      AND Id = @StatsRowIndex;

                    SET @StatsRowIndex += 1;
                    CONTINUE;
                END

            DECLARE @ObjectName NVARCHAR(MAX) = '',
                @IndexName NVARCHAR(MAX) = '',
                @SchemaName NVARCHAR(MAX) = ''

            -- We set these so we can fetch the specific index we want from dm_db_index_physical_stats
            SELECT @ObjectName = ObjectName,
                   @IndexName = IndexName,
                   @SchemaName = SchemaName
            FROM __RebuildIndexesAlterIndexes
            WHERE RunId = @RunId
              AND Id = @StatsRowIndex

            -- Save start info for reporting
            UPDATE __RebuildIndexesAlterIndexes
            SET __RebuildIndexesAlterIndexes.StartTime        = GETUTCDATE(),
                __RebuildIndexesAlterIndexes.StartFragPercent = Fragmented.FragPercent
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
            WHERE __RebuildIndexesAlterIndexes.RunId = @RunId
              AND __RebuildIndexesAlterIndexes.Id = @StatsRowIndex;

            -- Reorganise or rebuild
            RAISERROR ('Executing command ''%s''.', 0, 1, @Command) WITH NOWAIT;
            EXEC (@Command);

            -- Save end info for reporting
            UPDATE __RebuildIndexesAlterIndexes
            SET __RebuildIndexesAlterIndexes.EndTime        = GETUTCDATE(),
                __RebuildIndexesAlterIndexes.EndFragPercent = Fragmented.FragPercent,
                __RebuildIndexesAlterIndexes.Info           = IIF(@FragPercentBefore >= @FragmentationThresholdRebuild,
                                                                  'Rebuilt', 'Reorganized')
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
            WHERE __RebuildIndexesAlterIndexes.RunId = @RunId
              AND __RebuildIndexesAlterIndexes.Id = @StatsRowIndex;

            IF @StopTime < GETUTCDATE()
                BEGIN
                    -- Update Info for all unprocessed indexes, because we're skipping them now we've hit @StopTime
                    UPDATE __RebuildIndexesAlterIndexes
                    SET Info = 'Skipped: stored procedure run time exceeded timeout'
                    WHERE Info IS NULL;

                    SET @SkipRemainingTasks = 1;
                END

            IF @StatsRowIndex >= @StatsCount
                RAISERROR ('Completed optimising indexes for tables.', 0, 1, NULL) WITH NOWAIT;

            SET @StatsRowIndex += 1;
        END

    /*-----------------------------------------------------------------------*/

    SET @TablesRowIndex = 1;
    SELECT @TablesCount = COUNT(Id) FROM #TableList;
    WHILE @TablesRowIndex <= @TablesCount AND @SkipRemainingTasks = 0
        BEGIN
            DECLARE @CurrentTable NVARCHAR(MAX) = '';
            SELECT @CurrentTable = ObjectName
            FROM #TableList
            WHERE Id = @TablesRowIndex;
            -- @MarkFix update this file after making all PR changes
            SET @Command = 'UPDATE STATISTICS ' + @CurrentTable;

            -- Save start time
            UPDATE __RebuildIndexesUpdateStatistics
            SET StartTime = GETUTCDATE()
            WHERE RunId = @RunId
              AND TableName = @CurrentTable;

            -- Run
            RAISERROR ('Updating statistics on table ''%s''.', 0, 1, @CurrentTable) WITH NOWAIT;
            EXEC (@Command);

            -- Save end time
            UPDATE __RebuildIndexesUpdateStatistics
            SET EndTime = GETUTCDATE()
            WHERE RunId = @RunId
              AND TableName = @CurrentTable;

            -- If we completed the last update statistics query, it doesn't matter if we've exceeded @StopTime: we've not skipped anything
            IF @TablesRowIndex + 1 <= @TablesCount AND @StopTime < GETUTCDATE()
                SET @SkipRemainingTasks = 1;

            IF @TablesRowIndex + 1 > @TablesCount
                RAISERROR ('Completed updating statistics on tables.', 0, 1, NULL) WITH NOWAIT;

            SET @TablesRowIndex += 1;
        END

    IF @SkipRemainingTasks = 1
        RAISERROR ('Reindexing did not complete in %d minutes. Remaining indexes skipped.', 16, 1, @StopInMinutes) WITH NOWAIT;

    UPDATE __RebuildIndexes
    SET EndTime    = GETDATE(),
        HitTimeout = @SkipRemainingTasks
    WHERE Id = @RunId;
END
