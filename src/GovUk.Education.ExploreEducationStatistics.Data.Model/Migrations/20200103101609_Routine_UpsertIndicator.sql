CREATE PROCEDURE UpsertIndicator @Indicator dbo.IndicatorType READONLY
AS
BEGIN
    MERGE dbo.Indicator AS target
    USING @Indicator AS source
    ON (target.Id = source.Id)
    WHEN MATCHED THEN
        UPDATE SET Label = source.Label,
               Name = source.Name,
               Unit = source.Unit,
               IndicatorGroupId = source.IndicatorGroupId

    WHEN NOT MATCHED THEN
        INSERT (Id, Label, Name, Unit, IndicatorGroupId)
        VALUES (source.Id, source.Label, source.Name, source.Unit, source.IndicatorGroupId);
END
GO