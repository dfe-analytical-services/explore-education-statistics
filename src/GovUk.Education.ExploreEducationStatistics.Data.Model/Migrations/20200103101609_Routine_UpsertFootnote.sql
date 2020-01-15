CREATE PROCEDURE UpsertFootnote @Footnote dbo.FootnoteType READONLY
AS
BEGIN
    MERGE dbo.Footnote AS target
    USING @Footnote AS source
    ON (target.Id = source.Id)
    WHEN MATCHED THEN
        UPDATE SET Content = source.Content

    WHEN NOT MATCHED THEN
        INSERT (Id, Content)
        VALUES (source.Id, source.Content);
END
GO