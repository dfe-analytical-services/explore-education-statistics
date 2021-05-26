CREATE OR ALTER PROCEDURE UpsertPublication @Publication dbo.PublicationType READONLY
AS
BEGIN
    MERGE dbo.Publication AS target
    USING @Publication AS source
    ON (target.Id = source.Id)
    WHEN MATCHED THEN
        UPDATE SET Title = source.Title,
                   Slug = source.Slug

    WHEN NOT MATCHED THEN
        INSERT (Id, Title, Slug, TopicId)
        VALUES (source.Id, source.Title, source.Slug, source.TopicId);
END
