using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Dtos;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;

public class PublicDataDbContext : DbContext
{
    public const string PublicDataReadWriteRole = "public_data_read_write";
    public const string FilterOptionMetaLinkSequence = "FilterOptionMetaLink_seq";
    public const string LocationOptionMetasIdSequence = "LocationOptionMetas_Id_seq";

    public PublicDataDbContext()
    {
        // We intentionally don't run `Configure` here as Moq would call this constructor, and we'd immediately get
        // a MockException from interacting with its fields e.g. from adding events listeners to `ChangeTracker`.
        // We can just rely on the variant which takes options instead as this is what gets used in real application
        // scenarios.
    }

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
        modelBuilder.Entity<JsonString>().HasNoKey().ToView(null);
        modelBuilder.Entity<FilterAndOptionMappingTypeDto>().HasNoKey().ToView(null);
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

    public virtual DbSet<DataSet> DataSets { get; init; } = null!;
    public virtual DbSet<DataSetVersion> DataSetVersions { get; init; } = null!;
    public virtual DbSet<DataSetVersionImport> DataSetVersionImports { get; init; } = null!;
    public virtual DbSet<DataSetVersionMapping> DataSetVersionMappings { get; init; } = null!;
    public virtual DbSet<GeographicLevelMeta> GeographicLevelMetas { get; init; } = null!;
    public virtual DbSet<LocationMeta> LocationMetas { get; init; } = null!;
    public virtual DbSet<LocationOptionMeta> LocationOptionMetas { get; init; } = null!;
    public virtual DbSet<LocationOptionMetaLink> LocationOptionMetaLinks { get; init; } = null!;
    public virtual DbSet<FilterMeta> FilterMetas { get; init; } = null!;
    public virtual DbSet<FilterOptionMeta> FilterOptionMetas { get; init; } = null!;
    public virtual DbSet<FilterOptionMetaLink> FilterOptionMetaLinks { get; init; } = null!;
    public virtual DbSet<IndicatorMeta> IndicatorMetas { get; init; } = null!;
    public virtual DbSet<TimePeriodMeta> TimePeriodMetas { get; init; } = null!;
    public virtual DbSet<FilterMetaChange> FilterMetaChanges { get; init; } = null!;
    public virtual DbSet<FilterOptionMetaChange> FilterOptionMetaChanges { get; init; } = null!;
    public virtual DbSet<GeographicLevelMetaChange> GeographicLevelMetaChanges { get; init; } = null!;
    public virtual DbSet<IndicatorMetaChange> IndicatorMetaChanges { get; init; } = null!;
    public virtual DbSet<LocationMetaChange> LocationMetaChanges { get; init; } = null!;
    public virtual DbSet<LocationOptionMetaChange> LocationOptionMetaChanges { get; init; } = null!;
    public virtual DbSet<TimePeriodMetaChange> TimePeriodMetaChanges { get; init; } = null!;
    public virtual DbSet<PreviewToken> PreviewTokens { get; init; } = null!;
}
