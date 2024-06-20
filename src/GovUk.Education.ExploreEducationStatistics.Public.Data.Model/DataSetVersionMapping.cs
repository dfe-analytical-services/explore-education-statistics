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

    public bool LocationMappingsComplete { get; set; }
    
    public bool FilterMappingsComplete { get; set; }
    
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

/// <summary>
/// This enum indicates the type of mapping that an element from the source DataSetVersion has had applied
/// so far in the mapping process.
/// </summary>
public enum MappingType
{
    /// <summary>
    /// No mapping has yet been carried out, either automatically or by the user.
    /// </summary>
    None,

    /// <summary>
    /// The user has manually chosen a mapping candidate for this source element.
    /// </summary>
    ManualMapped,
    
    /// <summary>
    /// The user has manually indicated that no mapping candidate exists for this source element.
    /// </summary>
    ManualNone,

    /// <summary>
    /// The server has automatically selected a likely mapping candidate for this source element.
    /// </summary>
    AutoMapped,
    
    /// <summary>
    /// The server has automatically indicated that no likely mapping candidate exists for this source element.
    /// </summary>
    AutoNone
}

/// <summary>
/// This base class represents an element from the DataSetVersions that can be mapped.
/// </summary>
public abstract class MappableElement
{
    /// <summary>
    /// This is a synthetic identifier which is used to identify source and target
    /// elements in lieu of using actual database ids.
    /// </summary>
    public string Key { get; set; }
}

/// <summary>
/// This base class represents a mapping for a single mappable element e.g. a single Location.
/// This holds the source element itself from the source DataSetVersion e.g. a particular Location,
/// the type of mapping that has been performed (e.g. the user choosing a candidate Location from
/// the target DataSetVersion) and the candidate key (if a candidate has been chosen).
/// </summary>
public abstract class LeafMapping<TMappableElement>
    where TMappableElement : MappableElement
{
    public MappingType Type { get; set; }

    public TMappableElement Source { get; set; } = null!;

    public string? CandidateKey { get; set; }
}

public abstract class ParentMapping<TMappableElement, TOption, TOptionMapping>
    where TMappableElement : MappableElement
    where TOption : MappableElement
    where TOptionMapping : LeafMapping<TOption>
{
    public MappingType Type { get; set; }

    public TMappableElement Source { get; set; } = null!;

    public List<TOptionMapping> Options { get; set; } = [];

    public string? CandidateKey { get; set; }
}

public class LocationOption : MappableElement
{
    public string Label { get; set; } = string.Empty;
    
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

public class FilterOption : MappableElement
{
    public string Label { get; set; } = string.Empty;
}

public class Filter : MappableElement
{
    public string Label { get; set; } = string.Empty;
}

public class FilterMappingCandidate : MappableElement
{
    public string Label { get; set; } = string.Empty;
    
    public List<FilterOption> Options { get; set; } = [];
}

public class FilterOptionMapping : LeafMapping<FilterOption>;

public class FilterMapping : ParentMapping<Filter, FilterOption, FilterOptionMapping>;

public class FilterMappingPlan
{
    public List<FilterMapping> Mappings { get; set; } = [];

    public List<FilterMappingCandidate> Candidates { get; set; } = [];
}
