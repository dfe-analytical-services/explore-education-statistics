UPDATE ContentSections
SET ReleaseId = ReleaseContentSections.ReleaseId
FROM ReleaseContentSections
WHERE ReleaseContentSections.ContentSectionId = ContentSections.Id;