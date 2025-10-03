using System.Text.Json;
using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json.Converters;

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

    // Use boolean flags to describe meta types that have been deleted and cannot be
    // changed via mapping currently. We can use this when calculating the version number.
    // We've gone with this approach for simplicity and expedience, but we may need to
    // migrate away from this approach later e.g. for indicator mappings.

    public bool HasDeletedIndicators { get; set; }

    public bool HasDeletedGeographicLevels { get; set; }

    public bool HasDeletedTimePeriods { get; set; }

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? Updated { get; set; }

    public LocationLevelMappings GetLocationLevelMappings(GeographicLevel level)
    {
        return LocationMappingPlan.Levels[level];
    }

    public LocationOptionMapping GetLocationOptionMapping(GeographicLevel level, string locationOptionKey)
    {
        return GetLocationLevelMappings(level).Mappings[locationOptionKey];
    }

    public FilterMapping GetFilterMapping(string filterKey)
    {
        return FilterMappingPlan.Mappings[filterKey];
    }

    public FilterOptionMapping GetFilterOptionMapping(string filterKey, string filterOptionKey)
    {
        return GetFilterMapping(filterKey).OptionMappings[filterOptionKey];
    }

    internal class Config : IEntityTypeConfiguration<DataSetVersionMapping>
    {
        public void Configure(EntityTypeBuilder<DataSetVersionMapping> builder)
        {
            builder.Property(mapping => mapping.Id).HasValueGenerator<UuidV7ValueGenerator>();

            builder
                .HasIndex(mapping => new { mapping.SourceDataSetVersionId })
                .HasDatabaseName("IX_DataSetVersionMappings_SourceDataSetVersionId")
                .IsUnique();

            builder
                .HasIndex(mapping => new { mapping.TargetDataSetVersionId })
                .HasDatabaseName("IX_DataSetVersionMappings_TargetDataSetVersionId")
                .IsUnique();

            builder
                .Property(p => p.LocationMappingPlan)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<LocationMappingPlan>(v, (JsonSerializerOptions)null!)!
                );

            builder
                .Property(p => p.FilterMappingPlan)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<FilterMappingPlan>(v, (JsonSerializerOptions)null!)!
                );
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
    AutoNone,
}

/// <summary>
/// This base class represents an element from the DataSetVersions that can be mapped.
/// </summary>
public abstract record MappableElement
{
    public required string Label { get; set; }
}

public abstract record MappableElementWithOptions<TMappableOption> : MappableElement
    where TMappableOption : MappableElement
{
    public Dictionary<string, TMappableOption> Options { get; init; } = [];
}

/// <summary>
/// This base class represents a mapping for a single mappable element e.g. a single Location.
/// This holds the source element itself from the source DataSetVersion e.g. a particular Location,
/// the type of mapping that has been performed (e.g. the user choosing a candidate Location from
/// the target DataSetVersion) and the candidate key (if a candidate has been chosen).
/// </summary>
public abstract record Mapping<TMappableElement>
    where TMappableElement : MappableElement
{
    public required TMappableElement Source { get; init; }

    public required string PublicId { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    public MappingType Type { get; set; } = MappingType.None;

    public string? CandidateKey { get; set; }
}

/// <summary>
/// This base class represents a mapping for a parent element which then itself also contains
/// child elements (or "options") that can themselves be mapped.
/// </summary>
public abstract record ParentMapping<TMappableElement, TOption, TOptionMapping> : Mapping<TMappableElement>
    where TMappableElement : MappableElement
    where TOption : MappableElement
    where TOptionMapping : Mapping<TOption>
{
    public Dictionary<string, TOptionMapping> OptionMappings { get; set; } = [];
}

/// <summary>
/// This represents a location option that is potentially mappable to another location option
/// from the same geographic level.
/// </summary>
public record MappableLocationOption : MappableElement
{
    public string? Code { get; init; }

    public string? OldCode { get; init; }

    public string? Urn { get; init; }

    public string? LaEstab { get; init; }

    public string? Ukprn { get; init; }
};

/// <summary>
/// This represents the mapping, or failure to map, of a source location option to a target
/// location option from the same geographic level.
/// </summary>
public record LocationOptionMapping : Mapping<MappableLocationOption>;

/// <summary>
/// This represents a single geographic level's worth of location mappings from the source
/// data set version and potential candidates to map to from the target data set version.
/// </summary>
public record LocationLevelMappings
{
    public Dictionary<string, LocationOptionMapping> Mappings { get; init; } = [];

    public Dictionary<string, MappableLocationOption> Candidates { get; init; } = [];
}

/// <summary>
/// This represents the overall mapping plan for all the geographic levels
/// and locations from the source data set version to the target version.
/// </summary>
public class LocationMappingPlan
{
    public Dictionary<GeographicLevel, LocationLevelMappings> Levels { get; init; } = [];
}

/// <summary>
/// This represents a filter option that is potentially mappable to another filter option.
/// </summary>
public record MappableFilterOption : MappableElement;

/// <summary>
/// This represents a filter that is potentially mappable to another filter.
/// </summary>
public record MappableFilter : MappableElement;

/// <summary>
/// This represents a candidate filter and all of its candidate filter options from
/// the target data set version that could be mapped to from filters and filter options
/// from the source version.
/// </summary>
public record FilterMappingCandidate : MappableElementWithOptions<MappableFilterOption>;

/// <summary>
/// This represents a potential mapping of a filter option from the source data set version
/// to a filter option in the target version.  In order to be mappable, both filter options'
/// parent filters must firstly be mapped to each other.
/// </summary>
public record FilterOptionMapping : Mapping<MappableFilterOption>;

/// <summary>
/// This represents a potential mapping of a filter from the source data set version
/// to a filter in the target version.
/// </summary>
public record FilterMapping : ParentMapping<MappableFilter, MappableFilterOption, FilterOptionMapping>;

/// <summary>
/// This represents the overall mapping plan for filters and filter options from the source
/// data set version to filters and filter options in the target data set version.
/// </summary>
public record FilterMappingPlan
{
    public Dictionary<string, FilterMapping> Mappings { get; init; } = [];

    public Dictionary<string, FilterMappingCandidate> Candidates { get; init; } = [];
}
