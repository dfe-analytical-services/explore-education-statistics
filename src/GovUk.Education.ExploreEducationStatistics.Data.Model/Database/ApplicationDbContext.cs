using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Converters;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            Database.SetCommandTimeout(int.MaxValue);
        }

        public DbSet<Filter> Filter { get; set; }
        public DbSet<FilterItem> FilterItem { get; set; }
        public DbSet<FilterGroup> FilterGroup { get; set; }
        public DbSet<Footnote> Footnote { get; set; }
        public DbQuery<GeoJson> GeoJson { get; set; }
        public DbSet<Indicator> Indicator { get; set; }
        public DbSet<IndicatorGroup> IndicatorGroup { get; set; }
        public DbSet<Location> Location { get; set; }
        public DbSet<Observation> Observation { get; set; }
        public DbSet<ObservationFilterItem> ObservationFilterItem { get; set; }
        public DbSet<Publication> Publication { get; set; }
        public DbSet<Release> Release { get; set; }
        public DbSet<School> School { get; set; }
        public DbSet<Subject> Subject { get; set; }
        public DbSet<SubjectFootnote> SubjectFootnote { get; set; }
        public DbSet<Theme> Theme { get; set; }
        public DbSet<Topic> Topic { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureAdditionalTypes(modelBuilder);
            ConfigureData(modelBuilder);
            ConfigureGeographicLevel(modelBuilder);
            ConfigureGeoJson(modelBuilder);
            ConfigureLocation(modelBuilder);
            ConfigureMeasures(modelBuilder);
            ConfigureObservationFilterItem(modelBuilder);
            ConfigurePublication(modelBuilder);
            ConfigureSubjectFootnote(modelBuilder);
            ConfigureTimePeriod(modelBuilder);
            ConfigureUnit(modelBuilder);
        }

        private static void ConfigureData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Theme>().HasData(new List<Theme>
            {
                new Theme
                {
                    Id = new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f"),
                    Title = "Pupils and schools",
                    Slug = "pupils-and-schools"
                },
                new Theme
                {
                    Id = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"),
                    Title = "Children, early years and social care",
                    Slug = "children-and-early-years"
                },
                new Theme
                {
                    Id = new Guid("9aa81762-e52c-40d4-8a90-f469977360a7"),
                    Title = "Further education",
                    Slug = "further-education"
                },
                new Theme
                {
                    Id = new Guid("fe805471-17e9-4ac6-a555-c7d0ebec1b90"),
                    Title = "School and college outcomes and performance",
                    Slug = "outcomes-and-performance"
                }
            });

            modelBuilder.Entity<Topic>().HasData(new List<Topic>
            {
                new Topic
                {
                    Id = new Guid("67c249de-1cca-446e-8ccb-dcdac542f460"),
                    Title = "Pupil absence",
                    Slug = "pupil-absence",
                    ThemeId = new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f")
                },
                new Topic
                {
                    Id = new Guid("77941b7d-bbd6-4069-9107-565af89e2dec"),
                    Title = "Exclusions",
                    Slug = "exclusions",
                    ThemeId = new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f")
                },
                new Topic
                {
                    Id = new Guid("1a9636e4-29d5-4c90-8c07-f41db8dd019c"),
                    Title = "School applications",
                    Slug = "school-applications",
                    ThemeId = new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f")
                },
                new Topic
                {
                    Id = new Guid("17b2e32c-ed2f-4896-852b-513cdf466769"),
                    Title = "Early years foundation stage profile",
                    Slug = "early-years-foundation-stage-profile",
                    ThemeId = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53")
                },
                new Topic
                {
                    Id = new Guid("dfc908db-242a-4e3a-b6c6-e3f66cd152af"),
                    Title = "Special educational needs (SEN)",
                    Slug = "sen",
                    ThemeId = new Guid("ee1855ca-d1e1-4f04-a795-cbd61d326a1f")
                },
                new Topic
                {
                    Id = new Guid("721048b9-8c06-4bad-8585-8789fa38a03b"),
                    Title = "National achievement rates tables",
                    Slug = "national-achievement-rates-tables",
                    ThemeId = new Guid("9aa81762-e52c-40d4-8a90-f469977360a7")
                },
                new Topic
                {
                    Id = new Guid("71444ff6-614f-405b-b6c7-f72077d42e34"),
                    Title = "Further education and skills",
                    Slug = "further-education-and-skills",
                    ThemeId = new Guid("9aa81762-e52c-40d4-8a90-f469977360a7")
                },
                new Topic
                {
                    Id = new Guid("9e4fa097-2999-4c4d-9ecd-0c4733fc71b4"),
                    Title = "16 to 19 attainment",
                    Slug = "sixteen-to-nineteen-attainment",
                    ThemeId = new Guid("fe805471-17e9-4ac6-a555-c7d0ebec1b90")
                },
                new Topic
                {
                    Id = new Guid("f38469bd-a5f7-46b1-96bb-3b0a01e9e53f"),
                    Title = "Key stage 2",
                    Slug = "key-stage-two",
                    ThemeId = new Guid("fe805471-17e9-4ac6-a555-c7d0ebec1b90")
                },
                new Topic
                {
                    Id = new Guid("81fbb21d-3c49-46a2-8b43-0076974114f7"),
                    Title = "GCSEs (key stage 4)",
                    Slug = "key-stage-four",
                    ThemeId = new Guid("fe805471-17e9-4ac6-a555-c7d0ebec1b90")
                }
            });

            modelBuilder.Entity<Publication>().HasData(new List<Publication>
            {
                new Publication
                {
                    Id = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"),
                    Title = "Pupil absence in schools in England",
                    Slug = "pupil-absence-in-schools-in-england",
                    TopicId = new Guid("67c249de-1cca-446e-8ccb-dcdac542f460")
                },
                new Publication
                {
                    Id = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"),
                    Title = "Permanent and fixed-period exclusions in England",
                    Slug = "permanent-and-fixed-period-exclusions-in-england",
                    TopicId = new Guid("77941b7d-bbd6-4069-9107-565af89e2dec")
                },
                new Publication
                {
                    Id = new Guid("66c8e9db-8bf2-4b0b-b094-cfab25c20b05"),
                    Title = "Secondary and primary schools applications and offers",
                    Slug = "secondary-and-primary-schools-applications-and-offers",
                    TopicId = new Guid("1a9636e4-29d5-4c90-8c07-f41db8dd019c")
                },
                new Publication
                {
                    Id = new Guid("fcda2962-82a6-4052-afa2-ea398c53c85f"),
                    Title = "Early years foundation stage profile results",
                    Slug = "early-years-foundation-stage-profile-results",
                    TopicId = new Guid("17b2e32c-ed2f-4896-852b-513cdf466769")
                },
                new Publication
                {
                    Id = new Guid("2d94e5c8-a272-497c-bda0-c1f6b75155b0"),
                    Title = "Statements of SEN and EHC plans",
                    Slug = "statements-of-sen-and-ehc-plans",
                    TopicId = new Guid("dfc908db-242a-4e3a-b6c6-e3f66cd152af")
                },
                new Publication
                {
                    Id = new Guid("99ce35fb-3fe2-48bb-9b73-23159df9d5ea"),
                    Title = "National achievement rates tables",
                    Slug = "national-achievement-rates-tables",
                    TopicId = new Guid("721048b9-8c06-4bad-8585-8789fa38a03b")
                },
                new Publication
                {
                    Id = new Guid("5aea252e-fddc-42b3-a1da-47f12e523e70"),
                    Title = "Apprenticeships and traineeships",
                    Slug = "apprenticeships-and-traineeships",
                    TopicId = new Guid("71444ff6-614f-405b-b6c7-f72077d42e34")
                },
                new Publication
                {
                    Id = new Guid("d5a01a5c-cd57-482f-8a19-803b266e1012"),
                    Title = "Further education and skills",
                    Slug = "further-education-and-skills",
                    TopicId = new Guid("71444ff6-614f-405b-b6c7-f72077d42e34")
                },
                new Publication
                {
                    Id = new Guid("adb95888-64c7-4aa7-ba70-a9e535f8a30f"),
                    Title = "Level 2 and 3 attainment by young people aged 19",
                    Slug = "Level 2 and 3 attainment by young people aged 19",
                    TopicId = new Guid("9e4fa097-2999-4c4d-9ecd-0c4733fc71b4")
                },
                new Publication
                {
                    Id = new Guid("90ecb3d3-bd05-4f84-a73e-1d153568b320"),
                    Title = "National curriculum assessments at key stage 2",
                    Slug = "national-curriculum-assessments-key-stage2",
                    TopicId = new Guid("f38469bd-a5f7-46b1-96bb-3b0a01e9e53f")
                },
                new Publication
                {
                    Id = new Guid("15659c96-a624-4457-846d-2ab5f3db6aec"),
                    Title = "GCSE and equivalent results, including pupil characteristics",
                    Slug = "gcse-results-including-pupil-characteristics",
                    TopicId = new Guid("81fbb21d-3c49-46a2-8b43-0076974114f7")
                }
            });

            modelBuilder.Entity<Release>().HasData(new List<Release>
            {
                new Release
                {
                    Id = new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"),
                    ReleaseDate = new DateTime(2018, 4, 25),
                    Title = "2016 to 2017",
                    Slug = "2016-17",
                    PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad")
                },
                new Release
                {
                    Id = new Guid("47299b78-a4a6-4f7e-a86f-4713f4a0599a"),
                    ReleaseDate = new DateTime(2019, 5, 20),
                    Title = "2017 to 2018",
                    Slug = "2017-18",
                    PublicationId = new Guid("fcda2962-82a6-4052-afa2-ea398c53c85f")
                },
                new Release
                {
                    Id = new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278"),
                    ReleaseDate = new DateTime(2018, 7, 19),
                    Title = "2016 to 2017",
                    Slug = "2016-17",
                    PublicationId = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9")
                },
                new Release
                {
                    Id = new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717"),
                    ReleaseDate = new DateTime(2019, 4, 29),
                    Title = "2018",
                    Slug = "2018",
                    PublicationId = new Guid("66c8e9db-8bf2-4b0b-b094-cfab25c20b05")
                },
                new Release
                {
                    Id = new Guid("70efdb76-7e88-453f-95f1-7bb9af023db5"),
                    ReleaseDate = new DateTime(2019, 4, 29),
                    Title = "2018",
                    Slug = "2018",
                    PublicationId = new Guid("2d94e5c8-a272-497c-bda0-c1f6b75155b0")
                },
                new Release
                {
                    Id = new Guid("59258583-b075-47a2-bee4-5969e2d58873"),
                    ReleaseDate = new DateTime(2019, 4, 29),
                    Title = "2018",
                    Slug = "2018",
                    PublicationId = new Guid("99ce35fb-3fe2-48bb-9b73-23159df9d5ea")
                },
                new Release
                {
                    Id = new Guid("463c8521-d9b4-4ccc-aee9-0666e39c8e47"),
                    ReleaseDate = new DateTime(2019, 4, 29),
                    Title = "2018",
                    Slug = "2018",
                    PublicationId = new Guid("5aea252e-fddc-42b3-a1da-47f12e523e70")
                },
                new Release
                {
                    Id = new Guid("6ccc4416-7d22-46bf-a12a-56037831dc60"),
                    ReleaseDate = new DateTime(2019, 4, 29),
                    Title = "2018",
                    Slug = "2018",
                    PublicationId = new Guid("d5a01a5c-cd57-482f-8a19-803b266e1012")
                },
                new Release
                {
                    Id = new Guid("0dafd89b-b754-44a8-b3f1-72baac0a108a"),
                    ReleaseDate = new DateTime(2019, 4, 29),
                    Title = "2018",
                    Slug = "2018",
                    PublicationId = new Guid("adb95888-64c7-4aa7-ba70-a9e535f8a30f")
                },
                new Release
                {
                    Id = new Guid("dbaeb363-33fa-4928-870f-5054278e0c9a"),
                    ReleaseDate = new DateTime(2019, 4, 29),
                    Title = "2018",
                    Slug = "2018",
                    PublicationId = new Guid("90ecb3d3-bd05-4f84-a73e-1d153568b320")
                },
                new Release
                {
                    Id = new Guid("737dbab8-4e62-4d56-b0d6-5b4602a20801"),
                    ReleaseDate = new DateTime(2019, 4, 29),
                    Title = "2018",
                    Slug = "2018",
                    PublicationId = new Guid("15659c96-a624-4457-846d-2ab5f3db6aec")
                }
            });

            modelBuilder.Entity<Subject>().HasData(new List<Subject>
            {
                new Subject
                {
                    Id = 1,
                    Name = "Absence by characteristic",
                    ReleaseId = new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5")
                },
                new Subject
                {
                    Id = 2,
                    Name = "Absence by geographic level",
                    ReleaseId = new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5")
                },
                new Subject
                {
                    Id = 3,
                    Name = "Absence by term",
                    ReleaseId = new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5")
                },
                new Subject
                {
                    Id = 4,
                    Name = "Absence for four year olds",
                    ReleaseId = new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5")
                },
                new Subject
                {
                    Id = 5,
                    Name = "Absence in prus",
                    ReleaseId = new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5")
                },
                new Subject
                {
                    Id = 6,
                    Name = "Absence number missing at least one session by reason",
                    ReleaseId = new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5")
                },
                new Subject
                {
                    Id = 7,
                    Name = "Absence rate percent bands",
                    ReleaseId = new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5")
                },
                new Subject
                {
                    Id = 8,
                    Name = "ELG underlying data 2013 - 2018",
                    ReleaseId = new Guid("47299b78-a4a6-4f7e-a86f-4713f4a0599a")
                },
                new Subject
                {
                    Id = 9,
                    Name = "Areas of learning underlying data 2013 - 2018",
                    ReleaseId = new Guid("47299b78-a4a6-4f7e-a86f-4713f4a0599a")
                },
                new Subject
                {
                    Id = 10,
                    Name = "APS GLD ELG underlying data 2013 - 2018",
                    ReleaseId = new Guid("47299b78-a4a6-4f7e-a86f-4713f4a0599a")
                },
                new Subject
                {
                    Id = 11,
                    Name = "Exclusions by characteristic",
                    ReleaseId = new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278")
                },
                new Subject
                {
                    Id = 12,
                    Name = "Exclusions by geographic level",
                    ReleaseId = new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278")
                },
                new Subject
                {
                    Id = 13,
                    Name = "Exclusions by reason",
                    ReleaseId = new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278")
                },
                new Subject
                {
                    Id = 14,
                    Name = "Duration of fixed exclusions",
                    ReleaseId = new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278")
                },
                new Subject
                {
                    Id = 15,
                    Name = "Number of fixed exclusions",
                    ReleaseId = new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278")
                },
                new Subject
                {
                    Id = 16,
                    Name = "Total days missed due to fixed period exclusions",
                    ReleaseId = new Guid("e7774a74-1f62-4b76-b9b5-84f14dac7278")
                },
                new Subject
                {
                    Id = 17,
                    Name = "Applications and offers by school phase",
                    ReleaseId = new Guid("63227211-7cb3-408c-b5c2-40d3d7cb2717")
                },
                new Subject
                {
                    Id = 18,
                    Name = "New cases by age",
                    ReleaseId = new Guid("70efdb76-7e88-453f-95f1-7bb9af023db5")
                },
                new Subject
                {
                    Id = 19,
                    Name = "Stock cases by age",
                    ReleaseId = new Guid("70efdb76-7e88-453f-95f1-7bb9af023db5")
                },
                new Subject
                {
                    Id = 20,
                    Name = "New cases by establishment",
                    ReleaseId = new Guid("70efdb76-7e88-453f-95f1-7bb9af023db5")
                },
                new Subject
                {
                    Id = 21,
                    Name = "Stock cases by establishment",
                    ReleaseId = new Guid("70efdb76-7e88-453f-95f1-7bb9af023db5")
                },
                new Subject
                {
                    Id = 22,
                    Name = "Management information",
                    ReleaseId = new Guid("70efdb76-7e88-453f-95f1-7bb9af023db5")
                },
                new Subject
                {
                    Id = 23,
                    Name = "National achievement rates tables (NARTs)",
                    ReleaseId = new Guid("59258583-b075-47a2-bee4-5969e2d58873")
                },
                new Subject
                {
                    Id = 24,
                    Name = "Apprenticeship annual",
                    ReleaseId = new Guid("463c8521-d9b4-4ccc-aee9-0666e39c8e47")
                },
                new Subject
                {
                    Id = 25,
                    Name = "Further education and skills",
                    ReleaseId = new Guid("6ccc4416-7d22-46bf-a12a-56037831dc60")
                },
                new Subject
                {
                    Id = 26,
                    Name = "Level 2 and 3 National",
                    ReleaseId = new Guid("0dafd89b-b754-44a8-b3f1-72baac0a108a")
                },
                new Subject
                {
                    Id = 27,
                    Name = "Level 2 and 3 sf",
                    ReleaseId = new Guid("0dafd89b-b754-44a8-b3f1-72baac0a108a")
                },
                new Subject
                {
                    Id = 28,
                    Name = "Level 2 and 3 sf by Local authority",
                    ReleaseId = new Guid("0dafd89b-b754-44a8-b3f1-72baac0a108a")
                },
                new Subject
                {
                    Id = 29,
                    Name = "2016 test data",
                    ReleaseId = new Guid("dbaeb363-33fa-4928-870f-5054278e0c9a")
                },
                new Subject
                {
                    Id = 30,
                    Name = "Characteristic test data by Local authority",
                    ReleaseId = new Guid("737dbab8-4e62-4d56-b0d6-5b4602a20801")
                },
                new Subject
                {
                    Id = 31,
                    Name = "National characteristic test data",
                    ReleaseId = new Guid("737dbab8-4e62-4d56-b0d6-5b4602a20801")
                },
                new Subject
                {
                    Id = 32,
                    Name = "Subject tables S1 test data",
                    ReleaseId = new Guid("737dbab8-4e62-4d56-b0d6-5b4602a20801")
                },
                new Subject
                {
                    Id = 33,
                    Name = "Subject tables S3 test data",
                    ReleaseId = new Guid("737dbab8-4e62-4d56-b0d6-5b4602a20801")
                }
            });
        }

        private static void ConfigureLocation(ModelBuilder modelBuilder)
        {
            ConfigureCountry(modelBuilder);
            ConfigureInstitution(modelBuilder);
            ConfigureLocalAuthority(modelBuilder);
            ConfigureLocalAuthorityDistrict(modelBuilder);
            ConfigureLocalEnterprisePartnership(modelBuilder);
            ConfigureMultiAcademyTrust(modelBuilder);
            ConfigureMayoralCombinedAuthority(modelBuilder);
            ConfigureOpportunityArea(modelBuilder);
            ConfigureParliamentaryConstituency(modelBuilder);
            ConfigureRegion(modelBuilder);
            ConfigureRscRegion(modelBuilder);
            ConfigureSponsor(modelBuilder);
            ConfigureWard(modelBuilder);
        }

        private static void ConfigureObservationFilterItem(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ObservationFilterItem>()
                .HasKey(item => new {item.ObservationId, item.FilterItemId});

            modelBuilder.Entity<ObservationFilterItem>()
                .HasOne(observationFilterItem => observationFilterItem.Observation)
                .WithMany(observation => observation.FilterItems)
                .HasForeignKey(observationFilterItem => observationFilterItem.ObservationId);

            modelBuilder.Entity<ObservationFilterItem>()
                .HasOne(observationFilterItem => observationFilterItem.FilterItem)
                .WithMany()
                .HasForeignKey(observationFilterItem => observationFilterItem.FilterItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private static void ConfigureUnit(ModelBuilder modelBuilder)
        {
            var unitConverter = new EnumToEnumValueConverter<Unit>();

            modelBuilder.Entity<Indicator>()
                .Property(indicator => indicator.Unit)
                .HasConversion(unitConverter);
        }

        private static void ConfigurePublication(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Release>()
                .HasIndex(data => data.PublicationId);
        }

        private static void ConfigureMeasures(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Observation>()
                .Property(data => data.Measures)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Dictionary<long, string>>(v));
        }

        private static void ConfigureTimePeriod(ModelBuilder modelBuilder)
        {
            var timeIdentifierConverter = new EnumToEnumValueConverter<TimeIdentifier>();

            modelBuilder.Entity<Observation>()
                .Property(observation => observation.TimeIdentifier)
                .HasConversion(timeIdentifierConverter)
                .HasMaxLength(6);

            modelBuilder.Entity<Observation>()
                .HasIndex(observation => observation.Year);

            modelBuilder.Entity<Observation>()
                .HasIndex(observation => observation.TimeIdentifier);
        }

        private static void ConfigureGeographicLevel(ModelBuilder modelBuilder)
        {
            var geographicLevelConverter = new EnumToEnumValueConverter<GeographicLevel>();

            modelBuilder.Entity<Observation>()
                .Property(observation => observation.GeographicLevel)
                .HasConversion(geographicLevelConverter)
                .HasMaxLength(6);

            modelBuilder.Entity<Observation>()
                .HasIndex(observation => observation.GeographicLevel);
        }

        private static void ConfigureCountry(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Location>()
                .OwnsOne(level => level.Country,
                    builder => builder.HasIndex(country => country.Code));
        }

        private static void ConfigureLocalAuthority(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Location>()
                .OwnsOne(level => level.LocalAuthority,
                    builder => builder.HasIndex(localAuthority => localAuthority.Code));
        }

        private static void ConfigureLocalAuthorityDistrict(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Location>()
                .OwnsOne(level => level.LocalAuthorityDistrict,
                    builder => builder.HasIndex(localAuthorityDistrict => localAuthorityDistrict.Code));
        }

        private static void ConfigureInstitution(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Location>()
                .OwnsOne(level => level.Institution,
                    builder => builder.HasIndex(institution => institution.Code));
        }

        private static void ConfigureLocalEnterprisePartnership(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Location>()
                .OwnsOne(level => level.LocalEnterprisePartnership,
                    builder => builder.HasIndex(localEnterprisePartnership => localEnterprisePartnership.Code));
        }

        private static void ConfigureMultiAcademyTrust(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Location>()
                .OwnsOne(level => level.MultiAcademyTrust,
                    builder => builder.HasIndex(mat => mat.Code));
        }

        private static void ConfigureMayoralCombinedAuthority(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Location>()
                .OwnsOne(level => level.MayoralCombinedAuthority,
                    builder => builder.HasIndex(mca => mca.Code));
        }

        private static void ConfigureOpportunityArea(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Location>()
                .OwnsOne(level => level.OpportunityArea,
                    builder => builder.HasIndex(opportunityArea => opportunityArea.Code));
        }

        private static void ConfigureParliamentaryConstituency(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Location>()
                .OwnsOne(level => level.ParliamentaryConstituency,
                    builder => builder.HasIndex(parliamentaryConstituency => parliamentaryConstituency.Code));
        }

        private static void ConfigureRegion(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Location>()
                .OwnsOne(level => level.Region,
                    builder => builder.HasIndex(region => region.Code));
        }

        private static void ConfigureRscRegion(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Location>()
                .OwnsOne(level => level.RscRegion,
                    builder => builder.HasIndex(rscRegion => rscRegion.Code));
        }

        private static void ConfigureSponsor(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Location>()
                .OwnsOne(level => level.Sponsor,
                    builder => builder.HasIndex(sponsor => sponsor.Code));
        }

        private static void ConfigureSubjectFootnote(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SubjectFootnote>()
                .HasKey(item => new {item.SubjectId, item.FootnoteId});

            modelBuilder.Entity<SubjectFootnote>()
                .HasOne(subjectFootnote => subjectFootnote.Subject)
                .WithMany(subject => subject.Footnotes)
                .HasForeignKey(subjectFootnote => subjectFootnote.SubjectId);

            modelBuilder.Entity<SubjectFootnote>()
                .HasOne(subjectFootnote => subjectFootnote.Footnote)
                .WithMany()
                .HasForeignKey(subjectFootnote => subjectFootnote.FootnoteId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private static void ConfigureWard(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Location>()
                .OwnsOne(level => level.Ward,
                    builder => builder.HasIndex(ward => ward.Code));
        }

        private static void ConfigureGeoJson(ModelBuilder modelBuilder)
        {
            var geographicLevelConverter = new EnumToEnumValueConverter<GeographicLevel>();

            modelBuilder.Query<GeoJson>().ToView("geojson")
                .Property(geoJson => geoJson.GeographicLevel)
                .HasConversion(geographicLevelConverter);
        }

        private static void ConfigureAdditionalTypes(ModelBuilder modelBuilder)
        {
            // Register types used by custom SQL queries
            modelBuilder.Query<IdWrapper>();
        }
    }
}