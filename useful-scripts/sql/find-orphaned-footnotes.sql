SELECT *
FROM Footnote
WHERE Id NOT IN (
    SELECT FootnoteId
    FROM SubjectFootnote
    UNION
    SELECT FootnoteId
    FROM FilterFootnote
    UNION
    SELECT FootnoteId
    FROM FilterGroupFootnote UNION
SELECT FootnoteId
FROM FilterItemFootnote
UNION
SELECT FootnoteId
FROM IndicatorFootnote);
