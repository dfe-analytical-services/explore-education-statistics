using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;

public class PublicDataDbContext : DbContext
{
    public PublicDataDbContext(DbContextOptions<PublicDataDbContext> options, bool updateTimestamps = true) : base(options)
    {
        Configure(updateTimestamps: updateTimestamps);
    }

    private void Configure(bool updateTimestamps = true)
    {
        if (updateTimestamps)
        {
            ChangeTracker.StateChanged += DbContextUtils.UpdateTimestamps;
            ChangeTracker.Tracked += DbContextUtils.UpdateTimestamps;
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DataSet>()
            .Property(ds => ds.Status)
            .HasConversion<string>();

        modelBuilder.Entity<DataSetVersion>()
            .OwnsOne(v => v.MetaSummary, ms =>
            {
                ms.ToJson();
                ms.OwnsOne(msb => msb.TimePeriodRange, msb =>
                {
                    msb.OwnsOne(tpr => tpr.Start, tpr =>
                    {
                        tpr.Property(tpm => tpm.Code).HasConversion<string>();
                    });
                    msb.OwnsOne(tpr => tpr.End, tpr =>
                    {
                        tpr.Property(tpm => tpm.Code).HasConversion<string>();
                    });
                });
            });

        modelBuilder.Entity<DataSetVersion>()
            .Property(dsv => dsv.Status)
            .HasConversion<string>();

        modelBuilder.Entity<DataSetMeta>()
            .OwnsMany(m => m.Filters, m =>
            {
                m.ToJson();
                m.OwnsMany(fm => fm.Options);
            })
            .OwnsMany(m => m.Indicators,
                m => m.ToJson())
            .OwnsMany(m => m.TimePeriods,
                m => m.ToJson())
            .OwnsMany(m => m.Locations, m =>
            {
                m.ToJson();
                m.OwnsMany(lm => lm.Options);
            });

        modelBuilder.Entity<DataSetMeta>()
            .Property(m => m.GeographicLevels)
            .HasColumnType("text[]");

        modelBuilder.Entity<ChangeSetFilters>()
            .OwnsMany(cs => cs.Changes, cs =>
            {
                cs.ToJson();
                cs.Property(c => c.Type).HasConversion<string>();
                cs.OwnsOne(c => c.CurrentState);
                cs.OwnsOne(c => c.PreviousState);
            });

        modelBuilder.Entity<ChangeSetFilterOptions>()
            .OwnsMany(cs => cs.Changes, cs =>
            {
                cs.ToJson();
                cs.Property(c => c.Type).HasConversion<string>();
                cs.OwnsOne(c => c.CurrentState);
                cs.OwnsOne(c => c.PreviousState);
            });

        modelBuilder.Entity<ChangeSetIndicators>()
            .OwnsMany(cs => cs.Changes, cs =>
            {
                cs.ToJson();
                cs.Property(c => c.Type).HasConversion<string>();
                cs.OwnsOne(c => c.CurrentState);
                cs.OwnsOne(c => c.PreviousState);
            });

        modelBuilder.Entity<ChangeSetLocations>()
            .OwnsMany(cs => cs.Changes, cs =>
            {
                cs.ToJson();
                cs.Property(c => c.Type).HasConversion<string>();
                cs.OwnsOne(c => c.CurrentState);
                cs.OwnsOne(c => c.PreviousState);
            });

        modelBuilder.Entity<ChangeSetTimePeriods>()
            .OwnsMany(cs => cs.Changes, cs =>
            {
                cs.ToJson();
                cs.Property(c => c.Type).HasConversion<string>();
                cs.OwnsOne(c => c.CurrentState);
                cs.OwnsOne(c => c.PreviousState);
            });
    }

    public DbSet<DataSet> DataSets { get; init; } = null!;
    public DbSet<DataSetVersion> DataSetVersions { get; init; } = null!;
    public DbSet<DataSetMeta> DataSetMeta { get; init; } = null!;
    public DbSet<ChangeSetFilters> ChangeSetFilters { get; init; } = null!;
    public DbSet<ChangeSetFilterOptions> ChangeSetFilterOptions { get; init; } = null!;
    public DbSet<ChangeSetIndicators> ChangeSetIndicators { get; init; } = null!;
    public DbSet<ChangeSetLocations> ChangeSetLocations { get; init; } = null!;
    public DbSet<ChangeSetTimePeriods> ChangeSetTimePeriods { get; init; } = null!;
}
