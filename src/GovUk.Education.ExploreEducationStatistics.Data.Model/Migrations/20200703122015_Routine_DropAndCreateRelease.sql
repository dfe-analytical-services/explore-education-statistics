CREATE OR ALTER PROCEDURE DropAndCreateRelease @Release dbo.ReleaseType READONLY
AS
BEGIN
    DELETE FROM dbo.ReleaseFootnote WHERE dbo.ReleaseFootnote.ReleaseId IN (SELECT R.Id FROM @Release R);
    DELETE FROM dbo.ReleaseSubject WHERE dbo.ReleaseSubject.ReleaseId IN (SELECT R.Id FROM @Release R);
    DELETE FROM dbo.Release WHERE dbo.Release.Id IN (SELECT R.Id FROM @Release R);
    
    DELETE FROM dbo.Subject WHERE NOT EXISTS (SELECT * FROM dbo.ReleaseSubject WHERE dbo.ReleaseSubject.SubjectId = dbo.Subject.Id);

    DELETE FROM dbo.Footnote WHERE NOT EXISTS (SELECT * FROM dbo.ReleaseFootnote WHERE dbo.ReleaseFootnote.FootnoteId = dbo.Footnote.Id)
    AND NOT EXISTS (SELECT * FROM dbo.FilterFootnote WHERE dbo.FilterFootnote.FootnoteId = dbo.Footnote.Id)
    AND NOT EXISTS (SELECT * FROM dbo.FilterItemFootnote WHERE dbo.FilterItemFootnote.FootnoteId = dbo.Footnote.Id)
    AND NOT EXISTS (SELECT * FROM dbo.FilterGroupFootnote WHERE dbo.FilterGroupFootnote.FootnoteId = dbo.Footnote.Id)
    AND NOT EXISTS (SELECT * FROM dbo.IndicatorFootnote WHERE dbo.IndicatorFootnote.FootnoteId = dbo.Footnote.Id)
    AND NOT EXISTS (SELECT * FROM dbo.SubjectFootnote WHERE dbo.SubjectFootnote.FootnoteId = dbo.Footnote.Id);

    INSERT INTO dbo.Release (Id, TimeIdentifier, Slug, Year, PublicationId)
    SELECT Id, TimeIdentifier, Slug, Year, PublicationId FROM @Release;
END