CREATE OR ALTER PROCEDURE DropAndCreateRelease @Release dbo.ReleaseType READONLY, @LinkedSubjects varchar(4000), @LinkedFootnotes varchar(4000)
AS
BEGIN
    DECLARE @LinkedSubjectsList IdListGuidType,
        @LinkedFootnotesList IdListGuidType;

    IF LEN(ISNULL(@LinkedSubjects, '')) > 0
        INSERT @LinkedSubjectsList SELECT value FROM STRING_SPLIT(@LinkedSubjects,',');

    IF LEN(ISNULL(@LinkedFootnotes, '')) > 0
        INSERT @LinkedFootnotesList SELECT value FROM STRING_SPLIT(@LinkedFootnotes,',');

    DECLARE @FootnoteIds TABLE (Id UNIQUEIDENTIFIER PRIMARY KEY NOT NULL);
    INSERT INTO @FootnoteIds (Id) SELECT RF.FootnoteId FROM dbo.ReleaseFootnote RF WHERE ReleaseId = (SELECT R.Id FROM @Release R);

    DELETE FROM dbo.SubjectFootnote WHERE FootnoteId IN (SELECT F.Id FROM @FootnoteIds F);
    DELETE FROM dbo.FilterFootnote WHERE FootnoteId IN (SELECT F.Id FROM @FootnoteIds F);
    DELETE FROM dbo.FilterGroupFootnote WHERE FootnoteId IN (SELECT F.Id FROM @FootnoteIds F);
    DELETE FROM dbo.FilterItemFootnote WHERE FootnoteId IN (SELECT F.Id FROM @FootnoteIds F);
    DELETE FROM dbo.IndicatorFootnote WHERE FootnoteId IN (SELECT F.Id FROM @FootnoteIds F);
    DELETE FROM dbo.SubjectFootnote WHERE FootnoteId IN (SELECT F.Id FROM @FootnoteIds F);

    DELETE FROM dbo.ReleaseFootnote WHERE dbo.ReleaseFootnote.ReleaseId IN (SELECT R.Id FROM @Release R);

    DELETE FROM dbo.Footnote WHERE Id IN (SELECT F.Id FROM @FootnoteIds F);

    DELETE FROM dbo.ReleaseSubject WHERE dbo.ReleaseSubject.ReleaseId IN (SELECT R.Id FROM @Release R);
    DELETE FROM dbo.Release WHERE dbo.Release.Id IN (SELECT R.Id FROM @Release R);

    DELETE FROM dbo.Subject WHERE dbo.Subject.Id NOT IN (SELECT id FROM @LinkedSubjectsList) AND NOT EXISTS (SELECT * FROM dbo.ReleaseSubject WHERE dbo.ReleaseSubject.SubjectId = dbo.Subject.Id) AND SoftDeleted != 1;

    INSERT INTO dbo.Release (Id, TimeIdentifier, Slug, Year, PublicationId, PreviousVersionId)
    SELECT Id, TimeIdentifier, Slug, Year, PublicationId, PreviousVersionId FROM @Release;
END
