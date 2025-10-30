-- Migrate 'EmailSent' values for existing UserPublicationRoles and UserReleaseRoles to be `DateTimeOffset.MinValue`.
UPDATE UserReleaseRoles 
SET EmailSent = '0001-01-01T00:00:00+00:00' 
WHERE EmailSent IS NULL;

UPDATE UserPublicationRoles 
SET EmailSent = '0001-01-01T00:00:00+00:00' 
WHERE EmailSent IS NULL;