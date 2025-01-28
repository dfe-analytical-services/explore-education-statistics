using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class FilterMeta : ICreatedUpdatedTimestamps<DateTimeOffset, DateTimeOffset?>
{
    public int Id { get; set; }

    public required Guid DataSetVersionId { get; set; }

    public DataSetVersion DataSetVersion { get; set; } = null!;

    public required string PublicId { get; set; }

    public required string Column { get; set; }

    public required string Label { get; set; }

    public string Hint { get; set; } = string.Empty;

    public string? AutoSelectLabel { get; set; }

    public List<FilterOptionMeta> Options { get; set; } = [];

    public List<FilterOptionMetaLink> OptionLinks { get; set; } = [];

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? Updated { get; set; }

    internal class Config : IEntityTypeConfiguration<FilterMeta>
    {
        public void Configure(EntityTypeBuilder<FilterMeta> builder)
        {
            builder.Property(m => m.PublicId)
                .HasMaxLength(10);

            builder.Property(m => m.Column)
                .HasMaxLength(50);

            builder.Property(m => m.Label)
                .HasMaxLength(80);

            builder.Property(m => m.AutoSelectLabel)
                .HasMaxLength(120);

            builder.HasIndex(m => new { m.DataSetVersionId, m.PublicId })
                .IsUnique();

            builder.HasIndex(m => new { m.DataSetVersionId, m.Column })
                .IsUnique();

            builder.HasMany(m => m.Options)
                .WithMany(o => o.Metas)
                .UsingEntity<FilterOptionMetaLink>(
                    b => b
                        .HasOne(l => l.Option)
                        .WithMany(o => o.MetaLinks)
                        .HasForeignKey(l => l.OptionId),
                    b => b
                        .HasOne(l => l.Meta)
                        .WithMany(m => m.OptionLinks)
                        .HasForeignKey(l => l.MetaId)
                );
        }
    }
}
