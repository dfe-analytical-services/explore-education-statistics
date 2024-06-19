using GovUk.Education.ExploreEducationStatistics.Common.Converters;
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

    public LocationMappingPlan LocationMappingPlan { get; set; } = null!;

    public FilterMappingPlan FilterMappingPlan { get; set; } = null!;

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

            builder.OwnsOne(v => v.LocationMappingPlan, locations =>
            {
                locations.ToJson();
                locations.OwnsMany(l => l.Mappings, locationMappings =>
                {
                    locationMappings.OwnsMany(mapping => mapping.Mappings, locationMapping =>
                    {
                        locationMapping.OwnsOne(lm => lm.Source);
                        locationMapping.Property(lm => lm.Type)
                            .HasConversion(new EnumToEnumValueConverter<MappingType>());
                    });
                });

                locations.OwnsMany(locations => locations.Candidates, locationTargets =>
                {
                    locationTargets.OwnsMany(mapping => mapping.Candidates);
                });
            });

            builder.OwnsOne(mapping => mapping.FilterMappingPlan, filters =>
            {
                filters.ToJson();

                filters.OwnsMany(f => f.Mappings, filterMapping =>
                {
                    filterMapping.OwnsOne(mapping => mapping.Source);
                    filterMapping.Property(mapping => mapping.Type)
                        .HasConversion(new EnumToEnumValueConverter<MappingType>());

                    filterMapping.OwnsMany(mapping => mapping.Options, filterOptionMapping =>
                    {
                        filterOptionMapping.OwnsOne(mapping => mapping.Source);
                        filterOptionMapping.Property(mapping => mapping.Type)
                            .HasConversion(new EnumToEnumValueConverter<MappingType>());
                    });
                });

                filters.OwnsMany(f => f.Candidates, filterTarget =>
                {
                    filterTarget.OwnsMany(mapping => mapping.Options);
                });
            });
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
    public string Key { get; set; }

    public string Label { get; set; } = string.Empty;
}

public abstract class LeafMapping<TEntry>
    where TEntry : Entry
{
    public MappingType Type { get; set; }

    public TEntry Source { get; set; } = null!;

    public string? CandidateKey { get; set; }
}

public abstract class ParentMapping<TEntry, TOption, TOptionMapping>
    where TEntry : Entry
    where TOption : Entry
    where TOptionMapping : LeafMapping<TOption>
{
    public MappingType Type { get; set; }

    public TEntry Source { get; set; } = null!;

    public List<TOptionMapping> Options { get; set; } = [];

    public string? CandidateKey { get; set; }
}

public class LocationOption : Entry
{
    protected string? Code { get; set; }

    protected string? OldCode { get; set; }

    protected string? Urn { get; set; }

    protected string? LaEstab { get; set; }

    protected string? Ukprn { get; set; }
}

public class LocationOptionMapping : LeafMapping<LocationOption>;

public class LocationLevelMappingPlan
{
    public GeographicLevel Level { get; set; }

    public List<LocationOptionMapping> Mappings { get; set; } = [];
}

public class LocationLevelMappingCandidates
{
    public GeographicLevel Level { get; set; }

    public List<LocationOption> Candidates { get; set; } = [];
}

public class LocationMappingPlan
{
    public List<LocationLevelMappingPlan> Mappings { get; set; }

    public List<LocationLevelMappingCandidates> Candidates { get; set; }
}

public class FilterOption : Entry;

public class Filter : Entry;

public class FilterMappingCandidate : Filter
{
    public List<FilterOption> Options { get; set; } = [];
}

public class FilterOptionMapping : LeafMapping<FilterOption>;

public class FilterMapping : ParentMapping<Filter, FilterOption, FilterOptionMapping>;

public class FilterMappingPlan
{
    public List<FilterMapping> Mappings { get; set; } = [];

    public List<FilterMappingCandidate> Candidates { get; set; } = [];
}
