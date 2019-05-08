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
        public DbSet<IndicatorGroup> IndicatorGroup { get; set; }
        public DbSet<Indicator> Indicator { get; set; }
        public DbSet<Location> Location { get; set; }
        public DbSet<Observation> Observation { get; set; }
        public DbSet<ObservationFilterItem> ObservationFilterItem { get; set; }
        public DbSet<Release> Release { get; set; }
        public DbSet<School> School { get; set; }
        public DbSet<Subject> Subject { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureObservationFilterItem(modelBuilder);
            ConfigureUnit(modelBuilder);
            ConfigurePublication(modelBuilder);
            ConfigureMeasures(modelBuilder);
            ConfigureTimePeriod(modelBuilder);
            ConfigureGeographicLevel(modelBuilder);
            ConfigureCountry(modelBuilder);
            ConfigureLocalAuthority(modelBuilder);
            ConfigureLocalAuthorityDistrict(modelBuilder);
            ConfigureRegion(modelBuilder);
            ConfigureAdditionalTypes(modelBuilder);
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
            var unitConverter = new EnumToLabelConverter<Unit>();

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
                .HasConversion(timeIdentifierConverter);

            modelBuilder.Entity<Observation>()
                .HasIndex(observation => observation.Year);

            modelBuilder.Entity<Observation>()
                .HasIndex(observation => observation.TimeIdentifier);
        }

        private static void ConfigureGeographicLevel(ModelBuilder modelBuilder)
        {
            var geographicLevelConverter = new EnumToLabelConverter<GeographicLevel>();

            modelBuilder.Entity<Observation>()
                .Property(observation => observation.GeographicLevel)
                .HasConversion(geographicLevelConverter);
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

        private static void ConfigureRegion(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Location>()
                .OwnsOne(level => level.Region,
                    builder => builder.HasIndex(region => region.Code));
        }

        private static void ConfigureAdditionalTypes(ModelBuilder modelBuilder)
        {
            // Register types used by custom SQL queries
            modelBuilder.Query<IdWrapper>();
        }
    }
}