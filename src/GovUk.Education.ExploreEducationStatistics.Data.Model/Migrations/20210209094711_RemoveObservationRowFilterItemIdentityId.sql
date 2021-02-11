IF EXISTS(SELECT * FROM sys.columns
WHERE Name = 'Id' AND OBJECT_ID = OBJECT_ID('ObservationRowFilterItem'))
BEGIN
    ALTER TABLE ObservationRowFilterItem DROP CONSTRAINT PK_ObservationRowFilterItem;
    ALTER TABLE ObservationRowFilterItem DROP COLUMN Id;
    ALTER TABLE ObservationRowFilterItem ADD CONSTRAINT PK_ObservationRowFilterItem PRIMARY KEY CLUSTERED (ObservationId, FilterItemId);
END    