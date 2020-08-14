-- Populate the new Published field setting it to the Published date of the most recent Release
UPDATE dbo.Publications
SET Published = (SELECT MAX(R.Published)
        FROM dbo.Releases R
        WHERE R.PublicationId = Publications.Id
        AND R.Published IS NOT NULL)
WHERE Publications.Published IS NULL