#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class DataSetMapping
{
    public Guid Id { get; set; }
    public Guid OriginalDataSetId { get; set; }
    public Guid ReplacementDataSetId { get; set; }

    public Dictionary<Guid, IndicatorMapping> IndicatorMappings { get; set; } = new();
    public List<Guid> CandidateIndicatorReplacementIds { get; set; } = [];

    internal class Config : IEntityTypeConfiguration<DataSetMapping>
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            Converters = { new JsonStringEnumConverter() }, // so we save MapStatus as a string
            //PropertyNamingPolicy = null, // @MarkFix needed?
            //WriteIndented = false, // @MarkFix needed?
        };

        public void Configure(EntityTypeBuilder<DataSetMapping> builder)
        {
            builder.HasKey(x => x.Id);

            builder
                .Property(x => x.IndicatorMappings)
                .HasConversion(
                    indicatorMappings => JsonSerializer.Serialize(indicatorMappings, JsonOptions),
                    indMappingString =>
                        JsonSerializer.Deserialize<Dictionary<Guid, IndicatorMapping>>(indMappingString, JsonOptions)
                        ?? new(),
                    ValueComparer.CreateDefault<Dictionary<Guid, IndicatorMapping>>(false)
                )
                .HasColumnType("nvarchar(max)");

            builder.OwnsOne(
                dataSetMapping => dataSetMapping.CandidateIndicatorReplacementIds,
                b =>
                {
                    b.ToJson();
                }
            );
        }
    }
}

public enum MapStatus
{
    Incomplete,
    ManuallySet,
    AutoSet,
}

public class IndicatorMapping
{
    public Guid OriginalId { get; set; }
    public string OriginalGroupLabel { get; set; } = "";

    public Guid? ReplacementId { get; set; }
    public string? ReplacementGroupLabel { get; set; }

    public MapStatus Status { get; set; }
}
