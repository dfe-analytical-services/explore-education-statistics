UPDATE ContentBlock
SET ContentSectionId = NULL
WHERE Type = 'DataBlock'
  AND Id IN (SELECT DISTINCT DataBlockId FROM KeyStatisticsDataBlock)
  AND ContentSectionId IS NOT NULL
  AND ContentSectionId NOT IN (SELECT CS.Id FROM ContentSections CS WHERE CS.Type != 'KeyStatistics'); -- this condition for safety

DELETE FROM ContentSections WHERE Type = 'KeyStatistics'
  AND Id NOT IN (SELECT CB.ContentSectionId FROM ContentBlock CB); -- this condition added for safety
