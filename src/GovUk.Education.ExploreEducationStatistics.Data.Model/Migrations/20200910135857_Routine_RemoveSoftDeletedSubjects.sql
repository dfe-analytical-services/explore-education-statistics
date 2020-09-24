CREATE OR ALTER PROCEDURE RemoveSoftDeletedSubjects
AS
BEGIN
    DELETE FROM Subject WHERE SoftDeleted = 1;
END