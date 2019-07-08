using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Seed
{
    public static class SamplePublications
    {
        public static readonly Dictionary<long, DataCsvFile> SubjectFiles =
            new Dictionary<long, DataCsvFile>
            {
                {1, DataCsvFile.absence_by_characteristic},
                {2, DataCsvFile.absence_by_geographic_level},
                {3, DataCsvFile.absence_by_term},
                {4, DataCsvFile.absence_for_four_year_olds},
                {5, DataCsvFile.absence_in_prus},
                {6, DataCsvFile.absence_number_missing_at_least_one_session_by_reason},
                {7, DataCsvFile.absence_rate_percent_bands},
                {8, DataCsvFile.EYFSP_ELG_underlying_data_2013_2018},
                {9, DataCsvFile.EYFSP_areas_of_learning_underlying_data_2013_2018},
                {10, DataCsvFile.EYFSP_APS_GLD_ELG_underlying_data_2013_2018},
                {11, DataCsvFile.exclusions_by_characteristic},
                {12, DataCsvFile.exclusions_by_geographic_level},
                {13, DataCsvFile.exclusions_by_reason},
                {14, DataCsvFile.exclusions_duration_of_fixed_exclusions},
                {15, DataCsvFile.exclusions_number_of_fixed_exclusions},
                {16, DataCsvFile.exclusions_total_days_missed_fixed_exclusions},
                {17, DataCsvFile.school_applications_and_offers},
                {18, DataCsvFile.SEN2_AGE_NEW},
                {19, DataCsvFile.SEN2_AGE_STOCK},
                {20, DataCsvFile.SEN2_ESTAB_NEW},
                {21, DataCsvFile.SEN2_ESTAB_STOCK},
                {22, DataCsvFile.SEN2_MI},
                {23, DataCsvFile.skeleton_dashboard_tidy_data_NARTS},
                {24, DataCsvFile.skeleton_dashboard_tidy_data_annual_v4},
                {25, DataCsvFile.clean_data_fe},
                {26, DataCsvFile.level_2_3_national},
                {27, DataCsvFile.level_2_3_sf},
                {28, DataCsvFile.level_2_3_sfla},
                {29, DataCsvFile.KS2_2016_test_UD},
                {30, DataCsvFile.KS4_2018_LA_Char_Testdata},
                {31, DataCsvFile.KS4_2018_Nat_Char_Testdata},
                {32, DataCsvFile.KS4_2018_Subject_Tables_S1_TestData},
                {33, DataCsvFile.KS4_2018_Subject_Tables_S3_TestData}
            };

        public static readonly IEnumerable<Theme> Themes = new List<Theme>
        {
            new Theme
            {
                Id = new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f"),
                Title = "Pupils and schools",
                Slug = "pupils-and-schools",
                Topics = new[]
                {
                    new Topic
                    {
                        Id = new Guid("67c249de-1cca-446e-8ccb-dcdac542f460"),
                        Title = "Pupil absence",
                        Slug = "pupil-absence",
                        Publications = new[]
                        {
                            new Publication
                            {
                                Id = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                                Title = "Pupil absence in schools in England",
                                Slug = "pupil-absence-in-schools-in-england",
                                Releases = new[]
                                {
                                    new Release
                                    {
                                        Id = new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                                        Title = "2016 to 2017",
                                        ReleaseDate = new DateTime(2018, 4, 25),
                                        Slug = "2016-17",
                                        Subjects = new List<Subject>
                                        {
                                            new Subject
                                            {
                                                Id = 1,
                                                Name = "Absence by characteristic"
                                            },
                                            new Subject
                                            {
                                                Id = 2,
                                                Name = "Absence by geographic level"
                                            },
                                            new Subject
                                            {
                                                Id = 3,
                                                Name = "Absence by term"
                                            },
                                            new Subject
                                            {
                                                Id = 4,
                                                Name = "Absence for four year olds"
                                            },
                                            new Subject
                                            {
                                                Id = 5,
                                                Name = "Absence in prus"
                                            },
                                            new Subject
                                            {
                                                Id = 6,
                                                Name = "Absence number missing at least one session by reason"
                                            },
                                            new Subject
                                            {
                                                Id = 7,
                                                Name = "Absence rate percent bands"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new Topic
                    {
                        Id = new Guid("77941b7d-bbd6-4069-9107-565af89e2dec"),
                        Title = "Exclusions",
                        Slug = "exclusions",
                        Publications = new[]
                        {
                            new Publication
                            {
                                Id = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                                Title = "Permanent and fixed-period exclusions in England",
                                Slug = "permanent-and-fixed-period-exclusions-in-england",
                                Releases = new[]
                                {
                                    new Release
                                    {
                                        Id = new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                                        Title = "2016 to 2017",
                                        ReleaseDate = new DateTime(2018, 7, 19),
                                        Slug = "2016-17",
                                        Subjects = new List<Subject>
                                        {
                                            new Subject
                                            {
                                                Id = 11,
                                                Name = "Exclusions by characteristic"
                                            },
                                            new Subject
                                            {
                                                Id = 12,
                                                Name = "Exclusions by geographic level"
                                            },
                                            new Subject
                                            {
                                                Id = 13,
                                                Name = "Exclusions by reason"
                                            },
                                            new Subject
                                            {
                                                Id = 14,
                                                Name = "Duration of fixed exclusions"
                                            },
                                            new Subject
                                            {
                                                Id = 15,
                                                Name = "Number of fixed exclusions"
                                            },
                                            new Subject
                                            {
                                                Id = 16,
                                                Name = "Total days missed due to fixed period exclusions"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new Topic
                    {
                        Id = new Guid("1a9636e4-29d5-4c90-8c07-f41db8dd019c"),
                        Title = "School applications",
                        Slug = "school-applications",
                        Publications = new[]
                        {
                            new Publication
                            {
                                Id = new Guid("66c8e9db-8bf2-4b0b-b094-cfab25c20b05"),
                                Title = "Secondary and primary schools applications and offers",
                                Slug = "secondary-and-primary-schools-applications-and-offers",
                                Releases = new[]
                                {
                                    new Release
                                    {
                                        Id = new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"),
                                        Title = "2018",
                                        ReleaseDate = new DateTime(2019, 4, 29),
                                        Slug = "2018",
                                        Subjects = new List<Subject>
                                        {
                                            new Subject
                                            {
                                                Id = 17,
                                                Name = "Applications and offers by school phase"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new Topic
                    {
                        Id = new Guid("dfc908db-242a-4e3a-b6c6-e3f66cd152af"),
                        Title = "Special educational needs (SEN)",
                        Slug = "sen",
                        Publications = new[]
                        {
                            new Publication
                            {
                                Id = new Guid("2d94e5c8-a272-497c-bda0-c1f6b75155b0"),
                                Title = "Statements of SEN and EHC plans",
                                Slug = "statements-of-sen-and-ehc-plans",
                                Releases = new[]
                                {
                                    new Release
                                    {
                                        Id = new Guid("70efdb76-7e88-453f-95f1-7bb9af023db5"),
                                        Title = "2018",
                                        ReleaseDate = new DateTime(2019, 4, 29),
                                        Slug = "2018",
                                        Subjects = new List<Subject>
                                        {
                                            new Subject
                                            {
                                                Id = 18,
                                                Name = "New cases by age"
                                            },
                                            new Subject
                                            {
                                                Id = 19,
                                                Name = "Stock cases by age"
                                            },
                                            new Subject
                                            {
                                                Id = 20,
                                                Name = "New cases by establishment"
                                            },
                                            new Subject
                                            {
                                                Id = 21,
                                                Name = "Stock cases by establishment"
                                            },
                                            new Subject
                                            {
                                                Id = 22,
                                                Name = "Management information"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            },
            new Theme
            {
                Id = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"),
                Title = "Children, early years and social care",
                Slug = "children-and-early-years",
                Topics = new[]
                {
                    new Topic
                    {
                        Id = new Guid("17b2e32c-ed2f-4896-852b-513cdf466769"),
                        Title = "Early years foundation stage profile",
                        Slug = "early-years-foundation-stage-profile",
                        Publications = new[]
                        {
                            new Publication
                            {
                                Id = new Guid("fcda2962-82a6-4052-afa2-ea398c53c85f"),
                                Title = "Early years foundation stage profile results",
                                Slug = "early-years-foundation-stage-profile-results",
                                Releases = new[]
                                {
                                    new Release
                                    {
                                        Id = new Guid("47299b78-a4a6-4f7e-a86f-4713f4a0599a"),
                                        Title = "2017 to 2018",
                                        ReleaseDate = new DateTime(2019, 5, 20),
                                        Slug = "2017-18",
                                        Subjects = new List<Subject>
                                        {
                                            new Subject
                                            {
                                                Id = 8,
                                                Name = "ELG underlying data 2013 - 2018"
                                            },
                                            new Subject
                                            {
                                                Id = 9,
                                                Name = "Areas of learning underlying data 2013 - 2018"
                                            },
                                            new Subject
                                            {
                                                Id = 10,
                                                Name = "APS GLD ELG underlying data 2013 - 2018"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            },
            new Theme
            {
                Id = new Guid("9aa81762-e52c-40d4-8a90-f469977360a7"),
                Title = "Further education",
                Slug = "further-education",
                Topics = new[]
                {
                    new Topic
                    {
                        Id = new Guid("721048b9-8c06-4bad-8585-8789fa38a03b"),
                        Title = "National achievement rates tables",
                        Slug = "national-achievement-rates-tables",
                        Publications = new[]
                        {
                            new Publication
                            {
                                Id = new Guid("99ce35fb-3fe2-48bb-9b73-23159df9d5ea"),
                                Title = "National achievement rates tables",
                                Slug = "national-achievement-rates-tables",
                                Releases = new[]
                                {
                                    new Release
                                    {
                                        Id = new Guid("59258583-b075-47a2-bee4-5969e2d58873"),
                                        ReleaseDate = new DateTime(2019, 4, 29),
                                        Title = "2018",
                                        Slug = "2018",
                                        Subjects = new[]
                                        {
                                            new Subject
                                            {
                                                Id = 23,
                                                Name = "National achievement rates tables (NARTs)"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new Topic
                    {
                        Id = new Guid("71444ff6-614f-405b-b6c7-f72077d42e34"),
                        Title = "Further education and skills",
                        Slug = "further-education-and-skills",
                        Publications = new[]
                        {
                            new Publication
                            {
                                Id = new Guid("5aea252e-fddc-42b3-a1da-47f12e523e70"),
                                Title = "Apprenticeships and traineeships",
                                Slug = "apprenticeships-and-traineeships",
                                Releases = new[]
                                {
                                    new Release
                                    {
                                        Id = new Guid("463c8521-d9b4-4ccc-aee9-0666e39c8e47"),
                                        ReleaseDate = new DateTime(2019, 4, 29),
                                        Title = "2018",
                                        Slug = "2018",
                                        Subjects = new[]
                                        {
                                            new Subject
                                            {
                                                Id = 24,
                                                Name = "Apprenticeship annual"
                                            }
                                        }
                                    }
                                }
                            },
                            new Publication
                            {
                                Id = new Guid("d5a01a5c-cd57-482f-8a19-803b266e1012"),
                                Title = "Further education and skills",
                                Slug = "further-education-and-skills",
                                Releases = new[]
                                {
                                    new Release
                                    {
                                        Id = new Guid("6ccc4416-7d22-46bf-a12a-56037831dc60"),
                                        ReleaseDate = new DateTime(2019, 4, 29),
                                        Title = "2018",
                                        Slug = "2018",
                                        Subjects = new[]
                                        {
                                            new Subject
                                            {
                                                Id = 25,
                                                Name = "Further education and skills"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            },

            new Theme
            {
                Id = new Guid("fe805471-17e9-4ac6-a555-c7d0ebec1b90"),
                Title = "School and college outcomes and performance",
                Slug = "outcomes-and-performance",
                Topics = new[]
                {
                    new Topic
                    {
                        Id = new Guid("9e4fa097-2999-4c4d-9ecd-0c4733fc71b4"),
                        Title = "16 to 19 attainment",
                        Slug = "sixteen-to-nineteen-attainment",
                        Publications = new[]
                        {
                            new Publication
                            {
                                Id = new Guid("adb95888-64c7-4aa7-ba70-a9e535f8a30f"),
                                Title = "Level 2 and 3 attainment by young people aged 19",
                                Slug = "Level 2 and 3 attainment by young people aged 19",
                                Releases = new[]
                                {
                                    new Release
                                    {
                                        Id = new Guid("0dafd89b-b754-44a8-b3f1-72baac0a108a"),
                                        ReleaseDate = new DateTime(2019, 4, 29),
                                        Title = "2018",
                                        Slug = "2018",
                                        Subjects = new[]
                                        {
                                            new Subject
                                            {
                                                Id = 26,
                                                Name = "Level 2 and 3 National"
                                            },
                                            new Subject
                                            {
                                                Id = 27,
                                                Name = "Level 2 and 3 sf"
                                            },
                                            new Subject
                                            {
                                                Id = 28,
                                                Name = "Level 2 and 3 sf by Local authority"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new Topic
                    {
                        Id = new Guid("f38469bd-a5f7-46b1-96bb-3b0a01e9e53f"),
                        Title = "Key stage 2",
                        Slug = "key-stage-two",
                        Publications = new[]
                        {
                            new Publication
                            {
                                Id = new Guid("90ecb3d3-bd05-4f84-a73e-1d153568b320"),
                                Title = "National curriculum assessments at key stage 2",
                                Slug = "national-curriculum-assessments-key-stage2",
                                Releases = new[]
                                {
                                    new Release
                                    {
                                        Id = new Guid("dbaeb363-33fa-4928-870f-5054278e0c9a"),
                                        ReleaseDate = new DateTime(2019, 4, 29),
                                        Title = "2018",
                                        Slug = "2018",
                                        Subjects = new[]
                                        {
                                            new Subject
                                            {
                                                Id = 29,
                                                Name = "2016 test data"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new Topic
                    {
                        Id = new Guid("81fbb21d-3c49-46a2-8b43-0076974114f7"),
                        Title = "GCSEs (key stage 4)",
                        Slug = "key-stage-four",
                        Publications = new[]
                        {
                            new Publication
                            {
                                Id = new Guid("15659c96-a624-4457-846d-2ab5f3db6aec"),
                                Title = "GCSE and equivalent results, including pupil characteristics",
                                Slug = "gcse-results-including-pupil-characteristics",
                                Releases = new[]
                                {
                                    new Release
                                    {
                                        Id = new Guid("737dbab8-4e62-4d56-b0d6-5b4602a20801"),
                                        ReleaseDate = new DateTime(2019, 4, 29),
                                        Title = "2018",
                                        Slug = "2018",
                                        Subjects = new[]
                                        {
                                            new Subject
                                            {
                                                Id = 30,
                                                Name = "Characteristic test data by Local authority"
                                            },
                                            new Subject
                                            {
                                                Id = 31,
                                                Name = "National characteristic test data"
                                            },
                                            new Subject
                                            {
                                                Id = 32,
                                                Name = "Subject tables S1 test data"
                                            },
                                            new Subject
                                            {
                                                Id = 33,
                                                Name = "Subject tables S3 test data"
                                            }
                                        }
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