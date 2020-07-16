CREATE OR ALTER PROCEDURE DropAndCreateReleaseV2 @Release dbo.ReleaseType READONLY, @LinkedSubjects varchar(4000), @LinkedFootnotes varchar(4000)
AS
BEGIN
    DECLARE @LinkedSubjectsList IdListGuidType,
            @LinkedFootnotesList IdListGuidType,
            @sqlString1 NVARCHAR(2000) = N'INSERT @LinkedSubjectsList VALUES (' + @LinkedSubjects + ')',
            @sqlString2 NVARCHAR(2000) = N'INSERT @LinkedFootnotesList VALUES (' + @LinkedFootnotes + ')'
     
    EXEC sp_executesql @sqlString1, '@LinkedSubjectsList IdListGuidType READONLY', @LinkedSubjectsList = @LinkedSubjectsList;
    EXEC sp_executesql @sqlString2, '@LinkedFootnotesList IdListGuidType READONLY', @LinkedFootnotes = @LinkedFootnotes;   
    --INSERT @LinkedSubjectsList VALUES (@LinkedSubjects);
    --INSERT @LinkedFootnotesList VALUES (@LinkedFootnotes);
    
    DELETE FROM dbo.ReleaseFootnote WHERE dbo.ReleaseFootnote.ReleaseId IN (SELECT R.Id FROM @Release R);
    DELETE FROM dbo.ReleaseSubject WHERE dbo.ReleaseSubject.ReleaseId IN (SELECT R.Id FROM @Release R);
    DELETE FROM dbo.Release WHERE dbo.Release.Id IN (SELECT R.Id FROM @Release R);
    
    DELETE FROM dbo.Subject WHERE dbo.Subject.Id NOT IN (SELECT id FROM @LinkedSubjectsList) AND NOT EXISTS (SELECT * FROM dbo.ReleaseSubject WHERE dbo.ReleaseSubject.SubjectId = dbo.Subject.Id);
        
    DELETE FROM dbo.Footnote WHERE dbo.Footnote.Id NOT IN (SELECT id FROM @LinkedFootnotesList) AND NOT EXISTS (SELECT * FROM dbo.ReleaseFootnote WHERE dbo.ReleaseFootnote.FootnoteId = dbo.Footnote.Id)
    AND NOT EXISTS (SELECT * FROM dbo.FilterFootnote WHERE dbo.FilterFootnote.FootnoteId = dbo.Footnote.Id)
    AND NOT EXISTS (SELECT * FROM dbo.FilterItemFootnote WHERE dbo.FilterItemFootnote.FootnoteId = dbo.Footnote.Id)
    AND NOT EXISTS (SELECT * FROM dbo.FilterGroupFootnote WHERE dbo.FilterGroupFootnote.FootnoteId = dbo.Footnote.Id)
    AND NOT EXISTS (SELECT * FROM dbo.IndicatorFootnote WHERE dbo.IndicatorFootnote.FootnoteId = dbo.Footnote.Id)
    AND NOT EXISTS (SELECT * FROM dbo.SubjectFootnote WHERE dbo.SubjectFootnote.FootnoteId = dbo.Footnote.Id);
    
    INSERT INTO dbo.Release (Id, TimeIdentifier, Slug, Year, PublicationId, PreviousVersionId)
    SELECT Id, TimeIdentifier, Slug, Year, PublicationId, PreviousVersionId FROM @Release;
END