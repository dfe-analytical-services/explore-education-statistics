-- Pupil absence in schools in England 2016/17
DECLARE @absence_release uniqueidentifier = '4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5';
-- Permanent and fixed-period exclusions in England 2016/17
DECLARE @exclusions_release uniqueidentifier = 'e7774a74-1f62-4b76-b9b5-84f14dac7278';

-- Delete any existing data for the declared releases
DELETE FROM SubjectFootnote WHERE FootnoteId IN (SELECT Id FROM Footnote F WHERE F.ReleaseId IN (@absence_release, @exclusions_release));
DELETE FROM IndicatorFootnote WHERE FootnoteId IN (SELECT Id FROM Footnote F WHERE F.ReleaseId IN (@absence_release, @exclusions_release));
DELETE FROM FilterItemFootnote WHERE FootnoteId IN (SELECT Id FROM Footnote F WHERE F.ReleaseId IN (@absence_release, @exclusions_release));
DELETE FROM FilterGroupFootnote WHERE FootnoteId IN (SELECT Id FROM Footnote F WHERE F.ReleaseId IN (@absence_release, @exclusions_release));
DELETE FROM FilterFootnote WHERE FootnoteId IN (SELECT Id FROM Footnote F WHERE F.ReleaseId IN (@absence_release, @exclusions_release));
DELETE FROM Footnote WHERE ReleaseId IN (@absence_release, @exclusions_release);