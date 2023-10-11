UPDATE ContentBlock
SET ReleaseId = ReleaseContentSections.ReleaseId
FROM ContentBlock
JOIN ReleaseContentSections ON ReleaseContentSections.ContentSectionId = ContentBlock.ContentSectionId;

UPDATE ContentBlock
SET ReleaseId = ReleaseContentBlocks.ReleaseId
FROM ContentBlock
JOIN ReleaseContentBlocks ON ReleaseContentBlocks.ContentBlockId = ContentBlock.Id;