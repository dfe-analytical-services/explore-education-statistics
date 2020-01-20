CREATE PROCEDURE UpsertFilterFootnote @FilterFootnote dbo.FilterFootnoteType READONLY
AS
BEGIN
    MERGE dbo.FilterFootnote AS target
    USING @FilterFootnote AS source
    ON (target.FilterId = source.FilterId AND target.FootnoteId = source.FootnoteId)

    WHEN NOT MATCHED THEN
        INSERT (FilterId, FootnoteId)
        VALUES (source.FilterId, source.FootnoteId);
END
GO