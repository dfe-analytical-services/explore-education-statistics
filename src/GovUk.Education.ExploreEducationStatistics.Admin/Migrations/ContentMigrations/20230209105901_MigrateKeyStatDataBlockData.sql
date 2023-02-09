INSERT INTO [dbo].[KeyStatistics] (Id, ReleaseId, Trend, GuidanceTitle, GuidanceText, [Order], Created, Updated, CreatedById, UpdatedById, ContentBlockIdTemp)
SELECT NEWID() AS Id,
       RCS.ReleaseId AS ReleaseId,
       JSON_VALUE(DataBlock_Summary, '$.DataSummary[0]') AS Trend,
       JSON_VALUE(DataBlock_Summary, '$.DataDefinitionTitle[0]') AS GuidanceTitle,
       JSON_VALUE(DataBlock_Summary, '$.DataDefinition[0]') AS GuidanceText,
       CB.[Order],
       GETDATE() AS Created,
       NULL AS Updated,
       NULL AS CreatedById,
       NULL AS UpdatedById,
       CB.[Id] as ContentBlockIdTemp
FROM [dbo].[ContentBlock] CB
JOIN [dbo].[ContentSections] CS ON CS.Id = CB.ContentSectionId
JOIN [dbo].[ReleaseContentSections] RCS ON CS.Id = RCS.ContentSectionId
WHERE CS.Type = 'KeyStatistics';

-- NOTE: use temporary KeyStatistics.ContentBlockId column to simplify and derisk this INSERT
INSERT INTO [dbo].[KeyStatisticsDataBlock] (Id, DataBlockId)
SELECT KS.Id AS Id,
       KS.ContentBlockIdTemp AS DataBlockId
FROM [dbo].[KeyStatistics] KS;
