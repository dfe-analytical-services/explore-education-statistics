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

        public DbSet<BoundaryLevel> BoundaryLevel { get; set; }
        public DbSet<Filter> Filter { get; set; }
        public DbSet<FilterFootnote> FilterFootnote { get; set; }
        public DbSet<FilterGroup> FilterGroup { get; set; }
        public DbSet<FilterGroupFootnote> FilterGroupFootnote { get; set; }
        public DbSet<FilterItem> FilterItem { get; set; }
        public DbSet<FilterItemFootnote> FilterItemFootnote { get; set; }
        public DbSet<Footnote> Footnote { get; set; }
        public DbQuery<GeoJson> GeoJson { get; set; }
        public DbSet<Indicator> Indicator { get; set; }
        public DbSet<IndicatorFootnote> IndicatorFootnote { get; set; }
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
            modelBuilder.Entity<Observation>()
                .Property(o => o.Id).ForSqlServerUseSequenceHiLo();
            
            ConfigureAdditionalTypes(modelBuilder);
            ConfigureBoundaryLevel(modelBuilder);
            ConfigureIndicator(modelBuilder);
            ConfigureGeographicLevel(modelBuilder);
            ConfigureGeoJson(modelBuilder);
            ConfigureFilter(modelBuilder);
            ConfigureFilterFootnote(modelBuilder);
            ConfigureFilterGroupFootnote(modelBuilder);
            ConfigureFilterItemFootnote(modelBuilder);
            ConfigureIndicatorFootnote(modelBuilder);
            ConfigureLocation(modelBuilder);
            ConfigureMeasures(modelBuilder);
            ConfigureObservationFilterItem(modelBuilder);
            ConfigurePublication(modelBuilder);
            ConfigureSubjectFootnote(modelBuilder);
            ConfigureTimePeriod(modelBuilder);
            ConfigureUnit(modelBuilder);
        }

        private static void ConfigureBoundaryLevel(ModelBuilder modelBuilder)
        {
            var geographicLevelConverter = new EnumToEnumValueConverter<GeographicLevel>();

            modelBuilder.Entity<BoundaryLevel>()
                .Property(boundaryLevel => boundaryLevel.Level)
                .HasConversion(geographicLevelConverter);
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

        private static void ConfigureIndicator(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Indicator>()
                .HasIndex(indicator => indicator.Name);
        }

        private static void ConfigureFilter(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Filter>()
                .HasIndex(filter => filter.Name);
        }
        
        private static void ConfigureFilterFootnote(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FilterFootnote>()
                .HasKey(item => new {item.FilterId, item.FootnoteId});
            
            modelBuilder.Entity<FilterFootnote>()
                .HasOne(filterFootnote => filterFootnote.Filter)
                .WithMany(filter => filter.Footnotes)
                .HasForeignKey(filterFootnote => filterFootnote.FilterId);
            
            modelBuilder.Entity<FilterFootnote>()
                .HasOne(filterFootnote => filterFootnote.Footnote)
                .WithMany()
                .HasForeignKey(filterFootnote => filterFootnote.FootnoteId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        
        private static void ConfigureFilterGroupFootnote(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FilterGroupFootnote>()
                .HasKey(item => new {item.FilterGroupId, item.FootnoteId});
            
            modelBuilder.Entity<FilterGroupFootnote>()
                .HasOne(filterGroupFootnote => filterGroupFootnote.FilterGroup)
                .WithMany(filterGroup => filterGroup.Footnotes)
                .HasForeignKey(filterGroupFootnote => filterGroupFootnote.FilterGroupId);
            
            modelBuilder.Entity<FilterGroupFootnote>()
                .HasOne(filterGroupFootnote => filterGroupFootnote.Footnote)
                .WithMany()
                .HasForeignKey(filterGroupFootnote => filterGroupFootnote.FootnoteId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        
        private static void ConfigureFilterItemFootnote(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FilterItemFootnote>()
                .HasKey(item => new {item.FilterItemId, item.FootnoteId});
            
            modelBuilder.Entity<FilterItemFootnote>()
                .HasOne(filterItemFootnote => filterItemFootnote.FilterItem)
                .WithMany(filterItem => filterItem.Footnotes)
                .HasForeignKey(filterItemFootnote => filterItemFootnote.FilterItemId);
            
            modelBuilder.Entity<FilterItemFootnote>()
                .HasOne(filterItemFootnote => filterItemFootnote.Footnote)
                .WithMany()
                .HasForeignKey(filterItemFootnote => filterItemFootnote.FootnoteId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        
        private static void ConfigureIndicatorFootnote(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IndicatorFootnote>()
                .HasKey(item => new {item.IndicatorId, item.FootnoteId});

            modelBuilder.Entity<IndicatorFootnote>()
                .HasOne(indicatorFootnote => indicatorFootnote.Indicator)
                .WithMany(indicator => indicator.Footnotes)
                .HasForeignKey(indicatorFootnote => indicatorFootnote.IndicatorId);

            modelBuilder.Entity<IndicatorFootnote>()
                .HasOne(indicatorFootnote => indicatorFootnote.Footnote)
                .WithMany()
                .HasForeignKey(indicatorFootnote => indicatorFootnote.FootnoteId)
                .OnDelete(DeleteBehavior.Restrict);
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
            modelBuilder.Query<GeoJson>().ToView("geojson");
        }

        private static void ConfigureAdditionalTypes(ModelBuilder modelBuilder)
        {
            // Register types used by custom SQL queries
            modelBuilder.Query<IdWrapper>();
        }
    }
}