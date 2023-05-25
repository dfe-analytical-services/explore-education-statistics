CREATE OR ALTER PROCEDURE RebuildIndexes @Tables NVARCHAR(MAX), -- comma separated list of user-defined table names
                                         @FragmentationThresholdReorganize INT = 5,
                                         @FragmentationThresholdRebuild INT = 30
AS
BEGIN
    DECLARE @TableList AS TABLE
                          (
                              Id         INT IDENTITY (1, 1),
                              ObjectName NVARCHAR(MAX)
                          );

    DECLARE @Stats AS TABLE
                      (
                          Id          INT IDENTITY (1,1),
                          SchemaName  NVARCHAR(MAX),
                          ObjectName  NVARCHAR(MAX),
                          IndexName   NVARCHAR(MAX),
                          FragPercent FLOAT
                      );

    DECLARE @TableCount INT,
        @TableRowIndex INT = 1,
        @StatsCount INT,
        @StatsRowIndex INT = 1,
        @ObjectName NVARCHAR(MAX),
        @Command NVARCHAR(MAX);

    IF LEN(ISNULL(@Tables, '')) > 0
        BEGIN
            INSERT @TableList SELECT TRIM(value) FROM STRING_SPLIT(@Tables, ',');

            SELECT @TableCount = COUNT(Id) FROM @TableList;
            WHILE @TableRowIndex <= @TableCount
                BEGIN
                    SELECT @ObjectName = ObjectName
                    FROM @TableList
                    WHERE Id = @TableRowIndex;

                    RAISERROR ('Optimising indexes if necessary for table %s.', 0, 1, @ObjectName) WITH NOWAIT;

                    -- Query fragmented indexes of the table
                    INSERT INTO @Stats (IndexName, SchemaName, ObjectName, FragPercent)
                    SELECT i.name                           IndexName,
                           s.name                           SchemaName,
                           OBJECT_NAME(ips.object_id)       ObjectName,
                           ips.avg_fragmentation_in_percent FragPercent
                    FROM sys.dm_db_index_physical_stats(DB_ID(), OBJECT_ID(@ObjectName, 'U'), NULL, NULL, 'LIMITED') ips
                             JOIN sys.indexes i ON i.object_id = ips.object_id AND i.index_id = ips.index_id
                             JOIN sys.objects o ON o.object_id = ips.object_id
                             JOIN sys.schemas s ON s.schema_id = o.schema_id
                    WHERE ips.alloc_unit_type_desc = 'IN_ROW_DATA'
                      AND ips.avg_fragmentation_in_percent >= @FragmentationThresholdReorganize;

                    SELECT @StatsCount = COUNT(Id) FROM @Stats;

                    -- Rebuild or reorganise each index depending on the fragmentation percentage
                    WHILE(@StatsRowIndex <= @StatsCount)
                        BEGIN
                            SELECT @Command =
                                   'ALTER INDEX ' + IndexName + ' ON ' + SchemaName + '.' + ObjectName +
                                   IIF(FragPercent >= @FragmentationThresholdRebuild,
                                       ' REBUILD WITH (ONLINE = ON)',
                                       ' REORGANIZE')
                            FROM @Stats
                            WHERE Id = @StatsRowIndex;

                            RAISERROR ('Executing command ''%s''.', 0, 1, @Command) WITH NOWAIT;
                            EXEC (@Command);

                            SET @StatsRowIndex += 1;
                        END

                    RAISERROR ('Completed optimising indexes for table %s.', 0, 1, @ObjectName) WITH NOWAIT;

                    -- Update all statistics on the table
                    SET @Command = 'UPDATE STATISTICS ' + @ObjectName;
                    RAISERROR ('Updating statistics on table %s.', 0, 1, @ObjectName) WITH NOWAIT;
                    EXEC (@Command);

                    SET @TableRowIndex += 1;
                END
        END
END
