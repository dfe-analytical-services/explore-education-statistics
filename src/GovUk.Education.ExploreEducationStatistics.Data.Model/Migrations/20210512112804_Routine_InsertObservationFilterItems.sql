CREATE PROCEDURE InsertObservationFilterItems @ObservationFilterItems dbo.ObservationFilterItemType READONLY
AS
BEGIN
    INSERT INTO ObservationFilterItem SELECT * FROM @ObservationFilterItems
END
