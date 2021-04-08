using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Database
{
    public class StatisticsDbContext : DbContext
    {
        public StatisticsDbContext()
        {

        }

        public StatisticsDbContext(DbContextOptions<StatisticsDbContext> options) : this(options, int.MaxValue)
        {
        }

        public StatisticsDbContext(DbContextOptions<StatisticsDbContext> options, int? timeout) : base(options)
        {
            if (timeout.HasValue)
            {
                Database.SetCommandTimeout(timeout);
            }
        }

        public DbSet<BoundaryLevel> BoundaryLevel { get; set; }
        public DbSet<Filter> Filter { get; set; }
        public DbSet<FilterFootnote> FilterFootnote { get; set; }
        public DbSet<FilterGroup> FilterGroup { get; set; }
        public DbSet<FilterGroupFootnote> FilterGroupFootnote { get; set; }
        public DbSet<FilterItem> FilterItem { get; set; }
        public DbSet<FilterItemFootnote> FilterItemFootnote { get; set; }
        public DbSet<Footnote> Footnote { get; set; }
        public DbSet<GeoJson> GeoJson { get; set; }
        public DbSet<Indicator> Indicator { get; set; }
        public DbSet<IndicatorFootnote> IndicatorFootnote { get; set; }
        public DbSet<IndicatorGroup> IndicatorGroup { get; set; }
        public DbSet<Location> Location { get; set; }
        public DbSet<Observation> Observation { get; set; }
        public DbSet<ObservationFilterItem> ObservationFilterItem { get; set; }
        public DbSet<Publication> Publication { get; set; }
        public DbSet<Release> Release { get; set; }
        public DbSet<Subject> Subject { get; set; }
        public DbSet<SubjectFootnote> SubjectFootnote { get; set; }
        public DbSet<Theme> Theme { get; set; }
        public DbSet<Topic> Topic { get; set; }

        public DbSet<ReleaseSubject> ReleaseSubject { get; set; }
        public DbSet<ReleaseFootnote> ReleaseFootnote { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
            ConfigureObservation(modelBuilder);
            ConfigureObservationFilterItem(modelBuilder);
            ConfigurePublication(modelBuilder);
            ConfigureRelease(modelBuilder);
            ConfigureReleaseSubject(modelBuilder);
            ConfigureReleaseFootnote(modelBuilder);
            ConfigureSubjectFootnote(modelBuilder);
            ConfigureTimePeriod(modelBuilder);
            ConfigureUnit(modelBuilder);
            ConfigureSubject(modelBuilder);
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
            modelBuilder.Entity<Location>()
                .HasIndex(location => location.Country_Code);

            modelBuilder.Entity<Location>()
                .HasIndex(location => location.EnglishDevolvedArea_Code);

            modelBuilder.Entity<Location>()
                .HasIndex(location => location.Institution_Code);

            modelBuilder.Entity<Location>()
                .HasIndex(location => location.LocalAuthority_Code);

            modelBuilder.Entity<Location>()
                .HasIndex(location => location.LocalAuthority_OldCode);

            modelBuilder.Entity<Location>()
                .HasIndex(location => location.LocalAuthorityDistrict_Code);

            modelBuilder.Entity<Location>()
                .HasIndex(location => location.LocalEnterprisePartnership_Code);

            modelBuilder.Entity<Location>()
                .HasIndex(location => location.MultiAcademyTrust_Code);

            modelBuilder.Entity<Location>()
                .HasIndex(location => location.MayoralCombinedAuthority_Code);

            modelBuilder.Entity<Location>()
                .HasIndex(location => location.OpportunityArea_Code);

            modelBuilder.Entity<Location>()
                .HasIndex(location => location.ParliamentaryConstituency_Code);

            modelBuilder.Entity<Location>()
                .HasIndex(location => location.PlanningArea_Code);

            modelBuilder.Entity<Location>()
                .HasIndex(location => location.Region_Code);

            modelBuilder.Entity<Location>()
                .HasIndex(location => location.RscRegion_Code);

            modelBuilder.Entity<Location>()
                .HasIndex(location => location.Sponsor_Code);

            modelBuilder.Entity<Location>()
                .HasIndex(location => location.Ward_Code);
        }

        private static void ConfigureObservation(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Observation>()
                .HasOne(observation => observation.Location)
                .WithMany()
                .HasForeignKey(observation => observation.LocationId)
                .OnDelete(DeleteBehavior.Restrict);
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

            modelBuilder.Entity<ObservationFilterItem>()
                .HasOne(observationFilterItem => observationFilterItem.Filter)
                .WithMany()
                .HasForeignKey(observationFilterItem => observationFilterItem.FilterId)
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

        private static void ConfigureRelease(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Release>()
                .Property(r => r.TimeIdentifier)
                .HasConversion(new EnumToEnumValueConverter<TimeIdentifier>())
                .HasMaxLength(6);

            modelBuilder.Entity<Release>()
                .HasIndex(data => data.PreviousVersionId);
        }

        private static void ConfigureReleaseSubject(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReleaseSubject>()
                .HasKey(item => new {item.ReleaseId, item.SubjectId});

            modelBuilder.Entity<ReleaseSubject>()
                .HasOne(r => r.Release)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReleaseSubject>()
                .HasOne(r => r.Subject)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigureReleaseFootnote(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReleaseFootnote>()
                .HasKey(item => new {item.ReleaseId, item.FootnoteId});

            modelBuilder.Entity<ReleaseFootnote>()
                .HasOne(rf => rf.Release)
                .WithMany(release => release.Footnotes)
                .HasForeignKey(releaseFootnote => releaseFootnote.ReleaseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReleaseFootnote>()
                .HasOne(rf => rf.Footnote)
                .WithMany(footnote => footnote.Releases)
                .HasForeignKey(releaseFootnote => releaseFootnote.FootnoteId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigureMeasures(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Observation>()
                .Property(data => data.Measures)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Dictionary<Guid, string>>(v));
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
                .WithMany(footnote => footnote.Filters)
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
                .WithMany(footnote => footnote.FilterGroups)
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
                .WithMany(footnote => footnote.FilterItems)
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
                .WithMany(footnote => footnote.Indicators)
                .HasForeignKey(indicatorFootnote => indicatorFootnote.FootnoteId)
                .OnDelete(DeleteBehavior.Restrict);
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
                .WithMany(footnote => footnote.Subjects)
                .HasForeignKey(subjectFootnote => subjectFootnote.FootnoteId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private static void ConfigureSubject(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Subject>()
                .HasQueryFilter(r => !r.SoftDeleted);
        }

        private static void ConfigureGeoJson(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GeoJson>().HasNoKey().ToView("geojson");
        }
    }
}
