CREATE PROCEDURE UpsertSubjectFootnote @SubjectFootnote dbo.SubjectFootnoteType READONLY
AS
BEGIN
    MERGE dbo.SubjectFootnote AS target
    USING @SubjectFootnote AS source
    ON (target.SubjectId = source.SubjectId AND target.FootnoteId = source.FootnoteId)

    WHEN NOT MATCHED THEN
        INSERT (SubjectId, FootnoteId)
        VALUES (source.SubjectId, source.FootnoteId);
END
GO