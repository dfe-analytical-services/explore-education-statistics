CREATE OR ALTER PROCEDURE UpsertTopic @Topic dbo.TopicType READONLY
AS
BEGIN
    MERGE dbo.Topic AS target
    USING @Topic AS source
    ON (target.Id = source.Id)
    WHEN MATCHED THEN
        UPDATE SET Title = source.Title,
                   Slug = source.Slug
    WHEN NOT MATCHED THEN
        INSERT (Id, Title, Slug, ThemeId)
        VALUES (source.Id, source.Title, source.Slug, source.ThemeId);
END
