/**
  Script which deletes Locations not referenced by any Observations.
  Orphaned Locations occur when all Observations referencing them are deleted.
 */
CREATE OR
ALTER PROCEDURE DeleteOrphanedLocations AS
BEGIN
    DELETE Location
    FROM Location
        LEFT JOIN Observation ON Location.Id = Observation.LocationId
    WHERE Observation.Id IS NULL;
END;
