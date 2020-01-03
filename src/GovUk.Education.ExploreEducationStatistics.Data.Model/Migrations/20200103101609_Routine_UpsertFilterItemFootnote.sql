CREATE PROCEDURE UpsertFilterItemFootnote @FilterItemFootnote dbo.FilterItemFootnoteType READONLY
AS
BEGIN
    MERGE dbo.FilterItemFootnote AS target
    USING @FilterItemFootnote AS source
    ON (target.FilterItemId = source.FilterItemId AND target.FootnoteId = source.FootnoteId)

    WHEN NOT MATCHED THEN
        INSERT (FilterItemId, FootnoteId)
        VALUES (source.FilterItemId, source.FootnoteId);
END
GO