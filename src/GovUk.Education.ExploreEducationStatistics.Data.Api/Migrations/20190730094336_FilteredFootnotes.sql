CREATE OR ALTER PROCEDURE FilteredFootnotes @indicatorList IdListIntegerType READONLY,
                                            @filterItemList IdListIntegerType READONLY
AS

DECLARE @filterGroupList TABLE (Id BIGINT);
DECLARE @filterList TABLE (Id BIGINT);

INSERT INTO @filterGroupList (Id)
SELECT FI.FilterGroupId
FROM FilterItem FI
WHERE FI.Id IN (SELECT id FROM @filterItemList);

INSERT INTO @filterList (Id)
SELECT FG.FilterId
FROM FilterGroup FG
WHERE FG.Id IN (SELECT id FROM @filterGroupList);

SELECT DISTINCT F.Id              AS Id
              --I.IndicatorId,
              --FF.FilterId       AS FilterId,
              --FGF.FilterGroupId AS FilterGroupId,
              --FIF.FilterItemId  AS FilterItemId
FROM Footnote F
         LEFT JOIN IndicatorFootnote I ON F.Id = I.FootnoteId
         LEFT JOIN FilterFootnote FF on F.Id = FF.FootnoteId
         LEFT JOIN FilterGroupFootnote FGF on F.Id = FGF.FootnoteId
         LEFT JOIN FilterItemFootnote FIF on F.Id = FIF.FootnoteId
WHERE I.IndicatorId IN (SELECT id FROM @indicatorList)
  AND (
        (FF.FilterId IN (SELECT id FROM @filterList) OR FF.FilterId IS NULL) AND
        (FGF.FilterGroupId IN (SELECT id FROM @filterGroupList) OR FGF.FilterGroupId IS NULL) AND
        (FIF.FilterItemId IN (SELECT id FROM @filterItemList) OR FIF.FilterItemId IS NULL)
      )
ORDER BY (F.Id);