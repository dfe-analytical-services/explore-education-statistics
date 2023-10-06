-- Delete directly orphaned Data Blocks. These are Data Blocks that have no ReleaseContentBlocks link entry back to
-- their owning Release. Any further Data Blocks that are used in Release Content but are orphaned will be cleaned up
-- by subsequent delete statements further down in this file.
--
-- We can do this with safety because in valid data, there should always be a link in the ReleaseContentBlocks table in
-- order to use a Data Block correctly for a Key Stat, a Featured Table or to embed in Release Content.
DELETE FROM ContentBlock
WHERE ContentBlock.[Type] = 'DataBlock'
AND NOT EXISTS (
    SELECT 1
    FROM ReleaseContentBlocks
    WHERE ReleaseContentBlocks.ContentBlockId = ContentBlock.Id
);

-- Delete Content Blocks that are linked to orphaned Content Sections. These Content Blocks have links to existing
-- Content Sections but those Content Sections themselves are orphaned by virtue of having no ReleaseContentSections
-- links.
--
-- We can do this with safety because in valid data, a Content Block of any type that belongs to a Content Section can
-- only exist in Release Content if the Content Section that it belongs to is itself correctly linked to a Release. If
-- there is no link between a Content Block's Section and a Release, it can't be used to display any Release Content.
DELETE FROM ContentBlock
-- The ContentBlock is attached to a ContentSection.
WHERE EXISTS (
    SELECT 1
    FROM ContentSections
    WHERE ContentSections.Id = ContentBlock.ContentSectionId
)
-- But the ContentSection itself has no line back to the owning Release via the ReleaseContentSections table.
AND NOT EXISTS (
    SELECT 1
    FROM ReleaseContentSections
    WHERE ReleaseContentSections.ContentSectionId = ContentBlock.ContentSectionId
);

-- Delete orphaned Content Sections. These are Content Sections that have no link back to their owning Releases via
-- the ReleaseContentSections table.
--
-- We can do this safely because in valid data, only Content Sections that are linked to a Release can actually be used
-- to display content for that Release.
DELETE FROM ContentSections
WHERE NOT EXISTS (
    SELECT 1
    FROM ReleaseContentSections
    WHERE ReleaseContentSections.ContentSectionId = ContentSections.Id
);