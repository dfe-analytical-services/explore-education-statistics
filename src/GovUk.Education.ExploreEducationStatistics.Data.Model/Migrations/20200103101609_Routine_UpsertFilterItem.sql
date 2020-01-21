CREATE PROCEDURE UpsertFilterItem @FilterItem dbo.FilterItemType READONLY
AS
BEGIN
    MERGE dbo.FilterItem AS target
    USING @FilterItem AS source
    ON (target.Id = source.Id)
    WHEN MATCHED THEN
        UPDATE SET FilterGroupId = source.FilterGroupId,
                   Label = source.Label

    WHEN NOT MATCHED THEN
        INSERT (Id, FilterGroupId, Label)
        VALUES (source.Id, source.FilterGroupId, source.Label);
END
GO