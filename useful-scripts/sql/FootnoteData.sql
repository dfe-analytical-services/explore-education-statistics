CREATE OR ALTER PROCEDURE InsertSubjectFootnote
    @subjectName NVARCHAR(max),
    @footnoteId uniqueidentifier
AS
DECLARE @subjectId uniqueidentifier = (SELECT S.Id FROM Subject S WHERE S.Name = @subjectName);
BEGIN TRY
    INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subjectId, @footnoteId);
END TRY
BEGIN CATCH
    DECLARE @errorMessage NVARCHAR(max) = ERROR_MESSAGE();
    DECLARE @errorSeverity INT = ERROR_SEVERITY();
    DECLARE @errorState INT = ERROR_SEVERITY();
    DECLARE @errorProcedure NVARCHAR(max) = ERROR_PROCEDURE();
    DECLARE @KEY NVARCHAR(50) = CONVERT(nvarchar(50), @footnoteId);
    RAISERROR (N'Error executing %s(SUBJECT: %s, FOOTNOTE: %uniq) MESSAGE: %s', @errorSeverity, @errorState, @errorProcedure, @subjectName, @KEY, @errorMessage);
END CATCH
GO;

CREATE OR ALTER PROCEDURE InsertIndicatorFootnote
    @subjectName NVARCHAR(max),
    @indicatorName NVARCHAR(max),
    @footnoteId uniqueidentifier
AS
DECLARE
    @indicatorId uniqueidentifier = (SELECT I.Id FROM Indicator I JOIN IndicatorGroup IG on I.IndicatorGroupId = IG.Id JOIN Subject S on IG.SubjectId = S.Id WHERE S.Name = @subjectName AND I.Name = @indicatorName);
BEGIN TRY
    INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (@indicatorId, @footnoteId);
END TRY
BEGIN CATCH
    DECLARE @errorMessage NVARCHAR(max) = ERROR_MESSAGE();
    DECLARE @errorSeverity INT = ERROR_SEVERITY();
    DECLARE @errorState INT = ERROR_SEVERITY();
    DECLARE @errorProcedure NVARCHAR(max) = ERROR_PROCEDURE();
    DECLARE @KEY NVARCHAR(50) = CONVERT(nvarchar(50), @footnoteId);
    RAISERROR (N'Error executing %s(SUBJECT: %s, INDICATOR: %s, FOOTNOTE: %s) MESSAGE: %s', @errorSeverity, @errorState, @errorProcedure, @subjectName, @indicatorName, @KEY, @errorMessage);
END CATCH
GO;

CREATE OR ALTER PROCEDURE InsertFilterFootnote
    @subjectName NVARCHAR(max),
    @filterName NVARCHAR(max),
    @footnoteId uniqueidentifier
AS
DECLARE
    @filterId uniqueidentifier = (SELECT F.Id FROM Filter F JOIN Subject S on F.SubjectId = S.Id WHERE S.Name = @subjectName AND F.Name = @filterName);
BEGIN TRY
    INSERT INTO FilterFootnote (FilterId, FootnoteId) VALUES (@filterId, @footnoteId);
END TRY
BEGIN CATCH
    DECLARE @errorMessage NVARCHAR(max) = ERROR_MESSAGE();
    DECLARE @errorSeverity INT = ERROR_SEVERITY();
    DECLARE @errorState INT = ERROR_SEVERITY();
    DECLARE @errorProcedure NVARCHAR(max) = ERROR_PROCEDURE();
    DECLARE @KEY NVARCHAR(50) = CONVERT(nvarchar(50), @footnoteId);
    RAISERROR (N'Error executing %s(SUBJECT: %s, FILTER: %s, FOOTNOTE: %s) MESSAGE: %s', @errorSeverity, @errorState, @errorProcedure, @subjectName, @filterName, @KEY, @errorMessage);
END CATCH
GO;

CREATE OR ALTER PROCEDURE InsertFilterGroupFootnote
    @subjectName NVARCHAR(max),
    @filterGroupName NVARCHAR(max),
    @footnoteId uniqueidentifier
AS
DECLARE
    @filterGroupId uniqueidentifier = (SELECT FG.Id FROM FilterGroup FG JOIN Filter F ON FG.FilterId = F.Id JOIN Subject S on F.SubjectId = S.Id WHERE S.Name = @subjectName AND FG.Label = @filterGroupName);
BEGIN TRY
    INSERT INTO FilterGroupFootnote (FilterGroupId, FootnoteId) VALUES (@filterGroupId, @footnoteId);
END TRY
BEGIN CATCH
    DECLARE @errorMessage NVARCHAR(max) = ERROR_MESSAGE();
    DECLARE @errorSeverity INT = ERROR_SEVERITY();
    DECLARE @errorState INT = ERROR_SEVERITY();
    DECLARE @errorProcedure NVARCHAR(max) = ERROR_PROCEDURE();
    DECLARE @KEY NVARCHAR(50) = CONVERT(nvarchar(50), @footnoteId);
    RAISERROR (N'Error executing %s(SUBJECT: %s, FILTER GROUP: %s, FOOTNOTE: %s) MESSAGE: %s', @errorSeverity, @errorState, @errorProcedure, @subjectName, @filterGroupName, @KEY, @errorMessage);
END CATCH
GO;

CREATE OR ALTER PROCEDURE InsertFilterItemFootnote
    @subjectName NVARCHAR(max),
    @filterItemName NVARCHAR(max),
    @footnoteId uniqueidentifier
AS
DECLARE
    @filterItemId uniqueidentifier = (SELECT FI.Id FROM FilterItem FI JOIN FilterGroup FG ON FI.FilterGroupId = FG.Id JOIN Filter F on FG.FilterId = F.Id  JOIN Subject S on F.SubjectId = S.Id WHERE S.Name = @subjectName AND FI.Label = @filterItemName);
BEGIN TRY
    INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (@filterItemId, @footnoteId);
END TRY
BEGIN CATCH
    DECLARE @errorMessage NVARCHAR(max) = ERROR_MESSAGE();
    DECLARE @errorSeverity INT = ERROR_SEVERITY();
    DECLARE @errorState INT = ERROR_SEVERITY();
    DECLARE @errorProcedure NVARCHAR(max) = ERROR_PROCEDURE();
    DECLARE @KEY NVARCHAR(50) = CONVERT(nvarchar(50), @footnoteId);
    RAISERROR (N'Error executing %s(SUBJECT: %s, FILTER ITEM: %s, FOOTNOTE: %s) MESSAGE: %s', @errorSeverity, @errorState, @errorProcedure, @subjectName, @filterItemName, @KEY, @errorMessage);
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
DECLARE @footnote_id_1 uniqueidentifier = 'fbb6262f-213a-453a-98ca-b832d6ae1c16';
DECLARE @footnote_id_2 uniqueidentifier = 'e9076bab-6ff7-4c92-8972-2fb4affbe977';
DECLARE @footnote_id_3 uniqueidentifier = '128c6153-4937-40da-a99f-e137fc8c41e5';
DECLARE @footnote_id_4 uniqueidentifier = '41c1eb5a-8415-45eb-bc49-cfa42382ebba';
DECLARE @footnote_id_5 uniqueidentifier = '6c06f733-c30b-45e1-980a-b587e923f73b';
DECLARE @footnote_id_6 uniqueidentifier = 'b678373f-dfa2-41ad-817e-3f011d1b5173';
DECLARE @footnote_id_7 uniqueidentifier = '7fabae1a-5cc0-4a1a-861a-3ddd8ec0f7b2';
DECLARE @footnote_id_8 uniqueidentifier = '78c78049-22b3-4ec8-bc2a-7f47a6f7c74c';
DECLARE @footnote_id_9 uniqueidentifier = '7b18666a-ff4a-43a9-9639-8f324374db0c';
DECLARE @footnote_id_10 uniqueidentifier = '507b05b0-4015-4330-9901-20b47c5b154d';
DECLARE @footnote_id_11 uniqueidentifier = '82aa9459-2ff3-45b6-ba98-6d8819cea9f6';
DECLARE @footnote_id_12 uniqueidentifier = '86ec4e28-0f68-42f3-b153-d67e611aa6f1';
DECLARE @footnote_id_13 uniqueidentifier = '55f10d5a-792b-4065-9e32-446d29f43c7f';
DECLARE @footnote_id_14 uniqueidentifier = '7b32b523-e6c0-4091-91d1-b515550689d1';
DECLARE @footnote_id_15 uniqueidentifier = 'e03c3b82-75df-4dee-b3ed-dac39378a9b5';
DECLARE @footnote_id_16 uniqueidentifier = '96641ff5-d33b-495d-8691-8045c148a595';
DECLARE @footnote_id_17 uniqueidentifier = '6176a831-fdc9-4645-915e-4051a93ba05a';
DECLARE @footnote_id_18 uniqueidentifier = 'e8ebee03-dc9d-4ae8-8b5e-a7ff831611f0';
DECLARE @footnote_id_19 uniqueidentifier = '65fcb08f-e70a-4cf1-920b-5e326fbaa8b4';
DECLARE @footnote_id_20 uniqueidentifier = 'a9045d29-7c84-491d-918c-3885f11597d3';
DECLARE @footnote_id_21 uniqueidentifier = '2268f97a-5107-464f-ade6-30debc36519a';
DECLARE @footnote_id_22 uniqueidentifier = '8a79e8c6-3552-496a-8790-d072cec8c43f';

INSERT INTO Footnote (Id, Content, ReleaseId)
VALUES (@footnote_id_1, 'State-funded primary schools include all primary academies, including free schools.', '4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5');

INSERT INTO Footnote (Id, Content, ReleaseId)
VALUES (@footnote_id_2,
        'State-funded secondary schools include city technology colleges and all secondary academies, including all-through academies and free schools.', '4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5');

INSERT INTO Footnote (Id, Content, ReleaseId)
VALUES (@footnote_id_3,
        'Special schools include maintained special schools, non-maintained special schools and special academies.  Excludes general hospital schools, independent special schools and independent schools approved for SEN pupils.', '4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5');

INSERT INTO Footnote (Id, Content, ReleaseId)
VALUES (@footnote_id_4,
        'Totals may not appear to equal the sum of component parts because numbers have been rounded to the nearest 5.', '4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5');

INSERT INTO Footnote (Id, Content, ReleaseId)
VALUES (@footnote_id_5, 'x - 1 or 2 enrolments, or a percentage based on 1 or 2 enrolments.', '4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5');

INSERT INTO Footnote (Id, Content, ReleaseId)
VALUES (@footnote_id_6,
        'There may be discrepancies between totals and the sum of constituent parts  as national and regional totals and totals across school types have been rounded to the nearest 5.', '4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5');

INSERT INTO Footnote (Id, Content, ReleaseId)
VALUES (@footnote_id_7,
        'Absence rates are the number of absence sessions expressed as a percentage of the total number of possible sessions.', '4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5');

INSERT INTO Footnote (Id, Content, ReleaseId)
VALUES (@footnote_id_8,
        'Pupil enrolments missing 10 percent or more of their own possible sessions (due to authorised or unauthorised absence) are classified as persistent absentees.', '4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5');

INSERT INTO Footnote (Id, Content, ReleaseId)
VALUES (@footnote_id_9,
        'The definition of persistent absence changed from the 2015/16 academic year - The information presented for years prior to 2015/16 has been produced using the same methodology in order to allow users to make comparisons on a consistent basis over time.', '4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5');

INSERT INTO Footnote (Id, Content, ReleaseId)
VALUES (@footnote_id_10,
        'The persistent absentee rate is the number of persistent absentees expressed as a percentage of the total number of enrolments.', '4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5');

INSERT INTO Footnote (Id, Content, ReleaseId)
VALUES (@footnote_id_11,
        'Number of enrolments includes pupils on the school roll for at least one session who are aged between 5 and 15, excluding boarders. Some pupils may be counted more than once (if they moved schools during the academic year or are registered in more than one school).', '4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5');

INSERT INTO Footnote (Id, Content, ReleaseId)
VALUES (@footnote_id_12, 'The number of enrolments with one or more session of absence for specific reason.', '4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5');

INSERT INTO Footnote (Id, Content, ReleaseId)
VALUES (@footnote_id_13,
        'The number of sessions missed in each band expressed as a percentage of the total number of sessions missed for that category of absence overall.', '4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5');

INSERT INTO Footnote (Id, Content, ReleaseId)
VALUES (@footnote_id_14, 'Includes all schools with at least six enrolments who reported absence information via the school census for the associated academic year.', '4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5');

INSERT INTO Footnote (Id, Content, ReleaseId)
VALUES (@footnote_id_15,
        'See "Guide to absence statistics" for more information on how absence and pupil characteristic data have been linked.', '4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5');

INSERT INTO Footnote (Id, Content, ReleaseId)
VALUES (@footnote_id_16,
        'Figures for pupils with unclassified or missing characteristics information should be interpreted with caution.', '4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5');

INSERT INTO Footnote (Id, Content, ReleaseId)
VALUES (@footnote_id_17,
        'In September 2014 the Special Educational Needs and Disability (SEND) provisions were introduced in the Children and Families Act 2014. This reform caused changes to SEN categories - Education, Health and Care (EHC) plans were introduced and SEN support replaces school action and school action plus but some pupils remain with these provision types in first year of transition (2014/15).', '4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5');

INSERT INTO Footnote (Id, Content, ReleaseId)
VALUES (@footnote_id_18,
        'Minority ethnic group includes pupils who have been classified according to their ethnic group, excluding White British.', '4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5');

INSERT INTO Footnote (Id, Content, ReleaseId)
VALUES (@footnote_id_19,
        'From 2015 SEN support replaced school action and School Action plus but some pupils remain with these provision types. Those who were formerly School Action did not provide type of need in previous years. Those who remain on school action provision are not included here but have been included within the SEN support category in other tables.', '4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5');

INSERT INTO Footnote (Id, Content, ReleaseId)
VALUES (@footnote_id_20,
        'Social Emotional and Mental Health was added as a new type of need in 2015, the previous type of need Behaviour, Emotional and Social Difficulties has been removed although it is not expected it should be a direct replacement.', '4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5');

INSERT INTO Footnote (Id, Content, ReleaseId)
VALUES (@footnote_id_21,
        'Includes pupils on the school roll for at least one session who are aged 4, excluding boarders. Some pupils may be counted more than once (if they moved schools during the academic year or are registered at more than one school).', '4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5');

INSERT INTO Footnote (Id, Content, ReleaseId)
VALUES (@footnote_id_22,
        'Only overall absence is recorded for pupils aged 4, absences are not categorised as authorised or unauthorised.', '4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5');

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

EXEC InsertSubjectFootnote @subject_absence_by_characteristic, @footnote_id_4;
EXEC InsertSubjectFootnote @subject_absence_by_geographic_level, @footnote_id_4;
EXEC InsertSubjectFootnote @subject_absence_by_term, @footnote_id_4;
EXEC InsertSubjectFootnote @subject_absence_for_four_year_olds, @footnote_id_4;
EXEC InsertSubjectFootnote @subject_absence_in_prus, @footnote_id_4;
EXEC InsertSubjectFootnote @subject_absence_number_missing, @footnote_id_4;
EXEC InsertSubjectFootnote @subject_absence_rate_percent_bands, @footnote_id_4;

EXEC InsertSubjectFootnote @subject_absence_by_characteristic, @footnote_id_5;
EXEC InsertSubjectFootnote @subject_absence_by_geographic_level, @footnote_id_5;
EXEC InsertSubjectFootnote @subject_absence_by_term, @footnote_id_5;
EXEC InsertSubjectFootnote @subject_absence_for_four_year_olds, @footnote_id_5;
EXEC InsertSubjectFootnote @subject_absence_in_prus, @footnote_id_5;
EXEC InsertSubjectFootnote @subject_absence_number_missing, @footnote_id_5;
EXEC InsertSubjectFootnote @subject_absence_rate_percent_bands, @footnote_id_5;

EXEC InsertSubjectFootnote @subject_absence_by_characteristic, @footnote_id_6;
EXEC InsertSubjectFootnote @subject_absence_by_geographic_level, @footnote_id_6;
EXEC InsertSubjectFootnote @subject_absence_by_term, @footnote_id_6;
EXEC InsertSubjectFootnote @subject_absence_for_four_year_olds, @footnote_id_6;
EXEC InsertSubjectFootnote @subject_absence_in_prus, @footnote_id_6;
EXEC InsertSubjectFootnote @subject_absence_number_missing, @footnote_id_6;
EXEC InsertSubjectFootnote @subject_absence_rate_percent_bands, @footnote_id_6;

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

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_overall_percent, @footnote_id_7;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_overall_percent, @footnote_id_7;
EXEC InsertIndicatorFootnote @subject_absence_by_term, @indicator_sess_overall_percent, @footnote_id_7;
EXEC InsertIndicatorFootnote @subject_absence_for_four_year_olds, @indicator_sess_overall_percent, @footnote_id_7;
EXEC InsertIndicatorFootnote @subject_absence_in_prus, @indicator_sess_overall_percent, @footnote_id_7;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_authorised_percent, @footnote_id_7;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_authorised_percent, @footnote_id_7;
EXEC InsertIndicatorFootnote @subject_absence_by_term, @indicator_sess_authorised_percent, @footnote_id_7;
EXEC InsertIndicatorFootnote @subject_absence_in_prus, @indicator_sess_authorised_percent, @footnote_id_7;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_unauthorised_percent, @footnote_id_7;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_unauthorised_percent, @footnote_id_7;
EXEC InsertIndicatorFootnote @subject_absence_by_term, @indicator_sess_unauthorised_percent, @footnote_id_7;
EXEC InsertIndicatorFootnote @subject_absence_in_prus, @indicator_sess_unauthorised_percent, @footnote_id_7;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_enrolments_pa_10_exact, @footnote_id_8;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_enrolments_pa_10_exact, @footnote_id_8;
EXEC InsertIndicatorFootnote @subject_absence_in_prus, @indicator_enrolments_pa_10_exact, @footnote_id_8;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_enrolments_pa_10_exact_percent, @footnote_id_8;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_enrolments_pa_10_exact_percent, @footnote_id_8;
EXEC InsertIndicatorFootnote @subject_absence_in_prus, @indicator_enrolments_pa_10_exact_percent, @footnote_id_8;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_possible_pa_10_exact, @footnote_id_8;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_possible_pa_10_exact, @footnote_id_8;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_overall_pa_10_exact, @footnote_id_8;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_overall_pa_10_exact, @footnote_id_8;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_authorised_pa_10_exact, @footnote_id_8;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_authorised_pa_10_exact, @footnote_id_8;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_unauthorised_pa_10_exact, @footnote_id_8;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_unauthorised_pa_10_exact, @footnote_id_8;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_overall_percent_pa_10_exact, @footnote_id_8;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_overall_percent_pa_10_exact, @footnote_id_8;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_authorised_percent_pa_10_exact, @footnote_id_8;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_authorised_percent_pa_10_exact, @footnote_id_8;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_unauthorised_percent_pa_10_exact, @footnote_id_8;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_unauthorised_percent_pa_10_exact, @footnote_id_8;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_enrolments_pa_10_exact, @footnote_id_9;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_enrolments_pa_10_exact, @footnote_id_9;
EXEC InsertIndicatorFootnote @subject_absence_in_prus, @indicator_enrolments_pa_10_exact, @footnote_id_9;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_enrolments_pa_10_exact_percent, @footnote_id_9;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_enrolments_pa_10_exact_percent, @footnote_id_9;
EXEC InsertIndicatorFootnote @subject_absence_in_prus, @indicator_enrolments_pa_10_exact_percent, @footnote_id_9;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_possible_pa_10_exact, @footnote_id_9;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_possible_pa_10_exact, @footnote_id_9;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_overall_pa_10_exact, @footnote_id_9;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_overall_pa_10_exact, @footnote_id_9;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_authorised_pa_10_exact, @footnote_id_9;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_authorised_pa_10_exact, @footnote_id_9;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_unauthorised_pa_10_exact, @footnote_id_9;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_unauthorised_pa_10_exact, @footnote_id_9;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_overall_percent_pa_10_exact, @footnote_id_9;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_overall_percent_pa_10_exact, @footnote_id_9;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_authorised_percent_pa_10_exact, @footnote_id_9;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_authorised_percent_pa_10_exact, @footnote_id_9;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_sess_unauthorised_percent_pa_10_exact, @footnote_id_9;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_sess_unauthorised_percent_pa_10_exact, @footnote_id_9;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_enrolments_pa_10_exact_percent, @footnote_id_10;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_enrolments_pa_10_exact_percent, @footnote_id_10;
EXEC InsertIndicatorFootnote @subject_absence_in_prus, @indicator_enrolments_pa_10_exact_percent, @footnote_id_10;

EXEC InsertIndicatorFootnote @subject_absence_by_characteristic, @indicator_enrolments, @footnote_id_11;
EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_enrolments, @footnote_id_11;
EXEC InsertIndicatorFootnote @subject_absence_in_prus, @indicator_enrolments, @footnote_id_11;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrolments, @footnote_id_11;

EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_unauth_late, @footnote_id_12;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_unauth_holiday, @footnote_id_12;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_auth_other, @footnote_id_12;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_auth_excluded, @footnote_id_12;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_auth_ext_holiday, @footnote_id_12;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_auth_holiday, @footnote_id_12;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_unauth_other, @footnote_id_12;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_auth_traveller, @footnote_id_12;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_auth_religious, @footnote_id_12;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_auth_appointments, @footnote_id_12;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_auth_illness, @footnote_id_12;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_unauthorised, @footnote_id_12;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_authorised, @footnote_id_12;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_overall, @footnote_id_12;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_auth_study, @footnote_id_12;
EXEC InsertIndicatorFootnote @subject_absence_number_missing, @indicator_enrol_unauth_noyet, @footnote_id_12;

EXEC InsertIndicatorFootnote @subject_absence_rate_percent_bands, @indicator_enrolments_overall_percent, @footnote_id_13;
EXEC InsertIndicatorFootnote @subject_absence_rate_percent_bands, @indicator_enrolments_authorised_percent, @footnote_id_13;
EXEC InsertIndicatorFootnote @subject_absence_rate_percent_bands, @indicator_enrolments_unauthorised_percent, @footnote_id_13;

EXEC InsertIndicatorFootnote @subject_absence_by_geographic_level, @indicator_num_schools, @footnote_id_14;
EXEC InsertIndicatorFootnote @subject_absence_for_four_year_olds, @indicator_num_schools, @footnote_id_14
EXEC InsertIndicatorFootnote @subject_absence_in_prus, @indicator_num_schools, @footnote_id_14;

EXEC InsertIndicatorFootnote @subject_absence_for_four_year_olds, @indicator_enrolments, @footnote_id_21;

EXEC InsertIndicatorFootnote @subject_absence_for_four_year_olds, @indicator_sess_overall, @footnote_id_22;
EXEC InsertIndicatorFootnote @subject_absence_for_four_year_olds, @indicator_sess_overall_percent, @footnote_id_22;


--
-- Filter Items
--


DECLARE @filterItem_State_funded_primary NVARCHAR(max) = 'State-funded primary';
DECLARE @filterItem_State_funded_secondary NVARCHAR(max) = 'State-funded secondary';
DECLARE @filterItem_Special NVARCHAR(max) = 'Special';
DECLARE @filterItem_Social_emotional_and_mental_health NVARCHAR(max) = 'SEN primary need Social emotional and mental health';
DECLARE @filterItem_Ethnicity_Minority_Ethnic_Group NVARCHAR(max) = 'Ethnicity Minority Ethnic Group';

EXEC InsertFilterItemFootnote @subject_absence_by_characteristic, @filterItem_State_funded_primary, @footnote_id_1;
EXEC InsertFilterItemFootnote @subject_absence_by_geographic_level, @filterItem_State_funded_primary, @footnote_id_1;
EXEC InsertFilterItemFootnote @subject_absence_by_term, @filterItem_State_funded_primary, @footnote_id_1;
EXEC InsertFilterItemFootnote @subject_absence_for_four_year_olds, @filterItem_State_funded_primary, @footnote_id_1;
EXEC InsertFilterItemFootnote @subject_absence_number_missing, @filterItem_State_funded_primary, @footnote_id_1;
EXEC InsertFilterItemFootnote @subject_absence_rate_percent_bands, @filterItem_State_funded_primary, @footnote_id_1;

EXEC InsertFilterItemFootnote @subject_absence_by_characteristic, @filterItem_State_funded_secondary, @footnote_id_2;
EXEC InsertFilterItemFootnote @subject_absence_by_geographic_level, @filterItem_State_funded_secondary, @footnote_id_2;
EXEC InsertFilterItemFootnote @subject_absence_by_term, @filterItem_State_funded_secondary, @footnote_id_2;
EXEC InsertFilterItemFootnote @subject_absence_for_four_year_olds, @filterItem_State_funded_secondary, @footnote_id_2;
EXEC InsertFilterItemFootnote @subject_absence_number_missing, @filterItem_State_funded_secondary, @footnote_id_2;
EXEC InsertFilterItemFootnote @subject_absence_rate_percent_bands, @filterItem_State_funded_secondary, @footnote_id_2;

EXEC InsertFilterItemFootnote @subject_absence_by_characteristic, @filterItem_Special, @footnote_id_3;
EXEC InsertFilterItemFootnote @subject_absence_by_geographic_level, @filterItem_Special, @footnote_id_3;
EXEC InsertFilterItemFootnote @subject_absence_by_term, @filterItem_Special, @footnote_id_3;
EXEC InsertFilterItemFootnote @subject_absence_for_four_year_olds, @filterItem_Special, @footnote_id_3;
EXEC InsertFilterItemFootnote @subject_absence_number_missing, @filterItem_Special, @footnote_id_3;
EXEC InsertFilterItemFootnote @subject_absence_rate_percent_bands, @filterItem_Special, @footnote_id_3;

EXEC InsertFilterItemFootnote @subject_absence_by_characteristic, @filterItem_Ethnicity_Minority_Ethnic_Group, @footnote_id_18;

EXEC InsertFilterItemFootnote @subject_absence_by_characteristic, @filterItem_Social_emotional_and_mental_health, @footnote_id_20;


--
-- Filters
--


DECLARE @filter_Characteristic NVARCHAR(max) = 'Characteristic';

EXEC InsertFilterFootnote @subject_absence_by_characteristic, @filter_Characteristic, @footnote_id_15;
EXEC InsertFilterFootnote @subject_absence_by_characteristic, @filter_Characteristic, @footnote_id_16;


--
-- Filter Groups
--


DECLARE @filterGroup_SEN_provision NVARCHAR(max) = 'SEN provision';
DECLARE @filterGroup_SEN_primary_need NVARCHAR(max) = 'SEN primary need';

EXEC InsertFilterGroupFootnote @subject_absence_by_characteristic, @filterGroup_SEN_provision, @footnote_id_17;
EXEC InsertFilterGroupFootnote @subject_absence_by_characteristic, @filterGroup_SEN_primary_need, @footnote_id_19;

DROP PROCEDURE InsertSubjectFootnote;
DROP PROCEDURE InsertIndicatorFootnote;
DROP PROCEDURE InsertFilterFootnote;
DROP PROCEDURE InsertFilterGroupFootnote;
DROP PROCEDURE InsertFilterItemFootnote;