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
                {17, DataCsvFile.school_applications_and_offers}
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
                                Title = "Pupil absence data and statistics for schools in England",
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
                                Title = "Early years foundation stage profile data",
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
            }
        };
    }
}