CREATE OR ALTER PROCEDURE InsertObservationFilterItems @ObservationFilterItems dbo.ObservationFilterItemType READONLY
AS
BEGIN
    INSERT INTO ObservationFilterItem
    SELECT *
    FROM @ObservationFilterItems
    ORDER BY ObservationId ASC, FilterItemId ASC
END
