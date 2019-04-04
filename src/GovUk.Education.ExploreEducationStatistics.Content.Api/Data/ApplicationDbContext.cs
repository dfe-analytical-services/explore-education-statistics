using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Theme> Themes { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Publication> Publications { get; set; }
        public DbSet<Release> Releases { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Release>()
                .Property(b => b.Content)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<ContentSection>>(v));

            modelBuilder.Entity<Release>()
                .Property(b => b.KeyStatistics)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<DataBlock>(v));

            modelBuilder.Entity<Theme>().HasData(
                new Theme
                {
                    Id = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), Title = "Early years and schools",
                    Summary = "Lorem ipsum dolor sit amet.", Slug = "early-years-and-schools"
                },
                new Theme
                {
                    Id = new Guid("6412a76c-cf15-424f-8ebc-3a530132b1b3"), Title = "Social Care",
                    Summary = "Lorem ipsum dolor sit amet.", Slug = "social-care"
                },
                new Theme
                {
                    Id = new Guid("bc08839f-2970-4f34-af2d-29608a48082f"), Title = "Higher education",
                    Summary = "Lorem ipsum dolor sit amet.", Slug = "higher-education"
                }
            );

            modelBuilder.Entity<Topic>().HasData(
                new Topic
                {
                    Id = new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"), Title = "Absence and exclusions",
                    Summary = "Pupil absence and permanent and fixed-period exclusions statistics and data",
                    ThemeId = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), Slug = "absence-and-exclusions"
                },
                new Topic
                {
                    Id = new Guid("22c52d89-88c0-44b5-96c4-042f1bde6ddd"), Title = "School & pupil numbers",
                    Summary = "Schools, pupils and their characteristics, SEN and EHC plans, SEN in England",
                    ThemeId = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), Slug = "school-and-pupil-numbers"
                },
                new Topic
                {
                    Id = new Guid("734820b7-f80e-45c3-bb92-960edcc6faa5"), Title = "Capacity and admissions",
                    Summary = "School capacity, admission appeals",
                    ThemeId = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), Slug = "capacity-admissions"
                },
                new Topic
                {
                    Id = new Guid("17b2e32c-ed2f-4896-852b-513cdf466769"), Title = "Results",
                    Summary = "Local authority and school finance",
                    ThemeId = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), Slug = "results"
                },
                new Topic
                {
                    Id = new Guid("66ff5e67-36cf-4210-9ad2-632baeb4eca7"), Title = "School finance",
                    Summary = "Local authority and school finance",
                    ThemeId = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), Slug = "school-finance"
                },
                new Topic
                {
                    Id = new Guid("d5288137-e703-43a1-b634-d50fc9785cb9"), Title = "Teacher Numbers",
                    Summary = "The number and characteristics of teachers",
                    ThemeId = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), Slug = "teacher-numbers"
                },
                new Topic
                {
                    Id = new Guid("0b920c62-ff67-4cf1-89ec-0c74a364e6b4"), Title = "Number of Children",
                    Summary = "Lorem ipsum dolor sit amet.", ThemeId = new Guid("6412a76c-cf15-424f-8ebc-3a530132b1b3"),
                    Slug = "number-of-children"
                },
                new Topic
                {
                    Id = new Guid("3bef5b2b-76a1-4be1-83b1-a3269245c610"), Title = "Vulnerable Children",
                    Summary = "Lorem ipsum dolor sit amet.", ThemeId = new Guid("6412a76c-cf15-424f-8ebc-3a530132b1b3"),
                    Slug = "vulnerable-children"
                },
                new Topic
                {
                    Id = new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860"), Title = "Further Education",
                    Summary = "Lorem ipsum dolor sit amet.", ThemeId = new Guid("bc08839f-2970-4f34-af2d-29608a48082f"),
                    Slug = "further-education"
                },
                new Topic
                {
                    Id = new Guid("4c658598-450b-4493-b972-8812acd154a7"), Title = "Higher Education",
                    Summary = "Lorem ipsum dolor sit amet.", ThemeId = new Guid("bc08839f-2970-4f34-af2d-29608a48082f"),
                    Slug = "higher-education"
                }
            );

            modelBuilder.Entity<Publication>().HasData(
                // Absence and exclusions
                new Publication
                {
                    Id = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                    Title = "Pupil absence in schools in England",
                    Summary =
                        "View statistics, create charts and tables and download data files for authorised, overall, persistent and unauthorised absence",
                    TopicId = new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"),
                    Slug = "pupil-absence-in-schools-in-england",
                    NextUpdate = new DateTime(2018, 3, 22),
                    DataSource =
                        "[Pupil absence statistics: guide](https://www.gov.uk/government/publications/absence-statistics-guide#)"
                },
                new Publication
                {
                    Id = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                    Title = "Permanent and fixed period exclusions",
                    Summary =
                        "View statistics, create charts and tables and download data files for fixed-period and permanent exclusion statistics",
                    TopicId = new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"),
                    Slug = "permanent-and-fixed-period-exclusions"
                },

                // School and pupil numbers
                new Publication
                {
                    Id = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                    Title = "Schools, pupils and their characteristics",
                    Summary = "Statistics on the number and characteristics of schools and pupils.",
                    TopicId = new Guid("22c52d89-88c0-44b5-96c4-042f1bde6ddd"),
                    Slug = "schools-pupils-and-their-characteristics"
                },

                // Capacity Admissions
                new Publication
                {
                    Id = new Guid("d04142bd-f448-456b-97bc-03863143836b"), Title = "School capacity",
                    Summary = "Lorem ipsum dolor sit amet.", TopicId = new Guid("734820b7-f80e-45c3-bb92-960edcc6faa5"),
                    Slug = "school-capacity"
                },
                new Publication
                {
                    Id = new Guid("a20ea465-d2d0-4fc1-96ee-6b2ca4e0520e"), Title = "Admission appeals in England",
                    Summary = "Lorem ipsum dolor sit amet.", TopicId = new Guid("734820b7-f80e-45c3-bb92-960edcc6faa5"),
                    Slug = "admission-appeals-in-England"
                },

                // Results
                new Publication
                {
                    Id = new Guid("526dea0e-abf3-476e-9ca4-9dbd9b101bc8"),
                    Title = "Early years foundation stage profile results", Summary = "Lorem ipsum dolor sit amet.",
                    TopicId = new Guid("17b2e32c-ed2f-4896-852b-513cdf466769"),
                    Slug = "early-years-foundation-stage-profile-results"
                },
                new Publication
                {
                    Id = new Guid("9674ac24-649a-400c-8a2c-871793d9cd7a"),
                    Title = "Phonics screening check and KS1 assessments", Summary = "Lorem ipsum dolor sit amet.",
                    TopicId = new Guid("17b2e32c-ed2f-4896-852b-513cdf466769"),
                    Slug = "phonics-screening-check-and-ks1-assessments"
                },
                new Publication
                {
                    Id = new Guid("a4b22113-47d3-48fc-b2da-5336c801a31f"), Title = "KS2 statistics",
                    Summary = "Lorem ipsum dolor sit amet.", TopicId = new Guid("17b2e32c-ed2f-4896-852b-513cdf466769"),
                    Slug = "ks2-statistics"
                },
                new Publication
                {
                    Id = new Guid("bfdcaae1-ce6b-4f63-9b2b-0a1f3942887f"), Title = "KS4 statistics",
                    Summary = "Lorem ipsum dolor sit amet.", TopicId = new Guid("17b2e32c-ed2f-4896-852b-513cdf466769"),
                    Slug = "ks4-statistics"
                },

                // Teacher Numbers
                new Publication
                {
                    Id = new Guid("8b2c1269-3495-4f89-83eb-524fc0b6effc"), Title = "School workforce",
                    Summary = "Lorem ipsum dolor sit amet.", TopicId = new Guid("d5288137-e703-43a1-b634-d50fc9785cb9"),
                    Slug = "school-workforce"
                },
                new Publication
                {
                    Id = new Guid("fe94b33d-0419-4fac-bf73-28299d5e4247"),
                    Title = "Initial teacher training performance profiles", Summary = "Lorem ipsum dolor sit amet.",
                    TopicId = new Guid("d5288137-e703-43a1-b634-d50fc9785cb9"),
                    Slug = "initial-teacher-training-performance-profiles"
                },

                // Number of Children
                new Publication
                {
                    Id = new Guid("bd781dc5-cfc7-4543-b8d7-a3a7b3606b3d"), Title = "Children in need",
                    Summary = "Lorem ipsum dolor sit amet.", TopicId = new Guid("0b920c62-ff67-4cf1-89ec-0c74a364e6b4"),
                    Slug = "children-in-need"
                },
                new Publication
                {
                    Id = new Guid("143c672b-18d7-478b-a6e7-b843c9b3fd42"), Title = "Looked after children",
                    Summary = "Lorem ipsum dolor sit amet.", TopicId = new Guid("0b920c62-ff67-4cf1-89ec-0c74a364e6b4"),
                    Slug = "looked-after-children"
                },

                // Further Education 
                new Publication
                {
                    Id = new Guid("70902b3c-0bb4-457d-b40a-2a959cdc7d00"), Title = "16 to 18 school performance",
                    Summary = "Lorem ipsum dolor sit amet.", TopicId = new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860"),
                    Slug = "16-to-18-school-performance"
                },
                new Publication
                {
                    Id = new Guid("d0e56978-c944-4b12-9156-bfe50c94c2a0"), Title = "Destination of leavers",
                    Summary = "Lorem ipsum dolor sit amet.", TopicId = new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860"),
                    Slug = "destination-of-leavers"
                },
                new Publication
                {
                    Id = new Guid("ad81ebdd-2bbc-47e8-a32c-f396d6e2bb72"), Title = "Further education and skills",
                    Summary = "Lorem ipsum dolor sit amet.", TopicId = new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860"),
                    Slug = "further-education-and-skills"
                },
                new Publication
                {
                    Id = new Guid("201cb72d-ef35-4680-ade7-b09a8dca9cc1"), Title = "Apprenticeship and levy statistics",
                    Summary = "Lorem ipsum dolor sit amet.", TopicId = new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860"),
                    Slug = "apprenticeship-and-levy-statistics"
                },
                new Publication
                {
                    Id = new Guid("7bd128a3-ae7f-4e1b-984e-d1b795c61630"), Title = "Apprenticeships and traineeships",
                    Summary = "Lorem ipsum dolor sit amet.", TopicId = new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860"),
                    Slug = "apprenticeships-and-traineeships"
                }
            );

            modelBuilder.Entity<Release>().HasData(
                //absence
                new Release
                {
                    Id = new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                    Title = "Pupil absence data and statistics for schools in England",
                    ReleaseName = "2016 to 2017",
                    PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                    Published = new DateTime(2017, 3, 22),
                    Slug = "2016-17",
                    Summary =
                        "Read national statistical summaries and definitions, view charts and tables and download data files across a range of pupil absence subject areas.",

                    KeyStatistics = new DataBlock
                    {
                        Heading = "Latest headline facts and figures - 2016 to 2017",

                        DataQuery = new DataQuery
                        {
                            method = "POST",
                            path = "/api/tablebuilder/characteristics/national",
                            body =
                                "{ \"indicators\": [\"enrolments\",\"sess_authorised\",\"sess_overall\",\"enrolments_PA_10_exact\",\"sess_unauthorised_percent\",\"enrolments_pa_10_exact_percent\",\"sess_authorised_percent\",\"sess_overall_percent\" ], \"characteristics\": [ \"Total\" ], \"endYear\": 201617, \"publicationId\": \"cbbd299f-8297-44bc-92ac-558bcf51f8ad\", \"schoolTypes\": [ \"Total\" ], \"startYear\": 201213}"
                        },

                        Summary = new Summary
                        {
                            dataKeys = new List<string>
                            {
                                "sess_overall_percent",
                                "sess_authorised_percent",
                                "sess_unauthorised_percent"
                            },

                            description = new MarkDownBlock
                            {
                                Body =
                                    " * pupils missed on average 8.2 school days \n * overall and unauthorised absence rates up on previous year \n * unauthorised rise due to higher rates of unauthorised holidays \n * 10% of pupils persistently absent during 2016/17"
                            }
                        },

                        Charts = new List<IContentBlockChart>
                        {
                            new LineChart
                            {
                                XAxis = new Axis
                                {
                                    title = "School Year"
                                },
                                YAxis = new Axis
                                {
                                    title = "Absence Rate"
                                },
                                Indicators = new List<string>
                                {
                                    "sess_overall_percent",
                                    "sess_unauthorised_percent",
                                    "sess_authorised_percent"
                                },
                            }
                        }
                    },

                    Content = new List<ContentSection>
                    {
                        new ContentSection
                        {
                            Order = 1, Heading = "About this release", Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body =
                                        "This statistical first release (SFR) reports on absence of pupils of compulsory school age in state-funded primary, secondary and special schools during the 2016/17 academic year. Information on absence in pupil referral units, and for pupils aged four, is also included. The Department uses two key measures to monitor pupil absence – overall and persistent absence. Absence by reason and pupils characteristics is also included in this release. Figures are available at national, regional, local authority and school level. Figures held in this release are used for policy development as key indicators in behaviour and school attendance policy. Schools and local authorities also use the statistics to compare their local absence rates to regional and national averages for different pupil groups."
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 2, Heading = "Absence rates", Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body =
                                        "The overall absence rate across state-funded primary, secondary and special schools increased from 4.6 per cent in 2015/16 to 4.7 per cent in 2016/17. In primary schools the overall absence rate stayed the same at 4 per cent and the rate in secondary schools increased from 5.2 per cent to 5.4 per cent. Absence in special schools is much higher at 9.7 per cent in 2016/17\n\nThe increase in overall absence rate has been driven by an increase in the unauthorised absence rate across state-funded primary, secondary and special schools - which increased from 1.1 per cent to 1.3 per cent between 2015/16 and 2016/17.\n\nLooking at longer-term trends, overall and authorised absence rates have been fairly stable over recent years after decreasing gradually between 2006/07 and 2013/14. Unauthorised absence rates have not varied much since 2006/07, however the unauthorised absence rate is now at its highest since records began, at 1.3 per cent.\n\nThis increase in unauthorised absence is due to an increase in absence due to family holidays that were not agreed by the school. The authorised absence rate has not changed since last year, at 3.4 per cent. Though in primary schools authorised absence rates have been decreasing across recent years.\n\nThe total number of days missed due to overall absence across state-funded primary, secondary and special schools has increased since last year, from 54.8 million in 2015/16 to 56.7 million in 2016/17. This partly reflects the rise in the total number of pupil enrolments, the average number of days missed per enrolment has increased very slightly from 8.1 days in 2015/16 to 8.2 days in 2016/17.\n\nIn 2016/17, 91.8 per cent of pupils in state-funded primary, state-funded secondary and special schools missed at least one session during the school year, this is similar to the previous year (91.7 per cent in 2015/16)."
                                },
                                new DataBlock
                                {
                                    Heading = null,
                                    DataQuery = new DataQuery
                                    {
                                        method = "POST",
                                        path = "/api/tablebuilder/characteristics/national",
                                        body =
                                            "{ \"indicators\": [ \"num_schools\", \"enrolments\", \"sess_overall_percent\", \"sess_unauthorised_percent\", \"sess_authorised_percent\" ], \"characteristics\": [ \"Total\" ], \"endYear\": 201617, \"publicationId\": \"cbbd299f-8297-44bc-92ac-558bcf51f8ad\", \"schoolTypes\": [ \"Total\" ], \"startYear\": 201213}"
                                    },
                                    Charts = new List<IContentBlockChart>
                                    {
                                        new LineChart
                                        {
                                            XAxis = new Axis
                                            {
                                                title = "School Year"
                                            },
                                            YAxis = new Axis
                                            {
                                                title = "Absence Rate"
                                            },
                                            Indicators = new List<string>
                                            {
                                                "sess_overall_percent",
                                                "sess_unauthorised_percent",
                                                "sess_authorised_percent"
                                            },
                                        }
                                    }
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 3, Heading = "Persistent absence", Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body =
                                        "The percentage of enrolments in state-funded primary and state-funded secondary schools that were classified as persistent absentees in 2016/17 was 10.8 per cent. This is up from the equivalent figure of 10.5 per cent in 2015/16 (see Figure 2).\n\nIn 2016/17, persistent absentees accounted for 37.6 per cent of all absence compared to 36.6 per cent in 2015/16. Longer term, there has been a decrease in the proportion of absence that persistent absentees account for – down from 43.3 per cent in 2011/12.\n\nThe overall absence rate for persistent absentees across all schools was 18.1 per cent, nearly four times higher than the rate for all pupils. This is a slight increase from 2015/16, when the overall absence rate for persistent absentees was 17.6 per cent.\n\nPersistent absentees account for almost a third, 31.6 per cent, of all authorised absence and more than half, 53.8 per cent of all unauthorised absence. The rate of illness absences is almost four times higher for persistent absentees compared to other pupils, at 7.6 per cent and 2.0 per cent respectively."
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 4, Heading = "Reasons for absence", Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new InsetTextBlock
                                {
                                    Body =
                                        "Within this release absence by reason is broken down in three different ways:\n\nDistribution of absence by reason: The proportion of absence for each reason, calculated by taking the number of absences for a specific reason as a percentage of the total number of absences.\n\nRate of absence by reason: The rate of absence for each reason, calculated by taking the number of absences for a specific reason as a percentage of the total number of possible sessions.\n\nOne or more sessions missed due to each reason: The number of pupil enrolments missing at least one session due to each reason."
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 5, Heading = "Distribution of absence", Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body =
                                        "Nearly half of all pupils (48.9 per cent) were absent for five days or fewer across state-funded primary, secondary and special schools in 2016/17, down from 49.1 per cent in 2015/16.\n\n4.3 per cent of pupil enrolments had more than 25 days of absence in 2016/17 (the same as in 2015/16). These pupil enrolments accounted for 23.5 per cent of days missed. 8.2 per cent of pupil enrolments had no absence during 2016/17.\n\nPer pupil enrolment, the average total absence in primary schools was 7.2 days, compared to 16.9 days in special schools and 9.3 days in secondary schools.\n\nWhen looking at absence rates across terms for primary, secondary and special schools, the overall absence rate is lowest in the autumn term and highest in the summer term. The authorised rate is highest in the spring term and lowest in the summer term, and the unauthorised rate is highest in the summer term."
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 6, Heading = "Absence by pupil characteristics", Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body =
                                        "The patterns of absence rates for pupils with different characteristics have been consistent across recent years.\n\n### Gender\n\nThe overall absence rates across state-funded primary, secondary and special schools were very similar for boys and girls, at 4.7 per cent and 4.6 per cent respectively. The persistent absence rates were also similar, at 10.9 per cent for boys and 10.6 per cent for girls.\n\n### Free school meals (FSM) eligibility\n\nAbsence rates are higher for pupils who are known to be eligible for and claiming free school meals. The overall absence rate for these pupils was 7.3 per cent, compared to 4.2 per cent for non FSM pupils. The persistent absence rate for pupils who were eligible for FSM was more than twice the rate for those pupils not eligible for FSM.\n\n### National curriculum year group\n\nPupils in national curriculum year groups 3 and 4 had the lowest overall absence rates at 3.9 and 4 per cent respectively. Pupils in national curriculum year groups 10 and 11 had the highest overall absence rate at 6.1 per cent and 6.2 per cent respectively. This trend is repeated for persistent absence.\n\n### Special educational need (SEN)\n\nPupils with a statement of special educational needs (SEN) or education healthcare plan (EHC) had an overall absence rate of 8.2 per cent compared to 4.3 per cent for those with no identified SEN. The percentage of pupils with a statement of SEN or an EHC plan that are persistent absentees was more than two times higher than the percentage for pupils with no identified SEN.\n\n### Ethnic group\n\nThe highest overall absence rates were for Traveller of Irish Heritage and Gypsy/ Roma pupils at 18.1 per cent and 12.9 per cent respectively. Overall absence rates for pupils of a Chinese and Black African ethnicity were substantially lower than the national average of 4.7 per cent at 2.4 per cent and 2.9 per cent respectively. A similar pattern is seen in persistent absence rates; Traveller of Irish heritage pupils had the highest rate at 64 per cent and Chinese pupils had the lowest rate at 3.1 per cent."
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 7, Heading = "Absence for four year olds", Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body =
                                        "The overall absence rate for four year olds in 2016/17 was 5.1 per cent which is lower than the rate of 5.2 per cent which it has been for the last two years.\n\nAbsence recorded for four year olds is not treated as 'authorised' or 'unauthorised' and is therefore reported as overall absence only."
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 8, Heading = "Pupil referral unit absence", Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body =
                                        "The overall absence rate for pupil referral units in 2016/17 was 33.9 per cent, compared to 32.6 per cent in 2015/16. The percentage of enrolments in pupil referral units who were persistent absentees was 73.9 per cent in 2016/17, compared to 72.5 per cent in 2015/16."
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 9, Heading = "Pupil absence by local authority", Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body =
                                        "There is variation in overall and persistent absence rates across state-funded primary, secondary and special schools by region and local authority. Similarly to last year, the three regions with the highest overall absence rate across all state-funded primary, secondary and special schools are the North East (4.9 per cent), Yorkshire and the Humber (4.9 per cent) and the South West (4.8 per cent), with Inner and Outer London having the lowest overall absence rate (4.4 per cent). The region with the highest persistent absence rate is Yorkshire and the Humber, where 11.9 per cent of pupil enrolments are persistent absentees, with Outer London having the lowest rate of persistent absence (at 10.0 per cent).\n\nAbsence information at local authority district level is also published within this release, in the accompanying underlying data files."
                                }
                            }
                        }
                    }
                },
                new Release
                {
                    Id = new Guid("f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d"),
                    Title = "Pupil absence data and statistics for schools in England",
                    ReleaseName = "2015 to 2016",
                    PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                    Published = new DateTime(2016, 3, 25),
                    Slug = "2015-16",
                    Summary =
                        "Read national statistical summaries and definitions, view charts and tables and download data files across a range of pupil absence subject areas.",

                    KeyStatistics = new DataBlock
                    {
                        Heading = "Latest headline facts and figures - 2016 to 2017",

                        Summary = new Summary
                        {
                            dataKeys = new List<string>
                            {
                                "--",
                                "--",
                                "--"
                            },

                            description = new MarkDownBlock
                            {
                                Body = ""
                            }
                        }
                    },

                    Content = new List<ContentSection>
                    {
                        new ContentSection {Order = 1, Heading = "About this release", Caption = ""},
                        new ContentSection {Order = 2, Heading = "Absence rates", Caption = ""},
                        new ContentSection {Order = 3, Heading = "Persistent absence", Caption = ""},
                        new ContentSection {Order = 4, Heading = "Distribution of absence", Caption = ""},
                        new ContentSection {Order = 5, Heading = "Absence for four year olds", Caption = ""},
                        new ContentSection {Order = 6, Heading = "Pupil referral unit absence", Caption = ""},
                        new ContentSection {Order = 7, Heading = "Pupil absence by local authority", Caption = ""}
                    }
                },

                // exclusions
                new Release
                {
                    Id = new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                    Title = "Permanent and fixed period exclusions",
                    ReleaseName = "2016 to 2017",
                    PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                    Published = new DateTime(2018, 7, 19),
                    Slug = "2016-17",
                    Summary =
                        "Read national statistical summaries and definitions, view charts and tables and download data files across a range of permanent and fixed-period exclusion subject areas.",
                    KeyStatistics = new DataBlock
                    {
                        Heading = "Latest headline facts and figures - 2016 to 2017",

                        Summary = new Summary
                        {
                            dataKeys = new List<string>
                            {
                                "--",
                                "--",
                                "--"
                            },

                            description = new MarkDownBlock
                            {
                                Body = ""
                            }
                        }
                    },
                    Content = new List<ContentSection>
                    {
                        new ContentSection
                        {
                            Order = 1, Heading = "About this release", Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body =
                                        "This National Statistics release reports on permanent and fixed period exclusions from state-funded primary, state-funded secondary and special schools during the 2016/17 academic year as reported in the School Census. This release also includes school level exclusions figures for state-funded primary, secondary and special schools and national level figures on permanent and fixed-period exclusions from pupil referral units. All figures in this release are based on unrounded data; therefore, constituent parts may not add up due to rounding.\n\nAn Exclusions statistics guide, which provides historical information on exclusion statistics, technical background information to the figures and data collection, and definitions of key terms should be referenced alongside this release.\n\nIn this publication: The following tables are included in the statistical publication\n\n*   national tables (Excel .xls and open format)\n\n*   local authority (LA) tables\n\n*   underlying data (open format .csv and metadata .txt)\n\nThe underlying data is accompanied by a metadata document that describes underlying data files.\n\nWe welcome feedback on any aspect of this document at [schools.statistics@education.gov.uk](#)"
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 2, Heading = "Permanent exclusions", Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new InsetTextBlock
                                {
                                    Heading = "Permanent exclusion rate definition",
                                    Body =
                                        "A permanent exclusion refers to a pupil who is excluded and who will not come back to that school (unless the exclusion is overturned). The number of permanent exclusions across all state-funded primary, secondary and special schools has increased from 6,685 in 2015/16 to 7,720 in 2016/17. This corresponds to around 40.6 permanent exclusions per day in 2016/17, up from an average of 35.2 per day in 2015/16."
                                },
                                new MarkDownBlock
                                {
                                    Body =
                                        "The rate of permanent exclusions across all state-funded primary, secondary and special schools has also increased from 0.08 per cent to 0.10 per cent of pupil enrolments, which is equivalent to around 10 pupils per 10,000.\n\nMost (83 per cent) permanent exclusions occurred in secondary schools. The rate of permanent exclusions in secondary schools increased from 0.17 per cent in 2015/16 to 0.20 per cent in 2016/17, which is equivalent to around 20 pupils per 10,000.\n\nThe rate of permanent exclusions also rose in primary schools, at 0.03 per cent, but decreased in special schools from 0.08 per cent in 2015/16 to 0.07 per cent in 2016/17. Looking at longer-term trends, the rate of permanent exclusions across all state-funded primary, secondary and special schools followed a generally downward trend from 2006/07 when the rate was 0.12 per cent until 2012/13, and has been rising again since then, although rates are still lower now than in 2006/07."
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 3, Heading = "Fixed-period exclusions", Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new InsetTextBlock
                                {
                                    Heading = "Fixed-period exclusion rate definition",
                                    Body =
                                        "Fixed-period exclusion refers to a pupil who is excluded from a school for a set period of time. A fixed-period exclusion can involve a part of the school day and it does not have to be for a continuous period. A pupil may be excluded for one or more fixed periods up to a maximum of 45 school days in a single academic year. This total includes exclusions from previous schools covered by the exclusion legislation. A pupil may receive more than one fixed-period exclusion, so pupils with repeat exclusions can inflate fixed-period exclusion rates."
                                },
                                new MarkDownBlock
                                {
                                    Body =
                                        "The number of fixed-period exclusions across all state-funded primary, secondary and special schools has increased from 339,360 in 2015/16 to 381,865 in 2016/17. This corresponds to around 2,010 fixed-period exclusions per day1 in 2016/17, up from an average of 1,786 per day in 2015/16.\n\nThere were increases in the number and rate of fixed-period exclusions for state-funded primary and secondary schools and special schools:"
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 4, Heading = "Number and length of fixed-period exclusions", Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new InsetTextBlock
                                {
                                    Heading = "Enrolments with one or more fixed-period exclusion definition",
                                    Body =
                                        "Pupils with one or more fixed-period exclusion refer to pupil enrolments that had at least one fixed-period exclusion across the full academic year. It includes those with repeated fixed-period exclusions."
                                },
                                new MarkDownBlock
                                {
                                    Body =
                                        "In state-funded primary, secondary and special schools, there were 183,475 pupil enrolments, 2.29 per cent, with at least one fixed term exclusion in 2016/17, up from 167,125 pupil enrolments, 2.11 per cent, in 2015/16.\n\nOf those pupils with at least one fixed-period exclusion, 59.1 per cent were excluded only on one occasion, and 1.5 per cent received 10 or more fixed-period exclusions during the year. The percentage of pupils with at least one fixed-period exclusion that went on to receive a permanent one was 3.5 per cent.\n\nThe average length of fixed-period exclusions across state-funded primary, secondary and special schools in 2016/17 was 2.1 days, slightly shorter than in 2015/16.\n\nThe highest proportion of fixed-period exclusions (46.6 per cent) lasted for only one day. Only 2.0 per cent of fixed-period exclusions lasted for longer than one week and longer fixed-period exclusions were more prevalent in secondary schools."
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 5, Heading = "Reasons for exclusions", Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body =
                                        "Persistent disruptive behaviour remained the most common reason for permanent exclusions in state-funded primary, secondary and special schools - accounting for 2,755 (35.7 per cent) of all permanent exclusions in 2016/17. This is equivalent to 3 permanent exclusions per 10,000 pupils. However, in special schools alone, the most common reason for exclusion was physical assault against and adult, which made up 37.8 per cent of all permanent exclusions and 28.1 per cent of all fixed-period exclusions.\n\nAll reasons except bullying and theft saw an increase in permanent exclusions since last year. The most common reasons - persistent disruptive behaviour, physical assault against a pupil and other reasons had the largest increases.\n\nPersistent disruptive behaviour is also the most common reason for fixed-period exclusions. The 108,640 fixed-period exclusions for persistent disruptive behaviour in state-funded primary, secondary and special schools made up 28.4 per cent of all fixed-period exclusions, up from 27.7 per cent in 2015/16. This is equivalent to around 135 fixed-period exclusions per 10,000 pupils.\n\nAll reasons saw an increase in fixed-period exclusions since last year. Persistent disruptive behaviour and other reasons saw the biggest increases."
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 6, Heading = "Exclusions by pupil characteristics", Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body =
                                        "In 2016/17 we saw a similar pattern by pupil characteristics to previous years. The groups that we usually expect to have higher rates are the ones that have increased exclusions since last year e.g. boys, pupils with special educational needs, pupils known to be eligible for and claiming free school meals and national curriculum years 9 and 10.\n\n**Age, national curriculum year group and gender**\n\n*   Over half of all permanent (57.2 per cent) and fixed-period (52.6 per cent) exclusions occur in national curriculum year 9 or above.\n\n*   A quarter (25.0 per cent) of all permanent exclusions were for pupils aged 14, and pupils of this age group also had the highest rate of fixed-period exclusion, and the highest rate of pupils receiving one or more fixed-period exclusion.\n\n*   The permanent exclusion rate for boys (0.15 per cent) was over three times higher than that for girls (0.04 per cent) and the fixed-period exclusion rate was almost three times higher (6.91 compared with 2.53 per cent).\n\n**Free school meals (FSM) eligibility**\n\n*   Pupils known to be eligible for and claiming free school meals (FSM) had a permanent exclusion rate of 0.28 per cent and fixed period exclusion rate of 12.54 per cent - around four times higher than those who are not eligible (0.07 and 3.50 per cent respectively).\n\n*   Pupils known to be eligible for and claiming free school meals (FSM) accounted for 40.0 per cent of all permanent exclusions and 36.7 per cent of all fixed-period exclusions. Special educational need (SEN)\n\n*   Pupils with identified special educational needs (SEN) accounted for around half of all permanent exclusions (46.7 per cent) and fixed-period exclusions (44.9 per cent).\n\n*   Pupils with SEN support had the highest permanent exclusion rate at 0.35 per cent. This was six times higher than the rate for pupils with no SEN (0.06 per cent).\n\n*   Pupils with an Education, Health and Care (EHC) plan or with a statement of SEN had the highest fixed-period exclusion rate at 15.93 per cent - over five times higher than pupils with no SEN (3.06 per cent).\n\n**Ethnic group**\n\n*   Pupils of Gypsy/Roma and Traveller of Irish Heritage ethnic groups had the highest rates of both permanent and fixed-period exclusions, but as the population is relatively small these figures should be treated with some caution.\n\n*   Black Caribbean pupils had a permanent exclusion rate nearly three times higher (0.28 per cent) than the school population as a whole (0.10 per cent). Pupils of Asian ethnic groups had the lowest rates of permanent and fixed-period exclusion."
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 7, Heading = "Independent exclusion reviews", Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body =
                                        "Independent review Panel definition: Parents (and pupils if aged over 18) are able to request a review of a permanent exclusion. An independent review panel’s role is to review the decision of the governing body not to reinstate a permanently excluded pupil. The panel must consider the interests and circumstances of the excluded pupil, including the circumstances in which the pupil was excluded and have regard to the interests of other pupils and people working at the school.\n\nIn 2016/17 in maintained primary, secondary and special schools and academies there were 560 reviews lodged with independent review panels of which 525 (93.4%) were determined and 45 (8.0%) resulted in an offer of reinstatement."
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 8, Heading = "Exclusions from pupil referral units", Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body =
                                        "The rate of permanent exclusion in pupil referral units decreased from 0.14 per cent in 2015/16 to 0.13 in 2016/17. After an increase from 2013/14 to 2014/15, permanent exclusions rates have remained fairly steady. There were 25,815 fixed-period exclusions in pupil referral units in 2016/17, up from 23,400 in 2015/16. The fixed period exclusion rate has been steadily increasing since 2013/14.\n\nThe percentage of pupil enrolments in pupil referral units who one or more fixed-period exclusion was 59.17 per cent in 2016/17, up from 58.15 per cent in 2015/16."
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 9, Heading = "Exclusions by local authority", Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body =
                                        "There is considerable variation in the permanent and fixed-period exclusion rate at local authority level (see accompanying maps on the web page).\n\nThe regions with the highest overall rates of permanent exclusion across state-funded primary, secondary and special schools are the West Midlands and the North West (at 0.14 per cent). The regions with the lowest rates are the South East (at 0.06 per cent) and Yorkshire and the Humber (at 0.07 per cent).\n\nThe region with the highest fixed-period exclusion rate is Yorkshire and the Humber (at 7.22 per cent), whilst the lowest rate was seen in Outer London (3.49 per cent).\n\nThese regions also had the highest and lowest rates of exclusion in the previous academic year."
                                }
                            }
                        }
                    }
                },

                // school pupil numbers
                new Release
                {
                    Id = new Guid("e3288537-9adb-431d-adfb-9bc3ef7be48c"),
                    Title = " Schools, pupils and their characteristics: January 2018 ",
                    ReleaseName = "January 2018 ",
                    PublicationId = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                    Published = new DateTime(2018, 5, 28),
                    Slug = "january-2018",
                    Summary =
                        "Statistics on pupils in schools in England as collected in the January 2018 school census.",
                    KeyStatistics = new DataBlock
                    {
                        Heading = "Latest headline facts and figures - 2016 to 2017",

                        Summary = new Summary
                        {
                            dataKeys = new List<string>
                            {
                                "--",
                                "--",
                                "--"
                            },

                            description = new MarkDownBlock
                            {
                                Body = ""
                            }
                        }
                    },
                    Content = new List<ContentSection>
                    {
                        new ContentSection
                        {
                            Order = 1, Heading = "About this release", Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body =
                                        "This statistical publication provides the number of schools and pupils in schools in England, using data from the January 2018School Census.\n\n Breakdowns are given for school types as well as for pupil characteristics including free school meal eligibility, English as an additional languageand ethnicity.This release also contains information about average class sizes.\n\n SEN tables previously provided in thispublication will be published in the statistical publication ‘Special educational needs in England: January 2018’ scheduled for release on 26July 2018.\n\n Cross border movement tables will be added to this publication later this year."
                                }
                            }
                        },
                    }
                },

                // GCSE / KS4
                new Release
                {
                    Id = new Guid("e7ae88fb-afaf-4d51-a78a-bbb2de671daf"),
                    Title = "GCSE and equivalent results in England, 2016 to 2017",
                    ReleaseName = "2016 to 2017",
                    PublicationId = new Guid("bfdcaae1-ce6b-4f63-9b2b-0a1f3942887f"),
                    Published = new DateTime(2018, 6, 20),
                    Slug = "2016-17",
                    Summary = "",

                    KeyStatistics = new DataBlock
                    {
                        Heading = "Latest headline facts and figures - 2016 to 2017",


                        Summary = new Summary
                        {
                            dataKeys = new List<string>
                            {
                                "--",
                                "--",
                                "--"
                            },

                            description = new MarkDownBlock
                            {
                                Body =
                                    " * average Attainment8 scores remained stable compared to 2017s \n" +
                                    " * percentage of pupils achieving 5 or above in English and Maths increased \n" +
                                    " * EBacc entry increased slightly \n" +
                                    " * over 250 schools met the coasting definition in 2018"
                            }
                        },
                    },

                    Content = new List<ContentSection>
                    {
                        new ContentSection
                        {
                            Order = 1, 
                            Heading = "About this release", 
                            Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body ="This release shows results for GCSE and equivalent Key Stage 4 (KS4) qualifications in 2018 across a range of measures, broken down by pupil characteristics and education institutions. Results are also provided on schools below the floor standards and meeting the coasting definition.  \n" +
                                          "This is an update to Provisional figures released in October 2018. Users should be careful when comparing headline measures to results in previous years given recent methodological changes \n" +
                                          "Figures are available at national, regional, local authority, and school level. Figures held in this release are used for policy development and count towards the secondary performance tables. Schools and local authorities also use the statistics to compare their local performance to regional and national averages for different pupil groups."
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 2, 
                            Heading = "School performance for 2018",
                            Caption = "School performance for 2018 shows small increases across all headline measures compared to 2017",
                            Content = new List<IContentBlock>
                            {
                                new DataBlock
                                {
                                    Heading = "Average headline performance measures over time",
                                    Summary = new Summary
                                    {
                                        dataKeys = new List<string>(),
                                        description = new MarkDownBlock
                                        {
                                            Body = ""
                                        }
                                    },
                                    DataQuery = new DataQuery(),
                                    Charts = new List<IContentBlockChart>()
                                },
                                new MarkDownBlock
                                {
                                    Body = "Results for 2018 show an increases across all headline measures compared to 2017. **When drawing comparison over time, however, it is very important to note any changes to methodology or data changes underpinning these measures**. For example, changes in Attainment 8 may have been affected by the introduction of further reformed GCSEs graded on the 9-1 scale which have a higher maximum score than unreformed GCSEs. Similarly, in 2016 there were significant changes to the Attainment in English and Maths measure. \n" +
                                           "These results cover state-funded schools but results for all schools are available in the supporting tables and show slightly lower performance across all headline measures on average. Differences between the figures for all schools and state-funded schools are primarily due to the impact of unapproved and unregulated qualifications such as international GCSEs taken more commonly in independent schools. These qualification are not included in school performance tables. \n" +
                                           "There are five primary headline measures used throughout this report: \n" +
                                           " * **Attainment8** - measures the average achievement of pupils in up to 8 qualifications (including English and Maths). \n" +
                                           " * **Attainment in English & Maths (9-5)** - measures the percentage of pupils achieving a grade 5 or above in both English and maths.\n" +
                                           " * **EBacc Entries** – measure the percentage of pupils reaching the English Baccalaureate (EBacc) attainment threshold in core academic subjects at key stage 4. The EBacc is made up of English, maths, science, a language, and history or geography. \n" +
                                           " * **EBacc Average Point Score (APS)** – measures pupils’ point scores across the five pillars of the EBacc, ensuring the attainment of all pupils is recognised. New measure from 2018, replacing the previous threshold EBacc attainment measure. \n" +
                                           " * **Progress** - measures the progress a pupil makes from the end of key stage 2 to the end of key stage 4. It compares pupils’ Attainment 8 score with the average for all pupils nationally who had a similar starting point. Progress 8 is a relative measure, therefore the national average Progress 8 score for mainstream schools is very close to zero. "
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 3, 
                            Heading = "Schools meeting the coasting and floor standard",
                            Caption = "Over 250 schools failed to support pupils to fulfil their potential in 2018",
                            Content = new List<IContentBlock>
                            {
                                new DataBlock
                                {
                                    Heading = "There is wide variation in the percentage of schools meeting the coasting and floor standard by region"
                                },
                                new MarkDownBlock
                                {
                                    Body ="The floor and coasting standards give measures of whether schools are helping pupils to fulfil their potential based on progress measures. The floor standard is based on results in the most recent year, whereas the Coasting definition looks at slightly different measures over the past three years. Only state-funded mainstream schools are covered by these measures, subject to certain eligibility criteria. \n" +
                                          "* **11.6%** of eligible schools were below the floor standard in 2018. This represents 346 schools\n" +
                                          "* **9.2%** of eligible schools met the coasting definition in 2018. This represents 257 schools \n" +
                                          "* **161** schools were both coating and below the floor standard \n" +
                                          "* due to methodological changes no directly comparable measures exist for previous years \n"
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 4, 
                            Heading = "Pupil characteristics",
                            Caption = "Disadvantaged pupils and those with Special Education Needs continue to do less well than their peers",
                            Content = new List<IContentBlock>
                            {
                                new DataBlock
                                {
                                    Heading = "Average headline scores by pupil characteristics"
                                },
                                new MarkDownBlock
                                {
                                    Body = "Breakdowns by pupil characteristics show that across all headline measures: \n" +
                                           "* girls continue to do better than boys \n" +
                                           "* non-disadvantaged pupils continue to do better than disadvantaged pupils \n" +
                                           "* pupils with no identified Special Educational Needs (SEN) continue to do better perform than SEN pupils \n" +
                                           "In general the pattern of attainment gaps for Attainment 8 in 2018 remained the same as in 2017 although differences in Attainment 8 scores widened slightly across all groups. This is to be expected due to changes to reformed GCSEs in 2018, meaning more points are available for higher scores.  \n" +
                                           "Due to changes in performance measures over time, comparability over time is complicated. As such, for disadvantaged pupils is recommended to use to disadvantage gap index instead with is more resilient to changes in grading systems over time. The gap between disadvantaged pupils and others, measured using the gap index, has remained broadly stable, widening by 0.6% in 2018, and narrowing by 9.5% since 2011." 
                                },
                                new DataBlock
                                {
                                    Heading = "Disadvantage attainment gap index"
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 5, 
                            Heading = "Headline performance",
                            Caption = "Results across headline performance measures vary by ethnicity",
                            Content = new List<IContentBlock>
                            {
                                new DataBlock
                                {
                                    Heading = "Average headline scores by pupil ethnicity"
                                },
                                new MarkDownBlock
                                {
                                    Body = "Results across headline measures differ by ethnicity with Chinese pupils in particular achieving scores above the national average. \n" +
                                           "Performance across headline measures increased for all major ethnic groups from 2017 to 2018, with the exception of EBacc entries for white pupils were there was a small decrease. \n" +
                                           "Within the more detailed ethnic groupings, pupils from an Indian background are the highest performing group in key stage 4 headline measures other than Chinese pupils. Gypsy/Roma pupils and traveller of Irish heritage pupils are the lowest performing groups. \n" +
                                           "For context, White pupils made up 75.8% of pupils at the end of key stage 4 in 2018, 10.6% were Asian, 5.5% were black, 4.7% were mixed, 0.4% were Chinese. The remainder are in smaller breakdowns or unclassified."
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 6, 
                            Heading = "Local authority",
                            Caption = "Performance by local authority varies considerably ",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body ="Performance varies considerably across the country – for Attainment 8 score per pupil there is nearly a 23 point gap between the poorest and highest performing areas. The highest performing local authorities are concentrated in London and the south with the majority of the lowest performing local authorities are located in the northern and midland regions with average Attainment 8 score per pupil show that. This is similar to patterns seen in recent years and against other performance measures. "
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 7, 
                            Heading = "Pupil subject areas",
                            Caption = "Pupil subject entries are highest for science and humanities and continue to increase",
                            Content = new List<IContentBlock>
                            {
                                new DataBlock
                                {
                                    Heading = "Pupil subject entries are highest for science and humanities and continue to increase"
                                },
                                new MarkDownBlock
                                {
                                    Body ="It is compulsory for pupils to study English and Maths at key stage 4 in state-funded schools.  \n " +
                                          "### Science\n" +
                                          "It is compulsory for schools to teach Science at Key Stage 4. For these subjects, the proportion of pupils entering continues to increase.  \n " +
                                          "In 2018, 68.0% of the cohort entered the new combined science pathway rather than the individual science subjects like Chemistry, Biology, Physics or Computer Science. The general pattern is for pupils with higher prior attainment tend to take single sciences; those with lower prior attainment to opt for the combined science pathway; and those with the lowest prior attainment to take no science qualifications. \n " +
                                          "### Humanities \n" +
                                          "The proportion of pupils entering EBacc humanities continued to increase in 2018, to 78.3% in state-funded schools, a rise of 1.5 percentage points since 2017. This was driven by small increases in entries across the majority of prior attainment groups for geography, and small increases in entries for pupils with low and average prior attainment for history. In history, the slight increase in entries from pupils with low and average prior attainment groups was counter-balanced by continued decreases in proportion of entries for high prior attainers. This trend has continued since 2016. \n " +
                                          "### Languages \n " +
                                          "Entries to EBacc languages continued to decrease in 2018 to 46.1%, a fall of 1.3 percentage points compared to 2017. This was the fourth year in a row that entries have fallen. There were decreases across the majority of prior attainment bands but the largest drop occurred for pupils with higher prior attainment.. This decrease in entries for pupils with high prior attainment between 2018 and 2017 is much smaller than the drop that occurred between 2016 and 2017. Some of this drop can be explained by pupils who entered a language qualification early in a subject that was subsequently reformed in 2018. This was the case for over 3,500 pupils, whose language result did not count in 2018 performance tables.  \n " +
                                          "### Art and design subjects \n " +
                                          "The percentage of pupils entering at least one arts subject decreased in 2018, by 2.2 percentage points compared to equivalent data in 2017. 44.3% of pupils in state-funded schools entered at least one arts subject. This is the third consecutive year that a fall in entries has occurred. "
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 8, 
                            Heading = "Schools performance",
                            Caption = "Across state-funded schools performance is typically higher in converter academies, the most common school type",
                            Content = new List<IContentBlock>
                            {
                                new DataBlock
                                {
                                    Heading = "Across state-funded schools performance is typically higher in converter academies, the most common school type"
                                },
                                new MarkDownBlock
                                {
                                    Body ="Schools in England can be divided into state-funded and independent schools (funded by fees paid by attendees). Independent schools are considered separately, because the department holds state-funded schools accountable for their performance.  \n " +
                                          "The vast majority of pupils in state-funded schools are in either academies (68%) or LA maintained schools (29%). *Converter academies* were high performing schools that chose to convert to academies and have on average higher attainment across the headline measures. *Sponsored academies* were schools that were low performing prior to conversion and tend to perform below the average for state-funded schools.  \n " +
                                          "Between 2017 and 2018 EBacc entry remained stable for sponsored academies, with an increase of 0.1 percentage points to 30.1%. EBacc entry fell marginally for converter academies by 0.3 percentage points (from 44.2% to 43.8%). Over the same period, EBacc entry in local authority maintained schools increased by 0.2 percentage points to 37.0%."
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 9, 
                            Heading = "Attainment",
                            Caption = "Multi-academy trust schools generally perform below national averages, but typically face greater challenges.",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body ="Academies are state schools directly funded by the government, each belonging to a trust. Multi-Academy Trusts (MATs) can be responsible for a group of academies and cover around 13.6% of state-funded mainstream pupils. Most MATs are responsible for between 3 and 5 schools but just over 10% cover 11 or more schools.  \n" +
                                          "Generally speaking MATs are typically more likely to cover previously poor-performing schools and pupils are more likely to have lower prior attainment, be disadvantaged, have special educational needs (SEN) or have English as an additional language (EAL) than the national average. \n" +
                                          "The number of eligible MATs included in Key Stage 4 measures increased from 62 in 2017 to 85 in 2018. This is an increase from 384 to 494 schools, and from 54,356 to 69,169 pupils. "
                                },
                                new DataBlock
                                {
                                    Heading = "Performance in MATs compared to national average"
                                },
                                new MarkDownBlock
                                {
                                    Body ="On Progress8 measures, in 2018, 32.9% of MATs were below the national average and 7.1% well below average. 29.4% were not above or below the national average by a statistically significant amount. \n" +
                                          "Entry rate in EBacc is lower in MATs compared to the national average – in 2018 43.5% of MATs had an entry rate higher than the national average of 39.1%. The EBacc average point score is also lower in MATs – 32.9% of MATs had an APS higher than the national average. \n" +
                                          "Analysis by characteristics shows that in 2018 disadvantaged pupils in MATs made more progress than the national average for disadvantaged. However, non-disadvantaged pupils, SEN and non-SEN pupils, pupils with English as a first language and high prior attainment pupils made less progress than the national average for their respective group."
                                }
                            }
                        },
                    }
                }
            );

            modelBuilder.Entity<Update>().HasData(
                new Update
                {
                    Id = new Guid("9c0f0139-7f88-4750-afe0-1c85cdf1d047"),
                    ReleaseId = new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                    On = new DateTime(2017, 4, 19),
                    Reason =
                        "Underlying data file updated to include absence data by pupil residency and school location, and updated metadata document."
                },
                new Update
                {
                    Id = new Guid("18e0d40e-bdf7-4c84-99dd-732e72e9c9a5"),
                    ReleaseId = new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                    On = new DateTime(2017, 3, 22),
                    Reason = "First published."
                },
                new Update
                {
                    Id = new Guid("51bd1e2f-2669-4708-b300-799b6be9ec9a"),
                    ReleaseId = new Guid("f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d"),
                    On = new DateTime(2016, 3, 25),
                    Reason = "First published."
                },
                new Update
                {
                    Id = new Guid("4fca874d-98b8-4c79-ad20-d698fb0af7dc"),
                    ReleaseId = new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                    On = new DateTime(2018, 7, 19),
                    Reason = "First published."
                },
                new Update
                {
                    Id = new Guid("33ff3f17-0671-41e9-b404-5661ab8a9476"),
                    ReleaseId = new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                    On = new DateTime(2018, 8, 25),
                    Reason =
                        " Updated exclusion rates for Gypsy/Roma pupils, to include extended ethnicity categories within the headcount (Gypsy, Roma and other Gypsy/Roma). "
                },
                new Update
                {
                    Id = new Guid("9aab1af8-27d4-43c4-a7cd-afb375c8809c"),
                    ReleaseId = new Guid("e3288537-9adb-431d-adfb-9bc3ef7be48c"),
                    On = new DateTime(2018, 5, 28),
                    Reason = "First published."
                },
                new Update
                {
                    Id = new Guid("aa4c0f33-cdf4-4df9-9540-18472d46a301"),
                    ReleaseId = new Guid("e3288537-9adb-431d-adfb-9bc3ef7be48c"),
                    On = new DateTime(2018, 6, 13),
                    Reason =
                        "Amended title of table 8e in attachment 'Schools pupils and their characteristics 2018 - LA tables'."
                },
                new Update
                {
                    Id = new Guid("4bd0f73b-ef2b-4901-839a-80cbf8c0871f"),
                    ReleaseId = new Guid("e3288537-9adb-431d-adfb-9bc3ef7be48c"),
                    On = new DateTime(2018, 7, 23),
                    Reason =
                        "Removed unrelated extra material from table 7c in attachment 'Schools pupils and their characteristics 2018 - LA tables'."
                },
                new Update
                {
                    Id = new Guid("7f911a4e-7a56-4f6f-92a6-bd556a9bcfd3"),
                    ReleaseId = new Guid("e3288537-9adb-431d-adfb-9bc3ef7be48c"),
                    On = new DateTime(2018, 9, 5),
                    Reason = "Added cross-border movement local authority level and underlying data tables."
                },
                new Update
                {
                    Id = new Guid("d008b331-af29-4c7e-bb8a-5a2005aa0131"),
                    ReleaseId = new Guid("e3288537-9adb-431d-adfb-9bc3ef7be48c"),
                    On = new DateTime(2018, 9, 11),
                    Reason =
                        "Added open document version of 'Schools pupils and their characteristics 2018 - Cross-border movement local authority tables'."
                }
            );

            modelBuilder.Entity<Link>().HasData(
                new Link
                {
                    PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                    Id = new Guid("45bc02ff-de90-489b-b78e-cdc7db662353"), Description = "2014 to 2015",
                    Url = "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2014-to-2015"
                },
                new Link
                {
                    PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                    Id = new Guid("82292fe7-1545-44eb-a094-80c5064701a7"), Description = "2013 to 2014",
                    Url = "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2013-to-2014"
                },
                new Link
                {
                    PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                    Id = new Guid("6907625d-0c2e-4fd8-8e96-aedd85b2ff97"), Description = "2012 to 2013",
                    Url = "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2012-to-2013"
                },
                new Link
                {
                    PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                    Id = new Guid("a538e57a-da5e-4a2c-a89e-b74dbae0c30b"), Description = "2011 to 2012",
                    Url =
                        "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics"
                },
                new Link
                {
                    PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                    Id = new Guid("18b24d60-c56e-44f0-8baa-6db4c6e7deee"), Description = "2010 to 2011",
                    Url =
                        "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2010-to-2011"
                },
                new Link
                {
                    PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                    Id = new Guid("c5444f5a-6ba5-4c80-883c-6bca0d8a9eb5"), Description = "2009 to 2010",
                    Url =
                        "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2009-to-2010"
                },
                new Link
                {
                    PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                    Id = new Guid("e3a532c4-df72-4daf-b621-5d04418fd521"),
                    Description = "2015 to 2016",
                    Url =
                        "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2015-to-2016"
                },
                new Link
                {
                    PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                    Id = new Guid("a0a8999a-9580-48b8-b443-61446ea579e4"),
                    Description = "2014 to 2015",
                    Url =
                        "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2014-to-2015"
                },
                new Link
                {
                    PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                    Id = new Guid("97c8ee35-4c62-406c-880a-cdfc92590490"),
                    Description = "2013 to 2014",
                    Url =
                        "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2013-to-2014"
                },
                new Link
                {
                    PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                    Id = new Guid("d76afaed-a665-4366-8897-78e9b90aa28a"),
                    Description = "2012 to 2013",
                    Url =
                        "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2012-to-2013"
                },
                new Link
                {
                    PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                    Id = new Guid("7f6c5499-640a-44c7-afdc-41e78c7e8b24"),
                    Description = "2011 to 2012",
                    Url =
                        "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-from-schools-in-england-2011-to-2012-academic-year"
                },
                new Link
                {
                    PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                    Id = new Guid("8c12b38a-a071-4c47-ba16-dffa734849ed"),
                    Description = "2010 to 2011",
                    Url =
                        "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-from-schools-in-england-academic-year-2010-to-2011"
                },
                new Link
                {
                    PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                    Id = new Guid("04ebb3e6-67fd-41f3-89e4-ed9566bcbe96"),
                    Description = "2009 to 2010",
                    Url =
                        "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-from-schools-in-england-academic-year-2009-to-2010"
                },
                new Link
                {
                    PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                    Id = new Guid("1b9375dd-06c7-4391-8265-447d6992a853"),
                    Description = "2008 to 2009",
                    Url =
                        "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-academic-year-2008-to-2009"
                },

                //school pupil numbers
                new Link
                {
                    PublicationId = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                    Id = new Guid("6404a25c-7352-4887-aa0e-c62948d45b57"),
                    Description = "January 2017",
                    Url =
                        "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2017"
                },
                new Link
                {
                    PublicationId = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                    Id = new Guid("31b06d27-7e31-491f-bd5d-89105369ac60"),
                    Description = "January 2016",
                    Url =
                        "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2016"
                },
                new Link
                {
                    PublicationId = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                    Id = new Guid("e764ef78-97f6-406c-aaaf-d3a5c847b362"),
                    Description = "January 2015",
                    Url =
                        "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2015"
                },
                new Link
                {
                    PublicationId = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                    Id = new Guid("1479a18c-b5a6-47c5-bb48-c8d60084b1a4"),
                    Description = "January 2014",
                    Url =
                        "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2014"
                },
                new Link
                {
                    PublicationId = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                    Id = new Guid("86893fdd-4a24-4fe9-9902-02ad8bbf8632"),
                    Description = "January 2013",
                    Url =
                        "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2013"
                },
                new Link
                {
                    PublicationId = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                    Id = new Guid("95c24ea7-4ebe-4f73-88f1-91ea33ec00bf"),
                    Description = "January 2012",
                    Url =
                        "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2012"
                },
                new Link
                {
                    PublicationId = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                    Id = new Guid("758b83d6-bd47-44aa-8ee2-359f350fef0a"),
                    Description = "January 2011",
                    Url =
                        "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2011"
                },
                new Link
                {
                    PublicationId = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                    Id = new Guid("acbef79a-7b53-492e-a679-ca994edfc892"),
                    Description = "January 2010",
                    Url =
                        "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2010"
                }
            );
        }
    }
}