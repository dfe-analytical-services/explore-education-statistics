CREATE PROCEDURE UpsertIndicatorGroup @IndicatorGroup dbo.IndicatorGroupType READONLY
AS
BEGIN
    MERGE dbo.IndicatorGroup  AS target
    USING @IndicatorGroup  AS source
    ON (target.Id = source.Id)
    WHEN MATCHED THEN
        UPDATE SET Label = source.Label,
                   SubjectId = source.SubjectId

    WHEN NOT MATCHED THEN
        INSERT (Id, Label, SubjectId)
        VALUES (source.Id, source.Label, source.SubjectId);
END
GO