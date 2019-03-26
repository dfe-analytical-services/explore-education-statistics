using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Seed.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Seed
{
    public static class SamplePublications
    {
        public static readonly Dictionary<string, IndicatorMeta> IndicatorMetas = new Dictionary<string, IndicatorMeta>
        {
            {
                "all_through", new IndicatorMeta
                {
                    Name = "all_through",
                    Label = "All through school flag",
                    KeyIndicator = false,
                    Unit = Unit.String
                }
            },
            {
                "num_schools", new IndicatorMeta
                {
                    Name = "num_schools",
                    Label = "Number of schools",
                    KeyIndicator = false,
                    Unit = Unit.Number
                }
            },
            {
                "enrolments", new IndicatorMeta
                {
                    Name = "enrolments",
                    Label = "Number of pupil enrolments",
                    KeyIndicator = true,
                    Unit = Unit.Number
                }
            },
            {
                "sess_possible", new IndicatorMeta
                {
                    Name = "sess_possible",
                    Label = "Number of sessions possible",
                    KeyIndicator = false,
                    Unit = Unit.Number
                }
            },
            {
                "sess_overall", new IndicatorMeta
                {
                    Name = "sess_overall",
                    Label = "Number of overall absence sessions",
                    KeyIndicator = false,
                    Unit = Unit.Number
                }
            },
            {
                "sess_authorised", new IndicatorMeta
                {
                    Name = "sess_authorised",
                    Label = "Number of authorised absence sessions",
                    KeyIndicator = false,
                    Unit = Unit.Number
                }
            },
            {
                "sess_unauthorised", new IndicatorMeta
                {
                    Name = "sess_unauthorised",
                    Label = "Number of unauthorised absence sessions",
                    KeyIndicator = false,
                    Unit = Unit.Number
                }
            },
            {
                "sess_overall_percent", new IndicatorMeta
                {
                    Name = "sess_overall_percent",
                    Label = "Overall absence rate",
                    KeyIndicator = true,
                    Unit = Unit.Percent
                }
            },
            {
                "sess_authorised_percent", new IndicatorMeta
                {
                    Name = "sess_authorised_percent",
                    Label = "Authorised absence rate",
                    KeyIndicator = true,
                    Unit = Unit.Percent
                }
            },
            {
                "sess_unauthorised_percent", new IndicatorMeta
                {
                    Name = "sess_unauthorised_percent",
                    Label = "Unauthorised absence rate",
                    KeyIndicator = true,
                    Unit = Unit.Percent
                }
            },
            {
                "enrolments_PA_10_exact", new IndicatorMeta
                {
                    Name = "enrolments_PA_10_exact",
                    Label = "Number of persistent absentees",
                    KeyIndicator = true,
                    Unit = Unit.Number
                }
            },
            {
                "enrolments_pa_10_exact_percent", new IndicatorMeta
                {
                    Name = "enrolments_pa_10_exact_percent",
                    Label = "Percentage of persistent absentees",
                    KeyIndicator = true,
                    Unit = Unit.Percent
                }
            },
            {
                "sess_possible_pa_10_exact", new IndicatorMeta
                {
                    Name = "sess_possible_pa_10_exact",
                    Label = "Number of sessions possible for persistent absentees",
                    KeyIndicator = false,
                    Unit = Unit.Number
                }
            },
            {
                "sess_overall_pa_10_exact", new IndicatorMeta
                {
                    Name = "sess_overall_pa_10_exact",
                    Label = "Number of overall absence sessions for persistent absentees",
                    KeyIndicator = false,
                    Unit = Unit.Number
                }
            },
            {
                "sess_authorised_pa_10_exact", new IndicatorMeta
                {
                    Name = "sess_authorised_pa_10_exact",
                    Label = "Number of authorised absence sessions for persistent absentees",
                    KeyIndicator = false,
                    Unit = Unit.Number
                }
            },
            {
                "sess_unauthorised_pa_10_exact", new IndicatorMeta
                {
                    Name = "sess_unauthorised_pa_10_exact",
                    Label = "Number of unauthorised absence sessions for persistent absentees",
                    KeyIndicator = false,
                    Unit = Unit.Number
                }
            },
            {
                "sess_overall_percent_pa_10_exact", new IndicatorMeta
                {
                    Name = "sess_overall_percent_pa_10_exact",
                    Label = "Overall absence rate for persistent absentees",
                    KeyIndicator = false,
                    Unit = Unit.Percent
                }
            },
            {
                "sess_authorised_percent_pa_10_exact", new IndicatorMeta
                {
                    Name = "sess_authorised_percent_pa_10_exact",
                    Label = "Authorised absence rate for persistent absentees",
                    KeyIndicator = false,
                    Unit = Unit.Percent
                }
            },
            {
                "sess_unauthorised_percent_pa_10_exact", new IndicatorMeta
                {
                    Name = "sess_unauthorised_percent_pa_10_exact",
                    Label = "Unauthorised absence rate for persistent absentees",
                    KeyIndicator = false,
                    Unit = Unit.Percent
                }
            },
            {
                "sess_auth_illness", new IndicatorMeta
                {
                    Name = "sess_auth_illness",
                    Label = "Number of illness sessions",
                    KeyIndicator = false,
                    Unit = Unit.Number
                }
            },
            {
                "sess_auth_appointments", new IndicatorMeta
                {
                    Name = "sess_auth_appointments",
                    Label = "Number of medical appointments sessions",
                    KeyIndicator = false,
                    Unit = Unit.Number
                }
            },
            {
                "sess_auth_religious", new IndicatorMeta
                {
                    Name = "sess_auth_religious",
                    Label = "Number of religious observance sessions",
                    KeyIndicator = false,
                    Unit = Unit.Number
                }
            },
            {
                "sess_auth_study", new IndicatorMeta
                {
                    Name = "sess_auth_study",
                    Label = "Number of study leave sessions",
                    KeyIndicator = false,
                    Unit = Unit.Number
                }
            },
            {
                "sess_auth_traveller", new IndicatorMeta
                {
                    Name = "sess_auth_traveller",
                    Label = "Number of traveller sessions",
                    KeyIndicator = false,
                    Unit = Unit.Number
                }
            },
            {
                "sess_auth_holiday", new IndicatorMeta
                {
                    Name = "sess_auth_holiday",
                    Label = "Number of authorised holiday sessions",
                    KeyIndicator = false,
                    Unit = Unit.Number
                }
            },
            {
                "sess_auth_ext_holiday", new IndicatorMeta
                {
                    Name = "sess_auth_ext_holiday",
                    Label = "Number of extended authorised holiday sessions",
                    KeyIndicator = false,
                    Unit = Unit.Number
                }
            },
            {
                "sess_auth_excluded", new IndicatorMeta
                {
                    Name = "sess_auth_excluded",
                    Label = "Number of excluded sessions",
                    KeyIndicator = false,
                    Unit = Unit.Number
                }
            },
            {
                "sess_auth_other", new IndicatorMeta
                {
                    Name = "sess_auth_other",
                    Label = "Number of authorised other sessions",
                    KeyIndicator = false,
                    Unit = Unit.Number
                }
            },
            {
                "sess_auth_totalreasons", new IndicatorMeta
                {
                    Name = "sess_auth_totalreasons",
                    Label = "Number of authorised reasons sessions",
                    KeyIndicator = false,
                    Unit = Unit.Number
                }
            },
            {
                "sess_unauth_holiday", new IndicatorMeta
                {
                    Name = "sess_unauth_holiday",
                    Label = "Number of unauthorised holiday sessions",
                    KeyIndicator = false,
                    Unit = Unit.Number
                }
            },
            {
                "sess_unauth_late", new IndicatorMeta
                {
                    Name = "sess_unauth_late",
                    Label = "Number of late sessions",
                    KeyIndicator = false,
                    Unit = Unit.Number
                }
            },
            {
                "sess_unauth_other", new IndicatorMeta
                {
                    Name = "sess_unauth_other",
                    Label = "Number of unauthorised other sessions",
                    KeyIndicator = false,
                    Unit = Unit.Number
                }
            },
            {
                "sess_unauth_noyet", new IndicatorMeta
                {
                    Name = "sess_unauth_noyet",
                    Label = "Number of no reason yet sessions",
                    KeyIndicator = false,
                    Unit = Unit.Number
                }
            },
            {
                "sess_unauth_totalreasons", new IndicatorMeta
                {
                    Name = "sess_unauth_totalreasons",
                    Label = "Number of unauthorised reasons sessions",
                    KeyIndicator = false,
                    Unit = Unit.Number
                }
            },
            {
                "sess_overall_totalreasons", new IndicatorMeta
                {
                    Name = "sess_overall_totalreasons",
                    Label = "Number of overall reasons sessions",
                    KeyIndicator = false,
                    Unit = Unit.Number
                }
            },
            {
                "headcount", new IndicatorMeta
                {
                    Name = "headcount",
                    Label = "Number of pupils",
                    KeyIndicator = false,
                    Unit = Unit.Number
                }
            },
            {
                "perm_excl", new IndicatorMeta
                {
                    Name = "perm_excl",
                    Label = "Number of permanent exclusions",
                    KeyIndicator = true,
                    Unit = Unit.Number
                }
            },
            {
                "perm_excl_rate", new IndicatorMeta
                {
                    Name = "perm_excl_rate",
                    Label = "Permanent exclusion rate",
                    KeyIndicator = true,
                    Unit = Unit.Number
                }
            },
            {
                "fixed_excl", new IndicatorMeta
                {
                    Name = "fixed_excl",
                    Label = "Number of fixed period exclusions",
                    KeyIndicator = true,
                    Unit = Unit.Number
                }
            },
            {
                "fixed_excl_rate", new IndicatorMeta
                {
                    Name = "fixed_excl_rate",
                    Label = "Fixed period exclusion rate",
                    KeyIndicator = true,
                    Unit = Unit.Number
                }
            },
            {
                "one_plus_fixed", new IndicatorMeta
                {
                    Name = "one_plus_fixed",
                    Label = "Number of enrolments with one or more fixed period exclusions",
                    KeyIndicator = true,
                    Unit = Unit.Number
                }
            },
            {
                "one_plus_fixed_rate", new IndicatorMeta
                {
                    Name = "one_plus_fixed_rate",
                    Label = "Percentage of enrolments with one or more fixed period exclusions",
                    KeyIndicator = true,
                    Unit = Unit.Percent
                }
            }
        };

        public static readonly Dictionary<string, CharacteristicMeta> CharacteristicMetas =
            new Dictionary<string, CharacteristicMeta>
            {
                {
                    "Total", new CharacteristicMeta
                    {
                        Name = "Total",
                        Label = "All pupils",
                        Group = "Total"
                    }
                },
                {
                    "Gender_male", new CharacteristicMeta
                    {
                        Name = "Gender_male",
                        Label = "Boys",
                        Group = "Gender"
                    }
                },
                {
                    "Gender_female", new CharacteristicMeta
                    {
                        Name = "Gender_female",
                        Label = "Girl",
                        Group = "Gender"
                    }
                },
                {
                    "Age_4_and_under", new CharacteristicMeta
                    {
                        Name = "Age_4_and_under",
                        Label = "Age four and under",
                        Group = "Age"
                    }
                },
                {
                    "Age_5", new CharacteristicMeta
                    {
                        Name = "Age_5",
                        Label = "Age 5",
                        Group = "Age"
                    }
                },
                {
                    "Age_6", new CharacteristicMeta
                    {
                        Name = "Age_6",
                        Label = "Age 6",
                        Group = "Age"
                    }
                },
                {
                    "Age_7", new CharacteristicMeta
                    {
                        Name = "Age_7",
                        Label = "Age 7",
                        Group = "Age"
                    }
                },
                {
                    "Age_8", new CharacteristicMeta
                    {
                        Name = "Age_8",
                        Label = "Age 8",
                        Group = "Age"
                    }
                },
                {
                    "Age_9", new CharacteristicMeta
                    {
                        Name = "Age_9",
                        Label = "Age 9",
                        Group = "Age"
                    }
                },
                {
                    "Age_10", new CharacteristicMeta
                    {
                        Name = "Age_10",
                        Label = "Age 10",
                        Group = "Age"
                    }
                },
                {
                    "Age_11", new CharacteristicMeta
                    {
                        Name = "Age_11",
                        Label = "Age 11",
                        Group = "Age"
                    }
                },
                {
                    "Age_12", new CharacteristicMeta
                    {
                        Name = "Age_12",
                        Label = "Age 12",
                        Group = "Age"
                    }
                },
                {
                    "Age_13", new CharacteristicMeta
                    {
                        Name = "Age_13",
                        Label = "Age 13",
                        Group = "Age"
                    }
                },
                {
                    "Age_14", new CharacteristicMeta
                    {
                        Name = "Age_14",
                        Label = "Age 14",
                        Group = "Age"
                    }
                },
                {
                    "Age_15", new CharacteristicMeta
                    {
                        Name = "Age_15",
                        Label = "Age 15",
                        Group = "Age"
                    }
                },
                {
                    "Age_16", new CharacteristicMeta
                    {
                        Name = "Age_16",
                        Label = "Age 16",
                        Group = "Age"
                    }
                },
                {
                    "Age_17", new CharacteristicMeta
                    {
                        Name = "Age_17",
                        Label = "Age 17",
                        Group = "Age"
                    }
                },
                {
                    "Age_18", new CharacteristicMeta
                    {
                        Name = "Age_18",
                        Label = "Age 18",
                        Group = "Age"
                    }
                },
                {
                    "Age_19_and_over", new CharacteristicMeta
                    {
                        Name = "Age_19_and_over",
                        Label = "Age 19 and over",
                        Group = "Age"
                    }
                },
                {
                    "Ethnicity_Major_White_Total", new CharacteristicMeta
                    {
                        Name = "Ethnicity_Major_White_Total",
                        Label = "White",
                        Group = "Ethnic group major"
                    }
                },
                {
                    "Ethnicity_Minor_White_British", new CharacteristicMeta
                    {
                        Name = "Ethnicity_Minor_White_British",
                        Label = "White British",
                        Group = "Ethnic group minor"
                    }
                },
                {
                    "Ethnicity_Minor_Irish", new CharacteristicMeta
                    {
                        Name = "Ethnicity_Minor_Irish",
                        Label = "Irish",
                        Group = "Ethnic group minor"
                    }
                },
                {
                    "Ethnicity_Minor_Traveller_of_Irish_heritage", new CharacteristicMeta
                    {
                        Name = "Ethnicity_Minor_Traveller_of_Irish_heritage",
                        Label = "Traveller of Irish Heritage",
                        Group = "Ethnic group minor"
                    }
                },
                {
                    "Ethnicity_Minor_Gypsy_Roma", new CharacteristicMeta
                    {
                        Name = "Ethnicity_Minor_Gypsy_Roma",
                        Label = "Gypsy Roma",
                        Group = "Ethnic group minor"
                    }
                },
                {
                    "Ethnicity_Minor_Any_other_white_background", new CharacteristicMeta
                    {
                        Name = "Ethnicity_Minor_Any_other_white_background",
                        Label = "Any other White background",
                        Group = "Ethnic group minor"
                    }
                },
                {
                    "Ethnicity_Major_Mixed_Total", new CharacteristicMeta
                    {
                        Name = "Ethnicity_Major_Mixed_Total",
                        Label = "Mixed",
                        Group = "Ethnic group major"
                    }
                },
                {
                    "Ethnicity_Minor_White_and_Black_Caribbean", new CharacteristicMeta
                    {
                        Name = "Ethnicity_Minor_White_and_Black_Caribbean",
                        Label = "White and Black Caribbean",
                        Group = "Ethnic group minor"
                    }
                },
                {
                    "Ethnicity_Minor_White_and_Black_African", new CharacteristicMeta
                    {
                        Name = "Ethnicity_Minor_White_and_Black_African",
                        Label = "White and Black African",
                        Group = "Ethnic group minor"
                    }
                },
                {
                    "Ethnicity_Minor_White_and_Asian", new CharacteristicMeta
                    {
                        Name = "Ethnicity_Minor_White_and_Asian",
                        Label = "White and Asian",
                        Group = "Ethnic group minor"
                    }
                },
                {
                    "Ethnicity_Minor_Any_other_Mixed_background", new CharacteristicMeta
                    {
                        Name = "Ethnicity_Minor_Any_other_Mixed_background",
                        Label = "Any other Mixed Background",
                        Group = "Ethnic group minor"
                    }
                },
                {
                    "Ethnicity_Major_Asian_Total", new CharacteristicMeta
                    {
                        Name = "Ethnicity_Major_Asian_Total",
                        Label = "Asian",
                        Group = "Ethnic group major"
                    }
                },
                {
                    "Ethnicity_Minor_Indian", new CharacteristicMeta
                    {
                        Name = "Ethnicity_Minor_Indian",
                        Label = "India",
                        Group = "Ethnic group minor"
                    }
                },
                {
                    "Ethnicity_Minor_Pakistani", new CharacteristicMeta
                    {
                        Name = "Ethnicity_Minor_Pakistani",
                        Label = "Pakistani",
                        Group = "Ethnic group minor"
                    }
                },
                {
                    "Ethnicity_Minor_Bangladeshi", new CharacteristicMeta
                    {
                        Name = "Ethnicity_Minor_Bangladeshi",
                        Label = "Bangladeshi",
                        Group = "Ethnic group minor"
                    }
                },
                {
                    "Ethnicity_Minor_Any_other_Asian_background", new CharacteristicMeta
                    {
                        Name = "Ethnicity_Minor_Any_other_Asian_background",
                        Label = "Any other Asian background",
                        Group = "Ethnic group minor"
                    }
                },
                {
                    "Ethnicity_Major_Black_Total", new CharacteristicMeta
                    {
                        Name = "Ethnicity_Major_Black_Total",
                        Label = "Black",
                        Group = "Ethnic group major"
                    }
                },
                {
                    "Ethnicity_Minor_Black_Caribbean", new CharacteristicMeta
                    {
                        Name = "Ethnicity_Minor_Black_Caribbean",
                        Label = "Black Caribbean",
                        Group = "Ethnic group minor"
                    }
                },
                {
                    "Ethnicity_Minor_Black_African", new CharacteristicMeta
                    {
                        Name = "Ethnicity_Minor_Black_African",
                        Label = "Black African",
                        Group = "Ethnic group minor"
                    }
                },
                {
                    "Ethnicity_Minor_Any_other_black_background", new CharacteristicMeta
                    {
                        Name = "Ethnicity_Minor_Any_other_black_background",
                        Label = "Any other Black background",
                        Group = "Ethnic group minor"
                    }
                },
                {
                    "Ethnicity_Major_Chinese", new CharacteristicMeta
                    {
                        Name = "Ethnicity_Major_Chinese",
                        Label = "Chinese",
                        Group = "Ethnic group major"
                    }
                },
                {
                    "Ethnicity_Major_Any_Other_Ethnic_Group", new CharacteristicMeta
                    {
                        Name = "Ethnicity_Major_Any_Other_Ethnic_Group",
                        Label = "Any other ethnic group",
                        Group = "Ethnic group major"
                    }
                },
                {
                    "Ethnicity_Minority_Ethnic_Group", new CharacteristicMeta
                    {
                        Name = "Ethnicity_Minority_Ethnic_Group",
                        Label = "Minority ethnic group",
                        Group = "Ethnic group major"
                    }
                },
                {
                    "Ethnicity_Major_Unclassified", new CharacteristicMeta
                    {
                        Name = "Ethnicity_Major_Unclassified",
                        Label = "Ethnicity unclassified",
                        Group = "Ethnic group major"
                    }
                },
                {
                    "NC_Year_Nursery", new CharacteristicMeta
                    {
                        Name = "NC_Year_Nursery",
                        Label = "Nursery",
                        Group = "NC year"
                    }
                },
                {
                    "NC_Year_Reception", new CharacteristicMeta
                    {
                        Name = "NC_Year_Reception",
                        Label = "Reception",
                        Group = "NC year"
                    }
                },
                {
                    "NC_Year_1_and_below", new CharacteristicMeta
                    {
                        Name = "NC_Year_1_and_below",
                        Label = "Year 1",
                        Group = "NC year"
                    }
                },
                {
                    "NC_Year_1", new CharacteristicMeta
                    {
                        Name = "NC_Year_1",
                        Label = "Year 1",
                        Group = "NC year"
                    }
                },
                {
                    "NC_Year_2", new CharacteristicMeta
                    {
                        Name = "NC_Year_2",
                        Label = "Year 2",
                        Group = "NC year"
                    }
                },
                {
                    "NC_Year_3", new CharacteristicMeta
                    {
                        Name = "NC_Year_3",
                        Label = "Year 3",
                        Group = "NC year"
                    }
                },
                {
                    "NC_Year_4", new CharacteristicMeta
                    {
                        Name = "NC_Year_4",
                        Label = "Year 4",
                        Group = "NC year"
                    }
                },
                {
                    "NC_Year_5", new CharacteristicMeta
                    {
                        Name = "NC_Year_5",
                        Label = "Year 5",
                        Group = "NC year"
                    }
                },
                {
                    "NC_Year_6", new CharacteristicMeta
                    {
                        Name = "NC_Year_6",
                        Label = "Year 6",
                        Group = "NC year"
                    }
                },
                {
                    "NC_Year_7", new CharacteristicMeta
                    {
                        Name = "NC_Year_7",
                        Label = "Year 7",
                        Group = "NC year"
                    }
                },
                {
                    "NC_Year_8", new CharacteristicMeta
                    {
                        Name = "NC_Year_8",
                        Label = "Year 8 ",
                        Group = "NC year"
                    }
                },
                {
                    "NC_Year_9", new CharacteristicMeta
                    {
                        Name = "NC_Year_9",
                        Label = "Year 9",
                        Group = "NC year"
                    }
                },
                {
                    "NC_Year_10", new CharacteristicMeta
                    {
                        Name = "NC_Year_10",
                        Label = "Year 10",
                        Group = "NC year"
                    }
                },
                {
                    "NC_Year_11", new CharacteristicMeta
                    {
                        Name = "NC_Year_11",
                        Label = "Year 11",
                        Group = "NC year"
                    }
                },
                {
                    "NC_Year_12_and_above", new CharacteristicMeta
                    {
                        Name = "NC_Year_12_and_above",
                        Label = "Year 12 and above",
                        Group = "NC year"
                    }
                },
                {
                    "NC_Year_Not_followed_or_missing", new CharacteristicMeta
                    {
                        Name = "NC_Year_Not_followed_or_missing",
                        Label = "NC year missing",
                        Group = "NC year"
                    }
                },
                {
                    "NC_Year_Not_followed", new CharacteristicMeta
                    {
                        Name = "NC_Year_Not_followed",
                        Label = "NC year not followed",
                        Group = "NC year"
                    }
                },
                {
                    "NC_Year_Unclassified", new CharacteristicMeta
                    {
                        Name = "NC_Year_Unclassified",
                        Label = "NC year unclassified",
                        Group = "NC year"
                    }
                },
                {
                    "FSM_eligible", new CharacteristicMeta
                    {
                        Name = "FSM_eligible",
                        Label = "FSM eligible",
                        Group = "Free school meals"
                    }
                },
                {
                    "FSM_Not_eligible", new CharacteristicMeta
                    {
                        Name = "FSM_Not_eligible",
                        Label = "FSM not eligible",
                        Group = "Free school meals"
                    }
                },
                {
                    "FSM_unclassified", new CharacteristicMeta
                    {
                        Name = "FSM_unclassified",
                        Label = "FSM unclassified",
                        Group = "Free school meals"
                    }
                },
                {
                    "FSM_eligible_in_last_6_years", new CharacteristicMeta
                    {
                        Name = "FSM_eligible_in_last_6_years",
                        Label = "FSM eligible at some point in last 6 years",
                        Group = "Free school meals in last 6 years"
                    }
                },
                {
                    "FSM_not_eligible_in_last_6_years", new CharacteristicMeta
                    {
                        Name = "FSM_not_eligible_in_last_6_years",
                        Label = "FSM not eligible at some point in last 6 years",
                        Group = "Free school meals in last 6 years"
                    }
                },
                {
                    "FSM_unclassified_in_last_6_years", new CharacteristicMeta
                    {
                        Name = "FSM_unclassified_in_last_6_years",
                        Label = "FSM in last 6 years unclassified",
                        Group = "Free school meals in last 6 years"
                    }
                },
                {
                    "SEN_provision_No_identified_SEN", new CharacteristicMeta
                    {
                        Name = "SEN_provision_No_identified_SEN",
                        Label = "No identified SEN",
                        Group = "SEN provision"
                    }
                },
                {
                    "SEN_provision_Statement_or_EHCP", new CharacteristicMeta
                    {
                        Name = "SEN_provision_Statement_or_EHCP",
                        Label = "Statement of SEN or EHC plan",
                        Group = "SEN provision"
                    }
                },
                {
                    "SEN_provision_SEN_Support", new CharacteristicMeta
                    {
                        Name = "SEN_provision_SEN_Support",
                        Label = "SEN support",
                        Group = "SEN provision"
                    }
                },
                {
                    "SEN_provision_Unclassified", new CharacteristicMeta
                    {
                        Name = "SEN_provision_Unclassified",
                        Label = "SEN unclassified",
                        Group = "SEN provision"
                    }
                },
                {
                    "SEN_primary_need_Autistic_spectrum_disorder", new CharacteristicMeta
                    {
                        Name = "SEN_primary_need_Autistic_spectrum_disorder",
                        Label = "Autistic Spectrum Disorder",
                        Group = "SEN primary need"
                    }
                },
                {
                    "SEN_primary_need_Hearing_impairment", new CharacteristicMeta
                    {
                        Name = "SEN_primary_need_Hearing_impairment",
                        Label = "Hearing Impairment",
                        Group = "SEN primary need"
                    }
                },
                {
                    "SEN_primary_need_Moderate_learning_difficulty", new CharacteristicMeta
                    {
                        Name = "SEN_primary_need_Moderate_learning_difficulty",
                        Label = "Moderate Learning Difficulty",
                        Group = "SEN primary need"
                    }
                },
                {
                    "SEN_primary_need_Multi-sensory_impairment", new CharacteristicMeta
                    {
                        Name = "SEN_primary_need_Multi-sensory_impairment",
                        Label = "Multi-sensory impairment",
                        Group = "SEN primary need"
                    }
                },
                {
                    "SEN_primary_need_No_specialist_assessment", new CharacteristicMeta
                    {
                        Name = "SEN_primary_need_No_specialist_assessment",
                        Label = "No specialist assessment",
                        Group = "SEN primary need"
                    }
                },
                {
                    "SEN_primary_need_Other_difficulty/disability", new CharacteristicMeta
                    {
                        Name = "SEN_primary_need_Other_difficulty/disability",
                        Label = "Other difficulty or disabilty",
                        Group = "SEN primary need"
                    }
                },
                {
                    "SEN_primary_need_Physical_disability", new CharacteristicMeta
                    {
                        Name = "SEN_primary_need_Physical_disability",
                        Label = "Physical disability",
                        Group = "SEN primary need"
                    }
                },
                {
                    "SEN_primary_need_Profound_and_multiple_learning_difficulty", new CharacteristicMeta
                    {
                        Name = "SEN_primary_need_Profound_and_multiple_learning_difficulty",
                        Label = "Profound and multiple learning difficulty",
                        Group = "SEN primary need"
                    }
                },
                {
                    "SEN_primary_need_Social_emotional_and_mental_health", new CharacteristicMeta
                    {
                        Name = "SEN_primary_need_Social_emotional_and_mental_health",
                        Label = "Social emotional and mental health",
                        Group = "SEN primary need"
                    }
                },
                {
                    "SEN_primary_need_Speech_language_and_communications_needs", new CharacteristicMeta
                    {
                        Name = "SEN_primary_need_Speech_language_and_communications_needs",
                        Label = "Speech language and communications needs",
                        Group = "SEN primary need"
                    }
                },
                {
                    "SEN_primary_need_Severe_learning_difficulty", new CharacteristicMeta
                    {
                        Name = "SEN_primary_need_Severe_learning_difficulty",
                        Label = "Severe learning difficulty",
                        Group = "SEN primary need"
                    }
                },
                {
                    "SEN_primary_need_Specific_learning_difficulty", new CharacteristicMeta
                    {
                        Name = "SEN_primary_need_Specific_learning_difficulty",
                        Label = "Specific learning difficulty",
                        Group = "SEN primary need"
                    }
                },
                {
                    "SEN_primary_need_Visual_impairment", new CharacteristicMeta
                    {
                        Name = "SEN_primary_need_Visual_impairment",
                        Label = "Visual impairment",
                        Group = "SEN primary need"
                    }
                },
                {
                    "SEN_primary_need_Unclassified", new CharacteristicMeta
                    {
                        Name = "SEN_primary_need_Unclassified",
                        Label = "Primary need Unclassified",
                        Group = "SEN primary need"
                    }
                },
                {
                    "First_language_Known_or_believed_to_be_English", new CharacteristicMeta
                    {
                        Name = "First_language_Known_or_believed_to_be_English",
                        Label = "First language Known or believed to be English",
                        Group = "First language"
                    }
                },
                {
                    "First_language_Known_or_believed_to_be_other_than_English", new CharacteristicMeta
                    {
                        Name = "First_language_Known_or_believed_to_be_other_than_English",
                        Label = "First language Known or believed to be other than English",
                        Group = "First language"
                    }
                },
                {
                    "First_language_Unclassified", new CharacteristicMeta
                    {
                        Name = "First_language_Unclassified",
                        Label = "First language Unclassified",
                        Group = "First language"
                    }
                },
                {
                    "IDACI_decile_0_to_10pc_most_deprived", new CharacteristicMeta
                    {
                        Name = "IDACI_decile_0_to_10pc_most_deprived",
                        Label = "decile 0 to 10 (most deprived)",
                        Group = "IDACI"
                    }
                },
                {
                    "IDACI_decile_10_to_20pc", new CharacteristicMeta
                    {
                        Name = "IDACI_decile_10_to_20pc",
                        Label = "decile 10 to 20",
                        Group = "IDACI"
                    }
                },
                {
                    "IDACI_decile_20_to_30pc", new CharacteristicMeta
                    {
                        Name = "IDACI_decile_20_to_30pc",
                        Label = "decile 20 to 30 ",
                        Group = "IDACI"
                    }
                },
                {
                    "IDACI_decile_30_to_40pc", new CharacteristicMeta
                    {
                        Name = "IDACI_decile_30_to_40pc",
                        Label = "decile 30 to 40",
                        Group = "IDACI"
                    }
                },
                {
                    "IDACI_decile_40_to_50pc", new CharacteristicMeta
                    {
                        Name = "IDACI_decile_40_to_50pc",
                        Label = "decile 40 to 50",
                        Group = "IDACI"
                    }
                },
                {
                    "IDACI_decile_50_to_60pc", new CharacteristicMeta
                    {
                        Name = "IDACI_decile_50_to_60pc",
                        Label = "decile 50 to 60",
                        Group = "IDACI"
                    }
                },
                {
                    "IDACI_decile_60_to_70pc", new CharacteristicMeta
                    {
                        Name = "IDACI_decile_60_to_70pc",
                        Label = "decile 60 to 70",
                        Group = "IDACI"
                    }
                },
                {
                    "IDACI_decile_70_to_80pc", new CharacteristicMeta
                    {
                        Name = "IDACI_decile_70_to_80pc",
                        Label = "decile 70 to 80",
                        Group = "IDACI"
                    }
                },
                {
                    "IDACI_decile_80_to_90pc", new CharacteristicMeta
                    {
                        Name = "IDACI_decile_80_to_90pc",
                        Label = "decile 80 to 90",
                        Group = "IDACI"
                    }
                },
                {
                    "IDACI_decile_90_to_100pc_least_deprived", new CharacteristicMeta
                    {
                        Name = "IDACI_decile_90_to_100pc_least_deprived",
                        Label = "decile 90 to 100 (least deprived)",
                        Group = "IDACI"
                    }
                },
                {
                    "IDACI_decile_unclassified", new CharacteristicMeta
                    {
                        Name = "IDACI_decile_unclassified",
                        Label = "idaci unclassified",
                        Group = "IDACI"
                    }
                }
            };

        private static IndicatorMeta GetIndicatorMeta(string key)
        {
            return IndicatorMetas[key];
        }

        private static CharacteristicMeta GetCharacteristicMeta(string key)
        {
            return CharacteristicMetas[key];
        }

        public static readonly Dictionary<string, Publication> Publications = new Dictionary<string, Publication>
        {
            {
                "absence", new Publication
                {
                    PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                    Name = "Pupil absence in schools in England",
                    Releases = new[]
                    {
                        new Release
                        {
                            PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                            ReleaseDate = new DateTime(2018, 4, 25),
                            Name = "Pupil absence in schools in England",
                            DataSets = new[]
                            {
                                new DataSet
                                {
                                    Filename = DataCsvFilename.absence_geoglevels,
                                    IndicatorMetas = new[]
                                    {
                                        new MetaGroup<IndicatorMeta>
                                        {
                                            Name = "Choice",
                                            Meta = new[]
                                            {
                                                GetIndicatorMeta("all_through")
                                            }
                                        },
                                        new MetaGroup<IndicatorMeta>
                                        {
                                            Name = "Absence fields",
                                            Meta = new[]
                                            {
                                                GetIndicatorMeta("num_schools"),
                                                GetIndicatorMeta("enrolments"),
                                                GetIndicatorMeta("sess_possible"),
                                                GetIndicatorMeta("sess_overall"),
                                                GetIndicatorMeta("sess_authorised"),
                                                GetIndicatorMeta("sess_unauthorised"),
                                                GetIndicatorMeta("sess_overall_percent"),
                                                GetIndicatorMeta("sess_authorised_percent"),
                                                GetIndicatorMeta("sess_unauthorised_percent"),
                                                GetIndicatorMeta("enrolments_PA_10_exact"),
                                                GetIndicatorMeta("enrolments_pa_10_exact_percent")
                                            }
                                        },
                                        new MetaGroup<IndicatorMeta>
                                        {
                                            Name = "Absence for persistent absentees",
                                            Meta = new[]
                                            {
                                                GetIndicatorMeta("sess_possible_pa_10_exact"),
                                                GetIndicatorMeta("sess_overall_pa_10_exact"),
                                                GetIndicatorMeta("sess_authorised_pa_10_exact"),
                                                GetIndicatorMeta("sess_unauthorised_pa_10_exact"),
                                                GetIndicatorMeta("sess_overall_percent_pa_10_exact"),
                                                GetIndicatorMeta("sess_authorised_percent_pa_10_exact"),
                                                GetIndicatorMeta("sess_unauthorised_percent_pa_10_exact")
                                            }
                                        },
                                        new MetaGroup<IndicatorMeta>
                                        {
                                            Name = "Absence by reason",
                                            Meta = new[]
                                            {
                                                GetIndicatorMeta("sess_auth_illness"),
                                                GetIndicatorMeta("sess_auth_appointments"),
                                                GetIndicatorMeta("sess_auth_religious"),
                                                GetIndicatorMeta("sess_auth_study"),
                                                GetIndicatorMeta("sess_auth_traveller"),
                                                GetIndicatorMeta("sess_auth_holiday"),
                                                GetIndicatorMeta("sess_auth_ext_holiday"),
                                                GetIndicatorMeta("sess_auth_excluded"),
                                                GetIndicatorMeta("sess_auth_other"),
                                                GetIndicatorMeta("sess_auth_totalreasons"),
                                                GetIndicatorMeta("sess_unauth_holiday"),
                                                GetIndicatorMeta("sess_unauth_late"),
                                                GetIndicatorMeta("sess_unauth_other"),
                                                GetIndicatorMeta("sess_unauth_noyet"),
                                                GetIndicatorMeta("sess_unauth_totalreasons"),
                                                GetIndicatorMeta("sess_overall_totalreasons")
                                            }
                                        }
                                    },
                                    CharacteristicMetas = new CharacteristicMeta[]{}
                                },
                                new DataSet
                                {
                                    Filename = DataCsvFilename.absence_lacharacteristics,
                                    IndicatorMetas = new[]
                                    {
                                        new MetaGroup<IndicatorMeta>
                                        {
                                            Name = "Absence fields",
                                            Meta = new[]
                                            {
                                                GetIndicatorMeta("enrolments"),
                                                GetIndicatorMeta("sess_possible"),
                                                GetIndicatorMeta("sess_overall"),
                                                GetIndicatorMeta("sess_authorised"),
                                                GetIndicatorMeta("sess_unauthorised"),
                                                GetIndicatorMeta("sess_overall_percent"),
                                                GetIndicatorMeta("sess_authorised_percent"),
                                                GetIndicatorMeta("sess_unauthorised_percent"),
                                                GetIndicatorMeta("enrolments_PA_10_exact"),
                                                GetIndicatorMeta("enrolments_pa_10_exact_percent")
                                            }
                                        },
                                        new MetaGroup<IndicatorMeta>
                                        {
                                            Name = "Absence for persistent absentees",
                                            Meta = new[]
                                            {
                                                GetIndicatorMeta("sess_possible_pa_10_exact"),
                                                GetIndicatorMeta("sess_overall_pa_10_exact"),
                                                GetIndicatorMeta("sess_authorised_pa_10_exact"),
                                                GetIndicatorMeta("sess_unauthorised_pa_10_exact"),
                                                GetIndicatorMeta("sess_overall_percent_pa_10_exact"),
                                                GetIndicatorMeta("sess_authorised_percent_pa_10_exact"),
                                                GetIndicatorMeta("sess_unauthorised_percent_pa_10_exact")
                                            }
                                        },
                                        new MetaGroup<IndicatorMeta>
                                        {
                                            Name = "Absence by reason",
                                            Meta = new[]
                                            {
                                                GetIndicatorMeta("sess_auth_illness"),
                                                GetIndicatorMeta("sess_auth_appointments"),
                                                GetIndicatorMeta("sess_auth_religious"),
                                                GetIndicatorMeta("sess_auth_study"),
                                                GetIndicatorMeta("sess_auth_traveller"),
                                                GetIndicatorMeta("sess_auth_holiday"),
                                                GetIndicatorMeta("sess_auth_ext_holiday"),
                                                GetIndicatorMeta("sess_auth_excluded"),
                                                GetIndicatorMeta("sess_auth_other"),
                                                GetIndicatorMeta("sess_auth_totalreasons"),
                                                GetIndicatorMeta("sess_unauth_holiday"),
                                                GetIndicatorMeta("sess_unauth_late"),
                                                GetIndicatorMeta("sess_unauth_other"),
                                                GetIndicatorMeta("sess_unauth_noyet"),
                                                GetIndicatorMeta("sess_unauth_totalreasons"),
                                                GetIndicatorMeta("sess_overall_totalreasons")
                                            }
                                        }
                                    },
                                    CharacteristicMetas = new[]
                                    {
                                        GetCharacteristicMeta("Total"),
                                        GetCharacteristicMeta("Gender_male"),
                                        GetCharacteristicMeta("Gender_female"),
                                        GetCharacteristicMeta("Ethnicity_Major_White_Total"),
                                        GetCharacteristicMeta("Ethnicity_Minor_White_British"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Irish"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Traveller_of_Irish_heritage"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Gypsy_Roma"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Any_other_white_background"),
                                        GetCharacteristicMeta("Ethnicity_Major_Mixed_Total"),
                                        GetCharacteristicMeta("Ethnicity_Minor_White_and_Black_Caribbean"),
                                        GetCharacteristicMeta("Ethnicity_Minor_White_and_Black_African"),
                                        GetCharacteristicMeta("Ethnicity_Minor_White_and_Asian"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Any_other_Mixed_background"),
                                        GetCharacteristicMeta("Ethnicity_Major_Asian_Total"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Indian"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Pakistani"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Bangladeshi"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Any_other_Asian_background"),
                                        GetCharacteristicMeta("Ethnicity_Major_Black_Total"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Black_Caribbean"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Black_African"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Any_other_black_background"),
                                        GetCharacteristicMeta("Ethnicity_Major_Chinese"),
                                        GetCharacteristicMeta("Ethnicity_Major_Any_Other_Ethnic_Group"),
                                        GetCharacteristicMeta("Ethnicity_Minority_Ethnic_Group"),
                                        GetCharacteristicMeta("Ethnicity_Major_Unclassified"),
                                        GetCharacteristicMeta("NC_Year_1_and_below"),
                                        GetCharacteristicMeta("NC_Year_2"),
                                        GetCharacteristicMeta("NC_Year_3"),
                                        GetCharacteristicMeta("NC_Year_4"),
                                        GetCharacteristicMeta("NC_Year_5"),
                                        GetCharacteristicMeta("NC_Year_6"),
                                        GetCharacteristicMeta("NC_Year_7"),
                                        GetCharacteristicMeta("NC_Year_8"),
                                        GetCharacteristicMeta("NC_Year_9"),
                                        GetCharacteristicMeta("NC_Year_10"),
                                        GetCharacteristicMeta("NC_Year_11"),
                                        GetCharacteristicMeta("NC_Year_12_and_above"),
                                        GetCharacteristicMeta("NC_Year_Not_followed_or_missing"),
                                        GetCharacteristicMeta("FSM_eligible"),
                                        GetCharacteristicMeta("FSM_Not_eligible"),
                                        GetCharacteristicMeta("FSM_unclassified"),
                                        GetCharacteristicMeta("FSM_eligible_in_last_6_years"),
                                        GetCharacteristicMeta("FSM_not_eligible_in_last_6_years"),
                                        GetCharacteristicMeta("FSM_unclassified_in_last_6_years"),
                                        GetCharacteristicMeta("SEN_provision_No_identified_SEN"),
                                        GetCharacteristicMeta("SEN_provision_Statement_or_EHCP"),
                                        GetCharacteristicMeta("SEN_provision_SEN_Support"),
                                        GetCharacteristicMeta("SEN_provision_Unclassified"),
                                        GetCharacteristicMeta("SEN_primary_need_Autistic_spectrum_disorder"),
                                        GetCharacteristicMeta("SEN_primary_need_Hearing_impairment"),
                                        GetCharacteristicMeta("SEN_primary_need_Moderate_learning_difficulty"),
                                        GetCharacteristicMeta("SEN_primary_need_Multi-sensory_impairment"),
                                        GetCharacteristicMeta("SEN_primary_need_No_specialist_assessment"),
                                        GetCharacteristicMeta("SEN_primary_need_Other_difficulty/disability"),
                                        GetCharacteristicMeta("SEN_primary_need_Physical_disability"),
                                        GetCharacteristicMeta(
                                            "SEN_primary_need_Profound_and_multiple_learning_difficulty"),
                                        GetCharacteristicMeta("SEN_primary_need_Social_emotional_and_mental_health"),
                                        GetCharacteristicMeta(
                                            "SEN_primary_need_Speech_language_and_communications_needs"),
                                        GetCharacteristicMeta("SEN_primary_need_Severe_learning_difficulty"),
                                        GetCharacteristicMeta("SEN_primary_need_Specific_learning_difficulty"),
                                        GetCharacteristicMeta("SEN_primary_need_Visual_impairment"),
                                        GetCharacteristicMeta("SEN_primary_need_Unclassified"),
                                        GetCharacteristicMeta("First_language_Known_or_believed_to_be_English"),
                                        GetCharacteristicMeta(
                                            "First_language_Known_or_believed_to_be_other_than_English"),
                                        GetCharacteristicMeta("First_language_Unclassified")
                                    }
                                },
                                new DataSet
                                {
                                    Filename = DataCsvFilename.absence_natcharacteristics,
                                    IndicatorMetas = new[]
                                    {
                                        new MetaGroup<IndicatorMeta>
                                        {
                                            Name = "Absence fields",
                                            Meta = new[]
                                            {
                                                GetIndicatorMeta("enrolments"),
                                                GetIndicatorMeta("sess_possible"),
                                                GetIndicatorMeta("sess_overall"),
                                                GetIndicatorMeta("sess_authorised"),
                                                GetIndicatorMeta("sess_unauthorised"),
                                                GetIndicatorMeta("sess_overall_percent"),
                                                GetIndicatorMeta("sess_authorised_percent"),
                                                GetIndicatorMeta("sess_unauthorised_percent"),
                                                GetIndicatorMeta("enrolments_PA_10_exact"),
                                                GetIndicatorMeta("enrolments_pa_10_exact_percent")
                                            }
                                        },
                                        new MetaGroup<IndicatorMeta>
                                        {
                                            Name = "Absence for persistent absentees",
                                            Meta = new[]
                                            {
                                                GetIndicatorMeta("sess_possible_pa_10_exact"),
                                                GetIndicatorMeta("sess_overall_pa_10_exact"),
                                                GetIndicatorMeta("sess_authorised_pa_10_exact"),
                                                GetIndicatorMeta("sess_unauthorised_pa_10_exact"),
                                                GetIndicatorMeta("sess_overall_percent_pa_10_exact"),
                                                GetIndicatorMeta("sess_authorised_percent_pa_10_exact"),
                                                GetIndicatorMeta("sess_unauthorised_percent_pa_10_exact")
                                            }
                                        },
                                        new MetaGroup<IndicatorMeta>
                                        {
                                            Name = "Absence by reason",
                                            Meta = new[]
                                            {
                                                GetIndicatorMeta("sess_auth_illness"),
                                                GetIndicatorMeta("sess_auth_appointments"),
                                                GetIndicatorMeta("sess_auth_religious"),
                                                GetIndicatorMeta("sess_auth_study"),
                                                GetIndicatorMeta("sess_auth_traveller"),
                                                GetIndicatorMeta("sess_auth_holiday"),
                                                GetIndicatorMeta("sess_auth_ext_holiday"),
                                                GetIndicatorMeta("sess_auth_excluded"),
                                                GetIndicatorMeta("sess_auth_other"),
                                                GetIndicatorMeta("sess_auth_totalreasons"),
                                                GetIndicatorMeta("sess_unauth_holiday"),
                                                GetIndicatorMeta("sess_unauth_late"),
                                                GetIndicatorMeta("sess_unauth_other"),
                                                GetIndicatorMeta("sess_unauth_noyet"),
                                                GetIndicatorMeta("sess_unauth_totalreasons"),
                                                GetIndicatorMeta("sess_overall_totalreasons")
                                            }
                                        }
                                    },
                                    CharacteristicMetas = new[]
                                    {
                                        GetCharacteristicMeta("Total"),
                                        GetCharacteristicMeta("Gender_male"),
                                        GetCharacteristicMeta("Gender_female"),
                                        GetCharacteristicMeta("Ethnicity_Major_White_Total"),
                                        GetCharacteristicMeta("Ethnicity_Minor_White_British"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Irish"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Traveller_of_Irish_heritage"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Gypsy_Roma"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Any_other_white_background"),
                                        GetCharacteristicMeta("Ethnicity_Major_Mixed_Total"),
                                        GetCharacteristicMeta("Ethnicity_Minor_White_and_Black_Caribbean"),
                                        GetCharacteristicMeta("Ethnicity_Minor_White_and_Black_African"),
                                        GetCharacteristicMeta("Ethnicity_Minor_White_and_Asian"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Any_other_Mixed_background"),
                                        GetCharacteristicMeta("Ethnicity_Major_Asian_Total"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Indian"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Pakistani"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Bangladeshi"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Any_other_Asian_background"),
                                        GetCharacteristicMeta("Ethnicity_Major_Black_Total"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Black_Caribbean"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Black_African"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Any_other_black_background"),
                                        GetCharacteristicMeta("Ethnicity_Major_Chinese"),
                                        GetCharacteristicMeta("Ethnicity_Major_Any_Other_Ethnic_Group"),
                                        GetCharacteristicMeta("Ethnicity_Minority_Ethnic_Group"),
                                        GetCharacteristicMeta("Ethnicity_Major_Unclassified"),
                                        GetCharacteristicMeta("NC_Year_1_and_below"),
                                        GetCharacteristicMeta("NC_Year_2"),
                                        GetCharacteristicMeta("NC_Year_3"),
                                        GetCharacteristicMeta("NC_Year_4"),
                                        GetCharacteristicMeta("NC_Year_5"),
                                        GetCharacteristicMeta("NC_Year_6"),
                                        GetCharacteristicMeta("NC_Year_7"),
                                        GetCharacteristicMeta("NC_Year_8"),
                                        GetCharacteristicMeta("NC_Year_9"),
                                        GetCharacteristicMeta("NC_Year_10"),
                                        GetCharacteristicMeta("NC_Year_11"),
                                        GetCharacteristicMeta("NC_Year_12_and_above"),
                                        GetCharacteristicMeta("NC_Year_Not_followed_or_missing"),
                                        GetCharacteristicMeta("FSM_eligible"),
                                        GetCharacteristicMeta("FSM_Not_eligible"),
                                        GetCharacteristicMeta("FSM_unclassified"),
                                        GetCharacteristicMeta("FSM_eligible_in_last_6_years"),
                                        GetCharacteristicMeta("FSM_not_eligible_in_last_6_years"),
                                        GetCharacteristicMeta("FSM_unclassified_in_last_6_years"),
                                        GetCharacteristicMeta("SEN_provision_No_identified_SEN"),
                                        GetCharacteristicMeta("SEN_provision_Statement_or_EHCP"),
                                        GetCharacteristicMeta("SEN_provision_SEN_Support"),
                                        GetCharacteristicMeta("SEN_provision_Unclassified"),
                                        GetCharacteristicMeta("SEN_primary_need_Autistic_spectrum_disorder"),
                                        GetCharacteristicMeta("SEN_primary_need_Hearing_impairment"),
                                        GetCharacteristicMeta("SEN_primary_need_Moderate_learning_difficulty"),
                                        GetCharacteristicMeta("SEN_primary_need_Multi-sensory_impairment"),
                                        GetCharacteristicMeta("SEN_primary_need_No_specialist_assessment"),
                                        GetCharacteristicMeta("SEN_primary_need_Other_difficulty/disability"),
                                        GetCharacteristicMeta("SEN_primary_need_Physical_disability"),
                                        GetCharacteristicMeta(
                                            "SEN_primary_need_Profound_and_multiple_learning_difficulty"),
                                        GetCharacteristicMeta("SEN_primary_need_Social_emotional_and_mental_health"),
                                        GetCharacteristicMeta(
                                            "SEN_primary_need_Speech_language_and_communications_needs"),
                                        GetCharacteristicMeta("SEN_primary_need_Severe_learning_difficulty"),
                                        GetCharacteristicMeta("SEN_primary_need_Specific_learning_difficulty"),
                                        GetCharacteristicMeta("SEN_primary_need_Visual_impairment"),
                                        GetCharacteristicMeta("SEN_primary_need_Unclassified"),
                                        GetCharacteristicMeta("First_language_Known_or_believed_to_be_English"),
                                        GetCharacteristicMeta(
                                            "First_language_Known_or_believed_to_be_other_than_English"),
                                        GetCharacteristicMeta("First_language_Unclassified")
                                    }
                                }
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
                    Releases = new[]
                    {
                        new Release
                        {
                            PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                            ReleaseDate = new DateTime(2018, 3, 22),
                            Name = "Permanent and fixed period exclusions",
                            DataSets = new[]
                            {
                                new DataSet
                                {
                                    Filename = DataCsvFilename.exclusion_geoglevels,
                                    IndicatorMetas = new[]
                                    {
                                        new MetaGroup<IndicatorMeta>
                                        {
                                            Name = "Exclusion fields",
                                            Meta = new[]
                                            {
                                                GetIndicatorMeta("num_schools"),
                                                GetIndicatorMeta("headcount"),
                                                GetIndicatorMeta("perm_excl"),
                                                GetIndicatorMeta("perm_excl_rate"),
                                                GetIndicatorMeta("fixed_excl"),
                                                GetIndicatorMeta("fixed_excl_rate"),
                                                GetIndicatorMeta("one_plus_fixed"),
                                                GetIndicatorMeta("one_plus_fixed_rate")
                                            }
                                        }
                                    },
                                    CharacteristicMetas = new CharacteristicMeta[]{}
                                },
                                new DataSet
                                {
                                    Filename = DataCsvFilename.exclusion_lacharacteristics,
                                    IndicatorMetas = new[]
                                    {
                                        new MetaGroup<IndicatorMeta>
                                        {
                                            Name = "Exclusion fields",
                                            Meta = new[]
                                            {
                                                GetIndicatorMeta("headcount"),
                                                GetIndicatorMeta("perm_excl"),
                                                GetIndicatorMeta("perm_excl_rate"),
                                                GetIndicatorMeta("fixed_excl"),
                                                GetIndicatorMeta("fixed_excl_rate"),
                                                GetIndicatorMeta("one_plus_fixed"),
                                                GetIndicatorMeta("one_plus_fixed_rate")
                                            }
                                        }
                                    },
                                    CharacteristicMetas = new[]
                                    {
                                        GetCharacteristicMeta("Total"),
                                        GetCharacteristicMeta("Gender_male"),
                                        GetCharacteristicMeta("Gender_female"),
                                        GetCharacteristicMeta("Age_4_and_under"),
                                        GetCharacteristicMeta("Age_5"),
                                        GetCharacteristicMeta("Age_6"),
                                        GetCharacteristicMeta("Age_7"),
                                        GetCharacteristicMeta("Age_8"),
                                        GetCharacteristicMeta("Age_9"),
                                        GetCharacteristicMeta("Age_10"),
                                        GetCharacteristicMeta("Age_11"),
                                        GetCharacteristicMeta("Age_12"),
                                        GetCharacteristicMeta("Age_13"),
                                        GetCharacteristicMeta("Age_14"),
                                        GetCharacteristicMeta("Age_15"),
                                        GetCharacteristicMeta("Age_16"),
                                        GetCharacteristicMeta("Age_17"),
                                        GetCharacteristicMeta("Age_18"),
                                        GetCharacteristicMeta("Age_19_and_over"),
                                        GetCharacteristicMeta("Ethnicity_Major_White_Total"),
                                        GetCharacteristicMeta("Ethnicity_Minor_White_British"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Irish"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Traveller_of_Irish_heritage"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Gypsy_Roma"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Any_other_white_background"),
                                        GetCharacteristicMeta("Ethnicity_Major_Mixed_Total"),
                                        GetCharacteristicMeta("Ethnicity_Minor_White_and_Black_Caribbean"),
                                        GetCharacteristicMeta("Ethnicity_Minor_White_and_Black_African"),
                                        GetCharacteristicMeta("Ethnicity_Minor_White_and_Asian"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Any_other_Mixed_background"),
                                        GetCharacteristicMeta("Ethnicity_Major_Asian_Total"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Indian"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Pakistani"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Bangladeshi"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Any_other_Asian_background"),
                                        GetCharacteristicMeta("Ethnicity_Major_Black_Total"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Black_Caribbean"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Black_African"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Any_other_black_background"),
                                        GetCharacteristicMeta("Ethnicity_Major_Chinese"),
                                        GetCharacteristicMeta("Ethnicity_Major_Any_Other_Ethnic_Group"),
                                        GetCharacteristicMeta("Ethnicity_Minority_Ethnic_Group"),
                                        GetCharacteristicMeta("Ethnicity_Major_Unclassified"),
                                        GetCharacteristicMeta("NC_Year_Nursery"),
                                        GetCharacteristicMeta("NC_Year_Reception"),
                                        GetCharacteristicMeta("NC_Year_1"),
                                        GetCharacteristicMeta("NC_Year_2"),
                                        GetCharacteristicMeta("NC_Year_3"),
                                        GetCharacteristicMeta("NC_Year_4"),
                                        GetCharacteristicMeta("NC_Year_5"),
                                        GetCharacteristicMeta("NC_Year_6"),
                                        GetCharacteristicMeta("NC_Year_7"),
                                        GetCharacteristicMeta("NC_Year_8"),
                                        GetCharacteristicMeta("NC_Year_9"),
                                        GetCharacteristicMeta("NC_Year_10"),
                                        GetCharacteristicMeta("NC_Year_11"),
                                        GetCharacteristicMeta("NC_Year_12_and_above"),
                                        GetCharacteristicMeta("NC_Year_Not_followed"),
                                        GetCharacteristicMeta("NC_Year_Unclassified"),
                                        GetCharacteristicMeta("FSM_eligible"),
                                        GetCharacteristicMeta("FSM_Not_eligible"),
                                        GetCharacteristicMeta("FSM_unclassified"),
                                        GetCharacteristicMeta("FSM_eligible_in_last_6_years"),
                                        GetCharacteristicMeta("FSM_not_eligible_in_last_6_years"),
                                        GetCharacteristicMeta("FSM_unclassified_in_last_6_years"),
                                        GetCharacteristicMeta("SEN_provision_No_identified_SEN"),
                                        GetCharacteristicMeta("SEN_provision_Statement_or_EHCP"),
                                        GetCharacteristicMeta("SEN_provision_SEN_Support"),
                                        GetCharacteristicMeta("SEN_primary_need_Autistic_spectrum_disorder"),
                                        GetCharacteristicMeta("SEN_primary_need_Hearing_impairment"),
                                        GetCharacteristicMeta("SEN_primary_need_Moderate_learning_difficulty"),
                                        GetCharacteristicMeta("SEN_primary_need_Multi-sensory_impairment"),
                                        GetCharacteristicMeta("SEN_primary_need_No_specialist_assessment"),
                                        GetCharacteristicMeta("SEN_primary_need_Other_difficulty/disability"),
                                        GetCharacteristicMeta("SEN_primary_need_Physical_disability"),
                                        GetCharacteristicMeta(
                                            "SEN_primary_need_Profound_and_multiple_learning_difficulty"),
                                        GetCharacteristicMeta(
                                            "SEN_primary_need_Social_emotional_and_mental_health"),
                                        GetCharacteristicMeta(
                                            "SEN_primary_need_Speech_language_and_communications_needs"),
                                        GetCharacteristicMeta("SEN_primary_need_Severe_learning_difficulty"),
                                        GetCharacteristicMeta("SEN_primary_need_Specific_learning_difficulty"),
                                        GetCharacteristicMeta("SEN_primary_need_Visual_impairment"),
                                        GetCharacteristicMeta("IDACI_decile_0_to_10pc_most_deprived"),
                                        GetCharacteristicMeta("IDACI_decile_10_to_20pc"),
                                        GetCharacteristicMeta("IDACI_decile_20_to_30pc"),
                                        GetCharacteristicMeta("IDACI_decile_30_to_40pc"),
                                        GetCharacteristicMeta("IDACI_decile_40_to_50pc"),
                                        GetCharacteristicMeta("IDACI_decile_50_to_60pc"),
                                        GetCharacteristicMeta("IDACI_decile_60_to_70pc"),
                                        GetCharacteristicMeta("IDACI_decile_70_to_80pc"),
                                        GetCharacteristicMeta("IDACI_decile_80_to_90pc"),
                                        GetCharacteristicMeta("IDACI_decile_90_to_100pc_least_deprived"),
                                        GetCharacteristicMeta("IDACI_decile_unclassified")
                                    }
                                },
                                new DataSet
                                {
                                    Filename = DataCsvFilename.exclusion_natcharacteristics,
                                    IndicatorMetas = new[]
                                    {
                                        new MetaGroup<IndicatorMeta>
                                        {
                                            Name = "Exclusion fields",
                                            Meta = new[]
                                            {
                                                GetIndicatorMeta("headcount"),
                                                GetIndicatorMeta("perm_excl"),
                                                GetIndicatorMeta("perm_excl_rate"),
                                                GetIndicatorMeta("fixed_excl"),
                                                GetIndicatorMeta("fixed_excl_rate"),
                                                GetIndicatorMeta("one_plus_fixed"),
                                                GetIndicatorMeta("one_plus_fixed_rate")
                                            }
                                        }
                                    },
                                    CharacteristicMetas = new[]
                                    {
                                        GetCharacteristicMeta("Total"),
                                        GetCharacteristicMeta("Gender_male"),
                                        GetCharacteristicMeta("Gender_female"),
                                        GetCharacteristicMeta("Age_4_and_under"),
                                        GetCharacteristicMeta("Age_5"),
                                        GetCharacteristicMeta("Age_6"),
                                        GetCharacteristicMeta("Age_7"),
                                        GetCharacteristicMeta("Age_8"),
                                        GetCharacteristicMeta("Age_9"),
                                        GetCharacteristicMeta("Age_10"),
                                        GetCharacteristicMeta("Age_11"),
                                        GetCharacteristicMeta("Age_12"),
                                        GetCharacteristicMeta("Age_13"),
                                        GetCharacteristicMeta("Age_14"),
                                        GetCharacteristicMeta("Age_15"),
                                        GetCharacteristicMeta("Age_16"),
                                        GetCharacteristicMeta("Age_17"),
                                        GetCharacteristicMeta("Age_18"),
                                        GetCharacteristicMeta("Age_19_and_over"),
                                        GetCharacteristicMeta("Ethnicity_Major_White_Total"),
                                        GetCharacteristicMeta("Ethnicity_Minor_White_British"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Irish"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Traveller_of_Irish_heritage"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Gypsy_Roma"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Any_other_white_background"),
                                        GetCharacteristicMeta("Ethnicity_Major_Mixed_Total"),
                                        GetCharacteristicMeta("Ethnicity_Minor_White_and_Black_Caribbean"),
                                        GetCharacteristicMeta("Ethnicity_Minor_White_and_Black_African"),
                                        GetCharacteristicMeta("Ethnicity_Minor_White_and_Asian"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Any_other_Mixed_background"),
                                        GetCharacteristicMeta("Ethnicity_Major_Asian_Total"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Indian"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Pakistani"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Bangladeshi"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Any_other_Asian_background"),
                                        GetCharacteristicMeta("Ethnicity_Major_Black_Total"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Black_Caribbean"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Black_African"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Any_other_black_background"),
                                        GetCharacteristicMeta("Ethnicity_Major_Chinese"),
                                        GetCharacteristicMeta("Ethnicity_Major_Any_Other_Ethnic_Group"),
                                        GetCharacteristicMeta("Ethnicity_Minority_Ethnic_Group"),
                                        GetCharacteristicMeta("Ethnicity_Major_Unclassified"),
                                        GetCharacteristicMeta("NC_Year_Nursery"),
                                        GetCharacteristicMeta("NC_Year_Reception"),
                                        GetCharacteristicMeta("NC_Year_1"),
                                        GetCharacteristicMeta("NC_Year_2"),
                                        GetCharacteristicMeta("NC_Year_3"),
                                        GetCharacteristicMeta("NC_Year_4"),
                                        GetCharacteristicMeta("NC_Year_5"),
                                        GetCharacteristicMeta("NC_Year_6"),
                                        GetCharacteristicMeta("NC_Year_7"),
                                        GetCharacteristicMeta("NC_Year_8"),
                                        GetCharacteristicMeta("NC_Year_9"),
                                        GetCharacteristicMeta("NC_Year_10"),
                                        GetCharacteristicMeta("NC_Year_11"),
                                        GetCharacteristicMeta("NC_Year_12_and_above"),
                                        GetCharacteristicMeta("NC_Year_Not_followed"),
                                        GetCharacteristicMeta("NC_Year_Unclassified"),
                                        GetCharacteristicMeta("FSM_eligible"),
                                        GetCharacteristicMeta("FSM_Not_eligible"),
                                        GetCharacteristicMeta("FSM_unclassified"),
                                        GetCharacteristicMeta("FSM_eligible_in_last_6_years"),
                                        GetCharacteristicMeta("FSM_not_eligible_in_last_6_years"),
                                        GetCharacteristicMeta("FSM_unclassified_in_last_6_years"),
                                        GetCharacteristicMeta("SEN_provision_No_identified_SEN"),
                                        GetCharacteristicMeta("SEN_provision_Statement_or_EHCP"),
                                        GetCharacteristicMeta("SEN_provision_SEN_Support"),
                                        GetCharacteristicMeta("SEN_primary_need_Autistic_spectrum_disorder"),
                                        GetCharacteristicMeta("SEN_primary_need_Hearing_impairment"),
                                        GetCharacteristicMeta("SEN_primary_need_Moderate_learning_difficulty"),
                                        GetCharacteristicMeta("SEN_primary_need_Multi-sensory_impairment"),
                                        GetCharacteristicMeta("SEN_primary_need_No_specialist_assessment"),
                                        GetCharacteristicMeta("SEN_primary_need_Other_difficulty/disability"),
                                        GetCharacteristicMeta("SEN_primary_need_Physical_disability"),
                                        GetCharacteristicMeta(
                                            "SEN_primary_need_Profound_and_multiple_learning_difficulty"),
                                        GetCharacteristicMeta(
                                            "SEN_primary_need_Social_emotional_and_mental_health"),
                                        GetCharacteristicMeta(
                                            "SEN_primary_need_Speech_language_and_communications_needs"),
                                        GetCharacteristicMeta("SEN_primary_need_Severe_learning_difficulty"),
                                        GetCharacteristicMeta("SEN_primary_need_Specific_learning_difficulty"),
                                        GetCharacteristicMeta("SEN_primary_need_Visual_impairment"),
                                        GetCharacteristicMeta("IDACI_decile_0_to_10pc_most_deprived"),
                                        GetCharacteristicMeta("IDACI_decile_10_to_20pc"),
                                        GetCharacteristicMeta("IDACI_decile_20_to_30pc"),
                                        GetCharacteristicMeta("IDACI_decile_30_to_40pc"),
                                        GetCharacteristicMeta("IDACI_decile_40_to_50pc"),
                                        GetCharacteristicMeta("IDACI_decile_50_to_60pc"),
                                        GetCharacteristicMeta("IDACI_decile_60_to_70pc"),
                                        GetCharacteristicMeta("IDACI_decile_70_to_80pc"),
                                        GetCharacteristicMeta("IDACI_decile_80_to_90pc"),
                                        GetCharacteristicMeta("IDACI_decile_90_to_100pc_least_deprived"),
                                        GetCharacteristicMeta("IDACI_decile_unclassified")
                                    }
                                }
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
                    Releases = new[]
                    {
                        new Release
                        {
                            PublicationId = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                            ReleaseDate = new DateTime(2018, 5, 30),
                            Name = "Schools, pupils and their characteristics",
                            DataSets = new[]
                            {
                                new DataSet
                                {
                                    Filename = DataCsvFilename.schpupnum_geoglevels,
                                    IndicatorMetas = new[]
                                    {
                                        new MetaGroup<IndicatorMeta>
                                        {
                                            Name = "School pupil fields",
                                            Meta = new[]
                                            {
                                                GetIndicatorMeta("num_schools"),
                                                GetIndicatorMeta("headcount")
                                            }
                                        }
                                    },
                                    CharacteristicMetas = new CharacteristicMeta[]{}
                                },
                                new DataSet
                                {
                                    Filename = DataCsvFilename.schpupnum_lacharacteristics,
                                    IndicatorMetas = new[]
                                    {
                                        new MetaGroup<IndicatorMeta>
                                        {
                                            Name = "School pupil fields",
                                            Meta = new[]
                                            {
                                                GetIndicatorMeta("headcount")
                                            }
                                        }
                                    },
                                    CharacteristicMetas = new[]
                                    {
                                        GetCharacteristicMeta("Total"),
                                        GetCharacteristicMeta("Gender_male"),
                                        GetCharacteristicMeta("Gender_female"),
                                        GetCharacteristicMeta("Age_4_and_under"),
                                        GetCharacteristicMeta("Age_5"),
                                        GetCharacteristicMeta("Age_6"),
                                        GetCharacteristicMeta("Age_7"),
                                        GetCharacteristicMeta("Age_8"),
                                        GetCharacteristicMeta("Age_9"),
                                        GetCharacteristicMeta("Age_10"),
                                        GetCharacteristicMeta("Age_11"),
                                        GetCharacteristicMeta("Age_12"),
                                        GetCharacteristicMeta("Age_13"),
                                        GetCharacteristicMeta("Age_14"),
                                        GetCharacteristicMeta("Age_15"),
                                        GetCharacteristicMeta("Age_16"),
                                        GetCharacteristicMeta("Age_17"),
                                        GetCharacteristicMeta("Age_18"),
                                        GetCharacteristicMeta("Age_19_and_over"),
                                        GetCharacteristicMeta("Ethnicity_Major_White_Total"),
                                        GetCharacteristicMeta("Ethnicity_Minor_White_British"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Irish"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Traveller_of_Irish_heritage"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Gypsy_Roma"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Any_other_white_background"),
                                        GetCharacteristicMeta("Ethnicity_Major_Mixed_Total"),
                                        GetCharacteristicMeta("Ethnicity_Minor_White_and_Black_Caribbean"),
                                        GetCharacteristicMeta("Ethnicity_Minor_White_and_Black_African"),
                                        GetCharacteristicMeta("Ethnicity_Minor_White_and_Asian"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Any_other_Mixed_background"),
                                        GetCharacteristicMeta("Ethnicity_Major_Asian_Total"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Indian"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Pakistani"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Bangladeshi"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Any_other_Asian_background"),
                                        GetCharacteristicMeta("Ethnicity_Major_Black_Total"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Black_Caribbean"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Black_African"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Any_other_black_background"),
                                        GetCharacteristicMeta("Ethnicity_Major_Chinese"),
                                        GetCharacteristicMeta("Ethnicity_Major_Any_Other_Ethnic_Group"),
                                        GetCharacteristicMeta("Ethnicity_Minority_Ethnic_Group"),
                                        GetCharacteristicMeta("Ethnicity_Major_Unclassified"),
                                        GetCharacteristicMeta("NC_Year_Nursery"),
                                        GetCharacteristicMeta("NC_Year_Reception"),
                                        GetCharacteristicMeta("NC_Year_1"),
                                        GetCharacteristicMeta("NC_Year_2"),
                                        GetCharacteristicMeta("NC_Year_3"),
                                        GetCharacteristicMeta("NC_Year_4"),
                                        GetCharacteristicMeta("NC_Year_5"),
                                        GetCharacteristicMeta("NC_Year_6"),
                                        GetCharacteristicMeta("NC_Year_7"),
                                        GetCharacteristicMeta("NC_Year_8"),
                                        GetCharacteristicMeta("NC_Year_9"),
                                        GetCharacteristicMeta("NC_Year_10"),
                                        GetCharacteristicMeta("NC_Year_11"),
                                        GetCharacteristicMeta("NC_Year_12_and_above"),
                                        GetCharacteristicMeta("NC_Year_Not_followed"),
                                        GetCharacteristicMeta("NC_Year_Unclassified"),
                                        GetCharacteristicMeta("FSM_eligible"),
                                        GetCharacteristicMeta("FSM_Not_eligible"),
                                        GetCharacteristicMeta("FSM_unclassified"),
                                        GetCharacteristicMeta("FSM_eligible_in_last_6_years"),
                                        GetCharacteristicMeta("FSM_not_eligible_in_last_6_years"),
                                        GetCharacteristicMeta("FSM_unclassified_in_last_6_years"),
                                        GetCharacteristicMeta("SEN_provision_No_identified_SEN"),
                                        GetCharacteristicMeta("SEN_provision_Statement_or_EHCP"),
                                        GetCharacteristicMeta("SEN_provision_SEN_Support"),
                                        GetCharacteristicMeta("SEN_primary_need_Autistic_spectrum_disorder"),
                                        GetCharacteristicMeta("SEN_primary_need_Hearing_impairment"),
                                        GetCharacteristicMeta("SEN_primary_need_Moderate_learning_difficulty"),
                                        GetCharacteristicMeta("SEN_primary_need_Multi-sensory_impairment"),
                                        GetCharacteristicMeta("SEN_primary_need_No_specialist_assessment"),
                                        GetCharacteristicMeta("SEN_primary_need_Other_difficulty/disability"),
                                        GetCharacteristicMeta("SEN_primary_need_Physical_disability"),
                                        GetCharacteristicMeta(
                                            "SEN_primary_need_Profound_and_multiple_learning_difficulty"),
                                        GetCharacteristicMeta("SEN_primary_need_Social_emotional_and_mental_health"),
                                        GetCharacteristicMeta(
                                            "SEN_primary_need_Speech_language_and_communications_needs"),
                                        GetCharacteristicMeta("SEN_primary_need_Severe_learning_difficulty"),
                                        GetCharacteristicMeta("SEN_primary_need_Specific_learning_difficulty"),
                                        GetCharacteristicMeta("SEN_primary_need_Visual_impairment"),
                                        GetCharacteristicMeta("IDACI_decile_0_to_10pc_most_deprived"),
                                        GetCharacteristicMeta("IDACI_decile_10_to_20pc"),
                                        GetCharacteristicMeta("IDACI_decile_20_to_30pc"),
                                        GetCharacteristicMeta("IDACI_decile_30_to_40pc"),
                                        GetCharacteristicMeta("IDACI_decile_40_to_50pc"),
                                        GetCharacteristicMeta("IDACI_decile_50_to_60pc"),
                                        GetCharacteristicMeta("IDACI_decile_60_to_70pc"),
                                        GetCharacteristicMeta("IDACI_decile_70_to_80pc"),
                                        GetCharacteristicMeta("IDACI_decile_80_to_90pc"),
                                        GetCharacteristicMeta("IDACI_decile_90_to_100pc_least_deprived"),
                                        GetCharacteristicMeta("IDACI_decile_unclassified")
                                    }
                                },
                                new DataSet
                                {
                                    Filename = DataCsvFilename.schpupnum_natcharacteristics,
                                    IndicatorMetas = new[]
                                    {
                                        new MetaGroup<IndicatorMeta>
                                        {
                                            Name = "School pupil fields",
                                            Meta = new[]
                                            {
                                                GetIndicatorMeta("headcount")
                                            }
                                        }
                                    },
                                    CharacteristicMetas = new[]
                                    {
                                        GetCharacteristicMeta("Total"),
                                        GetCharacteristicMeta("Gender_male"),
                                        GetCharacteristicMeta("Gender_female"),
                                        GetCharacteristicMeta("Age_4_and_under"),
                                        GetCharacteristicMeta("Age_5"),
                                        GetCharacteristicMeta("Age_6"),
                                        GetCharacteristicMeta("Age_7"),
                                        GetCharacteristicMeta("Age_8"),
                                        GetCharacteristicMeta("Age_9"),
                                        GetCharacteristicMeta("Age_10"),
                                        GetCharacteristicMeta("Age_11"),
                                        GetCharacteristicMeta("Age_12"),
                                        GetCharacteristicMeta("Age_13"),
                                        GetCharacteristicMeta("Age_14"),
                                        GetCharacteristicMeta("Age_15"),
                                        GetCharacteristicMeta("Age_16"),
                                        GetCharacteristicMeta("Age_17"),
                                        GetCharacteristicMeta("Age_18"),
                                        GetCharacteristicMeta("Age_19_and_over"),
                                        GetCharacteristicMeta("Ethnicity_Major_White_Total"),
                                        GetCharacteristicMeta("Ethnicity_Minor_White_British"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Irish"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Traveller_of_Irish_heritage"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Gypsy_Roma"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Any_other_white_background"),
                                        GetCharacteristicMeta("Ethnicity_Major_Mixed_Total"),
                                        GetCharacteristicMeta("Ethnicity_Minor_White_and_Black_Caribbean"),
                                        GetCharacteristicMeta("Ethnicity_Minor_White_and_Black_African"),
                                        GetCharacteristicMeta("Ethnicity_Minor_White_and_Asian"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Any_other_Mixed_background"),
                                        GetCharacteristicMeta("Ethnicity_Major_Asian_Total"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Indian"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Pakistani"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Bangladeshi"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Any_other_Asian_background"),
                                        GetCharacteristicMeta("Ethnicity_Major_Black_Total"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Black_Caribbean"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Black_African"),
                                        GetCharacteristicMeta("Ethnicity_Minor_Any_other_black_background"),
                                        GetCharacteristicMeta("Ethnicity_Major_Chinese"),
                                        GetCharacteristicMeta("Ethnicity_Major_Any_Other_Ethnic_Group"),
                                        GetCharacteristicMeta("Ethnicity_Minority_Ethnic_Group"),
                                        GetCharacteristicMeta("Ethnicity_Major_Unclassified"),
                                        GetCharacteristicMeta("NC_Year_Nursery"),
                                        GetCharacteristicMeta("NC_Year_Reception"),
                                        GetCharacteristicMeta("NC_Year_1"),
                                        GetCharacteristicMeta("NC_Year_2"),
                                        GetCharacteristicMeta("NC_Year_3"),
                                        GetCharacteristicMeta("NC_Year_4"),
                                        GetCharacteristicMeta("NC_Year_5"),
                                        GetCharacteristicMeta("NC_Year_6"),
                                        GetCharacteristicMeta("NC_Year_7"),
                                        GetCharacteristicMeta("NC_Year_8"),
                                        GetCharacteristicMeta("NC_Year_9"),
                                        GetCharacteristicMeta("NC_Year_10"),
                                        GetCharacteristicMeta("NC_Year_11"),
                                        GetCharacteristicMeta("NC_Year_12_and_above"),
                                        GetCharacteristicMeta("NC_Year_Not_followed"),
                                        GetCharacteristicMeta("NC_Year_Unclassified"),
                                        GetCharacteristicMeta("FSM_eligible"),
                                        GetCharacteristicMeta("FSM_Not_eligible"),
                                        GetCharacteristicMeta("FSM_unclassified"),
                                        GetCharacteristicMeta("FSM_eligible_in_last_6_years"),
                                        GetCharacteristicMeta("FSM_not_eligible_in_last_6_years"),
                                        GetCharacteristicMeta("FSM_unclassified_in_last_6_years"),
                                        GetCharacteristicMeta("SEN_provision_No_identified_SEN"),
                                        GetCharacteristicMeta("SEN_provision_Statement_or_EHCP"),
                                        GetCharacteristicMeta("SEN_provision_SEN_Support"),
                                        GetCharacteristicMeta("SEN_primary_need_Autistic_spectrum_disorder"),
                                        GetCharacteristicMeta("SEN_primary_need_Hearing_impairment"),
                                        GetCharacteristicMeta("SEN_primary_need_Moderate_learning_difficulty"),
                                        GetCharacteristicMeta("SEN_primary_need_Multi-sensory_impairment"),
                                        GetCharacteristicMeta("SEN_primary_need_No_specialist_assessment"),
                                        GetCharacteristicMeta("SEN_primary_need_Other_difficulty/disability"),
                                        GetCharacteristicMeta("SEN_primary_need_Physical_disability"),
                                        GetCharacteristicMeta(
                                            "SEN_primary_need_Profound_and_multiple_learning_difficulty"),
                                        GetCharacteristicMeta("SEN_primary_need_Social_emotional_and_mental_health"),
                                        GetCharacteristicMeta(
                                            "SEN_primary_need_Speech_language_and_communications_needs"),
                                        GetCharacteristicMeta("SEN_primary_need_Severe_learning_difficulty"),
                                        GetCharacteristicMeta("SEN_primary_need_Specific_learning_difficulty"),
                                        GetCharacteristicMeta("SEN_primary_need_Visual_impairment"),
                                        GetCharacteristicMeta("IDACI_decile_0_to_10pc_most_deprived"),
                                        GetCharacteristicMeta("IDACI_decile_10_to_20pc"),
                                        GetCharacteristicMeta("IDACI_decile_20_to_30pc"),
                                        GetCharacteristicMeta("IDACI_decile_30_to_40pc"),
                                        GetCharacteristicMeta("IDACI_decile_40_to_50pc"),
                                        GetCharacteristicMeta("IDACI_decile_50_to_60pc"),
                                        GetCharacteristicMeta("IDACI_decile_60_to_70pc"),
                                        GetCharacteristicMeta("IDACI_decile_70_to_80pc"),
                                        GetCharacteristicMeta("IDACI_decile_80_to_90pc"),
                                        GetCharacteristicMeta("IDACI_decile_90_to_100pc_least_deprived"),
                                        GetCharacteristicMeta("IDACI_decile_unclassified")
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }
}