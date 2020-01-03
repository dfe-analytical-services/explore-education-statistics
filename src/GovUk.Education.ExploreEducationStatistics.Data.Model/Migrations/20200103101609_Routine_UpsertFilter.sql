CREATE PROCEDURE UpsertFilter @Filter dbo.FilterType READONLY
AS
BEGIN
    MERGE dbo.Filter AS target
    USING @Filter AS source
    ON (target.Id = source.Id)
    WHEN MATCHED THEN
        UPDATE SET Hint = source.Hint,
                   Label = source.Label,
                   Name = source.Name,
                   SubjectId = source.SubjectId

    WHEN NOT MATCHED THEN
        INSERT (Id, Hint, Label, Name, SubjectId)
        VALUES (source.Id, source.Hint, source.Label, source.Name, source.SubjectId);
END
GO