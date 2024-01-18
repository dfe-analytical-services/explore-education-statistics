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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PublicDataDbContext).Assembly);
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
