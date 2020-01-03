CREATE PROCEDURE UpsertRelease @Release dbo.ReleaseType READONLY
AS
BEGIN
    MERGE dbo.Release AS target
    USING @Release AS source
    ON (target.Id = source.Id)
    WHEN MATCHED THEN
        UPDATE SET Title = source.Title,
                   ReleaseDate = source.ReleaseDate,
                   Slug = source.Slug

    WHEN NOT MATCHED THEN
        INSERT (Id, Title, ReleaseDate, Slug, PublicationId)
        VALUES (source.Id, source.Title, source.ReleaseDate, source.Slug, source.PublicationId);
END
GO