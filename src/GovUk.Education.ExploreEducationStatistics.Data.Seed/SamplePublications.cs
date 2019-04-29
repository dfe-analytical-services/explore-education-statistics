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
                    Name = "Pupil absence statistics and data for schools in England",
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
            }
        };
    }
}