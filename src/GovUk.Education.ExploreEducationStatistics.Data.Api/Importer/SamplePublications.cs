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
                    AttributeMetas = new[]
                    {
                        new AttributeMeta
                        {
                            Name = "term",
                            Label = "Academic terms",
                            Group = "time",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "year",
                            Label = "Academic year",
                            Group = "time",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "level",
                            Label = "Geographic level",
                            Group = "choice",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "country_code",
                            Label = "Country code",
                            Group = "choice",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "country_name",
                            Label = "Country name",
                            Group = "choice",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "region_code",
                            Label = "Region code",
                            Group = "choice",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "region_name",
                            Label = "Region name",
                            Group = "choice",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "old_la_code",
                            Label = "Three digit LA code",
                            Group = "choice",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "new_la_code",
                            Label = "Nine digit LA code",
                            Group = "choice",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "la_name",
                            Label = "LA name",
                            Group = "choice",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "estab",
                            Label = "Establishment number",
                            Group = "choice",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "laestab",
                            Label = "LA/establishment number",
                            Group = "choice",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "urn",
                            Label = "School Unique Reference Number",
                            Group = "choice",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "school_type",
                            Label = "School type",
                            Group = "choice",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "academy_type",
                            Label = "Academy type",
                            Group = "choice",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "academy_open_date",
                            Label = "Academy open date",
                            Group = "choice",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "all_through",
                            Label = "All through school flag",
                            Group = "choice",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "characteristic_desc",
                            Label = "Characteristic grouping",
                            Group = "choice",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "characteristic_1",
                            Label = "Characteristic group",
                            Group = "choice",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "characteristic_2",
                            Label = "Characteristic group",
                            Group = "choice",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "num_schools",
                            Label = "Number of schools",
                            Group = "absence fields",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "enrolments",
                            Label = "Number of pupil enrolments",
                            Group = "absence fields",
                            KeyIndicator = true
                        },
                        new AttributeMeta
                        {
                            Name = "sess_possible",
                            Label = "Number of sessions possible",
                            Group = "absence fields",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "sess_overall",
                            Label = "Number of overall absence sessions",
                            Group = "absence fields",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "sess_authorised",
                            Label = "Number of authorised absence sessions",
                            Group = "absence fields",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "sess_unauthorised",
                            Label = "Number of unauthorised absence sessions",
                            Group = "absence fields",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "sess_overall_percent",
                            Label = "Overall absence rate",
                            Group = "absence fields",
                            KeyIndicator = true
                        },
                        new AttributeMeta
                        {
                            Name = "sess_authorised_percent",
                            Label = "Authorised absence rate",
                            Group = "absence fields",
                            KeyIndicator = true
                        },
                        new AttributeMeta
                        {
                            Name = "sess_unauthorised_percent",
                            Label = "Unauthorised absence rate",
                            Group = "absence fields",
                            KeyIndicator = true
                        },
                        new AttributeMeta
                        {
                            Name = "enrolments_PA_10_exact",
                            Label = "Number of persistent absentees",
                            Group = "absence fields",
                            KeyIndicator = true
                        },
                        new AttributeMeta
                        {
                            Name = "enrolments_pa_10_exact_percent",
                            Label = "Percentage of persistent absentees",
                            Group = "absence fields",
                            KeyIndicator = true
                        },
                        new AttributeMeta
                        {
                            Name = "sess_possible_pa_10_exact",
                            Label = "Number of sessions possible for persistent absentees",
                            Group = "absence for persistent absentees",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "sess_overall_pa_10_exact",
                            Label = "Number of overall absence sessions for persistent absentees",
                            Group = "absence for persistent absentees",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "sess_authorised_pa_10_exact",
                            Label = "Number of authorised absence sessions for persistent absentees",
                            Group = "absence for persistent absentees",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "sess_unauthorised_pa_10_exact",
                            Label = "Number of unauthorised absence sessions for persistent absentees",
                            Group = "absence for persistent absentees",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "sess_overall_percent_pa_10_exact",
                            Label = "Overall absence rate",
                            Group = "absence for persistent absentees",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "sess_authorised_percent_pa_10_exact",
                            Label = "Authorised absence rate",
                            Group = "absence for persistent absentees",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "sess_unauthorised_percent_pa_10_exact",
                            Label = "Unauthorised absence rate",
                            Group = "absence for persistent absentees",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "sess_auth_illness",
                            Label = "Number of illness sessions",
                            Group = "absence by reason",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "sess_auth_appointments",
                            Label = "Number of medical appointments sessions",
                            Group = "absence by reason",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "sess_auth_religious",
                            Label = "Number of religious observance sessions",
                            Group = "absence by reason",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "sess_auth_study",
                            Label = "Number of study leave sessions",
                            Group = "absence by reason",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "sess_auth_traveller",
                            Label = "Number of traveller sessions",
                            Group = "absence by reason",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "sess_auth_holiday",
                            Label = "Number of authorised holiday sessions",
                            Group = "absence by reason",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "sess_auth_ext_holiday",
                            Label = "Number of extended authorised holiday sessions",
                            Group = "absence by reason",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "sess_auth_excluded",
                            Label = "Number of excluded sessions",
                            Group = "absence by reason",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "sess_auth_other",
                            Label = "Number of authorised other sessions",
                            Group = "absence by reason",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "sess_auth_totalreasons",
                            Label = "Number of authorised reasons sessions",
                            Group = "absence by reason",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "sess_unauth_holiday",
                            Label = "Number of unauthorised holiday sessions",
                            Group = "absence by reason",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "sess_unauth_late",
                            Label = "Number of late sessions",
                            Group = "absence by reason",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "sess_unauth_other",
                            Label = "Number of unauthorised other sessions",
                            Group = "absence by reason",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "sess_unauth_noyet",
                            Label = "Number of no reason yet sessions",
                            Group = "absence by reason",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "sess_unauth_totalreasons",
                            Label = "Number of unauthorised reasons sessions",
                            Group = "absence by reason",
                            KeyIndicator = false
                        },
                        new AttributeMeta
                        {
                            Name = "sess_overall_totalreasons",
                            Label = "Number of overall reasons sessions",
                            Group = "absence by reason",
                            KeyIndicator = false
                        }
                    },
                    CharacteristicMetas = new[]
                    {
                        new CharacteristicMeta
                        {
                            Name = "Total",
                            Label = "All pupils"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Gender_male",
                            Label = "Boys"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Gender_female",
                            Label = "Girl"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Major_White_Total",
                            Label = "White"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_White_British",
                            Label = "White British"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Irish",
                            Label = "Irish"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Traveller_of_Irish_heritage",
                            Label = "Traveller of Irish Heritage"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Gypsy_Roma",
                            Label = "Gypsy Roma"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Any_other_white_background",
                            Label = "Any other White background"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Major_Mixed_Total",
                            Label = "Mixed"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_White_and_Black_Caribbean",
                            Label = "White and Black Caribbean"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_White_and_Black_African",
                            Label = "White and Black African"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_White_and_Asian",
                            Label = "White and Asian"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Any_other_Mixed_background",
                            Label = "Any other Mixed Background"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Major_Asian_Total",
                            Label = "Asian"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Indian",
                            Label = "India"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Pakistani",
                            Label = "Pakistani"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Bangladeshi",
                            Label = "Bangladeshi"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Any_other_Asian_background",
                            Label = "Any other Asian background"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Major_Black_Total",
                            Label = "Black"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Black_Caribbean",
                            Label = "Black Caribbean"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Black_African",
                            Label = "Black African"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minor_Any_other_black_background",
                            Label = "Any other Black background"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Major_Chinese",
                            Label = "Chinese"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Major_Any_Other_Ethnic_Group",
                            Label = "Any other ethnic group"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Minority_Ethnic_Group",
                            Label = "Minority ethnic group"
                        },
                        new CharacteristicMeta
                        {
                            Name = "Ethnicity_Major_Unclassified",
                            Label = "Ethnicity unclassified"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_1_and_below",
                            Label = "Year 1"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_2",
                            Label = "Year 2"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_3",
                            Label = "Year 3"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_4",
                            Label = "Year 4"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_5",
                            Label = "Year 5"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_6",
                            Label = "Year 6"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_7",
                            Label = "Year 7"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_8",
                            Label = "Year 8 "
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_9",
                            Label = "Year 9"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_10",
                            Label = "Year 10"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_11",
                            Label = "Year 11"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_12_and_above",
                            Label = "Year 12 and above"
                        },
                        new CharacteristicMeta
                        {
                            Name = "NC_Year_Not_followed_or_missing",
                            Label = "NC year missing"
                        },
                        new CharacteristicMeta
                        {
                            Name = "FSM_eligible",
                            Label = "FSM eligible"
                        },
                        new CharacteristicMeta
                        {
                            Name = "FSM_Not_eligible",
                            Label = "FSM not eligible"
                        },
                        new CharacteristicMeta
                        {
                            Name = "FSM_unclassified",
                            Label = "FSM unclassified"
                        },
                        new CharacteristicMeta
                        {
                            Name = "FSM_eligible_in_last_6_years",
                            Label = "FSM eligible at some point in last 6 years"
                        },
                        new CharacteristicMeta
                        {
                            Name = "FSM_not_eligible_in_last_6_years",
                            Label = "FSM not eligible at some point in last 6 years"
                        },
                        new CharacteristicMeta
                        {
                            Name = "FSM_unclassified_in_last_6_years",
                            Label = "FSM in last 6 years unclassified"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_provision_No_identified_SEN",
                            Label = "No identified SEN"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_provision_Statement_or_EHCP",
                            Label = "Statement of SEN or EHC plan"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_provision_SEN_Support",
                            Label = "SEN support"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_provision_Unclassified",
                            Label = "SEN unclassified"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Autistic_spectrum_disorder",
                            Label = "Autistic Spectrum Disorder"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Hearing_impairment",
                            Label = "Hearing Impairment"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Moderate_learning_difficulty",
                            Label = "Moderate Learning Difficulty"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Multi-sensory_impairment",
                            Label = "Multi-sensory impairment"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_No_specialist_assessment",
                            Label = "No specialist assessment"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Other_difficulty/disability",
                            Label = "Other difficulty or disabilty"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Physical_disability",
                            Label = "Physical disability"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Profound_and_multiple_learning_difficulty",
                            Label = "Profound and multiple learning difficulty"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Social_emotional_and_mental_health",
                            Label = "Social emotional and mental health"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Speech_language_and_communications_needs",
                            Label = "Speech language and communications needs"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Severe_learning_difficulty",
                            Label = "Severe learning difficulty"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Specific_learning_difficulty",
                            Label = "Specific learning difficulty"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Visual_impairment",
                            Label = "Visual impairment"
                        },
                        new CharacteristicMeta
                        {
                            Name = "SEN_primary_need_Unclassified",
                            Label = "Primary need Unclassified"
                        },
                        new CharacteristicMeta
                        {
                            Name = "First_language_Known_or_believed_to_be_English",
                            Label = "First language Known or believed to be English"
                        },
                        new CharacteristicMeta
                        {
                            Name = "First_language_Known_or_believed_to_be_other_than_English",
                            Label = "First language Known or believed to be other than English"
                        },
                        new CharacteristicMeta
                        {
                            Name = "First_language_Unclassified",
                            Label = "First language Unclassified"
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
                                DataCsvFilename.absence_geoglevels,
                                DataCsvFilename.absence_lacharacteristics,
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
                    AttributeMetas = new AttributeMeta[] { },
                    CharacteristicMetas = new CharacteristicMeta[] { },
                    Releases = new[]
                    {
                        new Release
                        {
                            PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                            ReleaseId = new Guid("ac602576-2d07-4324-8480-0cabb6294814"),
                            ReleaseDate = new DateTime(2018, 3, 22),
                            Filenames = new[]
                            {
                                DataCsvFilename.exclusion_geoglevels,
                                DataCsvFilename.exclusion_lacharacteristics,
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
                    AttributeMetas = new AttributeMeta[] { },
                    CharacteristicMetas = new CharacteristicMeta[] { },
                    Releases = new[]
                    {
                        new Release
                        {
                            PublicationId = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                            ReleaseId = new Guid("be51f939-e9f9-4509-8851-e72b66a3515b"),
                            ReleaseDate = new DateTime(2018, 5, 30),
                            Filenames = new[]
                            {
                                DataCsvFilename.schpupnum_geoglevels,
                                DataCsvFilename.schpupnum_lacharacteristics,
                                DataCsvFilename.schpupnum_natcharacteristics
                            }
                        }
                    }
                }
            }
        };
    }
}