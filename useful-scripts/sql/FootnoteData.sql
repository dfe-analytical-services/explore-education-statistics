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

DECLARE @subject_absence_by_characteristic   BIGINT = (SELECT Id FROM Subject WHERE Name = 'Absence by characteristic');
DECLARE @subject_absence_by_geographic_level BIGINT = (SELECT Id FROM Subject WHERE Name = 'Absence by geographic level');
DECLARE @subject_absence_by_term             BIGINT = (SELECT Id FROM Subject WHERE Name = 'Absence by term');
DECLARE @subject_absence_for_four_year_olds  BIGINT = (SELECT Id FROM Subject WHERE Name = 'Absence for four year olds');
DECLARE @subject_absence_in_prus             BIGINT = (SELECT Id FROM Subject WHERE Name = 'Absence in prus');
DECLARE @subject_absence_number_missing      BIGINT = (SELECT Id FROM Subject WHERE Name = 'Absence number missing at least one session by reason');
DECLARE @subject_absence_rate_percent_bands  BIGINT = (SELECT Id FROM Subject WHERE Name = 'Absence rate percent bands');

INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_characteristic, 1);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_geographic_level, 1);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_term, 1);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_for_four_year_olds, 1);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_in_prus, 1);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_number_missing, 1);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_rate_percent_bands, 1);

INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_characteristic, 2);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_geographic_level, 2);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_term, 2);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_for_four_year_olds, 2);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_in_prus, 2);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_number_missing, 2);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_rate_percent_bands, 2);

INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_characteristic, 3);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_geographic_level, 3);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_term, 3);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_for_four_year_olds, 3);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_in_prus, 3);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_number_missing, 3);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_rate_percent_bands, 3);

INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_characteristic, 4);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_geographic_level, 4);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_term, 4);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_for_four_year_olds, 4);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_in_prus, 4);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_number_missing, 4);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_rate_percent_bands, 4);

INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_characteristic, 5);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_geographic_level, 5);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_term, 5);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_for_four_year_olds, 5);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_in_prus, 5);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_number_missing, 5);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_rate_percent_bands, 5);

INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_characteristic, 6);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_geographic_level, 6);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_term, 6);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_for_four_year_olds, 6);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_in_prus, 6);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_number_missing, 6);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_rate_percent_bands, 6);

INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_characteristic, 7);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_geographic_level, 7);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_term, 7);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_for_four_year_olds, 7);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_in_prus, 7);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_number_missing, 7);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_rate_percent_bands, 7);

INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_characteristic, 8);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_geographic_level, 8);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_term, 8);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_for_four_year_olds, 8);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_in_prus, 8);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_number_missing, 8);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_rate_percent_bands, 8);

INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_characteristic, 9);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_geographic_level, 9);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_term, 9);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_for_four_year_olds, 9);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_in_prus, 9);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_number_missing, 9);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_rate_percent_bands, 9);

INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_characteristic, 10);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_geographic_level, 10);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_term, 10);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_for_four_year_olds, 10);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_in_prus, 10);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_number_missing, 10);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_rate_percent_bands, 10);

INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_characteristic, 11);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_geographic_level, 11);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_term, 11);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_for_four_year_olds, 11);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_in_prus, 11);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_number_missing, 11);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_rate_percent_bands, 11);

-- Except Absence for four year olds
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_characteristic, 12);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_geographic_level, 12);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_term, 12);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_in_prus, 12);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_number_missing, 12);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_rate_percent_bands, 12);

INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_characteristic, 13);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_geographic_level, 13);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_term, 13);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_for_four_year_olds, 13);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_in_prus, 13);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_number_missing, 13);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_rate_percent_bands, 13);

INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_characteristic, 14);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_geographic_level, 14);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_term, 14);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_for_four_year_olds, 14);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_in_prus, 14);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_number_missing, 14);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_rate_percent_bands, 14);

INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_characteristic, 15);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_geographic_level, 15);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_term, 15);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_for_four_year_olds, 15);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_in_prus, 15);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_number_missing, 15);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_rate_percent_bands, 15);

INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_characteristic, 16);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_geographic_level, 16);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_term, 16);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_for_four_year_olds, 16);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_in_prus, 16);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_number_missing, 16);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_rate_percent_bands, 16);

INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_characteristic, 17);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_geographic_level, 17);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_term, 17);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_for_four_year_olds, 17);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_in_prus, 17);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_number_missing, 17);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_rate_percent_bands, 17);

INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_characteristic, 18);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_geographic_level, 18);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_term, 18);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_for_four_year_olds, 18);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_in_prus, 18);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_number_missing, 18);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_rate_percent_bands, 18);

INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_characteristic, 19);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_geographic_level, 19);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_term, 19);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_for_four_year_olds, 19);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_in_prus, 19);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_number_missing, 19);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_rate_percent_bands, 19);

INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_characteristic, 20);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_geographic_level, 20);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_term, 20);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_for_four_year_olds, 20);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_in_prus, 20);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_number_missing, 20);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_rate_percent_bands, 20);

INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_characteristic, 21);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_geographic_level, 21);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_term, 21);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_for_four_year_olds, 21);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_in_prus, 21);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_number_missing, 21);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_rate_percent_bands, 21);

INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_characteristic, 22);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_geographic_level, 22);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_term, 22);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_for_four_year_olds, 22);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_in_prus, 22);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_number_missing, 22);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_rate_percent_bands, 22);

INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_characteristic, 23);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_geographic_level, 23);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_by_term, 23);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_for_four_year_olds, 23);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_in_prus, 23);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_number_missing, 23);
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_rate_percent_bands, 23);

-- Only Absence for four year olds
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_for_four_year_olds, 24);

-- Only Absence for four year olds
INSERT INTO SubjectFootnote (SubjectId, FootnoteId) VALUES (@subject_absence_for_four_year_olds, 25);

--
-- Indicators
--

-- sess_overall_percent
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (26, 8);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (57, 8);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (85, 8);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (95, 8);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (115, 8);
-- sess_authorised_percent
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (28, 8);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (59, 8);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (84, 8);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (114, 8);
-- sess_unauthorised_percent
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (23, 8);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (66, 8);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (83, 8);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (113, 8);


-- enrolments_pa_10_exact
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (31, 9);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (60, 9);
-- enrolments_pa_10_exact_percent
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (30, 9);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (61, 9);
-- sess_possible_pa_10_exact
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (2, 9);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (62, 9);
-- sess_overall_pa_10_exact
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (4, 9);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (63, 9);
-- sess_authorised_pa_10_exact
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (6, 9);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (64, 9);
-- sess_unauthorised_pa_10_exact
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (16, 9);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (65, 9);
-- sess_overall_percent_pa_10_exact
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (3, 9);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (51, 9);
-- sess_authorised_percent_pa_10_exact
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (5, 9);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (50, 9);
-- sess_unauthorised_percent_pa_10_exact
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (33, 9);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (49, 9);


-- enrolments_pa_10_exact
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (31, 10);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (60, 10);
-- enrolments_pa_10_exact_percent
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (30, 10);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (61, 10);
-- sess_possible_pa_10_exact
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (2, 10);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (62, 10);
-- sess_overall_pa_10_exact
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (4, 10);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (63, 10);
-- sess_authorised_pa_10_exact
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (6, 10);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (64, 10);
-- sess_unauthorised_pa_10_exact
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (16, 10);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (65, 10);
-- sess_overall_percent_pa_10_exact
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (3, 10);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (51, 10);
-- sess_authorised_percent_pa_10_exact
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (5, 10);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (50, 10);
-- sess_unauthorised_percent_pa_10_exact
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (33, 10);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (49, 10);


-- enrolments_pa10_exact_percent
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (30, 11);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (61, 11);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (111, 11);


--enrolments
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (1, 12);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (52, 12);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (120, 12);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (123, 12);


-- enrol_unauth_late
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (124, 13);
-- enrol_unauth_holiday
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (125, 13);
-- enrol_auth_other
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (126, 13);
-- enrol_auth_excluded
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (127, 13);
-- enrol_auth_ext_holiday
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (128, 13);
-- enrol_auth_holiday
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (129, 13);
-- enrol_unauth_other
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (130, 13);
-- enrol_auth_traveller
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (131, 13);
-- enrol_auth_religious
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (132, 13);
-- enrol_auth_appointments
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (133, 13);
-- enrol_auth_illness
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (134, 13);
-- enrol_unauthorised
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (135, 13);
-- enrol_authorised
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (136, 13);
-- enrol_overall
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (137, 13);
-- enrol_auth_study
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (138, 13);
-- enrol_unauth_noyet
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (139, 13);


-- enrolments_overall_percent
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (141, 14);
-- enrolments_authorised_percent
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (143, 14);
-- enrolments_unauthorised_percent
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (145, 14);


-- num_schools
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (34, 15);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (91, 15);
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (96, 15);


--enrolments
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (92, 24);


-- sess_overall
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (94, 25);


-- sess_overall_percent
INSERT INTO IndicatorFootnote (IndicatorId, FootnoteId) VALUES (95, 25);


--
-- Footnotes
--

-- State-funded primary 
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (71, 1);
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (86, 1);
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (89, 1);
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (94, 1);
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (98, 1);
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (149, 1);
-- State-funded secondary
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (75, 1);
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (87, 1);
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (90, 1);
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (92, 1);
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (99, 1);
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (151, 1);


-- State-funded primary
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (71, 2);
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (86, 2);
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (89, 2);
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (94, 2);
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (98, 2);
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (149, 2);


-- State-funded secondary
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (75, 3);
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (87, 3);
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (90, 3);
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (92, 3);
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (99, 3);
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (151, 3);


-- Special
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (74, 4);
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (85, 4);
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (91, 4);
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (93, 4);
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (100, 4);
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (150, 4);


-- Characteristic
INSERT INTO FilterFootnote (FilterId, FootnoteId) VALUES (1, 16);


-- Characteristic
INSERT INTO FilterFootnote (FilterId, FootnoteId) VALUES (1, 17);


-- SEN provision
INSERT INTO FilterGroupFootnote (FilterGroupId, FootnoteId) VALUES (10, 18);


-- SEN provision
INSERT INTO FilterGroupFootnote (FilterGroupId, FootnoteId) VALUES (10, 19);


-- SEN provision
INSERT INTO FilterGroupFootnote (FilterGroupId, FootnoteId) VALUES (10, 20);

-- Ethnic group minor
INSERT INTO FilterGroupFootnote (FilterGroupId, FootnoteId) VALUES (6, 21);


-- SEN primary need
INSERT INTO FilterGroupFootnote (FilterGroupId, FootnoteId) VALUES (11, 22);


-- SEN primary need Social emotional and mental health
INSERT INTO FilterItemFootnote (FilterItemId, FootnoteId) VALUES (58, 23);
