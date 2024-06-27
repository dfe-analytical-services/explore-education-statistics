using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;

public class PublicDataDbContext : DbContext
{
    public const string FilterOptionMetaLinkSequence = "FilterOptionMetaLink_seq";
    public const string LocationOptionMetasIdSequence = "LocationOptionMetas_Id_seq";

    public PublicDataDbContext(
        DbContextOptions<PublicDataDbContext> options, bool updateTimestamps = true) : base(options)
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

        modelBuilder.HasSequence<int>(FilterOptionMetaLinkSequence);
    }

    [SuppressMessage("Security", "EF1002:Risk of vulnerability to SQL injection.")]
    public Task<int> NextSequenceValue(string sequenceName, CancellationToken cancellationToken = default) =>
        Database.SqlQueryRaw<int>($"""
                                   SELECT nextval('public."{sequenceName}"') AS "Value"
                                   """)
            .FirstAsync(cancellationToken);

    [SuppressMessage("Security", "EF1002:Risk of vulnerability to SQL injection.")]
    public Task<int> SetSequenceValue(string sequenceName, long value, CancellationToken cancellationToken = default) =>
        Database.SqlQueryRaw<int>($"""
                                   SELECT setval('public."{sequenceName}"', {value}) AS "Value"
                                   """)
            .FirstAsync(cancellationToken);

    public DbSet<DataSet> DataSets { get; init; } = null!;
    public DbSet<DataSetVersion> DataSetVersions { get; init; } = null!;
    public DbSet<DataSetVersionImport> DataSetVersionImports { get; init; } = null!;
    public DbSet<DataSetVersionMapping> DataSetVersionMappings { get; init; } = null!;
    public DbSet<GeographicLevelMeta> GeographicLevelMetas { get; init; } = null!;
    public DbSet<LocationMeta> LocationMetas { get; init; } = null!;
    public DbSet<LocationOptionMeta> LocationOptionMetas { get; init; } = null!;
    public DbSet<LocationOptionMetaLink> LocationOptionMetaLinks { get; init; } = null!;
    public DbSet<FilterMeta> FilterMetas { get; init; } = null!;
    public DbSet<FilterOptionMeta> FilterOptionMetas { get; init; } = null!;
    public DbSet<FilterOptionMetaLink> FilterOptionMetaLinks { get; init; } = null!;
    public DbSet<IndicatorMeta> IndicatorMetas { get; init; } = null!;
    public DbSet<TimePeriodMeta> TimePeriodMetas { get; init; } = null!;
    public DbSet<ChangeSetFilters> ChangeSetFilters { get; init; } = null!;
    public DbSet<ChangeSetFilterOptions> ChangeSetFilterOptions { get; init; } = null!;
    public DbSet<ChangeSetIndicators> ChangeSetIndicators { get; init; } = null!;
    public DbSet<ChangeSetLocations> ChangeSetLocations { get; init; } = null!;
    public DbSet<ChangeSetTimePeriods> ChangeSetTimePeriods { get; init; } = null!;
}
