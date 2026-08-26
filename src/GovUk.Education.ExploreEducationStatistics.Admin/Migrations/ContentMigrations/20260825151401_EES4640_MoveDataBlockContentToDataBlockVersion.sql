-- Move the content of each DataBlock out of the ContentBlock table and into its DataBlockVersion. This is done for
-- every DataBlock, whether or not it is placed in a ContentSection.
UPDATE dbv
SET dbv.Heading  = cb.DataBlock_Heading,
    dbv.[Name]   = cb.[Name],
    dbv.[Source] = cb.[Source],
    dbv.Query    = cb.DataBlock_Query,
    dbv.Charts   = cb.DataBlock_Charts,
    dbv.[Table]  = cb.DataBlock_Table
FROM DataBlockVersions dbv
JOIN ContentBlock cb ON cb.Id = dbv.ContentBlockId
WHERE cb.[Type] = 'DataBlock';

-- Populate the new ContentBlock.DataBlockVersionId, pointing each "DataBlock" ContentBlock at the
-- DataBlockVersion that references it.
--
-- A "DataBlockVersionLink" ContentBlock only exists for as long as its DataBlockVersion is placed in a
-- ContentSection, so only DataBlocks with a ContentSectionId become links. The ContentBlocks of the remaining
-- (unattached) DataBlocks are deleted at the end of the migration, once the foreign keys into ContentBlock have
-- been dropped and re-pointed.
UPDATE cb
SET cb.DataBlockVersionId = dbv.Id
FROM ContentBlock cb
JOIN DataBlockVersions dbv ON dbv.ContentBlockId = cb.Id
WHERE cb.[Type] = 'DataBlock'
  AND cb.ContentSectionId IS NOT NULL;

-- Flip the table-per-hierarchy discriminator so these ContentBlocks are now "DataBlockVersionLink"s.
UPDATE ContentBlock
SET [Type] = 'DataBlockVersionLink'
WHERE [Type] = 'DataBlock'
  AND ContentSectionId IS NOT NULL;

-- Re-point the FeaturedTables and KeyStatisticsDataBlock foreign keys, as the meaning of both of their columns
-- changes in this migration:
--
--   * DataBlockId used to reference the ContentBlock of type "DataBlock" (i.e. a specific version of a DataBlock),
--     and now references the parent DataBlock instead. Its new value is therefore the old DataBlockParentId.
--   * DataBlockVersionId replaces DataBlockParentId, and references the DataBlockVersion of that ContentBlock. Its
--     value is found via DataBlockVersions.ContentBlockId, which maps one-to-one onto the ContentBlock that the old
--     DataBlockId pointed at.
--
-- Note that these values cannot simply be swapped over by renaming DataBlockParentId to DataBlockVersionId. A
-- DataBlock (parent) and its DataBlockVersion only share an Id for the historical rows seeded by migration
-- 20231004144344_EES4467_AddDataBlockAndDataBlockVersion; every DataBlockVersion created since then has an Id of
-- its own.
--
-- DataBlockVersionId must be populated before DataBlockId is overwritten, as the join relies on the old
-- DataBlockId value.
UPDATE ft
SET ft.DataBlockVersionId = dbv.Id
FROM FeaturedTables ft
JOIN DataBlockVersions dbv ON dbv.ContentBlockId = ft.DataBlockId;

UPDATE FeaturedTables
SET DataBlockId = DataBlockParentId;

UPDATE ks
SET ks.DataBlockVersionId = dbv.Id
FROM KeyStatisticsDataBlock ks
JOIN DataBlockVersions dbv ON dbv.ContentBlockId = ks.DataBlockId;

UPDATE KeyStatisticsDataBlock
SET DataBlockId = DataBlockParentId;
