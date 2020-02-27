using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
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

        private enum SubjectName
        {
            AbsenceByCharacteristic,
            ExclusionsByGeographicLevel,
            SchoolApplicationsAndOffers
        }
        
        private static readonly Dictionary<SubjectName, Guid> SubjectIds = new Dictionary<SubjectName, Guid>
        {
            {
                SubjectName.AbsenceByCharacteristic, new Guid("803fbf56-600f-490f-8409-6413a891720d")
            },
            {
                SubjectName.ExclusionsByGeographicLevel, new Guid("3c0fbe56-0a4b-4caa-82f2-ab696cd96090")
            },
            {
                SubjectName.SchoolApplicationsAndOffers, new Guid("fa0d7f1d-d181-43fb-955b-fc327da86f2c")
            }
        };
        
        private static readonly Dictionary<Guid, Dictionary<FilterItemName, Guid>> SubjectFilterItemIds =
            new Dictionary<Guid, Dictionary<FilterItemName, Guid>>
            {
                {
                    SubjectIds[SubjectName.AbsenceByCharacteristic], new Dictionary<FilterItemName, Guid>
                    {
                        {
                            FilterItemName.Characteristic__Total, new Guid("183f94c3-b5d7-4868-892d-c948e256744d")
                        },
                        {
                            FilterItemName.School_Type__Total, new Guid("cb9b57e8-9965-4cb6-b61a-acc6d34b32be")
                        }
                    }
                },
                {
                    SubjectIds[SubjectName.ExclusionsByGeographicLevel], new Dictionary<FilterItemName, Guid>
                    {
                        {
                            FilterItemName.School_Type__Total, new Guid("1f3f86a4-de9f-43d7-5bfd-08d78f900a85")
                        }
                    }
                },
                {
                    SubjectIds[SubjectName.SchoolApplicationsAndOffers], new Dictionary<FilterItemName, Guid>
                    {
                        {
                            FilterItemName.Year_of_admission__Primary_All_primary,
                            new Guid("e957db0c-3bf8-4e4b-5c6f-08d78f900a85")
                        },
                        {
                            FilterItemName.Year_of_admission__Secondary_All_secondary,
                            new Guid("5a7b4e97-7794-4037-5c71-08d78f900a85")
                        }
                    }
                }
            };

        private static readonly Dictionary<Guid, Dictionary<IndicatorName, Guid>> SubjectIndicatorIds =
            new Dictionary<Guid, Dictionary<IndicatorName, Guid>>
            {
                {
                    SubjectIds[SubjectName.AbsenceByCharacteristic], new Dictionary<IndicatorName, Guid>
                    {
                        {
                            IndicatorName.Unauthorised_absence_rate, new Guid("ccfe716a-6976-4dc3-8fde-a026cd30f3ae")
                        },
                        {
                            IndicatorName.Overall_absence_rate, new Guid("92d3437a-0a62-4cd7-8dfb-bcceba7eef61")
                        },
                        {
                            IndicatorName.Authorised_absence_rate, new Guid("f9ae4976-7cd3-4718-834a-09349b6eb377")
                        }
                    }
                },
                {
                    SubjectIds[SubjectName.ExclusionsByGeographicLevel], new Dictionary<IndicatorName, Guid>
                    {
                        {
                            IndicatorName.Number_of_schools, new Guid("b3df4fb1-dae3-4c16-4c01-08d78f90080f")
                        },
                        {
                            IndicatorName.Number_of_pupils, new Guid("a5a58f92-aba1-4955-4c02-08d78f90080f")
                        },
                        {
                            IndicatorName.Number_of_permanent_exclusions,
                            new Guid("167f4807-4fdd-461a-4c03-08d78f90080f")
                        },
                        {
                            IndicatorName.Permanent_exclusion_rate, new Guid("be3b765b-005f-4279-4c04-08d78f90080f")
                        },
                        {
                            IndicatorName.Number_of_fixed_period_exclusions,
                            new Guid("f045bc8d-8dd1-4f16-4c05-08d78f90080f")
                        },
                        {
                            IndicatorName.Fixed_period_exclusion_rate, new Guid("68aeda43-2b6a-433a-4c06-08d78f90080f")
                        },
                        {
                            IndicatorName.Percentage_of_pupils_with_fixed_period_exclusions,
                            new Guid("732f0d7b-dcd3-4bf8-4c08-08d78f90080f")
                        }
                    }
                },
                {
                    SubjectIds[SubjectName.SchoolApplicationsAndOffers], new Dictionary<IndicatorName, Guid>
                    {
                        {
                            IndicatorName.Number_of_admissions, new Guid("49d2a1f4-e4a9-4f25-4c24-08d78f90080f")
                        },
                        {
                            IndicatorName.Number_of_applications_received,
                            new Guid("020a4da6-1111-443d-af80-3a425c558d14")
                        },
                        {
                            IndicatorName.Number_of_first_preferences_offered,
                            new Guid("94f9b11c-df82-4eef-4c29-08d78f90080f")
                        },
                        {
                            IndicatorName.Number_of_second_preferences_offered,
                            new Guid("d22e1104-de56-4617-4c2a-08d78f90080f")
                        },
                        {
                            IndicatorName.Number_of_third_preferences_offered,
                            new Guid("319dd956-a714-40fd-4c2b-08d78f90080f")
                        },
                        {
                            IndicatorName.Number_that_received_one_of_their_first_three_preferences,
                            new Guid("a9211c9d-b467-48d7-4c2c-08d78f90080f")
                        },
                        {
                            IndicatorName.Number_that_received_an_offer_for_a_preferred_school,
                            new Guid("be1e1643-f7c8-40b0-4c2d-08d78f90080f")
                        },
                        {
                            IndicatorName.Number_that_received_an_offer_for_a_non_preferred_school,
                            new Guid("16cdfc0a-f66f-496b-4c2e-08d78f90080f")
                        },
                        {
                            IndicatorName.Number_that_did_not_receive_an_offer,
                            new Guid("2c63589e-b5d4-4922-4c2f-08d78f90080f")
                        },
                        {
                            IndicatorName.Number_that_received_an_offer_for_a_school_within_their_LA,
                            new Guid("d10d4f10-c2f8-4120-4c30-08d78f90080f")
                        }
                    }
                }
            };

        private const string CountryLabelEngland = "England";
        private const string CountryCodeEngland = "E92000001";
        
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
        public DbSet<ReleaseType> ReleaseTypes { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<ReleaseContentSection> ReleaseContentSections { get; set; }
        public virtual DbSet<ReleaseContentBlock> ReleaseContentBlocks { get; set; }
        public DbSet<Update> Update { get; set; }
        public DbSet<User> Users { get; set; }
        public virtual DbSet<UserReleaseRole> UserReleaseRoles { get; set; }

        public DbSet<Comment> Comment { get; set; }
        public DbSet<UserReleaseInvite> UserReleaseInvites { get; set; }

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

            modelBuilder.Entity<Methodology>()
                .Property(b => b.Status)
                .HasConversion(new EnumToStringConverter<MethodologyStatus>())
                .HasDefaultValue(MethodologyStatus.Draft);

            modelBuilder.Entity<Publication>()
                .Property(p => p.LegacyPublicationUrl)
                .HasConversion(
                    p => p.ToString(),
                    p => new Uri(p));

            modelBuilder.Entity<Publication>()
                .OwnsOne(p => p.ExternalMethodology).ToTable("ExternalMethodology");

            modelBuilder.Entity<Release>()
                .Property(r => r.TimePeriodCoverage)
                .HasConversion(new EnumToEnumValueConverter<TimeIdentifier>())
                .HasMaxLength(6);

            modelBuilder.Entity<Release>()
                .Property<List<BasicLink>>("RelatedInformation")
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<BasicLink>>(v));

            modelBuilder.Entity<Release>()
                .HasIndex(r => new {r.OriginalId, r.Version});

            modelBuilder.Entity<Release>()
                .HasOne(r => r.CreatedBy)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Release>()
                .HasOne(r => r.Original)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<IContentBlock>()
                .ToTable("ContentBlock")
                .HasDiscriminator<string>("Type");

            modelBuilder.Entity<ContentSection>()
                .Property(b => b.Type)
                .HasConversion(new EnumToStringConverter<ContentSectionType>());

            modelBuilder.Entity<Release>()
                .Property(b => b.NextReleaseDate)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<PartialDate>(v));

            modelBuilder.Entity<Release>()
                .Property(b => b.Status)
                .HasConversion(new EnumToStringConverter<ReleaseStatus>());

            modelBuilder.Entity<DataBlock>()
                .Property(block => block.Heading)
                .HasColumnName("DataBlock_Heading");

            modelBuilder.Entity<DataBlock>()
                .Property(block => block.DataBlockRequest)
                .HasColumnName("DataBlock_Request")
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<ObservationQueryContext>(v));

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

            modelBuilder.Entity<ReleaseContentSection>()
                .HasKey(item => new {item.ReleaseId, item.ContentSectionId});

            modelBuilder.Entity<ReleaseContentBlock>()
                .HasKey(item => new {item.ReleaseId, item.ContentBlockId});

            modelBuilder.Entity<User>();

            modelBuilder.Entity<UserReleaseRole>()
                .Property(r => r.Role)
                .HasConversion(new EnumToStringConverter<ReleaseRole>());

            modelBuilder.Entity<UserReleaseInvite>()
                .Property(r => r.Role)
                .HasConversion(new EnumToStringConverter<ReleaseRole>());

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

            var absenceReleaseId = new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5");
            var exclusionsReleaseId = new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278");
            var applicationOffersReleaseId = new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717");

            modelBuilder.Entity<Release>().HasData(
                //absence
                new Release
                {
                    Id = absenceReleaseId,
                    ReleaseName = "2016",
                    NextReleaseDate = new PartialDate
                    {
                        Day = "22",
                        Month = "3",
                        Year = "2019"
                    },
                    PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                    Published = new DateTime(2018, 3, 22),
                    Slug = "2016-17",
                    TimePeriodCoverage = TimeIdentifier.AcademicYear,
                    TypeId = new Guid("9d333457-9132-4e55-ae78-c55cb3673d7c"),
                },

                // exclusions
                new Release
                {
                    Id = exclusionsReleaseId,
                    ReleaseName = "2016",
                    NextReleaseDate = new PartialDate
                    {
                        Day = "19",
                        Month = "7",
                        Year = "2019"
                    },
                    PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                    Published = new DateTime(2018, 7, 19),
                    Slug = "2016-17",
                    TimePeriodCoverage = TimeIdentifier.AcademicYear,
                    RelatedInformation = new List<BasicLink>
                    {
                        new BasicLink
                        {
                            Id = new Guid("f3c67bc9-6132-496e-a848-c39dfcd16f49"),
                            Description = "Additional guidance",
                            Url = "http://example.com"
                        },
                        new BasicLink
                        {
                            Id = new Guid("45acb50c-8b21-46b4-989f-36f4b0ee37fb"),
                            Description = "Statistics guide",
                            Url = "http://example.com"
                        }
                    },
                    TypeId = new Guid("9d333457-9132-4e55-ae78-c55cb3673d7c"),
                },

                // Secondary and primary schools applications offers
                new Release
                {
                    Id = applicationOffersReleaseId,
                    ReleaseName = "2018",
                    PublicationId = new Guid("66c8e9db-8bf2-4b0b-b094-cfab25c20b05"),
                    NextReleaseDate = new PartialDate
                    {
                        Day = "14",
                        Month = "6",
                        Year = "2019"
                    },
                    Published = new DateTime(2018, 6, 14),
                    Slug = "2018",
                    TimePeriodCoverage = TimeIdentifier.AcademicYear,
                    TypeId = new Guid("9d333457-9132-4e55-ae78-c55cb3673d7c"),
                }
            );

            modelBuilder.Entity<ContentSection>().HasData(
                // absence
                new ContentSection
                {
                    Id = new Guid("24c6e9a3-1415-4ca5-9f21-b6b51cb7ba94"),
                    Order = 1, Heading = "About these statistics", Caption = "",
                    Type = ContentSectionType.Generic
                },
                new ContentSection
                {
                    Id = new Guid("8965ef44-5ad7-4ab0-a142-78453d6f40af"),
                    Order = 2, Heading = "Pupil absence rates", Caption = "",
                    Type = ContentSectionType.Generic
                },
                new ContentSection
                {
                    Id = new Guid("6f493eee-443a-4403-9069-fef82e2f5788"),
                    Order = 3, Heading = "Persistent absence", Caption = "",
                    Type = ContentSectionType.Generic
                },
                new ContentSection
                {
                    Id = new Guid("fbf99442-3b72-46bc-836d-8866c552c53d"),
                    Order = 4, Heading = "Reasons for absence", Caption = "",
                    Type = ContentSectionType.Generic
                },
                new ContentSection
                {
                    Id = new Guid("6898538c-3f8d-488d-9e50-12ca7a9fd70c"),
                    Order = 5, Heading = "Distribution of absence", Caption = "",
                    Type = ContentSectionType.Generic
                },
                new ContentSection
                {
                    Id = new Guid("08b204a2-0eeb-4797-9e0b-a1274e7f6a38"),
                    Order = 6, Heading = "Absence by pupil characteristics", Caption = "",
                    Type = ContentSectionType.Generic
                },
                new ContentSection
                {
                    Id = new Guid("60f8c7ca-faff-4f0d-937d-17fe376461cf"),
                    Order = 7, Heading = "Absence for 4-year-olds", Caption = "",
                    Type = ContentSectionType.Generic
                },
                new ContentSection
                {
                    Id = new Guid("d5d604af-6b63-4a51-b106-0c09b8dbedfa"),
                    Order = 8, Heading = "Pupil referral unit absence", Caption = "",
                    Type = ContentSectionType.Generic
                },
                new ContentSection
                {
                    Id = new Guid("68e3028c-1291-42b3-9e7c-9be285dac9a1"),
                    Order = 9, Heading = "Regional and local authority (LA) breakdown", Caption = "",
                    Type = ContentSectionType.Generic
                },

                // exclusions
                new ContentSection
                {
                    Id = new Guid("b7a968ab-eb49-4100-b133-3d9d94f23d60"),
                    Order = 1, Heading = "About this release", Caption = "",
                    Type = ContentSectionType.Generic
                },
                new ContentSection
                {
                    Id = new Guid("6ed87fd1-81a5-46dc-8841-4598bdae7fee"),
                    Order = 2, Heading = "Permanent exclusions", Caption = "",
                    Type = ContentSectionType.Generic
                },
                new ContentSection
                {
                    Id = new Guid("7981db34-afdb-4f84-99e8-bfd43e58f16d"),
                    Order = 3, Heading = "Fixed-period exclusions", Caption = "",
                    Type = ContentSectionType.Generic
                },
                new ContentSection
                {
                    Id = new Guid("50e7ca4c-e6c7-4ccd-afc1-93ee4298f358"),
                    Order = 4, Heading = "Number and length of fixed-period exclusions", Caption = "",
                    Type = ContentSectionType.Generic
                },
                new ContentSection
                {
                    Id = new Guid("015d0cdd-6630-4b57-9ef3-7341fc3d573e"),
                    Order = 5, Heading = "Reasons for exclusions", Caption = "",
                    Type = ContentSectionType.Generic
                },
                new ContentSection
                {
                    Id = new Guid("5600ca55-6800-418a-94a5-2f3c3310304e"),
                    Order = 6, Heading = "Exclusions by pupil characteristics", Caption = "",
                    Type = ContentSectionType.Generic
                },
                new ContentSection
                {
                    Id = new Guid("68f8b290-4b7c-4cac-b0d9-0263609c341b"),
                    Order = 7, Heading = "Independent exclusion reviews", Caption = "",
                    Type = ContentSectionType.Generic
                },
                new ContentSection
                {
                    Id = new Guid("5708d443-7669-47d8-b6a3-6ad851090710"),
                    Order = 8, Heading = "Pupil referral units exclusions", Caption = "",
                    Type = ContentSectionType.Generic
                },
                new ContentSection
                {
                    Id = new Guid("3960ab94-0fad-442c-8aaa-6233eff3bc32"),
                    Order = 9, Heading = "Regional and local authority (LA) breakdown", Caption = "",
                    Type = ContentSectionType.Generic
                },

                // Secondary and primary schools applications offers
                new ContentSection
                {
                    Id = new Guid("def347bd-0b29-405f-a11f-cd03c853a6ed"),
                    Order = 1, Heading = "About this release", Caption = "",
                    Type = ContentSectionType.Generic
                },
                new ContentSection
                {
                    Id = new Guid("6bfa9b19-25d6-4d45-8008-9447db541795"),
                    Order = 2, Heading = "Secondary applications and offers", Caption = "",
                    Type = ContentSectionType.Generic
                },
                new ContentSection
                {
                    Id = new Guid("c1f17b4e-f576-40bc-80e1-63767998d080"),
                    Order = 3, Heading = "Secondary geographical variation", Caption = "",
                    Type = ContentSectionType.Generic
                },
                new ContentSection
                {
                    Id = new Guid("c3eb66d0-ce13-4e68-861d-98bb914d0814"),
                    Order = 4, Heading = "Primary applications and offers", Caption = "",
                    Type = ContentSectionType.Generic
                },
                new ContentSection
                {
                    Id = new Guid("b87f2e62-e3e7-4492-9d68-18df8dc29041"),
                    Order = 5, Heading = "Primary geographical variation", Caption = "",
                    Type = ContentSectionType.Generic
                },

                // Summary sections for each Release
                new ContentSection
                {
                    Id = new Guid("4f30b382-ce28-4a3e-801a-ce76004f5eb4"),
                    Order = 1, Heading = "", Caption = "",
                    Type = ContentSectionType.ReleaseSummary
                },
                new ContentSection
                {
                    Id = new Guid("f599c2e2-f215-423a-beab-c5c6a0c2e5a9"),
                    Order = 1, Heading = "", Caption = "",
                    Type = ContentSectionType.ReleaseSummary
                },
                new ContentSection
                {
                    Id = new Guid("93ef0486-479f-4013-8012-a66ed01f1880"),
                    Order = 1, Heading = "", Caption = "",
                    Type = ContentSectionType.ReleaseSummary
                },

                // Key Statistics sections for each Release
                new ContentSection
                {
                    Id = new Guid("7b779d79-6caa-43fd-84ba-b8efd219b3c8"),
                    Order = 1, Heading = "", Caption = "",
                    Type = ContentSectionType.KeyStatistics
                },
                new ContentSection
                {
                    Id = new Guid("991a436a-9c7a-418b-ab06-60f2610b4bc6"),
                    Order = 1, Heading = "", Caption = "",
                    Type = ContentSectionType.KeyStatistics
                },
                new ContentSection
                {
                    Id = new Guid("de8f8547-cbae-4d52-88ec-d78d0ad836ae"),
                    Order = 1, Heading = "", Caption = "",
                    Type = ContentSectionType.KeyStatistics
                },

                // Key Statistics secondary sections for each Release
                new ContentSection
                {
                    Id = new Guid("30d74065-66b8-4843-9761-4578519e1394"),
                    Order = 1, Heading = "", Caption = "",
                    Type = ContentSectionType.KeyStatisticsSecondary
                },
                new ContentSection
                {
                    Id = new Guid("e8a813ce-c68a-417b-af31-91db19377b10"),
                    Order = 1, Heading = "", Caption = "",
                    Type = ContentSectionType.KeyStatisticsSecondary
                },
                new ContentSection
                {
                    Id = new Guid("39c298e9-6c5f-47be-85cb-6e49b1b1931f"),
                    Order = 1, Heading = "", Caption = "",
                    Type = ContentSectionType.KeyStatisticsSecondary
                },

                // Headline sections for each Release
                new ContentSection
                {
                    Id = new Guid("c0241ab7-f40a-4755-bc69-365eba8114a3"),
                    Order = 1, Heading = "", Caption = "",
                    Type = ContentSectionType.Headlines
                },
                new ContentSection
                {
                    Id = new Guid("601aadcc-be7d-4d3e-9154-c9eb64144692"),
                    Order = 1, Heading = "", Caption = "",
                    Type = ContentSectionType.Headlines
                },
                new ContentSection
                {
                    Id = new Guid("8abdae8f-4119-41ac-8efd-2229b7ea31da"),
                    Order = 1, Heading = "", Caption = "",
                    Type = ContentSectionType.Headlines
                }
            );

            modelBuilder.Entity<ReleaseContentSection>().HasData(
                // absence
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("24c6e9a3-1415-4ca5-9f21-b6b51cb7ba94"),
                    ReleaseId = absenceReleaseId,
                },
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("8965ef44-5ad7-4ab0-a142-78453d6f40af"),
                    ReleaseId = absenceReleaseId,
                },
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("6f493eee-443a-4403-9069-fef82e2f5788"),
                    ReleaseId = absenceReleaseId,
                },
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("fbf99442-3b72-46bc-836d-8866c552c53d"),
                    ReleaseId = absenceReleaseId,
                },
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("6898538c-3f8d-488d-9e50-12ca7a9fd70c"),
                    ReleaseId = absenceReleaseId,
                },
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("08b204a2-0eeb-4797-9e0b-a1274e7f6a38"),
                    ReleaseId = absenceReleaseId,
                },
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("60f8c7ca-faff-4f0d-937d-17fe376461cf"),
                    ReleaseId = absenceReleaseId,
                },
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("d5d604af-6b63-4a51-b106-0c09b8dbedfa"),
                    ReleaseId = absenceReleaseId,
                },
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("68e3028c-1291-42b3-9e7c-9be285dac9a1"),
                    ReleaseId = absenceReleaseId,
                },

                // exclusions
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("b7a968ab-eb49-4100-b133-3d9d94f23d60"),
                    ReleaseId = exclusionsReleaseId,
                },
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("6ed87fd1-81a5-46dc-8841-4598bdae7fee"),
                    ReleaseId = exclusionsReleaseId,
                },
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("7981db34-afdb-4f84-99e8-bfd43e58f16d"),
                    ReleaseId = exclusionsReleaseId,
                },
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("50e7ca4c-e6c7-4ccd-afc1-93ee4298f358"),
                    ReleaseId = exclusionsReleaseId,
                },
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("015d0cdd-6630-4b57-9ef3-7341fc3d573e"),
                    ReleaseId = exclusionsReleaseId,
                },
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("5600ca55-6800-418a-94a5-2f3c3310304e"),
                    ReleaseId = exclusionsReleaseId,
                },
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("68f8b290-4b7c-4cac-b0d9-0263609c341b"),
                    ReleaseId = exclusionsReleaseId,
                },
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("5708d443-7669-47d8-b6a3-6ad851090710"),
                    ReleaseId = exclusionsReleaseId,
                },
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("3960ab94-0fad-442c-8aaa-6233eff3bc32"),
                    ReleaseId = exclusionsReleaseId,
                },

                // Secondary and primary schools applications offers
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("def347bd-0b29-405f-a11f-cd03c853a6ed"),
                    ReleaseId = applicationOffersReleaseId,
                },
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("6bfa9b19-25d6-4d45-8008-9447db541795"),
                    ReleaseId = applicationOffersReleaseId,
                },
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("c1f17b4e-f576-40bc-80e1-63767998d080"),
                    ReleaseId = applicationOffersReleaseId,
                },
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("c3eb66d0-ce13-4e68-861d-98bb914d0814"),
                    ReleaseId = applicationOffersReleaseId,
                },
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("b87f2e62-e3e7-4492-9d68-18df8dc29041"),
                    ReleaseId = applicationOffersReleaseId,
                },

                // Summary sections for each Release
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("4f30b382-ce28-4a3e-801a-ce76004f5eb4"),
                    ReleaseId = absenceReleaseId,
                },
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("f599c2e2-f215-423a-beab-c5c6a0c2e5a9"),
                    ReleaseId = exclusionsReleaseId,
                },
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("93ef0486-479f-4013-8012-a66ed01f1880"),
                    ReleaseId = applicationOffersReleaseId,
                },

                // Key Statistics sections for each Release
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("7b779d79-6caa-43fd-84ba-b8efd219b3c8"),
                    ReleaseId = absenceReleaseId,
                },
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("991a436a-9c7a-418b-ab06-60f2610b4bc6"),
                    ReleaseId = exclusionsReleaseId,
                },
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("de8f8547-cbae-4d52-88ec-d78d0ad836ae"),
                    ReleaseId = applicationOffersReleaseId,
                },

                // Key Statistics secondary sections for each Release
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("30d74065-66b8-4843-9761-4578519e1394"),
                    ReleaseId = absenceReleaseId,
                },
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("e8a813ce-c68a-417b-af31-91db19377b10"),
                    ReleaseId = exclusionsReleaseId,
                },
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("39c298e9-6c5f-47be-85cb-6e49b1b1931f"),
                    ReleaseId = applicationOffersReleaseId,
                },

                // Headline sections for each Release
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("c0241ab7-f40a-4755-bc69-365eba8114a3"),
                    ReleaseId = absenceReleaseId,
                },
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("601aadcc-be7d-4d3e-9154-c9eb64144692"),
                    ReleaseId = exclusionsReleaseId,
                },
                new ReleaseContentSection
                {
                    ContentSectionId = new Guid("8abdae8f-4119-41ac-8efd-2229b7ea31da"),
                    ReleaseId = applicationOffersReleaseId,
                }
            );

            modelBuilder.Entity<ReleaseContentBlock>().HasData(
                // absence key stats tile 1 data block
                new ReleaseContentBlock
                {
                    ContentBlockId = new Guid("9ccb0daf-91a1-4cb0-b3c1-2aed452338bc"),
                    ReleaseId = absenceReleaseId,
                },
                // absence key stats tile 2 data block
                new ReleaseContentBlock
                {
                    ContentBlockId = new Guid("3da30a08-9eeb-4a99-9872-796c3ea518fa"),
                    ReleaseId = absenceReleaseId,
                },
                // absence key stats tile 3 data block
                new ReleaseContentBlock
                {
                    ContentBlockId = new Guid("045a9585-688f-46fa-b3a9-9bdc237e0381"),
                    ReleaseId = absenceReleaseId,
                },
                // absence key stats aggregate table data block
                new ReleaseContentBlock
                {
                    ContentBlockId = new Guid("5d1e6b67-26d7-4440-9e77-c0de71a9fc21"),
                    ReleaseId = absenceReleaseId,
                },
                // absence generic content data block
                new ReleaseContentBlock
                {
                    ContentBlockId = new Guid("5d3058f2-459e-426a-b0b3-9f60d8629fef"),
                    ReleaseId = absenceReleaseId,
                },
                // absence generic content data block
                new ReleaseContentBlock
                {
                    ContentBlockId = new Guid("4a1af98a-ed8a-438e-92d4-d21cca0429f9"),
                    ReleaseId = absenceReleaseId,
                },
                // exclusions key stats tile 1 data block
                new ReleaseContentBlock
                {
                    ContentBlockId = new Guid("d0397918-1697-40d8-b649-bea3c63c7d3e"),
                    ReleaseId = exclusionsReleaseId,
                },
                // exclusions key stats tile 2 data block
                new ReleaseContentBlock
                {
                    ContentBlockId = new Guid("695de169-947f-4f66-8564-6392b6113dfc"),
                    ReleaseId = exclusionsReleaseId,
                },
                // exclusions key stats tile 3 data block
                new ReleaseContentBlock
                {
                    ContentBlockId = new Guid("17251e1c-e978-419c-98f5-963131c952f7"),
                    ReleaseId = exclusionsReleaseId,
                },
                // exclusions aggregate table data block
                new ReleaseContentBlock
                {
                    ContentBlockId = new Guid("17a0272b-318d-41f6-bda9-3bd88f78cd3d"),
                    ReleaseId = exclusionsReleaseId,
                },
                // exclusions generic content data block
                new ReleaseContentBlock
                {
                    ContentBlockId = new Guid("dd572e49-87e3-46f5-bb04-e9008573fc91"),
                    ReleaseId = exclusionsReleaseId,
                },
                // exclusions generic content data block
                new ReleaseContentBlock
                {
                    ContentBlockId = new Guid("038093a2-0be3-440b-8b22-8116e34aa616"),
                    ReleaseId = exclusionsReleaseId,
                },
                // exclusions detached data block
                new ReleaseContentBlock
                {
                    ContentBlockId = new Guid("1869d10a-ca3f-450c-9685-780b11d916f5"),
                    ReleaseId = exclusionsReleaseId,
                },
                // exclusions detached data block 2
                new ReleaseContentBlock
                {
                    ContentBlockId = new Guid("0b4c43cd-fc12-4159-88b9-0c8646424555"),
                    ReleaseId = exclusionsReleaseId,
                },
                // Secondary and primary schools applications key stats tile 1 data block
                new ReleaseContentBlock
                {
                    ContentBlockId = new Guid("5947759d-c6f3-451b-b353-a4da063f020a"),
                    ReleaseId = applicationOffersReleaseId,
                },
                // Secondary and primary schools applications key stats tile 2 data block
                new ReleaseContentBlock
                {
                    ContentBlockId = new Guid("02a637e7-6cc7-44e5-8991-8982edfe49fc"),
                    ReleaseId = applicationOffersReleaseId,
                },
                // Secondary and primary schools applications key stats tile 3 data block
                new ReleaseContentBlock
                {
                    ContentBlockId = new Guid("5d5f9b1f-8d0d-47d4-ba2b-ea97413d3117"),
                    ReleaseId = applicationOffersReleaseId,
                },
                // Secondary and primary schools applications aggregate table data block
                new ReleaseContentBlock
                {
                    ContentBlockId = new Guid("475738b4-ba10-4c29-a50d-6ca82c10de6e"),
                    ReleaseId = applicationOffersReleaseId,
                },
                // Secondary and primary schools applications generic content data block
                new ReleaseContentBlock
                {
                    ContentBlockId = new Guid("52916052-81e3-4b66-80b8-24f8666d9cbf"),
                    ReleaseId = applicationOffersReleaseId,
                },
                // Secondary and primary schools applications generic content data block
                new ReleaseContentBlock
                {
                    ContentBlockId = new Guid("a8c408ed-45d8-4690-a9f3-2fb0e86377bf"),
                    ReleaseId = applicationOffersReleaseId,
                }
            );

            modelBuilder.Entity<Comment>()
                .HasData(
                    new Comment
                    {
                        Id = new Guid("514940e6-3b84-4e1b-aa5d-d1e5fa671e1b"),
                        IContentBlockId = new Guid("a0b85d7d-a9bd-48b5-82c6-a119adc74ca2"),
                        CommentText = "Test Text",
                        Name = "A Test User",
                        State = CommentState.open,
                        Time = new DateTime(2019, 12, 1, 15, 0, 0),
                        ResolvedBy = null,
                        ResolvedOn = null
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
                },

                // Summary sections for each Release
                new MarkDownBlock
                {
                    Id = new Guid("a0b85d7d-a9bd-48b5-82c6-a119adc74ca2"),
                    ContentSectionId = new Guid("4f30b382-ce28-4a3e-801a-ce76004f5eb4"),
                    Order = 1,
                    Body =
                        "Read national statistical summaries, view charts and tables and download data files.\n\n" +
                        "Find out how and why these statistics are collected and published - [Pupil absence statistics: methodology](../methodology/pupil-absence-in-schools-in-england)."
                },
                new MarkDownBlock
                {
                    Id = new Guid("7934e93d-2e11-478d-ab0e-f799f15164bb"),
                    ContentSectionId = new Guid("f599c2e2-f215-423a-beab-c5c6a0c2e5a9"),
                    Order = 1,
                    Body = "Read national statistical summaries, view charts and tables and download " +
                           "data files.\n\nFind out how and why these statistics are collected and " +
                           "published - [Permanent and fixed-period exclusion statistics: methodology]" +
                           "(../methodology/permanent-and-fixed-period-exclusions-in-england)",
                },
                new MarkDownBlock
                {
                    Id = new Guid("31c6b325-cbfa-4108-9956-cde7fa6a99ec"),
                    ContentSectionId = new Guid("93ef0486-479f-4013-8012-a66ed01f1880"),
                    Order = 1,
                    Body = "Read national statistical summaries, view charts and tables and download " +
                           "data files.\n\nFind out how and why these statistics are collected and " +
                           "published - [Secondary and primary school applications and offers: methodology]" +
                           "(../methodology/secondary-and-primary-schools-applications-and-offers)"
                },

                // Headline section for each Release
                new MarkDownBlock
                {
                    Id = new Guid("b9732ba9-8dc3-4fbc-9c9b-e504e4b58fb9"),
                    ContentSectionId = new Guid("93ef0486-479f-4013-8012-a66ed01f1880"),
                    Order = 1,
                    Body = " * pupils missed on average 8.2 school days\n" +
                           " * overall and unauthorised absence rates up on 2015/16\n" +
                           " * unauthorised absence rise due to higher rates of unauthorised holidays\n" +
                           " * 10% of pupils persistently absent during 2016/17"
                },
                new MarkDownBlock
                {
                    Id = new Guid("db00f19a-96b7-47c9-84eb-92d6ace41434"),
                    ContentSectionId = new Guid("601aadcc-be7d-4d3e-9154-c9eb64144692"),
                    Order = 1,
                    Body =
                        "* majority of applicants received a preferred offer\n" +
                        "* percentage of applicants receiving secondary first choice offers decreases as applications increase\n" +
                        "* slight proportional increase in applicants receiving primary first choice offer as applications decrease\n"
                },
                new MarkDownBlock
                {
                    Id = new Guid("8a108b91-ff08-4866-9566-cf03e33cd4ec"),
                    ContentSectionId = new Guid("8abdae8f-4119-41ac-8efd-2229b7ea31da"),
                    Order = 1,
                    Body =
                        "* majority of applicants received a preferred offer\n" +
                        "* percentage of applicants receiving secondary first choice offers decreases as applications increase\n" +
                        "* slight proportional increase in applicants receiving primary first choice offer as applications decrease\n"
                }
            );

            modelBuilder.Entity<DataBlock>().HasData(
                // absence key statistics tile 1
                new DataBlock
                {
                    Id = new Guid("9ccb0daf-91a1-4cb0-b3c1-2aed452338bc"),
                    ContentSectionId = new Guid("7b779d79-6caa-43fd-84ba-b8efd219b3c8"),
                    Order = 1,
                    Name = "Key Stat 1",
                    DataBlockRequest = new ObservationQueryContext
                    {
                        SubjectId = SubjectIds[SubjectName.AbsenceByCharacteristic],
                        Country = new List<string>
                        {
                            CountryCodeEngland
                        },
                        TimePeriod = new TimePeriodQuery
                        {
                            StartYear = 2012,
                            StartCode = TimeIdentifier.AcademicYear,
                            EndYear = 2016,
                            EndCode = TimeIdentifier.AcademicYear
                        },
                        Filters = new List<Guid>
                        {
                            FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                FilterItemName.Characteristic__Total),
                            FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.School_Type__Total)
                        },
                        Indicators = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                IndicatorName.Overall_absence_rate),
                        }
                    },

                    Summary = new Summary
                    {
                        dataKeys = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                IndicatorName.Overall_absence_rate)
                        },
                        dataDefinitionTitle = new List<string>
                        {
                            "What is overall absence?"
                        },
                        dataSummary = new List<string>
                        {
                            "Up from 4.6% in 2015/16",
                        },
                        dataDefinition = new List<string>
                        {
                            @"Total number of all authorised and unauthorised absences from possible school sessions for all pupils. <a href=""/glossary#overall-absence"">More >>></a>",
                        }
                    },
                    Tables = new List<Table>
                    {
                        new Table
                        {
                            tableHeaders = new TableHeaders
                            {
                                columnGroups = new List<List<TableOption>>(),
                                columns = new List<TableOption>
                                {
                                    new TableOption("2012/13", "2012_AY"),
                                    new TableOption("2013/14", "2013_AY"),
                                    new TableOption("2014/15", "2014_AY"),
                                    new TableOption("2015/16", "2015_AY"),
                                    new TableOption("2016/17", "2016_AY")
                                },
                                rowGroups = new List<List<TableRowGroupOption>>
                                {
                                    new List<TableRowGroupOption>
                                    {
                                        new TableRowGroupOption(CountryLabelEngland, GeographicLevel.Country.ToString().CamelCase(), CountryCodeEngland)
                                    }
                                },
                                rows = new List<TableOption>
                                {
                                    new TableOption("Overall absence rate", Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic], IndicatorName.Overall_absence_rate).ToString())
                                }
                            }
                        }
                    },
                    Charts = new List<IContentBlockChart>
                    {
                        new LineChart
                        {
                            Axes = new Dictionary<string, ChartAxisConfigurationItem>
                            {
                                ["major"] = new ChartAxisConfigurationItem
                                {
                                    GroupBy = AxisGroupBy.timePeriod,
                                    DataSets = new List<ChartDataSet>
                                    {
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                IndicatorName.Unauthorised_absence_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.Characteristic__Total),
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        },
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                IndicatorName.Overall_absence_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.Characteristic__Total),
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        },
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                IndicatorName.Authorised_absence_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.Characteristic__Total),
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        }
                                    },
                                    Title = "School Year"
                                },
                                ["minor"] = new ChartAxisConfigurationItem
                                {
                                    Min = 0,
                                    Title = "Absence Rate"
                                }
                            },
                            Labels = new Dictionary<string, ChartConfiguration>
                            {
                                [$"{Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic], IndicatorName.Overall_absence_rate)}_{FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.Characteristic__Total)}_{FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.School_Type__Total)}_____"]
                                    = new ChartConfiguration
                                    {
                                        Label = "Overall absence rate",
                                        Unit = "%",
                                        Colour = "#f5a450",
                                        symbol = ChartSymbol.cross
                                    },
                            },
                            Legend = Legend.top
                        }
                    }
                },
                // absence key statistics tile 2
                new DataBlock
                {
                    Id = new Guid("3da30a08-9eeb-4a99-9872-796c3ea518fa"),
                    ContentSectionId = new Guid("7b779d79-6caa-43fd-84ba-b8efd219b3c8"),
                    Name = "Key Stat 2",
                    Order = 2,
                    DataBlockRequest = new ObservationQueryContext
                    {
                        SubjectId = SubjectIds[SubjectName.AbsenceByCharacteristic],
                        Country = new List<string>
                        {
                            CountryCodeEngland
                        },
                        TimePeriod = new TimePeriodQuery
                        {
                            StartYear = 2012,
                            StartCode = TimeIdentifier.AcademicYear,
                            EndYear = 2016,
                            EndCode = TimeIdentifier.AcademicYear
                        },
                        Filters = new List<Guid>
                        {
                            FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                FilterItemName.Characteristic__Total),
                            FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.School_Type__Total)
                        },
                        Indicators = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                IndicatorName.Authorised_absence_rate)
                        }
                    },

                    Summary = new Summary
                    {
                        dataKeys = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                IndicatorName.Authorised_absence_rate)
                        },
                        dataDefinitionTitle = new List<string>
                        {
                            "What is authorized absence rate?"
                        },
                        dataSummary = new List<string>
                        {
                            "Similar to previous years",
                        },
                        dataDefinition = new List<string>
                        {
                            @"Number of authorised absences as a percentage of the overall school population. <a href=""/glossary#authorised-absence"">More >>></a>",
                        }
                    },
                    Tables = new List<Table>
                    {
                        new Table
                        {
                            tableHeaders = new TableHeaders
                            {
                                columnGroups = new List<List<TableOption>>(),
                                columns = new List<TableOption>
                                {
                                    new TableOption("2012/13", "2012_AY"),
                                    new TableOption("2013/14", "2013_AY"),
                                    new TableOption("2014/15", "2014_AY"),
                                    new TableOption("2015/16", "2015_AY"),
                                    new TableOption("2016/17", "2016_AY")
                                },
                                rowGroups = new List<List<TableRowGroupOption>>
                                {
                                    new List<TableRowGroupOption>
                                    {
                                        new TableRowGroupOption(CountryLabelEngland, GeographicLevel.Country.ToString().CamelCase(), CountryCodeEngland)
                                    }
                                },
                                rows = new List<TableOption>
                                {
                                    new TableOption("Authorised absence rate", Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic], IndicatorName.Authorised_absence_rate).ToString())
                                }
                            }
                        }
                    },
                    Charts = new List<IContentBlockChart>
                    {
                        new LineChart
                        {
                            Axes = new Dictionary<string, ChartAxisConfigurationItem>
                            {
                                ["major"] = new ChartAxisConfigurationItem
                                {
                                    GroupBy = AxisGroupBy.timePeriod,
                                    DataSets = new List<ChartDataSet>
                                    {
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                IndicatorName.Authorised_absence_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.Characteristic__Total),
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        }
                                    },
                                    Title = "School Year"
                                },
                                ["minor"] = new ChartAxisConfigurationItem
                                {
                                    Min = 0,
                                    Title = "Absence Rate"
                                }
                            },
                            Labels = new Dictionary<string, ChartConfiguration>
                            {
                                [$"{Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic], IndicatorName.Authorised_absence_rate)}_{FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.Characteristic__Total)}_{FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.School_Type__Total)}_____"]
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
                // absence key statistics tile 3
                new DataBlock
                {
                    Id = new Guid("045a9585-688f-46fa-b3a9-9bdc237e0381"),
                    ContentSectionId = new Guid("7b779d79-6caa-43fd-84ba-b8efd219b3c8"),
                    Name = "Key Stat 3",
                    Order = 3,
                    DataBlockRequest = new ObservationQueryContext
                    {
                        SubjectId = SubjectIds[SubjectName.AbsenceByCharacteristic],
                        Country = new List<string>
                        {
                            CountryCodeEngland
                        },
                        TimePeriod = new TimePeriodQuery
                        {
                            StartYear = 2012,
                            StartCode = TimeIdentifier.AcademicYear,
                            EndYear = 2016,
                            EndCode = TimeIdentifier.AcademicYear
                        },
                        Filters = new List<Guid>
                        {
                            FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                FilterItemName.Characteristic__Total),
                            FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.School_Type__Total)
                        },
                        Indicators = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                IndicatorName.Unauthorised_absence_rate),
                        }
                    },

                    Summary = new Summary
                    {
                        dataKeys = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                IndicatorName.Unauthorised_absence_rate)
                        },
                        dataDefinitionTitle = new List<string>
                        {
                            "What is unauthorized absence rate?"
                        },
                        dataSummary = new List<string>
                        {
                            "Up from 1.1% in 2015/16"
                        },
                        dataDefinition = new List<string>
                        {
                            @"Number of unauthorised absences as a percentage of the overall school population. <a href=""/glossary#unauthorised-absence"">More >>></a>"
                        }
                    },
                    Tables = new List<Table>
                    {
                        new Table
                        {
                            tableHeaders = new TableHeaders
                            {
                                columnGroups = new List<List<TableOption>>(),
                                columns = new List<TableOption>
                                {
                                    new TableOption("2012/13", "2012_AY"),
                                    new TableOption("2013/14", "2013_AY"),
                                    new TableOption("2014/15", "2014_AY"),
                                    new TableOption("2015/16", "2015_AY"),
                                    new TableOption("2016/17", "2016_AY")
                                },
                                rowGroups = new List<List<TableRowGroupOption>>
                                {
                                    new List<TableRowGroupOption>
                                    {
                                        new TableRowGroupOption(CountryLabelEngland, GeographicLevel.Country.ToString().CamelCase(), CountryCodeEngland)
                                    }
                                },
                                rows = new List<TableOption>
                                {
                                    new TableOption("Unauthorised absence rate", Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic], IndicatorName.Unauthorised_absence_rate).ToString())
                                }
                            }
                        }
                    },
                    Charts = new List<IContentBlockChart>
                    {
                        new LineChart
                        {
                            Axes = new Dictionary<string, ChartAxisConfigurationItem>
                            {
                                ["major"] = new ChartAxisConfigurationItem
                                {
                                    GroupBy = AxisGroupBy.timePeriod,
                                    DataSets = new List<ChartDataSet>
                                    {
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                IndicatorName.Unauthorised_absence_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.Characteristic__Total),
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        },
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                IndicatorName.Overall_absence_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.Characteristic__Total),
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        },
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                IndicatorName.Authorised_absence_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.Characteristic__Total),
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        }
                                    },
                                    Title = "School Year"
                                },
                                ["minor"] = new ChartAxisConfigurationItem
                                {
                                    Min = 0,
                                    Title = "Absence Rate"
                                }
                            },
                            Labels = new Dictionary<string, ChartConfiguration>
                            {
                                [$"{Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic], IndicatorName.Unauthorised_absence_rate)}_{FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.Characteristic__Total)}_{FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.School_Type__Total)}_____"]
                                    = new ChartConfiguration
                                    {
                                        Label = "Unauthorised absence rate",
                                        Unit = "%",
                                        Colour = "#4763a5",
                                        symbol = ChartSymbol.circle
                                    }
                            },
                            Legend = Legend.top
                        }
                    }
                },
                // absence key statistics aggregate table
                new DataBlock
                {
                    Id = new Guid("5d1e6b67-26d7-4440-9e77-c0de71a9fc21"),
                    ContentSectionId = new Guid("30d74065-66b8-4843-9761-4578519e1394"),
                    Name = "Key Stats aggregate table",
                    Order = 1,
                    DataBlockRequest = new ObservationQueryContext
                    {
                        SubjectId = SubjectIds[SubjectName.AbsenceByCharacteristic],
                        Country = new List<string>
                        {
                            CountryCodeEngland
                        },
                        TimePeriod = new TimePeriodQuery
                        {
                            StartYear = 2012,
                            StartCode = TimeIdentifier.AcademicYear,
                            EndYear = 2016,
                            EndCode = TimeIdentifier.AcademicYear
                        },
                        Filters = new List<Guid>
                        {
                            FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                FilterItemName.Characteristic__Total),
                            FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.School_Type__Total)
                        },
                        Indicators = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                IndicatorName.Unauthorised_absence_rate),
                            Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                IndicatorName.Overall_absence_rate),
                            Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                IndicatorName.Authorised_absence_rate)
                        }
                    },

                    Summary = new Summary
                    {
                        dataKeys = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                IndicatorName.Overall_absence_rate),
                            Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                IndicatorName.Authorised_absence_rate),
                            Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                IndicatorName.Unauthorised_absence_rate)
                        },
                        dataDefinitionTitle = new List<string>
                        {
                            "What is overall absence?",
                            "What is authorized absence?",
                            "What is unauthorized absence?"
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
                        }
                    },
                    Tables = new List<Table>
                    {
                        new Table
                        {
                            tableHeaders = new TableHeaders
                            {
                                columnGroups = new List<List<TableOption>>(),
                                columns = new List<TableOption>
                                {
                                    new TableOption("2012/13", "2012_AY"),
                                    new TableOption("2013/14", "2013_AY"),
                                    new TableOption("2014/15", "2014_AY"),
                                    new TableOption("2015/16", "2015_AY"),
                                    new TableOption("2016/17", "2016_AY")
                                },
                                rowGroups = new List<List<TableRowGroupOption>>
                                {
                                    new List<TableRowGroupOption>
                                    {
                                        new TableRowGroupOption(CountryLabelEngland, GeographicLevel.Country.ToString().CamelCase(), CountryCodeEngland)
                                    }
                                },
                                rows = new List<TableOption>
                                {
                                    new TableOption("Authorised absence rate", Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic], IndicatorName.Authorised_absence_rate).ToString()),
                                    new TableOption("Unauthorised absence rate", Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic], IndicatorName.Unauthorised_absence_rate).ToString()),
                                    new TableOption("Overall absence rate", Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic], IndicatorName.Overall_absence_rate).ToString())
                                }
                            }
                        }
                    },
                    Charts = new List<IContentBlockChart>
                    {
                        new LineChart
                        {
                            Axes = new Dictionary<string, ChartAxisConfigurationItem>
                            {
                                ["major"] = new ChartAxisConfigurationItem
                                {
                                    GroupBy = AxisGroupBy.timePeriod,
                                    DataSets = new List<ChartDataSet>
                                    {
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                IndicatorName.Unauthorised_absence_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.Characteristic__Total),
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        },
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                IndicatorName.Overall_absence_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.Characteristic__Total),
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        },
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                IndicatorName.Authorised_absence_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.Characteristic__Total),
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        }
                                    },
                                    Title = "School Year"
                                },
                                ["minor"] = new ChartAxisConfigurationItem
                                {
                                    Min = 0,
                                    Title = "Absence Rate"
                                }
                            },
                            Labels = new Dictionary<string, ChartConfiguration>
                            {
                                [$"{Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic], IndicatorName.Unauthorised_absence_rate)}_{FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.Characteristic__Total)}_{FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.School_Type__Total)}_____"]
                                    = new ChartConfiguration
                                    {
                                        Label = "Unauthorised absence rate",
                                        Unit = "%",
                                        Colour = "#4763a5",
                                        symbol = ChartSymbol.circle
                                    },
                                [$"{Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic], IndicatorName.Overall_absence_rate)}_{FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.Characteristic__Total)}_{FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.School_Type__Total)}_____"]
                                    = new ChartConfiguration
                                    {
                                        Label = "Overall absence rate",
                                        Unit = "%",
                                        Colour = "#f5a450",
                                        symbol = ChartSymbol.cross
                                    },
                                [$"{Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic], IndicatorName.Authorised_absence_rate)}_{FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.Characteristic__Total)}_{FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.School_Type__Total)}_____"]
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
                // absence generic data blocks used in content
                new DataBlock
                {
                    Id = new Guid("5d3058f2-459e-426a-b0b3-9f60d8629fef"),
                    ContentSectionId = new Guid("8965ef44-5ad7-4ab0-a142-78453d6f40af"),
                    Name = "Generic data block - National",
                    DataBlockRequest = new ObservationQueryContext
                    {
                        SubjectId = SubjectIds[SubjectName.AbsenceByCharacteristic],
                        Country = new List<string>
                        {
                            CountryCodeEngland
                        },
                        TimePeriod = new TimePeriodQuery
                        {
                            StartYear = 2012,
                            StartCode = TimeIdentifier.AcademicYear,
                            EndYear = 2016,
                            EndCode = TimeIdentifier.AcademicYear
                        },
                        Filters = new List<Guid>
                        {
                            FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                FilterItemName.Characteristic__Total),
                            FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.School_Type__Total)
                        },
                        Indicators = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                IndicatorName.Unauthorised_absence_rate),
                            Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                IndicatorName.Overall_absence_rate),
                            Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                IndicatorName.Authorised_absence_rate)
                        }
                    },
                    Tables = new List<Table>
                    {
                        new Table
                        {
                            tableHeaders = new TableHeaders
                            {
                                columnGroups = new List<List<TableOption>>(),
                                columns = new List<TableOption>
                                {
                                    new TableOption("2012/13", "2012_AY"),
                                    new TableOption("2013/14", "2013_AY"),
                                    new TableOption("2014/15", "2014_AY"),
                                    new TableOption("2015/16", "2015_AY"),
                                    new TableOption("2016/17", "2016_AY")
                                },
                                rowGroups = new List<List<TableRowGroupOption>>
                                {
                                    new List<TableRowGroupOption>
                                    {
                                        new TableRowGroupOption(CountryLabelEngland, GeographicLevel.Country.ToString().CamelCase(), CountryCodeEngland)
                                    }
                                },
                                rows = new List<TableOption>
                                {
                                    new TableOption("Authorised absence rate", Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic], IndicatorName.Authorised_absence_rate).ToString()),
                                    new TableOption("Unauthorised absence rate", Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic], IndicatorName.Unauthorised_absence_rate).ToString()),
                                    new TableOption("Overall absence rate", Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic], IndicatorName.Overall_absence_rate).ToString())
                                }
                            }
                        }
                    },
                    Charts = new List<IContentBlockChart>
                    {
                        new LineChart
                        {
                            Axes = new Dictionary<string, ChartAxisConfigurationItem>
                            {
                                ["major"] = new ChartAxisConfigurationItem
                                {
                                    GroupBy = AxisGroupBy.timePeriod,
                                    DataSets = new List<ChartDataSet>
                                    {
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                IndicatorName.Unauthorised_absence_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.Characteristic__Total),
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        },
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                IndicatorName.Overall_absence_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.Characteristic__Total),
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        },
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                IndicatorName.Authorised_absence_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.Characteristic__Total),
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        }
                                    },
                                    Title = "School Year"
                                },
                                ["minor"] = new ChartAxisConfigurationItem
                                {
                                    Min = 0,
                                    Title = "Absence Rate"
                                }
                            },
                            Labels = new Dictionary<string, ChartConfiguration>
                            {
                                [$"{Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic], IndicatorName.Unauthorised_absence_rate)}_{FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.Characteristic__Total)}_{FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.School_Type__Total)}_____"]
                                    = new ChartConfiguration
                                    {
                                        Label = "Unauthorised absence rate",
                                        Unit = "%",
                                        Colour = "#4763a5",
                                        symbol = ChartSymbol.circle
                                    },
                                [$"{Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic], IndicatorName.Overall_absence_rate)}_{FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.Characteristic__Total)}_{FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.School_Type__Total)}_____"]
                                    = new ChartConfiguration
                                    {
                                        Label = "Overall absence rate",
                                        Unit = "%",
                                        Colour = "#f5a450",
                                        symbol = ChartSymbol.cross
                                    },
                                [$"{Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic], IndicatorName.Authorised_absence_rate)}_{FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.Characteristic__Total)}_{FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.School_Type__Total)}_____"]
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
                    Name = "Generic data block - LA",
                    DataBlockRequest = new ObservationQueryContext
                    {
                        SubjectId = SubjectIds[SubjectName.AbsenceByCharacteristic],
                        GeographicLevel = GeographicLevel.LocalAuthorityDistrict,
                        TimePeriod = new TimePeriodQuery
                        {
                            StartYear = 2016,
                            StartCode = TimeIdentifier.AcademicYear,
                            EndYear = 2017,
                            EndCode = TimeIdentifier.AcademicYear
                        },

                        Indicators = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                IndicatorName.Unauthorised_absence_rate),
                            Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                IndicatorName.Overall_absence_rate),
                            Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                IndicatorName.Authorised_absence_rate)
                        },
                        Filters = new List<Guid>
                        {
                            FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                FilterItemName.Characteristic__Total),
                            FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.School_Type__Total)
                        }
                    },
                    Charts = new List<IContentBlockChart>
                    {
                        new MapChart
                        {
                            Axes = new Dictionary<string, ChartAxisConfigurationItem>
                            {
                                ["major"] = new ChartAxisConfigurationItem
                                {
                                    GroupBy = AxisGroupBy.timePeriod,
                                    DataSets = new List<ChartDataSet>
                                    {
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                IndicatorName.Unauthorised_absence_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.Characteristic__Total),
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        },
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                IndicatorName.Overall_absence_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.Characteristic__Total),
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        },
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                IndicatorName.Authorised_absence_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.Characteristic__Total),
                                                FItem(SubjectIds[SubjectName.AbsenceByCharacteristic],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        }
                                    },
                                    Title = "School Year"
                                },
                                ["minor"] = new ChartAxisConfigurationItem
                                {
                                    Title = "Absence Rate"
                                }
                            },
                            Labels = new Dictionary<string, ChartConfiguration>
                            {
                                [$"{Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic], IndicatorName.Unauthorised_absence_rate)}_{FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.Characteristic__Total)}_{FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.School_Type__Total)}_____"]
                                    = new ChartConfiguration
                                    {
                                        Label = "Unauthorised absence rate",
                                        Unit = "%",
                                        Colour = "#4763a5",
                                        symbol = ChartSymbol.circle
                                    },
                                [$"{Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic], IndicatorName.Overall_absence_rate)}_{FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.Characteristic__Total)}_{FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.School_Type__Total)}_____"]
                                    = new ChartConfiguration
                                    {
                                        Label = "Overall absence rate",
                                        Unit = "%",
                                        Colour = "#f5a450",
                                        symbol = ChartSymbol.cross
                                    },
                                [$"{Indicator(SubjectIds[SubjectName.AbsenceByCharacteristic], IndicatorName.Authorised_absence_rate)}_{FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.Characteristic__Total)}_{FItem(SubjectIds[SubjectName.AbsenceByCharacteristic], FilterItemName.School_Type__Total)}_____"]
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

                // exclusions key statistics tile 1
                new DataBlock
                {
                    Id = new Guid("d0397918-1697-40d8-b649-bea3c63c7d3e"),
                    ContentSectionId = new Guid("991a436a-9c7a-418b-ab06-60f2610b4bc6"),
                    Name = "Key Stat 1",
                    Order = 1,
                    DataBlockRequest = new ObservationQueryContext
                    {
                        SubjectId = SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                        Country = new List<string>
                        {
                            CountryCodeEngland
                        },
                        TimePeriod = new TimePeriodQuery
                        {
                            StartYear = 2012,
                            StartCode = TimeIdentifier.AcademicYear,
                            EndYear = 2016,
                            EndCode = TimeIdentifier.AcademicYear
                        },

                        Filters = new List<Guid>
                        {
                            FItem(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                FilterItemName.School_Type__Total)
                        },
                        Indicators = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                IndicatorName.Permanent_exclusion_rate),
                        }
                    },
                    Summary = new Summary
                    {
                        dataKeys = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                IndicatorName.Permanent_exclusion_rate),
                        },
                        dataDefinitionTitle = new List<string>
                        {
                            "What is permanent exclusion rate?"
                        },
                        dataSummary = new List<string>
                        {
                            "Up from 0.08% in 2015/16",
                        },
                        dataDefinition = new List<string>
                        {
                            @"Number of permanent exclusions as a percentage of the overall school population. <a href=""/glossary#permanent-exclusion"">More >>></a>",
                        },
                    },
                    Tables = new List<Table>
                    {
                        new Table
                        {
                            tableHeaders = new TableHeaders
                            {
                                columnGroups = new List<List<TableOption>>(),
                                columns = new List<TableOption>
                                {
                                    new TableOption("2012/13", "2012_AY"),
                                    new TableOption("2013/14", "2013_AY"),
                                    new TableOption("2014/15", "2014_AY"),
                                    new TableOption("2015/16", "2015_AY"),
                                    new TableOption("2016/17", "2016_AY")
                                },
                                rowGroups = new List<List<TableRowGroupOption>>
                                {
                                    new List<TableRowGroupOption>
                                    {
                                        new TableRowGroupOption(CountryLabelEngland, GeographicLevel.Country.ToString().CamelCase(), CountryCodeEngland)
                                    }
                                },
                                rows = new List<TableOption>
                                {
                                    new TableOption("Permanent exclusion rate", Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel], IndicatorName.Permanent_exclusion_rate).ToString())
                                }
                            }
                        }
                    },

                    Charts = new List<IContentBlockChart>
                    {
                        new LineChart
                        {
                            Axes = new Dictionary<string, ChartAxisConfigurationItem>
                            {
                                ["major"] = new ChartAxisConfigurationItem
                                {
                                    GroupBy = AxisGroupBy.timePeriod,
                                    DataSets = new List<ChartDataSet>
                                    {
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                                IndicatorName.Fixed_period_exclusion_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        },
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                                IndicatorName.Percentage_of_pupils_with_fixed_period_exclusions),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        }
                                    },
                                    Title = "School Year"
                                },
                                ["minor"] = new ChartAxisConfigurationItem
                                {
                                    Min = 0,
                                    Title = "Absence Rate"
                                }
                            },
                            Labels = new Dictionary<string, ChartConfiguration>
                            {
                                [$"{Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel], IndicatorName.Permanent_exclusion_rate)}_{FItem(SubjectIds[SubjectName.ExclusionsByGeographicLevel], FilterItemName.School_Type__Total)}_____"]
                                    =
                                    new ChartConfiguration
                                    {
                                        Label = "Permanent exclusion rate",
                                        Unit = "%",
                                        Colour = "#4763a5",
                                        symbol = ChartSymbol.circle
                                    },
                            },
                            Legend = Legend.top
                        }
                    }
                },

                // exclusions key statistics tile 2
                new DataBlock
                {
                    Id = new Guid("695de169-947f-4f66-8564-6392b6113dfc"),
                    ContentSectionId = new Guid("991a436a-9c7a-418b-ab06-60f2610b4bc6"),
                    Name = "Key Stat 2",
                    Order = 2,
                    DataBlockRequest = new ObservationQueryContext
                    {
                        SubjectId = SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                        Country = new List<string>
                        {
                            CountryCodeEngland
                        },
                        TimePeriod = new TimePeriodQuery
                        {
                            StartYear = 2012,
                            StartCode = TimeIdentifier.AcademicYear,
                            EndYear = 2016,
                            EndCode = TimeIdentifier.AcademicYear
                        },

                        Filters = new List<Guid>
                        {
                            FItem(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                FilterItemName.School_Type__Total)
                        },
                        Indicators = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                IndicatorName.Fixed_period_exclusion_rate),
                        }
                    },
                    Summary = new Summary
                    {
                        dataKeys = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                IndicatorName.Fixed_period_exclusion_rate),
                        },
                        dataDefinitionTitle = new List<string>
                        {
                            "What is fixed period exclusion rate?"
                        },
                        dataSummary = new List<string>
                        {
                            "Up from 4.29% in 2015/16",
                        },
                        dataDefinition = new List<string>
                        {
                            @"Number of fixed-period exclusions as a percentage of the overall school population. <a href=""/glossary#permanent-exclusion"">More >>></a>",
                        }
                    },
                    Tables = new List<Table>
                    {
                        new Table
                        {
                            tableHeaders = new TableHeaders
                            {
                                columnGroups = new List<List<TableOption>>(),
                                columns = new List<TableOption>
                                {
                                    new TableOption("2012/13", "2012_AY"),
                                    new TableOption("2013/14", "2013_AY"),
                                    new TableOption("2014/15", "2014_AY"),
                                    new TableOption("2015/16", "2015_AY"),
                                    new TableOption("2016/17", "2016_AY")
                                },
                                rowGroups = new List<List<TableRowGroupOption>>
                                {
                                    new List<TableRowGroupOption>
                                    {
                                        new TableRowGroupOption(CountryLabelEngland, GeographicLevel.Country.ToString().CamelCase(), CountryCodeEngland)
                                    }
                                },
                                rows = new List<TableOption>
                                {
                                    new TableOption("Fixed period exclusion rate", Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel], IndicatorName.Fixed_period_exclusion_rate).ToString())
                                }
                            }
                        }
                    },
                    Charts = new List<IContentBlockChart>
                    {
                        new LineChart
                        {
                            Axes = new Dictionary<string, ChartAxisConfigurationItem>
                            {
                                ["major"] = new ChartAxisConfigurationItem
                                {
                                    GroupBy = AxisGroupBy.timePeriod,
                                    DataSets = new List<ChartDataSet>
                                    {
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                                IndicatorName.Fixed_period_exclusion_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        },
                                    },
                                    Title = "School Year"
                                },
                                ["minor"] = new ChartAxisConfigurationItem
                                {
                                    Min = 0,
                                    Title = "Absence Rate"
                                }
                            },
                            Labels = new Dictionary<string, ChartConfiguration>
                            {
                                [$"{Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel], IndicatorName.Fixed_period_exclusion_rate)}_{FItem(SubjectIds[SubjectName.ExclusionsByGeographicLevel], FilterItemName.School_Type__Total)}_____"]
                                    =
                                    new ChartConfiguration
                                    {
                                        Label = "Fixed period exclusion rate",
                                        Unit = "%",
                                        Colour = "#4763a5",
                                        symbol = ChartSymbol.circle
                                    },
                            },
                            Legend = Legend.top
                        }
                    }
                },

                // exclusions key statistics tile 3
                new DataBlock
                {
                    Id = new Guid("17251e1c-e978-419c-98f5-963131c952f7"),
                    ContentSectionId = new Guid("991a436a-9c7a-418b-ab06-60f2610b4bc6"),
                    Name = "Key Stat 3",
                    Order = 3,
                    DataBlockRequest = new ObservationQueryContext
                    {
                        SubjectId = SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                        Country = new List<string>
                        {
                            CountryCodeEngland
                        },
                        TimePeriod = new TimePeriodQuery
                        {
                            StartYear = 2012,
                            StartCode = TimeIdentifier.AcademicYear,
                            EndYear = 2016,
                            EndCode = TimeIdentifier.AcademicYear
                        },

                        Filters = new List<Guid>
                        {
                            FItem(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                FilterItemName.School_Type__Total)
                        },
                        Indicators = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                IndicatorName.Number_of_permanent_exclusions)
                        }
                    },
                    Summary = new Summary
                    {
                        dataKeys = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                IndicatorName.Number_of_permanent_exclusions)
                        },
                        dataDefinitionTitle = new List<string>
                        {
                            "What is number of permanent exclusions?"
                        },
                        dataSummary = new List<string>
                        {
                            "Up from 6,685 in 2015/16"
                        },
                        dataDefinition = new List<string>
                        {
                            @"Total number of permanent exclusions within a school year. <a href=""/glossary#permanent-exclusion"">More >>></a>"
                        },
                    },
                    Tables = new List<Table>
                    {
                        new Table
                        {
                            tableHeaders = new TableHeaders
                            {
                                columnGroups = new List<List<TableOption>>(),
                                columns = new List<TableOption>
                                {
                                    new TableOption("2012/13", "2012_AY"),
                                    new TableOption("2013/14", "2013_AY"),
                                    new TableOption("2014/15", "2014_AY"),
                                    new TableOption("2015/16", "2015_AY"),
                                    new TableOption("2016/17", "2016_AY")
                                },
                                rowGroups = new List<List<TableRowGroupOption>>
                                {
                                    new List<TableRowGroupOption>
                                    {
                                        new TableRowGroupOption(CountryLabelEngland, GeographicLevel.Country.ToString().CamelCase(), CountryCodeEngland)
                                    }
                                },
                                rows = new List<TableOption>
                                {
                                    new TableOption("Number of permanent exclusions", Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel], IndicatorName.Number_of_permanent_exclusions).ToString())
                                }
                            }
                        }
                    },

                    Charts = new List<IContentBlockChart>
                    {
                        new LineChart
                        {
                            Axes = new Dictionary<string, ChartAxisConfigurationItem>
                            {
                                ["major"] = new ChartAxisConfigurationItem
                                {
                                    GroupBy = AxisGroupBy.timePeriod,
                                    DataSets = new List<ChartDataSet>
                                    {
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                                IndicatorName.Number_of_permanent_exclusions),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        },
                                    },
                                    Title = "School Year"
                                },
                                ["minor"] = new ChartAxisConfigurationItem
                                {
                                    Min = 0,
                                    Title = "Absence Rate"
                                }
                            },
                            Labels = new Dictionary<string, ChartConfiguration>
                            {
                                [$"{Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel], IndicatorName.Number_of_permanent_exclusions)}_{FItem(SubjectIds[SubjectName.ExclusionsByGeographicLevel], FilterItemName.School_Type__Total)}_____"]
                                    =
                                    new ChartConfiguration
                                    {
                                        Label = "Number of permanent exclusions",
                                        Unit = "",
                                        Colour = "#4763a5",
                                        symbol = ChartSymbol.circle
                                    }
                            },
                            Legend = Legend.top
                        }
                    }
                },

                // exclusions key statistics aggregate table
                new DataBlock
                {
                    Id = new Guid("17a0272b-318d-41f6-bda9-3bd88f78cd3d"),
                    ContentSectionId = new Guid("e8a813ce-c68a-417b-af31-91db19377b10"),
                    Name = "Key Stats aggregate table",
                    Order = 1,
                    DataBlockRequest = new ObservationQueryContext
                    {
                        SubjectId = SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                        Country = new List<string>
                        {
                            CountryCodeEngland
                        },
                        TimePeriod = new TimePeriodQuery
                        {
                            StartYear = 2012,
                            StartCode = TimeIdentifier.AcademicYear,
                            EndYear = 2016,
                            EndCode = TimeIdentifier.AcademicYear
                        },

                        Filters = new List<Guid>
                        {
                            FItem(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                FilterItemName.School_Type__Total)
                        },
                        Indicators = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                IndicatorName.Number_of_schools),
                            Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                IndicatorName.Number_of_pupils),
                            Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                IndicatorName.Number_of_permanent_exclusions),
                            Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                IndicatorName.Permanent_exclusion_rate),
                            Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                IndicatorName.Number_of_fixed_period_exclusions),
                            Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                IndicatorName.Fixed_period_exclusion_rate),
                            Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                IndicatorName.Percentage_of_pupils_with_fixed_period_exclusions)
                        }
                    },
                    Summary = new Summary
                    {
                        dataKeys = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                IndicatorName.Permanent_exclusion_rate),
                            Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                IndicatorName.Fixed_period_exclusion_rate),
                            Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                IndicatorName.Number_of_permanent_exclusions)
                        },
                        dataDefinitionTitle = new List<string>
                        {
                            "What is permanent exclusion rate?",
                            "What is fixed period exclusion rate?",
                            "What is number of permanent exclusions?"
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
                    },
                    Tables = new List<Table>
                    {
                        new Table
                        {
                            tableHeaders = new TableHeaders
                            {
                                columnGroups = new List<List<TableOption>>(),
                                columns = new List<TableOption>
                                {
                                    new TableOption("2012/13", "2012_AY"),
                                    new TableOption("2013/14", "2013_AY"),
                                    new TableOption("2014/15", "2014_AY"),
                                    new TableOption("2015/16", "2015_AY"),
                                    new TableOption("2016/17", "2016_AY")
                                },
                                rowGroups = new List<List<TableRowGroupOption>>
                                {
                                    new List<TableRowGroupOption>
                                    {
                                        new TableRowGroupOption(CountryLabelEngland, GeographicLevel.Country.ToString().CamelCase(), CountryCodeEngland)
                                    }
                                },
                                rows = new List<TableOption>
                                {
                                    new TableOption("Permanent exclusion rate", Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel], IndicatorName.Permanent_exclusion_rate).ToString()),
                                    new TableOption("Fixed period exclusion rate", Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel], IndicatorName.Fixed_period_exclusion_rate).ToString()),
                                    new TableOption("Number of permanent exclusions", Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel], IndicatorName.Number_of_permanent_exclusions).ToString())
                                }
                            }
                        }
                    },

                    Charts = new List<IContentBlockChart>
                    {
                        new LineChart
                        {
                            Axes = new Dictionary<string, ChartAxisConfigurationItem>
                            {
                                ["major"] = new ChartAxisConfigurationItem
                                {
                                    GroupBy = AxisGroupBy.timePeriod,
                                    DataSets = new List<ChartDataSet>
                                    {
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                                IndicatorName.Fixed_period_exclusion_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        },
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                                IndicatorName.Percentage_of_pupils_with_fixed_period_exclusions),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        }
                                    },
                                    Title = "School Year"
                                },
                                ["minor"] = new ChartAxisConfigurationItem
                                {
                                    Min = 0,
                                    Title = "Absence Rate"
                                }
                            },
                            Labels = new Dictionary<string, ChartConfiguration>
                            {
                                [$"{Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel], IndicatorName.Fixed_period_exclusion_rate)}_{FItem(SubjectIds[SubjectName.ExclusionsByGeographicLevel], FilterItemName.School_Type__Total)}_____"]
                                    =
                                    new ChartConfiguration
                                    {
                                        Label = "Fixed period exclusion rate",
                                        Unit = "%",
                                        Colour = "#4763a5",
                                        symbol = ChartSymbol.circle
                                    },
                                [$"{Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel], IndicatorName.Percentage_of_pupils_with_fixed_period_exclusions)}_{FItem(SubjectIds[SubjectName.ExclusionsByGeographicLevel], FilterItemName.School_Type__Total)}_____"]
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
                // exclusions generic data blocks used in content
                new DataBlock
                {
                    Id = new Guid("dd572e49-87e3-46f5-bb04-e9008573fc91"),
                    ContentSectionId = new Guid("6ed87fd1-81a5-46dc-8841-4598bdae7fee"),
                    Heading = "Chart showing permanent exclusions in England",
                    Name = "Generic data block 1",
                    DataBlockRequest = new ObservationQueryContext
                    {
                        SubjectId = SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                        Country = new List<string>
                        {
                            CountryCodeEngland
                        },
                        TimePeriod = new TimePeriodQuery
                        {
                            StartYear = 2012,
                            StartCode = TimeIdentifier.AcademicYear,
                            EndYear = 2016,
                            EndCode = TimeIdentifier.AcademicYear
                        },
                        Filters = new List<Guid>
                        {
                            FItem(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                FilterItemName.School_Type__Total)
                        },
                        Indicators = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                IndicatorName.Permanent_exclusion_rate),
                            Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                IndicatorName.Number_of_pupils),
                            Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                IndicatorName.Number_of_permanent_exclusions)
                        }
                    },
                    Tables = new List<Table>
                    {
                        new Table
                        {
                            tableHeaders = new TableHeaders
                            {
                                columnGroups = new List<List<TableOption>>(),
                                columns = new List<TableOption>
                                {
                                    new TableOption("2012/13", "2012_AY"),
                                    new TableOption("2013/14", "2013_AY"),
                                    new TableOption("2014/15", "2014_AY"),
                                    new TableOption("2015/16", "2015_AY"),
                                    new TableOption("2016/17", "2016_AY")
                                },
                                rowGroups = new List<List<TableRowGroupOption>>
                                {
                                    new List<TableRowGroupOption>
                                    {
                                        new TableRowGroupOption(CountryLabelEngland, GeographicLevel.Country.ToString().CamelCase(), CountryCodeEngland)
                                    }
                                },
                                rows = new List<TableOption>
                                {
                                    new TableOption("Number of pupils", Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel], IndicatorName.Number_of_pupils).ToString()),
                                    new TableOption("Number of permanent exclusions", Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel], IndicatorName.Number_of_permanent_exclusions).ToString()),
                                    new TableOption("Permanent exclusion rate", Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel], IndicatorName.Permanent_exclusion_rate).ToString())
                                }
                            }
                        }
                    },
                    Charts = new List<IContentBlockChart>
                    {
                        new LineChart
                        {
                            Axes = new Dictionary<string, ChartAxisConfigurationItem>
                            {
                                ["major"] = new ChartAxisConfigurationItem
                                {
                                    GroupBy = AxisGroupBy.timePeriod,
                                    DataSets = new List<ChartDataSet>
                                    {
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                                IndicatorName.Permanent_exclusion_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        }
                                    },
                                    Title = "School Year"
                                },
                                ["minor"] = new ChartAxisConfigurationItem
                                {
                                    Min = 0,
                                    Title = "Exclusion Rate"
                                }
                            },
                            Labels = new Dictionary<string, ChartConfiguration>
                            {
                                [$"{Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel], IndicatorName.Permanent_exclusion_rate)}_{FItem(SubjectIds[SubjectName.ExclusionsByGeographicLevel], FilterItemName.School_Type__Total)}_____"]
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
                    Name = "Generic data block 2",
                    DataBlockRequest = new ObservationQueryContext
                    {
                        SubjectId = SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                        Country = new List<string>
                        {
                            CountryCodeEngland
                        },
                        TimePeriod = new TimePeriodQuery
                        {
                            StartYear = 2012,
                            StartCode = TimeIdentifier.AcademicYear,
                            EndYear = 2016,
                            EndCode = TimeIdentifier.AcademicYear
                        },

                        Filters = new List<Guid>
                        {
                            FItem(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                FilterItemName.School_Type__Total)
                        },
                        Indicators = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                IndicatorName.Fixed_period_exclusion_rate),
                            Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                IndicatorName.Number_of_pupils),
                            Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                IndicatorName.Number_of_fixed_period_exclusions)
                        }
                    },
                    Tables = new List<Table>
                    {
                        new Table
                        {
                            tableHeaders = new TableHeaders
                            {
                                columnGroups = new List<List<TableOption>>(),
                                columns = new List<TableOption>
                                {
                                    new TableOption("2012/13", "2012_AY"),
                                    new TableOption("2013/14", "2013_AY"),
                                    new TableOption("2014/15", "2014_AY"),
                                    new TableOption("2015/16", "2015_AY"),
                                    new TableOption("2016/17", "2016_AY")
                                },
                                rowGroups = new List<List<TableRowGroupOption>>
                                {
                                    new List<TableRowGroupOption>
                                    {
                                        new TableRowGroupOption(CountryLabelEngland, GeographicLevel.Country.ToString().CamelCase(), CountryCodeEngland)
                                    }
                                },
                                rows = new List<TableOption>
                                {
                                    new TableOption("Number of pupils", Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel], IndicatorName.Number_of_pupils).ToString()),
                                    new TableOption("Number of fixed period exclusions", Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel], IndicatorName.Number_of_fixed_period_exclusions).ToString()),
                                    new TableOption("Fixed period exclusion rate", Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel], IndicatorName.Fixed_period_exclusion_rate).ToString())
                                }
                            }
                        }
                    },
                    Charts = new List<IContentBlockChart>
                    {
                        new LineChart
                        {
                            Axes = new Dictionary<string, ChartAxisConfigurationItem>
                            {
                                ["major"] = new ChartAxisConfigurationItem
                                {
                                    GroupBy = AxisGroupBy.timePeriod,
                                    DataSets = new List<ChartDataSet>
                                    {
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                                IndicatorName.Fixed_period_exclusion_rate),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        }
                                    },
                                    Title = "School Year"
                                },
                                ["minor"] = new ChartAxisConfigurationItem
                                {
                                    Min = 0,
                                    Title = "Absence Rate"
                                }
                            },
                            Labels = new Dictionary<string, ChartConfiguration>
                            {
                                [$"{Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel], IndicatorName.Fixed_period_exclusion_rate)}_{FItem(SubjectIds[SubjectName.ExclusionsByGeographicLevel], FilterItemName.School_Type__Total)}_____"]
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

                // exclusions detached Data Block (not yet belonging to any Content Section)
                new DataBlock
                {
                    Id = new Guid("1869d10a-ca3f-450c-9685-780b11d916f5"),
                    ContentSectionId = null, // not yet used in any Content
                    Name = "Available Data Block",
                    Order = 0,
                    DataBlockRequest = new ObservationQueryContext
                    {
                        SubjectId = SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                        Country = new List<string>
                        {
                            CountryCodeEngland
                        },
                        TimePeriod = new TimePeriodQuery
                        {
                            StartYear = 2012,
                            StartCode = TimeIdentifier.AcademicYear,
                            EndYear = 2016,
                            EndCode = TimeIdentifier.AcademicYear
                        },

                        Filters = new List<Guid>
                        {
                            FItem(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                FilterItemName.School_Type__Total)
                        },
                        Indicators = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                IndicatorName.Number_of_permanent_exclusions)
                        }
                    },
                    Summary = new Summary
                    {
                        dataKeys = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                IndicatorName.Number_of_permanent_exclusions)
                        },
                        dataDefinitionTitle = new List<string>
                        {
                            "What is number of permanent exclusions?"
                        },
                        dataSummary = new List<string>
                        {
                            "Up from 6,685 in 2015/16"
                        },
                        dataDefinition = new List<string>
                        {
                            @"Total number of permanent exclusions within a school year. <a href=""/glossary#permanent-exclusion"">More >>></a>"
                        },
                    },
                    Tables = new List<Table>
                    {
                        new Table
                        {
                            tableHeaders = new TableHeaders
                            {
                                columnGroups = new List<List<TableOption>>(),
                                columns = new List<TableOption>
                                {
                                    new TableOption("2012/13", "2012_AY"),
                                    new TableOption("2013/14", "2013_AY"),
                                    new TableOption("2014/15", "2014_AY"),
                                    new TableOption("2015/16", "2015_AY"),
                                    new TableOption("2016/17", "2016_AY")
                                },
                                rowGroups = new List<List<TableRowGroupOption>>
                                {
                                    new List<TableRowGroupOption>
                                    {
                                        new TableRowGroupOption(CountryLabelEngland, GeographicLevel.Country.ToString().CamelCase(), CountryCodeEngland)
                                    }
                                },
                                rows = new List<TableOption>
                                {
                                    new TableOption("Number of permanent exclusions", Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel], IndicatorName.Number_of_permanent_exclusions).ToString())
                                }
                            }
                        }
                    },

                    Charts = new List<IContentBlockChart>
                    {
                        new LineChart
                        {
                            Axes = new Dictionary<string, ChartAxisConfigurationItem>
                            {
                                ["major"] = new ChartAxisConfigurationItem
                                {
                                    GroupBy = AxisGroupBy.timePeriod,
                                    DataSets = new List<ChartDataSet>
                                    {
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                                IndicatorName.Number_of_permanent_exclusions),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        },
                                    },
                                    Title = "School Year"
                                },
                                ["minor"] = new ChartAxisConfigurationItem
                                {
                                    Min = 0,
                                    Title = "Absence Rate"
                                }
                            },
                            Labels = new Dictionary<string, ChartConfiguration>
                            {
                                [$"{Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel], IndicatorName.Number_of_permanent_exclusions)}_{FItem(SubjectIds[SubjectName.ExclusionsByGeographicLevel], FilterItemName.School_Type__Total)}_____"]
                                    =
                                    new ChartConfiguration
                                    {
                                        Label = "Number of permanent exclusions",
                                        Unit = "",
                                        Colour = "#4763a5",
                                        symbol = ChartSymbol.circle
                                    }
                            },
                            Legend = Legend.top
                        }
                    }
                },

                // another exclusions detached Data Block (not yet belonging to any Content Section)
                new DataBlock
                {
                    Id = new Guid("0b4c43cd-fc12-4159-88b9-0c8646424555"),
                    ContentSectionId = null, // not yet used in any Content
                    Name = "Available Data Block 2",
                    Order = 0,
                    DataBlockRequest = new ObservationQueryContext
                    {
                        SubjectId = SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                        Country = new List<string>
                        {
                            CountryCodeEngland
                        },
                        TimePeriod = new TimePeriodQuery
                        {
                            StartYear = 2012,
                            StartCode = TimeIdentifier.AcademicYear,
                            EndYear = 2016,
                            EndCode = TimeIdentifier.AcademicYear
                        },

                        Filters = new List<Guid>
                        {
                            FItem(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                FilterItemName.School_Type__Total)
                        },
                        Indicators = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                IndicatorName.Number_of_permanent_exclusions)
                        }
                    },
                    Summary = new Summary
                    {
                        dataKeys = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                IndicatorName.Number_of_permanent_exclusions)
                        },
                        dataDefinitionTitle = new List<string>
                        {
                            "What is number of permanent exclusions?"
                        },
                        dataSummary = new List<string>
                        {
                            "Up from 6,685 in 2015/16"
                        },
                        dataDefinition = new List<string>
                        {
                            @"Total number of permanent exclusions within a school year. <a href=""/glossary#permanent-exclusion"">More >>></a>"
                        },
                    },
                    Tables = new List<Table>
                    {
                        new Table
                        {
                            tableHeaders = new TableHeaders
                            {
                                columnGroups = new List<List<TableOption>>(),
                                columns = new List<TableOption>
                                {
                                    new TableOption("2012/13", "2012_AY"),
                                    new TableOption("2013/14", "2013_AY"),
                                    new TableOption("2014/15", "2014_AY"),
                                    new TableOption("2015/16", "2015_AY"),
                                    new TableOption("2016/17", "2016_AY")
                                },
                                rowGroups = new List<List<TableRowGroupOption>>
                                {
                                    new List<TableRowGroupOption>
                                    {
                                        new TableRowGroupOption(CountryLabelEngland, GeographicLevel.Country.ToString().CamelCase(), CountryCodeEngland)
                                    }
                                },
                                rows = new List<TableOption>
                                {
                                    new TableOption("Number of permanent exclusions", Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel], IndicatorName.Number_of_permanent_exclusions).ToString())
                                }
                            }
                        }
                    },

                    Charts = new List<IContentBlockChart>
                    {
                        new LineChart
                        {
                            Axes = new Dictionary<string, ChartAxisConfigurationItem>
                            {
                                ["major"] = new ChartAxisConfigurationItem
                                {
                                    GroupBy = AxisGroupBy.timePeriod,
                                    DataSets = new List<ChartDataSet>
                                    {
                                        new ChartDataSet
                                        {
                                            Indicator = Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                                IndicatorName.Number_of_permanent_exclusions),
                                            Filters = new List<Guid>
                                            {
                                                FItem(SubjectIds[SubjectName.ExclusionsByGeographicLevel],
                                                    FilterItemName.School_Type__Total)
                                            }
                                        },
                                    },
                                    Title = "School Year"
                                },
                                ["minor"] = new ChartAxisConfigurationItem
                                {
                                    Min = 0,
                                    Title = "Absence Rate"
                                }
                            },
                            Labels = new Dictionary<string, ChartConfiguration>
                            {
                                [$"{Indicator(SubjectIds[SubjectName.ExclusionsByGeographicLevel], IndicatorName.Number_of_permanent_exclusions)}_{FItem(SubjectIds[SubjectName.ExclusionsByGeographicLevel], FilterItemName.School_Type__Total)}_____"]
                                    =
                                    new ChartConfiguration
                                    {
                                        Label = "Number of permanent exclusions",
                                        Unit = "",
                                        Colour = "#4763a5",
                                        symbol = ChartSymbol.circle
                                    }
                            },
                            Legend = Legend.top
                        }
                    }
                },

                // Secondary and primary schools applications offers key statistics tile 1
                new DataBlock
                {
                    Id = new Guid("5947759d-c6f3-451b-b353-a4da063f020a"),
                    ContentSectionId = new Guid("de8f8547-cbae-4d52-88ec-d78d0ad836ae"),
                    Name = "Key Stat 1",
                    Order = 1,
                    DataBlockRequest = new ObservationQueryContext
                    {
                        SubjectId = SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                        Country = new List<string>
                        {
                            CountryCodeEngland
                        },
                        TimePeriod = new TimePeriodQuery
                        {
                            StartYear = 2014,
                            StartCode = TimeIdentifier.CalendarYear,
                            EndYear = 2018,
                            EndCode = TimeIdentifier.CalendarYear
                        },
                        Filters = new List<Guid>
                        {
                            FItem(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                FilterItemName.Year_of_admission__Primary_All_primary)
                        },
                        Indicators = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                IndicatorName.Number_of_applications_received),
                        }
                    },
                    Summary = new Summary
                    {
                        dataKeys = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                IndicatorName.Number_of_applications_received),
                        },
                        dataDefinitionTitle = new List<string>
                        {
                            "What is number of applications received?"
                        },
                        dataSummary = new List<string>
                        {
                            "Down from 620,330 in 2017",
                        },
                        dataDefinition = new List<string>
                        {
                            @"Total number of applications received for places at primary and secondary schools.",
                        }
                    },
                    Tables = new List<Table>
                    {
                        new Table
                        {
                            tableHeaders = new TableHeaders
                            {
                                columnGroups = new List<List<TableOption>>
                                {
                                    new List<TableOption>
                                    {
                                        new TableOption("All primary", FItem(SubjectIds[SubjectName.SchoolApplicationsAndOffers], FilterItemName.Year_of_admission__Primary_All_primary).ToString())
                                    }
                                },
                                columns = new List<TableOption>
                                {
                                    new TableOption("2014", "2014_CY"),
                                    new TableOption("2015", "2015_CY"),
                                    new TableOption("2016", "2016_CY"),
                                    new TableOption("2017", "2017_CY"),
                                    new TableOption("2018", "2018_CY")
                                },
                                rowGroups = new List<List<TableRowGroupOption>>
                                {
                                    new List<TableRowGroupOption>
                                    {
                                        new TableRowGroupOption(CountryLabelEngland, GeographicLevel.Country.ToString().CamelCase(), CountryCodeEngland)
                                    }
                                },
                                rows = new List<TableOption>
                                {
                                    new TableOption("Number of applications received", Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers], IndicatorName.Number_of_applications_received).ToString())
                                }
                            }
                        }
                    }
                },

                // Secondary and primary schools applications offers key statistics tile 2
                new DataBlock
                {
                    Id = new Guid("02a637e7-6cc7-44e5-8991-8982edfe49fc"),
                    ContentSectionId = new Guid("de8f8547-cbae-4d52-88ec-d78d0ad836ae"),
                    Name = "Key Stat 2",
                    Order = 2,
                    DataBlockRequest = new ObservationQueryContext
                    {
                        SubjectId = SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                        Country = new List<string>
                        {
                            CountryCodeEngland
                        },
                        TimePeriod = new TimePeriodQuery
                        {
                            StartYear = 2014,
                            StartCode = TimeIdentifier.CalendarYear,
                            EndYear = 2018,
                            EndCode = TimeIdentifier.CalendarYear
                        },
                        Filters = new List<Guid>
                        {
                            FItem(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                FilterItemName.Year_of_admission__Primary_All_primary)
                        },
                        Indicators = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                IndicatorName.Number_of_first_preferences_offered)
                        }
                    },
                    Summary = new Summary
                    {
                        dataKeys = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                IndicatorName.Number_of_first_preferences_offered)
                        },
                        dataDefinitionTitle = new List<string>
                        {
                            "What is number of first preferences offered?"
                        },
                        dataSummary = new List<string>
                        {
                            "Down from 558,411 in 2017"
                        },
                        dataDefinition = new List<string>
                        {
                            @"Total number of first preferences offered to applicants by schools."
                        }
                    },
                    Tables = new List<Table>
                    {
                        new Table
                        {
                            tableHeaders = new TableHeaders
                            {
                                columnGroups = new List<List<TableOption>>
                                {
                                    new List<TableOption>
                                    {
                                        new TableOption("All primary", FItem(SubjectIds[SubjectName.SchoolApplicationsAndOffers], FilterItemName.Year_of_admission__Primary_All_primary).ToString())
                                    }
                                },
                                columns = new List<TableOption>
                                {
                                    new TableOption("2014", "2014_CY"),
                                    new TableOption("2015", "2015_CY"),
                                    new TableOption("2016", "2016_CY"),
                                    new TableOption("2017", "2017_CY"),
                                    new TableOption("2018", "2018_CY")
                                },
                                rowGroups = new List<List<TableRowGroupOption>>
                                {
                                    new List<TableRowGroupOption>
                                    {
                                        new TableRowGroupOption(CountryLabelEngland, GeographicLevel.Country.ToString().CamelCase(), CountryCodeEngland)
                                    }
                                },
                                rows = new List<TableOption>
                                {
                                    new TableOption("Number of first preferences offered", Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers], IndicatorName.Number_of_first_preferences_offered).ToString())
                                }
                            }
                        }
                    }
                },

                // Secondary and primary schools applications offers key statistics tile 3
                new DataBlock
                {
                    Id = new Guid("5d5f9b1f-8d0d-47d4-ba2b-ea97413d3117"),
                    ContentSectionId = new Guid("de8f8547-cbae-4d52-88ec-d78d0ad836ae"),
                    Name = "Key Stat 3",
                    Order = 3,
                    DataBlockRequest = new ObservationQueryContext
                    {
                        SubjectId = SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                        Country = new List<string>
                        {
                            CountryCodeEngland
                        },
                        TimePeriod = new TimePeriodQuery
                        {
                            StartYear = 2014,
                            StartCode = TimeIdentifier.CalendarYear,
                            EndYear = 2018,
                            EndCode = TimeIdentifier.CalendarYear
                        },
                        Filters = new List<Guid>
                        {
                            FItem(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                FilterItemName.Year_of_admission__Primary_All_primary)
                        },
                        Indicators = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                IndicatorName.Number_of_second_preferences_offered)
                        }
                    },
                    Summary = new Summary
                    {
                        dataKeys = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                IndicatorName.Number_of_second_preferences_offered)
                        },
                        dataDefinitionTitle = new List<string>
                        {
                            "What is number of second preferences offered?"
                        },
                        dataSummary = new List<string>
                        {
                            "Down from 34,792 in 2017"
                        },
                        dataDefinition = new List<string>
                        {
                            @"Total number of second preferences offered to applicants by schools."
                        }
                    },
                    Tables = new List<Table>
                    {
                        new Table
                        {
                            tableHeaders = new TableHeaders
                            {
                                columnGroups = new List<List<TableOption>>
                                {
                                    new List<TableOption>
                                    {
                                        new TableOption("All primary", FItem(SubjectIds[SubjectName.SchoolApplicationsAndOffers], FilterItemName.Year_of_admission__Primary_All_primary).ToString())
                                    }
                                },
                                columns = new List<TableOption>
                                {
                                    new TableOption("2014", "2014_CY"),
                                    new TableOption("2015", "2015_CY"),
                                    new TableOption("2016", "2016_CY"),
                                    new TableOption("2017", "2017_CY"),
                                    new TableOption("2018", "2018_CY")
                                },
                                rowGroups = new List<List<TableRowGroupOption>>
                                {
                                    new List<TableRowGroupOption>
                                    {
                                        new TableRowGroupOption(CountryLabelEngland, GeographicLevel.Country.ToString().CamelCase(), CountryCodeEngland)
                                    }
                                },
                                rows = new List<TableOption>
                                {
                                    new TableOption("Number of second preferences offered", Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers], IndicatorName.Number_of_second_preferences_offered).ToString())
                                }
                            }
                        }
                    }
                },

                // Secondary and primary schools applications offers key statistics aggregate table
                new DataBlock
                {
                    Id = new Guid("475738b4-ba10-4c29-a50d-6ca82c10de6e"),
                    ContentSectionId = new Guid("39c298e9-6c5f-47be-85cb-6e49b1b1931f"),
                    Name = "Key Stats aggregate table",
                    Order = 1,
                    DataBlockRequest = new ObservationQueryContext
                    {
                        SubjectId = SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                        Country = new List<string>
                        {
                            CountryCodeEngland
                        },
                        TimePeriod = new TimePeriodQuery
                        {
                            StartYear = 2014,
                            StartCode = TimeIdentifier.CalendarYear,
                            EndYear = 2018,
                            EndCode = TimeIdentifier.CalendarYear
                        },
                        Filters = new List<Guid>
                        {
                            FItem(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                FilterItemName.Year_of_admission__Primary_All_primary)
                        },
                        Indicators = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                IndicatorName.Number_of_admissions),
                            Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                IndicatorName.Number_of_applications_received),
                            Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                IndicatorName.Number_of_first_preferences_offered),
                            Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                IndicatorName.Number_of_second_preferences_offered),
                            Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                IndicatorName.Number_of_third_preferences_offered),
                            Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                IndicatorName.Number_that_received_one_of_their_first_three_preferences),
                            Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                IndicatorName.Number_that_received_an_offer_for_a_preferred_school),
                            Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                IndicatorName.Number_that_received_an_offer_for_a_non_preferred_school),
                            Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                IndicatorName.Number_that_did_not_receive_an_offer)
                        }
                    },
                    Summary = new Summary
                    {
                        dataKeys = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                IndicatorName.Number_of_applications_received),
                            Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                IndicatorName.Number_of_first_preferences_offered),
                            Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                IndicatorName.Number_of_second_preferences_offered)
                        },
                        dataDefinitionTitle = new List<string>
                        {
                            "What is number of applications received?",
                            "What is number of first preferences offered?",
                            "What is number of second preferences offered?"
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
                        }
                    },
                    Tables = new List<Table>
                    {
                        new Table
                        {
                            tableHeaders = new TableHeaders
                            {
                                columnGroups = new List<List<TableOption>>
                                {
                                    new List<TableOption>
                                    {
                                        new TableOption("All primary", FItem(SubjectIds[SubjectName.SchoolApplicationsAndOffers], FilterItemName.Year_of_admission__Primary_All_primary).ToString())
                                    }
                                },
                                columns = new List<TableOption>
                                {
                                    new TableOption("2014", "2014_CY"),
                                    new TableOption("2015", "2015_CY"),
                                    new TableOption("2016", "2016_CY"),
                                    new TableOption("2017", "2017_CY"),
                                    new TableOption("2018", "2018_CY")
                                },
                                rowGroups = new List<List<TableRowGroupOption>>
                                {
                                    new List<TableRowGroupOption>
                                    {
                                        new TableRowGroupOption(CountryLabelEngland, GeographicLevel.Country.ToString().CamelCase(), CountryCodeEngland)
                                    }
                                },
                                rows = new List<TableOption>
                                {
                                    new TableOption("Number of applications received", Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers], IndicatorName.Number_of_applications_received).ToString()),
                                    new TableOption("Number of admissions", Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers], IndicatorName.Number_of_admissions).ToString()),
                                    new TableOption("Number of first preferences offered", Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers], IndicatorName.Number_of_first_preferences_offered).ToString()),
                                    new TableOption("Number of second preferences offered", Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers], IndicatorName.Number_of_second_preferences_offered).ToString()),
                                    new TableOption("Number of third preferences offered", Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers], IndicatorName.Number_of_third_preferences_offered).ToString()),
                                    new TableOption("Number that received an offer for a non preferred school", Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers], IndicatorName.Number_that_received_an_offer_for_a_non_preferred_school).ToString()),
                                    new TableOption("Number that did not receive an offer", Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers], IndicatorName.Number_that_did_not_receive_an_offer).ToString())
                                }
                            }
                        }
                    }
                },
                // Secondary and primary schools applications offers generic data blocks used in content
                new DataBlock
                {
                    Id = new Guid("52916052-81e3-4b66-80b8-24f8666d9cbf"),
                    ContentSectionId = new Guid("6bfa9b19-25d6-4d45-8008-9447db541795"),
                    Heading =
                        "Table of Timeseries of key secondary preference rates, England",
                    Name = "Generic data block 1",
                    DataBlockRequest = new ObservationQueryContext
                    {
                        SubjectId = SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                        Country = new List<string>
                        {
                            CountryCodeEngland
                        },
                        TimePeriod = new TimePeriodQuery
                        {
                            StartYear = 2014,
                            StartCode = TimeIdentifier.CalendarYear,
                            EndYear = 2018,
                            EndCode = TimeIdentifier.CalendarYear
                        },

                        Filters = new List<Guid>
                        {
                            FItem(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                FilterItemName.Year_of_admission__Secondary_All_secondary)
                        },
                        Indicators = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                IndicatorName.Number_that_received_an_offer_for_a_preferred_school),
                            Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                IndicatorName.Number_that_received_an_offer_for_a_non_preferred_school),
                            Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                IndicatorName.Number_that_did_not_receive_an_offer),
                            Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                IndicatorName.Number_that_received_an_offer_for_a_school_within_their_LA)
                        }
                    },
                    Tables = new List<Table>
                    {
                        new Table
                        {
                            tableHeaders = new TableHeaders
                            {
                                columnGroups = new List<List<TableOption>>
                                {
                                    new List<TableOption>
                                    {
                                        new TableOption("All secondary", FItem(SubjectIds[SubjectName.SchoolApplicationsAndOffers], FilterItemName.Year_of_admission__Secondary_All_secondary).ToString())
                                    }
                                },
                                columns = new List<TableOption>
                                {
                                    new TableOption("2014", "2014_CY"),
                                    new TableOption("2015", "2015_CY"),
                                    new TableOption("2016", "2016_CY"),
                                    new TableOption("2017", "2017_CY"),
                                    new TableOption("2018", "2018_CY")
                                },
                                rowGroups = new List<List<TableRowGroupOption>>
                                {
                                    new List<TableRowGroupOption>
                                    {
                                        new TableRowGroupOption(CountryLabelEngland, GeographicLevel.Country.ToString().CamelCase(), CountryCodeEngland)
                                    }
                                },
                                rows = new List<TableOption>
                                {
                                    new TableOption("Number that received an offer for a preferred school", Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers], IndicatorName.Number_that_received_an_offer_for_a_preferred_school).ToString()),
                                    new TableOption("Number that received an offer for a non preferred school", Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers], IndicatorName.Number_that_received_an_offer_for_a_non_preferred_school).ToString()),
                                    new TableOption("Number that did not receive an offer", Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers], IndicatorName.Number_that_did_not_receive_an_offer).ToString())
                                }
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
                    Name = "Generic data block 2",
                    DataBlockRequest = new ObservationQueryContext
                    {
                        SubjectId = SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                        Country = new List<string>
                        {
                            CountryCodeEngland
                        },
                        TimePeriod = new TimePeriodQuery
                        {
                            StartYear = 2014,
                            StartCode = TimeIdentifier.CalendarYear,
                            EndYear = 2018,
                            EndCode = TimeIdentifier.CalendarYear
                        },

                        Filters = new List<Guid>
                        {
                            FItem(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                FilterItemName.Year_of_admission__Primary_All_primary)
                        },
                        Indicators = new List<Guid>
                        {
                            Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                IndicatorName.Number_that_received_an_offer_for_a_preferred_school),
                            Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                IndicatorName.Number_that_received_an_offer_for_a_non_preferred_school),
                            Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                IndicatorName.Number_that_did_not_receive_an_offer),
                            Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers],
                                IndicatorName.Number_that_received_an_offer_for_a_school_within_their_LA)
                        }
                    },
                    Tables = new List<Table>
                    {
                        new Table
                        {
                            tableHeaders = new TableHeaders
                            {
                                columnGroups = new List<List<TableOption>>
                                {
                                    new List<TableOption>
                                    {
                                        new TableOption("All primary", FItem(SubjectIds[SubjectName.SchoolApplicationsAndOffers], FilterItemName.Year_of_admission__Primary_All_primary).ToString())
                                    }
                                },
                                columns = new List<TableOption>
                                {
                                    new TableOption("2014", "2014_CY"),
                                    new TableOption("2015", "2015_CY"),
                                    new TableOption("2016", "2016_CY"),
                                    new TableOption("2017", "2017_CY"),
                                    new TableOption("2018", "2018_CY")
                                },
                                rowGroups = new List<List<TableRowGroupOption>>
                                {
                                    new List<TableRowGroupOption>
                                    {
                                        new TableRowGroupOption(CountryLabelEngland, GeographicLevel.Country.ToString().CamelCase(), CountryCodeEngland)
                                    }
                                },
                                rows = new List<TableOption>
                                {
                                    new TableOption("Number that received an offer for a preferred school", Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers], IndicatorName.Number_that_received_an_offer_for_a_preferred_school).ToString()),
                                    new TableOption("Number that received an offer for a non preferred school", Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers], IndicatorName.Number_that_received_an_offer_for_a_non_preferred_school).ToString()),
                                    new TableOption("Number that did not receive an offer", Indicator(SubjectIds[SubjectName.SchoolApplicationsAndOffers], IndicatorName.Number_that_did_not_receive_an_offer).ToString())
                                }
                            }
                        }
                    }
                }
            );

            modelBuilder.Entity<Update>().HasData(
                new Update
                {
                    Id = new Guid("9c0f0139-7f88-4750-afe0-1c85cdf1d047"),
                    ReleaseId = absenceReleaseId,
                    On = new DateTime(2018, 4, 19),
                    Reason =
                        "Underlying data file updated to include absence data by pupil residency and school location, and updated metadata document."
                },
                new Update
                {
                    Id = new Guid("18e0d40e-bdf7-4c84-99dd-732e72e9c9a5"),
                    ReleaseId = absenceReleaseId,
                    On = new DateTime(2018, 3, 22),
                    Reason = "First published."
                },
                new Update
                {
                    Id = new Guid("4fca874d-98b8-4c79-ad20-d698fb0af7dc"),
                    ReleaseId = exclusionsReleaseId,
                    On = new DateTime(2018, 7, 19),
                    Reason = "First published."
                },
                new Update
                {
                    Id = new Guid("33ff3f17-0671-41e9-b404-5661ab8a9476"),
                    ReleaseId = exclusionsReleaseId,
                    On = new DateTime(2018, 8, 25),
                    Reason =
                        "Updated exclusion rates for Gypsy/Roma pupils, to include extended ethnicity categories within the headcount (Gypsy, Roma and other Gypsy/Roma)."
                },
                new Update
                {
                    Id = new Guid("448ca9ea-0cd2-4e6d-b85b-76c3ef7d3bf9"),
                    ReleaseId = applicationOffersReleaseId,
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
                                    Body = File.Exists(
                                        @"Migrations/ContentMigrations/Html/Pupil_Absence_Statistics/Section1.html")
                                        ? File.ReadAllText(
                                            @"Migrations/ContentMigrations/Html/Pupil_Absence_Statistics/Section1.html",
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
                                    Body = File.Exists(
                                        @"Migrations/ContentMigrations/Html/Pupil_Absence_Statistics/Section2.html")
                                        ? File.ReadAllText(
                                            @"Migrations/ContentMigrations/Html/Pupil_Absence_Statistics/Section2.html",
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
                                    Body = File.Exists(
                                        @"Migrations/ContentMigrations/Html/Pupil_Absence_Statistics/Section3.html")
                                        ? File.ReadAllText(
                                            @"Migrations/ContentMigrations/Html/Pupil_Absence_Statistics/Section3.html",
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
                                    Body = File.Exists(
                                        @"Migrations/ContentMigrations/Html/Pupil_Absence_Statistics/Section4.html")
                                        ? File.ReadAllText(
                                            @"Migrations/ContentMigrations/Html/Pupil_Absence_Statistics/Section4.html",
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
                                    Body = File.Exists(
                                        @"Migrations/ContentMigrations/Html/Pupil_Absence_Statistics/Section5.html")
                                        ? File.ReadAllText(
                                            @"Migrations/ContentMigrations/Html/Pupil_Absence_Statistics/Section5.html",
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
                                    Body = File.Exists(
                                        @"Migrations/ContentMigrations/Html/Pupil_Absence_Statistics/Section6.html")
                                        ? File.ReadAllText(
                                            @"Migrations/ContentMigrations/Html/Pupil_Absence_Statistics/Section6.html",
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
                                    Body = File.Exists(
                                        @"Migrations/ContentMigrations/Html/Pupil_Absence_Statistics/Section7.html")
                                        ? File.ReadAllText(
                                            @"Migrations/ContentMigrations/Html/Pupil_Absence_Statistics/Section7.html",
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
                                    Body = File.Exists(
                                        @"Migrations/ContentMigrations/Html/Pupil_Absence_Statistics/AnnexA.html")
                                        ? File.ReadAllText(
                                            @"Migrations/ContentMigrations/Html/Pupil_Absence_Statistics/AnnexA.html",
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
                                    Body = File.Exists(
                                        @"Migrations/ContentMigrations/Html/Pupil_Absence_Statistics/AnnexB.html")
                                        ? File.ReadAllText(
                                            @"Migrations/ContentMigrations/Html/Pupil_Absence_Statistics/AnnexB.html",
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
                                    Body = File.Exists(
                                        @"Migrations/ContentMigrations/Html/Pupil_Absence_Statistics/AnnexC.html")
                                        ? File.ReadAllText(
                                            @"Migrations/ContentMigrations/Html/Pupil_Absence_Statistics/AnnexC.html",
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
                                    Body = File.Exists(
                                        @"Migrations/ContentMigrations/Html/Pupil_Absence_Statistics/AnnexD.html")
                                        ? File.ReadAllText(
                                            @"Migrations/ContentMigrations/Html/Pupil_Absence_Statistics/AnnexD.html",
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
                                    Body = File.Exists(@"Migrations/ContentMigrations/Html/Pupil_Absence_Statistics/AnnexE.html")
                                        ? File.ReadAllText(@"Migrations/ContentMigrations/Html/Pupil_Absence_Statistics/AnnexE.html",
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
                                    Body = File.Exists(@"Migrations/ContentMigrations/Html/Pupil_Absence_Statistics/AnnexF.html")
                                        ? File.ReadAllText(@"Migrations/ContentMigrations/Html/Pupil_Absence_Statistics/AnnexF.html",
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
                                            @"Migrations/ContentMigrations/Html/Secondary_And_Primary_School_Applications_And_Offers/Section1.html")
                                            ? File.ReadAllText(
                                                @"Migrations/ContentMigrations/Html/Secondary_And_Primary_School_Applications_And_Offers/Section1.html",
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
                                            @"Migrations/ContentMigrations/Html/Secondary_And_Primary_School_Applications_And_Offers/Section2.html")
                                            ? File.ReadAllText(
                                                @"Migrations/ContentMigrations/Html/Secondary_And_Primary_School_Applications_And_Offers/Section2.html",
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
                                            @"Migrations/ContentMigrations/Html/Secondary_And_Primary_School_Applications_And_Offers/Section3.html")
                                            ? File.ReadAllText(
                                                @"Migrations/ContentMigrations/Html/Secondary_And_Primary_School_Applications_And_Offers/Section3.html",
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
                                            @"Migrations/ContentMigrations/Html/Secondary_And_Primary_School_Applications_And_Offers/Section4.html")
                                            ? File.ReadAllText(
                                                @"Migrations/ContentMigrations/Html/Secondary_And_Primary_School_Applications_And_Offers/Section4.html",
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
                                    Body = File.Exists(@"Migrations/ContentMigrations/Html/Pupil_Exclusion_Statistics/Section1.html")
                                        ? File.ReadAllText(@"Migrations/ContentMigrations/Html/Pupil_Exclusion_Statistics/Section1.html",
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
                                    Body = File.Exists(@"Migrations/ContentMigrations/Html/Pupil_Exclusion_Statistics/Section2.html")
                                        ? File.ReadAllText(@"Migrations/ContentMigrations/Html/Pupil_Exclusion_Statistics/Section2.html",
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
                                    Body = File.Exists(@"Migrations/ContentMigrations/Html/Pupil_Exclusion_Statistics/Section3.html")
                                        ? File.ReadAllText(@"Migrations/ContentMigrations/Html/Pupil_Exclusion_Statistics/Section3.html",
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
                                    Body = File.Exists(@"Migrations/ContentMigrations/Html/Pupil_Exclusion_Statistics/Section4.html")
                                        ? File.ReadAllText(@"Migrations/ContentMigrations/Html/Pupil_Exclusion_Statistics/Section4.html",
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
                                    Body = File.Exists(@"Migrations/ContentMigrations/Html/Pupil_Exclusion_Statistics/Section5.html")
                                        ? File.ReadAllText(@"Migrations/ContentMigrations/Html/Pupil_Exclusion_Statistics/Section5.html",
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
                                    Body = File.Exists(@"Migrations/ContentMigrations/Html/Pupil_Exclusion_Statistics/Section6.html")
                                        ? File.ReadAllText(@"Migrations/ContentMigrations/Html/Pupil_Exclusion_Statistics/Section6.html",
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
                                    Body = File.Exists(@"Migrations/ContentMigrations/Html/Pupil_Exclusion_Statistics/Section7.html")
                                        ? File.ReadAllText(@"Migrations/ContentMigrations/Html/Pupil_Exclusion_Statistics/Section7.html",
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
                                    Body = File.Exists(@"Migrations/ContentMigrations/Html/Pupil_Exclusion_Statistics/AnnexA.html")
                                        ? File.ReadAllText(@"Migrations/ContentMigrations/Html/Pupil_Exclusion_Statistics/AnnexA.html",
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
                                    Body = File.Exists(@"Migrations/ContentMigrations/Html/Pupil_Exclusion_Statistics/AnnexB.html")
                                        ? File.ReadAllText(@"Migrations/ContentMigrations/Html/Pupil_Exclusion_Statistics/AnnexB.html",
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
                                    Body = File.Exists(@"Migrations/ContentMigrations/Html/Pupil_Exclusion_Statistics/AnnexC.html")
                                        ? File.ReadAllText(@"Migrations/ContentMigrations/Html/Pupil_Exclusion_Statistics/AnnexC.html",
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
                                    Body = File.Exists(@"Migrations/ContentMigrations/Html/Pupil_Exclusion_Statistics/AnnexD.html")
                                        ? File.ReadAllText(@"Migrations/ContentMigrations/Html/Pupil_Exclusion_Statistics/AnnexD.html",
                                            Encoding.UTF8)
                                        : ""
                                }
                            }
                        }
                    }
                }
            );
            
            var analystMvcUser1Id = new Guid("e7f7c82e-aaf3-43db-a5ab-755678f67d04");
            var analystMvcUser2Id = new Guid("6620bccf-2433-495e-995d-fc76c59d9c62");
            var analystMvcUser3Id = new Guid("b390b405-ef90-4b9d-8770-22948e53189a");
            var bauMvcUser1Id = new Guid("b99e8358-9a5e-4a3a-9288-6f94c7e1e3dd");
            var bauMvcUser2Id = new Guid("b6f0dfa5-0102-4b91-9aa8-f23b7d8aca63");
            var prereleaseMvcUser1Id = new Guid("d5c85378-df85-482c-a1ce-09654dae567d");
            var prereleaseMvcUser2Id = new Guid("ee9a02c1-b3f9-402c-9e9b-4fb78d737050");

            modelBuilder.Entity<User>()
                .HasData(
                    new User
                    {
                        Id = analystMvcUser1Id,
                        FirstName = "Analyst1",
                        LastName = "User1",
                        Email = "analyst1@example.com"
                    },
                    new User
                    {
                        Id = analystMvcUser2Id,
                        FirstName = "Analyst2",
                        LastName = "User2",
                        Email = "analyst2@example.com"
                    },
                    new User
                    {
                        Id = analystMvcUser3Id,
                        FirstName = "Analyst3",
                        LastName = "User3",
                        Email = "analyst3@example.com"
                    },
                    new User
                    {
                        Id = bauMvcUser1Id,
                        FirstName = "Bau1",
                        LastName = "User1",
                        Email = "bau1@example.com"
                    },
                    new User
                    {
                        Id = bauMvcUser2Id,
                        FirstName = "Bau2",
                        LastName = "User2",
                        Email = "bau2@example.com"
                    },
                    new User
                    {
                        Id = prereleaseMvcUser1Id,
                        FirstName = "Prerelease1",
                        LastName = "User1",
                        Email = "prerelease1@example.com"
                    },
                    new User
                    {
                        Id = prereleaseMvcUser2Id,
                        FirstName = "Prerelease2",
                        LastName = "User2",
                        Email = "prerelease2@example.com"
                    }
                );
            
            modelBuilder.Entity<UserReleaseRole>()
                .HasData(
                    new UserReleaseRole
                    {
                        Id = new Guid("1501265c-979b-4cd4-8a55-00bfe909a2da"),
                        ReleaseId = absenceReleaseId,
                        UserId = analystMvcUser1Id,
                        Role = ReleaseRole.Contributor
                    },
                    new UserReleaseRole
                    {
                        Id = new Guid("086b1354-473c-48bb-9d30-0ac1963dc4cb"),
                        ReleaseId = absenceReleaseId,
                        UserId = analystMvcUser2Id,
                        Role = ReleaseRole.Lead
                    },
                    new UserReleaseRole
                    {
                        Id = new Guid("1851e50d-04ac-4e16-911b-3df3350c589b"),
                        ReleaseId = absenceReleaseId,
                        UserId = analystMvcUser2Id,
                        Role = ReleaseRole.Approver
                    },
                    new UserReleaseRole
                    {
                        Id = new Guid("239d8eed-8a7d-4f7a-ac0a-c20bc4e9167d"),
                        ReleaseId = exclusionsReleaseId,
                        UserId = analystMvcUser1Id,
                        Role = ReleaseRole.Contributor
                    },
                    new UserReleaseRole
                    {
                        Id = new Guid("e0dddf7a-f616-4e6f-bb9c-0b6e8ea3d9b9"),
                        ReleaseId = exclusionsReleaseId,
                        UserId = analystMvcUser2Id,
                        Role = ReleaseRole.Approver
                    },
                    new UserReleaseRole
                    {
                        Id = new Guid("f7884899-baf9-4009-8561-f0c5df0d0a69"),
                        ReleaseId = exclusionsReleaseId,
                        UserId = analystMvcUser3Id,
                        Role = ReleaseRole.Lead
                    },
                    new UserReleaseRole
                    {
                        Id = new Guid("77ff439d-e1cd-4e50-9c25-24a5207953a5"),
                        ReleaseId = applicationOffersReleaseId,
                        UserId = analystMvcUser2Id,
                        Role = ReleaseRole.Contributor
                    },
                    new UserReleaseRole
                    {
                        Id = new Guid("b00fd7c0-226f-474d-8cec-820a1a789182"),
                        ReleaseId = applicationOffersReleaseId,
                        UserId = analystMvcUser3Id,
                        Role = ReleaseRole.Lead
                    },
                    new UserReleaseRole
                    {
                        Id = new Guid("d1cbc96e-75c0-424f-bd63-c1920b763020"),
                        ReleaseId = applicationOffersReleaseId,
                        UserId = analystMvcUser3Id,
                        Role = ReleaseRole.Approver
                    },
                    new UserReleaseRole
                    {
                        Id = new Guid("69860a07-91d0-49d6-973d-98830fbbedfb"),
                        ReleaseId = absenceReleaseId,
                        UserId = prereleaseMvcUser1Id,
                        Role = ReleaseRole.PrereleaseViewer
                    },
                    new UserReleaseRole
                    {
                        Id = new Guid("00cf98d9-c16c-4004-9fde-fe212b059845"),
                        ReleaseId = exclusionsReleaseId,
                        UserId = prereleaseMvcUser1Id,
                        Role = ReleaseRole.PrereleaseViewer
                    },
                    new UserReleaseRole
                    {
                        Id = new Guid("ef19a735-81b4-482c-b1e2-31616ca26f51"),
                        ReleaseId = exclusionsReleaseId,
                        UserId = prereleaseMvcUser2Id,
                        Role = ReleaseRole.PrereleaseViewer
                    }
                );
        }

        private static Guid FItem(Guid subjectId, FilterItemName filterItemName)
        {
            return SubjectFilterItemIds[subjectId][filterItemName];
        }

        private static Guid Indicator(Guid subjectId, IndicatorName indicatorName)
        {
            return SubjectIndicatorIds[subjectId][indicatorName];
        }
    }
}