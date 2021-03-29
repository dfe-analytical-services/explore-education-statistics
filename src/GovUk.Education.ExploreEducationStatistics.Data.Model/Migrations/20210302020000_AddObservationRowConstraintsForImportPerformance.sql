-- create index IX_ObservationRow_SubjectId
--     on ObservationRow (SubjectId)
-- go
IF NOT EXISTS(SELECT * FROM sys.indexes WHERE Name = 'IX_ObservationRow_SubjectId')
CREATE INDEX IX_ObservationRow_SubjectId ON ObservationRow (SubjectId);