CREATE PROCEDURE UpsertIndicatorFootnote @IndicatorFootnote dbo.IndicatorFootnoteType READONLY
AS
BEGIN
    MERGE dbo.IndicatorFootnote AS target
    USING @IndicatorFootnote AS source
    ON (target.IndicatorId = source.IndicatorId AND target.FootnoteId = source.FootnoteId)

    WHEN NOT MATCHED THEN
        INSERT (IndicatorId, FootnoteId)
        VALUES (source.IndicatorId, source.FootnoteId);
END
GO