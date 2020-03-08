-- Update to reflect the new columns of Release
CREATE OR ALTER PROCEDURE DropAndCreateRelease @Release dbo.ReleaseType READONLY
AS
BEGIN
    DELETE FROM dbo.Release WHERE dbo.Release.Id IN (SELECT R.Id FROM @Release R);

    INSERT INTO dbo.Release (Id, TimeIdentifier, Slug, Year, PublicationId)
    SELECT Id, TimeIdentifier, Slug, Year, PublicationId FROM @Release;
END
GO