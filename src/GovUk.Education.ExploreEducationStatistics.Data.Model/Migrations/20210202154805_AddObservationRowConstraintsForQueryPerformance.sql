--
-- This file recreates the indexes originally found on Observation and ObservationFilterItem tables onto
-- the new ObservationRow and ObservationRowFilterItem tables.  The comments below are the original
-- index-creation statements that were a part of the CREATE TABLE scripts for Observation and 
-- ObservationFilterItem.
--

-- create index IX_Observation_GeographicLevel
--     on Observation (GeographicLevel)
-- go
IF NOT EXISTS(SELECT * FROM sys.indexes WHERE Name = 'IX_ObservationRow_GeographicLevel')
CREATE INDEX IX_ObservationRow_GeographicLevel ON ObservationRow (GeographicLevel);

-- 
-- create index IX_Observation_LocationId
--     on Observation (LocationId)
-- go
IF NOT EXISTS(SELECT * FROM sys.indexes WHERE Name = 'IX_ObservationRow_LocationId')
CREATE INDEX IX_ObservationRow_LocationId ON ObservationRow (LocationId);

-- 
-- create index IX_Observation_TimeIdentifier
--     on Observation (TimeIdentifier)
-- go
IF NOT EXISTS(SELECT * FROM sys.indexes WHERE Name = 'IX_ObservationRow_TimeIdentifier')
CREATE INDEX IX_ObservationRow_TimeIdentifier ON ObservationRow (TimeIdentifier);

-- 
-- create index IX_Observation_Year
--     on Observation (Year)
-- go
IF NOT EXISTS(SELECT * FROM sys.indexes WHERE Name = 'IX_ObservationRow_Year')
CREATE INDEX IX_ObservationRow_Year ON ObservationRow (Year);

-- 
-- create index NCI_WI_ObservationRow_SubjectId
--     on Observation (SubjectId) include (GeographicLevel, LocationId, TimeIdentifier, Year)
-- go
IF NOT EXISTS(SELECT * FROM sys.indexes WHERE Name = 'NCI_WI_ObservationRow_SubjectId')
CREATE INDEX NCI_WI_ObservationRow_SubjectId ON ObservationRow (SubjectId) include (GeographicLevel, LocationId, TimeIdentifier, Year);

-- create index IX_ObservationFilterItem_FilterItemId
--     on ObservationFilterItem (FilterItemId)
-- 
IF NOT EXISTS(SELECT * FROM sys.indexes WHERE Name = 'IX_ObservationRowFilterItem_FilterItemId')
CREATE INDEX IX_ObservationRowFilterItem_FilterItemId ON ObservationRowFilterItem (FilterItemId);

-- create index IX_ObservationFilterItem_FilterId
--     on ObservationFilterItem (FilterId)
-- go
IF NOT EXISTS(SELECT * FROM sys.indexes WHERE Name = 'IX_ObservationRowFilterItem_FilterId')
CREATE INDEX IX_ObservationRowFilterItem_FilterId ON ObservationRowFilterItem (FilterId);

-- create index IX_ObservationFilterItem_FilterItemId_ObservationId
--     on ObservationFilterItem (FilterItemId, ObservationId)
-- 
--
-- Not recreating this one at present, as the ObservationRowFilterItem table no longer has a compound key
-- using ObservationId and FilterItemId