-- Update to reflect the new columns of Release
CREATE PROCEDURE DropAndCreateRelease @Release dbo.ReleaseType READONLY
AS
BEGIN
    DELETE FROM dbo.Release WHERE dbo.Release.Id IN (SELECT R.Id FROM @Release R);

    INSERT INTO dbo.Release (Id, TimeIdentifier, ReleaseDate, Slug, Year, PublicationId)
    SELECT Id, TimeIdentifier, ReleaseDate, Slug, Year, PublicationId FROM @Release;
END
GO