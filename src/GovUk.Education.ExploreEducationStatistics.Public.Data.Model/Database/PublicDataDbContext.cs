using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Reflection.Metadata;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;

public class PublicDataDbContext : DbContext
{

    public PublicDataDbContext(DbContextOptions<PublicDataDbContext> options) : base(options)
    {
        Configure(updateTimestamps: true);
    }

    public PublicDataDbContext(DbContextOptions<PublicDataDbContext> options, bool updateTimestamps = true) : base(options)
    {
        Configure(updateTimestamps: updateTimestamps);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DataSet>()
            .Property(d => d.Status)
            .HasConversion<string>();

        modelBuilder.Entity<DataSetVersion>()
            .OwnsOne(c => c.MetaSummary, m =>
            {
                m.ToJson();
                m.OwnsOne(m => m.TimePeriodRange, m =>
                {
                    m.ToJson();
                    m.OwnsOne(m => m.Start);
                    m.OwnsOne(m => m.End);
                });
            })
            .Property(v => v.Status)
            .HasConversion<string>();

        modelBuilder.Entity<DataSetMeta>()
            .OwnsMany(m => m.Filters, m =>
            {
                m.ToJson()
                .OwnsMany(fm => fm.Options);
            })
            .OwnsMany(m => m.Indicators, m => m.ToJson())
            .OwnsMany(m => m.TimePeriods, m => m.ToJson())
            .OwnsMany(m => m.Locations, m =>
            {
                m.ToJson()
                .OwnsMany(l => l.Options);
            });

        modelBuilder.Entity<DataSetChangeFilter>()
            .OwnsMany(d => d.Changes, o => o.ToJson());

        modelBuilder.Entity<DataSetChangeFilterOption>()
            .OwnsMany(d => d.Changes, o => o.ToJson());

        modelBuilder.Entity<DataSetChangeIndicator>()
            .OwnsMany(d => d.Changes, o => o.ToJson());

        modelBuilder.Entity<DataSetChangeLocation>()
            .OwnsMany(d => d.Changes, o => o.ToJson());

        modelBuilder.Entity<DataSetChangeTimePeriod>()
            .OwnsMany(d => d.Changes, o => o.ToJson());
    }

    private void Configure(bool updateTimestamps = true)
    {
        if (updateTimestamps)
        {
            ChangeTracker.StateChanged += DbContextUtils.UpdateTimestamps;
            ChangeTracker.Tracked += DbContextUtils.UpdateTimestamps;
        }
    }

    public DbSet<DataSet> DataSets { get; init; } = null!;
    public DbSet<DataSetVersion> DataSetVersions { get; init; } = null!;
    public DbSet<DataSetMeta> DataSetMeta { get; init; } = null!;
    public DbSet<DataSetChangeFilter> DataSetChangeFilters { get; init; } = null!;
    public DbSet<DataSetChangeFilterOption> DataSetChangeFilterOptions { get; init; } = null!;
    public DbSet<DataSetChangeIndicator> DataSetChangeIndicators { get; init; } = null!;
    public DbSet<DataSetChangeLocation> DataSetChangeLocations { get; init; } = null!;
    public DbSet<DataSetChangeTimePeriod> DataSetChangeTimePeriods { get; init; } = null!;
}
