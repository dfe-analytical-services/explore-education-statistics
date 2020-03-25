CREATE PROCEDURE DropAndCreateRelease @Release dbo.ReleaseType READONLY
AS
BEGIN
    DELETE FROM dbo.Release WHERE dbo.Release.Id IN (SELECT R.Id FROM @Release R);

    INSERT INTO dbo.Release (Id, Title, ReleaseDate, Slug, PublicationId)
    SELECT * FROM @Release;
END
GO