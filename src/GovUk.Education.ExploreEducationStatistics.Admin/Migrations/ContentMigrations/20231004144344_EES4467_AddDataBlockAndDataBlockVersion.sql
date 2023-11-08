-- Add appropriate permissions to the new DataBlocks and DataBlockVersions table for the
-- contained database users that are in use by the Content API, Data API and Publisher Function.
GRANT SELECT ON DataBlocks TO [content];
GRANT SELECT ON DataBlockVersions TO [content];
GRANT SELECT ON DataBlocks TO [data];
GRANT SELECT ON DataBlockVersions TO [data];
GRANT SELECT ON DataBlocks TO [publisher];
GRANT SELECT ON DataBlockVersions TO [publisher];
GRANT UPDATE ON DataBlocks TO [publisher];
GRANT UPDATE ON DataBlockVersions TO [publisher];

-- As we're migrating historical data whereby we do not have the means to reliably determine a
-- version, we'll be treating each existing ContentBLock of type "DataBlock" as an individual
-- DataBlock (parent) and a single DataBlockVersion underneath it. This allows us to ensure that
-- all ContentBlocks of type "DataBlock" will *always* have at least one DataBlock (parent) and
-- individual DataBlockVersion that is linked to it.
--
-- Moving forwards from this point onwards, the real version tracking will be taking place.
--
-- The reason that we can't reliably determine a version history for a single DataBlock throughout
-- the life of a Release Amendment currently is that there are no pointers back to previous
-- versions of the ContentBlock that belong to previous Release Amendments. We could theoretically
-- judge that a particular ContentBlock of type "DataBlock" is the later version of an older one by
-- seeing if it's on the next Version of the same Release and it has exactly the same Heading or
-- Query, but this isn't guaranteed and is a very low value exercise.

-- This creates a single overarching DataBlock for each ContentBlock of type "DataBlock".
-- If the original ContentBlock that it's based on is published, we link it to the new
-- overarching DataBlock as its "LatestPublishedVersion". If it is still unpublished, we link
-- it to the parent as its "LatestDraftVersion".
INSERT INTO DataBlocks (Id, LatestDraftVersionId, LatestPublishedVersionId)
SELECT ContentBlock.Id AS Id,
    CASE
       WHEN Releases.Published IS NULL THEN ContentBlock.Id
       ELSE NULL
       END AS LatestDraftVersionId,
    CASE
       WHEN Releases.Published IS NOT NULL THEN ContentBlock.Id
       ELSE NULL
    END AS LatestPublishedVersionId
FROM ContentBlock
JOIN Releases ON ContentBlock.ReleaseId = Releases.Id
WHERE ContentBlock.[Type] = 'DataBlock';

-- This creates a single DataBlockVersion for each ContentBlock of type "DataBlock" and attaches it to
-- the single parent DataBlock entry created above.
INSERT INTO DataBlockVersions (
    Id,
    DataBlockId,
    ReleaseId,
    [Version],
    ContentBlockId,
    Created,
    Updated,
    Published)
SELECT
    ContentBlock.Id AS Id,
    ContentBlock.Id AS DataBlockId,
    Releases.Id AS ReleaseId,
    0 AS [Version],
    ContentBlock.Id AS ContentBlockId,
    COALESCE(ContentBlock.Created, Releases.Created) AS Created,
    ContentBlock.Updated AS Updated,
    Releases.Published AS Published
FROM ContentBlock
JOIN Releases ON ContentBlock.ReleaseId = Releases.Id
WHERE ContentBlock.[Type] = 'DataBlock';