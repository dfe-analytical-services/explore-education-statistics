-- Insert sections for 'RelatedDashboards' section type
INSERT INTO ContentSections (Id, [Order], Type, ReleaseVersionId)
SELECT NEWID(),
       0,
       'RelatedDashboards',
       rv.Id
FROM ReleaseVersions rv
WHERE rv.SoftDeleted = 0
  AND NOT EXISTS (SELECT 1
                  FROM ContentSections cs
                  WHERE cs.ReleaseVersionId = rv.Id
                    AND cs.Type = 'RelatedDashboards');

-- Insert sections for 'Warning' section type
INSERT INTO ContentSections (Id, [Order], Type, ReleaseVersionId)
SELECT NEWID(),
       0,
       'Warning',
       rv.Id
FROM ReleaseVersions rv
WHERE rv.SoftDeleted = 0
  AND NOT EXISTS (SELECT 1
                  FROM ContentSections cs
                  WHERE cs.ReleaseVersionId = rv.Id
                    AND cs.Type = 'Warning');