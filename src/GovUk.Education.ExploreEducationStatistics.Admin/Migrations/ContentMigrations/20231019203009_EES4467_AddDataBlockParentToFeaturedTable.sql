-- Add to each FeaturedTable the Id of the DataBlockParent associated with the ContentBlock of type "DataBlock" that
-- the FeaturedTable relates to.
--
-- This can be found via the DataBlockVersions table, which shares its primary key with its associated ContentBlock
-- of type "DataBlock".
UPDATE FeaturedTables
SET DataBlockParentId = DataBlockVersions.DataBlockId
FROM FeaturedTables
JOIN DataBlockVersions ON DataBlockVersions.Id = FeaturedTables.DataBlockId;