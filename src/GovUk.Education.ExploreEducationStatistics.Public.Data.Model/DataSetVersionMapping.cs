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

            builder.HasIndex(mapping => new {mapping.SourceDataSetVersionId})
                .HasDatabaseName("IX_DataSetVersionMappings_SourceDataSetVersionId")
                .IsUnique();

            builder.HasIndex(mapping => new {mapping.TargetDataSetVersionId})
                .HasDatabaseName("IX_DataSetVersionMappings_TargetDataSetVersionId")
                .IsUnique();

            builder.OwnsOne(v => v.LocationMappingPlan, locations =>
            {
                locations.ToJson();
                locations.OwnsMany(l => l.Levels, locationMappings =>
                {
                    locationMappings.OwnsMany(mapping => mapping.Mappings, locationMapping =>
                    {
                        locationMapping.OwnsOne(lm => lm.Source);
                        locationMapping.Property(lm => lm.Type)
                            .HasConversion(new EnumToEnumValueConverter<MappingType>());
                    });
                });

                locations.OwnsMany(location => location.Levels, level =>
                {
                    level.OwnsMany(mapping => mapping.Mappings);
                    level.OwnsMany(mapping => mapping.Candidates);
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

                    filterMapping.OwnsMany(mapping => mapping.OptionMappings, filterOptionMapping =>
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
    /// The service has automatically selected a likely mapping candidate for this source element.
    /// </summary>
    AutoMapped,

    /// <summary>
    /// The service has automatically indicated that no likely mapping candidate exists for this
    /// source element.  It will still take the user to confirm these and switch them to be
    /// "ManualNone" in the process before the service indicates that the mappings are complete.
    /// </summary>
    AutoNone
}

/// <summary>
/// This base class represents an element from the DataSetVersions that can be mapped.
/// </summary>
public abstract record MappableElement
{
    /// <summary>
    /// This is a synthetic identifier which is used to identify source and target
    /// elements in lieu of using actual database ids.
    /// </summary>
    public string Key { get; set; } = string.Empty;
}

public abstract record MappableElementWithOptions<TMappableOption>
    : MappableElement
    where TMappableOption : MappableElement
{
    public List<TMappableOption> Options { get; set; } = [];
}

/// <summary>
/// This base class represents a mapping for a single mappable element e.g. a single Location.
/// This holds the source element itself from the source DataSetVersion e.g. a particular Location,
/// the type of mapping that has been performed (e.g. the user choosing a candidate Location from
/// the target DataSetVersion) and the candidate key (if a candidate has been chosen).
/// </summary>
public abstract record Mapping<TMappableElement>
    where TMappableElement : class
{
    public MappingType Type { get; set; } = MappingType.None;

    public TMappableElement Source { get; set; } = null!;

    public string? CandidateKey { get; set; }
}

/// <summary>
/// This base class represents a mapping for a parent element which then itself also contains
/// child elements (or "options") that can themselves be mapped.
/// </summary>
public abstract record ParentMapping<TMappableElement, TOption, TOptionMapping>
    : Mapping<TMappableElement>
    where TMappableElement : MappableElement
    where TOption : MappableElement
    where TOptionMapping : Mapping<TOption>
{
    public List<TOptionMapping> OptionMappings { get; set; } = [];
}

/// <summary>
/// This represents a location option that is potentially mappable to another location option
/// from the same geographic level. 
/// </summary>
public record LocationOption : MappableElement
{
    public string Label { get; set; } = string.Empty;
}

/// <summary>
/// This represents the mapping, or failure to map, of a source location option to a target
/// location option from the same geographic level.
/// </summary>
public record LocationOptionMapping : Mapping<LocationOption>;

/// <summary>
/// This represents a single geographic level's worth of location mappings from the source
/// data set version and potential candidates to map to from the target data set version. 
/// </summary>
public record LocationLevelMappings
{
    public GeographicLevel Level { get; set; }

    public List<LocationOptionMapping> Mappings { get; set; } = [];

    public List<LocationOption> Candidates { get; set; } = [];
}

/// <summary>
/// This represents the overall mapping plan for all the geographic levels
/// and locations from the source data set version to the target version.
/// </summary>
public class LocationMappingPlan
{
    public List<LocationLevelMappings> Levels { get; set; } = [];
}

/// <summary>
/// This represents a filter option that is potentially mappable to another filter option. 
/// </summary>
public record FilterOption : MappableElement
{
    public string Label { get; set; } = string.Empty;
}

/// <summary>
/// This represents a filter that is potentially mappable to another filter.
/// </summary>
public record Filter : MappableElement
{
    public string Label { get; set; } = string.Empty;
}

/// <summary>
/// This represents a candidate filter and all of its candidate filter options from
/// the target data set version that could be mapped to from filters and filter options
/// from the source version.
/// </summary>
public record FilterMappingCandidate : MappableElementWithOptions<FilterOption>
{
    public string Label { get; set; } = string.Empty;
}

/// <summary>
/// This represents a potential mapping of a filter option from the source data set version
/// to a filter option in the target version.  In order to be mappable, both filter options'
/// parent filters must firstly be mapped to each other.  
/// </summary>
public record FilterOptionMapping : Mapping<FilterOption>;

/// <summary>
/// This represents a potential mapping of a filter from the source data set version
/// to a filter in the target version.  
/// </summary>
public record FilterMapping : ParentMapping<Filter, FilterOption, FilterOptionMapping>;

/// <summary>
/// This represents the overall mapping plan for filters and filter options from the source
/// data set version to filters and filter options in the target data set version.
/// </summary>
public record FilterMappingPlan
{
    public List<FilterMapping> Mappings { get; set; } = [];

    public List<FilterMappingCandidate> Candidates { get; set; } = [];
}
