CREATE PROCEDURE UpsertSubject @Subject dbo.SubjectType READONLY
AS
BEGIN
    MERGE dbo.Subject  AS target
    USING @Subject  AS source
    ON (target.Id = source.Id)
    WHEN MATCHED THEN
        UPDATE SET Name = source.Name,
                   ReleaseId = source.ReleaseId

    WHEN NOT MATCHED THEN
        INSERT (Id, Name, ReleaseId)
        VALUES (source.Id, source.Name, source.ReleaseId);
END
GO