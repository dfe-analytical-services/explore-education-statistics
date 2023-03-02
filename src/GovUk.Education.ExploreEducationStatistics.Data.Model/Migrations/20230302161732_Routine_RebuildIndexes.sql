CREATE OR ALTER PROCEDURE RebuildIndexes @Tables VARCHAR(MAX)
AS
BEGIN
    DECLARE @TableList AS TABLE
        (RowID INT NOT NULL PRIMARY KEY IDENTITY(1, 1),
        ObjectName NVARCHAR(MAX));

    DECLARE @TableCount INT,
        @RowIndex INT = 1,
        @RebuildCommand NVARCHAR(MAX);

    IF LEN(ISNULL(@Tables, '')) > 0
        BEGIN
            INSERT @TableList SELECT TRIM(value) FROM STRING_SPLIT(@Tables, ',');
            SET @TableCount = @@ROWCOUNT;
            WHILE @RowIndex <= @TableCount
                BEGIN
                    SELECT @RebuildCommand = 'ALTER INDEX ALL ON dbo.' + ObjectName + ' REBUILD WITH (ONLINE = ON)'
                    FROM @TableList
                    WHERE RowID = @RowIndex;

                    EXEC (@RebuildCommand);

                    SET @RowIndex += 1;
                END
        END
END
