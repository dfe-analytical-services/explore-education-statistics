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
        public DbSet<Release> Release { get; set; }
        public DbSet<School> School { get; set; }
        public DbSet<Subject> Subject { get; set; }
        public DbSet<Theme> Theme { get; set; }
        public DbSet<Topic> Topic { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureGeoJson(modelBuilder);
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
            ConfigureInstitution(modelBuilder);
            ConfigureLocalEnterprisePartnership(modelBuilder);
            ConfigureMat(modelBuilder);
            ConfigureMayoralCombinedAuthority(modelBuilder);
            ConfigureOpportunityArea(modelBuilder);
            ConfigureParliamentaryConstituency(modelBuilder);
            ConfigureProvider(modelBuilder);
            ConfigureWard(modelBuilder);
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

        private static void ConfigureRegion(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Location>()
                .OwnsOne(level => level.Region,
                    builder => builder.HasIndex(region => region.Code));
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

        private static void ConfigureProvider(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Location>()
                .OwnsOne(level => level.Provider,
                    builder => builder.HasIndex(provider => provider.Code));
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