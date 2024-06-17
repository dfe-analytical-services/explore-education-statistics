using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class DataSetVersionMapping : ICreatedUpdatedTimestamps<DateTimeOffset, DateTimeOffset?>
{
    public Guid Id { get; init; }

    public required Guid SourceDataSetVersionId { get; set; }

    public DataSetVersion SourceDataSetVersion { get; set; } = null!;

    public required Guid TargetDataSetVersionId { get; set; }

    public DataSetVersion TargetDataSetVersion { get; set; } = null!;

    public Locations Locations { get; set; } = null!;

    public Filters Filters { get; set; } = null!;

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? Updated { get; set; }

    internal class Config : IEntityTypeConfiguration<DataSetVersionMapping>
    {
        public void Configure(EntityTypeBuilder<DataSetVersionMapping> builder)
        {
            builder.Property(mapping => mapping.Id)
                .HasValueGenerator<UuidV7ValueGenerator>();

            builder.HasIndex(mapping => new { mapping.SourceDataSetVersionId })
                .HasDatabaseName("IX_DataSetVersionMappings_SourceDataSetVersionId")
                .IsUnique();

            builder.HasIndex(mapping => new { mapping.TargetDataSetVersionId })
                .HasDatabaseName("IX_DataSetVersionMappings_TargetDataSetVersionId")
                .IsUnique();
        }
    }
}

public enum MappingType
{
    None,

    ManualMapped,
    ManualNone,

    AutoMapped,
    AutoNone
}

public abstract class Entry
{
    public int Id { get; set; }

    public string Label { get; set; } = string.Empty;
}

public abstract class LeafMapping<TEntry>
    where TEntry : Entry
{
    public MappingType Type { get; set; }

    public TEntry Source { get; set; } = null!;
    
    public int? TargetId { get; set; }
}

public abstract class ParentMapping<TEntry, TOption, TOptionMapping>
    where TEntry : Entry
    where TOption : Entry
    where TOptionMapping : LeafMapping<TOption> 
{
    public MappingType Type { get; set; }

    public TEntry Source { get; set; } = null!;

    public List<TOptionMapping> Options { get; set; } = [];
    
    public int? TargetId { get; set; }
}

public class LocationOption : Entry
{
    protected string? Code { get; set; }

    protected string? OldCode { get; set; }

    protected string? Urn { get; set; }

    protected string? LaEstab { get; set; }

    protected string? Ukprn { get; set; }
}

public class LocationMapping : LeafMapping<LocationOption>;

public class LocationMappings
{
    public GeographicLevel Level { get; set; }

    public List<LocationMapping> Mappings { get; set; } = [];
}

public class LocationTargets
{
    public GeographicLevel Level { get; set; }

    public List<LocationOption> Options { get; set; } = [];
}

public class Locations
{
    public List<LocationMappings> Mappings { get; set; }

    public List<LocationTargets> Targets { get; set; }
}

public class FilterOption : Entry;

public class Filter : Entry;

public class FilterTarget : Filter
{
    public List<FilterOption> Options { get; set; } = [];
}

public class FilterOptionMapping : LeafMapping<FilterOption>;

public class FilterMapping : ParentMapping<Filter, FilterOption, FilterOptionMapping>;

public class Filters
{
    public List<FilterMapping> Mappings { get; set; } = [];

    public List<FilterTarget> Targets { get; set; } = [];
}
