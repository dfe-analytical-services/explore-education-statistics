-- Add to each KeyStatisticDataBlock the Id of the DataBlockParent associated with the ContentBlock of type "DataBlock" that
-- the KeyStatisticDataBlock relates to.
--
-- This can be found via the DataBlockVersions table, which shares its primary key with its associated ContentBlock
-- of type "DataBlock".
UPDATE KeyStatisticDataBlock
SET DataBlockParentId = DataBlockVersions.DataBlockId
FROM KeyStatisticDataBlock
JOIN DataBlockVersions ON DataBlockVersions.Id = FeaturedTables.DataBlockId;