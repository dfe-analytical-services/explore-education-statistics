BEGIN TRANSACTION;

DECLARE @NewPageId UNIQUEIDENTIFIER = NEWID();
DECLARE @NewVersionId UNIQUEIDENTIFIER = NEWID();
DECLARE @UserId UNIQUEIDENTIFIER = 'b99e8358-9a5e-4a3a-9288-6f94c7e1e3dd'; -- bau1
DECLARE @Now DATETIMEOFFSET = SYSDATETIMEOFFSET();

INSERT INTO EinPages (Id, [Title], [Description], [Order], LatestVersionId, LatestPublishedVersionId)
VALUES (@NewPageId, 'Education in numbers', 'Education statistics highlights', 0, NULL, NULL);

INSERT INTO EinPageVersions (Id, [Version], Published, Created, CreatedById, EinPageId)
VALUES (@NewVersionId, 0, NULL, @Now, @UserId, @NewPageId);

UPDATE EinPages
SET LatestVersionId = @NewVersionId
WHERE Id = @NewPageId;

COMMIT TRANSACTION;
