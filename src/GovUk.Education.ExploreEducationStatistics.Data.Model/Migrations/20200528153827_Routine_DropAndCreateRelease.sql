CREATE OR ALTER PROCEDURE DropAndCreateRelease @Release dbo.ReleaseType READONLY
AS
BEGIN
    DELETE FROM dbo.ReleaseSubject WHERE dbo.ReleaseSubject.ReleaseId IN (SELECT R.Id FROM @Release R);
    DELETE FROM dbo.ReleaseFootnote WHERE dbo.ReleaseFootnote.ReleaseId IN (SELECT R.Id FROM @Release R);
    DELETE FROM dbo.Release WHERE dbo.Release.Id IN (SELECT R.Id FROM @Release R);

    INSERT INTO dbo.Release (Id, TimeIdentifier, Slug, Year, PublicationId)
    SELECT Id, TimeIdentifier, Slug, Year, PublicationId FROM @Release;
END
GO