CREATE OR ALTER PROCEDURE InsertSubjectFootnote
    @subjectName NVARCHAR(max),
    @footnoteId BIGINT
AS
DECLARE @subjectId BIGINT = (SELECT S.Id FROM Subject S WHERE S.Name = @subjectName);
BEGIN TRY
    INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subjectId, @footnoteId);
END TRY
BEGIN CATCH
    DECLARE @errorMessage NVARCHAR(max) = ERROR_MESSAGE();
    DECLARE @errorSeverity INT = ERROR_SEVERITY();
    DECLARE @errorState INT = ERROR_SEVERITY();
    DECLARE @errorProcedure NVARCHAR(max) = ERROR_PROCEDURE();
    RAISERROR (N'Error executing %s(SUBJECT: %s, FOOTNOTE: %I64i) MESSAGE: %s', @errorSeverity, @errorState, @errorProcedure, @subjectName, @footnoteId, @errorMessage);
END CATCH
GO;

CREATE OR ALTER PROCEDURE InsertIndicatorFootnote
    @subjectName NVARCHAR(max),
    @indicatorName NVARCHAR(max),
    @footnoteId BIGINT
AS
DECLARE
    @indicatorId BIGINT = (SELECT I.Id FROM Indicator I JOIN IndicatorGroup IG on I.IndicatorGroupId = IG.Id JOIN Subject S on IG.SubjectId = S.Id WHERE S.Name = @subjectName AND I.Name = @indicatorName);
BEGIN TRY
    INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (@indicatorId, @footnoteId);
END TRY
BEGIN CATCH
    DECLARE @errorMessage NVARCHAR(max) = ERROR_MESSAGE();
    DECLARE @errorSeverity INT = ERROR_SEVERITY();
    DECLARE @errorState INT = ERROR_SEVERITY();
    DECLARE @errorProcedure NVARCHAR(max) = ERROR_PROCEDURE();
    RAISERROR (N'Error executing %s(SUBJECT: %s, INDICATOR: %s, FOOTNOTE: %I64i) MESSAGE: %s', @errorSeverity, @errorState, @errorProcedure, @subjectName, @indicatorName, @footnoteId, @errorMessage);
END CATCH
GO;

CREATE OR ALTER PROCEDURE InsertFilterFootnote
    @subjectName NVARCHAR(max),
    @filterName NVARCHAR(max),
    @footnoteId BIGINT
AS
DECLARE
    @filterId BIGINT = (SELECT F.Id FROM Filter F JOIN Subject S on F.SubjectId = S.Id WHERE S.Name = @subjectName AND F.Name = @filterName);
BEGIN TRY
    INSERT INTO FilterFootnote (FilterId, FootnoteId) VALUES (@filterId, @footnoteId);
END TRY
BEGIN CATCH
    DECLARE @errorMessage NVARCHAR(max) = ERROR_MESSAGE();
    DECLARE @errorSeverity INT = ERROR_SEVERITY();
    DECLARE @errorState INT = ERROR_SEVERITY();
    DECLARE @errorProcedure NVARCHAR(max) = ERROR_PROCEDURE();
    RAISERROR (N'Error executing %s(SUBJECT: %s, FILTER: %s, FOOTNOTE: %I64i) MESSAGE: %s', @errorSeverity, @errorState, @errorProcedure, @subjectName, @filterName, @footnoteId, @errorMessage);
END CATCH
GO;

CREATE OR ALTER PROCEDURE InsertFilterGroupFootnote
    @subjectName NVARCHAR(max),
    @filterGroupName NVARCHAR(max),
    @footnoteId BIGINT
AS
DECLARE
    @filterGroupId BIGINT = (SELECT FG.Id FROM FilterGroup FG JOIN Filter F ON FG.FilterId = F.Id JOIN Subject S on F.SubjectId = S.Id WHERE S.Name = @subjectName AND FG.Label = @filterGroupName);
BEGIN TRY
    INSERT INTO FilterGroupFootnote (FilterGroupId, FootnoteId) VALUES (@filterGroupId, @footnoteId);
END TRY
BEGIN CATCH
    DECLARE @errorMessage NVARCHAR(max) = ERROR_MESSAGE();
    DECLARE @errorSeverity INT = ERROR_SEVERITY();
    DECLARE @errorState INT = ERROR_SEVERITY();
    DECLARE @errorProcedure NVARCHAR(max) = ERROR_PROCEDURE();
    RAISERROR (N'Error executing %s(SUBJECT: %s, FILTER GROUP: %s, FOOTNOTE: %I64i) MESSAGE: %s', @errorSeverity, @errorState, @errorProcedure, @subjectName, @filterGroupName, @footnoteId, @errorMessage);
END CATCH
GO;

CREATE OR ALTER PROCEDURE InsertFilterItemFootnote
    @subjectName NVARCHAR(max),
    @filterItemName NVARCHAR(max),
    @footnoteId BIGINT
AS
DECLARE
    @filterItemId BIGINT = (SELECT FI.Id FROM FilterItem FI JOIN FilterGroup FG ON FI.FilterGroupId = FG.Id JOIN Filter F on FG.FilterId = F.Id  JOIN Subject S on F.SubjectId = S.Id WHERE S.Name = @subjectName AND FI.Label = @filterItemName);
BEGIN TRY
    INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (@filterItemId, @footnoteId);
END TRY
BEGIN CATCH
    DECLARE @errorMessage NVARCHAR(max) = ERROR_MESSAGE();
    DECLARE @errorSeverity INT = ERROR_SEVERITY();
    DECLARE @errorState INT = ERROR_SEVERITY();
    DECLARE @errorProcedure NVARCHAR(max) = ERROR_PROCEDURE();
    RAISERROR (N'Error executing %s(SUBJECT: %s, FILTER ITEM: %s, FOOTNOTE: %I64i) MESSAGE: %s', @errorSeverity, @errorState, @errorProcedure, @subjectName, @filterItemName, @footnoteId, @errorMessage);
END CATCH
GO;


DELETE FROM SubjectFootnote WHERE 1=1;
DELETE FROM IndicatorFootnote WHERE 1=1;
DELETE FROM FilterItemFootnote WHERE 1=1;
DELETE FROM FilterGroupFootnote WHERE 1=1;
DELETE FROM FilterFootnote WHERE 1=1;
DELETE FROM Footnote WHERE 1=1;


--
-- Footnotes
--

SET IDENTITY_INSERT Footnote ON

INSERT INTO Footnote (Id, Content)
VALUES (1, 'State-funded primary and secondary schools include middle schools as deemed.');

INSERT INTO Footnote (Id, Content)
VALUES (2, 'State-funded primary schools include all primary academies, including free schools.');

INSERT INTO Footnote (Id, Content)
VALUES (3,
        'State-funded secondary schools include city technology colleges and all secondary academies, including all-through academies and free schools.');

INSERT INTO Footnote (Id, Content)
VALUES (4,
        'Special schools include maintained special schools, non-maintained special schools and special academies.  Excludes general hospital schools, independent special schools and independent schools approved for SEN pupils.');

INSERT INTO Footnote (Id, Content)
VALUES (5,
        'Totals may not appear to equal the sum of component parts because numbers have been rounded to the nearest 5.');

INSERT INTO Footnote (Id, Content)
VALUES (6, 'x - 1 or 2 enrolments, or a percentage based on 1 or 2 enrolments.');

INSERT INTO Footnote (Id, Content)
VALUES (7,
        'There may be discrepancies between totals and the sum of constituent parts  as national and regional totals and totals across school types have been rounded to the nearest 5.');

INSERT INTO Footnote (Id, Content)
VALUES (8,
        'Absence rates are the number of absence sessions expressed as a percentage of the total number of possible sessions.');

INSERT INTO Footnote (Id, Content)
VALUES (9,
        'Pupil enrolments missing 10 percent or more of their own possible sessions (due to authorised or unauthorised absence) are classified as persistent absentees.');

INSERT INTO Footnote (Id, Content)
VALUES (10,
        'The definition of persistent absence changed from the 2015/16 academic year - The information presented for years prior to 2015/16 has been produced using the same methodology in order to allow users to make comparisons on a consistent basis over time.');

INSERT INTO Footnote (Id, Content)
VALUES (11,
        'The persistent absentee rate is the number of persistent absentees expressed as a percentage of the total number of enrolments.');

INSERT INTO Footnote (Id, Content)
VALUES (12,
        'Number of enrolments includes pupils on the school roll for at least one session who are aged between 5 and 15, excluding boarders. Some pupils may be counted more than once (if they moved schools during the academic year or are registered in more than one school).');

INSERT INTO Footnote (Id, Content)
VALUES (13, 'The number of enrolments with one or more session of absence for specific reason.');

INSERT INTO Footnote (Id, Content)
VALUES (14,
        'The number of sessions missed in each band expressed as a percentage of the total number of sessions missed for that category of absence overall.');

INSERT INTO Footnote (Id, Content)
VALUES (15, 'Includes schools with at least six enrolments in 2016/17.');

INSERT INTO Footnote (Id, Content)
VALUES (16,
        'See Chapter 5 of the "Guide to absence statistics" for more information on how absence and pupil characteristic data have been linked.');

INSERT INTO Footnote (Id, Content)
VALUES (17,
        'Figures for pupils with unclassified or missing characteristics information should be interpreted with caution.');

INSERT INTO Footnote (Id, Content)
VALUES (18,
        'In September 2014 the Special Educational Needs and Disability (SEND) provisions were introduced in the Children and Families Act 2014. This reform caused changes to SEN categories, for further information on these changes please see the "Special educational needs in England: January 2015" Statistical First Release.');

INSERT INTO Footnote (Id, Content)
VALUES (19,
        'Education, Health and Care (EHC) plans were introduced from September 2014 as part of a range of SEND reforms.');

INSERT INTO Footnote (Id, Content)
VALUES (20,
        'From 2015 SEN support replaces school action and school action plus but some pupils remain with these provision types in first year of transition.');

INSERT INTO Footnote (Id, Content)
VALUES (21,
        'Minority ethnic group includes pupils who have been classified according to their ethnic group, excluding White British.');

INSERT INTO Footnote (Id, Content)
VALUES (22,
        'From 2015 SEN support replaced school action and School Action plus but some pupils remain with these provision types. Those who were formerly School Action did not provide type of need in previous years. Those who remain on school action provision are not included here but have been included within the SEN support category in other tables.');

INSERT INTO Footnote (Id, Content)
VALUES (23,
        'Social Emotional and Mental Health was added as a new type of need in 2015, the previous type of need Behaviour, Emotional and Social Difficulties has been removed although it is not expected it should be a direct replacement.');

INSERT INTO Footnote (Id, Content)
VALUES (24,
        'Includes pupils on the school roll for at least one session who are aged 4, excluding boarders. Some pupils may be counted more than once (if they moved schools during the academic year or are registered at more than one school).');

INSERT INTO Footnote (Id, Content)
VALUES (25,
        'Only overall absence is recorded for pupils aged 4, absences are not categorised as authorised or unauthorised.');

SET IDENTITY_INSERT Footnote OFF


--
-- Subjects
--


DECLARE @subject_absence_by_characteristic   NVARCHAR(max) = 'Absence by characteristic';
DECLARE @subject_absence_by_geographic_level NVARCHAR(max) = 'Absence by geographic level';
DECLARE @subject_absence_by_term             NVARCHAR(max) = 'Absence by term';
DECLARE @subject_absence_for_four_year_olds  NVARCHAR(max) = 'Absence for four year olds';
DECLARE @subject_absence_in_prus             NVARCHAR(max) = 'Absence in prus';
DECLARE @subject_absence_number_missing      NVARCHAR(max) = 'Absence number missing at least one session by reason';
DECLARE @subject_absence_rate_percent_bands  NVARCHAR(max) = 'Absence rate percent bands';

EXEC InsertSubjectFootnote @subject_absence_by_characteristic, 1;
EXEC InsertSubjectFootnote @subject_absence_by_geographic_level, 1;
EXEC InsertSubjectFootnote @subject_absence_by_term, 1;
EXEC InsertSubjectFootnote @subject_absence_for_four_year_olds, 1;
EXEC InsertSubjectFootnote @subject_absence_in_prus, 1;
EXEC InsertSubjectFootnote @subject_absence_number_missing, 1;
EXEC InsertSubjectFootnote @subject_absence_rate_percent_bands, 1;

EXEC InsertSubjectFootnote @subject_absence_by_characteristic, 2;
EXEC InsertSubjectFootnote @subject_absence_by_geographic_level, 2;
EXEC InsertSubjectFootnote @subject_absence_by_term, 2;
EXEC InsertSubjectFootnote @subject_absence_for_four_year_olds, 2;
EXEC InsertSubjectFootnote @subject_absence_in_prus, 2;
EXEC InsertSubjectFootnote @subject_absence_number_missing, 2;
EXEC InsertSubjectFootnote @subject_absence_rate_percent_bands, 2;

EXEC InsertSubjectFootnote @subject_absence_by_characteristic, 3;
EXEC InsertSubjectFootnote @subject_absence_by_geographic_level, 3;
EXEC InsertSubjectFootnote @subject_absence_by_term, 3;
EXEC InsertSubjectFootnote @subject_absence_for_four_year_olds, 3;
EXEC InsertSubjectFootnote @subject_absence_in_prus, 3;
EXEC InsertSubjectFootnote @subject_absence_number_missing, 3;
EXEC InsertSubjectFootnote @subject_absence_rate_percent_bands, 3;

EXEC InsertSubjectFootnote @subject_absence_by_characteristic, 4;
EXEC InsertSubjectFootnote @subject_absence_by_geographic_level, 4;
EXEC InsertSubjectFootnote @subject_absence_by_term, 4;
EXEC InsertSubjectFootnote @subject_absence_for_four_year_olds, 4;
EXEC InsertSubjectFootnote @subject_absence_in_prus, 4;
EXEC InsertSubjectFootnote @subject_absence_number_missing, 4;
EXEC InsertSubjectFootnote @subject_absence_rate_percent_bands, 4;

EXEC InsertSubjectFootnote @subject_absence_by_characteristic, 5;
EXEC InsertSubjectFootnote @subject_absence_by_geographic_level, 5;
EXEC InsertSubjectFootnote @subject_absence_by_term, 5;
EXEC InsertSubjectFootnote @subject_absence_for_four_year_olds, 5;
EXEC InsertSubjectFootnote @subject_absence_in_prus, 5;
EXEC InsertSubjectFootnote @subject_absence_number_missing, 5;
EXEC InsertSubjectFootnote @subject_absence_rate_percent_bands, 5;

EXEC InsertSubjectFootnote @subject_absence_by_characteristic, 6;
EXEC InsertSubjectFootnote @subject_absence_by_geographic_level, 6;
EXEC InsertSubjectFootnote @subject_absence_by_term, 6;
EXEC InsertSubjectFootnote @subject_absence_for_four_year_olds, 6;
EXEC InsertSubjectFootnote @subject_absence_in_prus, 6;
EXEC InsertSubjectFootnote @subject_absence_number_missing, 6;
EXEC InsertSubjectFootnote @subject_absence_rate_percent_bands, 6;

EXEC InsertSubjectFootnote @subject_absence_by_characteristic, 7;
EXEC InsertSubjectFootnote @subject_absence_by_geographic_level, 7;
EXEC InsertSubjectFootnote @subject_absence_by_term, 7;
EXEC InsertSubjectFootnote @subject_absence_for_four_year_olds, 7;
EXEC InsertSubjectFootnote @subject_absence_in_prus, 7;
EXEC InsertSubjectFootnote @subject_absence_number_missing, 7;
EXEC InsertSubjectFootnote @subject_absence_rate_percent_bands, 7;

EXEC InsertSubjectFootnote @subject_absence_by_characteristic, 8;
EXEC InsertSubjectFootnote @subject_absence_by_geographic_level, 8;
EXEC InsertSubjectFootnote @subject_absence_by_term, 8;
EXEC InsertSubjectFootnote @subject_absence_for_four_year_olds, 8;
EXEC InsertSubjectFootnote @subject_absence_in_prus, 8;
EXEC InsertSubjectFootnote @subject_absence_number_missing, 8;
EXEC InsertSubjectFootnote @subject_absence_rate_percent_bands, 8;

EXEC InsertSubjectFootnote @subject_absence_by_characteristic, 9;
EXEC InsertSubjectFootnote @subject_absence_by_geographic_level, 9;
EXEC InsertSubjectFootnote @subject_absence_by_term, 9;
EXEC InsertSubjectFootnote @subject_absence_for_four_year_olds, 9;
EXEC InsertSubjectFootnote @subject_absence_in_prus, 9;
EXEC InsertSubjectFootnote @subject_absence_number_missing, 9;
EXEC InsertSubjectFootnote @subject_absence_rate_percent_bands, 9;

EXEC InsertSubjectFootnote @subject_absence_by_characteristic, 10;
EXEC InsertSubjectFootnote @subject_absence_by_geographic_level, 10;
EXEC InsertSubjectFootnote @subject_absence_by_term, 10;
EXEC InsertSubjectFootnote @subject_absence_for_four_year_olds, 10;
EXEC InsertSubjectFootnote @subject_absence_in_prus, 10;
EXEC InsertSubjectFootnote @subject_absence_number_missing, 10;
EXEC InsertSubjectFootnote @subject_absence_rate_percent_bands, 10;

EXEC InsertSubjectFootnote @subject_absence_by_characteristic, 11;
EXEC InsertSubjectFootnote @subject_absence_by_geographic_level, 11;
EXEC InsertSubjectFootnote @subject_absence_by_term, 11;
EXEC InsertSubjectFootnote @subject_absence_for_four_year_olds, 11;
EXEC InsertSubjectFootnote @subject_absence_in_prus, 11;
EXEC InsertSubjectFootnote @subject_absence_number_missing, 11;
EXEC InsertSubjectFootnote @subject_absence_rate_percent_bands, 11;

-- Except Absence for four year olds
EXEC InsertSubjectFootnote @subject_absence_by_characteristic, 12;
EXEC InsertSubjectFootnote @subject_absence_by_geographic_level, 12;
EXEC InsertSubjectFootnote @subject_absence_by_term, 12;
EXEC InsertSubjectFootnote @subject_absence_in_prus, 12;
EXEC InsertSubjectFootnote @subject_absence_number_missing, 12;
EXEC InsertSubjectFootnote @subject_absence_rate_percent_bands, 12;

EXEC InsertSubjectFootnote @subject_absence_by_characteristic, 13;
EXEC InsertSubjectFootnote @subject_absence_by_geographic_level, 13;
EXEC InsertSubjectFootnote @subject_absence_by_term, 13;
EXEC InsertSubjectFootnote @subject_absence_for_four_year_olds, 13;
EXEC InsertSubjectFootnote @subject_absence_in_prus, 13;
EXEC InsertSubjectFootnote @subject_absence_number_missing, 13;
EXEC InsertSubjectFootnote @subject_absence_rate_percent_bands, 13;

EXEC InsertSubjectFootnote @subject_absence_by_characteristic, 14;
EXEC InsertSubjectFootnote @subject_absence_by_geographic_level, 14;
EXEC InsertSubjectFootnote @subject_absence_by_term, 14;
EXEC InsertSubjectFootnote @subject_absence_for_four_year_olds, 14;
EXEC InsertSubjectFootnote @subject_absence_in_prus, 14;
EXEC InsertSubjectFootnote @subject_absence_number_missing, 14;
EXEC InsertSubjectFootnote @subject_absence_rate_percent_bands, 14;

EXEC InsertSubjectFootnote @subject_absence_by_characteristic, 15;
EXEC InsertSubjectFootnote @subject_absence_by_geographic_level, 15;
EXEC InsertSubjectFootnote @subject_absence_by_term, 15;
EXEC InsertSubjectFootnote @subject_absence_for_four_year_olds, 15;
EXEC InsertSubjectFootnote @subject_absence_in_prus, 15;
EXEC InsertSubjectFootnote @subject_absence_number_missing, 15;
EXEC InsertSubjectFootnote @subject_absence_rate_percent_bands, 15;

EXEC InsertSubjectFootnote @subject_absence_by_characteristic, 16;
EXEC InsertSubjectFootnote @subject_absence_by_geographic_level, 16;
EXEC InsertSubjectFootnote @subject_absence_by_term, 16;
EXEC InsertSubjectFootnote @subject_absence_for_four_year_olds, 16;
EXEC InsertSubjectFootnote @subject_absence_in_prus, 16;
EXEC InsertSubjectFootnote @subject_absence_number_missing, 16;
EXEC InsertSubjectFootnote @subject_absence_rate_percent_bands, 16;

EXEC InsertSubjectFootnote @subject_absence_by_characteristic, 17;
EXEC InsertSubjectFootnote @subject_absence_by_geographic_level, 17;
EXEC InsertSubjectFootnote @subject_absence_by_term, 17;
EXEC InsertSubjectFootnote @subject_absence_for_four_year_olds, 17;
EXEC InsertSubjectFootnote @subject_absence_in_prus, 17;
EXEC InsertSubjectFootnote @subject_absence_number_missing, 17;
EXEC InsertSubjectFootnote @subject_absence_rate_percent_bands, 17;

EXEC InsertSubjectFootnote @subject_absence_by_characteristic, 18;
EXEC InsertSubjectFootnote @subject_absence_by_geographic_level, 18;
EXEC InsertSubjectFootnote @subject_absence_by_term, 18;
EXEC InsertSubjectFootnote @subject_absence_for_four_year_olds, 18;
EXEC InsertSubjectFootnote @subject_absence_in_prus, 18;
EXEC InsertSubjectFootnote @subject_absence_number_missing, 18;
EXEC InsertSubjectFootnote @subject_absence_rate_percent_bands, 18;

EXEC InsertSubjectFootnote @subject_absence_by_characteristic, 19;
EXEC InsertSubjectFootnote @subject_absence_by_geographic_level, 19;
EXEC InsertSubjectFootnote @subject_absence_by_term, 19;
EXEC InsertSubjectFootnote @subject_absence_for_four_year_olds, 19;
EXEC InsertSubjectFootnote @subject_absence_in_prus, 19;
EXEC InsertSubjectFootnote @subject_absence_number_missing, 19;
EXEC InsertSubjectFootnote @subject_absence_rate_percent_bands, 19;

EXEC InsertSubjectFootnote @subject_absence_by_characteristic, 20;
EXEC InsertSubjectFootnote @subject_absence_by_geographic_level, 20;
EXEC InsertSubjectFootnote @subject_absence_by_term, 20;
EXEC InsertSubjectFootnote @subject_absence_for_four_year_olds, 20;
EXEC InsertSubjectFootnote @subject_absence_in_prus, 20;
EXEC InsertSubjectFootnote @subject_absence_number_missing, 20;
EXEC InsertSubjectFootnote @subject_absence_rate_percent_bands, 20;

EXEC InsertSubjectFootnote @subject_absence_by_characteristic, 21;
EXEC InsertSubjectFootnote @subject_absence_by_geographic_level, 21;
EXEC InsertSubjectFootnote @subject_absence_by_term, 21;
EXEC InsertSubjectFootnote @subject_absence_for_four_year_olds, 21;
EXEC InsertSubjectFootnote @subject_absence_in_prus, 21;
EXEC InsertSubjectFootnote @subject_absence_number_missing, 21;
EXEC InsertSubjectFootnote @subject_absence_rate_percent_bands, 21;

EXEC InsertSubjectFootnote @subject_absence_by_characteristic, 22;
EXEC InsertSubjectFootnote @subject_absence_by_geographic_level, 22;
EXEC InsertSubjectFootnote @subject_absence_by_term, 22;
EXEC InsertSubjectFootnote @subject_absence_for_four_year_olds, 22;
EXEC InsertSubjectFootnote @subject_absence_in_prus, 22;
EXEC InsertSubjectFootnote @subject_absence_number_missing, 22;
EXEC InsertSubjectFootnote @subject_absence_rate_percent_bands, 22;

EXEC InsertSubjectFootnote @subject_absence_by_characteristic, 23;
EXEC InsertSubjectFootnote @subject_absence_by_geographic_level, 23;
EXEC InsertSubjectFootnote @subject_absence_by_term, 23;
EXEC InsertSubjectFootnote @subject_absence_for_four_year_olds, 23;
EXEC InsertSubjectFootnote @subject_absence_in_prus, 23;
EXEC InsertSubjectFootnote @subject_absence_number_missing, 23;
EXEC InsertSubjectFootnote @subject_absence_rate_percent_bands, 23;

EXEC InsertSubjectFootnote @subject_absence_for_four_year_olds, 24;

EXEC InsertSubjectFootnote @subject_absence_for_four_year_olds, 25;

--
-- Indicators
--


DECLARE @indicator_sess_overall_percent NVARCHAR(max) = 'sess_overall_percent';
DECLARE @indicator_sess_authorised_percent NVARCHAR(max) = 'sess_authorised_percent';
DECLARE @indicator_sess_unauthorised_percent NVARCHAR(max) = 'sess_unauthorised_percent';
DECLARE @indicator_enrolments_pa_10_exact NVARCHAR(max) = 'enrolments_pa_10_exact';
DECLARE @indicator_enrolments_pa_10_exact_percent NVARCHAR(max) = 'enrolments_pa_10_exact_percent';
DECLARE @indicator_sess_possible_pa_10_exact NVARCHAR(max) = 'sess_possible_pa_10_exact';
DECLARE @indicator_sess_overall_pa_10_exact NVARCHAR(max) = 'sess_overall_pa_10_exact';
DECLARE @indicator_sess_authorised_pa_10_exact NVARCHAR(max) = 'sess_authorised_pa_10_exact';
DECLARE @indicator_sess_unauthorised_pa_10_exact NVARCHAR(max) = 'sess_unauthorised_pa_10_exact';
DECLARE @indicator_sess_overall_percent_pa_10_exact NVARCHAR(max) = 'sess_overall_percent_pa_10_exact';
DECLARE @indicator_sess_authorised_percent_pa_10_exact NVARCHAR(max) = 'sess_authorised_percent_pa_10_exact';
DECLARE @indicator_sess_unauthorised_percent_pa_10_exact NVARCHAR(max) = 'sess_unauthorised_percent_pa_10_exact';
DECLARE @indicator_enrolments_pa10_exact_percent NVARCHAR(max) = 'enrolments_pa10_exact_percent';
DECLARE @indicator_enrolments NVARCHAR(max) = 'enrolments';
DECLARE @indicator_enrol_unauth_late NVARCHAR(max) = 'enrol_unauth_late';
DECLARE @indicator_enrol_unauth_holiday NVARCHAR(max) = 'enrol_unauth_holiday';
DECLARE @indicator_enrol_auth_other NVARCHAR(max) = 'enrol_auth_other';
DECLARE @indicator_enrol_auth_excluded NVARCHAR(max) = 'enrol_auth_excluded';
DECLARE @indicator_enrol_auth_ext_holiday NVARCHAR(max) = 'enrol_auth_ext_holiday';
DECLARE @indicator_enrol_auth_holiday NVARCHAR(max) = 'enrol_auth_holiday';
DECLARE @indicator_enrol_unauth_other NVARCHAR(max) = 'enrol_unauth_other';
DECLARE @indicator_enrol_auth_traveller NVARCHAR(max) = 'enrol_auth_traveller';
DECLARE @indicator_enrol_auth_religious NVARCHAR(max) = 'enrol_auth_religious';
DECLARE @indicator_enrol_auth_appointments NVARCHAR(max) = 'enrol_auth_appointments';
DECLARE @indicator_enrol_auth_illness NVARCHAR(max) = 'enrol_auth_illness';
DECLARE @indicator_enrol_unauthorised NVARCHAR(max) = 'enrol_unauthorised';
DECLARE @indicator_enrol_authorised NVARCHAR(max) = 'enrol_authorised';
DECLARE @indicator_enrol_overall NVARCHAR(max) = 'enrol_overall';
DECLARE @indicator_enrol_auth_study NVARCHAR(max) = 'enrol_auth_study';
DECLARE @indicator_enrol_unauth_noyet NVARCHAR(max) = 'enrol_unauth_noyet';
DECLARE @indicator_enrolments_overall_percent NVARCHAR(max) = 'enrolments_overall_percent';
DECLARE @indicator_enrolments_authorised_percent NVARCHAR(max) = 'enrolments_authorised_percent';
DECLARE @indicator_enrolments_unauthorised_percent NVARCHAR(max) = 'enrolments_unauthorised_percent';
DECLARE @indicator_num_schools NVARCHAR(max) = 'num_schools';
DECLARE @indicator_sess_overall NVARCHAR(max) = 'sess_overall';

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_overall_percent, 8;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_overall_percent, 8;
EXEC InsertIndicatorFootnote @subject_absence_by_term, @indicator_sess_overall_percent, 8;
EXEC InsertIndicatorFootnote @subject_absence_for_four_year_olds, @indicator_sess_overall_percent, 8;
EXEC InsertIndicatorFootnote @subject_absence_in_prus, @indicator_sess_overall_percent, 8;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_authorised_percent, 8;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_authorised_percent, 8;
EXEC InsertIndicatorFootnote @subject_absence_by_term, @indicator_sess_authorised_percent, 8;
EXEC InsertIndicatorFootnote @subject_absence_in_prus, @indicator_sess_authorised_percent, 8;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_unauthorised_percent, 8;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_unauthorised_percent, 8;
EXEC InsertIndicatorFootnote @subject_absence_by_term, @indicator_sess_unauthorised_percent, 8;
EXEC InsertIndicatorFootnote @subject_absence_in_prus, @indicator_sess_unauthorised_percent, 8;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_enrolments_pa_10_exact, 9;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_enrolments_pa_10_exact, 9;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_enrolments_pa_10_exact_percent, 9;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_enrolments_pa_10_exact_percent, 9;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_possible_pa_10_exact, 9;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_possible_pa_10_exact, 9;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_overall_pa_10_exact, 9;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_overall_pa_10_exact, 9;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_authorised_pa_10_exact, 9;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_authorised_pa_10_exact, 9;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_unauthorised_pa_10_exact, 9;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_unauthorised_pa_10_exact, 9;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_overall_percent_pa_10_exact, 9;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_overall_percent_pa_10_exact, 9;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_authorised_percent_pa_10_exact, 9;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_authorised_percent_pa_10_exact, 9;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_unauthorised_percent_pa_10_exact, 9;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_unauthorised_percent_pa_10_exact, 9;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_enrolments_pa_10_exact, 10;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_enrolments_pa_10_exact, 10;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_enrolments_pa_10_exact_percent, 10;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_enrolments_pa_10_exact_percent, 10;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_possible_pa_10_exact, 10;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_possible_pa_10_exact, 10;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_overall_pa_10_exact, 10;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_overall_pa_10_exact, 10;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_authorised_pa_10_exact, 10;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_authorised_pa_10_exact, 10;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_unauthorised_pa_10_exact, 10;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_unauthorised_pa_10_exact, 10;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_overall_percent_pa_10_exact, 10;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_overall_percent_pa_10_exact, 10;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_authorised_percent_pa_10_exact, 10;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_authorised_percent_pa_10_exact, 10;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_unauthorised_percent_pa_10_exact, 10;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_unauthorised_percent_pa_10_exact, 10;

EXEC InsertIndicatorFootnote @subject_absence_in_prus, @indicator_enrolments_pa10_exact_percent, 11;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_enrolments, 12;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_enrolments, 12;
EXEC InsertIndicatorFootnote @subject_absence_in_prus, @indicator_enrolments, 12;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrolments, 12;

EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_unauth_late, 13;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_unauth_holiday, 13;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_auth_other, 13;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_auth_excluded, 13;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_auth_ext_holiday, 13;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_auth_holiday, 13;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_unauth_other, 13;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_auth_traveller, 13;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_auth_religious, 13;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_auth_appointments, 13;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_auth_illness, 13;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_unauthorised, 13;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_authorised, 13;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_overall, 13;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_auth_study, 13;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_unauth_noyet, 13;

EXEC InsertIndicatorFootnote @subject_absence_rate_percent_bands, @indicator_enrolments_overall_percent, 14;
EXEC InsertIndicatorFootnote @subject_absence_rate_percent_bands, @indicator_enrolments_authorised_percent, 14;
EXEC InsertIndicatorFootnote @subject_absence_rate_percent_bands, @indicator_enrolments_unauthorised_percent, 14;

EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_num_schools, 15;
EXEC InsertIndicatorFootnote @subject_absence_for_four_year_olds, @indicator_num_schools, 15
EXEC InsertIndicatorFootnote @subject_absence_in_prus, @indicator_num_schools, 15;

EXEC InsertIndicatorFootnote @subject_absence_for_four_year_olds, @indicator_enrolments, 24;

EXEC InsertIndicatorFootnote @subject_absence_for_four_year_olds, @indicator_sess_overall, 25;
EXEC InsertIndicatorFootnote @subject_absence_for_four_year_olds, @indicator_sess_overall_percent, 25;


--
-- Filter Items
--


DECLARE @filterItem_State_funded_primary NVARCHAR(max) = 'State-funded primary';
DECLARE @filterItem_State_funded_secondary NVARCHAR(max) = 'State-funded secondary';
DECLARE @filterItem_Special NVARCHAR(max) = 'Special';
DECLARE @filterItem_Social_emotional_and_mental_health NVARCHAR(max) = 'SEN primary need Social emotional and mental health';

EXEC InsertFilterItemFootnote @subject_absence_by_characteristic, @filterItem_State_funded_primary, 1;
EXEC InsertFilterItemFootnote @subject_absence_by_geographic_level, @filterItem_State_funded_primary, 1;
EXEC InsertFilterItemFootnote @subject_absence_by_term, @filterItem_State_funded_primary, 1;
EXEC InsertFilterItemFootnote @subject_absence_for_four_year_olds, @filterItem_State_funded_primary, 1;
EXEC InsertFilterItemFootnote @subject_absence_number_missing, @filterItem_State_funded_primary, 1;
EXEC InsertFilterItemFootnote @subject_absence_rate_percent_bands, @filterItem_State_funded_primary, 1;

EXEC InsertFilterItemFootnote @subject_absence_by_characteristic, @filterItem_State_funded_secondary, 1;
EXEC InsertFilterItemFootnote @subject_absence_by_geographic_level, @filterItem_State_funded_secondary, 1;
EXEC InsertFilterItemFootnote @subject_absence_by_term, @filterItem_State_funded_secondary, 1;
EXEC InsertFilterItemFootnote @subject_absence_for_four_year_olds, @filterItem_State_funded_secondary, 1;
EXEC InsertFilterItemFootnote @subject_absence_number_missing, @filterItem_State_funded_secondary, 1;
EXEC InsertFilterItemFootnote @subject_absence_rate_percent_bands, @filterItem_State_funded_secondary, 1;

EXEC InsertFilterItemFootnote @subject_absence_by_characteristic, @filterItem_State_funded_primary, 2;
EXEC InsertFilterItemFootnote @subject_absence_by_geographic_level, @filterItem_State_funded_primary, 2;
EXEC InsertFilterItemFootnote @subject_absence_by_term, @filterItem_State_funded_primary, 2;
EXEC InsertFilterItemFootnote @subject_absence_for_four_year_olds, @filterItem_State_funded_primary, 2;
EXEC InsertFilterItemFootnote @subject_absence_number_missing, @filterItem_State_funded_primary, 2;
EXEC InsertFilterItemFootnote @subject_absence_rate_percent_bands, @filterItem_State_funded_primary, 2;

EXEC InsertFilterItemFootnote @subject_absence_by_characteristic, @filterItem_State_funded_secondary, 3;
EXEC InsertFilterItemFootnote @subject_absence_by_geographic_level, @filterItem_State_funded_secondary, 3;
EXEC InsertFilterItemFootnote @subject_absence_by_term, @filterItem_State_funded_secondary, 3;
EXEC InsertFilterItemFootnote @subject_absence_for_four_year_olds, @filterItem_State_funded_secondary, 3;
EXEC InsertFilterItemFootnote @subject_absence_number_missing, @filterItem_State_funded_secondary, 3;
EXEC InsertFilterItemFootnote @subject_absence_rate_percent_bands, @filterItem_State_funded_secondary, 3;

EXEC InsertFilterItemFootnote @subject_absence_by_characteristic, @filterItem_Special, 4;
EXEC InsertFilterItemFootnote @subject_absence_by_geographic_level, @filterItem_Special, 4;
EXEC InsertFilterItemFootnote @subject_absence_by_term, @filterItem_Special, 4;
EXEC InsertFilterItemFootnote @subject_absence_for_four_year_olds, @filterItem_Special, 4;
EXEC InsertFilterItemFootnote @subject_absence_number_missing, @filterItem_Special, 4;
EXEC InsertFilterItemFootnote @subject_absence_rate_percent_bands, @filterItem_Special, 4;

EXEC InsertFilterItemFootnote @subject_absence_by_characteristic, @filterItem_Social_emotional_and_mental_health, 23;


--
-- Filters
--


DECLARE @filter_Characteristic NVARCHAR(max) = 'Characteristic';

EXEC InsertFilterFootnote @subject_absence_by_characteristic, @filter_Characteristic, 16;
EXEC InsertFilterFootnote @subject_absence_by_characteristic, @filter_Characteristic, 17;


--
-- Filter Groups
--


DECLARE @filterGroup_SEN_provision NVARCHAR(max) = 'SEN provision';
DECLARE @filterGroup_Ethnic_group_minor NVARCHAR(max) = 'Ethnic group minor';
DECLARE @filterGroup_SEN_primary_need NVARCHAR(max) = 'SEN primary need';

EXEC InsertFilterGroupFootnote @subject_absence_by_characteristic, @filterGroup_SEN_provision, 18;
EXEC InsertFilterGroupFootnote @subject_absence_by_characteristic, @filterGroup_SEN_provision, 19;
EXEC InsertFilterGroupFootnote @subject_absence_by_characteristic, @filterGroup_SEN_provision, 20;
EXEC InsertFilterGroupFootnote @subject_absence_by_characteristic, @filterGroup_Ethnic_group_minor, 21;
EXEC InsertFilterGroupFootnote @subject_absence_by_characteristic, @filterGroup_SEN_primary_need, 22;

DROP PROCEDURE InsertSubjectFootnote;
DROP PROCEDURE InsertIndicatorFootnote;
DROP PROCEDURE InsertFilterFootnote;
DROP PROCEDURE InsertFilterGroupFootnote;
DROP PROCEDURE InsertFilterItemFootnote;