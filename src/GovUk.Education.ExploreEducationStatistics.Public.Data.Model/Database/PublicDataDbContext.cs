using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;

public class PublicDataDbContext : DbContext
{

    public PublicDataDbContext(DbContextOptions<PublicDataDbContext> options) : base(options)
    {
    }

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

    public DbSet<DataSet> DataSets { get; init; } = null!;
    public DbSet<DataSetVersion> DataSetVersions { get; init; } = null!;
    public DbSet<DataSetMetaFilters> DataSetMetaFilters { get; init; } = null!;
    public DbSet<DataSetMetaIndicators> DataSetMetaIndicators { get; init; } = null!;
    public DbSet<DataSetMetaLocations> DataSetMetaLocations { get; init; } = null!;
    public DbSet<DataSetMetaTimePeriods> DataSetMetaTimePeriods { get; init; } = null!;
}
