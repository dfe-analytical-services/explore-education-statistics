#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Thinktecture;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Database
{
    public class MatchedObservation
    {
        public Guid Id { get; }

        public MatchedObservation(Guid id)
        {
            Id = id;
        }
    }

    public class IdTempTable
    {
        public Guid Id { get; }

        public IdTempTable(Guid id)
        {
            Id = id;
        }

        protected bool Equals(IdTempTable other)
        {
            return Id.Equals(other.Id);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IdTempTable)obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public class StatisticsDbContext : DbContext
    {
        public StatisticsDbContext()
        {
            // We intentionally don't run `Configure` here as Moq would call this constructor
            // and we'd immediately get a MockException from interacting with its fields
            // e.g. from adding events listeners to `ChangeTracker`.
            // We can just rely on the variants which take options instead as these
            // are what get used in real application scenarios.
        }

        public StatisticsDbContext(DbContextOptions<StatisticsDbContext> options) : this(options, timeout: 300)
        {
        }

        public StatisticsDbContext(
            DbContextOptions<StatisticsDbContext> options,
            int? timeout,
            bool updateTimestamps = true) : base(options)
        {
            Configure(timeout, updateTimestamps);
        }

        /// <summary>
        /// This constructor is required to allow sub-classes to pass in
        /// DbContextOptions without a type parameter.
        /// We would otherwise need to set a type parameter at the class
        /// level and this doesn't really work with the DI container (unless
        /// we created an abstract base class with a type parameter).
        /// </summary>
        protected StatisticsDbContext(
            DbContextOptions options, 
            int? timeout, 
            bool updateTimestamps = true) : base(options)
        {
            Configure(timeout, updateTimestamps);
        }

        private void Configure(int? timeout = null, bool updateTimestamps = true)
        {
            if (updateTimestamps)
            {
                ChangeTracker.StateChanged += DbContextUtils.UpdateTimestamps;
                ChangeTracker.Tracked += DbContextUtils.UpdateTimestamps;
            }

            if (timeout.HasValue && Database.IsRelational())
            {
                Database.SetCommandTimeout(timeout);
            }
        }

        public DbSet<BoundaryLevel> BoundaryLevel { get; set; } = null!;
        public DbSet<Filter> Filter { get; set; } = null!;
        public DbSet<FilterFootnote> FilterFootnote { get; set; } = null!;
        public DbSet<FilterGroup> FilterGroup { get; set; } = null!;
        public DbSet<FilterGroupFootnote> FilterGroupFootnote { get; set; } = null!;
        public DbSet<FilterItem> FilterItem { get; set; } = null!;
        public DbSet<FilterItemFootnote> FilterItemFootnote { get; set; } = null!;
        public DbSet<Footnote> Footnote { get; set; } = null!;
        public DbSet<GeoJson> GeoJson { get; set; } = null!;
        public DbSet<Indicator> Indicator { get; set; } = null!;
        public DbSet<IndicatorFootnote> IndicatorFootnote { get; set; } = null!;
        public DbSet<IndicatorGroup> IndicatorGroup { get; set; } = null!;
        public DbSet<Location> Location { get; set; } = null!;
        public DbSet<Observation> Observation { get; set; } = null!;
        public DbSet<ObservationFilterItem> ObservationFilterItem { get; set; } = null!;
        public DbSet<Release> Release { get; set; } = null!;
        public DbSet<Subject> Subject { get; set; } = null!;
        public DbSet<SubjectFootnote> SubjectFootnote { get; set; } = null!;

        public DbSet<ReleaseSubject> ReleaseSubject { get; set; } = null!;
        public DbSet<ReleaseFootnote> ReleaseFootnote { get; set; } = null!;
        public DbSet<MatchedObservation> MatchedObservations => this.TempTableSet<MatchedObservation>();

        private readonly EnumToEnumValueConverter<GeographicLevel> _geographicLevelConverter = new();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureBoundaryLevel(modelBuilder);
            ConfigureIndicator(modelBuilder);
            ConfigureGeoJson(modelBuilder);
            ConfigureFilter(modelBuilder);
            ConfigureFilterFootnote(modelBuilder);
            ConfigureFilterGroupFootnote(modelBuilder);
            ConfigureFilterItemFootnote(modelBuilder);
            ConfigureIdTempTable(modelBuilder);
            ConfigureIndicatorFootnote(modelBuilder);
            ConfigureLocation(modelBuilder);
            ConfigureMeasures(modelBuilder);
            ConfigureObservation(modelBuilder);
            ConfigureObservationFilterItem(modelBuilder);
            ConfigureObservationRowResultTempTable(modelBuilder);
            ConfigurePublication(modelBuilder);
            ConfigureReleaseSubject(modelBuilder);
            ConfigureReleaseFootnote(modelBuilder);
            ConfigureSubject(modelBuilder);
            ConfigureSubjectFootnote(modelBuilder);
            ConfigureTimePeriod(modelBuilder);
            ConfigureUnit(modelBuilder);
        }

        private void ConfigureBoundaryLevel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BoundaryLevel>()
                .Property(boundaryLevel => boundaryLevel.Level)
                .HasConversion(_geographicLevelConverter);

            modelBuilder.Entity<BoundaryLevel>()
                .Property(block => block.Created)
                .HasConversion(
                    v => v,
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder.Entity<BoundaryLevel>()
                .Property(block => block.Published)
                .HasConversion(
                    v => v,
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        }

        private void ConfigureLocation(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Location>()
                .Property(location => location.GeographicLevel)
                .HasConversion(_geographicLevelConverter)
                .HasMaxLength(6);

            modelBuilder.Entity<Location>()
                .HasIndex(location => location.GeographicLevel);

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
                .HasIndex(location => location.Provider_Code);

            modelBuilder.Entity<Location>()
                .HasIndex(location => location.PlanningArea_Code);

            modelBuilder.Entity<Location>()
                .HasIndex(location => location.Region_Code);

            modelBuilder.Entity<Location>()
                .HasIndex(location => location.RscRegion_Code);

            modelBuilder.Entity<Location>()
                .HasIndex(location => location.School_Code);

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
                .HasKey(item => new { item.ObservationId, item.FilterItemId });

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

        private static void ConfigureObservationRowResultTempTable(ModelBuilder modelBuilder)
        {
            modelBuilder
                .ConfigureTempTableEntity<MatchedObservation>(isKeyless: false,
                    builder => builder
                        .HasKey(matchedObservation => matchedObservation.Id));
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

        private static void ConfigureReleaseSubject(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReleaseSubject>()
                .HasKey(item => new { item.ReleaseId, item.SubjectId });

            modelBuilder.Entity<ReleaseSubject>()
                .HasOne(rs => rs.Release)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReleaseSubject>()
                .HasOne(rs => rs.Subject)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReleaseSubject>()
                .Property(rs => rs.FilterSequence)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<FilterSequenceEntry>>(v));

            modelBuilder.Entity<ReleaseSubject>()
                .Property(rs => rs.IndicatorSequence)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<IndicatorGroupSequenceEntry>>(v));
        }

        private static void ConfigureReleaseFootnote(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReleaseFootnote>()
                .HasKey(item => new { item.ReleaseId, item.FootnoteId });

            modelBuilder.Entity<ReleaseFootnote>()
                .HasOne(rf => rf.Release)
                .WithMany()
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
                    v => JsonConvert.DeserializeObject<Dictionary<Guid, string>>(v)!);
        }

        private static void ConfigureTimePeriod(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Observation>()
                .Property(observation => observation.TimeIdentifier)
                .HasConversion(new EnumToEnumValueConverter<TimeIdentifier>())
                .HasMaxLength(6);

            modelBuilder.Entity<Observation>()
                .HasIndex(observation => observation.Year);

            modelBuilder.Entity<Observation>()
                .HasIndex(observation => observation.TimeIdentifier);
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
                .HasKey(item => new { item.FilterId, item.FootnoteId });

            modelBuilder.Entity<FilterFootnote>()
                .HasOne(filterFootnote => filterFootnote.Filter)
                .WithMany(filter => filter.Footnotes)
                .HasForeignKey(filterFootnote => filterFootnote.FilterId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FilterFootnote>()
                .HasOne(filterFootnote => filterFootnote.Footnote)
                .WithMany(footnote => footnote.Filters)
                .HasForeignKey(filterFootnote => filterFootnote.FootnoteId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigureFilterGroupFootnote(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FilterGroupFootnote>()
                .HasKey(item => new { item.FilterGroupId, item.FootnoteId });

            modelBuilder.Entity<FilterGroupFootnote>()
                .HasOne(filterGroupFootnote => filterGroupFootnote.FilterGroup)
                .WithMany(filterGroup => filterGroup.Footnotes)
                .HasForeignKey(filterGroupFootnote => filterGroupFootnote.FilterGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FilterGroupFootnote>()
                .HasOne(filterGroupFootnote => filterGroupFootnote.Footnote)
                .WithMany(footnote => footnote.FilterGroups)
                .HasForeignKey(filterGroupFootnote => filterGroupFootnote.FootnoteId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigureFilterItemFootnote(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FilterItemFootnote>()
                .HasKey(item => new { item.FilterItemId, item.FootnoteId });

            modelBuilder.Entity<FilterItemFootnote>()
                .HasOne(filterItemFootnote => filterItemFootnote.FilterItem)
                .WithMany(filterItem => filterItem.Footnotes)
                .HasForeignKey(filterItemFootnote => filterItemFootnote.FilterItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FilterItemFootnote>()
                .HasOne(filterItemFootnote => filterItemFootnote.Footnote)
                .WithMany(footnote => footnote.FilterItems)
                .HasForeignKey(filterItemFootnote => filterItemFootnote.FootnoteId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigureIdTempTable(ModelBuilder modelBuilder)
        {
            modelBuilder
                .ConfigureTempTableEntity<IdTempTable>(isKeyless: false,
                    builder => builder.HasKey(idTempTable => idTempTable.Id));
        }

        private static void ConfigureIndicatorFootnote(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IndicatorFootnote>()
                .HasKey(item => new { item.IndicatorId, item.FootnoteId });

            modelBuilder.Entity<IndicatorFootnote>()
                .HasOne(indicatorFootnote => indicatorFootnote.Indicator)
                .WithMany(indicator => indicator.Footnotes)
                .HasForeignKey(indicatorFootnote => indicatorFootnote.IndicatorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<IndicatorFootnote>()
                .HasOne(indicatorFootnote => indicatorFootnote.Footnote)
                .WithMany(footnote => footnote.Indicators)
                .HasForeignKey(indicatorFootnote => indicatorFootnote.FootnoteId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigureSubjectFootnote(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SubjectFootnote>()
                .HasKey(item => new { item.SubjectId, item.FootnoteId });

            modelBuilder.Entity<SubjectFootnote>()
                .HasOne(subjectFootnote => subjectFootnote.Subject)
                .WithMany(subject => subject.Footnotes)
                .HasForeignKey(subjectFootnote => subjectFootnote.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SubjectFootnote>()
                .HasOne(subjectFootnote => subjectFootnote.Footnote)
                .WithMany(footnote => footnote.Subjects)
                .HasForeignKey(subjectFootnote => subjectFootnote.FootnoteId)
                .OnDelete(DeleteBehavior.Cascade);
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
