INSERT INTO [dbo].[FeaturedTables] (Id, Name, Description,
                                    [Order], DataBlockId, ReleaseId,
                                    Created, Updated, CreatedById, UpdatedById)
SELECT NEWID() AS Id,
       CB.DataBlock_HighlightName AS Name,
       NULLIF(TRIM(CB.DataBlock_HighlightDescription), '') AS Description,
       0 AS [Order],
       CB.Id AS DataBlockId,
       RCB.ReleaseId AS ReleaseId,
       GETUTCDATE() AS Created,
       NULL AS Updated,
       NULL AS CreatedById,
       NULL AS UpdatedById
FROM [dbo].[ContentBlock] CB
JOIN [dbo].[ReleaseContentBlocks] RCB ON RCB.ContentBlockId = CB.Id
WHERE CB.Type = 'DataBlock'
AND (CB.DataBlock_HighlightName IS NOT NULL AND TRIM(CB.DataBlock_HighlightName) != '');

