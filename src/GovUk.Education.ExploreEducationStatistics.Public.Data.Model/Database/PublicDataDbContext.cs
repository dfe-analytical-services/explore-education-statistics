using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;

public class PublicDataDbContext : DbContext
{
    public PublicDataDbContext(DbContextOptions<PublicDataDbContext> options) : base(options)
    {
    }

    public DbSet<DataSet> DataSets { get; init; } = null!;
    public DbSet<DataSetVersion> DataSetVersions { get; init; } = null!;
    public DbSet<DataSetMeta> DataSetMetas { get; init; } = null!;
}
