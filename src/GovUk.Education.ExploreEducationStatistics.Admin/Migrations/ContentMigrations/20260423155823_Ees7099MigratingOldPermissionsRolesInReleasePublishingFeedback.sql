-- Migrate all existing UserPublicationRole entries in the ReleasePublishingFeedback table
-- from the OLD permissions system roles to the corresponding NEW ones:
-- - `Allower` to `Approver`
-- - `Owner` to `Drafter` 
UPDATE rpf
SET rpf.UserPublicationRole = 'Approver'
FROM ReleasePublishingFeedback rpf
WHERE rpf.UserPublicationRole = 'Allower'

UPDATE rpf
SET rpf.UserPublicationRole = 'Drafter'
FROM ReleasePublishingFeedback rpf
WHERE rpf.UserPublicationRole = 'Owner'