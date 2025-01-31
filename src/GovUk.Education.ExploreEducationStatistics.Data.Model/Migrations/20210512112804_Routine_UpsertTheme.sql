CREATE OR ALTER PROCEDURE UpsertTheme @Theme dbo.ThemeType READONLY
AS
BEGIN
    MERGE dbo.Theme AS target
    USING @Theme AS source
    ON (target.Id = source.Id)
    WHEN MATCHED THEN
        UPDATE SET Title = source.Title,
                   Slug = source.Slug
    WHEN NOT MATCHED THEN
        INSERT (Id, Title, Slug)
        VALUES (source.Id, source.Title, source.Slug);
END
