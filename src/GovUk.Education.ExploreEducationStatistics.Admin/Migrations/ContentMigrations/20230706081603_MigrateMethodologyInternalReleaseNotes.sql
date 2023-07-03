INSERT INTO [dbo].[MethodologyStatus] (Id, MethodologyVersionId, InternalReleaseNote,
                                       ApprovalStatus, Created, CreatedById)
SELECT NEWID() AS Id,
       MV.Id AS MethodologyVersionId,
       MV.InternalReleaseNote AS InternalReleaseNote,
       MV.Status AS ApprovalStatus,
       MV.Published AS Created,
       NULL AS CreatedById
FROM [dbo].[MethodologyVersions] MV
WHERE MV.Published IS NOT NULL;

