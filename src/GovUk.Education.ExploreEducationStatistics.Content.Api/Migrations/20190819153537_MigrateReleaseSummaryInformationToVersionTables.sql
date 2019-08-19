-- TODO when we migrate over versioning of releases then this t-sql will copy across the the summary information 
-- TODO from the release to the version tables. git 
DECLARE @Release_Id UNIQUEIDENTIFIER
DECLARE @Release_Summary_Id UNIQUEIDENTIFIER
DECLARE @Release_Summary_Version_Id UNIQUEIDENTIFIER
DECLARE @Release_Name NVARCHAR(MAX)
DECLARE @Time_Period_Coverage NVARCHAR(6)
DECLARE @TypeId UNIQUEIDENTIFIER
DECLARE @Slug NVARCHAR(MAX)
DECLARE @Summary NVARCHAR(MAX)
DECLARE @Next_Release_Date NVARCHAR(MAX)
DECLARE @Publish_Scheduled DATETIME2

DEClARE releases_cursor CURSOR FOR 
    SELECT Id FROM Releases;

OPEN releases_cursor
FETCH NEXT FROM releases_cursor INTO @Release_Id
WHILE @@FETCH_STATUS = 0
BEGIN
  -- Reset all of the variables as SELECT into them will not change them if the SELECT returns nothing. 
  SET @Release_Summary_Id = NULL
  SET @Release_Summary_Version_Id = NULL
  SET @Release_Name = NULL
  SET @Time_Period_Coverage = NULL
  SET @TypeId = NULL
  SET @Slug = NULL
  SET @Summary = NULL
  SET @Next_Release_Date = NULL
  SET @Publish_Scheduled = NULL
  
  -- Check we have a release summary for the release and if not create one.
  SELECT @Release_Summary_Id = rs.Id
    FROM ReleaseSummaries rs
    JOIN Releases r
      ON r.Id = rs.ReleaseId
   WHERE r.Id = @Release_Id
      
  IF @Release_Summary_Id IS NULL
  BEGIN
    -- We do not have a release summary so we insert one 
    SET @Release_Summary_Id = NEWID()
      INSERT 
        INTO ReleaseSummaries (Id, ReleaseId) 
      VALUES (@Release_Summary_Id, @Release_Id)
  END
  
  -- Check we have a release summary version for the release and if not create one.
  SELECT @Release_Summary_Version_Id = rsv.Id 
     FROM ReleaseSummaryVersions rsv 
     JOIN ReleaseSummaries rs 
       ON rsv.ReleaseSummaryId = rs.Id
    WHERE rs.Id = @Release_Summary_Id

  IF @Release_Summary_Version_Id IS NULL
  BEGIN
    -- We do not have a release summary version so we insert one based on the original release.
    SET @Release_Summary_Version_Id = NEWID()
    SELECT @Release_Name = ReleaseName, 
           @Time_Period_Coverage = TimePeriodCoverage, 
           @TypeId = TypeId,
           @Slug = Slug,
           @Summary = Summary,
           @Next_Release_Date = NextReleaseDate,
           @Publish_Scheduled = PublishScheduled
      FROM Releases 
     WHERE Id = @Release_Id
    -- Now we have the values insert them.
    INSERT INTO ReleaseSummaryVersions 
                (Id, ReleaseSummaryId, Created, ReleaseName, TimePeriodCoverage, Slug, TypeId, PublishScheduled, NextReleaseDate, Summary)
    VALUES (@Release_Summary_Version_Id, @Release_Summary_Id, GETDATE(), @Release_Name, @Time_Period_Coverage, @Slug, @TypeId, @Publish_Scheduled, @Next_Release_Date, @Summary);
  END
  FETCH NEXT FROM releases_cursor INTO @Release_Id
END
CLOSE releases_cursor
DEALLOCATE releases_cursor