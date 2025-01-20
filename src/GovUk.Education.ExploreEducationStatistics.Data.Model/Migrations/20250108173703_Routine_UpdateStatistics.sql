CREATE OR ALTER PROCEDURE UpdateStatistics
    @ModifiedTables ModifiedTablesType READONLY
AS
BEGIN
    DECLARE @TablesRowIndex INT = 1,
            @TablesCount INT,
            @CurrentTable NVARCHAR(MAX),
            @Command NVARCHAR(MAX);

    SELECT @TablesCount = COUNT(Id) FROM @ModifiedTables;

    WHILE @TablesRowIndex <= @TablesCount
    BEGIN
        SELECT @CurrentTable = TableName
        FROM @ModifiedTables
        WHERE Id = @TablesRowIndex;

        BEGIN
            RAISERROR ('Updating statistics on table ''%s''.', 0, 1, @CurrentTable) WITH NOWAIT;
            SET @Command = 'UPDATE STATISTICS ' + @CurrentTable;
            EXEC (@Command);
        END

        SET @TablesRowIndex += 1;
    END

    RAISERROR ('Completed updating statistics on tables.', 0, 1, NULL) WITH NOWAIT;
END
GO
