CREATE OR ALTER PROCEDURE UpdateIndicatorPk
    @subjectId uniqueidentifier,
    @indicatorName NVARCHAR(max),
    @indicatorGroupLabel NVARCHAR(max),
    @newIndicatorId uniqueidentifier
AS
DECLARE @oldIndicatorId uniqueidentifier = (SELECT I.Id
FROM Indicator I
JOIN IndicatorGroup IG on I.IndicatorGroupId = IG.Id
JOIN Subject S on IG.SubjectId = S.Id
WHERE S.Id = @subjectId
AND I.Name = @indicatorName
AND IG.Label = @indicatorGroupLabel);
BEGIN TRANSACTION
    BEGIN TRY
        UPDATE Observation SET Measures = REPLACE(Measures, @oldIndicatorId, lower(@newIndicatorId)) WHERE SubjectId = @subjectId
        UPDATE Indicator SET Id = @newIndicatorId WHERE Id = @oldIndicatorId;
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

EXEC UpdateIndicatorPk '3C0FBE56-0A4B-4CAA-82F2-AB696CD96090', 'num_schools', 'Default', 'B3DF4FB1-DAE3-4C16-4C01-08D78F90080F';
EXEC UpdateIndicatorPk '3C0FBE56-0A4B-4CAA-82F2-AB696CD96090', 'headcount', 'Default', 'A5A58F92-ABA1-4955-4C02-08D78F90080F';
EXEC UpdateIndicatorPk '3C0FBE56-0A4B-4CAA-82F2-AB696CD96090', 'perm_excl', 'Default', '167F4807-4FDD-461A-4C03-08D78F90080F';
EXEC UpdateIndicatorPk '3C0FBE56-0A4B-4CAA-82F2-AB696CD96090', 'perm_excl_rate', 'Default', 'BE3B765B-005F-4279-4C04-08D78F90080F';
EXEC UpdateIndicatorPk '3C0FBE56-0A4B-4CAA-82F2-AB696CD96090', 'fixed_excl', 'Default', 'F045BC8D-8DD1-4F16-4C05-08D78F90080F';
EXEC UpdateIndicatorPk '3C0FBE56-0A4B-4CAA-82F2-AB696CD96090', 'fixed_excl_rate', 'Default', '68AEDA43-2B6A-433A-4C06-08D78F90080F';
EXEC UpdateIndicatorPk '3C0FBE56-0A4B-4CAA-82F2-AB696CD96090', 'one_plus_fixed_rate', 'Default', '732F0D7B-DCD3-4BF8-4C08-08D78F90080F';
EXEC UpdateIndicatorPk 'FA0D7F1D-D181-43FB-955B-FC327DA86F2C', 'admission_numbers', 'Admissions', '49D2A1F4-E4A9-4F25-4C24-08D78F90080F';
EXEC UpdateIndicatorPk 'FA0D7F1D-D181-43FB-955B-FC327DA86F2C', 'first_preference_offers', 'Preferences breakdowns', '94F9B11C-DF82-4EEF-4C29-08D78F90080F';
EXEC UpdateIndicatorPk 'FA0D7F1D-D181-43FB-955B-FC327DA86F2C', 'second_preference_offers', 'Preferences breakdowns', 'D22E1104-DE56-4617-4C2A-08D78F90080F';
EXEC UpdateIndicatorPk 'FA0D7F1D-D181-43FB-955B-FC327DA86F2C', 'third_preference_offers', 'Preferences breakdowns', '319DD956-A714-40FD-4C2B-08D78F90080F';
EXEC UpdateIndicatorPk 'FA0D7F1D-D181-43FB-955B-FC327DA86F2C', 'one_of_the_three_preference_offers', 'Preferences breakdowns', 'A9211C9D-B467-48D7-4C2C-08D78F90080F';
EXEC UpdateIndicatorPk 'FA0D7F1D-D181-43FB-955B-FC327DA86F2C', 'preferred_school_offer', 'Preferences breakdowns', 'BE1E1643-F7C8-40B0-4C2D-08D78F90080F';
EXEC UpdateIndicatorPk 'FA0D7F1D-D181-43FB-955B-FC327DA86F2C', 'non_preferred_offer', 'Preferences breakdowns', '16CDFC0A-F66F-496B-4C2E-08D78F90080F';
EXEC UpdateIndicatorPk 'FA0D7F1D-D181-43FB-955B-FC327DA86F2C', 'no_offer', 'Preferences breakdowns', '2C63589E-B5D4-4922-4C2F-08D78F90080F';
EXEC UpdateIndicatorPk 'FA0D7F1D-D181-43FB-955B-FC327DA86F2C', 'schools_in_la_offer', 'Preferences breakdowns', 'D10D4F10-C2F8-4120-4C30-08D78F90080F';

DROP PROCEDURE UpdateIndicatorPk;