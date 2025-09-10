-- Delete all Accepted UserPublicationInvites and UserReleaseInvites
DELETE upi 
FROM UserPublicationInvites upi 
JOIN UserInvites ui 
  ON ui.Email = upi.Email 
  AND ui.Accepted = 1;

DELETE uri 
FROM UserReleaseInvites uri 
JOIN UserInvites ui 
  ON ui.Email = uri.Email 
  AND ui.Accepted = 1;
