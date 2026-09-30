-- Assign 'Department for Education' as a publishing organisation,
-- to all non soft-deleted release versions which have no publishing organisations.
DECLARE @OrganisationId UNIQUEIDENTIFIER = (SELECT Id
                                            FROM dbo.Organisations
                                            WHERE Title = N'Department for Education');

IF @OrganisationId IS NOT NULL
    INSERT INTO dbo.ReleaseVersionPublishingOrganisations (OrganisationId, ReleaseVersionId)
    SELECT @OrganisationId, rv.Id
    FROM dbo.ReleaseVersions rv
    WHERE rv.SoftDeleted = 0
        AND NOT EXISTS (SELECT 1
                        FROM dbo.ReleaseVersionPublishingOrganisations rvpo
                        WHERE rvpo.ReleaseVersionId = rv.Id)
ELSE
    THROW 50000, 'Organisation ''Department for Education'' not found.', 1;