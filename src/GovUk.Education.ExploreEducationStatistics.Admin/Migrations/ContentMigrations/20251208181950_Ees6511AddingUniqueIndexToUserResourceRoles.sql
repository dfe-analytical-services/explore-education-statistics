-- Migrate existing UserPublicationInvites/UserReleaseInvites to corresponding UserPublicationRoles/UserReleaseRoles

-- ===========================================
-- Migrate UserReleaseInvites -> UserReleaseRoles
-- ===========================================

INSERT INTO UserReleaseRoles (
    Id,
    UserId,
    ReleaseVersionId,
    Role,
    Created,
    CreatedById,
    EmailSent
)
SELECT
    NEWID() AS Id,
    u.Id AS UserId,
    uri.ReleaseVersionId,
    uri.Role,
    uri.Created,
    uri.CreatedById,
    CASE 
        WHEN uri.EmailSent = 1 THEN '0001-01-01T00:00:00+00:00'
        ELSE NULL
    END AS EmailSent
FROM UserReleaseInvites AS uri
INNER JOIN Users AS u
    ON u.Email = uri.Email -- safe because case-insensitive collation
WHERE NOT EXISTS ( -- avoid duplicates
    SELECT 1
    FROM UserReleaseRoles AS urr
    WHERE urr.UserId = u.Id
      AND urr.ReleaseVersionId = uri.ReleaseVersionId
      AND urr.Role = uri.Role
);

-- ===========================================
-- Migrate UserPublicationInvites -> UserPublicationRoles
-- ===========================================

INSERT INTO UserPublicationRoles (
    Id,
    UserId,
    PublicationId,
    Role,
    Created,
    CreatedById,
    EmailSent
)
SELECT
    NEWID() AS Id,
    u.Id AS UserId,
    upi.PublicationId,
    upi.Role,
    upi.Created,
    upi.CreatedById,
    '0001-01-01T00:00:00+00:00' AS EmailSent
FROM UserPublicationInvites AS upi
INNER JOIN Users AS u
    ON u.Email = upi.Email -- safe because case-insensitive collation
WHERE NOT EXISTS ( -- avoid duplicates
    SELECT 1
    FROM UserPublicationRoles AS upr
    WHERE upr.UserId = u.Id
      AND upr.PublicationId = upi.PublicationId
      AND upr.Role = upi.Role
);
