using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Seed.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Seed
{
    public static class SamplePublications
    {
        public static readonly Dictionary<string, Publication> Publications = new Dictionary<string, Publication>
        {
            {
                "absence", new Publication
                {
                    PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                    Name = "Pupil absence data and statistics for schools in England",
                    Releases = new[]
                    {
                        new Release
                        {
                            PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                            ReleaseDate = new DateTime(2018, 4, 25),
                            Name = "2016/17",
                            Subjects = new[]
                            {
                                new Subject
                                {
                                    Filename = DataCsvFilename.absence_by_characteristic,
                                    Name = "Absence by characteristic"
                                },
                                new Subject
                                {
                                    Filename = DataCsvFilename.absence_by_geographic_level,
                                    Name = "Absence by geographic level"
                                },
                                new Subject
                                {
                                    Filename = DataCsvFilename.absence_by_term,
                                    Name = "Absence by term"
                                },
                                new Subject
                                {
                                    Filename = DataCsvFilename.absence_for_four_year_olds,
                                    Name = "Absence for four year olds"
                                },
                                new Subject
                                {
                                    Filename = DataCsvFilename.absence_in_prus,
                                    Name = "Absence in prus"
                                },
                                new Subject
                                {
                                    Filename = DataCsvFilename.absence_number_missing_at_least_one_session_by_reason,
                                    Name = "Absence number missing at least one session by reason"
                                },
                                new Subject
                                {
                                    Filename = DataCsvFilename.absence_rate_percent_bands,
                                    Name = "Absence rate percent bands"
                                }
                            }
                        }
                    }
                }
            },
            {
                "exclusions", new Publication
                {
                    PublicationId = new Guid("8345e27a-7a32-4b20-a056-309163bdf9c4"),
                    Name = "Permanent and fixed period exclusions",
                    Releases = new[]
                    {
                        new Release
                        {
                            PublicationId = new Guid("8345e27a-7a32-4b20-a056-309163bdf9c4"),
                            ReleaseDate = new DateTime(2018, 7, 19),
                            Name = "2016/17",
                            Subjects = new[]
                            {
                                new Subject
                                {
                                    Filename = DataCsvFilename.exclusions_by_characteristic,
                                    Name = "Exclusions by characteristic"
                                },
                                new Subject
                                {
                                    Filename = DataCsvFilename.exclusions_by_geographic_level,
                                    Name = "Exclusions by geographic level"
                                },
                                new Subject
                                {
                                    Filename = DataCsvFilename.exclusions_by_reason,
                                    Name = "Exclusions by reason"
                                },
                                new Subject
                                {
                                    Filename = DataCsvFilename.exclusions_duration_of_fixed_exclusions,
                                    Name = "Duration of fixed exclusions"
                                },
                                new Subject
                                {
                                    Filename = DataCsvFilename.exclusions_number_of_fixed_exclusions,
                                    Name = "Number of fixed exclusions"
                                },
                                new Subject
                                {
                                    Filename = DataCsvFilename.exclusions_total_days_missed_fixed_exclusions,
                                    Name = "Total days missed due to fixed period exclusions"
                                }
                            }
                        }
                    }
                }
            },
            {
                "school_applications_and_offers", new Publication
                {
                    PublicationId = new Guid("66c8e9db-8bf2-4b0b-b094-cfab25c20b05"),
                    Name = "School applications and offers",
                    Releases = new[]
                    {
                        new Release
                        {
                            PublicationId = new Guid("66c8e9db-8bf2-4b0b-b094-cfab25c20b05"),
                            ReleaseDate = new DateTime(2019, 4, 29),
                            Name = "2018",
                            Subjects = new[]
                            {
                                new Subject
                                {
                                    Filename = DataCsvFilename.school_applications_and_offers,
                                    Name = "Applications and offers by school phase"
                                }
                            }
                        }
                    }
                }
            }
        };
    }
}