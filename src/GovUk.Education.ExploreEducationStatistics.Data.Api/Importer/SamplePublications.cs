using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer
{
    public static class SamplePublications
    {
        public static readonly Dictionary<string, Publication> Publications = new Dictionary<string, Publication>
        {
            {
                "absence", new Publication
                {
                    PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                    Name = "Pupil absence in schools in England",
                    AttributeMetas = new[]
                    {
                        new AttributeMeta
                        {
                            Name = "all_through",
                            Label = "All through school flag",
                            Group = "Choice",
                            KeyIndicator = false,
                            Unit = Unit.String
                        },
                        new AttributeMeta
                        {
                            Name = "num_schools",
                            Label = "Number of schools",
                            Group = "Absence fields",
                            KeyIndicator = false,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "enrolments",
                            Label = "Number of pupil enrolments",
                            Group = "Absence fields",
                            KeyIndicator = true,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "sess_possible",
                            Label = "Number of sessions possible",
                            Group = "Absence fields",
                            KeyIndicator = false,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "sess_overall",
                            Label = "Number of overall absence sessions",
                            Group = "Absence fields",
                            KeyIndicator = false,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "sess_authorised",
                            Label = "Number of authorised absence sessions",
                            Group = "Absence fields",
                            KeyIndicator = false,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "sess_unauthorised",
                            Label = "Number of unauthorised absence sessions",
                            Group = "Absence fields",
                            KeyIndicator = false,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "sess_overall_percent",
                            Label = "Overall absence rate",
                            Group = "Absence fields",
                            KeyIndicator = true,
                            Unit = Unit.Percent
                        },
                        new AttributeMeta
                        {
                            Name = "sess_authorised_percent",
                            Label = "Authorised absence rate",
                            Group = "Absence fields",
                            KeyIndicator = true,
                            Unit = Unit.Percent
                        },
                        new AttributeMeta
                        {
                            Name = "sess_unauthorised_percent",
                            Label = "Unauthorised absence rate",
                            Group = "Absence fields",
                            KeyIndicator = true,
                            Unit = Unit.Percent
                        },
                        new AttributeMeta
                        {
                            Name = "enrolments_PA_10_exact",
                            Label = "Number of persistent absentees",
                            Group = "Absence fields",
                            KeyIndicator = true,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "enrolments_pa_10_exact_percent",
                            Label = "Percentage of persistent absentees",
                            Group = "Absence fields",
                            KeyIndicator = true,
                            Unit = Unit.Percent
                        },
                        new AttributeMeta
                        {
                            Name = "sess_possible_pa_10_exact",
                            Label = "Number of sessions possible for persistent absentees",
                            Group = "Absence for persistent absentees",
                            KeyIndicator = false,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "sess_overall_pa_10_exact",
                            Label = "Number of overall absence sessions for persistent absentees",
                            Group = "Absence for persistent absentees",
                            KeyIndicator = false,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "sess_authorised_pa_10_exact",
                            Label = "Number of authorised absence sessions for persistent absentees",
                            Group = "Absence for persistent absentees",
                            KeyIndicator = false,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "sess_unauthorised_pa_10_exact",
                            Label = "Number of unauthorised absence sessions for persistent absentees",
                            Group = "Absence for persistent absentees",
                            KeyIndicator = false,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "sess_overall_percent_pa_10_exact",
                            Label = "Overall absence rate for persistent absentees",
                            Group = "Absence for persistent absentees",
                            KeyIndicator = false,
                            Unit = Unit.Percent
                        },
                        new AttributeMeta
                        {
                            Name = "sess_authorised_percent_pa_10_exact",
                            Label = "Authorised absence rate for persistent absentees",
                            Group = "Absence for persistent absentees",
                            KeyIndicator = false,
                            Unit = Unit.Percent
                        },
                        new AttributeMeta
                        {
                            Name = "sess_unauthorised_percent_pa_10_exact",
                            Label = "Unauthorised absence rate for persistent absentees",
                            Group = "Absence for persistent absentees",
                            KeyIndicator = false,
                            Unit = Unit.Percent
                        },
                        new AttributeMeta
                        {
                            Name = "sess_auth_illness",
                            Label = "Number of illness sessions",
                            Group = "Absence by reason",
                            KeyIndicator = false,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "sess_auth_appointments",
                            Label = "Number of medical appointments sessions",
                            Group = "Absence by reason",
                            KeyIndicator = false,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "sess_auth_religious",
                            Label = "Number of religious observance sessions",
                            Group = "Absence by reason",
                            KeyIndicator = false,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "sess_auth_study",
                            Label = "Number of study leave sessions",
                            Group = "Absence by reason",
                            KeyIndicator = false,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "sess_auth_traveller",
                            Label = "Number of traveller sessions",
                            Group = "Absence by reason",
                            KeyIndicator = false,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "sess_auth_holiday",
                            Label = "Number of authorised holiday sessions",
                            Group = "Absence by reason",
                            KeyIndicator = false,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "sess_auth_ext_holiday",
                            Label = "Number of extended authorised holiday sessions",
                            Group = "Absence by reason",
                            KeyIndicator = false,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "sess_auth_excluded",
                            Label = "Number of excluded sessions",
                            Group = "Absence by reason",
                            KeyIndicator = false,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "sess_auth_other",
                            Label = "Number of authorised other sessions",
                            Group = "Absence by reason",
                            KeyIndicator = false,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "sess_auth_totalreasons",
                            Label = "Number of authorised reasons sessions",
                            Group = "Absence by reason",
                            KeyIndicator = false,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "sess_unauth_holiday",
                            Label = "Number of unauthorised holiday sessions",
                            Group = "Absence by reason",
                            KeyIndicator = false,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "sess_unauth_late",
                            Label = "Number of late sessions",
                            Group = "Absence by reason",
                            KeyIndicator = false,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "sess_unauth_other",
                            Label = "Number of unauthorised other sessions",
                            Group = "Absence by reason",
                            KeyIndicator = false,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "sess_unauth_noyet",
                            Label = "Number of no reason yet sessions",
                            Group = "Absence by reason",
                            KeyIndicator = false,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "sess_unauth_totalreasons",
                            Label = "Number of unauthorised reasons sessions",
                            Group = "Absence by reason",
                            KeyIndicator = false,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "sess_overall_totalreasons",
                            Label = "Number of overall reasons sessions",
                            Group = "Absence by reason",
                            KeyIndicator = false,
                            Unit = Unit.Number
                        }
                    },
                    CharacteristicMetas = new[]
                    {
                        new CharacteristicMeta
                        {
                            Name = "Total",
                            Label = "All pupils",
                            Group = "Total"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Gender_male",
                            Label = "Boys",
                            Group = "Gender"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Gender_female",
                            Label = "Girl",
                            Group = "Gender"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Major_White_Total",
                            Label = "White",
                            Group = "Ethnic group major"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_White_British",
                            Label = "White British",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Irish",
                            Label = "Irish",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Traveller_of_Irish_heritage",
                            Label = "Traveller of Irish Heritage",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Gypsy_Roma",
                            Label = "Gypsy Roma",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Any_other_white_background",
                            Label = "Any other White background",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Major_Mixed_Total",
                            Label = "Mixed",
                            Group = "Ethnic group major"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_White_and_Black_Caribbean",
                            Label = "White and Black Caribbean",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_White_and_Black_African",
                            Label = "White and Black African",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_White_and_Asian",
                            Label = "White and Asian",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Any_other_Mixed_background",
                            Label = "Any other Mixed Background",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Major_Asian_Total",
                            Label = "Asian",
                            Group = "Ethnic group major"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Indian",
                            Label = "India",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Pakistani",
                            Label = "Pakistani",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Bangladeshi",
                            Label = "Bangladeshi",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Any_other_Asian_background",
                            Label = "Any other Asian background",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Major_Black_Total",
                            Label = "Black",
                            Group = "Ethnic group major"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Black_Caribbean",
                            Label = "Black Caribbean",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Black_African",
                            Label = "Black African",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Any_other_black_background",
                            Label = "Any other Black background",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Major_Chinese",
                            Label = "Chinese",
                            Group = "Ethnic group major"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Major_Any_Other_Ethnic_Group",
                            Label = "Any other ethnic group",
                            Group = "Ethnic group major"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minority_Ethnic_Group",
                            Label = "Minority ethnic group",
                            Group = "Ethnic group major"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Major_Unclassified",
                            Label = "Ethnicity unclassified",
                            Group = "Ethnic group major"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_1_and_below",
                            Label = "Year 1",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_2",
                            Label = "Year 2",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_3",
                            Label = "Year 3",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_4",
                            Label = "Year 4",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_5",
                            Label = "Year 5",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_6",
                            Label = "Year 6",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_7",
                            Label = "Year 7",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_8",
                            Label = "Year 8 ",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_9",
                            Label = "Year 9",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_10",
                            Label = "Year 10",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_11",
                            Label = "Year 11",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_12_and_above",
                            Label = "Year 12 and above",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_Not_followed_or_missing",
                            Label = "NC year missing",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "FSM_eligible",
                            Label = "FSM eligible",
                            Group = "Free school meals"
                        },
                        new CharacteristicMeta
                        {
                            Name = "FSM_Not_eligible",
                            Label = "FSM not eligible",
                            Group = "Free school meals"
                        },
                        new CharacteristicMeta
                        {
                            Name = "FSM_unclassified",
                            Label = "FSM unclassified",
                            Group = "Free school meals"
                        },
                        new CharacteristicMeta
                        {
                            Name = "FSM_eligible_in_last_6_years",
                            Label = "FSM eligible at some point in last 6 years",
                            Group = "Free school meals in last 6 years"
                        },
                        new CharacteristicMeta
                        {
                            Name = "FSM_not_eligible_in_last_6_years",
                            Label = "FSM not eligible at some point in last 6 years",
                            Group = "Free school meals in last 6 years"
                        },
                        new CharacteristicMeta
                        {
                            Name = "FSM_unclassified_in_last_6_years",
                            Label = "FSM in last 6 years unclassified",
                            Group = "Free school meals in last 6 years"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_provision_No_identified_SEN",
                            Label = "No identified SEN",
                            Group = "SEN provision"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_provision_Statement_or_EHCP",
                            Label = "Statement of SEN or EHC plan",
                            Group = "SEN provision"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_provision_SEN_Support",
                            Label = "SEN support",
                            Group = "SEN provision"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_provision_Unclassified",
                            Label = "SEN unclassified",
                            Group = "SEN provision"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Autistic_spectrum_disorder",
                            Label = "Autistic Spectrum Disorder",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Hearing_impairment",
                            Label = "Hearing Impairment",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Moderate_learning_difficulty",
                            Label = "Moderate Learning Difficulty",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Multi-sensory_impairment",
                            Label = "Multi-sensory impairment",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_No_specialist_assessment",
                            Label = "No specialist assessment",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Other_difficulty/disability",
                            Label = "Other difficulty or disabilty",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Physical_disability",
                            Label = "Physical disability",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Profound_and_multiple_learning_difficulty",
                            Label = "Profound and multiple learning difficulty",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Social_emotional_and_mental_health",
                            Label = "Social emotional and mental health",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Speech_language_and_communications_needs",
                            Label = "Speech language and communications needs",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Severe_learning_difficulty",
                            Label = "Severe learning difficulty",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Specific_learning_difficulty",
                            Label = "Specific learning difficulty",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Visual_impairment",
                            Label = "Visual impairment",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Unclassified",
                            Label = "Primary need Unclassified",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "First_language_Known_or_believed_to_be_English",
                            Label = "First language Known or believed to be English",
                            Group = "First language"
                        },
                        new CharacteristicMeta
                        {
                            Name = "First_language_Known_or_believed_to_be_other_than_English",
                            Label = "First language Known or believed to be other than English",
                            Group = "First language"
                        },
                        new CharacteristicMeta
                        {
                            Name = "First_language_Unclassified",
                            Label = "First language Unclassified",
                            Group = "First language"
                        }
                    },
                    Releases = new[]
                    {
                        new Release
                        {
                            PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                            ReleaseId = new Guid("1d395b31-a68e-489c-a257-b3ab5c40bb01"),
                            ReleaseDate = new DateTime(2018, 4, 25),
                            Filenames = new[]
                            {
                                //DataCsvFilename.absence_geoglevels,
                                //DataCsvFilename.absence_lacharacteristics,
                                DataCsvFilename.absence_natcharacteristics
                            }
                        }
                    }
                }
            },

            {
                "exclusion", new Publication
                {
                    PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                    Name = "Permanent and fixed period exclusions",
                    AttributeMetas = new[]
                    {
                        new AttributeMeta
                        {
                            Name = "num_schools",
                            Label = "Number of schools",
                            Group = "Exclusion fields",
                            KeyIndicator = false,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "headcount",
                            Label = "Number of pupils",
                            Group = "Exclusion fields",
                            KeyIndicator = false,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "perm_excl",
                            Label = "Number of permanent exclusions",
                            Group = "Exclusion fields",
                            KeyIndicator = true,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "perm_excl_rate",
                            Label = "Permanent exclusion rate",
                            Group = "Exclusion fields",
                            KeyIndicator = true,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "fixed_excl",
                            Label = "Number of fixed period exclusions",
                            Group = "Exclusion fields",
                            KeyIndicator = true,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "fixed_excl_rate",
                            Label = "Fixed period exclusion rate",
                            Group = "Exclusion fields",
                            KeyIndicator = true,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "one_plus_fixed",
                            Label = "Number of enrolments with one or more fixed period exclusions",
                            Group = "Exclusion fields",
                            KeyIndicator = true,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "one_plus_fixed_rate",
                            Label = "Percentage of enrolments with one or more fixed period exclusions",
                            Group = "Exclusion fields",
                            KeyIndicator = true,
                            Unit = Unit.Percent
                        }
                    },
                    CharacteristicMetas = new[]
                    {
                        new CharacteristicMeta
                        {
                            Name = "Total",
                            Label = "All pupils",
                            Group = "Total"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Gender_male",
                            Label = "Boys",
                            Group = "Gender"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Gender_female",
                            Label = "Girl",
                            Group = "Gender"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_4_and_under",
                            Label = "Age four and under",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_5",
                            Label = "Age 5",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_6",
                            Label = "Age 6",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_7",
                            Label = "Age 7",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_8",
                            Label = "Age 8",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_9",
                            Label = "Age 9",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_10",
                            Label = "Age 10",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_11",
                            Label = "Age 11",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_12",
                            Label = "Age 12",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_13",
                            Label = "Age 13",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_14",
                            Label = "Age 14",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_15",
                            Label = "Age 15",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_16",
                            Label = "Age 16",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_17",
                            Label = "Age 17",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_18",
                            Label = "Age 18",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_19_and_over",
                            Label = "Age 19 and over",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Major_White_Total",
                            Label = "White",
                            Group = "Ethnic group major"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_White_British",
                            Label = "White British",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Irish",
                            Label = "Irish",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Traveller_of_Irish_heritage",
                            Label = "Traveller of Irish Heritage",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Gypsy_Roma",
                            Label = "Gypsy Roma",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Any_other_white_background",
                            Label = "Any other White background",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Major_Mixed_Total",
                            Label = "Mixed",
                            Group = "Ethnic group major"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_White_and_Black_Caribbean",
                            Label = "White and Black Caribbean",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_White_and_Black_African",
                            Label = "White and Black African",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_White_and_Asian",
                            Label = "White and Asian",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Any_other_Mixed_background",
                            Label = "Any other Mixed Background",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Major_Asian_Total",
                            Label = "Asian",
                            Group = "Ethnic group major"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Indian",
                            Label = "India",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Pakistani",
                            Label = "Pakistani",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Bangladeshi",
                            Label = "Bangladeshi",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Any_other_Asian_background",
                            Label = "Any other Asian background",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Major_Black_Total",
                            Label = "Black",
                            Group = "Ethnic group major"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Black_Caribbean",
                            Label = "Black Caribbean",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Black_African",
                            Label = "Black African",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Any_other_black_background",
                            Label = "Any other Black background",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Major_Chinese",
                            Label = "Chinese",
                            Group = "Ethnic group major"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Major_Any_Other_Ethnic_Group",
                            Label = "Any other ethnic group",
                            Group = "Ethnic group major"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minority_ethnic_group",
                            Label = "Minority ethnic group",
                            Group = "Ethnic group major"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Major_Unclassified",
                            Label = "Ethnicity unclassified",
                            Group = "Ethnic group major"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_Nursery",
                            Label = "Nursery",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_Reception",
                            Label = "Reception",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_1",
                            Label = "Year 1",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_2",
                            Label = "Year 2",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_3",
                            Label = "Year 3",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_4",
                            Label = "Year 4",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_5",
                            Label = "Year 5",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_6",
                            Label = "Year 6",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_7",
                            Label = "Year 7",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_8",
                            Label = "Year 8 ",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_9",
                            Label = "Year 9",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_10",
                            Label = "Year 10",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_11",
                            Label = "Year 11",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_12_and_above",
                            Label = "Year 12 and above",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_Not_followed",
                            Label = "NC year not followed",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_Unclassified",
                            Label = "NC year unclassified",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "FSM_Eligible",
                            Label = "FSM eligible",
                            Group = "Free school meals"
                        },
                        new CharacteristicMeta
                        {
                            Name = "FSM_Not_eligible",
                            Label = "FSM not eligible",
                            Group = "Free school meals"
                        },
                        new CharacteristicMeta
                        {
                            Name = "FSM_Unclassified",
                            Label = "FSM unclassified",
                            Group = "Free school meals"
                        },
                        new CharacteristicMeta
                        {
                            Name = "FSM_eligible_in_last_6_years",
                            Label = "FSM eligible at some point in last 6 years",
                            Group = "Free school meals in last 6 years"
                        },
                        new CharacteristicMeta
                        {
                            Name = "FSM_not_eligible_in_last_6_years",
                            Label = "FSM not eligible at some point in last 6 years",
                            Group = "Free school meals in last 6 years"
                        },
                        new CharacteristicMeta
                        {
                            Name = "FSM_unclassified_in_last_6_years",
                            Label = "FSM in last 6 years unclassified",
                            Group = "Free school meals in last 6 years"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_provision_No_identified_SEN",
                            Label = "No identified SEN",
                            Group = "SEN provision"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_provision_Statement_or_EHCP",
                            Label = "Statement of SEN or EHC plan",
                            Group = "SEN provision"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_provision_SEN_Support",
                            Label = "SEN support",
                            Group = "SEN provision"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Autistic_spectrum_disorder",
                            Label = "Autistic Spectrum Disorder",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Hearing_impairment",
                            Label = "Hearing Impairment",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Moderate_learning_difficulty",
                            Label = "Moderate Learning Difficulty",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Multi-sensory_impairment",
                            Label = "Multi-sensory impairment",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_No_specialist_assessment",
                            Label = "No specialist assessment",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Other_difficulty/disability",
                            Label = "Other difficulty or disabilty",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Physical_disability",
                            Label = "Physical disability",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Profound_and_multiple_learning_difficulty",
                            Label = "Profound and multiple learning difficulty",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Social_emotional_and_mental_health",
                            Label = "Social emotional and mental health",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Speech_language_and_communications_needs",
                            Label = "Speech language and communications needs",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Severe_learning_difficulty",
                            Label = "Severe learning difficulty",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Specific_learning_difficulty",
                            Label = "Specific learning difficulty",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Visual_impairment",
                            Label = "Visual impairment",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "IDACI_decile_0_to_10pc_most_deprived",
                            Label = "decile 0 to 10 (most deprived)",
                            Group = "IDACI"
                        },
                        new CharacteristicMeta
                        {
                            Name = "IDACI_decile_10_to_20pc",
                            Label = "decile 10 to 20",
                            Group = "IDACI"
                        },
                        new CharacteristicMeta
                        {
                            Name = "IDACI_decile_20_to_30pc",
                            Label = "decile 20 to 30 ",
                            Group = "IDACI"
                        },
                        new CharacteristicMeta
                        {
                            Name = "IDACI_decile_30_to_40pc",
                            Label = "decile 30 to 40",
                            Group = "IDACI"
                        },
                        new CharacteristicMeta
                        {
                            Name = "IDACI_decile_40_to_50pc",
                            Label = "decile 40 to 50",
                            Group = "IDACI"
                        },
                        new CharacteristicMeta
                        {
                            Name = "IDACI_decile_50_to_60pc",
                            Label = "decile 50 to 60",
                            Group = "IDACI"
                        },
                        new CharacteristicMeta
                        {
                            Name = "IDACI_decile_60_to_70pc",
                            Label = "decile 60 to 70",
                            Group = "IDACI"
                        },
                        new CharacteristicMeta
                        {
                            Name = "IDACI_decile_70_to_80pc",
                            Label = "decile 70 to 80",
                            Group = "IDACI"
                        },
                        new CharacteristicMeta
                        {
                            Name = "IDACI_decile_80_to_90pc",
                            Label = "decile 80 to 90",
                            Group = "IDACI"
                        },
                        new CharacteristicMeta
                        {
                            Name = "IDACI_decile_90_to_100pc_least_deprived",
                            Label = "decile 90 to 100 (least deprived)",
                            Group = "IDACI"
                        },
                        new CharacteristicMeta
                        {
                            Name = "IDACI_decile_unclassified",
                            Label = "idaci unclassified",
                            Group = "IDACI"
                        }
                    },
                    Releases = new[]
                    {
                        new Release
                        {
                            PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                            ReleaseId = new Guid("ac602576-2d07-4324-8480-0cabb6294814"),
                            ReleaseDate = new DateTime(2018, 3, 22),
                            Filenames = new[]
                            {
                                //DataCsvFilename.exclusion_geoglevels,
                                //DataCsvFilename.exclusion_lacharacteristics,
                                DataCsvFilename.exclusion_natcharacteristics
                            }
                        }
                    }
                }
            },
            {
                "schpupnum", new Publication
                {
                    PublicationId = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                    Name = "Schools, pupils and their characteristics",
                    AttributeMetas = new[]
                    {
                        new AttributeMeta
                        {
                            Name = "num_schools",
                            Label = "Number of schools",
                            Group = "Exclusion fields",
                            KeyIndicator = false,
                            Unit = Unit.Number
                        },
                        new AttributeMeta
                        {
                            Name = "headcount",
                            Label = "Number of pupils",
                            Group = "Exclusion fields",
                            KeyIndicator = false,
                            Unit = Unit.Number
                        }
                    },
                    CharacteristicMetas = new[]
                    {
                        new CharacteristicMeta
                        {
                            Name = "Total",
                            Label = "All pupils",
                            Group = "Total"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Gender_male",
                            Label = "Boys",
                            Group = "Gender"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Gender_female",
                            Label = "Girl",
                            Group = "Gender"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_4_and_under",
                            Label = "Age four and under",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_5",
                            Label = "Age 5",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_6",
                            Label = "Age 6",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_7",
                            Label = "Age 7",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_8",
                            Label = "Age 8",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_9",
                            Label = "Age 9",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_10",
                            Label = "Age 10",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_11",
                            Label = "Age 11",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_12",
                            Label = "Age 12",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_13",
                            Label = "Age 13",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_14",
                            Label = "Age 14",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_15",
                            Label = "Age 15",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_16",
                            Label = "Age 16",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_17",
                            Label = "Age 17",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_18",
                            Label = "Age 18",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Age_19_and_over",
                            Label = "Age 19 and over",
                            Group = "Age"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Major_White_Total",
                            Label = "White",
                            Group = "Ethnic group major"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_White_British",
                            Label = "White British",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Irish",
                            Label = "Irish",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Traveller_of_Irish_heritage",
                            Label = "Traveller of Irish Heritage",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Gypsy_Roma",
                            Label = "Gypsy Roma",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Any_other_white_background",
                            Label = "Any other White background",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Major_Mixed_Total",
                            Label = "Mixed",
                            Group = "Ethnic group major"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_White_and_Black_Caribbean",
                            Label = "White and Black Caribbean",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_White_and_Black_African",
                            Label = "White and Black African",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_White_and_Asian",
                            Label = "White and Asian",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Any_other_Mixed_background",
                            Label = "Any other Mixed Background",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Major_Asian_Total",
                            Label = "Asian",
                            Group = "Ethnic group major"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Indian",
                            Label = "India",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Pakistani",
                            Label = "Pakistani",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Bangladeshi",
                            Label = "Bangladeshi",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Any_other_Asian_background",
                            Label = "Any other Asian background",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Major_Black_Total",
                            Label = "Black",
                            Group = "Ethnic group major"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Black_Caribbean",
                            Label = "Black Caribbean",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Black_African",
                            Label = "Black African",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Any_other_black_background",
                            Label = "Any other Black background",
                            Group = "Ethnic group minor"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Major_Chinese",
                            Label = "Chinese",
                            Group = "Ethnic group major"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Major_Any_Other_Ethnic_Group",
                            Label = "Any other ethnic group",
                            Group = "Ethnic group major"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minority_ethnic_group",
                            Label = "Minority ethnic group",
                            Group = "Ethnic group major"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Major_Unclassified",
                            Label = "Ethnicity unclassified",
                            Group = "Ethnic group major"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_Nursery",
                            Label = "Nursery",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_Reception",
                            Label = "Reception",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_1",
                            Label = "Year 1",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_2",
                            Label = "Year 2",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_3",
                            Label = "Year 3",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_4",
                            Label = "Year 4",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_5",
                            Label = "Year 5",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_6",
                            Label = "Year 6",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_7",
                            Label = "Year 7",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_8",
                            Label = "Year 8 ",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_9",
                            Label = "Year 9",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_10",
                            Label = "Year 10",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_11",
                            Label = "Year 11",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_12_and_above",
                            Label = "Year 12 and above",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_Not_followed",
                            Label = "NC year not followed",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_Unclassified",
                            Label = "NC year unclassified",
                            Group = "NC year"
                        },
                        new CharacteristicMeta
                        {
                            Name = "FSM_Eligible",
                            Label = "FSM eligible",
                            Group = "Free school meals"
                        },
                        new CharacteristicMeta
                        {
                            Name = "FSM_Not_eligible",
                            Label = "FSM not eligible",
                            Group = "Free school meals"
                        },
                        new CharacteristicMeta
                        {
                            Name = "FSM_Unclassified",
                            Label = "FSM unclassified",
                            Group = "Free school meals"
                        },
                        new CharacteristicMeta
                        {
                            Name = "FSM_eligible_in_last_6_years",
                            Label = "FSM eligible at some point in last 6 years",
                            Group = "Free school meals in last 6 years"
                        },
                        new CharacteristicMeta
                        {
                            Name = "FSM_not_eligible_in_last_6_years",
                            Label = "FSM not eligible at some point in last 6 years",
                            Group = "Free school meals in last 6 years"
                        },
                        new CharacteristicMeta
                        {
                            Name = "FSM_unclassified_in_last_6_years",
                            Label = "FSM in last 6 years unclassified",
                            Group = "Free school meals in last 6 years"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_provision_No_identified_SEN",
                            Label = "No identified SEN",
                            Group = "SEN provision"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_provision_Statement_or_EHCP",
                            Label = "Statement of SEN or EHC plan",
                            Group = "SEN provision"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_provision_SEN_Support",
                            Label = "SEN support",
                            Group = "SEN provision"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Autistic_spectrum_disorder",
                            Label = "Autistic Spectrum Disorder",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Hearing_impairment",
                            Label = "Hearing Impairment",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Moderate_learning_difficulty",
                            Label = "Moderate Learning Difficulty",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Multi-sensory_impairment",
                            Label = "Multi-sensory impairment",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_No_specialist_assessment",
                            Label = "No specialist assessment",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Other_difficulty/disability",
                            Label = "Other difficulty or disabilty",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Physical_disability",
                            Label = "Physical disability",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Profound_and_multiple_learning_difficulty",
                            Label = "Profound and multiple learning difficulty",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Social_emotional_and_mental_health",
                            Label = "Social emotional and mental health",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Speech_language_and_communications_needs",
                            Label = "Speech language and communications needs",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Severe_learning_difficulty",
                            Label = "Severe learning difficulty",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Specific_learning_difficulty",
                            Label = "Specific learning difficulty",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Visual_impairment",
                            Label = "Visual impairment",
                            Group = "SEN primary need"
                        },
                        new CharacteristicMeta
                        {
                            Name = "IDACI_decile_0_to_10pc_most_deprived",
                            Label = "decile 0 to 10 (most deprived)",
                            Group = "IDACI"
                        },
                        new CharacteristicMeta
                        {
                            Name = "IDACI_decile_10_to_20pc",
                            Label = "decile 10 to 20",
                            Group = "IDACI"
                        },
                        new CharacteristicMeta
                        {
                            Name = "IDACI_decile_20_to_30pc",
                            Label = "decile 20 to 30 ",
                            Group = "IDACI"
                        },
                        new CharacteristicMeta
                        {
                            Name = "IDACI_decile_30_to_40pc",
                            Label = "decile 30 to 40",
                            Group = "IDACI"
                        },
                        new CharacteristicMeta
                        {
                            Name = "IDACI_decile_40_to_50pc",
                            Label = "decile 40 to 50",
                            Group = "IDACI"
                        },
                        new CharacteristicMeta
                        {
                            Name = "IDACI_decile_50_to_60pc",
                            Label = "decile 50 to 60",
                            Group = "IDACI"
                        },
                        new CharacteristicMeta
                        {
                            Name = "IDACI_decile_60_to_70pc",
                            Label = "decile 60 to 70",
                            Group = "IDACI"
                        },
                        new CharacteristicMeta
                        {
                            Name = "IDACI_decile_70_to_80pc",
                            Label = "decile 70 to 80",
                            Group = "IDACI"
                        },
                        new CharacteristicMeta
                        {
                            Name = "IDACI_decile_80_to_90pc",
                            Label = "decile 80 to 90",
                            Group = "IDACI"
                        },
                        new CharacteristicMeta
                        {
                            Name = "IDACI_decile_90_to_100pc_least_deprived",
                            Label = "decile 90 to 100 (least deprived)",
                            Group = "IDACI"
                        },
                        new CharacteristicMeta
                        {
                            Name = "IDACI_decile_unclassified",
                            Label = "idaci unclassified",
                            Group = "IDACI"
                        }
                    },
                    Releases = new[]
                    {
                        new Release
                        {
                            PublicationId = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                            ReleaseId = new Guid("be51f939-e9f9-4509-8851-e72b66a3515b"),
                            ReleaseDate = new DateTime(2018, 5, 30),
                            Filenames = new[]
                            {
                                //DataCsvFilename.schpupnum_geoglevels,
                                //DataCsvFilename.schpupnum_lacharacteristics,
                                DataCsvFilename.schpupnum_natcharacteristics
                            }
                        }
                    }
                }
            }
        };
    }
}