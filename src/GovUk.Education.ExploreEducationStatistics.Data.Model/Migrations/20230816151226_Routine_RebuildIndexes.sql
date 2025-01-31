CREATE OR ALTER PROCEDURE RebuildIndexes @Tables NVARCHAR(MAX), -- comma separated list of user-defined table names
                                         @FragmentationThresholdReorganize INT = 5,
                                         @FragmentationThresholdRebuild INT = 30
AS
BEGIN
    DECLARE @StatsCount INT,
        @StatsRowIndex INT = 1,
        @CurrentTable NVARCHAR(MAX) = '',
        @LastTable NVARCHAR(MAX) = '',
        @Command NVARCHAR(MAX);

    IF LEN(ISNULL(@Tables, '')) > 0
        BEGIN
            DROP TABLE IF EXISTS #TableList;

            CREATE TABLE #TableList
            (
                Id INT IDENTITY(1, 1) PRIMARY KEY,
                ObjectName NVARCHAR(128)
            );

            CREATE NONCLUSTERED INDEX ix_temp_idx ON #TableList (ObjectName);

            INSERT INTO #TableList (ObjectName)
            SELECT
                TRIM(value)
            FROM STRING_SPLIT(@Tables, ',');

            /*-----------------------------------------------------------------------*/

            DROP TABLE IF EXISTS #Stats;

            -- Get a list of all indexes and their fragmentation percentage
            SELECT
                Id = IDENTITY(INT, 1, 1),
                Fragmented.IndexName,
                Fragmented.SchemaName,
                Fragmented.ObjectName,
                Fragmented.FragPercent
            INTO #Stats
            FROM (
                SELECT
                    idx.name AS IndexName,
                    sch.name AS SchemaName,
                    tl.ObjectName,
                    ips.avg_fragmentation_in_percent AS FragPercent
                FROM #TableList AS tl
                JOIN sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ips ON ips.object_id = OBJECT_ID(tl.ObjectName, 'U')
                JOIN sys.indexes idx ON idx.object_id = ips.object_id AND idx.index_id = ips.index_id
                JOIN sys.objects obj ON obj.object_id = ips.object_id
                JOIN sys.schemas sch ON sch.schema_id = obj.schema_id
                WHERE ips.alloc_unit_type_desc = 'IN_ROW_DATA'
                  AND ips.avg_fragmentation_in_percent >= @FragmentationThresholdReorganize
            ) AS Fragmented;

            ALTER TABLE #Stats ADD PRIMARY KEY(Id)

            /*-----------------------------------------------------------------------*/

            SELECT @StatsCount = COUNT(Id) FROM #Stats;

            -- Rebuild or reorganise each index depending on the fragmentation percentage
            SET @StatsRowIndex = 1;
            WHILE(@StatsRowIndex <= @StatsCount)
                BEGIN
                    SELECT @CurrentTable = ObjectName, @Command =
                           'ALTER INDEX ' + IndexName + ' ON ' + SchemaName + '.' + ObjectName +
                           IIF(FragPercent >= @FragmentationThresholdRebuild,
                               ' REBUILD WITH (ONLINE = ON)',
                               ' REORGANIZE')
                    FROM #Stats
                    WHERE Id = @StatsRowIndex;

                    -- Only print table name if we're iterating over a new table
                    IF (@CurrentTable != @LastTable)
                        BEGIN
                            RAISERROR ('Optimising indexes if necessary for table ''%s''.', 0, 1, @CurrentTable) WITH NOWAIT;
                            SET @LastTable = @CurrentTable;
                        END

                    RAISERROR ('Executing command ''%s''.', 0, 1, @Command) WITH NOWAIT;
                    EXEC (@Command);

                    SET @StatsRowIndex += 1;
                END

            RAISERROR ('Completed optimising indexes for tables.', 0, 1, NULL) WITH NOWAIT;

            /*-----------------------------------------------------------------------*/

            SET @StatsRowIndex = 1;
            SET @LastTable = '';
            SELECT @StatsCount = COUNT(Id) FROM #Stats;
            WHILE(@StatsRowIndex <= @StatsCount)
                BEGIN
                    SELECT @CurrentTable = ObjectName
                    FROM #Stats
                    WHERE Id = @StatsRowIndex;

                    -- Only update statistics if we're iterating over a new table
                    IF (@CurrentTable != @LastTable)
                        BEGIN
                            -- Update all statistics on the table
                            SET @Command = 'UPDATE STATISTICS ' + @CurrentTable;
                            RAISERROR ('Updating statistics on table ''%s''.', 0, 1, @CurrentTable) WITH NOWAIT;
                            EXEC (@Command);

                            SET @LastTable = @CurrentTable;
                        END

                    SET @StatsRowIndex += 1;
                END

            RAISERROR ('Completed updating statistics on tables.', 0, 1, NULL) WITH NOWAIT;
        END
END
