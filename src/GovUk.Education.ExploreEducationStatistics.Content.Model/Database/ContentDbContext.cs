using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

// ReSharper disable StringLiteralTypo
namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Database
{
    public class ContentDbContext : DbContext
    {
        public ContentDbContext()
        {
        }

        public ContentDbContext(DbContextOptions<ContentDbContext> options)
            : base(options)
        {
        }

        private enum FilterItemName
        {
            Characteristic__Total,
            School_Type__Total,
            Year_of_admission__Primary_All_primary,
            Year_of_admission__Secondary_All_secondary
        }

        private enum IndicatorName
        {
            Unauthorised_absence_rate,
            Overall_absence_rate,
            Authorised_absence_rate,
            Number_of_schools,
            Number_of_pupils,
            Number_of_permanent_exclusions,
            Permanent_exclusion_rate,
            Number_of_fixed_period_exclusions,
            Fixed_period_exclusion_rate,
            Percentage_of_pupils_with_fixed_period_exclusions,
            Number_of_admissions,
            Number_of_applications_received,
            Number_of_first_preferences_offered,
            Number_of_second_preferences_offered,
            Number_of_third_preferences_offered,
            Number_that_received_one_of_their_first_three_preferences,
            Number_that_received_an_offer_for_a_preferred_school,
            Number_that_received_an_offer_for_a_non_preferred_school,
            Number_that_did_not_receive_an_offer,
            Number_that_received_an_offer_for_a_school_within_their_LA
        }

        private static readonly Dictionary<int, Dictionary<FilterItemName, int>> SubjectFilterItemIds =
            new Dictionary<int, Dictionary<FilterItemName, int>>
            {
                {
                    1, new Dictionary<FilterItemName, int>
                    {
                        {
                            FilterItemName.Characteristic__Total, 1
                        },
                        {
                            FilterItemName.School_Type__Total, 58
                        }
                    }
                },
                {
                    12, new Dictionary<FilterItemName, int>
                    {
                        {
                            FilterItemName.School_Type__Total, 461
                        }
                    }
                },
                {
                    17, new Dictionary<FilterItemName, int>
                    {
                        {
                            FilterItemName.Year_of_admission__Primary_All_primary, 575
                        },
                        {
                            FilterItemName.Year_of_admission__Secondary_All_secondary, 577
                        }
                    }
                }
            };

        private static readonly Dictionary<int, Dictionary<IndicatorName, int>> SubjectIndicatorIds =
            new Dictionary<int, Dictionary<IndicatorName, int>>
            {
                {
                    1, new Dictionary<IndicatorName, int>
                    {
                        {
                            IndicatorName.Unauthorised_absence_rate, 23
                        },
                        {
                            IndicatorName.Overall_absence_rate, 26
                        },
                        {
                            IndicatorName.Authorised_absence_rate, 28
                        }
                    }
                },
                {
                    12, new Dictionary<IndicatorName, int>
                    {
                        {
                            IndicatorName.Number_of_schools, 176
                        },
                        {
                            IndicatorName.Number_of_pupils, 177
                        },
                        {
                            IndicatorName.Number_of_permanent_exclusions, 178
                        },
                        {
                            IndicatorName.Permanent_exclusion_rate, 179
                        },
                        {
                            IndicatorName.Number_of_fixed_period_exclusions, 180
                        },
                        {
                            IndicatorName.Fixed_period_exclusion_rate, 181
                        },
                        {
                            IndicatorName.Percentage_of_pupils_with_fixed_period_exclusions, 183
                        }
                    }
                },
                {
                    17, new Dictionary<IndicatorName, int>
                    {
                        {
                            IndicatorName.Number_of_admissions, 211
                        },
                        {
                            IndicatorName.Number_of_applications_received, 212
                        },
                        {
                            IndicatorName.Number_of_first_preferences_offered, 216
                        },
                        {
                            IndicatorName.Number_of_second_preferences_offered, 217
                        },
                        {
                            IndicatorName.Number_of_third_preferences_offered, 218
                        },
                        {
                            IndicatorName.Number_that_received_one_of_their_first_three_preferences, 219
                        },
                        {
                            IndicatorName.Number_that_received_an_offer_for_a_preferred_school, 220
                        },
                        {
                            IndicatorName.Number_that_received_an_offer_for_a_non_preferred_school, 221
                        },
                        {
                            IndicatorName.Number_that_did_not_receive_an_offer, 222
                        },
                        {
                            IndicatorName.Number_that_received_an_offer_for_a_school_within_their_LA, 223
                        }
                    }
                }
            };

        public DbSet<Methodology> Methodologies { get; set; }
        public DbSet<Theme> Themes { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Publication> Publications { get; set; }
        public DbSet<Release> Releases { get; set; }
        public DbSet<ContentSection> ContentSections { get; set; }
        public DbSet<IContentBlock> ContentBlocks { get; set; }
        public DbSet<DataBlock> DataBlocks { get; set; }
        public DbSet<HtmlBlock> HtmlBlocks { get; set; }
        public DbSet<InsetTextBlock> InsetTextBlocks { get; set; }
        public DbSet<MarkDownBlock> MarkDownBlocks { get; set; }
        public DbSet<ReleaseSummary> ReleaseSummaries { get; set; }
        public DbSet<ReleaseSummaryVersion> ReleaseSummaryVersions { get; set; }
        public DbSet<ReleaseType> ReleaseTypes { get; set; }
        public DbSet<Contact> Contacts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Methodology>()
                .Property(b => b.Content)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<ContentSection>>(v));
            modelBuilder.Entity<Methodology>()
                .Property(b => b.Annexes)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<ContentSection>>(v));

            modelBuilder.Entity<Publication>()
                .Property(p => p.LegacyPublicationUrl)
                .HasConversion(
                    p => p.ToString(),
                    p => new Uri(p));
            
            modelBuilder.Entity<Release>()
                .Property(r => r.TimePeriodCoverage)
                .HasConversion(new EnumToEnumValueConverter<TimeIdentifier>())
                .HasMaxLength(6);
            
            modelBuilder.Entity<Release>()
                .Property<List<BasicLink>>("RelatedInformation")
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<BasicLink>>(v));

            modelBuilder.Entity<IContentBlock>()
                .ToTable("ContentBlock")
                .HasDiscriminator<string>("Type");

            modelBuilder.Entity<Release>()
                .Property(b => b.NextReleaseDate)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<PartialDate>(v));

            modelBuilder.Entity<Release>()
                .Property(b => b.Status)
                .HasConversion(new EnumToStringConverter<ReleaseStatus>());

            modelBuilder.Entity<ReleaseSummary>()
                .HasOne(rs => rs.Release).WithOne(r => r.ReleaseSummary)
                .HasForeignKey<ReleaseSummary>(rs => rs.ReleaseId);

            modelBuilder.Entity<ReleaseSummaryVersion>()
                .HasOne(rsv => rsv.ReleaseSummary)
                .WithMany(rs => rs.Versions)
                .HasForeignKey(rsv => rsv.ReleaseSummaryId);
            
            modelBuilder.Entity<ReleaseSummaryVersion>()
                .Property(b => b.NextReleaseDate)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<PartialDate>(v));
            
            modelBuilder.Entity<ReleaseSummaryVersion>()
                .Property(r => r.TimePeriodCoverage)
                .HasConversion(new EnumToEnumValueConverter<TimeIdentifier>())
                .HasMaxLength(6);
            
            modelBuilder.Entity<DataBlock>()
                .Property(block => block.Heading)
                .HasColumnName("DataBlock_Heading");
                        
            modelBuilder.Entity<DataBlock>()
                .Property(block => block.DataBlockRequest)
                .HasColumnName("DataBlock_Request");

            modelBuilder.Entity<DataBlock>()
                .Property(block => block.Charts)
                .HasColumnName("DataBlock_Charts")
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<IContentBlockChart>>(v));

            modelBuilder.Entity<DataBlock>()
                .Property(block => block.Summary)
                .HasColumnName("DataBlock_Summary")
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Summary>(v));
            
            modelBuilder.Entity<DataBlock>()
                .Property(block => block.Tables)
                .HasColumnName("DataBlock_Tables")
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<Table>>(v));
            
            modelBuilder.Entity<DataBlock>()
                .Property(block => block.DataBlockRequest)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<DataBlockRequest>(v));

            modelBuilder.Entity<HtmlBlock>()
                .Property(block => block.Body)
                .HasColumnName("HtmlBlock_Body");
 
            modelBuilder.Entity<InsetTextBlock>()
                .Property(block => block.Body)
                .HasColumnName("InsetTextBlock_Body");
            
            modelBuilder.Entity<InsetTextBlock>()
                .Property(block => block.Heading)
                .HasColumnName("InsetTextBlock_Heading");
            
            modelBuilder.Entity<MarkDownBlock>()
                .Property(block => block.Body)
                .HasColumnName("MarkDownBlock_Body");

            modelBuilder.Entity<ReleaseType>().HasData(
                new ReleaseType
                {
                    Id = new Guid("9d333457-9132-4e55-ae78-c55cb3673d7c"),
                    Title = "Official Statistics"
                },
                new ReleaseType
                {
                    Id = new Guid("1821abb8-68b0-431b-9770-0bea65d02ff0"),
                    Title = "Ad Hoc"
                },
                new ReleaseType
                {
                    Id = new Guid("8becd272-1100-4e33-8a7d-1c0c4e3b42b8"),
                    Title = "National Statistics"
                });

            modelBuilder.Entity<Theme>().HasData(
                new Theme
                {
                    Id = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"),
                    Title = "Children, early years and social care",
                    Summary =
                        "Including children in need, EYFS, and looked after children and social workforce statistics",
                    Slug = "children-and-early-years"
                },
                new Theme
                {
                    Id = new Guid("6412a76c-cf15-424f-8ebc-3a530132b1b3"),
                    Title = "Destination of pupils and students",
                    Summary =
                        "Including graduate labour market and not in education, employment or training (NEET) statistics",
                    Slug = "destination-of-pupils-and-students"
                },
                new Theme
                {
                    Id = new Guid("bc08839f-2970-4f34-af2d-29608a48082f"),
                    Title = "Finance and funding",
                    Summary = "Including local authority (LA) and student loan statistics",
                    Slug = "finance-and-funding"
                },
                new Theme
                {
                    Id = new Guid("92c5df93-c4da-4629-ab25-51bd2920cdca"),
                    Title = "Further education",
                    Summary =
                        "Including advanced learner loan, benefit claimant and apprenticeship and traineeship statistics",
                    Slug = "further-education"
                },
                new Theme
                {
                    Id = new Guid("2ca22e34-b87a-4281-a0eb-b80f4f8dd374"),
                    Title = "Higher education",
                    Summary = "Including university graduate employment and participation statistics",
                    Slug = "higher-education"
                },
                new Theme
                {
                    Id = new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f"),
                    Title = "Pupils and schools",
                    Summary =
                        "Including absence, application and offers, capacity exclusion and special educational needs (SEN) statistics",
                    Slug = "pupils-and-schools"
                },
                new Theme
                {
                    Id = new Guid("74648781-85a9-4233-8be3-fe6f137165f4"),
                    Title = "School and college outcomes and performance",
                    Summary = "Including GCSE and key stage statistcs",
                    Slug = "school-and-college-performance"
                },
                new Theme
                {
                    Id = new Guid("b601b9ea-b1c7-4970-b354-d1f695c446f1"),
                    Title = "Teachers and school workforce",
                    Summary = "Including initial teacher training (ITT) statistics",
                    Slug = "teachers-and-school-workforce"
                },
                new Theme
                {
                    Id = new Guid("a95d2ca2-a969-4320-b1e9-e4781112574a"),
                    Title = "UK education and training statistics",
                    Summary =
                        "Including summarised expenditure, post-compulsory education, qualitification and school statistics",
                    Slug = "uk-education-and-training-statistics"
                }
            );

            modelBuilder.Entity<Topic>().HasData(
                new Topic
                {
                    Id = new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"),
                    Title = "Childcare and early years",
                    Summary = "",
                    ThemeId = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"),
                    Slug = "childcare-and-early-years"
                },
                new Topic
                {
                    Id = new Guid("22c52d89-88c0-44b5-96c4-042f1bde6ddd"),
                    Title = "Children in need and child protection",
                    Summary = "",
                    ThemeId = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"),
                    Slug = "children-in-need-and-child-protection"
                },
                new Topic
                {
                    Id = new Guid("734820b7-f80e-45c3-bb92-960edcc6faa5"),
                    Title = "Children's social work workforce",
                    Summary = "",
                    ThemeId = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"),
                    Slug = "childrens-social-work-workforce"
                },
                new Topic
                {
                    Id = new Guid("17b2e32c-ed2f-4896-852b-513cdf466769"),
                    Title = "Early years foundation stage profile",
                    Summary = "",
                    ThemeId = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"),
                    Slug = "early-years-foundation-stage-profile"
                },
                new Topic
                {
                    Id = new Guid("66ff5e67-36cf-4210-9ad2-632baeb4eca7"),
                    Title = "Looked-after children",
                    Summary = "",
                    ThemeId = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"),
                    Slug = "looked-after-children"
                },
                new Topic
                {
                    Id = new Guid("d5288137-e703-43a1-b634-d50fc9785cb9"),
                    Title = "Secure children's homes",
                    Summary = "",
                    ThemeId = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"),
                    Slug = "secure-children-homes"
                },
                new Topic
                {
                    Id = new Guid("0b920c62-ff67-4cf1-89ec-0c74a364e6b4"),
                    Title = "Destinations of key stage 4 and key stage 5 pupils",
                    Summary = "",
                    ThemeId = new Guid("6412a76c-cf15-424f-8ebc-3a530132b1b3"),
                    Slug = "destinations-of-ks4-and-ks5-pupils"
                },
                new Topic
                {
                    Id = new Guid("3bef5b2b-76a1-4be1-83b1-a3269245c610"),
                    Title = "Graduate labour market",
                    Summary = "",
                    ThemeId = new Guid("6412a76c-cf15-424f-8ebc-3a530132b1b3"),
                    Slug = "graduate-labour-market"
                },
                new Topic
                {
                    Id = new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860"),
                    Title = "NEET and participation",
                    Summary = "",
                    ThemeId = new Guid("6412a76c-cf15-424f-8ebc-3a530132b1b3"),
                    Slug = "neet-and-participation"
                },
                new Topic
                {
                    Id = new Guid("4c658598-450b-4493-b972-8812acd154a7"),
                    Title = "Local authority and school finance",
                    Summary = "",
                    ThemeId = new Guid("bc08839f-2970-4f34-af2d-29608a48082f"),
                    Slug = "local-authority-and-school-finance"
                },
                new Topic
                {
                    Id = new Guid("5c5bc908-f813-46e2-aae8-494804a57aa1"),
                    Title = "Student loan forecasts",
                    Summary = "",
                    ThemeId = new Guid("bc08839f-2970-4f34-af2d-29608a48082f"),
                    Slug = "student-loan-forecasts"
                },
                new Topic
                {
                    Id = new Guid("ba0e4a29-92ef-450c-97c5-80a0a6144fb5"),
                    Title = "Advanced learner loans",
                    Summary = "",
                    ThemeId = new Guid("92c5df93-c4da-4629-ab25-51bd2920cdca"),
                    Slug = "advanced-learner-loans"
                },
                new Topic
                {
                    Id = new Guid("dd4a5d02-fcc9-4b7f-8c20-c153754ba1e4"),
                    Title = "FE choices",
                    Summary = "",
                    ThemeId = new Guid("92c5df93-c4da-4629-ab25-51bd2920cdca"),
                    Slug = "fe-choices"
                },
                new Topic
                {
                    Id = new Guid("88d08425-fcfd-4c87-89da-70b2062a7367"),
                    Title = "Further education and skills",
                    Summary = "",
                    ThemeId = new Guid("92c5df93-c4da-4629-ab25-51bd2920cdca"),
                    Slug = "further-education-and-skills"
                },
                new Topic
                {
                    Id = new Guid("cf1f1dc5-27c2-4d15-a55a-9363b7757ff3"),
                    Title = "Further education for benefits claimants",
                    Summary = "",
                    ThemeId = new Guid("92c5df93-c4da-4629-ab25-51bd2920cdca"),
                    Slug = "further-education-for-benefits-claimants"
                },
                new Topic
                {
                    Id = new Guid("dc7b7a89-e968-4a7e-af5f-bd7d19c346a5"),
                    Title = "National achievement rates tables",
                    Summary = "",
                    ThemeId = new Guid("92c5df93-c4da-4629-ab25-51bd2920cdca"),
                    Slug = "national-achievement-rates-tables"
                },
                new Topic
                {
                    Id = new Guid("53a1fbb7-5234-435f-892b-9baad4c82535"),
                    Title = "Higher education graduate employment and earnings",
                    Summary = "",
                    ThemeId = new Guid("2ca22e34-b87a-4281-a0eb-b80f4f8dd374"),
                    Slug = "higher-education-graduate-employment-and-earnings"
                },
                new Topic
                {
                    Id = new Guid("2458a916-df6e-4845-9658-a81eace42ffd"),
                    Title = "Higher education statistics",
                    Summary = "",
                    ThemeId = new Guid("2ca22e34-b87a-4281-a0eb-b80f4f8dd374"),
                    Slug = "higher-education-statistics"
                },
                new Topic
                {
                    Id = new Guid("04d95654-9fe0-4f78-9dfd-cf396661ebe9"),
                    Title = "Participation rates in higher education",
                    Summary = "",
                    ThemeId = new Guid("2ca22e34-b87a-4281-a0eb-b80f4f8dd374"),
                    Slug = "participation-rates-in-higher-education"
                },
                new Topic
                {
                    Id = new Guid("7871f559-0cfe-47c0-b48d-25b2bc8a0418"),
                    Title = "Widening participation in higher education",
                    Summary = "",
                    ThemeId = new Guid("2ca22e34-b87a-4281-a0eb-b80f4f8dd374"),
                    Slug = "widening-participation-in-higher-education"
                },
                new Topic
                {
                    Id = new Guid("c9f0b897-d58a-42b0-9d12-ca874cc7c810"),
                    Title = "Admission appeals",
                    Summary = "",
                    ThemeId = new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f"),
                    Slug = "admission-appeals"
                },
                new Topic
                {
                    Id = new Guid("77941b7d-bbd6-4069-9107-565af89e2dec"),
                    Title = "Exclusions",
                    Summary = "",
                    ThemeId = new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f"),
                    Slug = "exclusions"
                },
                new Topic
                {
                    Id = new Guid("67c249de-1cca-446e-8ccb-dcdac542f460"),
                    Title = "Pupil absence",
                    Summary = "",
                    ThemeId = new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f"),
                    Slug = "pupil-absence"
                },
                new Topic
                {
                    Id = new Guid("6b8c0242-68e2-420c-910c-e19524e09cd2"),
                    Title = "Parental responsibility measures",
                    Summary = "",
                    ThemeId = new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f"),
                    Slug = "parental-responsibility-measures"
                },
                new Topic
                {
                    Id = new Guid("5e196d11-8ac4-4c82-8c46-a10a67c1118e"),
                    Title = "Pupil projections",
                    Summary = "",
                    ThemeId = new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f"),
                    Slug = "pupil-projections"
                },
                new Topic
                {
                    Id = new Guid("e50ba9fd-9f19-458c-aceb-4422f0c7d1ba"),
                    Title = "School and pupil numbers",
                    Summary = "",
                    ThemeId = new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f"),
                    Slug = "school-and-pupil-numbers"
                },
                new Topic
                {
                    Id = new Guid("1a9636e4-29d5-4c90-8c07-f41db8dd019c"),
                    Title = "School applications",
                    Summary = "",
                    ThemeId = new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f"),
                    Slug = "school-applications"
                },
                new Topic
                {
                    Id = new Guid("87c27c5e-ae49-4932-aedd-4405177d9367"),
                    Title = "School capacity",
                    Summary = "",
                    ThemeId = new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f"),
                    Slug = "school-capacity"
                },
                new Topic
                {
                    Id = new Guid("85349b0a-19c7-4089-a56b-ad8dbe85449a"),
                    Title = "Special educational needs (SEN)",
                    Summary = "",
                    ThemeId = new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f"),
                    Slug = "special-educational-needs"
                },
                new Topic
                {
                    Id = new Guid("85b5454b-3761-43b1-8e84-bd056a8efcd3"),
                    Title = "16 to 19 attainment",
                    Summary = "",
                    ThemeId = new Guid("74648781-85a9-4233-8be3-fe6f137165f4"),
                    Slug = "16-to-19-attainment"
                },
                new Topic
                {
                    Id = new Guid("1e763f55-bf09-4497-b838-7c5b054ba87b"),
                    Title = "GCSEs (key stage 4)",
                    Summary = "",
                    ThemeId = new Guid("74648781-85a9-4233-8be3-fe6f137165f4"),
                    Slug = "gcses-key-stage-4"
                },
                new Topic
                {
                    Id = new Guid("504446c2-ddb1-4d52-bdbc-4148c2c4c460"),
                    Title = "Key stage 1",
                    Summary = "",
                    ThemeId = new Guid("74648781-85a9-4233-8be3-fe6f137165f4"),
                    Slug = "key-stage-1"
                },
                new Topic
                {
                    Id = new Guid("eac38700-b968-4029-b8ac-0eb8e1356480"),
                    Title = "Key stage 2",
                    Summary = "",
                    ThemeId = new Guid("74648781-85a9-4233-8be3-fe6f137165f4"),
                    Slug = "key-stage-2"
                },
                new Topic
                {
                    Id = new Guid("a7ce9542-20e6-401d-91f4-f832c9e58b12"),
                    Title = "Outcome based success measures",
                    Summary = "",
                    ThemeId = new Guid("74648781-85a9-4233-8be3-fe6f137165f4"),
                    Slug = "outcome-based-success-measures"
                },
                new Topic
                {
                    Id = new Guid("1318eb73-02a8-4e50-82a9-7e271176c4d1"),
                    Title = "Performance tables",
                    Summary = "",
                    ThemeId = new Guid("74648781-85a9-4233-8be3-fe6f137165f4"),
                    Slug = "performance-tables"
                },
                new Topic
                {
                    Id = new Guid("0f8792d2-28b1-4537-a1b4-3e139fcf0ca7"),
                    Title = "Initial teacher training (ITT)",
                    Summary = "",
                    ThemeId = new Guid("b601b9ea-b1c7-4970-b354-d1f695c446f1"),
                    Slug = "initial-teacher-training"
                },
                new Topic
                {
                    Id = new Guid("28cfa002-83cb-4011-9ddd-859ec99e0aa0"),
                    Title = "School workforce",
                    Summary = "",
                    ThemeId = new Guid("b601b9ea-b1c7-4970-b354-d1f695c446f1"),
                    Slug = "school-workforce"
                },
                new Topic
                {
                    Id = new Guid("6d434e17-7b76-425d-897d-c7b369b42e35"),
                    Title = "Teacher workforce statistics and analysis",
                    Summary = "",
                    ThemeId = new Guid("b601b9ea-b1c7-4970-b354-d1f695c446f1"),
                    Slug = "teacher-workforce-statistics-and-analysis"
                },
                new Topic
                {
                    Id = new Guid("692050da-9ac9-435a-80d5-a6be4915f0f7"),
                    Title = "UK education and training statistics",
                    Summary = "",
                    ThemeId = new Guid("a95d2ca2-a969-4320-b1e9-e4781112574a"),
                    Slug = "uk-education-and-training-statistics"
                }
            );

            modelBuilder.Entity<Contact>().HasData(
                new Contact
                {
                    Id = new Guid("58117de4-5951-48e4-8537-9f74967a6233"),
                    TeamName = "Test Team",
                    TeamEmail = "explore.statistics@education.gov.uk",
                    ContactName = "Laura Selby",
                    ContactTelNo = "07384237142"
                },
                new Contact
                {
                    Id = new Guid("72f846d7-1580-484e-b299-3ce13070f297"),
                    TeamName = "Another Test Team",
                    TeamEmail = "explore.statistics@education.gov.uk",
                    ContactName = "John Shale",
                    ContactTelNo = "07919937921"
                },
                new Contact
                {
                    Id = new Guid("32d61132-e4c0-442c-88f4-f879971eb699"),
                    TeamName = "Explore Education Statistics",
                    TeamEmail = "explore.statistics@education.gov.uk",
                    ContactName = "Cameron Race",
                    ContactTelNo = "07780991976"
                },
                new Contact
                {
                    Id = new Guid("d246c696-4b3a-4aeb-842c-c1318ee334e8"),
                    TeamName = "School absence and exclusions team",
                    TeamEmail = "schools.statistics@education.gov.uk",
                    ContactName = "Mark Pearson",
                    ContactTelNo = "01142742585"
                },
                new Contact
                {
                    Id = new Guid("74f5aade-6d24-4a0b-be23-2ab4b4b2d191"),
                    TeamName = "School preference statistics team",
                    TeamEmail = "school.preference@education.gov.uk",
                    ContactName = "Helen Bray",
                    ContactTelNo = "02077838553"
                },
                new Contact
                {
                    Id = new Guid("0b63e6c7-5a9d-4c48-b30f-f0729e0644c0"),
                    TeamName = "Special educational needs statistics team",
                    TeamEmail = "sen.statistics@education.gov.uk",
                    ContactName = "Sean Gibson",
                    ContactTelNo = "01325340987"
                },
                new Contact
                {
                    Id = new Guid("0d2ead36-3ebc-482f-a9c9-e17d746a0dd9"),
                    TeamName = "Looked-after children statistics team",
                    TeamEmail = "cla.stats@education.gov.uk",
                    ContactName = "Justin Ushie",
                    ContactTelNo = "01325340817"
                },
                new Contact
                {
                    Id = new Guid("18c9a473-465d-4b8a-b2cf-b24fd3b9c094"),
                    TeamName = "Attainment statistics team",
                    TeamEmail = "Attainment.STATISTICS@education.gov.uk",
                    ContactName = "Raffaele Sasso",
                    ContactTelNo = "07469413581"
                }
            );

            modelBuilder.Entity<Publication>().HasData(
                new Publication
                {
                    Id = new Guid("d63daa75-5c3e-48bf-a232-f232e0d13898"),
                    Title = "30 hours free childcare",
                    Summary = "",
                    TopicId = new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"),
                    Slug = "30-hours-free-childcare",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-childcare-and-early-years#30-hours-free-childcare")
                },
                new Publication
                {
                    Id = new Guid("79a08466-dace-4ff0-94b6-59c5528c9262"),
                    Title = "Childcare and early years provider survey",
                    Summary = "",
                    TopicId = new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"),
                    Slug = "childcare-and-early-years-provider-survey",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-childcare-and-early-years#childcare-and-early-years-providers-survey")
                },
                new Publication
                {
                    Id = new Guid("060c5376-35d8-420b-8266-517a9339b7bc"),
                    Title = "Childcare and early years survey of parents",
                    Summary = "",
                    TopicId = new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"),
                    Slug = "childcare-and-early-years-survey-of-parents",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-childcare-and-early-years#childcare-and-early-years-providers-survey")
                },
                new Publication
                {
                    Id = new Guid("0ce6a6c6-5451-4967-8dd4-2f4fa8131982"),
                    Title = "Education provision: children under 5 years of age",
                    Summary = "",
                    TopicId = new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"),
                    Slug = "education-provision-children-under-5",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-childcare-and-early-years#provision-for-children-under-5-years-of-age-in-england")
                },
                new Publication
                {
                    Id = new Guid("89869bba-0c00-40f7-b7d6-e28cb904ad37"),
                    Title = "Characteristics of children in need",
                    Summary = "",
                    TopicId = new Guid("22c52d89-88c0-44b5-96c4-042f1bde6ddd"),
                    Slug = "characteristics-of-children-in-need",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-children-in-need#characteristics-of-children-in-need")
                },
                new Publication
                {
                    Id = new Guid("d8baee79-3c88-45f4-b12a-07b91e9b5c11"),
                    Title = "Children's social work workforce",
                    Summary = "",
                    TopicId = new Guid("734820b7-f80e-45c3-bb92-960edcc6faa5"),
                    Slug = "childrens-social-work-workforce",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-childrens-social-care-workforce#statutory-collection")
                },
                new Publication
                {
                    Id = new Guid("fcda2962-82a6-4052-afa2-ea398c53c85f"),
                    Title = "Early years foundation stage profile results",
                    Summary = "",
                    TopicId = new Guid("17b2e32c-ed2f-4896-852b-513cdf466769"),
                    Slug = "early-years-foundation-stage-profile-results",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-early-years-foundation-stage-profile#results-at-national-and-local-authority-level")
                },
                new Publication
                {
                    Id = new Guid("3260801d-601a-48c6-93b7-cf51680323d1"),
                    Title = "Children looked after in England including adoptions",
                    Summary = "",
                    TopicId = new Guid("66ff5e67-36cf-4210-9ad2-632baeb4eca7"),
                    Slug = "children-looked-after-in-england-including-adoptions",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-looked-after-children#looked-after-children")
                },
                new Publication
                {
                    Id = new Guid("f51895df-c682-45e6-b23e-3138ddbfdaeb"),
                    Title = "Outcomes for children looked after by LAs",
                    Summary = "",
                    TopicId = new Guid("66ff5e67-36cf-4210-9ad2-632baeb4eca7"),
                    Slug = "outcomes-for-children-looked-after-by-las",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-looked-after-children#outcomes-for-looked-after-children")
                },
                new Publication
                {
                    Id = new Guid("d7bd5d9d-dc65-4b1d-99b1-4d815b7369a3"),
                    Title = "Children accommodated in secure children's homes",
                    Summary = "",
                    TopicId = new Guid("d5288137-e703-43a1-b634-d50fc9785cb9"),
                    Slug = "children-accommodated-in-secure-childrens-homes",
                    LegacyPublicationUrl =
                        new Uri("https://www.gov.uk/government/collections/statistics-secure-children-s-homes")
                },
                new Publication
                {
                    Id = new Guid("8a92c6a5-8110-4c9c-87b1-e15f1c80c66a"),
                    Title = "Destinations of key stage 4 and key stage 5 pupils",
                    Summary = "",
                    TopicId = new Guid("0b920c62-ff67-4cf1-89ec-0c74a364e6b4"),
                    Slug = "destinations-of-ks4-and-ks5-pupils",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-destinations#destinations-after-key-stage-4-and-5")
                },
                new Publication
                {
                    Id = new Guid("42a888c4-9ee7-40fd-9128-f5de546780b3"),
                    Title = "Graduate labour market statistics",
                    Summary = "",
                    TopicId = new Guid("3bef5b2b-76a1-4be1-83b1-a3269245c610"),
                    Slug = "graduate-labour-markets",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/graduate-labour-market-quarterly-statistics#documents")
                },
                new Publication
                {
                    Id = new Guid("a0eb117e-44a8-4732-adf1-8fbc890cbb62"),
                    Title = "Participation in education and training and employment",
                    Summary = "",
                    TopicId = new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860"),
                    Slug = "participation-in-education-training-and-employement",
                    LegacyPublicationUrl =
                        new Uri("https://www.gov.uk/government/collections/statistics-neet#participation-in-education")
                },
                new Publication
                {
                    Id = new Guid("2e510281-ca8c-41bf-bbe0-fd15fcc81aae"),
                    Title = "NEET statistics quarterly brief",
                    Summary = "",
                    TopicId = new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860"),
                    Slug = "neet-statistics-quarterly-brief",
                    LegacyPublicationUrl =
                        new Uri("https://www.gov.uk/government/collections/statistics-neet#neet:-2016-to-2017-data-")
                },
                new Publication
                {
                    Id = new Guid("8ab47806-e36f-4226-9988-1efe23156872"),
                    Title = "Income and expenditure in academies in England",
                    Summary = "",
                    TopicId = new Guid("4c658598-450b-4493-b972-8812acd154a7"),
                    Slug = "income-and-expenditure-in-academies-in-england",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-local-authority-school-finance-data#academy-spending")
                },
                new Publication
                {
                    Id = new Guid("dcb8b32b-4e50-4fe2-a539-58f9b6b3a366"),
                    Title = "LA and school expenditure",
                    Summary = "",
                    TopicId = new Guid("4c658598-450b-4493-b972-8812acd154a7"),
                    Slug = "la-and-school-expenditure",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-local-authority-school-finance-data#local-authority-and-school-finance")
                },
                new Publication
                {
                    Id = new Guid("94d16c6e-1e5f-48d5-8195-8ea770f1b0d4"),
                    Title = "Planned LA and school expenditure",
                    Summary = "",
                    TopicId = new Guid("4c658598-450b-4493-b972-8812acd154a7"),
                    Slug = "planned-la-and-school-expenditure",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-local-authority-school-finance-data#planned-local-authority-and-school-spending-")
                },
                new Publication
                {
                    Id = new Guid("fd68e147-b7ee-464f-8b02-dcd917dc362d"),
                    Title = "Student loan forecasts for England",
                    Summary = "",
                    TopicId = new Guid("5c5bc908-f813-46e2-aae8-494804a57aa1"),
                    Slug = "student-loan-forecasts-for-england",
                    LegacyPublicationUrl =
                        new Uri("https://www.gov.uk/government/collections/statistics-student-loan-forecasts#documents")
                },
                new Publication
                {
                    Id = new Guid("75568912-25ba-499a-8a96-6161b54994db"),
                    Title = "Advanced learner loans applications",
                    Summary = "",
                    TopicId = new Guid("ba0e4a29-92ef-450c-97c5-80a0a6144fb5"),
                    Slug = "advanced-learner-loans-applications",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/further-education#advanced-learner-loans-applications-2017-to-2018")
                },
                new Publication
                {
                    Id = new Guid("f00a784b-52e8-475b-b8ee-dbe730382ba8"),
                    Title = "FE chioces employer satisfaction survey",
                    Summary = "",
                    TopicId = new Guid("dd4a5d02-fcc9-4b7f-8c20-c153754ba1e4"),
                    Slug = "fe-choices-employer-satisfaction-survey",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/fe-choices#employer-satisfaction-survey-data")
                },
                new Publication
                {
                    Id = new Guid("657b1484-0369-4a0e-873a-367b79a48c35"),
                    Title = "FE choices learner satisfaction survey",
                    Summary = "",
                    TopicId = new Guid("dd4a5d02-fcc9-4b7f-8c20-c153754ba1e4"),
                    Slug = "fe-choices-learner-satisfaction-survey",
                    LegacyPublicationUrl =
                        new Uri("https://www.gov.uk/government/collections/fe-choices#learner-satisfaction-survey-data")
                },
                new Publication
                {
                    Id = new Guid("d24783b6-24a7-4ef3-8304-fd07eeedff92"),
                    Title = "Apprenticeship and levy statistics",
                    Summary = "",
                    TopicId = new Guid("88d08425-fcfd-4c87-89da-70b2062a7367"),
                    Slug = "apprenticeship-and-levy-statistics",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/further-education-and-skills-statistical-first-release-sfr#apprenticeships-and-levy---older-data")
                },
                new Publication
                {
                    Id = new Guid("cf0ec981-3583-42a5-b21b-3f2f32008f1b"),
                    Title = "Apprenticeships and traineeships",
                    Summary = "",
                    TopicId = new Guid("88d08425-fcfd-4c87-89da-70b2062a7367"),
                    Slug = "apprenticeships-and-traineeships",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/further-education-and-skills-statistical-first-release-sfr#apprenticeships-and-traineeships---older-data")
                },
                new Publication
                {
                    Id = new Guid("13b81bcb-e8cd-4431-9807-ca588fd1d02a"),
                    Title = "Further education and skills",
                    Summary = "",
                    TopicId = new Guid("88d08425-fcfd-4c87-89da-70b2062a7367"),
                    Slug = "further-education-and-skills",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/further-education-and-skills-statistical-first-release-sfr#fe-and-skills---older-data")
                },
                new Publication
                {
                    Id = new Guid("ce6098a6-27b6-44b5-8e63-36df3a659e69"),
                    Title = "Further education and benefits claimants",
                    Summary = "",
                    TopicId = new Guid("cf1f1dc5-27c2-4d15-a55a-9363b7757ff3"),
                    Slug = "further-education-and-benefits-claimants",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/further-education-for-benefit-claimants#documents")
                },
                new Publication
                {
                    Id = new Guid("7a57d4c0-5233-4d46-8e27-748fbc365715"),
                    Title = "National achievement rates tables",
                    Summary = "",
                    TopicId = new Guid("dc7b7a89-e968-4a7e-af5f-bd7d19c346a5"),
                    Slug = "national-achievement-rates-tables",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/sfa-national-success-rates-tables#national-achievement-rates-tables")
                },
                new Publication
                {
                    Id = new Guid("4d29c28c-efd1-4245-a80c-b55c6a50e3f7"),
                    Title = "Graduate outcomes (LEO)",
                    Summary = "",
                    TopicId = new Guid("53a1fbb7-5234-435f-892b-9baad4c82535"),
                    Slug = "graduate-outcomes",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-higher-education-graduate-employment-and-earnings#documents")
                },
                new Publication
                {
                    Id = new Guid("d4b9551b-d92c-4f98-8731-847780d3c9fa"),
                    Title = "Higher education: destinations of leavers",
                    Summary = "",
                    TopicId = new Guid("2458a916-df6e-4845-9658-a81eace42ffd"),
                    Slug = "higher-education-destinations-of-leavers",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/official-statistics-releases#destinations-of-higher-education-leavers")
                },
                new Publication
                {
                    Id = new Guid("14cfd218-5480-4ba1-a051-5b1e6be14b46"),
                    Title = "Higher education enrolments and qualifications",
                    Summary = "",
                    TopicId = new Guid("2458a916-df6e-4845-9658-a81eace42ffd"),
                    Slug = "higher-education-enrolments-and-qualifications",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/official-statistics-releases#higher-education-enrolments-and-qualifications")
                },
                new Publication
                {
                    Id = new Guid("b83f55db-73fc-46fc-9fda-9b59f5896e9d"),
                    Title = "Performance indicators in higher education",
                    Summary = "",
                    TopicId = new Guid("2458a916-df6e-4845-9658-a81eace42ffd"),
                    Slug = "performance-indicators-in-higher-education",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/official-statistics-releases#performance-indicators")
                },
                new Publication
                {
                    Id = new Guid("6c25a3e9-fc96-472f-895c-9ae4492dd2a4"),
                    Title = "Staff at higher education providers in the UK",
                    Summary = "",
                    TopicId = new Guid("2458a916-df6e-4845-9658-a81eace42ffd"),
                    Slug = "staff-at-higher-education-providers-in-the-uk",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/official-statistics-releases#staff-at-higher-education")
                },
                new Publication
                {
                    Id = new Guid("0c67bbdb-4eb0-41cf-a62e-2589cee58538"),
                    Title = "Participation rates in higher education",
                    Summary = "",
                    TopicId = new Guid("04d95654-9fe0-4f78-9dfd-cf396661ebe9"),
                    Slug = "participation-rates-in-higher-education",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-on-higher-education-initial-participation-rates#participation-rates-in-higher-education-for-england")
                },
                new Publication
                {
                    Id = new Guid("c28f7aca-f1e8-4916-8ce3-fc177b140695"),
                    Title = "Widening participation in higher education",
                    Summary = "",
                    TopicId = new Guid("7871f559-0cfe-47c0-b48d-25b2bc8a0418"),
                    Slug = "widening-participation-in-higher-education",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/widening-participation-in-higher-education#documents")
                },
                new Publication
                {
                    Id = new Guid("123461ab-50be-45d9-8523-c5241a2c9c5b"),
                    Title = "Admission appeals in England",
                    Summary = "",
                    TopicId = new Guid("c9f0b897-d58a-42b0-9d12-ca874cc7c810"),
                    Slug = "admission-appeals-in-england",
                    LegacyPublicationUrl =
                        new Uri("https://www.gov.uk/government/collections/statistics-admission-appeals#documents")
                },
                new Publication
                {
                    Id = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                    Title = "Permanent and fixed-period exclusions in England",
                    MethodologyId = new Guid("c8c911e3-39c1-452b-801f-25bb79d1deb7"),
                    Summary = "",
                    TopicId = new Guid("77941b7d-bbd6-4069-9107-565af89e2dec"),
                    Slug = "permanent-and-fixed-period-exclusions-in-england",
                    NextUpdate = new DateTime(2019, 7, 19),
                    ContactId = new Guid("d246c696-4b3a-4aeb-842c-c1318ee334e8")
                },
                new Publication
                {
                    Id = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                    Title = "Pupil absence in schools in England",
                    MethodologyId = new Guid("caa8e56f-41d2-4129-a5c3-53b051134bd7"),
                    Summary = "",
                    TopicId = new Guid("67c249de-1cca-446e-8ccb-dcdac542f460"),
                    Slug = "pupil-absence-in-schools-in-england",
                    NextUpdate = new DateTime(2019, 3, 22),
                    DataSource =
                        "[Pupil absence statistics: guide](https://www.gov.uk/government/publications/absence-statistics-guide#)",
                    ContactId = new Guid("d246c696-4b3a-4aeb-842c-c1318ee334e8")
                },
                new Publication
                {
                    Id = new Guid("6c388293-d027-4f74-8d74-29a42e02231c"),
                    Title = "Pupil absence in schools in England: autumn term",
                    Summary = "",
                    TopicId = new Guid("67c249de-1cca-446e-8ccb-dcdac542f460"),
                    Slug = "pupil-absence-in-schools-in-england-autumn-term",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-pupil-absence#autumn-term-release")
                },
                new Publication
                {
                    Id = new Guid("14953fda-02ff-45ed-9573-3a7a0ad8cb10"),
                    Title = "Pupil absence in schools in England: autumn and spring",
                    Summary = "",
                    TopicId = new Guid("67c249de-1cca-446e-8ccb-dcdac542f460"),
                    Slug = "pupil-absence-in-schools-in-england-autumn-and-spring",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-pupil-absence#combined-autumn--and-spring-term-release")
                },
                new Publication
                {
                    Id = new Guid("86af24dc-67c4-47f0-a849-e94c7a1cfe9b"),
                    Title = "Parental responsibility measures",
                    Summary = "",
                    TopicId = new Guid("6b8c0242-68e2-420c-910c-e19524e09cd2"),
                    Slug = "parental-responsibility-measures",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/parental-responsibility-measures#official-statistics")
                },
                new Publication
                {
                    Id = new Guid("aa545525-9ffe-496c-a5b3-974ace56746e"),
                    Title = "National pupil projections",
                    Summary = "",
                    TopicId = new Guid("5e196d11-8ac4-4c82-8c46-a10a67c1118e"),
                    Slug = "national-pupil-projections",
                    LegacyPublicationUrl =
                        new Uri("https://www.gov.uk/government/collections/statistics-pupil-projections#documents")
                },
                new Publication
                {
                    Id = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                    Title = "Schools, pupils and their characteristics",
                    Summary = "",
                    TopicId = new Guid("e50ba9fd-9f19-458c-aceb-4422f0c7d1ba"),
                    Slug = "school-pupils-and-their-characteristics",
                    LegacyPublicationUrl =
                        new Uri("https://www.gov.uk/government/collections/statistics-school-and-pupil-numbers")
                },
                new Publication
                {
                    Id = new Guid("66c8e9db-8bf2-4b0b-b094-cfab25c20b05"),
                    MethodologyId = new Guid("8ab41234-cc9d-4b3d-a42c-c9fce7762719"),
                    
                    Title = "Secondary and primary schools applications and offers",
                    Summary = "",
                    TopicId = new Guid("1a9636e4-29d5-4c90-8c07-f41db8dd019c"),
                    Slug = "secondary-and-primary-schools-applications-and-offers",
                    NextUpdate = new DateTime(2019, 6, 14),
                    ContactId = new Guid("74f5aade-6d24-4a0b-be23-2ab4b4b2d191")
                },
                new Publication
                {
                    Id = new Guid("fa591a15-ae37-41b5-98f6-4ce06e5225f4"),
                    Title = "School capacity",
                    Summary = "",
                    TopicId = new Guid("87c27c5e-ae49-4932-aedd-4405177d9367"),
                    Slug = "school-capacity",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-school-capacity#school-capacity-data:-by-academic-year")
                },
                new Publication
                {
                    Id = new Guid("f657afb4-8f4a-427d-a683-15f11a2aefb5"),
                    Title = "Special educational needs in England",
                    Summary = "",
                    TopicId = new Guid("85349b0a-19c7-4089-a56b-ad8dbe85449a"),
                    Slug = "special-educational-needs-in-england",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-special-educational-needs-sen#national-statistics-on-special-educational-needs-in-england")
                },
                new Publication
                {
                    Id = new Guid("30874b87-483a-427e-8916-43cf9020d9a1"),
                    Title = "Special educational needs: analysis and summary of data sources",
                    Summary = "",
                    TopicId = new Guid("85349b0a-19c7-4089-a56b-ad8dbe85449a"),
                    Slug = "special-educational-needs-analysis-and-summary-of-data-sources",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-special-educational-needs-sen#analysis-of-children-with-special-educational-needs")
                },
                new Publication
                {
                    Id = new Guid("88312cc0-fe1d-4ab5-81df-33fd708185cb"),
                    Title = "Statements on SEN and EHC plans",
                    Summary = "",
                    TopicId = new Guid("85349b0a-19c7-4089-a56b-ad8dbe85449a"),
                    Slug = "statements-on-sen-and-ehc-plans",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-special-educational-needs-sen#statements-of-special-educational-needs-(sen)-and-education,-health-and-care-(ehc)-plans")
                },
                new Publication
                {
                    Id = new Guid("1b2fb05c-eb2c-486b-80be-ebd772eda4f1"),
                    Title = "16 to 18 school and college performance tables",
                    Summary = "",
                    TopicId = new Guid("85b5454b-3761-43b1-8e84-bd056a8efcd3"),
                    Slug = "16-to-18-school-and-college-performance-tables",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-attainment-at-19-years#16-to-18-school-and-college-performance-tables")
                },
                new Publication
                {
                    Id = new Guid("3f3a66ec-5777-42ee-b427-8102a14ce0c5"),
                    Title = "A level and other 16 to 18 results",
                    Summary = "",
                    TopicId = new Guid("85b5454b-3761-43b1-8e84-bd056a8efcd3"),
                    Slug = "a-level-and-other-16-to-18-results",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-attainment-at-19-years#a-levels-and-other-16-to-18-results")
                },
                new Publication
                {
                    Id = new Guid("2e95f880-629c-417b-981f-0901e97776ff"),
                    Title = "Level 2 and 3 attainment by young people aged 19",
                    Summary = "",
                    TopicId = new Guid("85b5454b-3761-43b1-8e84-bd056a8efcd3"),
                    Slug = "level-2-and-3-attainment-by-young-people-aged-19",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-attainment-at-19-years#level-2-and-3-attainment")
                },
                new Publication
                {
                    Id = new Guid("bfdcaae1-ce6b-4f63-9b2b-0a1f3942887f"),
                    Title = "GCSE and equivalent results",
                    Summary = "",
                    TopicId = new Guid("1e763f55-bf09-4497-b838-7c5b054ba87b"),
                    Slug = "gcse-and-equivalent-results",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-gcses-key-stage-4#gcse-and-equivalent-results")
                },
                new Publication
                {
                    Id = new Guid("1d0e4263-3d70-433e-bd95-f29754db5888"),
                    Title = "Multi-academy trust performance measures",
                    Summary = "",
                    TopicId = new Guid("1e763f55-bf09-4497-b838-7c5b054ba87b"),
                    Slug = "multi-academy-trust-performance-measures",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-gcses-key-stage-4#multi-academy-trust-performance-measures")
                },
                new Publication
                {
                    Id = new Guid("c8756008-ed50-4632-9b96-01b5ca002a43"),
                    Title = "Revised GCSE and equivalent results in England",
                    Summary = "",
                    TopicId = new Guid("1e763f55-bf09-4497-b838-7c5b054ba87b"),
                    Slug = "revised-gcse-and-equivalent-results-in-england",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-gcses-key-stage-4#gcse-and-equivalent-results,-including-pupil-characteristics")
                },
                new Publication
                {
                    Id = new Guid("9e7e9d5c-b761-43a4-9685-4892392200b7"),
                    Title = "Secondary school performance tables",
                    Summary = "",
                    TopicId = new Guid("1e763f55-bf09-4497-b838-7c5b054ba87b"),
                    Slug = "secondary-school-performance-tables",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-gcses-key-stage-4#secondary-school-performance-tables")
                },
                new Publication
                {
                    Id = new Guid("441a13f6-877c-4f18-828f-119dbd401a5b"),
                    Title = "Phonics screening check and key stage 1 assessments",
                    Summary = "",
                    TopicId = new Guid("504446c2-ddb1-4d52-bdbc-4148c2c4c460"),
                    Slug = "phonics-screening-check-and-ks1-assessments",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-key-stage-1#phonics-screening-check-and-key-stage-1-assessment")
                },
                new Publication
                {
                    Id = new Guid("7ecea655-7b22-4832-b697-26e86769399a"),
                    Title = "Key stage 2 national curriculum test:review outcomes",
                    Summary = "",
                    TopicId = new Guid("eac38700-b968-4029-b8ac-0eb8e1356480"),
                    Slug = "ks2-national-curriculum-test-review-outcomes",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-key-stage-2#key-stage-2-national-curriculum-tests:-review-outcomes")
                },
                new Publication
                {
                    Id = new Guid("eab51107-4ef0-4926-8f8b-c8bd7f5a21d5"),
                    Title = "Multi-academy trust performance measures",
                    Summary = "",
                    TopicId = new Guid("eac38700-b968-4029-b8ac-0eb8e1356480"),
                    Slug = "multi-academy-trust-performance-measures",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-key-stage-2#national-curriculum-assessments-at-key-stage-2")
                },
                new Publication
                {
                    Id = new Guid("10370062-93b0-4dde-9097-5a56bf5b3064"),
                    Title = "National curriculum assessments at key stage 2",
                    Summary = "",
                    TopicId = new Guid("eac38700-b968-4029-b8ac-0eb8e1356480"),
                    Slug = "national-curriculum-assessments-at-ks2",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-key-stage-2#national-curriculum-assessments-at-key-stage-2")
                },
                new Publication
                {
                    Id = new Guid("2434335f-f8e1-41fb-8d6e-4a11bc62b14a"),
                    Title = "Primary school performance tables",
                    Summary = "",
                    TopicId = new Guid("eac38700-b968-4029-b8ac-0eb8e1356480"),
                    Slug = "primary-school-performance-tables",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-key-stage-2#primary-school-performance-tables")
                },
                new Publication
                {
                    Id = new Guid("8b12776b-3d36-4475-8115-00974d7de1d0"),
                    Title = "Further education outcome-based success measures",
                    Summary = "",
                    TopicId = new Guid("a7ce9542-20e6-401d-91f4-f832c9e58b12"),
                    Slug = "further-education-outcome-based-success-measures",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-outcome-based-success-measures#statistics")
                },
                new Publication
                {
                    Id = new Guid("bddcd4b8-db0d-446c-b6e9-03d4230c6927"),
                    Title = "Primary school performance tables",
                    Summary = "",
                    TopicId = new Guid("1318eb73-02a8-4e50-82a9-7e271176c4d1"),
                    Slug = "primary-school-performance-tables-2",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-performance-tables#primary-school-(key-stage-2)")
                },
                new Publication
                {
                    Id = new Guid("263e10d2-b9c3-4e90-a6aa-b52b86de1f5f"),
                    Title = "School and college performance tables",
                    Summary = "",
                    TopicId = new Guid("1318eb73-02a8-4e50-82a9-7e271176c4d1"),
                    Slug = "school-and-college-performance-tables",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-performance-tables#school-and-college:-post-16-(key-stage-5)")
                },
                new Publication
                {
                    Id = new Guid("28aabfd4-a3fb-45e1-bb34-21ca3b7d1aec"),
                    Title = "Secondary school performance tables",
                    Summary = "",
                    TopicId = new Guid("1318eb73-02a8-4e50-82a9-7e271176c4d1"),
                    Slug = "secondary-school-performance-tables",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-performance-tables#secondary-school-(key-stage-4)")
                },
                new Publication
                {
                    Id = new Guid("d34978d5-0317-46bc-9258-13412270ac4d"),
                    Title = "Initial teacher training performance profiles",
                    Summary = "",
                    TopicId = new Guid("0f8792d2-28b1-4537-a1b4-3e139fcf0ca7"),
                    Slug = "initial-teacher-training-performance-profiles",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-teacher-training#performance-data")
                },
                new Publication
                {
                    Id = new Guid("9cc08298-7370-499f-919a-7d203ba21415"),
                    Title = "Initial teacher training: trainee number census",
                    Summary = "",
                    TopicId = new Guid("0f8792d2-28b1-4537-a1b4-3e139fcf0ca7"),
                    Slug = "initial-teacher-training-trainee-number-census",
                    LegacyPublicationUrl =
                        new Uri("https://www.gov.uk/government/collections/statistics-teacher-training#census-data")
                },
                new Publication
                {
                    Id = new Guid("3ceb43d0-e705-4cb9-aeb9-cb8638fcbf3d"),
                    Title = "TSM and initial teacher training allocations",
                    Summary = "",
                    TopicId = new Guid("0f8792d2-28b1-4537-a1b4-3e139fcf0ca7"),
                    Slug = "tsm-and-initial-teacher-training-allocations",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/statistics-teacher-training#teacher-supply-model-and-itt-allocations")
                },
                new Publication
                {
                    Id = new Guid("b318967f-2931-472a-93f2-fbed1e181e6a"),
                    Title = "School workforce in England",
                    Summary = "",
                    TopicId = new Guid("28cfa002-83cb-4011-9ddd-859ec99e0aa0"),
                    Slug = "school-workforce-in-england",
                    LegacyPublicationUrl =
                        new Uri("https://www.gov.uk/government/collections/statistics-school-workforce#documents")
                },
                new Publication
                {
                    Id = new Guid("d0b47c96-d7de-4d80-9ff7-3bff135d2636"),
                    Title = "Teacher analysis compendium",
                    Summary = "",
                    TopicId = new Guid("6d434e17-7b76-425d-897d-c7b369b42e35"),
                    Slug = "teacher-analysis-compendium",
                    LegacyPublicationUrl =
                        new Uri(
                            "https://www.gov.uk/government/collections/teacher-workforce-statistics-and-analysis#documents")
                },
                new Publication
                {
                    Id = new Guid("2ffbc8d3-eb53-4c4b-a6fb-219a5b95ebc8"),
                    Title = "Education and training statistics for the UK",
                    Summary = "",
                    TopicId = new Guid("692050da-9ac9-435a-80d5-a6be4915f0f7"),
                    Slug = "education-and-training-statistics-for-the-uk",
                    LegacyPublicationUrl =
                        new Uri("https://www.gov.uk/government/collections/statistics-education-and-training#documents")
                }
            );

            modelBuilder.Entity<ReleaseSummary>().HasData(
                    new ReleaseSummary
                    {
                        ReleaseId = new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                        Id = new Guid("1bf7c51f-4d12-4697-8868-455760a887a7")
                    }
                    ,
                    new ReleaseSummary
                    {
                        ReleaseId = new Guid("f75bc75e-ae58-4bc4-9b14-305ad5e4ff7d"),
                        Id = new Guid("51eb730b-d76c-4a0c-aaf2-cf7aa96f133a"),
                    },
                    new ReleaseSummary
                    {
                        ReleaseId = new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                        Id = new Guid("06c45b1e-533d-4c95-900b-62beb4620f59"),
                    },
                    new ReleaseSummary
                    {
                        ReleaseId = new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"),
                        Id = new Guid("c6e08ed3-d93a-410a-9e7e-600f2cf25725"),
                    }
                )
                ;

            modelBuilder.Entity<ReleaseSummaryVersion>().HasData(
                new ReleaseSummaryVersion
                {
                    Created = new DateTime(2018, 1, 1),
                    Id = new Guid("420ca58e-278b-456b-9031-fe74a6966159"),
                    Slug = "2016-17",
                    ReleaseName = "2016",
                    Summary =
                        "Read national statistical summaries, view charts and tables and download data files.\n\n" +
                        "Find out how and why these statistics are collected and published - [Pupil absence statistics: methodology](../methodology/pupil-absence-in-schools-in-england).",
                    TimePeriodCoverage = TimeIdentifier.AcademicYear,
                    TypeId = new Guid("9d333457-9132-4e55-ae78-c55cb3673d7c"),
                    ReleaseSummaryId = new Guid("1bf7c51f-4d12-4697-8868-455760a887a7")
                },
                new ReleaseSummaryVersion
                {
                    Id = new Guid("fe5e8cac-a574-4e83-861b-7b5f927d7d34"),
                    Created = new DateTime(2016, 1, 1),
                    ReleaseName = "2015",
                    Slug = "2015-16",
                    Summary =
                        "Read national statistical summaries and definitions, view charts and tables and download data files across a range of pupil absence subject areas.",
                    TypeId = new Guid("9d333457-9132-4e55-ae78-c55cb3673d7c"),
                    TimePeriodCoverage = TimeIdentifier.AcademicYear,
                    ReleaseSummaryId = new Guid("51eb730b-d76c-4a0c-aaf2-cf7aa96f133a"),
                },
                new ReleaseSummaryVersion
                {
                    Id = new Guid("04adfe47-9057-4abd-a0e8-5a6ac56e1560"),
                    Created = new DateTime(2018, 1, 1),
                    ReleaseName = "2016",
                    Slug = "2016-17",
                    Summary =
                        "Read national statistical summaries, view charts and tables and download data files.\n\n" +
                        "Find out how and why these statistics are collected and published - [Permanent and fixed-period exclusion statistics: methodology](../methodology/permanent-and-fixed-period-exclusions-in-england)",
                    TypeId = new Guid("9d333457-9132-4e55-ae78-c55cb3673d7c"),
                    TimePeriodCoverage = TimeIdentifier.AcademicYear,
                    ReleaseSummaryId = new Guid("06c45b1e-533d-4c95-900b-62beb4620f59"),
                },
                new ReleaseSummaryVersion
                {
                    Id = new Guid("c6e08ed3-d93a-410a-9e7e-600f2cf25725"),
                    Created = new DateTime(2018, 1, 1),
                    ReleaseName = "2018",
                    Slug = "2018",
                    Summary =
                        "Read national statistical summaries, view charts and tables and download data files.\n\n" +
                        "Find out how and why these statistics are collected and published - [Secondary and primary school applications and offers: methodology](../methodology/secondary-and-primary-schools-applications-and-offers)",
                    TimePeriodCoverage = TimeIdentifier.AcademicYear,
                    TypeId = new Guid("9d333457-9132-4e55-ae78-c55cb3673d7c"),
                    ReleaseSummaryId = new Guid("c6e08ed3-d93a-410a-9e7e-600f2cf25725"),
                });
             
            
            modelBuilder.Entity<Release>().HasData(
                //absence
                new Release
                {
                    Id = new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                    ReleaseName = "2016",
                    PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                    Published = new DateTime(2018, 3, 22),
                    Slug = "2016-17",
                    Summary =
                        "Read national statistical summaries, view charts and tables and download data files.",
                    TimePeriodCoverage = TimeIdentifier.AcademicYear,
                    TypeId = new Guid("9d333457-9132-4e55-ae78-c55cb3673d7c"),
                    KeyStatisticsId = new Guid("5d1e6b67-26d7-4440-9e77-c0de71a9fc21")
                },

                // exclusions
                new Release
                {
                    Id = new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                    ReleaseName = "2016",
                    PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                    Published = new DateTime(2018, 7, 19),
                    Slug = "2016-17",
                    Summary =
                        "Read national statistical summaries, view charts and tables and download data files.",
                    TimePeriodCoverage = TimeIdentifier.AcademicYear,
                    RelatedInformation = new List<BasicLink> {
                        new BasicLink {
                            Id = new Guid("f3c67bc9-6132-496e-a848-c39dfcd16f49"),
                            Description = "Additional guidance",
                            Url = "http://example.com"
                        },
                        new BasicLink {
                            Id = new Guid("45acb50c-8b21-46b4-989f-36f4b0ee37fb"),
                            Description = "Statistics guide",
                            Url = "http://example.com"
                        }
                    },
                    TypeId = new Guid("9d333457-9132-4e55-ae78-c55cb3673d7c"),
                    KeyStatisticsId = new Guid("17a0272b-318d-41f6-bda9-3bd88f78cd3d")
                },
                
                // Secondary and primary schools applications offers
                new Release
                {
                    Id = new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"),
                    ReleaseName = "2018",
                    PublicationId = new Guid("66c8e9db-8bf2-4b0b-b094-cfab25c20b05"),
                    Published = new DateTime(2018, 6, 14),
                    Slug = "2018",
                    Summary =
                        "Read national statistical summaries, view charts and tables and download data files.",
                    TimePeriodCoverage = TimeIdentifier.AcademicYear,
                    TypeId = new Guid("9d333457-9132-4e55-ae78-c55cb3673d7c"),
                    KeyStatisticsId = new Guid("475738b4-ba10-4c29-a50d-6ca82c10de6e")
                }
            );

            modelBuilder.Entity<ContentSection>().HasData(
                // absence
                new ContentSection
                {
                    Id = new Guid("24c6e9a3-1415-4ca5-9f21-b6b51cb7ba94"),
                    ReleaseId = new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                    Order = 1, Heading = "About these statistics", Caption = ""
                },
                new ContentSection
                {
                    Id = new Guid("8965ef44-5ad7-4ab0-a142-78453d6f40af"),
                    ReleaseId = new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                    Order = 2, Heading = "Pupil absence rates", Caption = ""
                },
                new ContentSection
                {
                    Id = new Guid("6f493eee-443a-4403-9069-fef82e2f5788"),
                    ReleaseId = new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                    Order = 3, Heading = "Persistent absence", Caption = ""
                },
                new ContentSection
                {
                    Id = new Guid("fbf99442-3b72-46bc-836d-8866c552c53d"),
                    ReleaseId = new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                    Order = 4, Heading = "Reasons for absence", Caption = ""
                },
                new ContentSection
                {
                    Id = new Guid("6898538c-3f8d-488d-9e50-12ca7a9fd70c"),
                    ReleaseId = new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                    Order = 5, Heading = "Distribution of absence", Caption = ""
                },
                new ContentSection
                {
                    Id = new Guid("08b204a2-0eeb-4797-9e0b-a1274e7f6a38"),
                    ReleaseId = new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                    Order = 6, Heading = "Absence by pupil characteristics", Caption = ""
                },
                new ContentSection
                {
                    Id = new Guid("60f8c7ca-faff-4f0d-937d-17fe376461cf"),
                    ReleaseId = new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                    Order = 7, Heading = "Absence for 4-year-olds", Caption = ""
                },
                new ContentSection
                {
                    Id = new Guid("d5d604af-6b63-4a51-b106-0c09b8dbedfa"),
                    ReleaseId = new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                    Order = 8, Heading = "Pupil referral unit absence", Caption = ""
                },
                new ContentSection
                {
                    Id = new Guid("68e3028c-1291-42b3-9e7c-9be285dac9a1"),
                    ReleaseId = new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                    Order = 9, Heading = "Regional and local authority (LA) breakdown", Caption = ""
                },

                // exclusions
                new ContentSection
                {
                    Id = new Guid("b7a968ab-eb49-4100-b133-3d9d94f23d60"),
                    ReleaseId = new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                    Order = 1, Heading = "About this release", Caption = ""
                },
                new ContentSection
                {
                    Id = new Guid("6ed87fd1-81a5-46dc-8841-4598bdae7fee"),
                    ReleaseId = new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                    Order = 2, Heading = "Permanent exclusions", Caption = ""
                },
                new ContentSection
                {
                    Id = new Guid("7981db34-afdb-4f84-99e8-bfd43e58f16d"),
                    ReleaseId = new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                    Order = 3, Heading = "Fixed-period exclusions", Caption = ""
                },
                new ContentSection
                {
                    Id = new Guid("50e7ca4c-e6c7-4ccd-afc1-93ee4298f358"),
                    ReleaseId = new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                    Order = 4, Heading = "Number and length of fixed-period exclusions", Caption = ""
                },
                new ContentSection
                {
                    Id = new Guid("015d0cdd-6630-4b57-9ef3-7341fc3d573e"),
                    ReleaseId = new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                    Order = 5, Heading = "Reasons for exclusions", Caption = ""
                },
                new ContentSection
                {
                    Id = new Guid("5600ca55-6800-418a-94a5-2f3c3310304e"),
                    ReleaseId = new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                    Order = 6, Heading = "Exclusions by pupil characteristics", Caption = ""
                },
                new ContentSection
                {
                    Id = new Guid("68f8b290-4b7c-4cac-b0d9-0263609c341b"),
                    ReleaseId = new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                    Order = 7, Heading = "Independent exclusion reviews", Caption = ""
                },
                new ContentSection
                {
                    Id = new Guid("5708d443-7669-47d8-b6a3-6ad851090710"),
                    ReleaseId = new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                    Order = 8, Heading = "Pupil referral units exclusions", Caption = ""
                },
                new ContentSection
                {
                    Id = new Guid("3960ab94-0fad-442c-8aaa-6233eff3bc32"),
                    ReleaseId = new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                    Order = 9, Heading = "Regional and local authority (LA) breakdown", Caption = ""
                },

                // Secondary and primary schools applications offers
                new ContentSection
                {
                    Id = new Guid("def347bd-0b29-405f-a11f-cd03c853a6ed"),
                    ReleaseId = new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"),
                    Order = 1, Heading = "About this release", Caption = ""
                },
                new ContentSection
                {
                    Id = new Guid("6bfa9b19-25d6-4d45-8008-9447db541795"),
                    ReleaseId = new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"),
                    Order = 2, Heading = "Secondary applications and offers", Caption = ""
                },
                new ContentSection
                {
                    Id = new Guid("c1f17b4e-f576-40bc-80e1-63767998d080"),
                    ReleaseId = new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"),
                    Order = 3, Heading = "Secondary geographical variation", Caption = ""
                },
                new ContentSection
                {
                    Id = new Guid("c3eb66d0-ce13-4e68-861d-98bb914d0814"),
                    ReleaseId = new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"),
                    Order = 4, Heading = "Primary applications and offers", Caption = ""
                },
                new ContentSection
                {
                    Id = new Guid("b87f2e62-e3e7-4492-9d68-18df8dc29041"),
                    ReleaseId = new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"),
                    Order = 5, Heading = "Primary geographical variation", Caption = ""
                }
            );

            modelBuilder.Entity<MarkDownBlock>().HasData(
                // absence
                new MarkDownBlock
                {
                    Id = new Guid("7eeb1478-ab26-4b70-9128-b976429efa2f"),
                    ContentSectionId = new Guid("24c6e9a3-1415-4ca5-9f21-b6b51cb7ba94"),
                    Body = SampleMarkDownContent.Content[new Guid("7eeb1478-ab26-4b70-9128-b976429efa2f")]
                },
                new MarkDownBlock
                {
                    Id = new Guid("2c369594-3bbc-40b4-ad19-196c923f5c7f"),
                    ContentSectionId = new Guid("8965ef44-5ad7-4ab0-a142-78453d6f40af"),
                    Body = SampleMarkDownContent.Content[new Guid("2c369594-3bbc-40b4-ad19-196c923f5c7f")]
                },
                new MarkDownBlock
                {
                    Id = new Guid("3913a0af-9455-4802-a037-c4cfd4719b18"),
                    ContentSectionId = new Guid("8965ef44-5ad7-4ab0-a142-78453d6f40af"),
                    Body = SampleMarkDownContent.Content[new Guid("3913a0af-9455-4802-a037-c4cfd4719b18")]
                },
                new MarkDownBlock
                {
                    Id = new Guid("8a8add13-368c-4067-9210-166bb19a00c1"),
                    ContentSectionId = new Guid("6f493eee-443a-4403-9069-fef82e2f5788"),
                    Body = SampleMarkDownContent.Content[new Guid("8a8add13-368c-4067-9210-166bb19a00c1")]
                },
                new MarkDownBlock
                {
                    Id = new Guid("4aa06200-406b-4f5a-bee4-19e3b83eb1d2"),
                    ContentSectionId = new Guid("6f493eee-443a-4403-9069-fef82e2f5788"),
                    Body = SampleMarkDownContent.Content[new Guid("4aa06200-406b-4f5a-bee4-19e3b83eb1d2")]
                },
                new MarkDownBlock
                {
                    Id = new Guid("33c3a82e-7d8d-47fc-9019-2fe5344ec32d"),
                    ContentSectionId = new Guid("fbf99442-3b72-46bc-836d-8866c552c53d"),
                    Body = SampleMarkDownContent.Content[new Guid("33c3a82e-7d8d-47fc-9019-2fe5344ec32d")]
                },
                new MarkDownBlock
                {
                    Id = new Guid("2ef5f84f-e151-425d-8906-2921712f9157"),
                    ContentSectionId = new Guid("fbf99442-3b72-46bc-836d-8866c552c53d"),
                    Body = SampleMarkDownContent.Content[new Guid("2ef5f84f-e151-425d-8906-2921712f9157")]
                },
                new MarkDownBlock
                {
                    Id = new Guid("cf01208f-cbab-41d1-9fa5-4793d2a0bc13"),
                    ContentSectionId = new Guid("6898538c-3f8d-488d-9e50-12ca7a9fd70c"),
                    Body = SampleMarkDownContent.Content[new Guid("cf01208f-cbab-41d1-9fa5-4793d2a0bc13")]
                },
                new MarkDownBlock
                {
                    Id = new Guid("eb4318f9-11e0-46ea-9796-c36a9dc25014"),
                    ContentSectionId = new Guid("08b204a2-0eeb-4797-9e0b-a1274e7f6a38"),
                    Body = SampleMarkDownContent.Content[new Guid("eb4318f9-11e0-46ea-9796-c36a9dc25014")]
                },
                new MarkDownBlock
                {
                    Id = new Guid("97c54e5f-2406-4333-851d-b6c9cc4bf612"),
                    ContentSectionId = new Guid("60f8c7ca-faff-4f0d-937d-17fe376461cf"),
                    Body = SampleMarkDownContent.Content[new Guid("97c54e5f-2406-4333-851d-b6c9cc4bf612")]
                },
                new MarkDownBlock
                {
                    Id = new Guid("3aaafa20-bc32-4523-bb23-dd55c458f928"),
                    ContentSectionId = new Guid("d5d604af-6b63-4a51-b106-0c09b8dbedfa"),
                    Body = SampleMarkDownContent.Content[new Guid("3aaafa20-bc32-4523-bb23-dd55c458f928")]
                },
                new MarkDownBlock
                {
                    Id = new Guid("7d97f8ed-e1d0-4244-bec3-3432af356f57"),
                    ContentSectionId = new Guid("68e3028c-1291-42b3-9e7c-9be285dac9a1"),
                    Body = SampleMarkDownContent.Content[new Guid("7d97f8ed-e1d0-4244-bec3-3432af356f57")]
                },

                // exclusions
                new MarkDownBlock
                {
                    Id = new Guid("97d414f4-1a27-4ed7-85ea-c4c903e1d8cb"),
                    ContentSectionId = new Guid("b7a968ab-eb49-4100-b133-3d9d94f23d60"),
                    Body = SampleMarkDownContent.Content[new Guid("97d414f4-1a27-4ed7-85ea-c4c903e1d8cb")]
                },
                new MarkDownBlock
                {
                    Id = new Guid("70546a7d-5edd-4b8f-b096-cfd50153f4cb"),
                    ContentSectionId = new Guid("6ed87fd1-81a5-46dc-8841-4598bdae7fee"),
                    Body = SampleMarkDownContent.Content[new Guid("70546a7d-5edd-4b8f-b096-cfd50153f4cb")]
                },
                new MarkDownBlock
                {
                    Id = new Guid("81d8eba2-9cba-4b04-bb02-e00ace5c4418"),
                    ContentSectionId = new Guid("6ed87fd1-81a5-46dc-8841-4598bdae7fee"),
                    Body = SampleMarkDownContent.Content[new Guid("81d8eba2-9cba-4b04-bb02-e00ace5c4418")]
                },
                new MarkDownBlock
                {
                    Id = new Guid("7971329a-9e16-468b-9eb3-62bfc384b5a3"),
                    ContentSectionId = new Guid("7981db34-afdb-4f84-99e8-bfd43e58f16d"),
                    Body = SampleMarkDownContent.Content[new Guid("7971329a-9e16-468b-9eb3-62bfc384b5a3")]
                },
                new MarkDownBlock
                {
                    Id = new Guid("e9462ed0-10dc-4ff5-8cda-f8c3b66f2714"),
                    ContentSectionId = new Guid("7981db34-afdb-4f84-99e8-bfd43e58f16d"),
                    Body = SampleMarkDownContent.Content[new Guid("e9462ed0-10dc-4ff5-8cda-f8c3b66f2714")]
                },
                new MarkDownBlock
                {
                    Id = new Guid("4e05bbb3-bd4e-4602-8424-069e59034c87"),
                    ContentSectionId = new Guid("50e7ca4c-e6c7-4ccd-afc1-93ee4298f358"),
                    Body = SampleMarkDownContent.Content[new Guid("4e05bbb3-bd4e-4602-8424-069e59034c87")]
                },
                new MarkDownBlock
                {
                    Id = new Guid("99d75d39-7ea5-456e-979d-1215fa673a83"),
                    ContentSectionId = new Guid("015d0cdd-6630-4b57-9ef3-7341fc3d573e"),
                    Body = SampleMarkDownContent.Content[new Guid("99d75d39-7ea5-456e-979d-1215fa673a83")]
                },
                new MarkDownBlock
                {
                    Id = new Guid("c73382ce-73ff-465f-8f1b-7a08cb6af089"),
                    ContentSectionId = new Guid("5600ca55-6800-418a-94a5-2f3c3310304e"),
                    Body = SampleMarkDownContent.Content[new Guid("c73382ce-73ff-465f-8f1b-7a08cb6af089")]
                },
                new MarkDownBlock
                {
                    Id = new Guid("d988a5e8-4e3c-4c1d-b5a9-bf0e1d947085"),
                    ContentSectionId = new Guid("68f8b290-4b7c-4cac-b0d9-0263609c341b"),
                    Body = SampleMarkDownContent.Content[new Guid("d988a5e8-4e3c-4c1d-b5a9-bf0e1d947085")]
                },
                new MarkDownBlock
                {
                    Id = new Guid("d3288340-2689-4346-91a6-c070e7b0799d"),
                    ContentSectionId = new Guid("5708d443-7669-47d8-b6a3-6ad851090710"),
                    Body = SampleMarkDownContent.Content[new Guid("d3288340-2689-4346-91a6-c070e7b0799d")]
                },
                new MarkDownBlock
                {
                    Id = new Guid("1a1d29f6-c4d5-41a9-9a06-b2ce84043edd"),
                    ContentSectionId = new Guid("3960ab94-0fad-442c-8aaa-6233eff3bc32"),
                    Body = SampleMarkDownContent.Content[new Guid("1a1d29f6-c4d5-41a9-9a06-b2ce84043edd")]
                },

                // Secondary and primary schools applications offers
                new MarkDownBlock
                {
                    Id = new Guid("49aa2ac2-1b65-4c25-9828-fec65a5ed7e8"),
                    ContentSectionId = new Guid("def347bd-0b29-405f-a11f-cd03c853a6ed"),
                    Body = SampleMarkDownContent.Content[new Guid("49aa2ac2-1b65-4c25-9828-fec65a5ed7e8")]
                },
                new MarkDownBlock
                {
                    Id = new Guid("13e4577a-2291-4ce4-a8c9-6c76baa06092"),
                    ContentSectionId = new Guid("6bfa9b19-25d6-4d45-8008-9447db541795"),
                    Body = SampleMarkDownContent.Content[new Guid("13e4577a-2291-4ce4-a8c9-6c76baa06092")]
                },
                new MarkDownBlock
                {
                    Id = new Guid("8510640f-d8b6-4fe2-a161-d025e14930a4"),
                    ContentSectionId = new Guid("c1f17b4e-f576-40bc-80e1-63767998d080"),
                    Body = SampleMarkDownContent.Content[new Guid("8510640f-d8b6-4fe2-a161-d025e14930a4")]
                },
                new MarkDownBlock
                {
                    Id = new Guid("87f5343b-b7a5-4775-b483-d1668fac03fb"),
                    ContentSectionId = new Guid("c1f17b4e-f576-40bc-80e1-63767998d080"),
                    Body = SampleMarkDownContent.Content[new Guid("87f5343b-b7a5-4775-b483-d1668fac03fb")]
                },
                new MarkDownBlock
                {
                    Id = new Guid("5f194c52-0ffb-4205-8c03-068ff4d1384b"),
                    ContentSectionId = new Guid("c3eb66d0-ce13-4e68-861d-98bb914d0814"),
                    Body = SampleMarkDownContent.Content[new Guid("5f194c52-0ffb-4205-8c03-068ff4d1384b")]
                },
                new MarkDownBlock
                {
                    Id = new Guid("e4497a91-3e3b-460a-8965-42eab5e06ce5"),
                    ContentSectionId = new Guid("b87f2e62-e3e7-4492-9d68-18df8dc29041"),
                    Body = SampleMarkDownContent.Content[new Guid("e4497a91-3e3b-460a-8965-42eab5e06ce5")]
                },
                new MarkDownBlock
                {
                    Id = new Guid("8e10ad6c-9a68-4162-84f9-81fb6dc93ae3"),
                    ContentSectionId = new Guid("b87f2e62-e3e7-4492-9d68-18df8dc29041"),
                    Body = SampleMarkDownContent.Content[new Guid("8e10ad6c-9a68-4162-84f9-81fb6dc93ae3")]
                }
            );

            modelBuilder.Entity<DataBlock>().HasData(
                // absence
                new DataBlock
                {
                    Id = new Guid("5d1e6b67-26d7-4440-9e77-c0de71a9fc21"),
                    ContentSectionId = null, //KeyStatistics
                    DataBlockRequest = new DataBlockRequest
                    {
                        SubjectId = 1,
                        GeographicLevel = GeographicLevel.Country,
                        TimePeriod = new TimePeriod
                        {
                            StartYear = "2012",
                            StartCode = TimeIdentifier.AcademicYear,
                            EndYear = "2016",
                            EndCode = TimeIdentifier.AcademicYear
                        },
                        Filters = new List<string>
                        {
                            FItem(1, FilterItemName.Characteristic__Total),
                            FItem(1, FilterItemName.School_Type__Total)
                        },
                        Indicators = new List<string>
                        {
                            Indicator(1, IndicatorName.Unauthorised_absence_rate),
                            Indicator(1, IndicatorName.Overall_absence_rate),
                            Indicator(1, IndicatorName.Authorised_absence_rate)
                        }
                    },

                    Summary = new Summary
                    {
                        dataKeys = new List<string>
                        {
                            Indicator(1, IndicatorName.Overall_absence_rate),
                            Indicator(1, IndicatorName.Authorised_absence_rate),
                            Indicator(1, IndicatorName.Unauthorised_absence_rate)
                        },
                        dataSummary = new List<string>
                        {
                            "Up from 4.6% in 2015/16",
                            "Similar to previous years",
                            "Up from 1.1% in 2015/16"
                        },
                        dataDefinition = new List<string>
                        {
                            @"Total number of all authorised and unauthorised absences from possible school sessions for all pupils. <a href=""/glossary#overall-absence"">More >>></a>",
                            @"Number of authorised absences as a percentage of the overall school population. <a href=""/glossary#authorised-absence"">More >>></a>",
                            @"Number of unauthorised absences as a percentage of the overall school population. <a href=""/glossary#unauthorised-absence"">More >>></a>"
                        },
                        description = new MarkDownBlock
                        {
                            Id = new Guid("f928762e-9bd5-4538-a4f0-d7f34b2874e6"),
                            Body = " * pupils missed on average 8.2 school days\n" +
                                   " * overall and unauthorised absence rates up on 2015/16\n" +
                                   " * unauthorised absence rise due to higher rates of unauthorised holidays\n" +
                                   " * 10% of pupils persistently absent during 2016/17"
                        }
                    },
                    Tables = new List<Table>
                    {
                        new Table
                        {
                            indicators = new List<string>
                            {
                                Indicator(1, IndicatorName.Unauthorised_absence_rate),
                                Indicator(1, IndicatorName.Overall_absence_rate),
                                Indicator(1, IndicatorName.Authorised_absence_rate)
                            }
                        }
                    },
                    Charts = new List<IContentBlockChart>
                    {
                        new LineChart
                        {
                            Axes = new Dictionary<string, AxisConfigurationItem>
                            {
                                ["major"] = new AxisConfigurationItem
                                {
                                    GroupBy = AxisGroupBy.timePeriod,
                                    DataSets = new List<ChartDataSet>
                                    {
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(1, IndicatorName.Unauthorised_absence_rate),
                                            Filters = new List<string>
                                            {
                                                FItem(1, FilterItemName.Characteristic__Total),
                                                FItem(1, FilterItemName.School_Type__Total)
                                            }
                                        },
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(1, IndicatorName.Overall_absence_rate),
                                            Filters = new List<string>
                                            {
                                                FItem(1, FilterItemName.Characteristic__Total),
                                                FItem(1, FilterItemName.School_Type__Total)
                                            }
                                        },
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(1, IndicatorName.Authorised_absence_rate),
                                            Filters = new List<string>
                                            {
                                                FItem(1, FilterItemName.Characteristic__Total),
                                                FItem(1, FilterItemName.School_Type__Total)
                                            }
                                        }
                                    },
                                    Title = "School Year"
                                },
                                ["minor"] = new AxisConfigurationItem
                                {
                                    Min = 0,
                                    Title = "Absence Rate"
                                }
                            },
                            Labels = new Dictionary<string, ChartConfiguration>
                            {
                                [$"{Indicator(1, IndicatorName.Unauthorised_absence_rate)}_{FItem(1, FilterItemName.Characteristic__Total)}_{FItem(1, FilterItemName.School_Type__Total)}_____"]
                                    = new ChartConfiguration
                                    {
                                        Label = "Unauthorised absence rate",
                                        Unit = "%",
                                        Colour = "#4763a5",
                                        symbol = ChartSymbol.circle
                                    },
                                [$"{Indicator(1, IndicatorName.Overall_absence_rate)}_{FItem(1, FilterItemName.Characteristic__Total)}_{FItem(1, FilterItemName.School_Type__Total)}_____"]
                                    = new ChartConfiguration
                                    {
                                        Label = "Overall absence rate",
                                        Unit = "%",
                                        Colour = "#f5a450",
                                        symbol = ChartSymbol.cross
                                    },
                                [$"{Indicator(1, IndicatorName.Authorised_absence_rate)}_{FItem(1, FilterItemName.Characteristic__Total)}_{FItem(1, FilterItemName.School_Type__Total)}_____"]
                                    = new ChartConfiguration
                                    {
                                        Label = "Authorised absence rate",
                                        Unit = "%",
                                        Colour = "#005ea5",
                                        symbol = ChartSymbol.diamond
                                    }
                            },
                            Legend = Legend.top
                        }
                    }
                },
                new DataBlock
                {
                    Id = new Guid("5d3058f2-459e-426a-b0b3-9f60d8629fef"),
                    ContentSectionId = new Guid("8965ef44-5ad7-4ab0-a142-78453d6f40af"),
                    DataBlockRequest = new DataBlockRequest
                    {
                        SubjectId = 1,
                        GeographicLevel = GeographicLevel.Country,
                        TimePeriod = new TimePeriod
                        {
                            StartYear = "2012",
                            StartCode = TimeIdentifier.AcademicYear,
                            EndYear = "2016",
                            EndCode = TimeIdentifier.AcademicYear
                        },
                        Filters = new List<string>
                        {
                            FItem(1, FilterItemName.Characteristic__Total),
                            FItem(1, FilterItemName.School_Type__Total)
                        },
                        Indicators = new List<string>
                        {
                            Indicator(1, IndicatorName.Unauthorised_absence_rate),
                            Indicator(1, IndicatorName.Overall_absence_rate),
                            Indicator(1, IndicatorName.Authorised_absence_rate)
                        }
                    },
                    Tables = new List<Table>
                    {
                        new Table
                        {
                            indicators = new List<string>
                            {
                                Indicator(1, IndicatorName.Unauthorised_absence_rate),
                                Indicator(1, IndicatorName.Overall_absence_rate),
                                Indicator(1, IndicatorName.Authorised_absence_rate)
                            }
                        }
                    },
                    Charts = new List<IContentBlockChart>
                    {
                        new LineChart
                        {
                            Axes = new Dictionary<string, AxisConfigurationItem>
                            {
                                ["major"] = new AxisConfigurationItem
                                {
                                    GroupBy = AxisGroupBy.timePeriod,
                                    DataSets = new List<ChartDataSet>
                                    {
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(1,
                                                IndicatorName.Unauthorised_absence_rate),
                                            Filters = new List<string>
                                            {
                                                FItem(1,
                                                    FilterItemName.Characteristic__Total),
                                                FItem(1, FilterItemName.School_Type__Total)
                                            }
                                        },
                                        new ChartDataSet
                                        {
                                            Indicator =
                                                Indicator(1, IndicatorName.Overall_absence_rate),
                                            Filters = new List<string>
                                            {
                                                FItem(1,
                                                    FilterItemName.Characteristic__Total),
                                                FItem(1, FilterItemName.School_Type__Total)
                                            }
                                        },
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(1,
                                                IndicatorName.Authorised_absence_rate),
                                            Filters = new List<string>
                                            {
                                                FItem(1,
                                                    FilterItemName.Characteristic__Total),
                                                FItem(1, FilterItemName.School_Type__Total)
                                            }
                                        }
                                    },
                                    Title = "School Year"
                                },
                                ["minor"] = new AxisConfigurationItem
                                {
                                    Min = 0,
                                    Title = "Absence Rate"
                                }
                            },
                            Labels = new Dictionary<string, ChartConfiguration>
                            {
                                [$"{Indicator(1, IndicatorName.Unauthorised_absence_rate)}_{FItem(1, FilterItemName.Characteristic__Total)}_{FItem(1, FilterItemName.School_Type__Total)}_____"]
                                    = new ChartConfiguration
                                    {
                                        Label = "Unauthorised absence rate",
                                        Unit = "%",
                                        Colour = "#4763a5",
                                        symbol = ChartSymbol.circle
                                    },
                                [$"{Indicator(1, IndicatorName.Overall_absence_rate)}_{FItem(1, FilterItemName.Characteristic__Total)}_{FItem(1, FilterItemName.School_Type__Total)}_____"]
                                    = new ChartConfiguration
                                    {
                                        Label = "Overall absence rate",
                                        Unit = "%",
                                        Colour = "#f5a450",
                                        symbol = ChartSymbol.cross
                                    },
                                [$"{Indicator(1, IndicatorName.Authorised_absence_rate)}_{FItem(1, FilterItemName.Characteristic__Total)}_{FItem(1, FilterItemName.School_Type__Total)}_____"]
                                    = new ChartConfiguration
                                    {
                                        Label = "Authorised absence rate",
                                        Unit = "%",
                                        Colour = "#005ea5",
                                        symbol = ChartSymbol.diamond
                                    }
                            }, 
                            Legend = Legend.top
                        }
                    }
                },
                new DataBlock
                {
                    Id = new Guid("4a1af98a-ed8a-438e-92d4-d21cca0429f9"),
                    ContentSectionId = new Guid("68e3028c-1291-42b3-9e7c-9be285dac9a1"),
                    DataBlockRequest = new DataBlockRequest
                    {
                        SubjectId = 1,
                        GeographicLevel = GeographicLevel.LocalAuthorityDistrict,
                        TimePeriod = new TimePeriod
                        {
                            StartYear = "2016",
                            StartCode = TimeIdentifier.AcademicYear,
                            EndYear = "2017",
                            EndCode = TimeIdentifier.AcademicYear
                        },

                        Indicators = new List<string>
                        {
                            Indicator(1, IndicatorName.Unauthorised_absence_rate),
                            Indicator(1, IndicatorName.Overall_absence_rate),
                            Indicator(1, IndicatorName.Authorised_absence_rate)
                        },
                        Filters = new List<string>
                        {
                            FItem(1, FilterItemName.Characteristic__Total),
                            FItem(1, FilterItemName.School_Type__Total)
                        }
                    },
                    Charts = new List<IContentBlockChart>
                    {
                        new MapChart
                        {
                            Axes = new Dictionary<string, AxisConfigurationItem>
                            {
                                ["major"] = new AxisConfigurationItem
                                {
                                    GroupBy = AxisGroupBy.timePeriod,
                                    DataSets = new List<ChartDataSet>
                                    {
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(1,
                                                IndicatorName.Unauthorised_absence_rate),
                                            Filters = new List<string>
                                            {
                                                FItem(1, FilterItemName.Characteristic__Total),
                                                FItem(1, FilterItemName.School_Type__Total)
                                            }
                                        },
                                        new ChartDataSet
                                        {
                                            Indicator =
                                                Indicator(1, IndicatorName.Overall_absence_rate),
                                            Filters = new List<string>
                                            {
                                                FItem(1, FilterItemName.Characteristic__Total),
                                                FItem(1, FilterItemName.School_Type__Total)
                                            }
                                        },
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(1,
                                                IndicatorName.Authorised_absence_rate),
                                            Filters = new List<string>
                                            {
                                                FItem(1, FilterItemName.Characteristic__Total),
                                                FItem(1, FilterItemName.School_Type__Total)
                                            }
                                        }
                                    },
                                    Title = "School Year"
                                },
                                ["minor"] = new AxisConfigurationItem
                                {
                                    Title = "Absence Rate"
                                }
                            },
                            Labels = new Dictionary<string, ChartConfiguration>
                            {
                                [$"{Indicator(1, IndicatorName.Unauthorised_absence_rate)}_{FItem(1, FilterItemName.Characteristic__Total)}_{FItem(1, FilterItemName.School_Type__Total)}_____"]
                                    = new ChartConfiguration
                                    {
                                        Label = "Unauthorised absence rate",
                                        Unit = "%",
                                        Colour = "#4763a5",
                                        symbol = ChartSymbol.circle
                                    },
                                [$"{Indicator(1, IndicatorName.Overall_absence_rate)}_{FItem(1, FilterItemName.Characteristic__Total)}_{FItem(1, FilterItemName.School_Type__Total)}_____"]
                                    = new ChartConfiguration
                                    {
                                        Label = "Overall absence rate",
                                        Unit = "%",
                                        Colour = "#f5a450",
                                        symbol = ChartSymbol.cross
                                    },
                                [$"{Indicator(1, IndicatorName.Authorised_absence_rate)}_{FItem(1, FilterItemName.Characteristic__Total)}_{FItem(1, FilterItemName.School_Type__Total)}_____"]
                                    = new ChartConfiguration
                                    {
                                        Label = "Authorised absence rate",
                                        Unit = "%",
                                        Colour = "#005ea5",
                                        symbol = ChartSymbol.diamond
                                    }
                            }
                        }
                    }
                },

                // exclusions
                new DataBlock
                {
                    Id = new Guid("17a0272b-318d-41f6-bda9-3bd88f78cd3d"),
                    ContentSectionId = null, // KeyStatistics
                    DataBlockRequest = new DataBlockRequest
                    {
                        SubjectId = 12,
                        GeographicLevel = GeographicLevel.Country,
                        TimePeriod = new TimePeriod
                        {
                            StartYear = "2012",
                            StartCode = TimeIdentifier.AcademicYear,
                            EndYear = "2016",
                            EndCode = TimeIdentifier.AcademicYear
                        },

                        Filters = new List<string>
                        {
                            FItem(12, FilterItemName.School_Type__Total)
                        },
                        Indicators = new List<string>
                        {
                            Indicator(12, IndicatorName.Number_of_schools),
                            Indicator(12, IndicatorName.Number_of_pupils),
                            Indicator(12, IndicatorName.Number_of_permanent_exclusions),
                            Indicator(12, IndicatorName.Permanent_exclusion_rate),
                            Indicator(12, IndicatorName.Number_of_fixed_period_exclusions),
                            Indicator(12, IndicatorName.Fixed_period_exclusion_rate),
                            Indicator(12, IndicatorName.Percentage_of_pupils_with_fixed_period_exclusions)
                        }
                    },
                    Summary = new Summary
                    {
                        dataKeys = new List<string>
                        {
                            Indicator(12, IndicatorName.Permanent_exclusion_rate),
                            Indicator(12, IndicatorName.Fixed_period_exclusion_rate),
                            Indicator(12, IndicatorName.Number_of_permanent_exclusions)
                        },
                        dataSummary = new List<string>
                        {
                            "Up from 0.08% in 2015/16",
                            "Up from 4.29% in 2015/16",
                            "Up from 6,685 in 2015/16"
                        },
                        dataDefinition = new List<string>
                        {
                            @"Number of permanent exclusions as a percentage of the overall school population. <a href=""/glossary#permanent-exclusion"">More >>></a>",
                            @"Number of fixed-period exclusions as a percentage of the overall school population. <a href=""/glossary#permanent-exclusion"">More >>></a>",
                            @"Total number of permanent exclusions within a school year. <a href=""/glossary#permanent-exclusion"">More >>></a>"
                        },
                        description = new MarkDownBlock
                        {
                            Id = new Guid("132bef6e-c2a3-459d-996e-40f29ed6e74f"),
                            Body =
                                " * overall permanent exclusions rate has increased to 0.10% - up from 0.08% in 2015/16\n" +
                                " * number of exclusions increased to 7,720 - up from 6,685 in 2015/16\n" +
                                " * overall fixed-period exclusions rate increased to 4.76% - up from 4.29% in 2015/16\n" +
                                " * number of exclusions increased to 381,865 - up from 339,360 in 2015/16\n"
                        }
                    },
                    Tables = new List<Table>
                    {
                        new Table
                        {
                            indicators = new List<string>
                            {
                                Indicator(12, IndicatorName.Permanent_exclusion_rate),
                                Indicator(12, IndicatorName.Fixed_period_exclusion_rate),
                                Indicator(12, IndicatorName.Number_of_permanent_exclusions)
                            }
                        }
                    },

                    Charts = new List<IContentBlockChart>
                    {
                        new LineChart
                        {
                            Axes = new Dictionary<string, AxisConfigurationItem>
                            {
                                ["major"] = new AxisConfigurationItem
                                {
                                    GroupBy = AxisGroupBy.timePeriod,
                                    DataSets = new List<ChartDataSet>
                                    {
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(12, IndicatorName.Fixed_period_exclusion_rate),
                                            Filters = new List<string>
                                            {
                                                FItem(12, FilterItemName.School_Type__Total)
                                            }
                                        },
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(12,
                                                IndicatorName.Percentage_of_pupils_with_fixed_period_exclusions),
                                            Filters = new List<string>
                                            {
                                                FItem(12, FilterItemName.School_Type__Total)
                                            }
                                        }
                                    },
                                    Title = "School Year"
                                },
                                ["minor"] = new AxisConfigurationItem
                                {
                                    Min = 0,
                                    Title = "Absence Rate"
                                }
                            },
                            Labels = new Dictionary<string, ChartConfiguration>
                            {
                                [$"{Indicator(12, IndicatorName.Fixed_period_exclusion_rate)}_{FItem(12, FilterItemName.School_Type__Total)}_____"]
                                    =
                                    new ChartConfiguration
                                    {
                                        Label = "Fixed period exclusion rate",
                                        Unit = "%",
                                        Colour = "#4763a5",
                                        symbol = ChartSymbol.circle
                                    },
                                [$"{Indicator(12, IndicatorName.Percentage_of_pupils_with_fixed_period_exclusions)}_{FItem(12, FilterItemName.School_Type__Total)}_____"]
                                    =
                                    new ChartConfiguration
                                    {
                                        Label = "Pupils with one or more exclusion",
                                        Unit = "%",
                                        Colour = "#f5a450",
                                        symbol = ChartSymbol.cross
                                    }
                            },
                            Legend = Legend.top
                        }
                    }
                },
                new DataBlock
                {
                    Id = new Guid("dd572e49-87e3-46f5-bb04-e9008573fc91"),
                    ContentSectionId = new Guid("6ed87fd1-81a5-46dc-8841-4598bdae7fee"),
                    Heading = "Chart showing permanent exclusions in England",
                    DataBlockRequest = new DataBlockRequest
                    {
                        SubjectId = 12,
                        GeographicLevel = GeographicLevel.Country,
                        TimePeriod = new TimePeriod
                        {
                            StartYear = "2012",
                            StartCode = TimeIdentifier.AcademicYear,
                            EndYear = "2016",
                            EndCode = TimeIdentifier.AcademicYear
                        },
                        Filters = new List<string>
                        {
                            FItem(12, FilterItemName.School_Type__Total)
                        },
                        Indicators = new List<string>
                        {
                            Indicator(12, IndicatorName.Permanent_exclusion_rate),
                            Indicator(12, IndicatorName.Number_of_pupils),
                            Indicator(12, IndicatorName.Number_of_permanent_exclusions)
                        }
                    },
                    Tables = new List<Table>
                    {
                        new Table
                        {
                            indicators = new List<string>
                            {
                                Indicator(12, IndicatorName.Number_of_pupils),
                                Indicator(12, IndicatorName.Number_of_permanent_exclusions),
                                Indicator(12, IndicatorName.Permanent_exclusion_rate)
                            }
                        }
                    },
                    Charts = new List<IContentBlockChart>
                    {
                        new LineChart
                        {
                            Axes = new Dictionary<string, AxisConfigurationItem>
                            {
                                ["major"] = new AxisConfigurationItem
                                {
                                    GroupBy = AxisGroupBy.timePeriod,
                                    DataSets = new List<ChartDataSet>
                                    {
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(12,
                                                IndicatorName.Permanent_exclusion_rate),
                                            Filters = new List<string>
                                            {
                                                FItem(12, FilterItemName.School_Type__Total)
                                            }
                                        }
                                    },
                                    Title = "School Year"
                                },
                                ["minor"] = new AxisConfigurationItem
                                {
                                    Min = 0,
                                    Title = "Exclusion Rate"
                                }
                            },
                            Labels = new Dictionary<string, ChartConfiguration>
                            {
                                [$"{Indicator(12, IndicatorName.Permanent_exclusion_rate)}_{FItem(12, FilterItemName.School_Type__Total)}_____"]
                                    =
                                    new ChartConfiguration
                                    {
                                        Label = "Fixed period exclusion rate",
                                        Unit = "%",
                                        Colour = "#4763a5",
                                        symbol = ChartSymbol.circle
                                    }
                            },
                            Legend = Legend.top
                        }
                    }
                },
                new DataBlock
                {
                    Id = new Guid("038093a2-0be3-440b-8b22-8116e34aa616"),
                    ContentSectionId = new Guid("7981db34-afdb-4f84-99e8-bfd43e58f16d"),
                    Heading = "Chart showing fixed-period exclusions in England",
                    DataBlockRequest = new DataBlockRequest
                    {
                        SubjectId = 12,
                        GeographicLevel = GeographicLevel.Country,
                        TimePeriod = new TimePeriod
                        {
                            StartYear = "2012",
                            StartCode = TimeIdentifier.AcademicYear,
                            EndYear = "2016",
                            EndCode = TimeIdentifier.AcademicYear
                        },

                        Filters = new List<string>
                        {
                            FItem(12, FilterItemName.School_Type__Total)
                        },
                        Indicators = new List<string>
                        {
                            Indicator(12, IndicatorName.Fixed_period_exclusion_rate),
                            Indicator(12, IndicatorName.Number_of_pupils),
                            Indicator(12, IndicatorName.Number_of_fixed_period_exclusions)
                        }
                    },
                    Tables = new List<Table>
                    {
                        new Table
                        {
                            indicators = new List<string>
                            {
                                Indicator(12, IndicatorName.Number_of_pupils),
                                Indicator(12, IndicatorName.Number_of_fixed_period_exclusions),
                                Indicator(12, IndicatorName.Fixed_period_exclusion_rate)
                            }
                        }
                    },
                    Charts = new List<IContentBlockChart>
                    {
                        new LineChart
                        {
                            Axes = new Dictionary<string, AxisConfigurationItem>
                            {
                                ["major"] = new AxisConfigurationItem
                                {
                                    GroupBy = AxisGroupBy.timePeriod,
                                    DataSets = new List<ChartDataSet>
                                    {
                                        new ChartDataSet
                                        {
                                            Indicator =
                                                Indicator(12,
                                                    IndicatorName.Fixed_period_exclusion_rate),
                                            Filters = new List<string>
                                            {
                                                FItem(12, FilterItemName.School_Type__Total)
                                            }
                                        }
                                    },
                                    Title = "School Year"
                                },
                                ["minor"] = new AxisConfigurationItem
                                {
                                    Min = 0,
                                    Title = "Absence Rate"
                                }
                            },
                            Labels = new Dictionary<string, ChartConfiguration>
                            {
                                [$"{Indicator(12, IndicatorName.Fixed_period_exclusion_rate)}_{FItem(12, FilterItemName.School_Type__Total)}_____"]
                                    =
                                    new ChartConfiguration
                                    {
                                        Label = "Fixed period exclusion rate",
                                        Unit = "%",
                                        Colour = "#4763a5",
                                        symbol = ChartSymbol.circle
                                    }
                            },
                            Legend = Legend.top
                        }
                    }
                },

                // Secondary and primary schools applications offers
                new DataBlock
                {
                    Id = new Guid("475738b4-ba10-4c29-a50d-6ca82c10de6e"),
                    ContentSectionId = null, // KeyStatistics
                    DataBlockRequest = new DataBlockRequest
                    {
                        SubjectId = 17,
                        GeographicLevel = GeographicLevel.Country,
                        TimePeriod = new TimePeriod
                        {
                            StartYear = "2014",
                            StartCode = TimeIdentifier.CalendarYear,
                            EndYear = "2018",
                            EndCode = TimeIdentifier.CalendarYear
                        },
                        Filters = new List<string>
                        {
                            FItem(17, FilterItemName.Year_of_admission__Primary_All_primary)
                        },
                        Indicators = new List<string>
                        {
                            Indicator(17, IndicatorName.Number_of_admissions),
                            Indicator(17, IndicatorName.Number_of_applications_received),
                            Indicator(17, IndicatorName.Number_of_first_preferences_offered),
                            Indicator(17, IndicatorName.Number_of_second_preferences_offered),
                            Indicator(17, IndicatorName.Number_of_third_preferences_offered),
                            Indicator(17, IndicatorName.Number_that_received_one_of_their_first_three_preferences),
                            Indicator(17, IndicatorName.Number_that_received_an_offer_for_a_preferred_school),
                            Indicator(17, IndicatorName.Number_that_received_an_offer_for_a_non_preferred_school),
                            Indicator(17, IndicatorName.Number_that_did_not_receive_an_offer)
                        }
                    },
                    Summary = new Summary
                    {
                        dataKeys = new List<string>
                        {
                            Indicator(17, IndicatorName.Number_of_applications_received),
                            Indicator(17, IndicatorName.Number_of_first_preferences_offered),
                            Indicator(17, IndicatorName.Number_of_second_preferences_offered)
                        },
                        dataSummary = new List<string>
                        {
                            "Down from 620,330 in 2017",
                            "Down from 558,411 in 2017",
                            "Down from 34,792 in 2017"
                        },
                        dataDefinition = new List<string>
                        {
                            @"Total number of applications received for places at primary and secondary schools.",
                            @"Total number of first preferences offered to applicants by schools.",
                            @"Total number of second preferences offered to applicants by schools."
                        },
                        description = new MarkDownBlock
                        {
                            Id = new Guid("fdcac9d3-dab5-445d-9802-a8af0990efb2"),
                            Body =
                                "* majority of applicants received a preferred offer\n" +
                                "* percentage of applicants receiving secondary first choice offers decreases as applications increase\n" +
                                "* slight proportional increase in applicants receiving primary first choice offer as applications decrease\n"
                        }
                    },
                    Tables = new List<Table>
                    {
                        new Table
                        {
                            indicators = new List<string>
                            {
                                Indicator(17, IndicatorName.Number_of_applications_received),
                                Indicator(17, IndicatorName.Number_of_admissions),
                                Indicator(17, IndicatorName.Number_of_first_preferences_offered),
                                Indicator(17, IndicatorName.Number_of_second_preferences_offered),
                                Indicator(17, IndicatorName.Number_of_third_preferences_offered),
                                Indicator(17,
                                    IndicatorName.Number_that_received_an_offer_for_a_non_preferred_school),
                                Indicator(17, IndicatorName.Number_that_did_not_receive_an_offer)
                            }
                        }
                    }
                },
                new DataBlock
                {
                    Id = new Guid("52916052-81e3-4b66-80b8-24f8666d9cbf"),
                    ContentSectionId = new Guid("6bfa9b19-25d6-4d45-8008-9447db541795"),
                    Heading =
                        "Table of Timeseries of key secondary preference rates, England",
                    DataBlockRequest = new DataBlockRequest
                    {
                        SubjectId = 17,
                        GeographicLevel = GeographicLevel.Country,
                        TimePeriod = new TimePeriod
                        {
                            StartYear = "2014",
                            StartCode = TimeIdentifier.CalendarYear,
                            EndYear = "2018",
                            EndCode = TimeIdentifier.CalendarYear
                        },

                        Filters = new List<string>
                        {
                            FItem(17, FilterItemName.Year_of_admission__Secondary_All_secondary)
                        },
                        Indicators = new List<string>
                        {
                            Indicator(17,
                                IndicatorName.Number_that_received_an_offer_for_a_preferred_school),
                            Indicator(17,
                                IndicatorName.Number_that_received_an_offer_for_a_non_preferred_school),
                            Indicator(17, IndicatorName.Number_that_did_not_receive_an_offer),
                            Indicator(17,
                                IndicatorName
                                    .Number_that_received_an_offer_for_a_school_within_their_LA)
                        }
                    },
                    Tables = new List<Table>
                    {
                        new Table
                        {
                            indicators = new List<string>
                            {
                                Indicator(17,
                                    IndicatorName.Number_that_received_an_offer_for_a_preferred_school),
                                Indicator(17,
                                    IndicatorName
                                        .Number_that_received_an_offer_for_a_non_preferred_school),
                                Indicator(17, IndicatorName.Number_that_did_not_receive_an_offer)
                            }
                        }
                    }
                },
                new DataBlock
                {
                    Id = new Guid("a8c408ed-45d8-4690-a9f3-2fb0e86377bf"),
                    ContentSectionId = new Guid("c3eb66d0-ce13-4e68-861d-98bb914d0814"),
                    Heading =
                        "Table showing Timeseries of key primary preference rates, England Entry into academic year",
                    DataBlockRequest = new DataBlockRequest
                    {
                        SubjectId = 17,
                        GeographicLevel = GeographicLevel.Country,
                        TimePeriod = new TimePeriod
                        {
                            StartYear = "2014",
                            StartCode = TimeIdentifier.CalendarYear,
                            EndYear = "2018",
                            EndCode = TimeIdentifier.CalendarYear
                        },

                        Filters = new List<string>
                        {
                            FItem(17, FilterItemName.Year_of_admission__Primary_All_primary)
                        },
                        Indicators = new List<string>
                        {
                            Indicator(17,
                                IndicatorName.Number_that_received_an_offer_for_a_preferred_school),
                            Indicator(17,
                                IndicatorName.Number_that_received_an_offer_for_a_non_preferred_school),
                            Indicator(17, IndicatorName.Number_that_did_not_receive_an_offer),
                            Indicator(17,
                                IndicatorName
                                    .Number_that_received_an_offer_for_a_school_within_their_LA)
                        }
                    },
                    Tables = new List<Table>
                    {
                        new Table
                        {
                            indicators = new List<string>
                            {
                                Indicator(17,
                                    IndicatorName.Number_that_received_an_offer_for_a_preferred_school),
                                Indicator(17,
                                    IndicatorName
                                        .Number_that_received_an_offer_for_a_non_preferred_school),
                                Indicator(17, IndicatorName.Number_that_did_not_receive_an_offer)
                            }
                        }
                    }
                }
            );

            modelBuilder.Entity<Update>().HasData(
                new Update
                {
                    Id = new Guid("9c0f0139-7f88-4750-afe0-1c85cdf1d047"),
                    ReleaseId = new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                    On = new DateTime(2018, 4, 19),
                    Reason =
                        "Underlying data file updated to include absence data by pupil residency and school location, and updated metadata document."
                },
                new Update
                {
                    Id = new Guid("18e0d40e-bdf7-4c84-99dd-732e72e9c9a5"),
                    ReleaseId = new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                    On = new DateTime(2018, 3, 22),
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
                        "Updated exclusion rates for Gypsy/Roma pupils, to include extended ethnicity categories within the headcount (Gypsy, Roma and other Gypsy/Roma)."
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
                },
                new Update
                {
                    Id = new Guid("8900bab9-74ec-4b5d-8be1-648ff4870167"),
                    ReleaseId = new Guid("e7ae88fb-afaf-4d51-a78a-bbb2de671daf"),
                    On = new DateTime(2018, 6, 20),
                    Reason = "First published."
                },
                new Update
                {
                    Id = new Guid("448ca9ea-0cd2-4e6d-b85b-76c3ef7d3bf9"),
                    ReleaseId = new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"),
                    On = new DateTime(2018, 6, 14),
                    Reason = "First published."
                }
            );
            modelBuilder.Entity<Link>().HasData(
                new Link
                {
                    PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                    Id = new Guid("08134c1d-8a58-49a4-8d8b-22e586ffd5ae"),
                    Description = "2008 to 2009",
                    Url =
                        "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-academic-year-2008-to-2009",
                },
                new Link
                {
                    PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                    Id = new Guid("250bace6-aeb9-4fe9-8de2-3a25e0dc717f"),
                    Description = "2009 to 2010",
                    Url =
                        "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-from-schools-in-england-academic-year-2009-to-2010",
                },
                new Link
                {
                    PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                    Id = new Guid("a319e4ef-b957-40fb-8a47-b1a97814b220"),
                    Description = "2010 to 2011",
                    Url =
                        "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-from-schools-in-england-academic-year-2010-to-2011",
                },
                new Link
                {
                    PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                    Id = new Guid("13acf54a-8016-49ff-9050-c61ebe7acad2"),
                    Description = "2011 to 2012",
                    Url =
                        "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-from-schools-in-england-2011-to-2012-academic-year",
                },
                new Link
                {
                    PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                    Id = new Guid("45bd7f62-2018-4a5c-9b93-ccece8e89c46"),
                    Description = "2012 to 2013",
                    Url =
                        "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2012-to-2013",
                },
                new Link
                {
                    PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                    Id = new Guid("f1225f98-40d5-494c-90f9-99f9fb59ac9d"),
                    Description = "2013 to 2014",
                    Url =
                        "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2013-to-2014",
                },
                new Link
                {
                    PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                    Id = new Guid("78239816-507d-42b7-98fd-4a71d0d4eb1f"),
                    Description = "2014 to 2015",
                    Url =
                        "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2014-to-2015",
                },
                new Link
                {
                    PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                    Id = new Guid("564dacdc-f58e-4aa0-8dbd-d8368b4fb6ba"),
                    Description = "2015 to 2016",
                    Url =
                        "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2015-to-2016",
                },
                new Link
                {
                    PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                    Id = new Guid("89c02688-646d-45b5-8919-9a3fafcfe0e9"),
                    Description = "2009 to 2010",
                    Url =
                        "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2009-to-2010",
                },
                new Link
                {
                    PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                    Id = new Guid("81d91c86-9bf2-496c-b026-9dc255c35635"),
                    Description = "2010 to 2011",
                    Url =
                        "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2010-to-2011",
                },
                new Link
                {
                    PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                    Id = new Guid("e20141d0-d894-4b8d-a78f-e41c23500786"),
                    Description = "2011 to 2012",
                    Url =
                        "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics",
                },
                new Link
                {
                    PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                    Id = new Guid("ce15f487-87b0-4c07-98f1-6c6732196be7"),
                    Description = "2012 to 2013",
                    Url =
                        "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2012-to-2013",
                },
                new Link
                {
                    PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                    Id = new Guid("75991639-ad77-4ba6-91fc-ac08c00a4ce8"),
                    Description = "2013 to 2014",
                    Url =
                        "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2013-to-2014",
                },
                new Link
                {
                    PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                    Id = new Guid("28e53936-5a52-44be-a7a6-d2f14a426d28"),
                    Description = "2014 to 2015",
                    Url =
                        "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2014-to-2015",
                },
                new Link
                {
                    PublicationId = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                    Id = new Guid("dc8b0d8c-08bb-47cc-b3a1-9e6ac9c2c268"),
                    Description = "January 2010",
                    Url =
                        "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2010",
                },
                new Link
                {
                    PublicationId = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                    Id = new Guid("b086ba70-703c-40dd-aaef-d2e19335188e"),
                    Description = "January 2011",
                    Url =
                        "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2011",
                },
                new Link
                {
                    PublicationId = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                    Id = new Guid("181ec43e-cf22-4cab-a128-0a5702468566"),
                    Description = "January 2012",
                    Url =
                        "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2012",
                },
                new Link
                {
                    PublicationId = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                    Id = new Guid("e6b36ee8-ef66-4864-a4b3-9047ee3da338"),
                    Description = "January 2013",
                    Url =
                        "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2013",
                },
                new Link
                {
                    PublicationId = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                    Id = new Guid("398ba8c6-3ea0-49da-8645-ceb3c7fb9860"),
                    Description = "January 2014",
                    Url =
                        "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2014",
                },
                new Link
                {
                    PublicationId = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                    Id = new Guid("5e244416-6f2a-4d22-bea4-c22a229befef"),
                    Description = "January 2015",
                    Url =
                        "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2015",
                },
                new Link
                {
                    PublicationId = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                    Id = new Guid("e3c1db23-8a8f-47fe-b2cd-8e677db700a2"),
                    Description = "January 2016",
                    Url =
                        "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2016",
                },
                new Link
                {
                    PublicationId = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                    Id = new Guid("313435b3-fe56-4b92-8e13-670dbf510062"),
                    Description = "January 2017",
                    Url =
                        "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2017",
                }
            );
            modelBuilder.Entity<Methodology>().HasData(
                new Methodology
                {
                    Id = new Guid("caa8e56f-41d2-4129-a5c3-53b051134bd7"),
                    Title = "Pupil absence statistics: methodology",
                    Published = new DateTime(2018, 3, 22),
                    LastUpdated = new DateTime(2019, 6, 26),
                    Slug = "pupil-absence-in-schools-in-england",
                    Summary =
                        "Find out about the methodology behind pupil absence statistics and data and how and why they're collected and published.",
                    Content = new List<ContentSection>
                    {
                        new ContentSection
                        {
                            Id = new Guid("5a7fd947-d131-475d-afcd-11ab2b1ece67"),
                            Heading = "1. Overview of absence statistics",
                            Caption = "",
                            Order = 1,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = new Guid("4d5ae97d-fa1c-4a09-a0a3-b28307fcfb09"),
                                    Body = File.Exists(@"Migrations/Html/Pupil_Absence_Statistics/Section1.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Absence_Statistics/Section1.html",
                                            Encoding.UTF8)
                                        : ""
                                },
                            }
                        },
                        new ContentSection
                        {
                            Id = new Guid("dabb7562-0433-42fc-96e4-64a68f399dac"),
                            Heading = "2. National Statistics badging",
                            Caption = "",
                            Order = 2,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = new Guid("6bf20dd4-a7d6-4bc6-a13a-9f574935c9af"),
                                    Body = File.Exists(@"Migrations/Html/Pupil_Absence_Statistics/Section2.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Absence_Statistics/Section2.html",
                                            Encoding.UTF8)
                                        : ""
                                },
                            }
                        },
                        new ContentSection
                        {
                            Id = new Guid("50b5031a-93e4-4756-843e-21f88f52ba68"),
                            Heading = "3. Methodology",
                            Caption = "",
                            Order = 3,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = new Guid("63a318d9-05fa-40eb-9808-b825a6deb54a"),
                                    Body = File.Exists(@"Migrations/Html/Pupil_Absence_Statistics/Section3.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Absence_Statistics/Section3.html",
                                            Encoding.UTF8)
                                        : ""
                                },
                            }
                        },
                        new ContentSection
                        {
                            Id = new Guid("e4ca520f-b609-4abb-a38c-c2d610a18e9f"),
                            Heading = "4. Data collection",
                            Caption = "",
                            Order = 4,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = new Guid("7714efb9-cc82-4895-ba27-bf5464541e38"),
                                    Body = File.Exists(@"Migrations/Html/Pupil_Absence_Statistics/Section4.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Absence_Statistics/Section4.html",
                                            Encoding.UTF8)
                                        : ""
                                }
                            }
                        },
                        new ContentSection
                        {
                            Id = new Guid("da91d355-b878-4135-a0a9-fb538c601246"),
                            Heading = "5. Data processing",
                            Caption = "",
                            Order = 5,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = new Guid("6f81ab70-5730-4cf1-a513-669f5c4bef09"),
                                    Body = File.Exists(@"Migrations/Html/Pupil_Absence_Statistics/Section5.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Absence_Statistics/Section5.html",
                                            Encoding.UTF8)
                                        : ""
                                }
                            }
                        },
                        new ContentSection
                        {
                            Id = new Guid("8df45966-5444-4487-be49-763c5009eea6"),
                            Heading = "6. Data quality",
                            Caption = "",
                            Order = 6,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = new Guid("a40d6c9e-fe61-48c0-b907-9757148beb0d"),
                                    Body = File.Exists(@"Migrations/Html/Pupil_Absence_Statistics/Section6.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Absence_Statistics/Section6.html",
                                            Encoding.UTF8)
                                        : ""
                                }
                            }
                        },
                        new ContentSection
                        {
                            Id = new Guid("bf6870de-07d3-4e65-a877-373a63dbcc5d"),
                            Heading = "7. Contacts",
                            Caption = "",
                            Order = 7,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = new Guid("f620a229-21b7-4c6e-afd4-e9feb111f09a"),
                                    Body = File.Exists(@"Migrations/Html/Pupil_Absence_Statistics/Section7.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Absence_Statistics/Section7.html",
                                            Encoding.UTF8)
                                        : ""
                                }
                            }
                        }
                    },
                    Annexes = new List<ContentSection>
                    {
                        new ContentSection
                        {
                            Id = new Guid("0522bb29-1e0d-455a-88ef-5887f76fb069"),
                            Heading = "Annex A - Calculations",
                            Caption = "",
                            Order = 1,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = new Guid("8b90b3b2-f63d-4499-91aa-41ccae74e1c7"),
                                    Body = File.Exists(@"Migrations/Html/Pupil_Absence_Statistics/AnnexA.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Absence_Statistics/AnnexA.html",
                                            Encoding.UTF8)
                                        : ""
                                }
                            }
                        },
                        new ContentSection
                        {
                            Id = new Guid("f1aac714-665d-436e-a488-1ca409d618bf"),
                            Heading = "Annex B - School attendance codes",
                            Caption = "",
                            Order = 2,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = new Guid("47f3e500-ec9f-4a00-96f8-c488f76b06e6"),
                                    Body = File.Exists(@"Migrations/Html/Pupil_Absence_Statistics/AnnexB.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Absence_Statistics/AnnexB.html",
                                            Encoding.UTF8)
                                        : ""
                                }
                            }
                        },
                        new ContentSection
                        {
                            Id = new Guid("0b888133-215a-4b28-8c24-e0ee9a32df6e"),
                            Heading = "Annex C - Links to pupil absence national statistics and data",
                            Caption = "",
                            Order = 3,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = new Guid("a00a7765-aa81-43f2-afe1-fead7f070291"),
                                    Body = File.Exists(@"Migrations/Html/Pupil_Absence_Statistics/AnnexC.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Absence_Statistics/AnnexC.html",
                                            Encoding.UTF8)
                                        : ""
                                }
                            }
                        },
                        new ContentSection
                        {
                            Id = new Guid("4c4c71e2-24e1-4b57-8a23-ce54fae9b329"),
                            Heading = "Annex D - Standard breakdowns",
                            Caption = "",
                            Order = 4,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = new Guid("7cc516d4-fc79-4e22-b35b-a042d5b14d35"),
                                    Body = File.Exists(@"Migrations/Html/Pupil_Absence_Statistics/AnnexD.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Absence_Statistics/AnnexD.html",
                                            Encoding.UTF8)
                                        : ""
                                }
                            }
                        },
                        new ContentSection
                        {
                            Id = new Guid("97a138bf-4ebb-4b17-86ab-ed78584608e3"),
                            Heading = "Annex E - Timeline",
                            Caption = "",
                            Order = 5,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = new Guid("8ddc6877-acd2-479d-a86f-1139c1bd429f"),
                                    Body = File.Exists(@"Migrations/Html/Pupil_Absence_Statistics/AnnexE.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Absence_Statistics/AnnexE.html",
                                            Encoding.UTF8)
                                        : ""
                                }
                            }
                        },
                        new ContentSection
                        {
                            Id = new Guid("dc00e749-0893-47f7-8440-5a4da47ceed7"),
                            Heading = "Annex F - Absence rates over time",
                            Caption = "",
                            Order = 6,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = new Guid("0fd71cfd-cfdd-42c5-86e7-9e311beee646"),
                                    Body = File.Exists(@"Migrations/Html/Pupil_Absence_Statistics/AnnexF.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Absence_Statistics/AnnexF.html",
                                            Encoding.UTF8)
                                        : ""
                                }
                            }
                        }
                    }
                },
                new Methodology
                {
                    Id = new Guid("8ab41234-cc9d-4b3d-a42c-c9fce7762719"),
                    Title = "Secondary and primary school applications and offers: methodology",
                    Published = new DateTime(2018, 6, 14),
                    Slug = "secondary-and-primary-schools-applications-and-offers",
                    Summary = "",
                    Content = new List<ContentSection>
                    {
                        new ContentSection
                        {
                            Id = new Guid("d82b2a2c-b117-4f96-b812-80de5304ae21"),
                            Heading = "1. Overview of applications and offers statistics",
                            Caption = "",
                            Order = 1,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = new Guid("ff7b34e3-58ba-4578-849a-e5044fc14b8d"),
                                    Body =
                                        File.Exists(
                                            @"Migrations/Html/Secondary_And_Primary_School_Applications_And_Offers/Section1.html")
                                            ? File.ReadAllText(
                                                @"Migrations/Html/Secondary_And_Primary_School_Applications_And_Offers/Section1.html",
                                                Encoding.UTF8)
                                            : ""
                                }
                            }
                        },
                        new ContentSection
                        {
                            Id = new Guid("f0814433-92d4-4ce5-b63b-2f2cb1b6f48a"),
                            Heading = "2. The admissions process",
                            Caption = "",
                            Order = 2,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = new Guid("d7f310d7-e917-47e2-9e05-065c8bcab891"),
                                    Body =
                                        File.Exists(
                                            @"Migrations/Html/Secondary_And_Primary_School_Applications_And_Offers/Section2.html")
                                            ? File.ReadAllText(
                                                @"Migrations/Html/Secondary_And_Primary_School_Applications_And_Offers/Section2.html",
                                                Encoding.UTF8)
                                            : ""
                                }
                            }
                        },
                        new ContentSection
                        {
                            Id = new Guid("1d7a492b-3e59-4624-9a2a-076635d1f780"),
                            Heading = "3. Methodology",
                            Caption = "",
                            Order = 3,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = new Guid("a89226ef-1d6a-48ba-a795-0dbc334a9198"),
                                    Body =
                                        File.Exists(
                                            @"Migrations/Html/Secondary_And_Primary_School_Applications_And_Offers/Section3.html")
                                            ? File.ReadAllText(
                                                @"Migrations/Html/Secondary_And_Primary_School_Applications_And_Offers/Section3.html",
                                                Encoding.UTF8)
                                            : ""
                                }
                            }
                        },
                        new ContentSection
                        {
                            Id = new Guid("f129939b-803f-461b-8838-e7a3d8c6eca2"),
                            Heading = "4. Contacts",
                            Caption = "",
                            Order = 4,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = new Guid("16898320-cf69-45c4-8dbb-2486901759b1"),
                                    Body =
                                        File.Exists(
                                            @"Migrations/Html/Secondary_And_Primary_School_Applications_And_Offers/Section4.html")
                                            ? File.ReadAllText(
                                                @"Migrations/Html/Secondary_And_Primary_School_Applications_And_Offers/Section4.html",
                                                Encoding.UTF8)
                                            : ""
                                }
                            }
                        }
                    }
                },
                new Methodology
                {
                    Id = new Guid("c8c911e3-39c1-452b-801f-25bb79d1deb7"),
                    Title = "Pupil exclusion statistics: methodology",
                    Published = new DateTime(2018, 8, 25),
                    Slug = "permanent-and-fixed-period-exclusions-in-england",
                    Summary = "",
                    Content = new List<ContentSection>
                    {
                        new ContentSection
                        {
                            Id = new Guid("bceaafc1-9548-4a03-98d5-d3476c8b9d99"),
                            Heading = "1. Overview of exclusion statistics",
                            Caption = "",
                            Order = 1,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = new Guid("9a034a5f-7cdb-4895-b205-864e7f834ebf"),
                                    Body = File.Exists(@"Migrations/Html/Pupil_Exclusion_Statistics/Section1.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Exclusion_Statistics/Section1.html",
                                            Encoding.UTF8)
                                        : ""
                                }
                            }
                        },
                        new ContentSection
                        {
                            Id = new Guid("66b15928-46c6-48d5-90e6-12cf354b4e04"),
                            Heading = "2. National Statistics badging",
                            Caption = "",
                            Order = 2,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = new Guid("f745893e-68a6-4813-ba8b-35d44c0935aa"),
                                    Body = File.Exists(@"Migrations/Html/Pupil_Exclusion_Statistics/Section2.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Exclusion_Statistics/Section2.html",
                                            Encoding.UTF8)
                                        : ""
                                }
                            }
                        },
                        new ContentSection
                        {
                            Id = new Guid("863f2b02-67b1-41bd-b1c9-f998f4581297"),
                            Heading = "3. Methodology",
                            Caption = "",
                            Order = 3,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = new Guid("4c88cbdd-e0e2-4019-a656-31e4b97d19d5"),
                                    Body = File.Exists(@"Migrations/Html/Pupil_Exclusion_Statistics/Section3.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Exclusion_Statistics/Section3.html",
                                            Encoding.UTF8)
                                        : ""
                                }
                            }
                        },
                        new ContentSection
                        {
                            Id = new Guid("fc66f72e-0176-4c75-b15f-2f35c7329563"),
                            Heading = "4. Data collection",
                            Caption = "",
                            Order = 4,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = new Guid("085ba061-918b-4e6e-9a02-3a8b12671587"),
                                    Body = File.Exists(@"Migrations/Html/Pupil_Exclusion_Statistics/Section4.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Exclusion_Statistics/Section4.html",
                                            Encoding.UTF8)
                                        : ""
                                }
                            }
                        },
                        new ContentSection
                        {
                            Id = new Guid("0c44636a-9a31-4e05-8db7-331ed5eae366"),
                            Heading = "5. Data processing",
                            Caption = "",
                            Order = 5,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = new Guid("29f138eb-070e-41fe-95c6-e271cdf4eaf4"),
                                    Body = File.Exists(@"Migrations/Html/Pupil_Exclusion_Statistics/Section5.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Exclusion_Statistics/Section5.html",
                                            Encoding.UTF8)
                                        : ""
                                }
                            }
                        },
                        new ContentSection
                        {
                            Id = new Guid("69df08b6-dcda-449e-828e-5666c8e6d533"),
                            Heading = "6. Data quality",
                            Caption = "",
                            Order = 6,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = new Guid("8263cdc1-a2e8-4db6-a581-44043a6add64"),
                                    Body = File.Exists(@"Migrations/Html/Pupil_Exclusion_Statistics/Section6.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Exclusion_Statistics/Section6.html",
                                            Encoding.UTF8)
                                        : ""
                                }
                            }
                        },
                        new ContentSection
                        {
                            Id = new Guid("fa315759-a51b-4860-8ae5-7b9505873108"),
                            Heading = "7. Contacts",
                            Caption = "",
                            Order = 7,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = new Guid("e1eb03d9-25e4-4247-b3de-49805cce7889"),
                                    Body = File.Exists(@"Migrations/Html/Pupil_Exclusion_Statistics/Section7.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Exclusion_Statistics/Section7.html",
                                            Encoding.UTF8)
                                        : ""
                                }
                            }
                        },
                    },
                    Annexes = new List<ContentSection>
                    {
                        new ContentSection
                        {
                            Id = new Guid("2bb1ce6d-8b54-4a77-bf7d-466c5f7f6bc3"),
                            Heading = "Annex A - Calculations",
                            Caption = "",
                            Order = 1,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = new Guid("c2d4d345-59b6-443b-bcb0-67c1f1dd9732"),
                                    Body = File.Exists(@"Migrations/Html/Pupil_Exclusion_Statistics/AnnexA.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Exclusion_Statistics/AnnexA.html",
                                            Encoding.UTF8)
                                        : ""
                                }
                            }
                        },
                        new ContentSection
                        {
                            Id = new Guid("01e9feb8-8ca0-4d98-8a17-78672e4641a7"),
                            Heading = "Annex B - Exclusion by reason codes",
                            Caption = "",
                            Order = 2,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = new Guid("e4e4f98b-cbeb-451f-bd17-8e2d572b83f4"),
                                    Body = File.Exists(@"Migrations/Html/Pupil_Exclusion_Statistics/AnnexB.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Exclusion_Statistics/AnnexB.html",
                                            Encoding.UTF8)
                                        : ""
                                }
                            }
                        },
                        new ContentSection
                        {
                            Id = new Guid("39576875-4a54-4028-bdb0-fecc67041f82"),
                            Heading = "Annex C - Links to pupil exclusions statistics and data",
                            Caption = "",
                            Order = 3,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = new Guid("94246f85-e43a-4b6c-97a0-b045701dc077"),
                                    Body = File.Exists(@"Migrations/Html/Pupil_Exclusion_Statistics/AnnexC.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Exclusion_Statistics/AnnexC.html",
                                            Encoding.UTF8)
                                        : ""
                                }
                            }
                        },
                        new ContentSection
                        {
                            Id = new Guid("e3bfcc04-7d91-45b7-b0ee-19713de4b433"),
                            Heading = "Annex D - Standard breakdowns",
                            Caption = "",
                            Order = 4,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Id = new Guid("b05ae1f7-b2d1-4ae3-9db6-28fb0edf98ae"),
                                    Body = File.Exists(@"Migrations/Html/Pupil_Exclusion_Statistics/AnnexD.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Exclusion_Statistics/AnnexD.html",
                                            Encoding.UTF8)
                                        : ""
                                }
                            }
                        }
                    }
                }
            );
        }

        private static string FItem(int subjectId, FilterItemName filterItemName)
        {
            return SubjectFilterItemIds[subjectId][filterItemName].ToString();
        }

        private static string Indicator(int subjectId, IndicatorName indicatorName)
        {
            return SubjectIndicatorIds[subjectId][indicatorName].ToString();
        }
    }
}