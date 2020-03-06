using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Seed
{
    public static class SamplePublications
    {
        public static readonly Dictionary<Guid, DataCsvFile> SubjectFiles =
            new Dictionary<Guid, DataCsvFile>
            {
                {new Guid("803fbf56-600f-490f-8409-6413a891720d"), DataCsvFile.absence_by_characteristic},
                {new Guid("568576e5-d386-450e-a8db-307b7061d0d8"), DataCsvFile.absence_by_geographic_level},
                {new Guid("b7bc537b-0c04-4b15-9eb6-4f0e8cc2e70a"), DataCsvFile.absence_by_term},
                {new Guid("353db5ea-befd-488b-ad16-2ce7963c9bc9"), DataCsvFile.absence_for_four_year_olds},
                {new Guid("95c7f584-907e-4756-bbf0-4905ceae57df"), DataCsvFile.absence_in_prus},
                {new Guid("faf2152e-0a6c-4e97-af02-e9a89d48c47a"), DataCsvFile.absence_number_missing_at_least_one_session_by_reason},
                {new Guid("666cd878-87bb-4f77-9a3f-f5c75078e112"), DataCsvFile.absence_rate_percent_bands},
                {new Guid("01ebdcfc-979a-4fe7-8d39-e5f8a6546e95"), DataCsvFile.EYFSP_ELG_underlying_data_2013_2018},
                {new Guid("55759ccd-da8f-4a27-9f4a-ca669f4218e0"), DataCsvFile.EYFSP_areas_of_learning_underlying_data_2013_2018},
                {new Guid("8e3d1bc0-2beb-4dc6-9db7-3d27d0608042"), DataCsvFile.EYFSP_APS_GLD_ELG_underlying_data_2013_2018},
                {new Guid("92039f68-a894-46a9-bd44-4482728698b0"), DataCsvFile.exclusions_by_characteristic},
                {new Guid("3c0fbe56-0a4b-4caa-82f2-ab696cd96090"), DataCsvFile.exclusions_by_geographic_level},
                {new Guid("8fe88bbe-dce7-4698-a55f-8e1e3e41c5a7"), DataCsvFile.exclusions_by_reason},
                {new Guid("926e33e4-b3ce-41aa-9ed2-e04106068ffb"), DataCsvFile.exclusions_duration_of_fixed_exclusions},
                {new Guid("048a6276-1df1-487a-a501-fbd5e64d4b79"), DataCsvFile.exclusions_number_of_fixed_exclusions},
                {new Guid("28feb263-4bf9-4dd7-9440-48e2685f6954"), DataCsvFile.exclusions_total_days_missed_fixed_exclusions},
                {new Guid("fa0d7f1d-d181-43fb-955b-fc327da86f2c"), DataCsvFile.school_applications_and_offers},
                {new Guid("685104a5-1424-40ab-bf4e-bf0515a4fd7b"), DataCsvFile.SEN2_AGE_NEW},
                {new Guid("8074c6a2-0fb5-4732-8c82-82ec8dccf7e6"), DataCsvFile.SEN2_AGE_STOCK},
                {new Guid("d64069e1-f97e-4a9b-afc1-a5f121cae921"), DataCsvFile.SEN2_ESTAB_NEW},
                {new Guid("eaad58a4-3f80-4703-b2af-3de4b678fa77"), DataCsvFile.SEN2_ESTAB_STOCK},
                {new Guid("3e36d5f0-668d-4d9b-bfa1-8585b5223769"), DataCsvFile.SEN2_MI},
                {new Guid("0339cd60-f0f1-4b27-a9cc-c2066d21293a"), DataCsvFile.skeleton_dashboard_tidy_data_NARTS},
                {new Guid("46602e6d-f725-46eb-bb34-8473565f43a8"), DataCsvFile.skeleton_dashboard_tidy_data_annual_v4},
                {new Guid("357b7e2c-7e65-477b-a576-16b8011da7d9"), DataCsvFile.level_2_3_national},
                {new Guid("59db9158-a168-4fcb-a2ba-c46146960b1a"), DataCsvFile.level_2_3_sf},
                {new Guid("13faaf84-0a54-470f-bfd9-ebd8b090ed1d"), DataCsvFile.level_2_3_sfla},
                {new Guid("25ea589f-660a-46ec-83c3-b60647baea3d"), DataCsvFile.KS2_2016_test_UD},
                {new Guid("0c7a7a5d-9520-4ca1-b01e-fe061ebeb122"), DataCsvFile.KS4_2018_LA_Char_Testdata},
                {new Guid("ce5cc950-ccf6-4423-817a-48eb6a411fcc"), DataCsvFile.KS4_2018_Nat_Char_Testdata},
                {new Guid("6b81ee8c-95d2-4603-8f71-ad03686cfbe1"), DataCsvFile.KS4_2018_Subject_Tables_S1_TestData},
                {new Guid("3d46479c-0adc-49c7-baf9-250e4b86ee4b"), DataCsvFile.KS4_2018_Subject_Tables_S3_TestData},
                
                {new Guid("7a9a3afc-e751-4f64-a3fa-edff5a5f9417"), DataCsvFile.clean_data_fe}

            };

        // Ignore these subjects for now until we are provided with valid data files.
        private static readonly List<DataCsvFile> IgnoredSubjectFiles = new List<DataCsvFile>
        {
            DataCsvFile.clean_data_fe,
            DataCsvFile.KS2_2016_test_UD

//            DataCsvFile.absence_by_characteristic,
//            DataCsvFile.absence_by_geographic_level,
//            DataCsvFile.absence_by_term,
//            DataCsvFile.absence_for_four_year_olds,
//            DataCsvFile.absence_in_prus,
//            DataCsvFile.absence_number_missing_at_least_one_session_by_reason,
//            DataCsvFile.absence_rate_percent_bands,
//            DataCsvFile.EYFSP_ELG_underlying_data_2013_2018,
//            DataCsvFile.EYFSP_areas_of_learning_underlying_data_2013_2018,
//            DataCsvFile.EYFSP_APS_GLD_ELG_underlying_data_2013_2018,
//            DataCsvFile.exclusions_by_characteristic,
//            DataCsvFile.exclusions_by_geographic_level,
//            DataCsvFile.exclusions_by_reason,
//            DataCsvFile.exclusions_duration_of_fixed_exclusions,
//            DataCsvFile.exclusions_number_of_fixed_exclusions,
//            DataCsvFile.exclusions_total_days_missed_fixed_exclusions,
//            DataCsvFile.school_applications_and_offers,
//            DataCsvFile.SEN2_AGE_NEW,
//            DataCsvFile.SEN2_AGE_STOCK,
//            DataCsvFile.SEN2_ESTAB_NEW,
//            DataCsvFile.SEN2_ESTAB_STOCK,
//            DataCsvFile.SEN2_MI,
//            DataCsvFile.skeleton_dashboard_tidy_data_NARTS,
//            DataCsvFile.skeleton_dashboard_tidy_data_annual_v4,
//            DataCsvFile.level_2_3_national,
//            DataCsvFile.level_2_3_sf,
//            DataCsvFile.level_2_3_sfla,
//            DataCsvFile.KS4_2018_LA_Char_Testdata,
//            DataCsvFile.KS4_2018_Nat_Char_Testdata,
//            DataCsvFile.KS4_2018_Subject_Tables_S1_TestData,
//            DataCsvFile.KS4_2018_Subject_Tables_S3_TestData
        };

        public static List<Subject> GetSubjects()
        {
            var subjects = GetThemes()
                .SelectMany(theme => theme.Topics)
                .SelectMany(topic => topic.Publications)
                .SelectMany(publication => publication.Releases)
                .SelectMany(release => release.Subjects);

            return subjects.Where(subject => !IgnoredSubjectFiles.Contains(
                GetSubjectFile(subject.Id))).ToList();
        }

        private static Guid GetSubjectKey(int i)
        {
            return SubjectFiles.ElementAt(i).Key;
        }
        
        private static DataCsvFile GetSubjectFile(Guid key)
        {
            SubjectFiles.TryGetValue(key, out DataCsvFile file);
            return file;
        }
        
        private static IEnumerable<Theme> GetThemes()
        {
            var themes = new List<Theme>
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
                                            Published = new DateTime(2018, 4, 25),
                                            Slug = "2016-17",
                                            Subjects = new List<Subject>
                                            {
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(0),
                                                    Name = "Absence by characteristic"
                                                },
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(1),
                                                    Name = "Absence by geographic level"
                                                },
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(2),
                                                    Name = "Absence by term"
                                                },
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(3),
                                                    Name = "Absence for four year olds"
                                                },
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(4),
                                                    Name = "Absence in prus"
                                                },
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(5),
                                                    Name = "Absence number missing at least one session by reason"
                                                },
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(6),
                                                    Name = "Absence rate percent bands"
                                                }
                                            },
                                            TimeIdentifier = TimeIdentifier.AcademicYear,
                                            Year = 2016
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
                                            Published = new DateTime(2018, 7, 19),
                                            Slug = "2016-17",
                                            Subjects = new List<Subject>
                                            {
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(10),
                                                    Name = "Exclusions by characteristic"
                                                },
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(11),
                                                    Name = "Exclusions by geographic level"
                                                },
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(12),
                                                    Name = "Exclusions by reason"
                                                },
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(13),
                                                    Name = "Duration of fixed exclusions"
                                                },
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(14),
                                                    Name = "Number of fixed exclusions"
                                                },
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(15),
                                                    Name = "Total days missed due to fixed period exclusions"
                                                }
                                            },
                                            TimeIdentifier = TimeIdentifier.AcademicYear,
                                            Year = 2016
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
                                            Published = new DateTime(2019, 4, 29),
                                            Slug = "2018",
                                            Subjects = new List<Subject>
                                            {
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(16),
                                                    Name = "Applications and offers by school phase"
                                                }
                                            },
                                            TimeIdentifier = TimeIdentifier.CalendarYear,
                                            Year = 2018
                                        }
                                    }
                                }
                            }
                        },
                        new Topic
                        {
                            Id = new Guid("85349B0A-19C7-4089-A56B-AD8DBE85449A"),
                            Title = "Special educational needs (SEN)",
                            Slug = "sen",
                            Publications = new[]
                            {
                                new Publication
                                {
                                    Id = new Guid("88312CC0-FE1D-4AB5-81DF-33FD708185CB"),
                                    Title = "Statements of SEN and EHC plans",
                                    Slug = "statements-of-sen-and-ehc-plans",
                                    Releases = new[]
                                    {
                                        new Release
                                        {
                                            Id = new Guid("70efdb76-7e88-453f-95f1-7bb9af023db5"),
                                            Published = new DateTime(2019, 4, 29),
                                            Slug = "2018",
                                            Subjects = new List<Subject>
                                            {
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(17),
                                                    Name = "New cases by age"
                                                },
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(18),
                                                    Name = "Stock cases by age"
                                                },
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(19),
                                                    Name = "New cases by establishment"
                                                },
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(20),
                                                    Name = "Stock cases by establishment"
                                                },
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(21),
                                                    Name = "Management information"
                                                }
                                            },
                                            TimeIdentifier = TimeIdentifier.CalendarYear,
                                            Year = 2018
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
                                            Published = new DateTime(2019, 5, 20),
                                            Slug = "2017-18",
                                            Subjects = new List<Subject>
                                            {
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(7),
                                                    Name = "ELG underlying data 2013 - 2018"
                                                },
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(8),
                                                    Name = "Areas of learning underlying data 2013 - 2018"
                                                },
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(9),
                                                    Name = "APS GLD ELG underlying data 2013 - 2018"
                                                }
                                            },
                                            TimeIdentifier = TimeIdentifier.AcademicYear,
                                            Year = 2017
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                new Theme
                {
                    Id = new Guid("92C5DF93-C4DA-4629-AB25-51BD2920CDCA"),
                    Title = "Further education",
                    Slug = "further-education",
                    Topics = new[]
                    {
                        new Topic
                        {
                            Id = new Guid("DC7B7A89-E968-4A7E-AF5F-BD7D19C346A5"),
                            Title = "National achievement rates tables",
                            Slug = "national-achievement-rates-tables",
                            Publications = new[]
                            {
                                new Publication
                                {
                                    Id = new Guid("7A57D4C0-5233-4D46-8E27-748FBC365715"),
                                    Title = "National achievement rates tables",
                                    Slug = "national-achievement-rates-tables",
                                    Releases = new[]
                                    {
                                        new Release
                                        {
                                            Id = new Guid("59258583-b075-47a2-bee4-5969e2d58873"),
                                            Published = new DateTime(2019, 4, 29),
                                            Slug = "2018",
                                            Subjects = new[]
                                            {
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(22),
                                                    Name = "National achievement rates tables (NARTs)"
                                                }
                                            },
                                            TimeIdentifier = TimeIdentifier.CalendarYear,
                                            Year = 2018
                                        }
                                    }
                                }
                            }
                        },
                        new Topic
                        {
                            Id = new Guid("88D08425-FCFD-4C87-89DA-70B2062A7367"),
                            Title = "Further education and skills",
                            Slug = "further-education-and-skills",
                            Publications = new[]
                            {
                                new Publication
                                {
                                    Id = new Guid("CF0EC981-3583-42A5-B21B-3F2F32008F1B"),
                                    Title = "Apprenticeships and traineeships",
                                    Slug = "apprenticeships-and-traineeships",
                                    Releases = new[]
                                    {
                                        new Release
                                        {
                                            Id = new Guid("463c8521-d9b4-4ccc-aee9-0666e39c8e47"),
                                            Published = new DateTime(2019, 4, 29),
                                            Slug = "2018",
                                            Subjects = new[]
                                            {
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(23),
                                                    Name = "Apprenticeship annual"
                                                }
                                            },
                                            TimeIdentifier = TimeIdentifier.CalendarYear,
                                            Year = 2018
                                        }
                                    }
                                },
                                new Publication
                                {
                                    Id = new Guid("13B81BCB-E8CD-4431-9807-CA588FD1D02A"),
                                    Title = "Further education and skills",
                                    Slug = "further-education-and-skills",
                                    Releases = new[]
                                    {
                                        new Release
                                        {
                                            Id = new Guid("6ccc4416-7d22-46bf-a12a-56037831dc60"),
                                            Published = new DateTime(2019, 4, 29),
                                            Slug = "2018",
                                            Subjects = new[]
                                            {
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(32),
                                                    Name = "Further education and skills"
                                                }
                                            },
                                            TimeIdentifier = TimeIdentifier.CalendarYear,
                                            Year = 2018
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                new Theme
                {
                    Id = new Guid("74648781-85A9-4233-8BE3-FE6F137165F4"),
                    Title = "School and college outcomes and performance",
                    Slug = "outcomes-and-performance",
                    Topics = new[]
                    {
                        new Topic
                        {
                            Id = new Guid("85B5454B-3761-43B1-8E84-BD056A8EFCD3"),
                            Title = "16 to 19 attainment",
                            Slug = "sixteen-to-nineteen-attainment",
                            Publications = new[]
                            {
                                new Publication
                                {
                                    Id = new Guid("2E95F880-629C-417B-981F-0901E97776FF"),
                                    Title = "Level 2 and 3 attainment by young people aged 19",
                                    Slug = "Level 2 and 3 attainment by young people aged 19",
                                    Releases = new[]
                                    {
                                        new Release
                                        {
                                            Id = new Guid("0dafd89b-b754-44a8-b3f1-72baac0a108a"),
                                            Published = new DateTime(2019, 4, 29),
                                            Slug = "2018",
                                            Subjects = new[]
                                            {
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(24),
                                                    Name = "Level 2 and 3 National"
                                                },
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(25),
                                                    Name = "Level 2 and 3 sf"
                                                },
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(26),
                                                    Name = "Level 2 and 3 sf by Local authority"
                                                }
                                            },
                                            TimeIdentifier = TimeIdentifier.CalendarYear,
                                            Year = 2018
                                        }
                                    }
                                }
                            }
                        },
                        new Topic
                        {
                            Id = new Guid("EAC38700-B968-4029-B8AC-0EB8E1356480"),
                            Title = "Key stage 2",
                            Slug = "key-stage-two",
                            Publications = new[]
                            {
                                new Publication
                                {
                                    Id = new Guid("10370062-93B0-4DDE-9097-5A56BF5B3064"),
                                    Title = "National curriculum assessments at key stage 2",
                                    Slug = "national-curriculum-assessments-key-stage2",
                                    Releases = new[]
                                    {
                                        new Release
                                        {
                                            Id = new Guid("dbaeb363-33fa-4928-870f-5054278e0c9a"),
                                            Published = new DateTime(2019, 4, 29),
                                            Slug = "2018",
                                            Subjects = new[]
                                            {
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(27),
                                                    Name = "2016 test data"
                                                }
                                            },
                                            TimeIdentifier = TimeIdentifier.CalendarYear,
                                            Year = 2018
                                        }
                                    }
                                }
                            }
                        },
                        new Topic
                        {
                            Id = new Guid("1E763F55-BF09-4497-B838-7C5B054BA87B"),
                            Title = "GCSEs (key stage 4)",
                            Slug = "key-stage-four",
                            Publications = new[]
                            {
                                new Publication
                                {
                                    Id = new Guid("BFDCAAE1-CE6B-4F63-9B2B-0A1F3942887F"),
                                    Title = "GCSE and equivalent results, including pupil characteristics",
                                    Slug = "gcse-results-including-pupil-characteristics",
                                    Releases = new[]
                                    {
                                        new Release
                                        {
                                            Id = new Guid("737dbab8-4e62-4d56-b0d6-5b4602a20801"),
                                            Published = new DateTime(2019, 4, 29),
                                            Slug = "2018",
                                            Subjects = new[]
                                            {
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(28),
                                                    Name = "Characteristic test data by Local authority"
                                                },
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(29),
                                                    Name = "National characteristic test data"
                                                },
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(30),
                                                    Name = "Subject tables S1 test data"
                                                },
                                                new Subject
                                                {
                                                    Id = GetSubjectKey(31),
                                                    Name = "Subject tables S3 test data"
                                                }
                                            },
                                            TimeIdentifier = TimeIdentifier.CalendarYear,
                                            Year = 2018
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            foreach (var theme in themes)
            {
                foreach (var topic in theme.Topics)
                {
                    topic.Theme = theme;
                    topic.ThemeId = theme.Id;

                    foreach (var publication in topic.Publications)
                    {
                        publication.Topic = topic;
                        publication.TopicId = topic.Id;

                        foreach (var release in publication.Releases)
                        {
                            release.Publication = publication;
                            release.PublicationId = publication.Id;

                            foreach (var subject in release.Subjects)
                            {
                                subject.Release = release;
                                subject.ReleaseId = release.Id;
                            }
                        }
                    }
                }
            }

            return themes;
        }
    }
}