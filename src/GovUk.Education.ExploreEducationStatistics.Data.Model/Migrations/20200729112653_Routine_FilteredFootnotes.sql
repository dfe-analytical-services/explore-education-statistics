CREATE OR ALTER PROCEDURE FilteredFootnotes @releaseId uniqueidentifier,
                                            @subjectId uniqueidentifier,
                                            @indicatorList IdListGuidType READONLY,
                                            @filterItemList IdListGuidType READONLY
AS

DECLARE @filterGroupList TABLE (Id uniqueidentifier);
DECLARE @filterList TABLE (Id uniqueidentifier);

INSERT INTO @filterGroupList (Id)
SELECT FI.FilterGroupId
FROM FilterItem FI
WHERE FI.Id IN (SELECT id FROM @filterItemList);

INSERT INTO @filterList (Id)
SELECT FG.FilterId
FROM FilterGroup FG
WHERE FG.Id IN (SELECT id FROM @filterGroupList);

SELECT DISTINCT FNOTE.Id,
                FNOTE.Content
FROM Footnote FNOTE
         INNER JOIN ReleaseFootnote RF
                    ON FNOTE.Id = RF.FootnoteId
         LEFT JOIN SubjectFootnote SF
                   ON FNOTE.Id = SF.FootnoteId AND SF.SubjectId = @subjectId
         LEFT JOIN (IndicatorFootnote IFOOT INNER JOIN Indicator I ON I.Id = IFOOT.IndicatorId INNER JOIN IndicatorGroup IG ON IG.Id = I.IndicatorGroupId AND IG.SubjectId = @subjectId)
                   ON FNOTE.Id = IFOOT.FootnoteId
         LEFT JOIN (FilterFootnote FF INNER JOIN FILTER F ON F.Id = FF.FilterId AND F.SubjectId = @subjectId)
                   ON FNOTE.Id = FF.FootnoteId
         LEFT JOIN (FilterItemFootnote FIF INNER JOIN FilterItem FI ON FI.Id = FIF.FilterItemId INNER JOIN FilterGroup FG on FI.FilterGroupId = FG.Id INNER JOIN Filter F2 ON F2.Id = FG.FilterId AND F2.SubjectId = @subjectId)
                   ON FNOTE.Id = FIF.FootnoteId
WHERE RF.ReleaseId = @releaseId
  AND (SF.SubjectId = @subjectId OR SF.SubjectId IS NULL)
  AND (IFOOT.IndicatorId IN (SELECT id FROM @indicatorList) OR IFOOT.IndicatorId IS NULL)
  AND ((FF.FilterId IN (SELECT id FROM @filterList))
    OR (FIF.FilterItemId IN (SELECT id FROM @filterItemList) OR (FF.FilterId IS NULL AND FIF.FilterItemId IS NULL)))
ORDER BY FNOTE.Id;