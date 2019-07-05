using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Database
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

        public DbSet<Methodology> Methodologies { get; set; }
        public DbSet<Theme> Themes { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Publication> Publications { get; set; }
        public DbSet<Release> Releases { get; set; }
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
                    Title = "School and pupils numbers",
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
                    Id = new Guid("11bb7387-e85e-4571-9669-8a760dcb004f"),
                    TeamName = "Simon's Team",
                    TeamEmail = "teamshakes@gmail.com",
                    ContactName = "Simon Shakespeare",
                    ContactTelNo = "0114 262 1619"
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
                    Summary = "",
                    TopicId = new Guid("77941b7d-bbd6-4069-9107-565af89e2dec"),
                    Slug = "permanent-and-fixed-period-exclusions-in-england",
                    NextUpdate = new DateTime(2019, 7, 19)
                },
                new Publication
                {
                    Id = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                    Title = "Pupil absence in schools in England",
                    Summary = "",
                    TopicId = new Guid("67c249de-1cca-446e-8ccb-dcdac542f460"),
                    Slug = "pupil-absence-in-schools-in-england",
                    NextUpdate = new DateTime(2019, 3, 22),
                    DataSource =
                        "[Pupil absence statistics: guide](https://www.gov.uk/government/publications/absence-statistics-guide#)",
                    ContactId = new Guid("11bb7387-e85e-4571-9669-8a760dcb004f")
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
                    Title = "School and pupils and their characteristics",
                    Summary = "",
                    TopicId = new Guid("e50ba9fd-9f19-458c-aceb-4422f0c7d1ba"),
                    Slug = "school-pupils-and-their-characteristics",
                    LegacyPublicationUrl =
                        new Uri("https://www.gov.uk/government/collections/statistics-school-and-pupil-numbers")
                },
                new Publication
                {
                    Id = new Guid("66c8e9db-8bf2-4b0b-b094-cfab25c20b05"),
                    Title = "Secondary and primary schools applications and offers",
                    Summary = "",
                    TopicId = new Guid("1a9636e4-29d5-4c90-8c07-f41db8dd019c"),
                    Slug = "secondary-and-primary-schools-applications-and-offers",
                    NextUpdate = new DateTime(2019, 6, 14),
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

            modelBuilder.Entity<Release>().HasData(
                //absence
                new Release
                {
                    Id = new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                    Title = "Pupil absence data and statistics for schools in England",
                    ReleaseName = "2016 to 2017",
                    PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                    Published = new DateTime(2018, 3, 22),
                    Slug = "2016-17",
                    Summary =
                        "Read national statistical summaries, view charts and tables and download data files.\n\n" +
                        "Find out how and why these statistics are collected and published - [Pupil absence statistics: methodology](../methodology/pupil-absence-in-schools-in-england).",

                    KeyStatistics = new DataBlock
                    {
                        DataBlockRequest = new DataBlockRequest
                        {
                            subjectId = 1,
                            geographicLevel = "Country",
                            startYear = "2012",
                            endYear = "2016",
                            filters = new List<string> {"1", "2"},
                            indicators = new List<string> {"23", "26", "28"}
                        },

                        Summary = new Summary
                        {
                            dataKeys = new List<string>
                            {
                                "26",
                                "28",
                                "23"
                            },
                            dataSummary = new List<string>
                            {
                                "Up from 4.6% in 2015/16",
                                "Similar to previous years",
                                "Up from 1.1% in 2015/16"
                            },
                            description = new MarkDownBlock
                            {
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
                                indicators = new List<string> {"23", "26", "28"}
                            }
                        },
                        Charts = new List<IContentBlockChart>
                        {
                            new LineChart
                            {
                                Axes = new Dictionary<string, ChartAxisConfiguration>
                                {
                                    ["major"] = new ChartAxisConfiguration
                                    {
                                        GroupBy = new List<string> {"timePeriod"},
                                        DataSets = new List<ChartDataSet>
                                        {
                                            new ChartDataSet {Indicator = "23", filters = new List<string> {"1", "2"}},
                                            new ChartDataSet {Indicator = "26", filters = new List<string> {"1", "2"}},
                                            new ChartDataSet {Indicator = "28", filters = new List<string> {"1", "2"}},
                                        },
                                        Title = "School Year"
                                    },
                                    ["minor"] = new ChartAxisConfiguration
                                    {
                                        Title = "Absence Rate",
                                        GroupBy = new List<string>(),
                                    }
                                },
                                Labels = new Dictionary<string, ChartLabelConfiguration>
                                {
                                    ["2012_HT6"] = new ChartLabelConfiguration
                                    {
                                        Label = "2012/2013"
                                    },
                                    ["2013_HT6"] = new ChartLabelConfiguration
                                    {
                                        Label = "2013/2014"
                                    },
                                    ["2014_HT6"] = new ChartLabelConfiguration
                                    {
                                        Label = "2014/2015"
                                    },
                                    ["2015_HT6"] = new ChartLabelConfiguration
                                    {
                                        Label = "2015/2016"
                                    },
                                    ["2016_HT6"] = new ChartLabelConfiguration
                                    {
                                        Label = "2016/2017"
                                    },
                                    ["23_1_2_____"] = new ChartLabelConfiguration
                                    {
                                        Label = "Unauthorised Absence Rate",
                                        Unit = "%"
                                    },
                                    ["26_1_2_____"] = new ChartLabelConfiguration
                                    {
                                        Label = "Overall Absence Rate",
                                        Unit = "%"
                                    },
                                    ["28_1_2_____"] = new ChartLabelConfiguration
                                    {
                                        Label = "Authorised Absence Rate",
                                        Unit = "%"
                                    },
                                }
                            },
                        },
                    },

                    Content = new List<ContentSection>
                    {
                        new ContentSection
                        {
                            Order = 1, Heading = "About these statistics", Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body =
                                        "The statistics and data cover the absence of pupils of compulsory school age during the 2016/17 academic year in the following state-funded school types:\n\n" +
                                        "- primary schools\n" +
                                        "- secondary schools\n" +
                                        "- special schools\n\n" +
                                        "They also includes information fo [pupil referral units](../glossary#pupil-referral-unit) and pupils aged 4 years.\n\n" +
                                        "We use the key measures of [overall absence](../glossary#overall-absence) and [persistent absence](../glossary#persistent-absence) to monitor pupil absence and also include [absence by reason](#contents-sections-heading-4) and [pupil characteristics](#contents-sections-heading-6).\n\n" +
                                        "The statistics and data are available at national, regional, local authority (LA) and school level and are used by LAs and schools to compare their local absence rates to regional and national averages for different pupil groups.\n\n" +
                                        "They're also used for policy development as key indicators in behaviour and school attendance policy.\n"
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 2, Heading = "Pupil absence rates", Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body =
                                        "**Overall absence**\n\n" +
                                        "The [overall absence](../glossary#overall-absence) rate has increased across state-funded primary, secondary and special schools between 2015/16 and 2016/17 driven by an increase in the unauthorised absence rate.\n\n" +
                                        "It increased from 4.6% to 4.7% over this period while the [unauthorised absence](../glossary#unauthorised-absence) rate increased from 1.1% to 1.3%.\n\n" +
                                        "The rate stayed the same at 4% in primary schools but increased from 5.2% to 5.4% for secondary schools. However, in special schools it was much higher and rose to 9.7%.\n\n" +
                                        "The overall and [authorised absence](../glossary#authorised-absence) rates have been fairly stable over recent years after gradually decreasing between 2006/07 and 2013/14."
                                },
                                new DataBlock
                                {
                                    Heading = null,
                                    DataBlockRequest = new DataBlockRequest
                                    {
                                        subjectId = 1,
                                        geographicLevel = "Country",
                                        startYear = "2012",
                                        endYear = "2016",
                                        filters = new List<string> {"1", "2"},
                                        indicators = new List<string> {"23", "26", "28"}
                                    },
                                    Tables = new List<Table>
                                    {
                                        new Table
                                        {
                                            indicators = new List<string> {"23", "26", "28"}
                                        }
                                    },
                                    Charts = new List<IContentBlockChart>
                                    {
                                        new LineChart
                                        {
                                            Axes = new Dictionary<string, ChartAxisConfiguration>
                                            {
                                                ["major"] = new ChartAxisConfiguration
                                                {
                                                    GroupBy = new List<string> {"timePeriod"},
                                                    DataSets = new List<ChartDataSet>
                                                    {
                                                        new ChartDataSet
                                                            {Indicator = "23", filters = new List<string> {"1", "2"}},
                                                        new ChartDataSet
                                                            {Indicator = "26", filters = new List<string> {"1", "2"}},
                                                        new ChartDataSet
                                                            {Indicator = "28", filters = new List<string> {"1", "2"}},
                                                    },
                                                    Title = "School Year"
                                                },
                                                ["minor"] = new ChartAxisConfiguration
                                                {
                                                    Title = "Absence Rate",
                                                    GroupBy = new List<string>(),
                                                }
                                            },
                                            Labels = new Dictionary<string, ChartLabelConfiguration>
                                            {
                                                ["2012_HT6"] = new ChartLabelConfiguration
                                                {
                                                    Label = "2012/2013"
                                                },
                                                ["2013_HT6"] = new ChartLabelConfiguration
                                                {
                                                    Label = "2013/2014"
                                                },
                                                ["2014_HT6"] = new ChartLabelConfiguration
                                                {
                                                    Label = "2014/2015"
                                                },
                                                ["2015_HT6"] = new ChartLabelConfiguration
                                                {
                                                    Label = "2015/2016"
                                                },
                                                ["2016_HT6"] = new ChartLabelConfiguration
                                                {
                                                    Label = "2016/2017"
                                                },
                                                ["23_1_2_____"] = new ChartLabelConfiguration
                                                {
                                                    Label = "Unauthorised Absence Rate",
                                                    Unit = "%"
                                                },
                                                ["26_1_2_____"] = new ChartLabelConfiguration
                                                {
                                                    Label = "Overall Absence Rate",
                                                    Unit = "%"
                                                },
                                                ["28_1_2_____"] = new ChartLabelConfiguration
                                                {
                                                    Label = "Authorised Absence Rate",
                                                    Unit = "%"
                                                },
                                            }
                                        }
                                    }
                                },
                                new MarkDownBlock
                                {
                                    Body =
                                        "**Unauthorised absence**\n\n" +
                                        "The [unauthorised absence](../glossary#unauthorised-absence) rate has not varied much since 2006/07 but is at its highest since records began - 1.3%.\n\n" +
                                        "This is due to an increase in absence due to family holidays not agreed by schools.\n\n" +
                                        "**Authorised absence**\n\n" +
                                        "The [authorised absence](../glossary#authorised-absence) rate has stayed at 3.4% since 2015/16 but has been decreasing in recent years within primary schools.\n\n" +
                                        "**Total number of days missed**\n\n" +
                                        "The total number of days missed for [overall absence](../glossary#overall-absence) across state-funded primary, secondary and special schools has increased to 56.7 million from 54.8 million in 2015/16.\n\n" +
                                        "This partly reflects a rise in the total number of pupils with the average number of days missed per pupil slightly increased to 8.2 days from 8.1 days in 2015/16.\n\n" +
                                        "In 2016/17, 91.8% of primary, secondary and special school pupils missed at least 1 session during the school year - similar to the 91.7% figure from 2015/16."
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
                                        "The [persistent absence](../glossary#persistent-absence) rate increased to and accounted for 37.6% of all absence - up from 36.6% in 2015 to 16 but still down from 43.3% in 2011 to 12.\n\n" +
                                        "It also accounted for almost a third (31.6%) of all [authorised absence](../glossary#authorised-absence) and more than half (53.8%) of all [unauthorised absence](../glossary#unauthorised-absence).\n\n" +
                                        "Overall, it's increased across primary and secondary schools to 10.8% - up from 10.5% in 2015 to 16."
                                },
/*                                new DataBlock
                                {
                                    Heading = null,
                                    DataBlockRequest = new DataBlockRequest
                                    {
                                        subjectId = 1,
                                        geographicLevel = "Country",
                                        startYear = "2012",
                                        endYear = "2016",
                                        filters = new List<string> {"1", "2"},
                                        indicators = new List<string> {"23", "26", "28"}
                                    },
                                    Tables = new List<Table>
                                    {
                                        new Table
                                        {
                                            indicators = new List<string> {"23", "26", "28"}
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
                                                "23", "26", "28"
                                            },
                                        }
                                    }
                                },*/
                                new MarkDownBlock
                                {
                                    Body =
                                        "**Persistent absentees**\n\n" +
                                        "The [overall absence](../glossary#overall-absence) rate for persistent absentees across all schools increased to 18.1% - nearly 4 times higher than the rate for all pupils. This is slightly up from 17.6% in 2015/16.\n\n" +
                                        "**Illness absence rate**\n\n" +
                                        "The illness absence rate is almost 4 times higher for persistent absentees at 7.6% compared to 2% for other pupils."
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 4, Heading = "Reasons for absence", Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock()
                                {
                                    Body =
                                        "These have been broken down into the following:\n\n" +
                                        "* distribution of absence by reason - the proportion of absence for each reason, calculated by taking the number of absences for a specific reason as a percentage of the total number of absences\n\n" +
                                        "* rate of absence by reason - the rate of absence for each reason, calculated by taking the number of absences for a specific reason as a percentage of the total number of possible sessions\n\n" +
                                        "* one or more sessions missed due to each reason - the number of pupils missing at least 1 session due to each reason"
                                },
/*                                new DataBlock
                                {
                                    Heading = null,
                                    DataBlockRequest = new DataBlockRequest
                                    {
                                        subjectId = 1,
                                        geographicLevel = "Country",
                                        startYear = "2012",
                                        endYear = "2016",
                                        filters = new List<string> {"1", "2"},
                                        indicators = new List<string> {"23", "26", "28"}
                                    },
                                    Tables = new List<Table>
                                    {
                                        new Table
                                        {
                                            indicators = new List<string> {"23", "26", "28"}
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
                                                "23", "26", "28"
                                            },
                                        }
                                    }
                                },*/
                                new MarkDownBlock
                                {
                                    Body =
                                        "**Illness**\n\n" +
                                        "This is the main driver behind [overall absence](../glossary#overall-absence) and accounted for 55.3% of all absence - down from 57.3% in 2015/16 and 60.1% in 2014/15.\n\n" +
                                        "While the overall absence rate has slightly increased since 2015/16 the illness rate has stayed the same at 2.6%.\n\n" +
                                        "The absence rate due to other unauthorised circumstances has also stayed the same since 2015/16 at 0.7%.\n\n" +
                                        "**Absence due to family holiday**\n\n" +
                                        "The unauthorised holiday absence rate has increased gradually since 2006/07 while authorised holiday absence rates are much lower than in 2006/07 and remained steady over recent years.\n\n" +
                                        "The percentage of pupils who missed at least 1 session due to family holiday increased to 16.9% - up from 14.7% in 2015/16.\n\n" +
                                        "The absence rate due to family holidays agreed by the school stayed at 0.1%.\n\n" +
                                        "Meanwhile, the percentage of all possible sessions missed due to unauthorised family holidays increased to 0.4% - up from 0.3% in 2015/16.\n\n" +
                                        "**Regulation amendment**\n\n" +
                                        "A regulation amendment in September 2013 stated that term-time leave could only be granted in exceptional circumstances which explains the sharp fall in authorised holiday absence between 2012/13 and 2013/14.\n\n" +
                                        "These statistics and data relate to the period after the [Isle of Wight Council v Jon Platt High Court judgment (May 2016)](https://commonslibrary.parliament.uk/insights/term-time-holidays-supreme-court-judgment/) where the High Court supported a local magistrates’ ruling that there was no case to answer.\n\n" +
                                        "They also partially relate to the period after the April 2017 Supreme Court judgment where it unanimously agreed that no children should be taken out of school without good reason and clarified that 'regularly' means 'in accordance with the rules prescribed by the school'."
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
                                        "Nearly half of all pupils (48.9%) were absent for 5 days or less across primary, secondary and special schools - down from 49.1% in 2015/16.\n\n" +
                                        "The average total absence for primary school pupils was 7.2 days compared to 16.9 days for special school and 9.3 day for secondary school pupils.\n\n" +
                                        "The rate of pupils who had more than 25 days of absence stayed the same as in 2015/16 at 4.3%.\n\n" +
                                        "These pupils accounted for 23.5% of days missed while 8.2% of pupils had no absence.\n\n" +
                                        "**Absence by term**\n\n" +
                                        "Across all schools:\n\n" +
                                        "* [overall absence](../glossary#overall-absence) - highest in summer and lowest in autumn\n\n" +
                                        "* [authorised absence](../glossary#authorised-absence) - highest in spring and lowest in summer\n\n" +
                                        "* [unauthorised absence](../glossary#unauthorised-absence) - highest in summer"
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
                                        "The [overall absence](../glossary#overall-absence) and [persistent absence](../glossary#persistent-absence) patterns for pupils with different characteristics have been consistent over recent years.\n\n" +
                                        "**Ethnic groups**\n\n" +
                                        "Overall absence rate:\n\n" +
                                        "* Travellers of Irish heritage and Gypsy / Roma pupils - highest at 18.1% and 12.9% respectively\n\n" +
                                        "* Chinese and Black African ethnicity pupils - substantially lower than the national average of 4.7% at 2.4% and 2.9% respectively\n\n" +
                                        "Persistent absence rate:\n\n" +
                                        "* Travellers of Irish heritage pupils - highest at 64%\n\n" +
                                        "* Chinese pupils - lowest at 3.1%\n\n" +
                                        "**Free school meals (FSM) eligibility**\n\n" +
                                        "Overall absence rate:\n\n" +
                                        "* pupils known to be eligible for and claiming FSM - higher at 7.3% compared to 4.2% for non-FSM pupils\n\n" +
                                        "Persistent absence rate:\n\n" +
                                        "* pupils known to be eligible for and claiming FSM - more than double the rate of non-FSM pupils\n\n" +
                                        "**Gender**\n\n" +
                                        "Overall absence rate:\n\n" +
                                        "* boys and girls - very similar at 4.7% and 4.6% respectively\n\n" +
                                        "Persistent absence rate:\n\n" +
                                        "* boys and girls - similar at 10.9% and 10.6% respectively\n\n" +
                                        "**National curriculum year group**\n\n" +
                                        "Overall absence rate:\n\n" +
                                        "* pupils in national curriculum year groups 3 and 4 - lowest at 3.9% and 4% respectively\n\n" +
                                        "* pupils in national curriculum year groups 10 and 11 - highest at 6.1% and 6.2% respectively\n\n" +
                                        "This trend is repeated for the persistent absence rate.\n\n" +
                                        "**Special educational need (SEN)**\n\n" +
                                        "Overall absence rate:\n\n" +
                                        "* pupils with a SEN statement or education healthcare (EHC) plan - 8.2% compared to 4.3% for those with no identified SEN\n\n" +
                                        "Persistent absence rate:\n\n" +
                                        "* pupils with a SEN statement or education healthcare (EHC) plan - more than 2 times higher than pupils with no identified SEN"
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 7, Heading = "Absence for 4-year-olds", Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body =
                                        "The [overall absence](../glossary#overall-absence) rate decreased to 5.1% - down from 5.2% for the previous 2 years.\n\n" +
                                        "Absence recorded for 4-year-olds is not treated as authorised or unauthorised and only reported as overall absence."
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
                                        "The [overall absence](../glossary#overall-absence) rate increased to 33.9% - up from 32.6% in 2015/16.\n\n" +
                                        "The [persistent absence](../glossary#persistent-absence) rate increased to 73.9% - up from 72.5% in 2015/16."
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 9, Heading = "Regional and local authority (LA) breakdown", Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new DataBlock
                                {
                                    DataBlockRequest = new DataBlockRequest
                                    {
                                        subjectId = 1,
                                        geographicLevel = "Local_Authority_District",
                                        startYear = "2016",
                                        endYear = "2017",
                                        indicators = new List<string> {"23", "26", "28"},
                                        filters = new List<string> {"1", "2"}
                                    },
                                    Charts = new List<IContentBlockChart>
                                    {
                                        new MapChart
                                        {
                                            Axes = new Dictionary<string, ChartAxisConfiguration>
                                            {
                                                ["major"] = new ChartAxisConfiguration
                                                {
                                                    GroupBy = new List<string> {"timePeriod"},
                                                    DataSets = new List<ChartDataSet>
                                                    {
                                                        new ChartDataSet
                                                            {Indicator = "23", filters = new List<string> {"1", "2"}},
                                                        new ChartDataSet
                                                            {Indicator = "26", filters = new List<string> {"1", "2"}},
                                                        new ChartDataSet
                                                            {Indicator = "28", filters = new List<string> {"1", "2"}},
                                                    },
                                                    Title = "School Year"
                                                },
                                                ["minor"] = new ChartAxisConfiguration
                                                {
                                                    Title = "Absence Rate",
                                                    GroupBy = new List<string>(),
                                                }
                                            },
                                            Labels = new Dictionary<string, ChartLabelConfiguration>
                                            {
                                                ["2016_HT6"] = new ChartLabelConfiguration
                                                {
                                                    Label = "2016/2017"
                                                },
                                                ["23_1_2_____"] = new ChartLabelConfiguration
                                                {
                                                    Label = "Unauthorised Absence Rate",
                                                    Unit = "%"
                                                },
                                                ["26_1_2_____"] = new ChartLabelConfiguration
                                                {
                                                    Label = "Overall Absence Rate",
                                                    Unit = "%"
                                                },
                                                ["28_1_2_____"] = new ChartLabelConfiguration
                                                {
                                                    Label = "Authorised Absence Rate",
                                                    Unit = "%"
                                                },
                                            }
                                        }
                                    }
                                },
                                new MarkDownBlock
                                {
                                    Body =
                                        "[Overall absence](../glossary#overall-absence) and [persistent absence](../glossary#persistent-absence) rates vary across primary, secondary and special schools by region and local authority (LA).\n\n" +
                                        "**Overall absence**\n\n" +
                                        "Similar to 2015/16, the 3 regions with the highest rates across all school types were:\n\n" +
                                        "* North East - 4.9%\n\n" +
                                        "* Yorkshire and the Humber - 4.9%\n\n" +
                                        "* South West - 4.8%\n\n" +
                                        "Meanwhile, Inner and Outer London had the lowest rates at 4.4%.\n\n" +
                                        "**Persistent absence**\n\n" +
                                        "The region with the highest persistent absence rate was Yorkshire and the Humber with 11.9% while Outer London had the lowest rate at 10%."
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
                        Summary = new Summary
                        {
                            dataKeys = new List<string>
                            {
                                "--",
                                "--",
                                "--"
                            },
                            dataSummary = new List<string>
                            {
                                "",
                                "",
                                ""
                            },
                            description = new MarkDownBlock
                            {
                                Body = ""
                            }
                        }
                    },

                    Content = new List<ContentSection>
                    {
                        new ContentSection {Order = 1, Heading = "About these statistics", Caption = ""},
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
                    Title = "Permanent and fixed-period exclusions statistics for schools in England",
                    ReleaseName = "2016 to 2017",
                    PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                    Published = new DateTime(2018, 7, 19),
                    Slug = "2016-17",
                    Summary =
                        "Read national statistical summaries, view charts and tables and download data files.\n\n" +
                        "Find out how and why these statistics are collected and published - [Permanent and fixed-period exclusion statistics: methodology](../methodology/permanent-and-fixed-period-exclusions-in-england)",
                    KeyStatistics = new DataBlock
                    {
                        DataBlockRequest = new DataBlockRequest
                        {
                            subjectId = 12,
                            geographicLevel = "Country",
                            startYear = "2012",
                            endYear = "2016",
                            filters = new List<string> {"727"},
                            indicators = new List<string> {"153", "154", "155", "156", "157", "158", "160"}
                        },
                        Summary = new Summary
                        {
                            dataKeys = new List<string>
                            {
                                "156",
                                "158",
                                "155"
                            },
                            dataSummary = new List<string>
                            {
                                "Up from 0.08% in 2015/16",
                                "Up from 4.29% in 2015/16",
                                "Up from 6,685 in 2015/16"
                            },
                            description = new MarkDownBlock
                            {
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
                                indicators = new List<string> {"156", "158", "155"}
                            }
                        },

                        Charts = new List<IContentBlockChart>
                        {
                            new LineChart
                            {
                                Axes = new Dictionary<string, ChartAxisConfiguration>
                                {
                                    ["major"] = new ChartAxisConfiguration
                                    {
                                        GroupBy = new List<string> {"timePeriod"},
                                        DataSets = new List<ChartDataSet>
                                        {
                                            new ChartDataSet {Indicator = "158", filters = new List<string> {"727"}},
                                            new ChartDataSet {Indicator = "160", filters = new List<string> {"727"}},
                                        },
                                        Title = "School Year"
                                    },
                                    ["minor"] = new ChartAxisConfiguration
                                    {
                                        Title = "Absence Rate",
                                        GroupBy = new List<string>(),
                                    }
                                },
                                Labels = new Dictionary<string, ChartLabelConfiguration>
                                {
                                    ["2012_HT6"] = new ChartLabelConfiguration
                                    {
                                        Label = "2012/2013"
                                    },
                                    ["2013_HT6"] = new ChartLabelConfiguration
                                    {
                                        Label = "2013/2014"
                                    },
                                    ["2014_HT6"] = new ChartLabelConfiguration
                                    {
                                        Label = "2014/2015"
                                    },
                                    ["2015_HT6"] = new ChartLabelConfiguration
                                    {
                                        Label = "2015/2016"
                                    },
                                    ["2016_HT6"] = new ChartLabelConfiguration
                                    {
                                        Label = "2016/2017"
                                    },
                                    ["158_727_____"] = new ChartLabelConfiguration
                                    {
                                        Label = "Fixed period exclusion Rate",
                                        Unit = "%"
                                    },
                                    ["160_727_____"] = new ChartLabelConfiguration
                                    {
                                        Label = "Pupils with one ore more exclusion",
                                        Unit = "%"
                                    }
                                }
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
                                        "The statistics and data cover permanent and fixed period exclusions and school-level exclusions during the 2016/17 academic year in the following state-funded school types as reported in the school census:\n\n" +
                                        "* primary schools\n\n" +
                                        "* secondary schools\n\n" +
                                        "* special schools\n\n" +
                                        "They also include national-level information on permanent and fixed-period exclusions for [pupil referral units](../glossary#pupil-referral-unit).\n\n" +
                                        "All figures are based on unrounded data so constituent parts may not add up due to rounding."
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 2, Heading = "Permanent exclusions", Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body =
                                        "The number of [permanent exclusions](../glossary#permanent-exclusion) has increased across all state-funded primary, secondary and special schools to 7,720 - up from 6,685 in 2015/16.\n\n" +
                                        "This works out to an average 40.6 permanent exclusions per day - up from an 35.2 per day in 2015/16.\n\n" +
                                        "The permanent exclusion rate has also increased to 0.10% of pupils - up from from 0.08% in 2015/16 - which is equivalent to around 10 pupils per 10,000."
                                },
                                new DataBlock
                                {
                                    Heading = "Chart showing permanent exclusions in England",
                                    DataBlockRequest = new DataBlockRequest
                                    {
                                        subjectId = 12,
                                        geographicLevel = "Country",
                                        startYear = "2012",
                                        endYear = "2016",
                                        filters = new List<string> {"727"},
                                        indicators = new List<string> {"156", "154", "155"}
                                    },

                                    Tables = new List<Table>
                                    {
                                        new Table
                                        {
                                            indicators = new List<string> {"154", "155", "156"}
                                        }
                                    },

                                    Charts = new List<IContentBlockChart>
                                    {
                                        new LineChart
                                        {
                                            Axes = new Dictionary<string, ChartAxisConfiguration>
                                            {
                                                ["major"] = new ChartAxisConfiguration
                                                {
                                                    GroupBy = new List<string> {"timePeriod"},
                                                    DataSets = new List<ChartDataSet>
                                                    {
                                                        new ChartDataSet
                                                            {Indicator = "156", filters = new List<string> {"727"}}
                                                    },
                                                    Title = "School Year"
                                                },
                                                ["minor"] = new ChartAxisConfiguration
                                                {
                                                    Title = "Exclusion Rate",
                                                    GroupBy = new List<string>()
                                                }
                                            },
                                            Labels = new Dictionary<string, ChartLabelConfiguration>
                                            {
                                                ["2012_HT6"] = new ChartLabelConfiguration
                                                {
                                                    Label = "2012/2013"
                                                },
                                                ["2013_HT6"] = new ChartLabelConfiguration
                                                {
                                                    Label = "2013/2014"
                                                },
                                                ["2014_HT6"] = new ChartLabelConfiguration
                                                {
                                                    Label = "2014/2015"
                                                },
                                                ["2015_HT6"] = new ChartLabelConfiguration
                                                {
                                                    Label = "2015/2016"
                                                },
                                                ["2016_HT6"] = new ChartLabelConfiguration
                                                {
                                                    Label = "2016/2017"
                                                },
                                                ["156_727_____"] = new ChartLabelConfiguration
                                                {
                                                    Label = "Fixed period exclusion Rate",
                                                    Unit = "%"
                                                },
                                            },
                                        },
                                    },
                                },

                                new MarkDownBlock
                                {
                                    Body =
                                        "Most occurred in secondary schools which accounted for 83% of all permanent exclusions.\n\n"
                                        +
                                        "The [permanent exclusion](../glossary#permanent-exclusion) rate in secondary schools increased 0.20% - up from from 0.17% in 2015/16 - which is equivalent to 20 pupils per 10,000.\n\n"
                                        +
                                        "The rate also rose in primary schools to 0.03% but decreased in special schools to 0.07% - down from from 0.08% in 2015/16.\n\n"
                                        +
                                        "The rate generally followed a downward trend after 2006/07 - when it stood at 0.12%.\n\n"
                                        +
                                        "However, since 2012/13 it has been on the rise although rates are still lower now than in 2006/07."
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 3, Heading = "Fixed-period exclusions", Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body =
                                        "The number of fixed-period exclusionshas increased across all state-funded primary, secondary and special schools to 381,865 - up from 339,360 in 2015/16.\n\n" +
                                        "This works out to around 2,010 fixed-period exclusions per day - up from an 1,786 per day in 2015/16."
                                },
                                new DataBlock
                                {
                                    Heading = "Chart showing fixed-period exclusions in England",
                                    DataBlockRequest = new DataBlockRequest
                                    {
                                        subjectId = 12,
                                        geographicLevel = "Country",
                                        startYear = "2012",
                                        endYear = "2016",
                                        filters = new List<string> {"727"},
                                        indicators = new List<string> {"158", "154", "157"}
                                    },

                                    Tables = new List<Table>
                                    {
                                        new Table
                                        {
                                            indicators = new List<string> {"154", "157", "158"}
                                        }
                                    },

                                    Charts = new List<IContentBlockChart>
                                    {
                                        new LineChart
                                        {
                                            Axes = new Dictionary<string, ChartAxisConfiguration>
                                            {
                                                ["major"] = new ChartAxisConfiguration
                                                {
                                                    GroupBy = new List<string> {"timePeriod"},
                                                    DataSets = new List<ChartDataSet>
                                                    {
                                                        new ChartDataSet
                                                            {Indicator = "158", filters = new List<string> {"727"}},
                                                    },
                                                    Title = "School Year"
                                                },
                                                ["minor"] = new ChartAxisConfiguration
                                                {
                                                    Title = "Absence Rate",
                                                    GroupBy = new List<string>(),
                                                }
                                            },
                                            Labels = new Dictionary<string, ChartLabelConfiguration>
                                            {
                                                ["2012_HT6"] = new ChartLabelConfiguration
                                                {
                                                    Label = "2012/2013"
                                                },
                                                ["2013_HT6"] = new ChartLabelConfiguration
                                                {
                                                    Label = "2013/2014"
                                                },
                                                ["2014_HT6"] = new ChartLabelConfiguration
                                                {
                                                    Label = "2014/2015"
                                                },
                                                ["2015_HT6"] = new ChartLabelConfiguration
                                                {
                                                    Label = "2015/2016"
                                                },
                                                ["2016_HT6"] = new ChartLabelConfiguration
                                                {
                                                    Label = "2016/2017"
                                                },
                                                ["158_727_____"] = new ChartLabelConfiguration
                                                {
                                                    Label = "Fixed period exclusion Rate",
                                                    Unit = "%"
                                                }
                                            }
                                        }
                                    }
                                },
                                new MarkDownBlock
                                {
                                    Body =
                                        "**Primary schools**\n\n" +
                                        "* fixed-period exclusions numbers increased to 64,340 - up from 55,740 in 2015/16\n\n" +
                                        "* fixed-period exclusions rate increased to 1.37% - up from 1.21% in 2015/16\n\n" +
                                        "**Secondary schools**\n\n" +
                                        "* fixed-period exclusions numbers increased to 302,890 - up from 270,135 in 2015/16\n\n" +
                                        "* fixed-period exclusions rate increased to 9.4% - up from 8.46% in 2015/16\n\n" +
                                        "**Special schools**\n\n" +
                                        "* fixed-period exclusions numbers increased to 14,635 - up from 13,485 in 2015/16\n\n" +
                                        "* fixed-period exclusions rate increased to 13.03% - up from 12.53% in 2015/16"
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 4, Heading = "Number and length of fixed-period exclusions",
                            Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body =
                                        "**Pupils with one or more fixed-period exclusion definition**\n\n" +
                                        "The number of pupils with [one or more fixed-period exclusion](../glossary#one-or-more-fixed-period-exclusion) has increased across state-funded primary, secondary and special schools to 183,475 (2.29% of pupils) up from 167,125 (2.11% of pupils) in 2015/16.\n\n" +
                                        "Of these kinds of pupils, 59.1% excluded on only 1 occasion while 1.5% received 10 or more fixed-period exclusions during the year.\n\n" +
                                        "The percentage of pupils who went on to receive a [permanent exclusion](../glossary#permanent-exclusion) was 3.5%.\n\n" +
                                        "The average length of [fixed-period exclusion](../glossary#fixed-period-exclusion) across schools decreased to 2.1 days - slightly shorter than in 2015/16.\n\n" +
                                        "The highest proportion of fixed-period exclusions (46.6%) lasted for only 1 day.\n\n" +
                                        "Only 2.0% of fixed-period exclusions lasted for longer than 1 week and longer fixed-period exclusions were more prevalent in secondary schools."
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
                                        "All reasons (except bullying and theft) saw an increase in [permanent exclusions](../glossary#permanent-exclusion) since 2015/16.\n\n" +
                                        "The following most common reasons saw the largest increases:\n\n" +
                                        "* physical assault against a pupil\n\n" +
                                        "* persistent disruptive behaviour\n\n" +
                                        "* other reasons\n\n" +
                                        "**Persistent disruptive behaviour**\n\n" +
                                        "Remained the most common reason for permanent exclusions accounting for 2,755 (35.7%) of all permanent exclusions - which is equivalent to 3 permanent exclusions per 10,000 pupils.\n\n" +
                                        "However, in special schools the most common reason for exclusion was physical assault against an adult - accounting for 37.8% of all permanent exclusions and 28.1% of all [fixed-period exclusions](../glossary#fixed-period-exclusion).\n\n" +
                                        "Persistent disruptive behaviour is also the most common reason for fixed-period exclusions accounting for 108,640 %) of all fixed-period exclusions - up from 27.7% in 2015/16. This is equivalent to around 135 fixed-period exclusions per 10,000 pupils.\n\n" +
                                        "All reasons saw an increase in fixed-period exclusions since 2015/16. Persistent disruptive behaviour and other reasons saw the largest increases."
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
                                        "There was a similar pattern to previous years where the following groups (where higher exclusion rates are expected) showed an increase in exclusions since 2015/16:\n\n" +
                                        "* boys\n\n" +
                                        "* national curriculum years 9 and 10\n\n" +
                                        "* pupils with special educational needs (SEN)\n\n" +
                                        "* pupils known to be eligible for and claiming free school meals (FSM)\n\n" +
                                        "**Age, national curriculum year group and gender**\n\n" +
                                        "* more than half of all [permanent exclusions](../glossary#permanent-exclusion) (57.2%) and [fixed-period exclusions](../glossary#fixed-period-exclusion) (52.6 %) occur in national curriculum year 9 or above\n\n" +
                                        "* a quarter (25%) of all permanent exclusions were for pupils aged 14 - who also had the highest rates for fixed-period exclusion and pupils receiving [one or more fixed-period exclusion](../glossary#one-or-more-fixed-period-exclusion)\n\n" +
                                        "* the permanent exclusion rate for boys (0.15%) was more than 3 times higher than for girls (0.04%)\n\n" +
                                        "* the fixed-period exclusion rate for boys (6.91%) was almost 3 times higher than for girls (2.53%)\n\n" +
                                        "**Pupils eligible for and claiming free school meals (FSM)**\n\n" +
                                        "* had a permanent exclusion rate of 0.28% and fixed period exclusion rate of 12.54% - around 4 times higher than those not eligible for FSM at 0.07% and 3.50% respectively\n\n" +
                                        "* accounted for 40% of all permanent exclusions and 36.7% of all fixed-period exclusions\n\n" +
                                        "**Special educational needs (SEN) pupils**\n\n" +
                                        "* accounted for around half of all permanent exclusions (46.7%) and fixed-period exclusions (44.9%)\n\n" +
                                        "* had the highest permanent exclusion rate (0.35%0 - 6 times higher than the rate for pupils with no SEN (0.06%)\n\n" +
                                        "* pupils with a statement of SEN or education, health and care (EHC) plan had the highest fixed-period exclusion rate at 15.93% - more than 5 times higher than pupils with no SEN (3.06%)\n\n" +
                                        "**Ethnic group**\n\n" +
                                        "* pupils of Gypsy/Roma and Traveller of Irish Heritage ethnic groups had the highest rates of permanent and fixed-period exclusions - but as the population is relatively small these figures should be treated with some caution\n\n" +
                                        "* pupils from a Black Caribbean background had a permanent exclusion rate nearly 3 times higher (0.28%) than the school population as a whole (0.10%)\n\n" +
                                        "* pupils of Asian ethnic groups had the lowest permanent and fixed-period exclusion rates"
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
                                        "There were 560 reviews lodged with [independent review panels](../glossary#independent-review-panel) in maintained primary, secondary and special schools and academies of which 525 (93.4%) were determined and 45 (8.0%) resulted in an offer of reinstatement."
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 8, Heading = "Pupil referral units exclusions", Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body =
                                        "**Permanent exclusion**\n\n" +
                                        "The [permanent exclusion](../glossary#permanent-exclusion) rate in [pupil referral units](../glossary#pupil-referral-unit) decreased to 0.13 - down from 0.14% in 2015/16.\n\n" +
                                        "Permanent exclusions rates have remained fairly steady following an increase between 2013/14 and 2014/15.\n\n" +
                                        "**Fixed-period exclusion**\n\n" +
                                        "The [fixed period exclusion](../glossary#fixed-period-exclusion) rate has been steadily increasing since 2013/14.\n\n" +
                                        "The percentage of pupils in pupil referral units who 1 or more fixed-period exclusion increased to 59.17% - up from 58.15% in 2015/16."
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 9, Heading = "Regional and local authority (LA) breakdown",
                            Caption = "",
                            Content = new List<IContentBlock>
                            {
                                // new DataBlock {
                                //     DataBlockRequest = new DataBlockRequest {
                                //         subjectId = 12,
                                //         geographicLevel = "Local_Authority",
                                //         startYear = "2016",
                                //         endYear = "2017",
                                //         indicators = new List<string> { "155" , "156" , "158" },
                                //         filters = new List<string> { "727" }
                                //     },
                                //     Charts = new List<IContentBlockChart> {
                                //         new MapChart {
                                //             Indicators = new List<string> { "155" , "156" , "158" }
                                //         }
                                //     }

                                // },
                                new MarkDownBlock
                                {
                                    Body =
                                        "There's considerable variation in the [permanent exclusion](../glossary#permanent-exclusion) and [fixed-period exclusion](../glossary#fixed-period-exclusion) rate at the LA level.\n\n" +
                                        "**Permanent exclusion**\n\n" +
                                        "Similar to 2015/16, the regions with the joint-highest rates across all school types were:\n\n" +
                                        "* North West - 0.14%\n\n" +
                                        "* North West - 0.14%\n\n" +
                                        "Similar to 2015/16, the regions with the lowest rates were:\n\n" +
                                        "* South East - 0.06%\n\n" +
                                        "* Yorkshire and the Humber - 0.07%\n\n" +
                                        "**Fixed-period exclusion**\n\n" +
                                        "Similar to 2015/16, the region with the highest rates across all school types was Yorkshire and the Humber at 7.22% while the lowest rate was in Outer London (3.49%)."
                                }
                            }
                        }
                    }
                },
                /*
                // // school pupil numbers
                new Release
                {
                    Id = new Guid("e3288537-9adb-431d-adfb-9bc3ef7be48c"),
                    Title = "Schools, pupils and their characteristics: January 2018",
                    ReleaseName = "January 2018",
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
                            dataSummary = new List<string>
                            {
                                "",
                                "",
                                ""
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
                */
                /*
                // // GCSE / KS4
                new Release
                {
                    Id = new Guid("e7ae88fb-afaf-4d51-a78a-bbb2de671daf"),
                    Title = "GCSE and equivalent results in England, 2016 to 2017",
                    ReleaseName = "2016 to 2017",
                    PublicationId = new Guid("bfdcaae1-ce6b-4f63-9b2b-0a1f3942887f"),
                    Published = new DateTime(2018, 6, 20),
                    Slug = "2016-17",
                    Summary =
                        "This statistical first release (SFR) provides information on the achievements in GCSE examinations and other qualifications of young people in academic year 2016 to 2017. This typically covers those starting the academic year aged 15. \n\n" +
                        "You can also view a regional breakdown of statistics and data within the [local authorities section](#contents-sections-content-6) \n\n" +
                        "[Find out more about our GCSE and equivalent results methodology and terminology](#extra-information-sections-heading-1)",
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
                            dataSummary = new List<string>
                            {
                                "",
                                "",
                                ""
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
                                    Body =
                                        "This release shows results for GCSE and equivalent Key Stage 4 (KS4) qualifications in 2018 across a range of measures, broken down by pupil characteristics and education institutions. Results are also provided on schools below the floor standards and meeting the coasting definition.  \n\n" +
                                        "This is an update to Provisional figures released in October 2018. Users should be careful when comparing headline measures to results in previous years given recent methodological changes \n\n" +
                                        "Figures are available at national, regional, local authority, and school level. Figures held in this release are used for policy development and count towards the secondary performance tables. Schools and local authorities also use the statistics to compare their local performance to regional and national averages for different pupil groups."
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 2,
                            Heading = "School performance for 2018",
                            Caption =
                                "School performance for 2018 shows small increases across all headline measures compared to 2017",
                            Content = new List<IContentBlock>
                            {
                                new DataBlock
                                {
                                    Heading = "Average headline performance measures over time",
                                },
                                new MarkDownBlock
                                {
                                    Body =
                                        "Results for 2018 show an increases across all headline measures compared to 2017. **When drawing comparison over time, however, it is very important to note any changes to methodology or data changes underpinning these measures**. For example, changes in Attainment 8 may have been affected by the introduction of further reformed GCSEs graded on the 9-1 scale which have a higher maximum score than unreformed GCSEs. Similarly, in 2016 there were significant changes to the Attainment in English and Maths measure. \n\n" +
                                        "These results cover state-funded schools but results for all schools are available in the supporting tables and show slightly lower performance across all headline measures on average. Differences between the figures for all schools and state-funded schools are primarily due to the impact of unapproved and unregulated qualifications such as international GCSEs taken more commonly in independent schools. These qualification are not included in school performance tables. \n\n" +
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
                                    Heading =
                                        "There is wide variation in the percentage of schools meeting the coasting and floor standard by region"
                                },
                                new MarkDownBlock
                                {
                                    Body =
                                        "The floor and coasting standards give measures of whether schools are helping pupils to fulfil their potential based on progress measures. The floor standard is based on results in the most recent year, whereas the Coasting definition looks at slightly different measures over the past three years. Only state-funded mainstream schools are covered by these measures, subject to certain eligibility criteria. \n" +
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
                            Caption =
                                "Disadvantaged pupils and those with Special Education Needs continue to do less well than their peers",
                            Content = new List<IContentBlock>
                            {
                                new DataBlock
                                {
                                    Heading = "Average headline scores by pupil characteristics"
                                },
                                new MarkDownBlock
                                {
                                    Body =
                                        "Breakdowns by pupil characteristics show that across all headline measures: \n" +
                                        "* girls continue to do better than boys \n" +
                                        "* non-disadvantaged pupils continue to do better than disadvantaged pupils \n" +
                                        "* pupils with no identified Special Educational Needs (SEN) continue to do better perform than SEN pupils \n" +
                                        "In general the pattern of attainment gaps for Attainment 8 in 2018 remained the same as in 2017 although differences in Attainment 8 scores widened slightly across all groups. This is to be expected due to changes to reformed GCSEs in 2018, meaning more points are available for higher scores.  \n\n" +
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
                                    Body =
                                        "Results across headline measures differ by ethnicity with Chinese pupils in particular achieving scores above the national average. \n\n" +
                                        "Performance across headline measures increased for all major ethnic groups from 2017 to 2018, with the exception of EBacc entries for white pupils were there was a small decrease. \n\n" +
                                        "Within the more detailed ethnic groupings, pupils from an Indian background are the highest performing group in key stage 4 headline measures other than Chinese pupils. Gypsy/Roma pupils and traveller of Irish heritage pupils are the lowest performing groups. \n\n" +
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
                                    Body =
                                        "Performance varies considerably across the country – for Attainment 8 score per pupil there is nearly a 23 point gap between the poorest and highest performing areas. The highest performing local authorities are concentrated in London and the south with the majority of the lowest performing local authorities are located in the northern and midland regions with average Attainment 8 score per pupil show that. This is similar to patterns seen in recent years and against other performance measures. "
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 7,
                            Heading = "Pupil subject areas",
                            Caption =
                                "Pupil subject entries are highest for science and humanities and continue to increase",
                            Content = new List<IContentBlock>
                            {
                                new DataBlock
                                {
                                    Heading =
                                        "Pupil subject entries are highest for science and humanities and continue to increase"
                                },
                                new MarkDownBlock
                                {
                                    Body =
                                        "It is compulsory for pupils to study English and Maths at key stage 4 in state-funded schools.  \n " +
                                        "### Science\n" +
                                        "It is compulsory for schools to teach Science at Key Stage 4. For these subjects, the proportion of pupils entering continues to increase.  \n\n " +
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
                            Caption =
                                "Across state-funded schools performance is typically higher in converter academies, the most common school type",
                            Content = new List<IContentBlock>
                            {
                                new DataBlock
                                {
                                    Heading =
                                        "Across state-funded schools performance is typically higher in converter academies, the most common school type"
                                },
                                new MarkDownBlock
                                {
                                    Body =
                                        "Schools in England can be divided into state-funded and independent schools (funded by fees paid by attendees). Independent schools are considered separately, because the department holds state-funded schools accountable for their performance.  \n\n " +
                                        "The vast majority of pupils in state-funded schools are in either academies (68%) or LA maintained schools (29%). *Converter academies* were high performing schools that chose to convert to academies and have on average higher attainment across the headline measures. *Sponsored academies* were schools that were low performing prior to conversion and tend to perform below the average for state-funded schools.  \n\n " +
                                        "Between 2017 and 2018 EBacc entry remained stable for sponsored academies, with an increase of 0.1 percentage points to 30.1%. EBacc entry fell marginally for converter academies by 0.3 percentage points (from 44.2% to 43.8%). Over the same period, EBacc entry in local authority maintained schools increased by 0.2 percentage points to 37.0%."
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 9,
                            Heading = "Attainment",
                            Caption =
                                "Multi-academy trust schools generally perform below national averages, but typically face greater challenges.",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body =
                                        "Academies are state schools directly funded by the government, each belonging to a trust. Multi-Academy Trusts (MATs) can be responsible for a group of academies and cover around 13.6% of state-funded mainstream pupils. Most MATs are responsible for between 3 and 5 schools but just over 10% cover 11 or more schools.  \n\n" +
                                        "Generally speaking MATs are typically more likely to cover previously poor-performing schools and pupils are more likely to have lower prior attainment, be disadvantaged, have special educational needs (SEN) or have English as an additional language (EAL) than the national average. \n\n" +
                                        "The number of eligible MATs included in Key Stage 4 measures increased from 62 in 2017 to 85 in 2018. This is an increase from 384 to 494 schools, and from 54,356 to 69,169 pupils. "
                                },
                                new DataBlock
                                {
                                    Heading = "Performance in MATs compared to national average"
                                },
                                new MarkDownBlock
                                {
                                    Body =
                                        "On Progress8 measures, in 2018, 32.9% of MATs were below the national average and 7.1% well below average. 29.4% were not above or below the national average by a statistically significant amount. \n\n" +
                                        "Entry rate in EBacc is lower in MATs compared to the national average – in 2018 43.5% of MATs had an entry rate higher than the national average of 39.1%. The EBacc average point score is also lower in MATs – 32.9% of MATs had an APS higher than the national average. \n\n" +
                                        "Analysis by characteristics shows that in 2018 disadvantaged pupils in MATs made more progress than the national average for disadvantaged. However, non-disadvantaged pupils, SEN and non-SEN pupils, pupils with English as a first language and high prior attainment pupils made less progress than the national average for their respective group."
                                }
                            }
                        },
                    }
                },
                */
                // Secondary and primary schools applications offers
                new Release
                {
                    Id = new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"),
                    Title = "Secondary and primary school applications and offers",
                    ReleaseName = "2018",
                    PublicationId = new Guid("66c8e9db-8bf2-4b0b-b094-cfab25c20b05"),
                    Published = new DateTime(2018, 6, 14),
                    Slug = "2018",
                    Summary =
                        "Read national statistical summaries, view charts and tables and download data files.\n\n" +
                        "Find out how and why these statistics are collected and published - [Secondary and primary school applications and offers: methodology](../methodology/secondary-and-primary-schools-applications-and-offers)",
                    KeyStatistics = new DataBlock
                    {
                        DataBlockRequest = new DataBlockRequest
                        {
                            subjectId = 17,
                            geographicLevel = "Country",
                            startYear = "2014",
                            endYear = "2018",
                            filters = new List<string> {"845"},
                            indicators = new List<string>
                                {"189", "193", "194", "195", "196", "197", "198", "199"}
                        },
                        Summary = new Summary
                        {
                            dataKeys = new List<string>
                            {
                                "189", "193", "194"
                            },
                            dataSummary = new List<string>
                            {
                                "Down from 620,330 in 2017",
                                "Down from 558,411 in 2017",
                                "Down from 34,792 in 2017"
                            },
                            description = new MarkDownBlock
                            {
                                Body =
                                    "* majority of applicants received a preferred offer\n" +
                                    "* percentage of applicants receiving secondary first choice offers decreases as applications increase\n" +
                                    "* slight proportional increase in applicants receiving primary first choice offer as applications decrease\n"
                            },
                        },
                        Tables = new List<Table>
                        {
                            new Table
                            {
                                indicators = new List<string> {"189", "193", "194", "195", "198", "199"}
                            }
                        }

                        /*
                        Charts = new List<IContentBlockChart> {
                            new LineChart
                            {
                                XAxis = new Axis
                                {
                                    title = "School Year"
                                },
                                YAxis = new Axis
                                {
                                    title = ""
                                },
                                Indicators = new List<string>
                                {
                                    "189", "196", "197"
                                },
                            }
                        }
                        */
                    },
                    Content = new List<ContentSection>
                    {
                        new ContentSection
                        {
                            Order = 1, Heading = "About this release",
                            Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body =
                                        "The statistics and data cover the number of offers made to applicants for primary and secondary school places and the proportion which have received their preferred offers.\n\n" +
                                        "The data was collected from local authorities (LAs) where it was produced as part of the annual applications and offers process for applicants requiring a primary or secondary school place in September 2018.\n\n" +
                                        "The offers were made, and data collected, based on the National Offer Days of 1 March 2018 for secondary schools and 16 April 2018 for primary schools."
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 2, Heading = "Secondary applications and offers",
                            Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body =
                                        "**Secondary applications**\n\n" +
                                        "The number of applications received for secondary school places increased to 582,761 - up 3.6% since 2017. This follows a 2.6% increase between 2016 and 2017.\n\n" +
                                        "This continues the increase in secondary applications seen since 2013 which came on the back of a rise in births which began in the previous decade.\n\n" +
                                        "Since 2013, when secondary applications were at their lowest, there has been a 16.6% increase in the number of applications.\n\n" +
                                        "**Secondary offers**\n\n" +
                                        "The proportion of secondary applicants receiving an offer of their first-choice school has decreased to 82.1% - down from 83.5% in 2017.\n\n" +
                                        "The proportion of applicants who received an offer of any of their preferred schools also decreased slightly to 95.5% - down from 96.1% in 2017.\n\n" +
                                        "**Secondary National Offer Day**\n\n" +
                                        "These statistics come from the process undertaken by local authorities (LAs) which enabled them to send out offers of secondary school places to all applicants on the [Secondary National Offer Day](../glossary#national-offer-day) of 1 March 2018.\n\n" +
                                        "The secondary figures have been collected since 2008 and can be viewed as a time series in the following table."
                                },
                                new DataBlock
                                {
                                    Heading =
                                        "Table of Timeseries of key secondary preference rates, England",
                                    DataBlockRequest = new DataBlockRequest
                                    {
                                        subjectId = 17,
                                        geographicLevel = "Country",
                                        startYear = "2014",
                                        endYear = "2018",
                                        filters = new List<string> {"848"},
                                        indicators = new List<string> {"197", "198", "199", "200"}
                                    },
                                    Tables = new List<Table>
                                    {
                                        new Table {indicators = new List<string> {"197", "198", "199"}}
                                    }
                                },
                            }
                        },
                        new ContentSection
                        {
                            Order = 3, Heading = "Secondary geographical variation",
                            Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body =
                                        "**First preference rates**\n\n" +
                                        "At local authority (LA) level, the 3 highest first preference rates were achieved by the following local authorities:\n\n" +
                                        "* Northumberland - 98.1%\n\n" +
                                        "* East Riding of Yorkshire - 96.7%\n\n" +
                                        "* Bedford - 96.4%\n\n" +
                                        "Northumberland has been the top performer in this measure since 2015.\n\n" +
                                        "As in previous years, the lowest first preference rates were all in London.\n\n" +
                                        "* Hammersmith and Fulham - 51.4%\n\n" +
                                        "* Kensington and Chelsea - 54.3%\n\n" +
                                        "* Lambeth - 55.2%\n\n" +
                                        "These figures do not include City of London which has a tiny number of applications and no secondary schools.\n\n" +
                                        "Hammersmith and Fulham has had the lowest first preference rate since 2015.\n\n" +
                                        "The higher number of practical options available to London applicants and ability to name 6 preferences may encourage parents to make more speculative choices for their top preferences.\n\n" +
                                        "**Regional variation**\n\n" +
                                        "There's much less regional variation in the proportions receiving any preferred offer compared to those for receiving a first preference as shown in the following chart."
                                },
                                // new DataBlock
                                // {
                                //   Heading = "Chart showing Secondary school preferences by region, 2018",
                                //   DataBlockRequest = new DataBlockRequest {
                                //       subjectId = 17,
                                //       geographicLevel = "Local_Authority",
                                //       startYear = "2017",
                                //       endYear = "2018",
                                //       indicators = new List<string> { "192" },
                                //       filters = new List<string> { "848" }
                                //   },
                                //   Charts = new List<IContentBlockChart> {
                                //       new MapChart {
                                //           Indicators = new List<string> { "192" }
                                //       }
                                //   }
                                // },
                                new MarkDownBlock
                                {
                                    Body =
                                        "An applicant can apply for any school, including those situated in another local authority (LA).\n\n" +
                                        "Their authority liaises with the requested school (to make sure the applicant is considered under the admissions criteria) and makes the offer.\n\n" +
                                        "**Secondary offers**\n\n" +
                                        "In 2018, 91.6% of secondary offers made were from schools inside the home authority. This figure has been stable for the past few years.\n\n" +
                                        "This release concentrates on the headline figures for the proportion of children receiving their first preference or a preferred offer.\n\n" +
                                        "However, the main tables provide more information including:\n\n" +
                                        "* the number of places available\n\n" +
                                        "* the proportion of children for whom a preferred offer was not received\n\n" +
                                        "* whether applicants were provided with offers inside or outside their home authority"
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 4, Heading = "Primary applications and offers",
                            Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body =
                                        "**Primary applications**\n\n" +
                                        "The number of applications received for primary school places decreased to 608,180 - down 2% on 2017 (620,330).\n\n" +
                                        "This is the result of a notable fall in births since 2013 which is now feeding into primary school applications.\n\n" +
                                        "The number of primary applications is the lowest seen since 2013 - when this data was first collected.\n\n" +
                                        "**Primary offers**\n\n" +
                                        "The proportion of primary applicants receiving an offer of their first-choice school has increased to 91% - up from 90% in 2017.\n\n" +
                                        "The proportion of applicants who received an offer of any of their offer of any of their preferences has also increased slightly to 98.1% - up from 97.7% in 2017.\n\n" +
                                        "**Primary National Offer Day**\n\n" +
                                        "These statistics come from the process undertaken by local authorities (LAs) which enabled them to send out offers of primary school places to all applicants on the Primary National Offer Day of 16 April 2018.\n\n" +
                                        "The primary figures have been collected and published since 2014 and can be viewed as a time series in the following table."
                                },
                                new DataBlock
                                {
                                    Heading =
                                        "Table showing Timeseries of key primary preference rates, England Entry into academic year",
                                    DataBlockRequest = new DataBlockRequest
                                    {
                                        subjectId = 17,
                                        geographicLevel = "Country",
                                        startYear = "2014",
                                        endYear = "2018",
                                        filters = new List<string> {"845"},
                                        indicators = new List<string> {"197", "198", "199", "200"}
                                    },
                                    Tables = new List<Table>
                                    {
                                        new Table {indicators = new List<string> {"197", "198", "199"}}
                                    }
                                }
                            }
                        },
                        new ContentSection
                        {
                            Order = 5, Heading = "Primary geographical variation",
                            Caption = "",
                            Content = new List<IContentBlock>
                            {
                                new MarkDownBlock
                                {
                                    Body =
                                        "**First preference rates**\n\n" +
                                        "At local authority (LA) level, the 3 highest first preference rates were achieved by the following local authorities:\n\n" +
                                        "* East Riding of Yorkshire - 97.6%\n\n" +
                                        "* Northumberland - 97.4%\n\n" +
                                        "* Rutland - 97.4%\n\n" +
                                        "These authorities are in the top 3 for the first time since 2015.\n\n" +
                                        "The lowest first preference rates were all in London.\n\n" +
                                        "* Kensington and Chelsea - 68.4%\n\n" +
                                        "* Camden - 76.5%\n\n" +
                                        "* Hammersmith and Fulham - 76.6%\n\n" +
                                        "Hammersmith and Fulham and Kensington and Chelsea have both been in the bottom 3 since 2015.\n\n" +
                                        "Although overall results are better at primary level than at secondary, for London as a whole the improvement is much more marked:\n\n" +
                                        "* primary first preference rate increased to 86.6% - up from 85.9% in 2017\n\n" +
                                        "* secondary first preference rate decreased to 66% - down from 68.% in 2017"
                                },
                                // new DataBlock
                                // {
                                //   Heading = "Chart showing Primary school preferences by region, 2018",
                                //   DataBlockRequest = new DataBlockRequest {
                                //       subjectId = 17,
                                //       geographicLevel = "Local_Authority",
                                //       startYear = "2017",
                                //       endYear = "2018",
                                //       indicators = new List<string> { "193" },
                                //       filters = new List<string> { "845" }
                                //   },
                                //   Charts = new List<IContentBlockChart> {
                                //       new MapChart {
                                //           Indicators = new List<string> { "193" }
                                //       }
                                //   }
                                // },
                                new MarkDownBlock
                                {
                                    Body =
                                        "**Primary offers**\n\n" +
                                        "In 2018, 97.1% of primary offers made were from schools inside the home authority. This figure has been stable since 2014 when this data was first collected and published.\n\n" +
                                        "As in previous years, at primary level a smaller proportion of offers were made of schools outside the applicant’s home authority compared to secondary level."
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
                    Summary =
                        "Find out about the methodology behind pupil absence statistics and data and how and why they're collected and published.",
                    PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                    Content = new List<ContentSection>
                    {
                        new ContentSection
                        {
                            Heading = "1. Overview of absence statistics",
                            Caption = "",
                            Order = 1,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Body = File.Exists(@"Migrations/Html/Pupil_Absence_Statistics/Section1.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Absence_Statistics/Section1.html",
                                            Encoding.UTF8)
                                        : ""
                                },
                            }
                        },
                        new ContentSection
                        {
                            Heading = "2. National Statistics badging",
                            Caption = "",
                            Order = 2,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Body = File.Exists(@"Migrations/Html/Pupil_Absence_Statistics/Section2.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Absence_Statistics/Section2.html",
                                            Encoding.UTF8)
                                        : ""
                                },
                            }
                        },
                        new ContentSection
                        {
                            Heading = "3. Methodology",
                            Caption = "",
                            Order = 3,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Body = File.Exists(@"Migrations/Html/Pupil_Absence_Statistics/Section3.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Absence_Statistics/Section3.html",
                                            Encoding.UTF8)
                                        : ""
                                },
                            }
                        },
                        new ContentSection
                        {
                            Heading = "4. Data collection",
                            Caption = "",
                            Order = 4,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Body = File.Exists(@"Migrations/Html/Pupil_Absence_Statistics/Section4.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Absence_Statistics/Section4.html",
                                            Encoding.UTF8)
                                        : ""
                                },
                            }
                        },
                        new ContentSection
                        {
                            Heading = "5. Data processing",
                            Caption = "",
                            Order = 5,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Body = File.Exists(@"Migrations/Html/Pupil_Absence_Statistics/Section5.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Absence_Statistics/Section5.html",
                                            Encoding.UTF8)
                                        : ""
                                },
                            }
                        },
                        new ContentSection
                        {
                            Heading = "6. Data quality",
                            Caption = "",
                            Order = 6,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Body = File.Exists(@"Migrations/Html/Pupil_Absence_Statistics/Section6.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Absence_Statistics/Section6.html",
                                            Encoding.UTF8)
                                        : ""
                                },
                            }
                        },
                        new ContentSection
                        {
                            Heading = "7. Contacts",
                            Caption = "",
                            Order = 7,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Body = File.Exists(@"Migrations/Html/Pupil_Absence_Statistics/Section7.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Absence_Statistics/Section7.html",
                                            Encoding.UTF8)
                                        : ""
                                },
                            }
                        }
                    },
                    Annexes = new List<ContentSection>
                    {
                        new ContentSection
                        {
                            Heading = "Annex A - Calculations",
                            Caption = "",
                            Order = 1,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Body = File.Exists(@"Migrations/Html/Pupil_Absence_Statistics/AnnexA.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Absence_Statistics/AnnexA.html",
                                            Encoding.UTF8)
                                        : ""
                                },
                            }
                        },
                        new ContentSection
                        {
                            Heading = "Annex B - School attendance codes",
                            Caption = "",
                            Order = 2,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Body = File.Exists(@"Migrations/Html/Pupil_Absence_Statistics/AnnexB.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Absence_Statistics/AnnexB.html",
                                            Encoding.UTF8)
                                        : ""
                                },
                            }
                        },
                        new ContentSection
                        {
                            Heading = "Annex C - Links to pupil absence national statistics and data",
                            Caption = "",
                            Order = 3,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Body = File.Exists(@"Migrations/Html/Pupil_Absence_Statistics/AnnexC.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Absence_Statistics/AnnexC.html",
                                            Encoding.UTF8)
                                        : ""
                                },
                            }
                        },
                        new ContentSection
                        {
                            Heading = "Annex D - Standard breakdowns",
                            Caption = "",
                            Order = 4,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Body = File.Exists(@"Migrations/Html/Pupil_Absence_Statistics/AnnexD.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Absence_Statistics/AnnexD.html",
                                            Encoding.UTF8)
                                        : ""
                                },
                            }
                        },
                        new ContentSection
                        {
                            Heading = "Annex E - Timeline",
                            Caption = "",
                            Order = 5,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Body = File.Exists(@"Migrations/Html/Pupil_Absence_Statistics/AnnexE.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Absence_Statistics/AnnexE.html",
                                            Encoding.UTF8)
                                        : ""
                                },
                            }
                        },
                        new ContentSection
                        {
                            Heading = "Annex F - Absence rates over time",
                            Caption = "",
                            Order = 6,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Body = File.Exists(@"Migrations/Html/Pupil_Absence_Statistics/AnnexF.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Absence_Statistics/AnnexF.html",
                                            Encoding.UTF8)
                                        : ""
                                },
                            }
                        }
                    }
                },
                new Methodology
                {
                    Id = new Guid("8ab41234-cc9d-4b3d-a42c-c9fce7762719"),
                    Title = "Secondary and primary school applications and offers: methodology",
                    Published = new DateTime(2018, 6, 14),
                    Summary = "",
                    PublicationId = new Guid("66c8e9db-8bf2-4b0b-b094-cfab25c20b05"),
                    Content = new List<ContentSection>
                    {
                        new ContentSection
                        {
                            Heading = "1. Overview of applications and offers statistics",
                            Caption = "",
                            Order = 1,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Body =
                                        File.Exists(
                                            @"Migrations/Html/Secondary_And_Primary_School_Applications_And_Offers/Section1.html")
                                            ? File.ReadAllText(
                                                @"Migrations/Html/Secondary_And_Primary_School_Applications_And_Offers/Section1.html",
                                                Encoding.UTF8)
                                            : ""
                                },
                            }
                        },
                        new ContentSection
                        {
                            Heading = "2. The admissions process",
                            Caption = "",
                            Order = 2,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Body =
                                        File.Exists(
                                            @"Migrations/Html/Secondary_And_Primary_School_Applications_And_Offers/Section2.html")
                                            ? File.ReadAllText(
                                                @"Migrations/Html/Secondary_And_Primary_School_Applications_And_Offers/Section2.html",
                                                Encoding.UTF8)
                                            : ""
                                },
                            }
                        },
                        new ContentSection
                        {
                            Heading = "3. Methodology",
                            Caption = "",
                            Order = 3,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Body =
                                        File.Exists(
                                            @"Migrations/Html/Secondary_And_Primary_School_Applications_And_Offers/Section3.html")
                                            ? File.ReadAllText(
                                                @"Migrations/Html/Secondary_And_Primary_School_Applications_And_Offers/Section3.html",
                                                Encoding.UTF8)
                                            : ""
                                },
                            }
                        },
                        new ContentSection
                        {
                            Heading = "4. Contacts",
                            Caption = "",
                            Order = 4,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Body =
                                        File.Exists(
                                            @"Migrations/Html/Secondary_And_Primary_School_Applications_And_Offers/Section4.html")
                                            ? File.ReadAllText(
                                                @"Migrations/Html/Secondary_And_Primary_School_Applications_And_Offers/Section4.html",
                                                Encoding.UTF8)
                                            : ""
                                },
                            }
                        },
                    },
                },
                new Methodology
                {
                    Id = new Guid("c8c911e3-39c1-452b-801f-25bb79d1deb7"),
                    Title = "Pupil exclusion statistics: methodology",
                    Published = new DateTime(2018, 8, 25),
                    Summary = "",
                    PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                    Content = new List<ContentSection>
                    {
                        new ContentSection
                        {
                            Heading = "1 Overview of exclusion statistics",
                            Caption = "",
                            Order = 1,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Body = File.Exists(@"Migrations/Html/Pupil_Exclusion_Statistics/Section1.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Exclusion_Statistics/Section1.html",
                                            Encoding.UTF8)
                                        : ""
                                },
                            }
                        },
                        new ContentSection
                        {
                            Heading = "2. National Statistics badging",
                            Caption = "",
                            Order = 2,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Body = File.Exists(@"Migrations/Html/Pupil_Exclusion_Statistics/Section2.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Exclusion_Statistics/Section2.html",
                                            Encoding.UTF8)
                                        : ""
                                },
                            }
                        },
                        new ContentSection
                        {
                            Heading = "3. Methodology",
                            Caption = "",
                            Order = 3,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Body = File.Exists(@"Migrations/Html/Pupil_Exclusion_Statistics/Section3.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Exclusion_Statistics/Section3.html",
                                            Encoding.UTF8)
                                        : ""
                                },
                            }
                        },
                        new ContentSection
                        {
                            Heading = "4. Data collection",
                            Caption = "",
                            Order = 4,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Body = File.Exists(@"Migrations/Html/Pupil_Exclusion_Statistics/Section4.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Exclusion_Statistics/Section4.html",
                                            Encoding.UTF8)
                                        : ""
                                },
                            }
                        },
                        new ContentSection
                        {
                            Heading = "5. Data processing",
                            Caption = "",
                            Order = 5,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Body = File.Exists(@"Migrations/Html/Pupil_Exclusion_Statistics/Section5.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Exclusion_Statistics/Section5.html",
                                            Encoding.UTF8)
                                        : ""
                                },
                            }
                        },
                        new ContentSection
                        {
                            Heading = "6. Data quality",
                            Caption = "",
                            Order = 6,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Body = File.Exists(@"Migrations/Html/Pupil_Exclusion_Statistics/Section6.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Exclusion_Statistics/Section6.html",
                                            Encoding.UTF8)
                                        : ""
                                },
                            }
                        },
                        new ContentSection
                        {
                            Heading = "7. Contacts",
                            Caption = "",
                            Order = 7,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Body = File.Exists(@"Migrations/Html/Pupil_Exclusion_Statistics/Section7.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Exclusion_Statistics/Section7.html",
                                            Encoding.UTF8)
                                        : ""
                                },
                            }
                        },
                    },
                    Annexes = new List<ContentSection>
                    {
                        new ContentSection
                        {
                            Heading = "Annex A - Calculations",
                            Caption = "",
                            Order = 1,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Body = File.Exists(@"Migrations/Html/Pupil_Exclusion_Statistics/AnnexA.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Exclusion_Statistics/AnnexA.html",
                                            Encoding.UTF8)
                                        : ""
                                },
                            }
                        },
                        new ContentSection
                        {
                            Heading = "Annex B - Exclusion by reason codes",
                            Caption = "",
                            Order = 2,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Body = File.Exists(@"Migrations/Html/Pupil_Exclusion_Statistics/AnnexB.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Exclusion_Statistics/AnnexB.html",
                                            Encoding.UTF8)
                                        : ""
                                },
                            }
                        },
                        new ContentSection
                        {
                            Heading = "Annex C - Links to pupil exclusions statistics and data",
                            Caption = "",
                            Order = 3,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Body = File.Exists(@"Migrations/Html/Pupil_Exclusion_Statistics/AnnexC.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Exclusion_Statistics/AnnexC.html",
                                            Encoding.UTF8)
                                        : ""
                                },
                            }
                        },
                        new ContentSection
                        {
                            Heading = "Annex D - Standard breakdowns",
                            Caption = "",
                            Order = 4,
                            Content = new List<IContentBlock>
                            {
                                new HtmlBlock
                                {
                                    Body = File.Exists(@"Migrations/Html/Pupil_Exclusion_Statistics/AnnexD.html")
                                        ? File.ReadAllText(@"Migrations/Html/Pupil_Exclusion_Statistics/AnnexD.html",
                                            Encoding.UTF8)
                                        : ""
                                },
                            }
                        },
                    }
                }
            );
        }
    }
}