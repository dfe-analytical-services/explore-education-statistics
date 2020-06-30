INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId) 
SELECT Id, ReleaseId from Footnote AS f
WHERE NOT EXISTS(select * FROM ReleaseFootnote WHERE FootnoteId = f.Id AND ReleaseId = f.ReleaseId);