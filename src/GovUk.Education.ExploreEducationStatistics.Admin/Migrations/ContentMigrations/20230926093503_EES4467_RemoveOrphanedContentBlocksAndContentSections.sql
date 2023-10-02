-- Delete orphaned Data Blocks
DELETE FROM ContentBlock
WHERE ContentBlock.[Type] = 'DataBlock'
AND NOT EXISTS (
    SELECT 1
    FROM Publications
    JOIN Releases ON Releases.PublicationId = Publications.Id
    JOIN ReleaseContentBlocks ON ReleaseContentBlocks.ReleaseId = Releases.Id
    JOIN ContentBlock cb ON cb.Id = ReleaseContentBlocks.ContentBlockId
    AND cb.Id = ContentBlock.Id
);

-- Delete Content Blocks that are linked to orphaned Content Sections. These Content Blocks have links to existing
-- Content Sections but those Content Sections themselves are orphaned by virtue of having no ReleaseContentSections
-- links.
DELETE FROM ContentBlock
WHERE EXISTS (
    SELECT 1
    FROM ContentSections
    WHERE ContentSections.Id = ContentBlock.ContentSectionId
)
AND NOT EXISTS (
    SELECT 1
    FROM ContentSections
    JOIN ReleaseContentSections ON ReleaseContentSections.ContentSectionId = ContentSections.Id
    WHERE ReleaseContentSections.ContentSectionId = ContentSections.Id
    AND ContentSections.Id = ContentBlock.ContentSectionId
);

-- Delete orphaned Content Sections
DELETE FROM ContentSections
WHERE NOT EXISTS (
    SELECT 1
    FROM ReleaseContentSections
    WHERE ReleaseContentSections.ContentSectionId = ContentSections.Id
);