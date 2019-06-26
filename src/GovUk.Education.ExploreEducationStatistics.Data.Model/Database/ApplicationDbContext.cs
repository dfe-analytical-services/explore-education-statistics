using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
            ConfigureMat(modelBuilder);
            ConfigureMayoralCombinedAuthority(modelBuilder);
            ConfigureOpportunityArea(modelBuilder);
            ConfigureParliamentaryConstituency(modelBuilder);
            ConfigureRegion(modelBuilder);
            ConfigureRscRegion(modelBuilder);
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
            var timeIdentifierConverter = new EnumToStringConverter<TimeIdentifier>();

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

        private static void ConfigureMat(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Location>()
                .OwnsOne(level => level.Mat,
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