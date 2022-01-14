CREATE OR ALTER PROCEDURE DropAndCreateRelease @Release dbo.ReleaseType READONLY
AS
BEGIN
    DECLARE @ReleaseId AS UNIQUEIDENTIFIER;
    SET @ReleaseId = (SELECT R.Id FROM @Release R);

    IF @ReleaseId IN (SELECT Id FROM dbo.Release R WHERE R.Id = @ReleaseId)
        BEGIN
            DECLARE @FootnoteIds TABLE (Id UNIQUEIDENTIFIER PRIMARY KEY NOT NULL);
            INSERT INTO @FootnoteIds (Id) SELECT RF.FootnoteId FROM dbo.ReleaseFootnote RF WHERE ReleaseId = @ReleaseId

            DELETE dbo.FilterFootnote FROM dbo.FilterFootnote FF JOIN @FootnoteIds F ON F.Id = FF.FootnoteId;
            DELETE dbo.FilterGroupFootnote FROM dbo.FilterGroupFootnote FGF JOIN @FootnoteIds F ON F.Id = FGF.FootnoteId;
            DELETE dbo.FilterItemFootnote FROM dbo.FilterItemFootnote FIF JOIN @FootnoteIds F ON F.Id = FIF.FootnoteId;
            DELETE dbo.IndicatorFootnote FROM dbo.IndicatorFootnote INF JOIN @FootnoteIds F ON F.Id = INF.FootnoteId;
            DELETE dbo.SubjectFootnote FROM dbo.SubjectFootnote SF JOIN @FootnoteIds F ON F.Id = SF.FootnoteId;

            DELETE FROM dbo.ReleaseFootnote WHERE dbo.ReleaseFootnote.ReleaseId = @ReleaseId;

            DELETE FROM dbo.Footnote WHERE Id IN (SELECT F.Id FROM @FootnoteIds F);

            DECLARE @SubjectIds TABLE (Id UNIQUEIDENTIFIER PRIMARY KEY NOT NULL);
            INSERT INTO @SubjectIds (Id) SELECT RS.SubjectId FROM dbo.ReleaseSubject RS WHERE ReleaseId = @ReleaseId

            DELETE FROM dbo.ReleaseSubject WHERE dbo.ReleaseSubject.ReleaseId = @ReleaseId
            DELETE FROM dbo.Subject WHERE dbo.Subject.Id IN (SELECT S.Id FROM @SubjectIds S)
                                      AND dbo.Subject.Id NOT IN (Select RS.SubjectId FROM dbo.ReleaseSubject RS);

            DELETE FROM dbo.Release WHERE dbo.Release.Id = @ReleaseId;
        END

    INSERT INTO dbo.Release (Id, TimeIdentifier, Slug, Year, PublicationId, PreviousVersionId)
    SELECT Id, TimeIdentifier, Slug, Year, PublicationId, PreviousVersionId FROM @Release;
END
