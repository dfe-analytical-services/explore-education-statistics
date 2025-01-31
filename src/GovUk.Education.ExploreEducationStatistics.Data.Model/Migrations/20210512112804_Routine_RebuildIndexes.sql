CREATE OR ALTER PROCEDURE RebuildIndexes @Tables varchar(4000), @Percent INT
AS
BEGIN
    DECLARE @Frag_Temp AS Table
                          (
                              ID INT IDENTITY (1,1),
                              [objectid][INT] NULL,
                              [indexid][INT] NULL,
                              [frag] [FLOAT] NULL
                          );

    DECLARE @TableList AS TABLE (RowID INT NOT NULL PRIMARY KEY IDENTITY(1,1), Name NVARCHAR(max));

    DECLARE @FragCount INT,
        @RowsToProcess INT,
        @TableName NVARCHAR(max),
        @CurrentRow INT,
        @i TINYINT=1,
        @SchemaName SYSNAME,
        @ObjectName SYSNAME,
        @IndexName SYSNAME,
        @ObjectId INT,
        @IndexId INT,
        @SQLCommand as NVARCHAR(3000);

    IF LEN(ISNULL(@Tables, '')) > 0
        BEGIN
            INSERT @TableList SELECT value FROM STRING_SPLIT(@Tables,',');

            SET @RowsToProcess=@@ROWCOUNT;
            SET @CurrentRow=0;

            WHILE @CurrentRow<@RowsToProcess
                BEGIN
                    SET @CurrentRow=@CurrentRow+1;

                    SELECT @TableName=Name
                    FROM @TableList
                    WHERE RowID=@CurrentRow;

                    INSERT INTO @Frag_Temp
                    SELECT object_id AS ObjectId,
                           index_id AS IndexId,
                           avg_fragmentation_in_percent AS frag
                    FROM sys.dm_db_index_physical_stats(DB_ID(), OBJECT_ID(@TableName), NULL, NULL, 'LIMITED')
                    WHERE avg_fragmentation_in_percent >= @Percent
                      AND index_id > 0;

                    SELECT @FragCount = Count(*) FROM @Frag_Temp;

                    WHILE(@i <= @FragCount)
                        BEGIN
                            SELECT @ObjectId = objectid,
                                   @IndexId = indexid
                            FROM @Frag_Temp WHERE ID=@i;

                            --Get tableName and its schema
                            SELECT @ObjectName = o.name, @SchemaName = c.name
                            FROM sys.objects o
                                     INNER JOIN sys.schemas c ON o.schema_ID = c.schema_ID
                            WHERE o.object_id = @ObjectId;

                            --Get Index Name
                            SELECT @IndexName=name
                            FROM sys.indexes
                            WHERE index_id=@IndexId
                              AND object_id=@ObjectId;

                            SELECT @SQLCommand = 'ALTER INDEX ' + @IndexName + ' ON ' + @SchemaName + '.' + @ObjectName + ' REBUILD WITH (ONLINE = ON)';

                            EXEC (@SQLCommand);

                            SET @i = @i + 1;
                        END
                END
        END
END
