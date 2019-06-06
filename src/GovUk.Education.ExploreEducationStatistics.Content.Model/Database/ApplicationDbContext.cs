using System;
using System.Collections.Generic;
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
                  Summary = "Including children in need, EYFS, and looked after children and social workforce statistics",
                  Slug = "children-and-early-years"
              },
              new Theme
              {
                  Id = new Guid("6412a76c-cf15-424f-8ebc-3a530132b1b3"),
                  Title = "Destination of pupils and students",
                  Summary = "Including graduate labour market and not in education, employment or training (NEET) statistics",
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
                  Summary = "Including advanced learner loan, benefit claimant and apprenticeship and traineeship statistics",
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
                  Summary = "Including absence, application and offers, capacity exclusion and special educational needs (SEN) statistics",
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
                  Summary = "Including summarised expenditure, post-compulsory education, qualitification and school statistics",
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

            modelBuilder.Entity<Publication>().HasData(
              new Publication
              {
                  Id = new Guid("d63daa75-5c3e-48bf-a232-f232e0d13898"),
                  Title = "30 hours free childcare",
                  Summary = "",
                  TopicId = new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"),
                  Slug = "30-hours-free-childcare",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-childcare-and-early-years#30-hours-free-childcare")
              },
              new Publication
              {
                  Id = new Guid("79a08466-dace-4ff0-94b6-59c5528c9262"),
                  Title = "Childcare and early years provider survey",
                  Summary = "",
                  TopicId = new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"),
                  Slug = "childcare-and-early-years-provider-survey",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-childcare-and-early-years#childcare-and-early-years-providers-survey")
              },
              new Publication
              {
                  Id = new Guid("060c5376-35d8-420b-8266-517a9339b7bc"),
                  Title = "Childcare and early years survey of parents",
                  Summary = "",
                  TopicId = new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"),
                  Slug = "childcare-and-early-years-survey-of-parents",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-childcare-and-early-years#childcare-and-early-years-providers-survey")
              },
              new Publication
              {
                  Id = new Guid("0ce6a6c6-5451-4967-8dd4-2f4fa8131982"),
                  Title = "Education provision: children under 5 years of age",
                  Summary = "",
                  TopicId = new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"),
                  Slug = "education-provision-children-under-5",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-childcare-and-early-years#provision-for-children-under-5-years-of-age-in-england")
              },
              new Publication
              {
                  Id = new Guid("89869bba-0c00-40f7-b7d6-e28cb904ad37"),
                  Title = "Characteristics of children in need",
                  Summary = "",
                  TopicId = new Guid("22c52d89-88c0-44b5-96c4-042f1bde6ddd"),
                  Slug = "characteristics-of-children-in-need",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-children-in-need#characteristics-of-children-in-need")
              },
              new Publication
              {
                  Id = new Guid("d8baee79-3c88-45f4-b12a-07b91e9b5c11"),
                  Title = "Children's social work workforce",
                  Summary = "",
                  TopicId = new Guid("734820b7-f80e-45c3-bb92-960edcc6faa5"),
                  Slug = "childrens-social-work-workforce",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-childrens-social-care-workforce#statutory-collection")
              },
              new Publication
              {
                  Id = new Guid("fcda2962-82a6-4052-afa2-ea398c53c85f"),
                  Title = "Early years foundation stage profile results",
                  Summary = "",
                  TopicId = new Guid("17b2e32c-ed2f-4896-852b-513cdf466769"),
                  Slug = "early-years-foundation-stage-profile-results",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-early-years-foundation-stage-profile#results-at-national-and-local-authority-level")
              },
              new Publication
              {
                  Id = new Guid("3260801d-601a-48c6-93b7-cf51680323d1"),
                  Title = "Children looked after in England including adoptions",
                  Summary = "",
                  TopicId = new Guid("66ff5e67-36cf-4210-9ad2-632baeb4eca7"),
                  Slug = "children-looked-after-in-england-including-adoptions",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-looked-after-children#looked-after-children")
              },
              new Publication
              {
                  Id = new Guid("f51895df-c682-45e6-b23e-3138ddbfdaeb"),
                  Title = "Outcomes for children looked after by LAs",
                  Summary = "",
                  TopicId = new Guid("66ff5e67-36cf-4210-9ad2-632baeb4eca7"),
                  Slug = "outcomes-for-children-looked-after-by-las",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-looked-after-children#outcomes-for-looked-after-children")
              },
              new Publication
              {
                  Id = new Guid("d7bd5d9d-dc65-4b1d-99b1-4d815b7369a3"),
                  Title = "Children accommodated in secure children's homes",
                  Summary = "",
                  TopicId = new Guid("d5288137-e703-43a1-b634-d50fc9785cb9"),
                  Slug = "children-accommodated-in-secure-childrens-homes",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-secure-children-s-homes")
              },
              new Publication
              {
                  Id = new Guid("8a92c6a5-8110-4c9c-87b1-e15f1c80c66a"),
                  Title = "Destinations of key stage 4 and key stage 5 pupils",
                  Summary = "",
                  TopicId = new Guid("0b920c62-ff67-4cf1-89ec-0c74a364e6b4"),
                  Slug = "destinations-of-ks4-and-ks5-pupils",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-destinations#destinations-after-key-stage-4-and-5")
              },
              new Publication
              {
                  Id = new Guid("42a888c4-9ee7-40fd-9128-f5de546780b3"),
                  Title = "Graduate labour market statistics",
                  Summary = "",
                  TopicId = new Guid("3bef5b2b-76a1-4be1-83b1-a3269245c610"),
                  Slug = "graduate-labour-markets",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/graduate-labour-market-quarterly-statistics#documents")
              },
              new Publication
              {
                  Id = new Guid("a0eb117e-44a8-4732-adf1-8fbc890cbb62"),
                  Title = "Participation in education and training and employment",
                  Summary = "",
                  TopicId = new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860"),
                  Slug = "participation-in-education-training-and-employement",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-neet#participation-in-education")
              },
              new Publication
              {
                  Id = new Guid("2e510281-ca8c-41bf-bbe0-fd15fcc81aae"),
                  Title = "NEET statistics quarterly brief",
                  Summary = "",
                  TopicId = new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860"),
                  Slug = "neet-statistics-quarterly-brief",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-neet#neet:-2016-to-2017-data-")
              },
              new Publication
              {
                  Id = new Guid("8ab47806-e36f-4226-9988-1efe23156872"),
                  Title = "Income and expenditure in academies in England",
                  Summary = "",
                  TopicId = new Guid("4c658598-450b-4493-b972-8812acd154a7"),
                  Slug = "income-and-expenditure-in-academies-in-england",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-local-authority-school-finance-data#academy-spending")
              },
              new Publication
              {
                  Id = new Guid("dcb8b32b-4e50-4fe2-a539-58f9b6b3a366"),
                  Title = "LA and school expenditure",
                  Summary = "",
                  TopicId = new Guid("4c658598-450b-4493-b972-8812acd154a7"),
                  Slug = "la-and-school-expenditure",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-local-authority-school-finance-data#local-authority-and-school-finance")
              },
              new Publication
              {
                  Id = new Guid("94d16c6e-1e5f-48d5-8195-8ea770f1b0d4"),
                  Title = "Planned LA and school expenditure",
                  Summary = "",
                  TopicId = new Guid("4c658598-450b-4493-b972-8812acd154a7"),
                  Slug = "planned-la-and-school-expenditure",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-local-authority-school-finance-data#planned-local-authority-and-school-spending-")
              },
              new Publication
              {
                  Id = new Guid("fd68e147-b7ee-464f-8b02-dcd917dc362d"),
                  Title = "Student loan forecasts for England",
                  Summary = "",
                  TopicId = new Guid("5c5bc908-f813-46e2-aae8-494804a57aa1"),
                  Slug = "student-loan-forecasts-for-england",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-student-loan-forecasts#documents")
              },
              new Publication
              {
                  Id = new Guid("75568912-25ba-499a-8a96-6161b54994db"),
                  Title = "Advanced learner loans applications",
                  Summary = "",
                  TopicId = new Guid("ba0e4a29-92ef-450c-97c5-80a0a6144fb5"),
                  Slug = "advanced-learner-loans-applications",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/further-education#advanced-learner-loans-applications-2017-to-2018")
              },
              new Publication
              {
                  Id = new Guid("f00a784b-52e8-475b-b8ee-dbe730382ba8"),
                  Title = "FE chioces employer satisfaction survey",
                  Summary = "",
                  TopicId = new Guid("dd4a5d02-fcc9-4b7f-8c20-c153754ba1e4"),
                  Slug = "fe-choices-employer-satisfaction-survey",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/fe-choices#employer-satisfaction-survey-data")
              },
              new Publication
              {
                  Id = new Guid("657b1484-0369-4a0e-873a-367b79a48c35"),
                  Title = "FE choices learner satisfaction survey",
                  Summary = "",
                  TopicId = new Guid("dd4a5d02-fcc9-4b7f-8c20-c153754ba1e4"),
                  Slug = "fe-choices-learner-satisfaction-survey",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/fe-choices#learner-satisfaction-survey-data")
              },
              new Publication
              {
                  Id = new Guid("d24783b6-24a7-4ef3-8304-fd07eeedff92"),
                  Title = "Apprenticeship and levy statistics",
                  Summary = "",
                  TopicId = new Guid("88d08425-fcfd-4c87-89da-70b2062a7367"),
                  Slug = "apprenticeship-and-levy-statistics",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/further-education-and-skills-statistical-first-release-sfr#apprenticeships-and-levy---older-data")
              },
              new Publication
              {
                  Id = new Guid("cf0ec981-3583-42a5-b21b-3f2f32008f1b"),
                  Title = "Apprenticeships and traineeships",
                  Summary = "",
                  TopicId = new Guid("88d08425-fcfd-4c87-89da-70b2062a7367"),
                  Slug = "apprenticeships-and-traineeships",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/further-education-and-skills-statistical-first-release-sfr#apprenticeships-and-traineeships---older-data")
              },
              new Publication
              {
                  Id = new Guid("13b81bcb-e8cd-4431-9807-ca588fd1d02a"),
                  Title = "Further education and skills",
                  Summary = "",
                  TopicId = new Guid("88d08425-fcfd-4c87-89da-70b2062a7367"),
                  Slug = "further-education-and-skills",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/further-education-and-skills-statistical-first-release-sfr#fe-and-skills---older-data")
              },
              new Publication
              {
                  Id = new Guid("ce6098a6-27b6-44b5-8e63-36df3a659e69"),
                  Title = "Further education and benefits claimants",
                  Summary = "",
                  TopicId = new Guid("cf1f1dc5-27c2-4d15-a55a-9363b7757ff3"),
                  Slug = "further-education-and-benefits-claimants",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/further-education-for-benefit-claimants#documents")
              },
              new Publication
              {
                  Id = new Guid("7a57d4c0-5233-4d46-8e27-748fbc365715"),
                  Title = "National achievement rates tables",
                  Summary = "",
                  TopicId = new Guid("dc7b7a89-e968-4a7e-af5f-bd7d19c346a5"),
                  Slug = "national-achievement-rates-tables",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/sfa-national-success-rates-tables#national-achievement-rates-tables")
              },
              new Publication
              {
                  Id = new Guid("4d29c28c-efd1-4245-a80c-b55c6a50e3f7"),
                  Title = "Graduate outcomes (LEO)",
                  Summary = "",
                  TopicId = new Guid("53a1fbb7-5234-435f-892b-9baad4c82535"),
                  Slug = "graduate-outcomes",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-higher-education-graduate-employment-and-earnings#documents")
              },
              new Publication
              {
                  Id = new Guid("d4b9551b-d92c-4f98-8731-847780d3c9fa"),
                  Title = "Higher education: destinations of leavers",
                  Summary = "",
                  TopicId = new Guid("2458a916-df6e-4845-9658-a81eace42ffd"),
                  Slug = "higher-education-destinations-of-leavers",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/official-statistics-releases#destinations-of-higher-education-leavers")
              },
              new Publication
              {
                  Id = new Guid("14cfd218-5480-4ba1-a051-5b1e6be14b46"),
                  Title = "Higher education enrolments and qualifications",
                  Summary = "",
                  TopicId = new Guid("2458a916-df6e-4845-9658-a81eace42ffd"),
                  Slug = "higher-education-enrolments-and-qualifications",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/official-statistics-releases#higher-education-enrolments-and-qualifications")
              },
              new Publication
              {
                  Id = new Guid("b83f55db-73fc-46fc-9fda-9b59f5896e9d"),
                  Title = "Performance indicators in higher education",
                  Summary = "",
                  TopicId = new Guid("2458a916-df6e-4845-9658-a81eace42ffd"),
                  Slug = "performance-indicators-in-higher-education",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/official-statistics-releases#performance-indicators")
              },
              new Publication
              {
                  Id = new Guid("6c25a3e9-fc96-472f-895c-9ae4492dd2a4"),
                  Title = "Staff at higher education providers in the UK",
                  Summary = "",
                  TopicId = new Guid("2458a916-df6e-4845-9658-a81eace42ffd"),
                  Slug = "staff-at-higher-education-providers-in-the-uk",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/official-statistics-releases#staff-at-higher-education")
              },
              new Publication
              {
                  Id = new Guid("0c67bbdb-4eb0-41cf-a62e-2589cee58538"),
                  Title = "Participation rates in higher education",
                  Summary = "",
                  TopicId = new Guid("04d95654-9fe0-4f78-9dfd-cf396661ebe9"),
                  Slug = "participation-rates-in-higher-education",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-on-higher-education-initial-participation-rates#participation-rates-in-higher-education-for-england")
              },
              new Publication
              {
                  Id = new Guid("c28f7aca-f1e8-4916-8ce3-fc177b140695"),
                  Title = "Widening participation in higher education",
                  Summary = "",
                  TopicId = new Guid("7871f559-0cfe-47c0-b48d-25b2bc8a0418"),
                  Slug = "widening-participation-in-higher-education",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/widening-participation-in-higher-education#documents")
              },
              new Publication
              {
                  Id = new Guid("123461ab-50be-45d9-8523-c5241a2c9c5b"),
                  Title = "Admission appeals in England",
                  Summary = "",
                  TopicId = new Guid("c9f0b897-d58a-42b0-9d12-ca874cc7c810"),
                  Slug = "admission-appeals-in-england",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-admission-appeals#documents")
              },
              new Publication
              {
                  Id = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                  Title = "Permanent and fixed-period exclusions in England",
                  Summary = "",
                  TopicId = new Guid("77941b7d-bbd6-4069-9107-565af89e2dec"),
                  Slug = "permanent-and-fixed-period-exclusions-in-england"
              },
              new Publication
              {
                  Id = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                  Title = "Pupil absence in schools in England",
                  Summary = "",
                  TopicId = new Guid("67c249de-1cca-446e-8ccb-dcdac542f460"),
                  Slug = "pupil-absence-in-schools-in-england",
                  NextUpdate = new DateTime(2018, 3, 22),
                  DataSource =
                    "[Pupil absence statistics: guide](https://www.gov.uk/government/publications/absence-statistics-guide#)"
              },
              new Publication
              {
                  Id = new Guid("6c388293-d027-4f74-8d74-29a42e02231c"),
                  Title = "Pupil absence in schools in England: autumn term",
                  Summary = "",
                  TopicId = new Guid("67c249de-1cca-446e-8ccb-dcdac542f460"),
                  Slug = "pupil-absence-in-schools-in-england-autumn-term",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-pupil-absence#autumn-term-release")
              },
              new Publication
              {
                  Id = new Guid("14953fda-02ff-45ed-9573-3a7a0ad8cb10"),
                  Title = "Pupil absence in schools in England: autumn and spring",
                  Summary = "",
                  TopicId = new Guid("67c249de-1cca-446e-8ccb-dcdac542f460"),
                  Slug = "pupil-absence-in-schools-in-england-autumn-and-spring",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-pupil-absence#combined-autumn--and-spring-term-release")
              },
              new Publication
              {
                  Id = new Guid("86af24dc-67c4-47f0-a849-e94c7a1cfe9b"),
                  Title = "Parental responsibility measures",
                  Summary = "",
                  TopicId = new Guid("6b8c0242-68e2-420c-910c-e19524e09cd2"),
                  Slug = "parental-responsibility-measures",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/parental-responsibility-measures#official-statistics")
              },
              new Publication
              {
                  Id = new Guid("aa545525-9ffe-496c-a5b3-974ace56746e"),
                  Title = "National pupil projections",
                  Summary = "",
                  TopicId = new Guid("5e196d11-8ac4-4c82-8c46-a10a67c1118e"),
                  Slug = "national-pupil-projections",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-pupil-projections#documents")
              },
              new Publication
              {
                  Id = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                  Title = "School and pupils and their characteristics",
                  Summary = "",
                  TopicId = new Guid("e50ba9fd-9f19-458c-aceb-4422f0c7d1ba"),
                  Slug = "school-pupils-and-their-characteristics"
              },
              new Publication
              {
                  Id = new Guid("66c8e9db-8bf2-4b0b-b094-cfab25c20b05"),
                  Title = "Secondary and primary schools applications and offers",
                  Summary = "",
                  TopicId = new Guid("1a9636e4-29d5-4c90-8c07-f41db8dd019c"),
                  Slug = "secondary-and-primary-schools-applications-and-offers",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-school-applications#documents")
              },
              new Publication
              {
                  Id = new Guid("fa591a15-ae37-41b5-98f6-4ce06e5225f4"),
                  Title = "School capacity",
                  Summary = "",
                  TopicId = new Guid("87c27c5e-ae49-4932-aedd-4405177d9367"),
                  Slug = "school-capacity",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-school-capacity#school-capacity-data:-by-academic-year")
              },
              new Publication
              {
                  Id = new Guid("f657afb4-8f4a-427d-a683-15f11a2aefb5"),
                  Title = "Special educational needs in England",
                  Summary = "",
                  TopicId = new Guid("85349b0a-19c7-4089-a56b-ad8dbe85449a"),
                  Slug = "special-educational-needs-in-england",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-special-educational-needs-sen#national-statistics-on-special-educational-needs-in-england")
              },
              new Publication
              {
                  Id = new Guid("30874b87-483a-427e-8916-43cf9020d9a1"),
                  Title = "Special educational needs: analysis and summary of data sources",
                  Summary = "",
                  TopicId = new Guid("85349b0a-19c7-4089-a56b-ad8dbe85449a"),
                  Slug = "special-educational-needs-analysis-and-summary-of-data-sources",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-special-educational-needs-sen#analysis-of-children-with-special-educational-needs")
              },
              new Publication
              {
                  Id = new Guid("88312cc0-fe1d-4ab5-81df-33fd708185cb"),
                  Title = "Statements on SEN and EHC plans",
                  Summary = "",
                  TopicId = new Guid("85349b0a-19c7-4089-a56b-ad8dbe85449a"),
                  Slug = "statements-on-sen-and-ehc-plans",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-special-educational-needs-sen#statements-of-special-educational-needs-(sen)-and-education,-health-and-care-(ehc)-plans")
              },
              new Publication
              {
                  Id = new Guid("1b2fb05c-eb2c-486b-80be-ebd772eda4f1"),
                  Title = "16 to 18 school and college performance tables",
                  Summary = "",
                  TopicId = new Guid("85b5454b-3761-43b1-8e84-bd056a8efcd3"),
                  Slug = "16-to-18-school-and-college-performance-tables",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-attainment-at-19-years#16-to-18-school-and-college-performance-tables")
              },
              new Publication
              {
                  Id = new Guid("3f3a66ec-5777-42ee-b427-8102a14ce0c5"),
                  Title = "A level and other 16 to 18 results",
                  Summary = "",
                  TopicId = new Guid("85b5454b-3761-43b1-8e84-bd056a8efcd3"),
                  Slug = "a-level-and-other-16-to-18-results",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-attainment-at-19-years#a-levels-and-other-16-to-18-results")
              },
              new Publication
              {
                  Id = new Guid("2e95f880-629c-417b-981f-0901e97776ff"),
                  Title = "Level 2 and 3 attainment by young people aged 19",
                  Summary = "",
                  TopicId = new Guid("85b5454b-3761-43b1-8e84-bd056a8efcd3"),
                  Slug = "level-2-and-3-attainment-by-young-people-aged-19",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-attainment-at-19-years#level-2-and-3-attainment")
              },
              new Publication
              {
                  Id = new Guid("bfdcaae1-ce6b-4f63-9b2b-0a1f3942887f"),
                  Title = "GCSE and equivalent results",
                  Summary = "",
                  TopicId = new Guid("1e763f55-bf09-4497-b838-7c5b054ba87b"),
                  Slug = "gcse-and-equivalent-results"
              },
              new Publication
              {
                  Id = new Guid("1d0e4263-3d70-433e-bd95-f29754db5888"),
                  Title = "Multi-academy trust performance measures",
                  Summary = "",
                  TopicId = new Guid("1e763f55-bf09-4497-b838-7c5b054ba87b"),
                  Slug = "multi-academy-trust-performance-measures",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-gcses-key-stage-4#multi-academy-trust-performance-measures")
              },
              new Publication
              {
                  Id = new Guid("c8756008-ed50-4632-9b96-01b5ca002a43"),
                  Title = "Revised GCSE and equivalent results in England",
                  Summary = "",
                  TopicId = new Guid("1e763f55-bf09-4497-b838-7c5b054ba87b"),
                  Slug = "revised-gcse-and-equivalent-results-in-england",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-gcses-key-stage-4#gcse-and-equivalent-results,-including-pupil-characteristics")
              },
              new Publication
              {
                  Id = new Guid("9e7e9d5c-b761-43a4-9685-4892392200b7"),
                  Title = "Secondary school performance tables",
                  Summary = "",
                  TopicId = new Guid("1e763f55-bf09-4497-b838-7c5b054ba87b"),
                  Slug = "secondary-school-performance-tables",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-gcses-key-stage-4#secondary-school-performance-tables")
              },
              new Publication
              {
                  Id = new Guid("441a13f6-877c-4f18-828f-119dbd401a5b"),
                  Title = "Phonics screening check and key stage 1 assessments",
                  Summary = "",
                  TopicId = new Guid("504446c2-ddb1-4d52-bdbc-4148c2c4c460"),
                  Slug = "phonics-screening-check-and-ks1-assessments",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-key-stage-1#phonics-screening-check-and-key-stage-1-assessment")
              },
              new Publication
              {
                  Id = new Guid("7ecea655-7b22-4832-b697-26e86769399a"),
                  Title = "Key stage 2 national curriculum test:review outcomes",
                  Summary = "",
                  TopicId = new Guid("eac38700-b968-4029-b8ac-0eb8e1356480"),
                  Slug = "ks2-national-curriculum-test-review-outcomes",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-key-stage-2#key-stage-2-national-curriculum-tests:-review-outcomes")
              },
              new Publication
              {
                  Id = new Guid("eab51107-4ef0-4926-8f8b-c8bd7f5a21d5"),
                  Title = "Multi-academy trust performance measures",
                  Summary = "",
                  TopicId = new Guid("eac38700-b968-4029-b8ac-0eb8e1356480"),
                  Slug = "multi-academy-trust-performance-measures",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-key-stage-2#national-curriculum-assessments-at-key-stage-2")
              },
              new Publication
              {
                  Id = new Guid("10370062-93b0-4dde-9097-5a56bf5b3064"),
                  Title = "National curriculum assessments at key stage 2",
                  Summary = "",
                  TopicId = new Guid("eac38700-b968-4029-b8ac-0eb8e1356480"),
                  Slug = "national-curriculum-assessments-at-ks2",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-key-stage-2#national-curriculum-assessments-at-key-stage-2")
              },
              new Publication
              {
                  Id = new Guid("2434335f-f8e1-41fb-8d6e-4a11bc62b14a"),
                  Title = "Primary school performance tables",
                  Summary = "",
                  TopicId = new Guid("eac38700-b968-4029-b8ac-0eb8e1356480"),
                  Slug = "primary-school-performance-tables",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-key-stage-2#primary-school-performance-tables")
              },
              new Publication
              {
                  Id = new Guid("8b12776b-3d36-4475-8115-00974d7de1d0"),
                  Title = "Further education outcome-based success measures",
                  Summary = "",
                  TopicId = new Guid("a7ce9542-20e6-401d-91f4-f832c9e58b12"),
                  Slug = "further-education-outcome-based-success-measures",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-outcome-based-success-measures#statistics")
              },
              new Publication
              {
                  Id = new Guid("bddcd4b8-db0d-446c-b6e9-03d4230c6927"),
                  Title = "Primary school performance tables",
                  Summary = "",
                  TopicId = new Guid("1318eb73-02a8-4e50-82a9-7e271176c4d1"),
                  Slug = "primary-school-performance-tables-2",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-performance-tables#primary-school-(key-stage-2)")
              },
              new Publication
              {
                  Id = new Guid("263e10d2-b9c3-4e90-a6aa-b52b86de1f5f"),
                  Title = "School and college performance tables",
                  Summary = "",
                  TopicId = new Guid("1318eb73-02a8-4e50-82a9-7e271176c4d1"),
                  Slug = "school-and-college-performance-tables",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-performance-tables#school-and-college:-post-16-(key-stage-5)")
              },
              new Publication
              {
                  Id = new Guid("28aabfd4-a3fb-45e1-bb34-21ca3b7d1aec"),
                  Title = "Secondary school performance tables",
                  Summary = "",
                  TopicId = new Guid("1318eb73-02a8-4e50-82a9-7e271176c4d1"),
                  Slug = "secondary-school-performance-tables",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-performance-tables#secondary-school-(key-stage-4)")
              },
              new Publication
              {
                  Id = new Guid("d34978d5-0317-46bc-9258-13412270ac4d"),
                  Title = "Initial teacher training performance profiles",
                  Summary = "",
                  TopicId = new Guid("0f8792d2-28b1-4537-a1b4-3e139fcf0ca7"),
                  Slug = "initial-teacher-training-performance-profiles",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-teacher-training#performance-data")
              },
              new Publication
              {
                  Id = new Guid("9cc08298-7370-499f-919a-7d203ba21415"),
                  Title = "Initial teacher training: trainee number census",
                  Summary = "",
                  TopicId = new Guid("0f8792d2-28b1-4537-a1b4-3e139fcf0ca7"),
                  Slug = "initial-teacher-training-trainee-number-census",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-teacher-training#census-data")
              },
              new Publication
              {
                  Id = new Guid("3ceb43d0-e705-4cb9-aeb9-cb8638fcbf3d"),
                  Title = "TSM and initial teacher training allocations",
                  Summary = "",
                  TopicId = new Guid("0f8792d2-28b1-4537-a1b4-3e139fcf0ca7"),
                  Slug = "tsm-and-initial-teacher-training-allocations",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-teacher-training#teacher-supply-model-and-itt-allocations")
              },
              new Publication
              {
                  Id = new Guid("b318967f-2931-472a-93f2-fbed1e181e6a"),
                  Title = "School workforce in England",
                  Summary = "",
                  TopicId = new Guid("28cfa002-83cb-4011-9ddd-859ec99e0aa0"),
                  Slug = "school-workforce-in-england",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-school-workforce#documents")
              },
              new Publication
              {
                  Id = new Guid("d0b47c96-d7de-4d80-9ff7-3bff135d2636"),
                  Title = "Teacher analysis compendium",
                  Summary = "",
                  TopicId = new Guid("6d434e17-7b76-425d-897d-c7b369b42e35"),
                  Slug = "teacher-analysis-compendium",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/teacher-workforce-statistics-and-analysis#documents")
              },
              new Publication
              {
                  Id = new Guid("2ffbc8d3-eb53-4c4b-a6fb-219a5b95ebc8"),
                  Title = "Education and training statistics for the UK",
                  Summary = "",
                  TopicId = new Guid("692050da-9ac9-435a-80d5-a6be4915f0f7"),
                  Slug = "education-and-training-statistics-for-the-uk",
                  LegacyPublicationUrl = new Uri("https://www.gov.uk/government/collections/statistics-education-and-training#documents")
              }
            ); ;

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
                        "Read national statistical summaries and definitions, view charts and tables and download data files across a range of pupil absence subject areas. \n\n",

                    KeyStatistics = new DataBlock
                    {
                        DataBlockRequest = new DataBlockRequest
                        {
                            subjectId = 1,
                            geographicLevel = "National",
                            startYear = "2016",
                            endYear = "2017",
                            filters = new List<string> { "1", "2" },
                            indicators = new List<string> { "23", "26", "28" }
                        },

                        Summary = new Summary
                        {
                            dataKeys = new List<string>
                            {
                                "23",
                                "26",
                                "28"
                            },

                            description = new MarkDownBlock
                            {
                                Body = " * pupils missed on average 8.2 school days \n " +
                                       " * overall and unauthorised absence rates up on previous year \n" +
                                       " * unauthorised rise due to higher rates of unauthorised holidays \n" +
                                       " * 10% of pupils persistently absent during 2016/17"
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
                                    DataBlockRequest = new DataBlockRequest
                                    {
                                        subjectId = 1,
                                        geographicLevel = "National",
                                        startYear = "2012",
                                        endYear = "2017",
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
                        "Read national statistical summaries and definitions, view charts and tables and download data files across a range of permanent and fixed-period exclusion subject areas. \n\n" +
                        "You can also view a regional breakdown of statistics and data within the [local authorities section](#contents-sections-heading-9)",
                    KeyStatistics = new DataBlock
                    {
                        Summary = new Summary
                        {
                            dataKeys = new List<string>
                            {
                                "perm_excl_rate",
                                "perm_excl",
                                "fixed_excl_rate"
                            },

                            description = new MarkDownBlock
                            {
                                Body =
                                    " * overall rate of permanent exclusions has increased from 0.08 per cent of pupil enrolments in 2015/16 to 0.10 per cent in 2016/17 \n" +
                                    " * number of exclusions has also increased, from 6,685 to 7,720 \n" +
                                    " * overall rate of fixed period exclusions increased, from 4.29 per cent of pupil enrolments in 2015/16 to 4.76 per cent in 2016/17 \n" +
                                    " * number of exclusions has also increased, from 339,360 to 381,865. \n"
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
                                new DataBlock
                                {
                                    Heading = "Chart showing permanent exclusions in England",
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
                                new DataBlock
                                {
                                    Heading = "Chart showing fixed-period exclusions in England",
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
                }
            );

            modelBuilder.Entity<Link>().HasData(
              new Link
              {
                  PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                  Id = new Guid("08134c1d-8a58-49a4-8d8b-22e586ffd5ae"),
                  Description = "2008 to 2009",
                  Url = "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-academic-year-2008-to-2009",
              },
              new Link
              {
                  PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                  Id = new Guid("250bace6-aeb9-4fe9-8de2-3a25e0dc717f"),
                  Description = "2009 to 2010",
                  Url = "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-from-schools-in-england-academic-year-2009-to-2010",
              },
              new Link
              {
                  PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                  Id = new Guid("a319e4ef-b957-40fb-8a47-b1a97814b220"),
                  Description = "2010 to 2011",
                  Url = "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-from-schools-in-england-academic-year-2010-to-2011",
              },
              new Link
              {
                  PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                  Id = new Guid("13acf54a-8016-49ff-9050-c61ebe7acad2"),
                  Description = "2011 to 2012",
                  Url = "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-from-schools-in-england-2011-to-2012-academic-year",
              },
              new Link
              {
                  PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                  Id = new Guid("45bd7f62-2018-4a5c-9b93-ccece8e89c46"),
                  Description = "2012 to 2013",
                  Url = "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2012-to-2013",
              },
              new Link
              {
                  PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                  Id = new Guid("f1225f98-40d5-494c-90f9-99f9fb59ac9d"),
                  Description = "2013 to 2014",
                  Url = "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2013-to-2014",
              },
              new Link
              {
                  PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                  Id = new Guid("78239816-507d-42b7-98fd-4a71d0d4eb1f"),
                  Description = "2014 to 2015",
                  Url = "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2014-to-2015",
              },
              new Link
              {
                  PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                  Id = new Guid("564dacdc-f58e-4aa0-8dbd-d8368b4fb6ba"),
                  Description = "2015 to 2016",
                  Url = "https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2015-to-2016",
              },
              new Link
              {
                  PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                  Id = new Guid("89c02688-646d-45b5-8919-9a3fafcfe0e9"),
                  Description = "2009 to 2010",
                  Url = "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2009-to-2010",
              },
              new Link
              {
                  PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                  Id = new Guid("81d91c86-9bf2-496c-b026-9dc255c35635"),
                  Description = "2010 to 2011",
                  Url = "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2010-to-2011",
              },
              new Link
              {
                  PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                  Id = new Guid("e20141d0-d894-4b8d-a78f-e41c23500786"),
                  Description = "2011 to 2012",
                  Url = "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics",
              },
              new Link
              {
                  PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                  Id = new Guid("ce15f487-87b0-4c07-98f1-6c6732196be7"),
                  Description = "2012 to 2013",
                  Url = "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2012-to-2013",
              },
              new Link
              {
                  PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                  Id = new Guid("75991639-ad77-4ba6-91fc-ac08c00a4ce8"),
                  Description = "2013 to 2014",
                  Url = "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2013-to-2014",
              },
              new Link
              {
                  PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                  Id = new Guid("28e53936-5a52-44be-a7a6-d2f14a426d28"),
                  Description = "2014 to 2015",
                  Url = "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2014-to-2015",
              },
              new Link
              {
                  PublicationId = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                  Id = new Guid("dc8b0d8c-08bb-47cc-b3a1-9e6ac9c2c268"),
                  Description = "January 2010",
                  Url = "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2010",
              },
              new Link
              {
                  PublicationId = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                  Id = new Guid("b086ba70-703c-40dd-aaef-d2e19335188e"),
                  Description = "January 2011",
                  Url = "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2011",
              },
              new Link
              {
                  PublicationId = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                  Id = new Guid("181ec43e-cf22-4cab-a128-0a5702468566"),
                  Description = "January 2012",
                  Url = "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2012",
              },
              new Link
              {
                  PublicationId = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                  Id = new Guid("e6b36ee8-ef66-4864-a4b3-9047ee3da338"),
                  Description = "January 2013",
                  Url = "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2013",
              },
              new Link
              {
                  PublicationId = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                  Id = new Guid("398ba8c6-3ea0-49da-8645-ceb3c7fb9860"),
                  Description = "January 2014",
                  Url = "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2014",
              },
              new Link
              {
                  PublicationId = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                  Id = new Guid("5e244416-6f2a-4d22-bea4-c22a229befef"),
                  Description = "January 2015",
                  Url = "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2015",
              },
              new Link
              {
                  PublicationId = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                  Id = new Guid("e3c1db23-8a8f-47fe-b2cd-8e677db700a2"),
                  Description = "January 2016",
                  Url = "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2016",
              },
              new Link
              {
                  PublicationId = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"),
                  Id = new Guid("313435b3-fe56-4b92-8e13-670dbf510062"),
                  Description = "January 2017",
                  Url = "https://www.gov.uk/government/statistics/schools-pupils-and-their-characteristics-january-2017",
              }
            );

            modelBuilder.Entity<Methodology>().HasData(
                new Methodology
                {
                    Id = new Guid("caa8e56f-41d2-4129-a5c3-53b051134bd7"),
                    Title = "Pupil absence statistics: methodology",
                    Published = new DateTime(2018, 3, 22),
                    Summary = "Find out about the methodology behind pupil absence statistics and data and how and why they're collected and published.",
                    PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                    Content = new List<ContentSection>
                    {
                        new ContentSection
                        {
                            Heading = "1. Overview of absence statistics",
                            Caption = "",
                            Order = 1,
                            Content = new List<IContentBlock>()
                        },
                        new ContentSection
                        {
                            Heading = "2. National Statistics badging",
                            Caption = "",
                            Order = 2,
                            Content = new List<IContentBlock>()
                        },
                        new ContentSection
                        {
                            Heading = "3. Methodology",
                            Caption = "",
                            Order = 3,
                            Content = new List<IContentBlock>()
                        },
                        new ContentSection
                        {
                            Heading = "4. Data collection",
                            Caption = "",
                            Order = 4,
                            Content = new List<IContentBlock>()
                        },
                        new ContentSection
                        {
                            Heading = "5. Data processing",
                            Caption = "",
                            Order = 5,
                            Content = new List<IContentBlock>()
                        },
                        new ContentSection
                        {
                            Heading = "6. Data quality",
                            Caption = "",
                            Order = 6,
                            Content = new List<IContentBlock>()
                        },
                        new ContentSection
                        {
                            Heading = "7. Contacts",
                            Caption = "",
                            Order = 7,
                            Content = new List<IContentBlock>()
                        }
                    },
                    Annexes = new List<ContentSection>
                    {
                        new ContentSection
                        {
                            Heading = "Annex A - Glossary",
                            Caption = "",
                            Order = 1,
                            Content = new List<IContentBlock>()
                        },
                        new ContentSection
                        {
                            Heading = "Annex B - Calculations",
                            Caption = "",
                            Order = 2,
                            Content = new List<IContentBlock>()
                        },
                        new ContentSection
                        {
                            Heading = "Annex C - School attendance codes",
                            Caption = "",
                            Order = 3,
                            Content = new List<IContentBlock>()
                        },
                        new ContentSection
                        {
                            Heading = "Annex D - Links to pupil absence national statistics and data",
                            Caption = "",
                            Order = 4,
                            Content = new List<IContentBlock>()
                        },
                        new ContentSection
                        {
                            Heading = "Annex E - Standard breakdowns",
                            Caption = "",
                            Order = 5,
                            Content = new List<IContentBlock>()
                        },
                        new ContentSection
                        {
                            Heading = "Annex F - Timeline",
                            Caption = "",
                            Order = 6,
                            Content = new List<IContentBlock>()
                        }
                    },
                }
            );
        }
    }
}