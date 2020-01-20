CREATE PROCEDURE UpsertFilterGroupFootnote @FilterGroupFootnote dbo.FilterGroupFootnoteType READONLY
AS
BEGIN
    MERGE dbo.FilterGroupFootnote AS target
    USING @FilterGroupFootnote AS source
    ON (target.FilterGroupId = source.FilterGroupId AND target.FootnoteId = source.FootnoteId)

    WHEN NOT MATCHED THEN
        INSERT (FilterGroupId, FootnoteId)
        VALUES (source.FilterGroupId, source.FootnoteId);
END
GO