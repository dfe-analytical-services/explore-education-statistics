IF EXISTS (SELECT * FROM sys.columns
WHERE Name = 'FilterId' AND OBJECT_ID = OBJECT_ID('ObservationRowFilterItem'))
ALTER TABLE ObservationRowFilterItem DROP COLUMN FilterId;