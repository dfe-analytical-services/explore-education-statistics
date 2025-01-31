CREATE OR ALTER PROCEDURE InsertSubjectFootnote
    @subjectId uniqueidentifier,
    @footnoteId uniqueidentifier
AS
BEGIN TRY
    INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subjectId, @footnoteId);
END TRY
BEGIN CATCH
    DECLARE @errorMessage NVARCHAR(max) = ERROR_MESSAGE();
    DECLARE @errorSeverity INT = ERROR_SEVERITY();
    DECLARE @errorState INT = ERROR_SEVERITY();
    DECLARE @errorProcedure NVARCHAR(max) = ERROR_PROCEDURE();
    DECLARE @subjectIdString NVARCHAR(50) = CONVERT(nvarchar(50), @subjectId);
    DECLARE @footnoteIdString NVARCHAR(50) = CONVERT(nvarchar(50), @footnoteId);
    RAISERROR (N'Error executing %s(SUBJECT: %s, FOOTNOTE: %s) MESSAGE: %s', @errorSeverity, @errorState, @errorProcedure, @subjectIdString, @footnoteIdString, @errorMessage);
END CATCH
GO

CREATE OR ALTER PROCEDURE InsertIndicatorFootnote
    @subjectId uniqueidentifier,
    @indicatorName NVARCHAR(max),
    @footnoteId uniqueidentifier
AS
DECLARE
    @indicatorId uniqueidentifier = (SELECT I.Id FROM Indicator I JOIN IndicatorGroup IG ON I.IndicatorGroupId = IG.Id WHERE IG.SubjectId = @subjectId AND I.Name = @indicatorName);
BEGIN TRY
    INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (@indicatorId, @footnoteId);
END TRY
BEGIN CATCH
    DECLARE @errorMessage NVARCHAR(max) = ERROR_MESSAGE();
    DECLARE @errorSeverity INT = ERROR_SEVERITY();
    DECLARE @errorState INT = ERROR_SEVERITY();
    DECLARE @errorProcedure NVARCHAR(max) = ERROR_PROCEDURE();
    DECLARE @subjectIdString NVARCHAR(50) = CONVERT(nvarchar(50), @subjectId);
    DECLARE @footnoteIdString NVARCHAR(50) = CONVERT(nvarchar(50), @footnoteId);
    RAISERROR (N'Error executing %s(SUBJECT: %s, INDICATOR: %s, FOOTNOTE: %s) MESSAGE: %s', @errorSeverity, @errorState, @errorProcedure, @subjectIdString, @indicatorName, @footnoteIdString, @errorMessage);
END CATCH
GO

CREATE OR ALTER PROCEDURE InsertFilterFootnote
    @subjectId uniqueidentifier,
    @filterName NVARCHAR(max),
    @footnoteId uniqueidentifier
AS
DECLARE
    @filterId uniqueidentifier = (SELECT F.Id FROM Filter F WHERE F.SubjectId = @subjectId AND F.Name = @filterName);
BEGIN TRY
    INSERT INTO FilterFootnote (FilterId, FootnoteId) VALUES (@filterId, @footnoteId);
END TRY
BEGIN CATCH
    DECLARE @errorMessage NVARCHAR(max) = ERROR_MESSAGE();
    DECLARE @errorSeverity INT = ERROR_SEVERITY();
    DECLARE @errorState INT = ERROR_SEVERITY();
    DECLARE @errorProcedure NVARCHAR(max) = ERROR_PROCEDURE();
    DECLARE @subjectIdString NVARCHAR(50) = CONVERT(nvarchar(50), @subjectId);
    DECLARE @footnoteIdString NVARCHAR(50) = CONVERT(nvarchar(50), @footnoteId);
    RAISERROR (N'Error executing %s(SUBJECT: %s, FILTER: %s, FOOTNOTE: %s) MESSAGE: %s', @errorSeverity, @errorState, @errorProcedure, @subjectIdString, @filterName, @footnoteIdString, @errorMessage);
END CATCH
GO

CREATE OR ALTER PROCEDURE InsertFilterGroupFootnote
    @subjectId uniqueidentifier,
    @filterGroupName NVARCHAR(max),
    @footnoteId uniqueidentifier
AS
DECLARE
    @filterGroupId uniqueidentifier = (SELECT FG.Id FROM FilterGroup FG JOIN Filter F ON FG.FilterId = F.Id WHERE F.SubjectId = @subjectId AND FG.Label = @filterGroupName);
BEGIN TRY
    INSERT INTO FilterGroupFootnote (FilterGroupId, FootnoteId) VALUES (@filterGroupId, @footnoteId);
END TRY
BEGIN CATCH
    DECLARE @errorMessage NVARCHAR(max) = ERROR_MESSAGE();
    DECLARE @errorSeverity INT = ERROR_SEVERITY();
    DECLARE @errorState INT = ERROR_SEVERITY();
    DECLARE @errorProcedure NVARCHAR(max) = ERROR_PROCEDURE();
    DECLARE @subjectIdString NVARCHAR(50) = CONVERT(nvarchar(50), @subjectId);
    DECLARE @footnoteIdString NVARCHAR(50) = CONVERT(nvarchar(50), @footnoteId);
    RAISERROR (N'Error executing %s(SUBJECT: %s, FILTER GROUP: %s, FOOTNOTE: %s) MESSAGE: %s', @errorSeverity, @errorState, @errorProcedure, @subjectIdString, @filterGroupName, @footnoteIdString, @errorMessage);
END CATCH
GO

CREATE OR ALTER PROCEDURE InsertFilterItemFootnote
    @subjectId uniqueidentifier,
    @filterItemName NVARCHAR(max),
    @footnoteId uniqueidentifier
AS
DECLARE
    @filterItemId uniqueidentifier = (SELECT FI.Id FROM FilterItem FI JOIN FilterGroup FG ON FI.FilterGroupId = FG.Id JOIN Filter F ON FG.FilterId = F.Id WHERE F.SubjectId = @subjectId AND FI.Label = @filterItemName);
BEGIN TRY
    INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (@filterItemId, @footnoteId);
END TRY
BEGIN CATCH
    DECLARE @errorMessage NVARCHAR(max) = ERROR_MESSAGE();
    DECLARE @errorSeverity INT = ERROR_SEVERITY();
    DECLARE @errorState INT = ERROR_SEVERITY();
    DECLARE @errorProcedure NVARCHAR(max) = ERROR_PROCEDURE();
    DECLARE @subjectIdString NVARCHAR(50) = CONVERT(nvarchar(50), @subjectId);
    DECLARE @footnoteIdString NVARCHAR(50) = CONVERT(nvarchar(50), @footnoteId);
    RAISERROR (N'Error executing %s(SUBJECT: %s, FILTER ITEM: %s, FOOTNOTE: %s) MESSAGE: %s', @errorSeverity, @errorState, @errorProcedure, @subjectIdString, @filterItemName, @footnoteIdString, @errorMessage);
END CATCH
GO


-- Pupil absence in schools in England 2016/17
DECLARE @absence_release uniqueidentifier = '4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5';
-- Permanent and fixed-period exclusions in England 2016/17
DECLARE @exclusions_release uniqueidentifier = 'e7774a74-1f62-4b76-b9b5-84f14dac7278';

-- Delete any existing data for the declared releases
DELETE FROM SubjectFootnote WHERE FootnoteId IN (SELECT FootnoteId FROM ReleaseFootnote F WHERE F.ReleaseId IN (@absence_release, @exclusions_release));
DELETE FROM IndicatorFootnote WHERE FootnoteId IN (SELECT FootnoteId FROM ReleaseFootnote F WHERE F.ReleaseId IN (@absence_release, @exclusions_release));
DELETE FROM FilterItemFootnote WHERE FootnoteId IN (SELECT FootnoteId FROM ReleaseFootnote F WHERE F.ReleaseId IN (@absence_release, @exclusions_release));
DELETE FROM FilterGroupFootnote WHERE FootnoteId IN (SELECT FootnoteId FROM ReleaseFootnote F WHERE F.ReleaseId IN (@absence_release, @exclusions_release));
DELETE FROM FilterFootnote WHERE FootnoteId IN (SELECT FootnoteId FROM ReleaseFootnote F WHERE F.ReleaseId IN (@absence_release, @exclusions_release));
DELETE FROM ReleaseFootnote WHERE ReleaseId IN (@absence_release, @exclusions_release);
DELETE FROM Footnote WHERE Id NOT IN (SELECT FootnoteId FROM ReleaseFootnote);
--
-- Footnotes
--


-- Absence footnotes
DECLARE @footnote_id_1 uniqueidentifier = 'fbb6262f-213a-453a-98ca-b832d6ae1c16';
DECLARE @footnote_id_2 uniqueidentifier = 'e9076bab-6ff7-4c92-8972-2fb4affbe977';
DECLARE @footnote_id_3 uniqueidentifier = '128c6153-4937-40da-a99f-e137fc8c41e5';
DECLARE @footnote_id_4 uniqueidentifier = '6c06f733-c30b-45e1-980a-b587e923f73b';
DECLARE @footnote_id_5 uniqueidentifier = 'b678373f-dfa2-41ad-817e-3f011d1b5173';
DECLARE @footnote_id_6 uniqueidentifier = '7fabae1a-5cc0-4a1a-861a-3ddd8ec0f7b2';
DECLARE @footnote_id_7 uniqueidentifier = '78c78049-22b3-4ec8-bc2a-7f47a6f7c74c';
DECLARE @footnote_id_8 uniqueidentifier = '7b18666a-ff4a-43a9-9639-8f324374db0c';
DECLARE @footnote_id_9 uniqueidentifier = '507b05b0-4015-4330-9901-20b47c5b154d';
DECLARE @footnote_id_10 uniqueidentifier = '82aa9459-2ff3-45b6-ba98-6d8819cea9f6';
DECLARE @footnote_id_11 uniqueidentifier = '86ec4e28-0f68-42f3-b153-d67e611aa6f1';
DECLARE @footnote_id_12 uniqueidentifier = '55f10d5a-792b-4065-9e32-446d29f43c7f';
DECLARE @footnote_id_13 uniqueidentifier = '7b32b523-e6c0-4091-91d1-b515550689d1';
DECLARE @footnote_id_14 uniqueidentifier = 'e03c3b82-75df-4dee-b3ed-dac39378a9b5';
DECLARE @footnote_id_15 uniqueidentifier = '96641ff5-d33b-495d-8691-8045c148a595';
DECLARE @footnote_id_16 uniqueidentifier = '6176a831-fdc9-4645-915e-4051a93ba05a';
DECLARE @footnote_id_17 uniqueidentifier = 'e8ebee03-dc9d-4ae8-8b5e-a7ff831611f0';
DECLARE @footnote_id_18 uniqueidentifier = '65fcb08f-e70a-4cf1-920b-5e326fbaa8b4';
DECLARE @footnote_id_19 uniqueidentifier = 'a9045d29-7c84-491d-918c-3885f11597d3';
DECLARE @footnote_id_20 uniqueidentifier = '2268f97a-5107-464f-ade6-30debc36519a';
DECLARE @footnote_id_21 uniqueidentifier = '8a79e8c6-3552-496a-8790-d072cec8c43f';

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_1, 'State-funded primary schools include all primary academies, including free schools.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_1, @absence_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_2,
        'State-funded secondary schools include city technology colleges and all secondary academies, including all-through academies and free schools.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_2, @absence_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_3,
        'Special schools include maintained special schools, non-maintained special schools and special academies.  Excludes general hospital schools, independent special schools and independent schools approved for SEN pupils.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_3, @absence_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_4, 'x - 1 or 2 enrolments, or a percentage based on 1 or 2 enrolments.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_4, @absence_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_5,
        'There may be discrepancies between totals and the sum of constituent parts  as national and regional totals and totals across school types have been rounded to the nearest 5.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_5, @absence_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_6,
        'Absence rates are the number of absence sessions expressed as a percentage of the total number of possible sessions.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_6, @absence_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_7,
        'Pupil enrolments missing 10 percent or more of their own possible sessions (due to authorised or unauthorised absence) are classified as persistent absentees.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_7, @absence_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_8,
        'The definition of persistent absence changed from the 2015/16 academic year - The information presented for years prior to 2015/16 has been produced using the same methodology in order to allow users to make comparisons on a consistent basis over time.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_8, @absence_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_9,
        'The persistent absentee rate is the number of persistent absentees expressed as a percentage of the total number of enrolments.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_9, @absence_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_10,
        'Number of enrolments includes pupils on the school roll for at least one session who are aged between 5 and 15, excluding boarders. Some pupils may be counted more than once (if they moved schools during the academic year or are registered in more than one school).');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_10, @absence_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_11, 'The number of enrolments with one or more session of absence for specific reason.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_11, @absence_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_12,
        'The number of sessions missed in each band expressed as a percentage of the total number of sessions missed for that category of absence overall.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_12, @absence_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_13, 'Includes all schools with at least six enrolments who reported absence information via the school census for the associated academic year.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_13, @absence_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_14,
        'See "Guide to absence statistics" for more information on how absence and pupil characteristic data have been linked.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_14, @absence_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_15,
        'Figures for pupils with unclassified or missing characteristics information should be interpreted with caution.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_15, @absence_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_16,
        'In September 2014 the Special Educational Needs and Disability (SEND) provisions were introduced in the Children and Families Act 2014. This reform caused changes to SEN categories - Education, Health and Care (EHC) plans were introduced and SEN support replaces school action and school action plus but some pupils remain with these provision types in first year of transition (2014/15).');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_16, @absence_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_17,
        'Minority ethnic group includes pupils who have been classified according to their ethnic group, excluding White British.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_17, @absence_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_18,
        'From 2015 SEN support replaced school action and School Action plus but some pupils remain with these provision types. Those who were formerly School Action did not provide type of need in previous years. Those who remain on school action provision are not included here but have been included within the SEN support category in other tables.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_18, @absence_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_19,
        'Social Emotional and Mental Health was added as a new type of need in 2015, the previous type of need Behaviour, Emotional and Social Difficulties has been removed although it is not expected it should be a direct replacement.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_19, @absence_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_20,
        'Includes pupils on the school roll for at least one session who are aged 4, excluding boarders. Some pupils may be counted more than once (if they moved schools during the academic year or are registered at more than one school).');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_20, @absence_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_21,
        'Only overall absence is recorded for pupils aged 4, absences are not categorised as authorised or unauthorised.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_21, @absence_release);

-- Exclusions footnotes
DECLARE @footnote_id_22 uniqueidentifier = '16514a6b-4a4c-4200-83c8-c0e3c93a866c';
DECLARE @footnote_id_23 uniqueidentifier = 'a02f6380-311b-4551-a42e-ba4091879ed8';
DECLARE @footnote_id_24 uniqueidentifier = 'b5c6814b-78d7-4bf9-b894-92caf334d432';
DECLARE @footnote_id_25 uniqueidentifier = 'e43f4a88-5cd0-4cad-b84c-d5390f0ecd0f';
DECLARE @footnote_id_26 uniqueidentifier = '1cf61bb7-b8fc-44ae-a272-e3b2d87d3cf4';
DECLARE @footnote_id_27 uniqueidentifier = 'a18f2ab2-663a-45cd-8cea-86c716c0453c';
DECLARE @footnote_id_28 uniqueidentifier = '58717767-3cf5-42b1-8e7a-6c247dfcfc02';
DECLARE @footnote_id_29 uniqueidentifier = '2b87e363-3a9c-4f43-88c0-e8a602e4c3c0';
DECLARE @footnote_id_30 uniqueidentifier = '27b8480c-4a53-4385-afb1-69e76f627885';
DECLARE @footnote_id_31 uniqueidentifier = 'c549699c-83cb-49a3-85a7-6c5e2a91c2b7';
DECLARE @footnote_id_32 uniqueidentifier = '2eb0b631-48c7-4f4e-b91d-dab25bb9e9a3';
DECLARE @footnote_id_33 uniqueidentifier = 'eaae1d1f-6e28-4570-b0dd-966366950e60';
DECLARE @footnote_id_34 uniqueidentifier = '95a70fdf-ea82-4276-869a-22879c5ced20';
DECLARE @footnote_id_35 uniqueidentifier = '2185b382-8be9-4e48-aba0-096cf5d9b3a3';
DECLARE @footnote_id_36 uniqueidentifier = '9544977a-24e9-4aee-80d7-77d7cf805986';
DECLARE @footnote_id_37 uniqueidentifier = 'f2f400ac-b977-4c40-aa34-4a93ef1202f0';
DECLARE @footnote_id_38 uniqueidentifier = '31457a0b-042c-41a6-9df8-5e4b26e98d6b';
DECLARE @footnote_id_39 uniqueidentifier = '5c06d319-a0a1-48ed-b747-6a6f8d1354e3';
DECLARE @footnote_id_40 uniqueidentifier = 'e6890af0-28ae-44c6-8ca9-eaddfb7a381f';

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_22,
        'The number of permanent exclusions expressed as a percentage of the number of pupils in January each year.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_22, @exclusions_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_23,
        'The number of pupil enrolments receiving one or more fixed period exclusion expressed as a percentage of the number of pupils in January each year.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_23, @exclusions_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_24,
        'The number of fixed period exclusions expressed as a percentage of the number of pupils in January each year.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_24, @exclusions_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_25,
        'Includes pupils who are sole or dual main registrations. Includes boarding pupils.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_25, @exclusions_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_26,
        'SEN status is as recorded at the time of exclusion. For enrolments the SEN status at the time of the most recent exclusion is used.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_26, @exclusions_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_27,
        'Education, Health and Care (EHC) plans were introduced from September 2014 as part of a range of SEND reforms.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_27, @exclusions_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_28,
        'Figures are presented for pupils with a Statement of SEN, Education, Health and Care plan or SEN support and recorded primary need. This excludes those with SEN support who are yet to be assessed for a type of need and those with school action provision. SEN status is as recorded at the time of exclusion. For enrolments the SEN status at the time of the most recent exclusion is used.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_28, @exclusions_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_29,
        'For enrolments the national curriculum year of the most recent exclusion is used.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_29, @exclusions_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_30,
        'Free school meal eligibility as at January each year.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_30, @exclusions_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_31,
        'Pupils who have been classified according to their ethnic group, excluding White British.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_31, @exclusions_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_32,
        'Ethnic group is at January each year.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_32, @exclusions_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_33,
        'Includes pupils whose ethnic information was not sought or was refused or could not be determined.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_33, @exclusions_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_34,
        'Income Deprivation Affecting Children Index at Super Output Area level based on the location of the school. Includes all schools which returned information on exclusions.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_34, @exclusions_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_35,
        'Length of exclusion in days is derived from the number of sessions where one session equates to half a school day. Part days have been counted as one school day.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_35, @exclusions_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_36,
        'The number of open schools as at January plus the number of schools which opened after January but reported exclusions.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_36, @exclusions_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_37,
        'State-funded primary schools include all primary academies, including free schools.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_37, @exclusions_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_38,
        'State-funded secondary schools include city technology colleges and all secondary academies, including all-through academies and free schools.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_38, @exclusions_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_39,
        'Special schools include maintained special schools, non-maintained special schools and special academies. Excludes general hospital schools.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_39, @exclusions_release);

INSERT INTO Footnote (Id, Content)
VALUES (@footnote_id_40,
        'x - 1 or 2 pupils, or a percentage based on 1 or 2.');
INSERT INTO ReleaseFootnote (FootnoteId, ReleaseId)
VALUES (@footnote_id_40, @exclusions_release);

--
-- Subjects
--


-- Absence subjects
DECLARE @subject_id_absence_by_characteristic   uniqueidentifier = '803fbf56-600f-490f-8409-6413a891720d';
DECLARE @subject_id_absence_by_geographic_level uniqueidentifier = '568576e5-d386-450e-a8db-307b7061d0d8';
DECLARE @subject_id_absence_by_term             uniqueidentifier = 'b7bc537b-0c04-4b15-9eb6-4f0e8cc2e70a';
DECLARE @subject_id_absence_for_four_year_olds  uniqueidentifier = '353db5ea-befd-488b-ad16-2ce7963c9bc9';
DECLARE @subject_id_absence_in_prus             uniqueidentifier = '95c7f584-907e-4756-bbf0-4905ceae57df';
DECLARE @subject_id_absence_number_missing      uniqueidentifier = 'faf2152e-0a6c-4e97-af02-e9a89d48c47a';
DECLARE @subject_id_absence_rate_percent_bands  uniqueidentifier = '666cd878-87bb-4f77-9a3f-f5c75078e112';

EXEC InsertSubjectFootnote @subject_id_absence_by_characteristic, @footnote_id_4;
EXEC InsertSubjectFootnote @subject_id_absence_by_geographic_level, @footnote_id_4;
EXEC InsertSubjectFootnote @subject_id_absence_by_term, @footnote_id_4;
EXEC InsertSubjectFootnote @subject_id_absence_for_four_year_olds, @footnote_id_4;
EXEC InsertSubjectFootnote @subject_id_absence_in_prus, @footnote_id_4;
EXEC InsertSubjectFootnote @subject_id_absence_number_missing, @footnote_id_4;
EXEC InsertSubjectFootnote @subject_id_absence_rate_percent_bands, @footnote_id_4;

EXEC InsertSubjectFootnote @subject_id_absence_by_characteristic, @footnote_id_5;
EXEC InsertSubjectFootnote @subject_id_absence_by_geographic_level, @footnote_id_5;
EXEC InsertSubjectFootnote @subject_id_absence_by_term, @footnote_id_5;
EXEC InsertSubjectFootnote @subject_id_absence_for_four_year_olds, @footnote_id_5;
EXEC InsertSubjectFootnote @subject_id_absence_in_prus, @footnote_id_5;
EXEC InsertSubjectFootnote @subject_id_absence_number_missing, @footnote_id_5;
EXEC InsertSubjectFootnote @subject_id_absence_rate_percent_bands, @footnote_id_5;

-- Exclusion subjects
DECLARE @subject_id_duration_of_fixed_exclusions                     uniqueidentifier = '926e33e4-b3ce-41aa-9ed2-e04106068ffb';
DECLARE @subject_id_exclusions_by_characteristic                     uniqueidentifier = '92039f68-a894-46a9-bd44-4482728698b0';
DECLARE @subject_id_exclusions_by_geographic_level                   uniqueidentifier = '3c0fbe56-0a4b-4caa-82f2-ab696cd96090';
DECLARE @subject_id_exclusions_by_reason                             uniqueidentifier = '8fe88bbe-dce7-4698-a55f-8e1e3e41c5a7';
DECLARE @subject_id_number_of_fixed_exclusions                       uniqueidentifier = '048a6276-1df1-487a-a501-fbd5e64d4b79';
DECLARE @subject_id_total_days_missed_due_to_fixed_period_exclusions uniqueidentifier = '28feb263-4bf9-4dd7-9440-48e2685f6954';

EXEC InsertSubjectFootnote @subject_id_duration_of_fixed_exclusions, @footnote_id_35;

EXEC InsertSubjectFootnote @subject_id_duration_of_fixed_exclusions, @footnote_id_40;
EXEC InsertSubjectFootnote @subject_id_exclusions_by_characteristic, @footnote_id_40;
EXEC InsertSubjectFootnote @subject_id_exclusions_by_geographic_level, @footnote_id_40;
EXEC InsertSubjectFootnote @subject_id_exclusions_by_reason, @footnote_id_40;
EXEC InsertSubjectFootnote @subject_id_number_of_fixed_exclusions, @footnote_id_40;
EXEC InsertSubjectFootnote @subject_id_total_days_missed_due_to_fixed_period_exclusions, @footnote_id_40;


--
-- Indicators
--


-- Absence indicators
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

EXEC InsertIndicatorFootnote @subject_id_absence_by_characteristic, @indicator_sess_overall_percent, @footnote_id_6;
EXEC InsertIndicatorFootnote @subject_id_absence_by_geographic_level, @indicator_sess_overall_percent, @footnote_id_6;
EXEC InsertIndicatorFootnote @subject_id_absence_by_term, @indicator_sess_overall_percent, @footnote_id_6;
EXEC InsertIndicatorFootnote @subject_id_absence_for_four_year_olds, @indicator_sess_overall_percent, @footnote_id_6;
EXEC InsertIndicatorFootnote @subject_id_absence_in_prus, @indicator_sess_overall_percent, @footnote_id_6;

EXEC InsertIndicatorFootnote @subject_id_absence_by_characteristic, @indicator_sess_authorised_percent, @footnote_id_6;
EXEC InsertIndicatorFootnote @subject_id_absence_by_geographic_level, @indicator_sess_authorised_percent, @footnote_id_6;
EXEC InsertIndicatorFootnote @subject_id_absence_by_term, @indicator_sess_authorised_percent, @footnote_id_6;
EXEC InsertIndicatorFootnote @subject_id_absence_in_prus, @indicator_sess_authorised_percent, @footnote_id_6;

EXEC InsertIndicatorFootnote @subject_id_absence_by_characteristic, @indicator_sess_unauthorised_percent, @footnote_id_6;
EXEC InsertIndicatorFootnote @subject_id_absence_by_geographic_level, @indicator_sess_unauthorised_percent, @footnote_id_6;
EXEC InsertIndicatorFootnote @subject_id_absence_by_term, @indicator_sess_unauthorised_percent, @footnote_id_6;
EXEC InsertIndicatorFootnote @subject_id_absence_in_prus, @indicator_sess_unauthorised_percent, @footnote_id_6;

EXEC InsertIndicatorFootnote @subject_id_absence_by_characteristic, @indicator_enrolments_pa_10_exact, @footnote_id_7;
EXEC InsertIndicatorFootnote @subject_id_absence_by_geographic_level, @indicator_enrolments_pa_10_exact, @footnote_id_7;
EXEC InsertIndicatorFootnote @subject_id_absence_in_prus, @indicator_enrolments_pa_10_exact, @footnote_id_7;

EXEC InsertIndicatorFootnote @subject_id_absence_by_characteristic, @indicator_enrolments_pa_10_exact_percent, @footnote_id_7;
EXEC InsertIndicatorFootnote @subject_id_absence_by_geographic_level, @indicator_enrolments_pa_10_exact_percent, @footnote_id_7;
EXEC InsertIndicatorFootnote @subject_id_absence_in_prus, @indicator_enrolments_pa_10_exact_percent, @footnote_id_7;

EXEC InsertIndicatorFootnote @subject_id_absence_by_characteristic, @indicator_sess_possible_pa_10_exact, @footnote_id_7;
EXEC InsertIndicatorFootnote @subject_id_absence_by_geographic_level, @indicator_sess_possible_pa_10_exact, @footnote_id_7;

EXEC InsertIndicatorFootnote @subject_id_absence_by_characteristic, @indicator_sess_overall_pa_10_exact, @footnote_id_7;
EXEC InsertIndicatorFootnote @subject_id_absence_by_geographic_level, @indicator_sess_overall_pa_10_exact, @footnote_id_7;

EXEC InsertIndicatorFootnote @subject_id_absence_by_characteristic, @indicator_sess_authorised_pa_10_exact, @footnote_id_7;
EXEC InsertIndicatorFootnote @subject_id_absence_by_geographic_level, @indicator_sess_authorised_pa_10_exact, @footnote_id_7;

EXEC InsertIndicatorFootnote @subject_id_absence_by_characteristic, @indicator_sess_unauthorised_pa_10_exact, @footnote_id_7;
EXEC InsertIndicatorFootnote @subject_id_absence_by_geographic_level, @indicator_sess_unauthorised_pa_10_exact, @footnote_id_7;

EXEC InsertIndicatorFootnote @subject_id_absence_by_characteristic, @indicator_sess_overall_percent_pa_10_exact, @footnote_id_7;
EXEC InsertIndicatorFootnote @subject_id_absence_by_geographic_level, @indicator_sess_overall_percent_pa_10_exact, @footnote_id_7;

EXEC InsertIndicatorFootnote @subject_id_absence_by_characteristic, @indicator_sess_authorised_percent_pa_10_exact, @footnote_id_7;
EXEC InsertIndicatorFootnote @subject_id_absence_by_geographic_level, @indicator_sess_authorised_percent_pa_10_exact, @footnote_id_7;

EXEC InsertIndicatorFootnote @subject_id_absence_by_characteristic, @indicator_sess_unauthorised_percent_pa_10_exact, @footnote_id_7;
EXEC InsertIndicatorFootnote @subject_id_absence_by_geographic_level, @indicator_sess_unauthorised_percent_pa_10_exact, @footnote_id_7;

EXEC InsertIndicatorFootnote @subject_id_absence_by_characteristic, @indicator_enrolments_pa_10_exact, @footnote_id_8;
EXEC InsertIndicatorFootnote @subject_id_absence_by_geographic_level, @indicator_enrolments_pa_10_exact, @footnote_id_8;
EXEC InsertIndicatorFootnote @subject_id_absence_in_prus, @indicator_enrolments_pa_10_exact, @footnote_id_8;

EXEC InsertIndicatorFootnote @subject_id_absence_by_characteristic, @indicator_enrolments_pa_10_exact_percent, @footnote_id_8;
EXEC InsertIndicatorFootnote @subject_id_absence_by_geographic_level, @indicator_enrolments_pa_10_exact_percent, @footnote_id_8;
EXEC InsertIndicatorFootnote @subject_id_absence_in_prus, @indicator_enrolments_pa_10_exact_percent, @footnote_id_8;

EXEC InsertIndicatorFootnote @subject_id_absence_by_characteristic, @indicator_sess_possible_pa_10_exact, @footnote_id_8;
EXEC InsertIndicatorFootnote @subject_id_absence_by_geographic_level, @indicator_sess_possible_pa_10_exact, @footnote_id_8;

EXEC InsertIndicatorFootnote @subject_id_absence_by_characteristic, @indicator_sess_overall_pa_10_exact, @footnote_id_8;
EXEC InsertIndicatorFootnote @subject_id_absence_by_geographic_level, @indicator_sess_overall_pa_10_exact, @footnote_id_8;

EXEC InsertIndicatorFootnote @subject_id_absence_by_characteristic, @indicator_sess_authorised_pa_10_exact, @footnote_id_8;
EXEC InsertIndicatorFootnote @subject_id_absence_by_geographic_level, @indicator_sess_authorised_pa_10_exact, @footnote_id_8;

EXEC InsertIndicatorFootnote @subject_id_absence_by_characteristic, @indicator_sess_unauthorised_pa_10_exact, @footnote_id_8;
EXEC InsertIndicatorFootnote @subject_id_absence_by_geographic_level, @indicator_sess_unauthorised_pa_10_exact, @footnote_id_8;

EXEC InsertIndicatorFootnote @subject_id_absence_by_characteristic, @indicator_sess_overall_percent_pa_10_exact, @footnote_id_8;
EXEC InsertIndicatorFootnote @subject_id_absence_by_geographic_level, @indicator_sess_overall_percent_pa_10_exact, @footnote_id_8;

EXEC InsertIndicatorFootnote @subject_id_absence_by_characteristic, @indicator_sess_authorised_percent_pa_10_exact, @footnote_id_8;
EXEC InsertIndicatorFootnote @subject_id_absence_by_geographic_level, @indicator_sess_authorised_percent_pa_10_exact, @footnote_id_8;

EXEC InsertIndicatorFootnote @subject_id_absence_by_characteristic, @indicator_sess_unauthorised_percent_pa_10_exact, @footnote_id_8;
EXEC InsertIndicatorFootnote @subject_id_absence_by_geographic_level, @indicator_sess_unauthorised_percent_pa_10_exact, @footnote_id_8;

EXEC InsertIndicatorFootnote @subject_id_absence_by_characteristic, @indicator_enrolments_pa_10_exact_percent, @footnote_id_9;
EXEC InsertIndicatorFootnote @subject_id_absence_by_geographic_level, @indicator_enrolments_pa_10_exact_percent, @footnote_id_9;
EXEC InsertIndicatorFootnote @subject_id_absence_in_prus, @indicator_enrolments_pa_10_exact_percent, @footnote_id_9;

EXEC InsertIndicatorFootnote @subject_id_absence_by_characteristic, @indicator_enrolments, @footnote_id_10;
EXEC InsertIndicatorFootnote @subject_id_absence_by_geographic_level, @indicator_enrolments, @footnote_id_10;
EXEC InsertIndicatorFootnote @subject_id_absence_in_prus, @indicator_enrolments, @footnote_id_10;
EXEC InsertIndicatorFootnote @subject_id_absence_number_missing, @indicator_enrolments, @footnote_id_10;

EXEC InsertIndicatorFootnote @subject_id_absence_number_missing, @indicator_enrol_unauth_late, @footnote_id_11;
EXEC InsertIndicatorFootnote @subject_id_absence_number_missing, @indicator_enrol_unauth_holiday, @footnote_id_11;
EXEC InsertIndicatorFootnote @subject_id_absence_number_missing, @indicator_enrol_auth_other, @footnote_id_11;
EXEC InsertIndicatorFootnote @subject_id_absence_number_missing, @indicator_enrol_auth_excluded, @footnote_id_11;
EXEC InsertIndicatorFootnote @subject_id_absence_number_missing, @indicator_enrol_auth_ext_holiday, @footnote_id_11;
EXEC InsertIndicatorFootnote @subject_id_absence_number_missing, @indicator_enrol_auth_holiday, @footnote_id_11;
EXEC InsertIndicatorFootnote @subject_id_absence_number_missing, @indicator_enrol_unauth_other, @footnote_id_11;
EXEC InsertIndicatorFootnote @subject_id_absence_number_missing, @indicator_enrol_auth_traveller, @footnote_id_11;
EXEC InsertIndicatorFootnote @subject_id_absence_number_missing, @indicator_enrol_auth_religious, @footnote_id_11;
EXEC InsertIndicatorFootnote @subject_id_absence_number_missing, @indicator_enrol_auth_appointments, @footnote_id_11;
EXEC InsertIndicatorFootnote @subject_id_absence_number_missing, @indicator_enrol_auth_illness, @footnote_id_11;
EXEC InsertIndicatorFootnote @subject_id_absence_number_missing, @indicator_enrol_unauthorised, @footnote_id_11;
EXEC InsertIndicatorFootnote @subject_id_absence_number_missing, @indicator_enrol_authorised, @footnote_id_11;
EXEC InsertIndicatorFootnote @subject_id_absence_number_missing, @indicator_enrol_overall, @footnote_id_11;
EXEC InsertIndicatorFootnote @subject_id_absence_number_missing, @indicator_enrol_auth_study, @footnote_id_11;
EXEC InsertIndicatorFootnote @subject_id_absence_number_missing, @indicator_enrol_unauth_noyet, @footnote_id_11;

EXEC InsertIndicatorFootnote @subject_id_absence_rate_percent_bands, @indicator_enrolments_overall_percent, @footnote_id_12;
EXEC InsertIndicatorFootnote @subject_id_absence_rate_percent_bands, @indicator_enrolments_authorised_percent, @footnote_id_12;
EXEC InsertIndicatorFootnote @subject_id_absence_rate_percent_bands, @indicator_enrolments_unauthorised_percent, @footnote_id_12;

EXEC InsertIndicatorFootnote @subject_id_absence_by_geographic_level, @indicator_num_schools, @footnote_id_13;
EXEC InsertIndicatorFootnote @subject_id_absence_for_four_year_olds, @indicator_num_schools, @footnote_id_13
EXEC InsertIndicatorFootnote @subject_id_absence_in_prus, @indicator_num_schools, @footnote_id_13;

EXEC InsertIndicatorFootnote @subject_id_absence_for_four_year_olds, @indicator_enrolments, @footnote_id_20;

EXEC InsertIndicatorFootnote @subject_id_absence_for_four_year_olds, @indicator_sess_overall, @footnote_id_21;
EXEC InsertIndicatorFootnote @subject_id_absence_for_four_year_olds, @indicator_sess_overall_percent, @footnote_id_21;

-- Exclusion indicators
DECLARE @indicator_permanent_exclusion_rate NVARCHAR(max) = 'perm_excl_rate';
DECLARE @indicator_fixed_period_exclusion_rate NVARCHAR(max) = 'fixed_excl_rate';
DECLARE @indicator_number_of_pupils NVARCHAR(max) = 'headcount';
DECLARE @indicator_number_of_schools NVARCHAR(max) = 'num_schools';
DECLARE @indicator_percentage_of_pupils_with_one_or_more_fixed_period_exclusions NVARCHAR(max) = 'one_plus_fixed_rate';
DECLARE @indicator_number_of_pupils_with_one_or_more_fixed_period_exclusions NVARCHAR(max) = 'one_plus_fixed';

EXEC InsertIndicatorFootnote @subject_id_exclusions_by_characteristic, @indicator_permanent_exclusion_rate, @footnote_id_22;
EXEC InsertIndicatorFootnote @subject_id_exclusions_by_geographic_level, @indicator_permanent_exclusion_rate, @footnote_id_22;

EXEC InsertIndicatorFootnote @subject_id_exclusions_by_characteristic, @indicator_percentage_of_pupils_with_one_or_more_fixed_period_exclusions, @footnote_id_23;
EXEC InsertIndicatorFootnote @subject_id_exclusions_by_geographic_level, @indicator_percentage_of_pupils_with_one_or_more_fixed_period_exclusions, @footnote_id_23;

EXEC InsertIndicatorFootnote @subject_id_exclusions_by_characteristic, @indicator_fixed_period_exclusion_rate, @footnote_id_24;
EXEC InsertIndicatorFootnote @subject_id_exclusions_by_geographic_level, @indicator_fixed_period_exclusion_rate, @footnote_id_24;

EXEC InsertIndicatorFootnote @subject_id_exclusions_by_characteristic, @indicator_number_of_pupils, @footnote_id_25;
EXEC InsertIndicatorFootnote @subject_id_exclusions_by_geographic_level, @indicator_number_of_pupils, @footnote_id_25;

EXEC InsertIndicatorFootnote @subject_id_exclusions_by_characteristic, @indicator_number_of_pupils_with_one_or_more_fixed_period_exclusions, @footnote_id_29;
EXEC InsertIndicatorFootnote @subject_id_exclusions_by_characteristic, @indicator_percentage_of_pupils_with_one_or_more_fixed_period_exclusions, @footnote_id_29;

EXEC InsertIndicatorFootnote @subject_id_exclusions_by_geographic_level, @indicator_number_of_schools, @footnote_id_36;


--
-- Filter Items
--


DECLARE @filterItem_State_funded_primary NVARCHAR(max) = 'State-funded primary';
DECLARE @filterItem_State_funded_secondary NVARCHAR(max) = 'State-funded secondary';
DECLARE @filterItem_Special NVARCHAR(max) = 'Special';
DECLARE @filterItem_Social_emotional_and_mental_health NVARCHAR(max) = 'SEN primary need Social emotional and mental health';
DECLARE @filterItem_Ethnicity_Minority_Ethnic_Group NVARCHAR(max) = 'Ethnicity Minority Ethnic Group';

DECLARE @filterItem_SEN_provision_SEN_with_statement_EHC NVARCHAR(max) = 'SEN provision SEN with statement EHC';
DECLARE @filterItem_ethnicity_minority_ethnic_pupil NVARCHAR(max) = 'Ethnicity Minority ethnic pupil';
DECLARE @filterItem_ethnicity_minority_ethnic_pupil_male NVARCHAR(max) = 'Ethnicity Minority ethnic pupil Male';
DECLARE @filterItem_ethnicity_minority_ethnic_pupil_female NVARCHAR(max) = 'Ethnicity Minority ethnic pupil Female';
DECLARE @filterItem_ethnicity_unclassified NVARCHAR(max) = 'Ethnicity Unclassified';
DECLARE @filterItem_ethnicity_unclassified_male NVARCHAR(max) = 'Ethnicity Unclassified Male';
DECLARE @filterItem_ethnicity_unclassified_female NVARCHAR(max) = 'Ethnicity Unclassified Female';

EXEC InsertFilterItemFootnote @subject_id_absence_by_characteristic, @filterItem_State_funded_primary, @footnote_id_1;
EXEC InsertFilterItemFootnote @subject_id_absence_by_geographic_level, @filterItem_State_funded_primary, @footnote_id_1;
EXEC InsertFilterItemFootnote @subject_id_absence_by_term, @filterItem_State_funded_primary, @footnote_id_1;
EXEC InsertFilterItemFootnote @subject_id_absence_for_four_year_olds, @filterItem_State_funded_primary, @footnote_id_1;
EXEC InsertFilterItemFootnote @subject_id_absence_number_missing, @filterItem_State_funded_primary, @footnote_id_1;
EXEC InsertFilterItemFootnote @subject_id_absence_rate_percent_bands, @filterItem_State_funded_primary, @footnote_id_1;

EXEC InsertFilterItemFootnote @subject_id_absence_by_characteristic, @filterItem_State_funded_secondary, @footnote_id_2;
EXEC InsertFilterItemFootnote @subject_id_absence_by_geographic_level, @filterItem_State_funded_secondary, @footnote_id_2;
EXEC InsertFilterItemFootnote @subject_id_absence_by_term, @filterItem_State_funded_secondary, @footnote_id_2;
EXEC InsertFilterItemFootnote @subject_id_absence_for_four_year_olds, @filterItem_State_funded_secondary, @footnote_id_2;
EXEC InsertFilterItemFootnote @subject_id_absence_number_missing, @filterItem_State_funded_secondary, @footnote_id_2;
EXEC InsertFilterItemFootnote @subject_id_absence_rate_percent_bands, @filterItem_State_funded_secondary, @footnote_id_2;

EXEC InsertFilterItemFootnote @subject_id_absence_by_characteristic, @filterItem_Special, @footnote_id_3;
EXEC InsertFilterItemFootnote @subject_id_absence_by_geographic_level, @filterItem_Special, @footnote_id_3;
EXEC InsertFilterItemFootnote @subject_id_absence_by_term, @filterItem_Special, @footnote_id_3;
EXEC InsertFilterItemFootnote @subject_id_absence_for_four_year_olds, @filterItem_Special, @footnote_id_3;
EXEC InsertFilterItemFootnote @subject_id_absence_number_missing, @filterItem_Special, @footnote_id_3;
EXEC InsertFilterItemFootnote @subject_id_absence_rate_percent_bands, @filterItem_Special, @footnote_id_3;

EXEC InsertFilterItemFootnote @subject_id_absence_by_characteristic, @filterItem_Ethnicity_Minority_Ethnic_Group, @footnote_id_17;

EXEC InsertFilterItemFootnote @subject_id_absence_by_characteristic, @filterItem_Social_emotional_and_mental_health, @footnote_id_19;

EXEC InsertFilterItemFootnote @subject_id_exclusions_by_characteristic, @filterItem_SEN_provision_SEN_with_statement_EHC, @footnote_id_27;

EXEC InsertFilterItemFootnote @subject_id_exclusions_by_characteristic, @filterItem_ethnicity_minority_ethnic_pupil, @footnote_id_31;
EXEC InsertFilterItemFootnote @subject_id_exclusions_by_characteristic, @filterItem_ethnicity_minority_ethnic_pupil_male, @footnote_id_31;
EXEC InsertFilterItemFootnote @subject_id_exclusions_by_characteristic, @filterItem_ethnicity_minority_ethnic_pupil_female, @footnote_id_31;

EXEC InsertFilterItemFootnote @subject_id_exclusions_by_characteristic, @filterItem_ethnicity_unclassified, @footnote_id_33;
EXEC InsertFilterItemFootnote @subject_id_exclusions_by_characteristic, @filterItem_ethnicity_unclassified_male, @footnote_id_33;
EXEC InsertFilterItemFootnote @subject_id_exclusions_by_characteristic, @filterItem_ethnicity_unclassified_female, @footnote_id_33;

EXEC InsertFilterItemFootnote @subject_id_duration_of_fixed_exclusions, @filterItem_State_funded_primary, @footnote_id_37;
EXEC InsertFilterItemFootnote @subject_id_exclusions_by_characteristic, @filterItem_State_funded_primary, @footnote_id_37;
EXEC InsertFilterItemFootnote @subject_id_exclusions_by_geographic_level, @filterItem_State_funded_primary, @footnote_id_37;
EXEC InsertFilterItemFootnote @subject_id_exclusions_by_reason, @filterItem_State_funded_primary, @footnote_id_37;
EXEC InsertFilterItemFootnote @subject_id_number_of_fixed_exclusions, @filterItem_State_funded_primary, @footnote_id_37;
EXEC InsertFilterItemFootnote @subject_id_total_days_missed_due_to_fixed_period_exclusions, @filterItem_State_funded_primary, @footnote_id_37;

EXEC InsertFilterItemFootnote @subject_id_duration_of_fixed_exclusions, @filterItem_State_funded_secondary, @footnote_id_38;
EXEC InsertFilterItemFootnote @subject_id_exclusions_by_characteristic, @filterItem_State_funded_secondary, @footnote_id_38;
EXEC InsertFilterItemFootnote @subject_id_exclusions_by_geographic_level, @filterItem_State_funded_secondary, @footnote_id_38;
EXEC InsertFilterItemFootnote @subject_id_exclusions_by_reason, @filterItem_State_funded_secondary, @footnote_id_38;
EXEC InsertFilterItemFootnote @subject_id_number_of_fixed_exclusions, @filterItem_State_funded_secondary, @footnote_id_38;
EXEC InsertFilterItemFootnote @subject_id_total_days_missed_due_to_fixed_period_exclusions, @filterItem_State_funded_secondary, @footnote_id_38;

EXEC InsertFilterItemFootnote @subject_id_duration_of_fixed_exclusions, @filterItem_Special, @footnote_id_39;
EXEC InsertFilterItemFootnote @subject_id_exclusions_by_characteristic, @filterItem_Special, @footnote_id_39;
EXEC InsertFilterItemFootnote @subject_id_exclusions_by_geographic_level, @filterItem_Special, @footnote_id_39;
EXEC InsertFilterItemFootnote @subject_id_exclusions_by_reason, @filterItem_Special, @footnote_id_39;
EXEC InsertFilterItemFootnote @subject_id_number_of_fixed_exclusions, @filterItem_Special, @footnote_id_39;
EXEC InsertFilterItemFootnote @subject_id_total_days_missed_due_to_fixed_period_exclusions, @filterItem_Special, @footnote_id_39;


--
-- Filter Groups
--


DECLARE @filterGroup_gender NVARCHAR(max) = 'Gender';
DECLARE @filterGroup_ethnic_group_major NVARCHAR(max) = 'Ethnic group major';
DECLARE @filterGroup_ethnic_group_minor NVARCHAR(max) = 'Ethnic group minor';
DECLARE @filterGroup_NC_year NVARCHAR(max) = 'NC year';
DECLARE @filterGroup_FSM NVARCHAR(max) = 'FSM';
DECLARE @filterGroup_FSM_ever_6 NVARCHAR(max) = 'FSM ever 6';
DECLARE @filterGroup_SEN_provision NVARCHAR(max) = 'SEN provision';
DECLARE @filterGroup_SEN_primary_need NVARCHAR(max) = 'SEN primary need';
DECLARE @filterGroup_first_language NVARCHAR(max) = 'First language';

DECLARE @filterGroup_NC_year_and_gender NVARCHAR(max) = 'NC year and gender';
DECLARE @filterGroup_FSM_eligible NVARCHAR(max) = 'FSM eligible';
DECLARE @filterGroup_FSM_eligible_in_last_6_years NVARCHAR(max) = 'FSM eligible in last 6 years';
DECLARE @filterGroup_ethnicity NVARCHAR(max) = 'Ethnicity';
DECLARE @filterGroup_ethnicity_and_gender NVARCHAR(max) = 'Ethnicity and gender';
DECLARE @filterGroup_IDACI_decile NVARCHAR(max) = 'IDACI decile';

EXEC InsertFilterGroupFootnote @subject_id_absence_by_characteristic, @filterGroup_gender, @footnote_id_14;
EXEC InsertFilterGroupFootnote @subject_id_absence_by_characteristic, @filterGroup_ethnic_group_major, @footnote_id_14;
EXEC InsertFilterGroupFootnote @subject_id_absence_by_characteristic, @filterGroup_ethnic_group_minor, @footnote_id_14;
EXEC InsertFilterGroupFootnote @subject_id_absence_by_characteristic, @filterGroup_NC_year, @footnote_id_14;
EXEC InsertFilterGroupFootnote @subject_id_absence_by_characteristic, @filterGroup_FSM, @footnote_id_14;
EXEC InsertFilterGroupFootnote @subject_id_absence_by_characteristic, @filterGroup_FSM_ever_6, @footnote_id_14;
EXEC InsertFilterGroupFootnote @subject_id_absence_by_characteristic, @filterGroup_SEN_provision, @footnote_id_14;
EXEC InsertFilterGroupFootnote @subject_id_absence_by_characteristic, @filterGroup_SEN_primary_need, @footnote_id_14;
EXEC InsertFilterGroupFootnote @subject_id_absence_by_characteristic, @filterGroup_first_language, @footnote_id_14;

EXEC InsertFilterGroupFootnote @subject_id_absence_by_characteristic, @filterGroup_gender, @footnote_id_15;
EXEC InsertFilterGroupFootnote @subject_id_absence_by_characteristic, @filterGroup_ethnic_group_major, @footnote_id_15;
EXEC InsertFilterGroupFootnote @subject_id_absence_by_characteristic, @filterGroup_ethnic_group_minor, @footnote_id_15;
EXEC InsertFilterGroupFootnote @subject_id_absence_by_characteristic, @filterGroup_NC_year, @footnote_id_15;
EXEC InsertFilterGroupFootnote @subject_id_absence_by_characteristic, @filterGroup_FSM, @footnote_id_15;
EXEC InsertFilterGroupFootnote @subject_id_absence_by_characteristic, @filterGroup_FSM_ever_6, @footnote_id_15;
EXEC InsertFilterGroupFootnote @subject_id_absence_by_characteristic, @filterGroup_SEN_provision, @footnote_id_15;
EXEC InsertFilterGroupFootnote @subject_id_absence_by_characteristic, @filterGroup_SEN_primary_need, @footnote_id_15;
EXEC InsertFilterGroupFootnote @subject_id_absence_by_characteristic, @filterGroup_first_language, @footnote_id_15;

EXEC InsertFilterGroupFootnote @subject_id_absence_by_characteristic, @filterGroup_SEN_provision, @footnote_id_16;
EXEC InsertFilterGroupFootnote @subject_id_absence_by_characteristic, @filterGroup_SEN_primary_need, @footnote_id_18;

EXEC InsertFilterGroupFootnote @subject_id_exclusions_by_characteristic, @filterGroup_SEN_provision, @footnote_id_26;
EXEC InsertFilterGroupFootnote @subject_id_exclusions_by_characteristic, @filterGroup_SEN_primary_need, @footnote_id_28;
EXEC InsertFilterGroupFootnote @subject_id_exclusions_by_characteristic, @filterGroup_NC_year, @footnote_id_29;
EXEC InsertFilterGroupFootnote @subject_id_exclusions_by_characteristic, @filterGroup_NC_year_and_gender, @footnote_id_29;

EXEC InsertFilterGroupFootnote @subject_id_exclusions_by_characteristic, @filterGroup_FSM_eligible, @footnote_id_30;
EXEC InsertFilterGroupFootnote @subject_id_exclusions_by_characteristic, @filterGroup_FSM_eligible_in_last_6_years, @footnote_id_30;

EXEC InsertFilterGroupFootnote @subject_id_exclusions_by_characteristic, @filterGroup_ethnicity, @footnote_id_32;
EXEC InsertFilterGroupFootnote @subject_id_exclusions_by_characteristic, @filterGroup_ethnicity_and_gender, @footnote_id_32;

EXEC InsertFilterGroupFootnote @subject_id_exclusions_by_characteristic, @filterGroup_IDACI_decile, @footnote_id_34;

DROP PROCEDURE InsertSubjectFootnote;
DROP PROCEDURE InsertIndicatorFootnote;
DROP PROCEDURE InsertFilterFootnote;
DROP PROCEDURE InsertFilterGroupFootnote;
DROP PROCEDURE InsertFilterItemFootnote;
