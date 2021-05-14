CREATE OR ALTER PROCEDURE DropAndCreateRelease @Release dbo.ReleaseType READONLY, @LinkedSubjects varchar(4000), @LinkedFootnotes varchar(4000)
AS
BEGIN
    DECLARE @LinkedSubjectsList IdListGuidType,
        @LinkedFootnotesList IdListGuidType;

    IF LEN(ISNULL(@LinkedSubjects, '')) > 0
        INSERT @LinkedSubjectsList SELECT value FROM STRING_SPLIT(@LinkedSubjects,',');

    IF LEN(ISNULL(@LinkedFootnotes, '')) > 0
        INSERT @LinkedFootnotesList SELECT value FROM STRING_SPLIT(@LinkedFootnotes,',');

    DELETE FROM dbo.ReleaseFootnote WHERE dbo.ReleaseFootnote.ReleaseId IN (SELECT R.Id FROM @Release R);
    DELETE FROM dbo.ReleaseSubject WHERE dbo.ReleaseSubject.ReleaseId IN (SELECT R.Id FROM @Release R);
    DELETE FROM dbo.Release WHERE dbo.Release.Id IN (SELECT R.Id FROM @Release R);

    DELETE FROM dbo.Subject WHERE dbo.Subject.Id NOT IN (SELECT id FROM @LinkedSubjectsList) AND NOT EXISTS (SELECT * FROM dbo.ReleaseSubject WHERE dbo.ReleaseSubject.SubjectId = dbo.Subject.Id) AND SoftDeleted != 1;

    DELETE FROM dbo.Footnote WHERE dbo.Footnote.Id NOT IN (SELECT id FROM @LinkedFootnotesList) AND NOT EXISTS (SELECT * FROM dbo.ReleaseFootnote WHERE dbo.ReleaseFootnote.FootnoteId = dbo.Footnote.Id)
                               AND NOT EXISTS (SELECT * FROM dbo.FilterFootnote WHERE dbo.FilterFootnote.FootnoteId = dbo.Footnote.Id)
                               AND NOT EXISTS (SELECT * FROM dbo.FilterItemFootnote WHERE dbo.FilterItemFootnote.FootnoteId = dbo.Footnote.Id)
                               AND NOT EXISTS (SELECT * FROM dbo.FilterGroupFootnote WHERE dbo.FilterGroupFootnote.FootnoteId = dbo.Footnote.Id)
                               AND NOT EXISTS (SELECT * FROM dbo.IndicatorFootnote WHERE dbo.IndicatorFootnote.FootnoteId = dbo.Footnote.Id)
                               AND NOT EXISTS (SELECT * FROM dbo.SubjectFootnote WHERE dbo.SubjectFootnote.FootnoteId = dbo.Footnote.Id);

    INSERT INTO dbo.Release (Id, TimeIdentifier, Slug, Year, PublicationId, PreviousVersionId)
    SELECT Id, TimeIdentifier, Slug, Year, PublicationId, PreviousVersionId FROM @Release;
END
