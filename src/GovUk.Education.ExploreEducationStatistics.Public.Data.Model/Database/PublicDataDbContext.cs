using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;

public class PublicDataDbContext : DbContext
{
    public const string PublicDataReadWriteRole = "public_data_read_write";
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

        // These Json classes are DTOs for extracting JSON fragments / query results  from JSONB columns.
        // They are mapped here to inform EF that they are not entities.
        modelBuilder.Entity<JsonFragment>().HasNoKey().ToView(null);
        modelBuilder.Entity<JsonBool>().HasNoKey().ToView(null);
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
    public DbSet<FilterMetaChange> FilterMetaChanges { get; init; } = null!;
    public DbSet<FilterOptionMetaChange> FilterOptionMetaChanges { get; init; } = null!;
    public DbSet<GeographicLevelMetaChange> GeographicLevelMetaChanges { get; init; } = null!;
    public DbSet<IndicatorMetaChange> IndicatorMetaChanges { get; init; } = null!;
    public DbSet<LocationMetaChange> LocationMetaChanges { get; init; } = null!;
    public DbSet<LocationOptionMetaChange> LocationOptionMetaChanges { get; init; } = null!;
    public DbSet<TimePeriodMetaChange> TimePeriodMetaChanges { get; init; } = null!;
    public DbSet<PreviewToken> PreviewTokens { get; init; } = null!;

    public DbSet<JsonFragment> JsonFragments { get; init; } = null!;
    
    public DbSet<JsonBool> JsonBool { get; init; } = null!;
}
