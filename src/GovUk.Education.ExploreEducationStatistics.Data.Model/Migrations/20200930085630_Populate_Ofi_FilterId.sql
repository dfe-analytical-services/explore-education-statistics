DECLARE @Rows INT,
        @BatchSize INT;

SET @BatchSize = 5000;
SET @Rows = @BatchSize;

BEGIN
    WHILE (@Rows = @BatchSize)
    BEGIN
        UPDATE TOP (@BatchSize) o
        SET o.FilterId = fg.FilterId
        FROM ObservationFilterItem o
        JOIN FilterItem fi on o.FilterItemId = fi.Id
        JOIN FilterGroup fg ON fi.FilterGroupId = fg.Id
        WHERE o.FilterId IS NULL;
        SET @Rows = @@ROWCOUNT;
        PRINT @BatchSize;
    END
END