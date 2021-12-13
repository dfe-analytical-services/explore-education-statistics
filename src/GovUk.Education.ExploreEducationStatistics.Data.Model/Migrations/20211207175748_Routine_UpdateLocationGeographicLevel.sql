/**
  Temporary script which sets Location.GeographicLevel = Observation.GeographicLevel
  for Locations referenced by Observations.

  Skips any orphaned Locations with a warning message.

  First checks that all Observations referencing a Location have a unique GeographicLevel.
  Skips any Locations without a unique GeographicLevel with a warning message.

  Locations are not updated if they already have a GeographicLevel making this script re-runnable.

  Remember to run this script in both the `statistics` and `public_statistics` databases.
 */
CREATE OR
ALTER PROCEDURE UpdateLocationGeographicLevel
AS
BEGIN
    DECLARE @warningCountOrphaned INT;
    DECLARE @warningCountNotUnique INT;
    DECLARE @locationsWithMultipleLevels TABLE
                                         (
                                             LocationId UNIQUEIDENTIFIER
                                         );
    DECLARE @orphanLocations TABLE
                             (
                                 LocationId UNIQUEIDENTIFIER
                             );

    INSERT INTO @orphanLocations
    SELECT Location.Id
    FROM Location
             LEFT JOIN Observation on Location.Id = Observation.LocationId
    WHERE Observation.Id IS NULL;

    SELECT @warningCountOrphaned = COUNT(LocationId)
    FROM @orphanLocations;

    IF @warningCountOrphaned > 0
        RAISERROR ('Warning: %i Locations are orphaned', 0, 1, @warningCountOrphaned);

    INSERT INTO @locationsWithMultipleLevels
    SELECT L.LocationId
    FROM (
             SELECT DISTINCT LocationId, GeographicLevel
             FROM Observation
         ) L
    GROUP BY L.LocationId
    HAVING COUNT(GeographicLevel) > 1;

    SELECT @warningCountNotUnique = COUNT(LocationId)
    FROM @locationsWithMultipleLevels;

    IF @warningCountNotUnique > 0
        RAISERROR ('Warning: %i Locations are referenced by Observations without a unique GeographicLevel', 0, 1, @warningCountNotUnique);

    -- Update all Locations that are referenced by Observations
    -- except those with a geographic level already set or where a unique geographic level can't be determined 
    UPDATE Location
    SET Location.GeographicLevel = Observation.GeographicLevel
    FROM Location
             JOIN Observation ON Location.Id = Observation.LocationId
    WHERE Location.GeographicLevel IS NULL
      AND Location.Id NOT IN (SELECT LocationId FROM @locationsWithMultipleLevels);
END
