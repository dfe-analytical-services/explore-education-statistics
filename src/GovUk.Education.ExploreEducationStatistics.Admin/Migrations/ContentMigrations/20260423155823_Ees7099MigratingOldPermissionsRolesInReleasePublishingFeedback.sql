-- Migrate all existing UserPublicationRole entries in the ReleasePublishingFeedback table
-- from the OLD permissions system roles to the corresponding NEW ones:
-- - `Allower` to `Approver`
-- - `Owner` to `Drafter` 
UPDATE ReleasePublishingFeedback
SET UserPublicationRole = 'Approver'
WHERE UserPublicationRole = 'Allower';

UPDATE ReleasePublishingFeedback
SET UserPublicationRole = 'Drafter'
WHERE UserPublicationRole = 'Owner';