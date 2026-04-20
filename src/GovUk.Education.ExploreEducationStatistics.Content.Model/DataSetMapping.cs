#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public record DataSetMapping
{
    public Guid Id { get; set; }
    public Guid OriginalDataSetId { get; set; }
    public Guid ReplacementDataSetId { get; set; }

    public Dictionary<Guid, IndicatorMapping> IndicatorMappings { get; set; } = new();
    public List<UnmappedIndicator> UnmappedReplacementIndicators { get; set; } = [];

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
            builder.HasKey(x => x.Id);

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
