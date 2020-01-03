CREATE PROCEDURE UpsertFilterGroup @FilterGroup dbo.FilterGroupType READONLY
AS
BEGIN
    MERGE dbo.FilterGroup AS target
    USING @FilterGroup AS source
    ON (target.Id = source.Id)
    WHEN MATCHED THEN
        UPDATE SET FilterId = source.FilterId,
                   Label = source.Label

    WHEN NOT MATCHED THEN
        INSERT (Id, FilterId, Label)
        VALUES (source.Id, source.FilterId, source.Label);
END
GO