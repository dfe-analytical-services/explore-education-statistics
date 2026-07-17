#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public record DataSetMapping
{
    public Guid Id { get; init; }

    public Guid OriginalDataFileId { get; init; }
    public File OriginalDataFile { get; init; } = null!;
    public Guid ReplacementDataFileId { get; init; }
    public File ReplacementDataFile { get; init; } = null!;

    public Dictionary<Guid, IndicatorMapping> IndicatorMappings { get; init; } = null!;
    public List<UnmappedIndicator> UnmappedReplacementIndicators { get; init; } = [];

    public Dictionary<Guid, LocationMapping> LocationMappings { get; init; } = null!;

    public List<UnmappedLocation> UnmappedReplacementLocations { get; init; } = [];

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }, // so we save MapStatus as a string
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    internal class Config : IEntityTypeConfiguration<DataSetMapping>
    {
        public void Configure(EntityTypeBuilder<DataSetMapping> builder)
        {
            builder.HasIndex(x => x.OriginalDataFileId).IsUnique();
            builder.HasIndex(x => x.ReplacementDataFileId).IsUnique();

            builder
                .HasOne(x => x.OriginalDataFile)
                .WithMany()
                .HasForeignKey(x => x.OriginalDataFileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.ReplacementDataFile)
                .WithMany()
                .HasForeignKey(x => x.ReplacementDataFileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .Property(x => x.IndicatorMappings)
                .HasConversion(
                    indicatorMappings => JsonSerializer.Serialize(indicatorMappings, JsonOptions),
                    indMappingString =>
                        JsonSerializer.Deserialize<Dictionary<Guid, IndicatorMapping>>(indMappingString, JsonOptions)
                        ?? new Dictionary<Guid, IndicatorMapping>(),
                    ValueComparer.CreateDefault<Dictionary<Guid, IndicatorMapping>>(false)
                )
                .HasColumnType("nvarchar(max)");

            builder
                .Property(x => x.UnmappedReplacementIndicators)
                .HasConversion(
                    unmappedIndicators => JsonSerializer.Serialize(unmappedIndicators, JsonOptions),
                    unmappedIndicatorsString =>
                        JsonSerializer.Deserialize<List<UnmappedIndicator>>(unmappedIndicatorsString, JsonOptions)
                        ?? new List<UnmappedIndicator>(),
                    ValueComparer.CreateDefault<List<UnmappedIndicator>>(false)
                );

            builder
                .Property(x => x.LocationMappings)
                .HasConversion(
                    locationMappings => JsonSerializer.Serialize(locationMappings, JsonOptions),
                    locMappingString =>
                        JsonSerializer.Deserialize<Dictionary<Guid, LocationMapping>>(locMappingString, JsonOptions)
                        ?? new Dictionary<Guid, LocationMapping>(),
                    ValueComparer.CreateDefault<Dictionary<Guid, LocationMapping>>(false)
                )
                .HasColumnType("nvarchar(max)");

            builder
                .Property(x => x.UnmappedReplacementLocations)
                .HasConversion(
                    unmappedLocations => JsonSerializer.Serialize(unmappedLocations, JsonOptions),
                    unmappedLocationsString =>
                        JsonSerializer.Deserialize<List<UnmappedLocation>>(unmappedLocationsString, JsonOptions)
                        ?? new List<UnmappedLocation>(),
                    ValueComparer.CreateDefault<List<UnmappedLocation>>(false)
                );
        }
    }
}

public enum MapStatus
{
    Unset,
    ManuallySet, // user manually mapped this (whether to another Id or nothing)
    AutoSet, // automatically mapped when the initial mapping was created
}

public record UnmappedIndicator
{
    public Guid Id { get; set; }
    public string Label { get; set; } = "";
    public string ColumnName { get; set; } = "";
    public Guid GroupId { get; set; }
    public string GroupLabel { get; set; } = "";
}

public record IndicatorMapping
{
    public Guid OriginalId { get; set; }
    public string OriginalLabel { get; set; } = "";
    public string OriginalColumnName { get; set; } = "";
    public Guid OriginalGroupId { get; set; }
    public string OriginalGroupLabel { get; set; } = "";

    public Guid? ReplacementId { get; set; }
    public string? ReplacementLabel { get; set; }
    public string? ReplacementColumnName { get; set; }
    public Guid? ReplacementGroupId { get; set; }
    public string? ReplacementGroupLabel { get; set; }

    public MapStatus Status { get; set; }
}

public record UnmappedLocation
{
    public Guid Id { get; set; }
    public GeographicLevel GeographicLevel { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
}

public record LocationMapping
{
    public Guid OriginalId { get; set; }
    public GeographicLevel OriginalGeographicLevel { get; set; }
    public string OriginalCode { get; set; } = "";
    public string OriginalName { get; set; } = "";

    public Guid? ReplacementId { get; set; }
    public GeographicLevel? ReplacementGeographicLevel { get; set; }
    public string? ReplacementCode { get; set; } = "";
    public string? ReplacementName { get; set; } = "";

    public MapStatus Status { get; set; }
}
