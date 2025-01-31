CREATE OR ALTER PROCEDURE UpdateFilterItemPk
    @subjectId uniqueidentifier,
    @filterName NVARCHAR(max),
    @filterGroupLabel NVARCHAR(max),
    @filterItemLabel NVARCHAR(max),
    @newFilterItemId uniqueidentifier
AS
DECLARE @oldFilterItemId uniqueidentifier = (SELECT FI.Id
FROM FilterItem FI
JOIN FilterGroup FG on FI.FilterGroupId = FG.Id
JOIN Filter F on FG.FilterId = F.Id
JOIN Subject S on F.SubjectId = S.Id
WHERE S.Id = @subjectId
AND F.Name = @filterName
AND FG.Label = @filterGroupLabel
AND FI.Label = @filterItemLabel);
BEGIN TRANSACTION
    BEGIN TRY
        INSERT INTO FilterItem (Id, Label, FilterGroupId) SELECT @newFilterItemId, FI.Label, FI.FilterGroupId
        FROM FilterItem FI
        JOIN FilterGroup FG on FI.FilterGroupId = FG.Id
        JOIN Filter F on FG.FilterId = F.Id
        JOIN Subject S on F.SubjectId = S.Id
        WHERE S.Id = @subjectId
        AND F.Name = @filterName
        AND FG.Label = @filterGroupLabel
        AND FI.Label = @filterItemLabel;
        UPDATE ObservationFilterItem SET FilterItemId = @newFilterItemId WHERE FilterItemId = @oldFilterItemId;
        DELETE FROM FilterItem Where Id = @oldFilterItemId;
    COMMIT
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION
    DECLARE @errorMessage NVARCHAR(max) = ERROR_MESSAGE();
    DECLARE @errorSeverity INT = ERROR_SEVERITY();
    DECLARE @errorState INT = ERROR_SEVERITY();
    DECLARE @errorProcedure NVARCHAR(max) = ERROR_PROCEDURE();
    RAISERROR (N'Error executing %s MESSAGE: %s', @errorSeverity, @errorState, @errorProcedure, @errorMessage);
END CATCH
GO

EXEC UpdateFilterItemPk '3C0FBE56-0A4B-4CAA-82F2-AB696CD96090', 'school_type', 'Default', 'Total', '1F3F86A4-DE9F-43D7-5BFD-08D78F900A85';
EXEC UpdateFilterItemPk 'FA0D7F1D-D181-43FB-955B-FC327DA86F2C', 'nc_year_admission', 'Primary', 'All primary', 'E957DB0C-3BF8-4E4B-5C6F-08D78F900A85';
EXEC UpdateFilterItemPk 'FA0D7F1D-D181-43FB-955B-FC327DA86F2C', 'nc_year_admission', 'Secondary', 'All secondary', '5A7B4E97-7794-4037-5C71-08D78F900A85';

DROP PROCEDURE UpdateFilterItemPk;